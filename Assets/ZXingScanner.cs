using ZXing;
using ZXing.Common;
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using Unity.Collections.LowLevel.Unsafe;

/// <summary>
/// Extremely basic usage of BarcodeDecoder in ZXing
/// library to actively decode QR codes from WebCam.
/// You may download ZXing C# binaries from:
/// http://zxingnet.codeplex.com/
/// This is tested with Unity3D 5.3.5f1
/// 
/// A Single threaded version of this class is available at:
/// https://gist.github.com/sepehr-laal/d52d0ee173c5c28d1f324729b35cb96a
/// 
/// Attach this script to an object in your scene and
/// hit play. If you hold a QR code in front of your
/// WebCam, its decoded string appears in "decodedResult"
/// </summary>
///


// Slouzi ke scanovani QR kodu, ktery se vytahne z AR camera image a nad nim se pusti detekce QR kodu pres knihovnu ZXing
public class ZXingScanner : MonoBehaviour
{
    public MyGameManager myGameManager;
    private const String ORI_PARAMETER = "ori=Q";

    [SerializeField]
    [Tooltip("The ARCameraManager which will produce frame events.")]
    ARCameraManager m_CameraManager;

    Texture2D m_CameraTexture;
    /// <summary>
    /// Get or set the <c>ARCameraManager</c>.
    /// </summary>
    public ARCameraManager cameraManager
    {
        get => m_CameraManager;
        set => m_CameraManager = value;
    }

    internal class Data
    {
        public Color32[] image;
        public int imageHeight;
        public int imageWidth;
    }

    public string decodedResult;
    BarcodeReader barcodeReader;
    Thread barcodeDecoderThread;
    TimeSpan barcodeThreadDelay;
    volatile bool stopSignal;
    volatile bool dataSignal;
    volatile Data data;
    bool isRunning = false;

    void Start()
    {
        isRunning = false;
        decodedResult = null;
        disableFrameReceive();
        var formats = new List<BarcodeFormat>
        {
            BarcodeFormat.QR_CODE
        };

        barcodeReader = new BarcodeReader
        {
            AutoRotate = false,
            Options = new DecodingOptions
            {
                PossibleFormats = formats,
                TryHarder = true,
            }
        };

        data = new Data { image = null };
        dataSignal = true;
        stopSignal = false;
    }

    // Spusti detekovani QR kodu
    public void StartDecoding() {
        enableFrameReceive();
        isRunning = true;
        data = new Data { image = null };
        dataSignal = true;
        stopSignal = false;
        decodedResult = null;

        barcodeThreadDelay = TimeSpan.FromMilliseconds(1);
        barcodeDecoderThread = new Thread(DecodeQR);
        barcodeDecoderThread.Start();
    }

    void enableFrameReceive() {
        if (m_CameraManager != null)
        {
            m_CameraManager.frameReceived += OnCameraFrameReceived;
        }
    }

    void disableFrameReceive()
    {
        if (m_CameraManager != null)
        {
            m_CameraManager.frameReceived -= OnCameraFrameReceived;
        }
    }

    void OnEnable()
    {
        enableFrameReceive();
    }

    void OnDisable()
    {
        disableFrameReceive();
    }

    unsafe void UpdateCameraImage()
    {
        // Attempt to get the latest camera image. If this method succeeds,
        // it acquires a native resource that must be disposed (see below).
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            return;
        }

        // Once we have a valid XRCpuImage, we can access the individual image "planes"
        // (the separate channels in the image). XRCpuImage.GetPlane provides
        // low-overhead access to this data. This could then be passed to a
        // computer vision algorithm. Here, we will convert the camera image
        // to an RGBA texture and draw it on the screen.

        // Choose an RGBA format.
        // See XRCpuImage.FormatSupported for a complete list of supported formats.
        var format = TextureFormat.RGBA32;

        if (m_CameraTexture == null || m_CameraTexture.width != image.width || m_CameraTexture.height != image.height)
        {
            m_CameraTexture = new Texture2D(image.width, image.height, format, false);
        }

        // Convert the image to format, flipping the image across the Y axis.
        // We can also get a sub rectangle, but we'll get the full image here.
        var conversionParams = new XRCpuImage.ConversionParams(image, format, XRCpuImage.Transformation.MirrorY);

        // Texture2D allows us write directly to the raw texture data
        // This allows us to do the conversion in-place without making any copies.
        var rawTextureData = m_CameraTexture.GetRawTextureData<byte>();
        try
        {
            image.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
        }
        finally
        {
            // We must dispose of the XRCpuImage after we're finished
            // with it to avoid leaking native resources.
            image.Dispose();
        }

        // Apply the updated texture data to our texture
        m_CameraTexture.Apply();
        data.image = m_CameraTexture.GetPixels32();
        data.imageHeight = m_CameraTexture.height;
        data.imageWidth = m_CameraTexture.width;
        dataSignal = false;
    }


    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (isRunning)
        {
            UpdateCameraImage();
        }
    }

    void DecodeQR()
    {
        while (!stopSignal)
        {
            if (!dataSignal)
            {
                Result result = barcodeReader.Decode(
                    data.image,
                    data.imageWidth,
                    data.imageHeight);

                if (result != null)
                {
                    decodedResult = result.Text;
                    StopDecoding();
                }

                dataSignal = true;
            }

            Thread.Sleep(barcodeThreadDelay);
        }
    }

    void OnDestroy()
    {
        StopDecoding();
    }

    public void StopDecoding() {
        if (isRunning)
        { 
            isRunning = false;
            stopSignal = true;
            disableFrameReceive();
            barcodeDecoderThread.Join(1000);
        }
    }
}