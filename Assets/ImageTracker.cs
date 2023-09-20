using System;
using System.Collections;
using UnityEngine.XR.ARSubsystems;


namespace UnityEngine.XR.ARFoundation.Samples
{
    /// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class ImageTracker : MonoBehaviour
    {
        ARTrackedImageManager m_TrackedImageManager;
        public MyGameManager myGameManager;
        public Boolean tracking = false;
        public ARTrackedImage arTrackedImage = null;

        public Boolean paused = true;


        void Awake()
        {
            m_TrackedImageManager = myGameManager.aRTrackedImageManager;
        }

        void OnEnable()
        {
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        void OnDisable()
        {
            m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        // Aktualizuje polohu markeru a cele cesty
        void UpdateInfo(ARTrackedImage trackedImage)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                // Naskenovali jsme marker - vypneme overlay pro sken markeru
                if (myGameManager.showMarkerOverlay)
                {
                    myGameManager.showMarkerOverlay = false;
                    myGameManager.MarkerOverlay.SetActive(false);
                    paused = true;
                }
                if (!tracking)
                {
                    tracking = true;                   
                }
                trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);
                myGameManager.completeWay.transform.position = new Vector3(trackedImage.transform.position.x, trackedImage.transform.position.y, trackedImage.transform.position.z);
                myGameManager.completeWay.transform.localEulerAngles = new Vector3(myGameManager.completeWay.transform.eulerAngles.x, trackedImage.transform.eulerAngles.y + 180, myGameManager.completeWay.transform.eulerAngles.z);
            }
            else {
                trackedImage.transform.localScale = new Vector3(0, 0, 0);
            }
            if (!myGameManager.showMarkerOverlay && paused)
            {
                StartCoroutine(Slowdown());
            }
        }
        void Update()
        {
            // Pokud jsme vzdaleny od markeru 20m, prepnu do ScanPhaze, abych uzivatele donutil naskenovat dalsi QR a updatoval tim jeho polohu (nyni se nepouziva)
            /*if (arTrackedImage != null)
            {
                var distance = Vector3.Distance(myGameManager.mainCamera.transform.position, arTrackedImage.transform.position);
                if (distance >= 20 && myGameManager.phaze != "scan")
                {
                    arTrackedImage = null;
                    myGameManager.ScanPhaze();
                    myGameManager.ScanText.text = LocaleUtils.GetLocalizetString(myGameManager.NEXT_SCAN_TEXT);
                }
            }*/
        }

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            // Detekovan novy marker
            foreach (var trackedImage in eventArgs.added)
            {
                // Give the initial image a reasonable default scale
                if (trackedImage.trackingState == TrackingState.Tracking)
                {
                    arTrackedImage = trackedImage;
                    trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);
                    UpdateInfo(trackedImage);
                }
            }

            // Aktualizace jiz naskenovaneho markeru
            foreach (var trackedImage in eventArgs.updated)
            {  
                if (trackedImage.trackingState == TrackingState.Tracking && !paused )
                {
                    arTrackedImage = trackedImage;
                    UpdateInfo(trackedImage);
                }
            }
        }

        IEnumerator Slowdown()
        {
            
            paused = false;
            yield return new WaitForSeconds(300f);

        }
    }
}