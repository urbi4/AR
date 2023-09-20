using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;


public class MyGameManager : MonoBehaviour
{
    // API credentials
    public string BASIC_AUTH_USERNAME = "navapp";

    public string BASIC_AUTH_PASSWORD = "overhead-trickery-unpaved";

    private const String ORI_PARAMETER = "ori=Q";

    public GameObject mainCamera;

    public ArrowGenerator arrowGenerator;

    public GameObject completeWay;

    public GameObject defaultPanel;

    public GameObject scanPanel;

    public GameObject arPanel;

    public GameObject bannerPanel;

    public GameObject objectRecognitionPlane;

    public GameObject assignmentPanel;

    public ARSession arSession;

    public GameObject arSessionOrigin;

    public ARTrackedImageManager aRTrackedImageManager;

    public NavApi navApi;

    public ScannerBLE scannerBLE;

    public ZXingScanner zXingScanner;

    public ImageTracker imageTracker;

    public SpawnableManager spawnableManager;

    // Orientace markeru v uhlech vzhledem k severu
    public GameObject signOrientation;

    public ARPlaneManager arPlaneManager;

    //ScanPanel
    public GameObject ScanSign;

    public GameObject ScanBack;

    //ARPanel
    public GameObject MarkerOverlay;

    public GameObject ArNavigationPanel;

    public GameObject ArBack;

    public Text origin;

    public Text Distance;

    public Text DestinationInfo;

    public GameObject PathPanel;

    public GameObject NavigationLabel;

    public float NavigationLabelDefaultWidth = 592;

    public float NavigationLabelDefaultHeight = 150;

    public float NavigationLabelNavigatingHeight = 210;

    public GameObject SpawnObject;

    public GameObject FeatureMarkerTemplate; // Assign your prefab in the Inspector

    public GameObject ViewChecklist;

    [HideInInspector]
    public LocalizedString
        LOADING_PATH =
            new LocalizedString()
            { TableReference = "Locales", TableEntryReference = "LoadingPath" };

    [HideInInspector]
    public LocalizedString
        FIRST_SCAN_TEXT =
            new LocalizedString()
            {
                TableReference = "Locales",
                TableEntryReference = "ScanCodeOnTheGround"
            };

    [HideInInspector]
    public LocalizedString
        NEXT_SCAN_TEXT =
            new LocalizedString()
            { TableReference = "Locales", TableEntryReference = "Scan" };

    public Text ScanText;

    public bool showMarkerOverlay = true;

    public GameObject PrimaryMark;

    public Text PrimaryMarkText;

    public Image PrimaryMarkBuildingImage;

    public Image PrimaryMarkTypeImage;

    public Sprite BuildingImage;

    public Sprite ExitImage;

    public Sprite DoorImage;

    public Sprite StairsImage;

    public Sprite ElevatorImage;

    public Sprite CorridorImage;

    public Sprite DestinationImage;

    public Sprite BuildingA;

    public Sprite BuildingB;

    public Sprite BuildingC;

    public Sprite BuildingNone;

    public string phaze;

    public Locale localeEN;

    public Locale localeCS;

    public Sprite LocaleCSImage;

    public Sprite LocaleENImage;

    public Text LocaleText;

    public Image LocaleImage;

    public Text NameField;

    public Text DistanceField;

    public GameObject TestCube;



    IEnumerator Start()
    {
        // Pokud se jedna o tablet, prepneme na tablet canvas
        if (DeviceTypeChecker.GetDeviceType() == ENUM_Device_Type.Tablet)
        {
            List<GameObject> gamesObjects = FindAllObjectsInScene();

            GameObject tabletCanvas = FindGameObjectsAll(gamesObjects, "Tablet Canvas");
            tabletCanvas.SetActive(true);
            GameObject tabletDefaultPanel =
                FindGameObjectsAll(gamesObjects, "TabletDefaultPanel");
            defaultPanel = tabletDefaultPanel;
            scanPanel = FindGameObjectsAll(gamesObjects, "TabletScanPanel");
            arPanel = FindGameObjectsAll(gamesObjects, "TabletARPanel");
            assignmentPanel = FindGameObjectsAll(gamesObjects, "TabletAssignmentPanel");
            ScanSign = FindGameObjectsAll(gamesObjects, "TabletScanSignBtn");
            ScanBack = FindGameObjectsAll(gamesObjects, "TabletScanBack");
            MarkerOverlay = FindGameObjectsAll(gamesObjects, "TabletMarkerOverlay");
            ArBack = FindGameObjectsAll(gamesObjects, "TabletArBack");
            origin =
                FindGameObjectsAll(gamesObjects, "TabletOriginText").GetComponent<Text>();
            Distance =
                FindGameObjectsAll(gamesObjects, "TabletPathPanelText").GetComponent<Text>();
            DestinationInfo =
                FindGameObjectsAll(gamesObjects, "TabletDestinationText")
                    .GetComponent<Text>();
            PathPanel = FindGameObjectsAll(gamesObjects, "TabletPathPanel");
            NavigationLabel = FindGameObjectsAll(gamesObjects, "TabletNavigationLabelImage");
            ScanText =
                FindGameObjectsAll(gamesObjects, "TabletScanText").GetComponent<Text>();
            PrimaryMark = FindGameObjectsAll(gamesObjects, "TabletPrimaryMark");
            PrimaryMarkText =
                FindGameObjectsAll(gamesObjects, "TabletPrimaryMarkText")
                    .GetComponent<Text>();
            PrimaryMarkBuildingImage =
                FindGameObjectsAll(gamesObjects, "TabletPrimaryMark").GetComponent<Image>();
            PrimaryMarkTypeImage =
                FindGameObjectsAll(gamesObjects, "TabletPrimaryMarkTypeImage")
                    .GetComponent<Image>();
            LocaleText =
                FindGameObjectsAll(gamesObjects, "TabletLocaleText").GetComponent<Text>();
            LocaleImage =
                FindGameObjectsAll(gamesObjects, "TabletLocaleButton").GetComponent<Image>();
            NameField =
                FindGameObjectsAll(gamesObjects, "NameInputFieldTabletText")
                    .GetComponent<Text>();
            DistanceField =
                FindGameObjectsAll(gamesObjects,"DistanceInputFieldTabletText")
                    .GetComponent<Text>();
            GameObject.Find("Canvas").SetActive(false);
        }

        if (NameField != null && DistanceField != null)
        {
            NameField.text = "";
            DistanceField.text = "";
        }

        //scannerBLE.InitBLE();
        //receivedQRCode("https://fe01-b.hekate.club/navigation?ori=Q15486");
        // GetOrigin();
        // TEST DefaultPhaze();
        yield return LocalizationSettings.InitializationOperation;
    }

    public static GameObject FindGameObjectsAll(List<GameObject> array, string name) =>
        array.First(x => x.name == name);


    public static List<GameObject> FindAllObjectsInScene()
    {
        UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        List<GameObject> objectsInScene = new List<GameObject>();

        for (int i = 0; i < rootObjects.Length; i++)
        {
            objectsInScene.Add(rootObjects[i]);
        }

        for (int i = 0; i < allObjects.Length; i++)
        {
            if (allObjects[i].transform.root)
            {
                for (int i2 = 0; i2 < rootObjects.Length; i2++)
                {
                    if (allObjects[i].transform.root == rootObjects[i2].transform && allObjects[i] != rootObjects[i2])
                    {
                        objectsInScene.Add(allObjects[i]);
                        break;
                    }
                }
            }
        }
        return objectsInScene;
    }
        void Update()
    {
       
        if (
            zXingScanner.decodedResult != null /*&& imageTracker.tracking*/
        )
        {
            receivedQRCode(zXingScanner.decodedResult);
            zXingScanner.decodedResult = null;
            
         }
    }

    // Vola se pokud jsme detekovali QR kod s ORI parametrem, nasledne se vola API pro ziskani cesty a dalsich informaci
    public void receivedQRCode(string qrCode)
    {
        try {
            var customQRCode =  JsonConvert.DeserializeObject<CustomObjectDetection>(qrCode);
            //ObjectDetectionPhaze(customQRCode.ObjectDetection)
        }
        catch (Exception e)
        {
            e.ToString();
            if (qrCode.Contains(ORI_PARAMETER)) {
                navApi.StartApi(qrCode);
                ARPhaze();
            }
          
        } 
        
    }

    public void receivedQ(string q)
    {
        navApi.StartApiId(q);
        ARPhaze();
    }

    // Home screen
    public void DefaultPhaze()
    {
        ScanText.text = LocaleUtils.GetLocalizetString(FIRST_SCAN_TEXT);
        phaze = "default";
        if (NameField != null && DistanceField != null)
        {
            NameField.text = "";
            DistanceField.text = "";
        }

        //sphere.SetActive(false);
        defaultPanel.SetActive(true);
        scanPanel.SetActive(false);
        arPanel.SetActive(false);
        bannerPanel.SetActive(false);
        assignmentPanel.SetActive(false);
        objectRecognitionPlane.SetActive(false);
        zXingScanner.StopDecoding();
        arrowGenerator.DestroyCompleteWay();
        aRTrackedImageManager.enabled = false;
        arPlaneManager.enabled = false;


    }

    
    
    // SpawnFeatureMarker(new Vector3(540f, 1170f, 0f), "Jistič 1", 0.5f, Color.green);
    
    public void SpawnFeatureMarker(Vector3 spawnPosition, string labelText, float scale, Color color)
    {
        // Instantiate the prefab at the desired position and rotation
        GameObject spawnedPrefab = Instantiate(FeatureMarkerTemplate, spawnPosition, Quaternion.identity) as GameObject;
        // spawnedPrefab.SetActive(true);
        spawnedPrefab.transform.SetParent(defaultPanel.transform);

        Text labelTextComponent = spawnedPrefab.GetComponentInChildren<Text>();
        if (labelTextComponent != null)
        {
            labelTextComponent.text = labelText;
            labelTextComponent.color = color;
            labelTextComponent.fontSize = (int)(16 * scale);
        }

        spawnedPrefab.transform.localScale = new Vector3(1f * scale, 1f * scale, 1f * scale);
        UnityEngine.Debug.Log(spawnedPrefab.transform.localScale);

        RawImage imageComponent = spawnedPrefab.GetComponentInChildren<RawImage>();
        if (imageComponent != null)
        {
            imageComponent.color = color;
        }
    }

    

   

    // Screen scanovani QR kodu
    public void ScanPhaze()
    {
        phaze = "scan";
        defaultPanel.SetActive(false);
        scanPanel.SetActive(true);
        arPanel.SetActive(false);
        assignmentPanel.SetActive(false);
        bannerPanel.SetActive(false);
        objectRecognitionPlane.SetActive(false);
        zXingScanner.StartDecoding();
        aRTrackedImageManager.enabled = true;
    }

    // Faze kdy zobrazujeme AR a navigujeme podle sipky
    public void ARPhaze()
    {
        phaze = "ar";
        zXingScanner.StopDecoding();
        showMarkerOverlay = true;
        MarkerOverlay.SetActive(true);
        defaultPanel.SetActive(false);
        scanPanel.SetActive(false);
        assignmentPanel.SetActive(false);
        bannerPanel.SetActive(false);
        objectRecognitionPlane.SetActive(false);
        arPanel.SetActive(true);
        aRTrackedImageManager.enabled = true;
        navApi.DrawNavigationPath();
    }

    public void ARBannerPositioningPhaze()
    {

        phaze = "ar";
        zXingScanner.StopDecoding();
        defaultPanel.SetActive(false);
        scanPanel.SetActive(false);
        arPanel.SetActive(false);
        arPlaneManager.enabled = true;
        objectRecognitionPlane.SetActive(false);
        bannerPanel.SetActive(true);
        
        //navApi.DrawNavigationPath();

    }

    public void RecognitionPhaze()
    {
        phaze = "recog";
        zXingScanner.StartDecoding();
        defaultPanel.SetActive(false);
        scanPanel.SetActive(false);
        assignmentPanel.SetActive(false);
        arPanel.SetActive(false);
        bannerPanel.SetActive(false);
        objectRecognitionPlane.SetActive(true);
        TestCube.SetActive(true);
    }

    public void AssignmentPhaze()
    {
        phaze = "assign";
        assignmentPanel.SetActive(true);
        zXingScanner.StopDecoding();
        defaultPanel.SetActive(false);
        scanPanel.SetActive(false);
        objectRecognitionPlane.SetActive(false);
        bannerPanel.SetActive(false);
        arPanel.SetActive(false);
      
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // OnResetButton();
    }

    public void OnResetButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnChecklistButton()
    {
        if (ViewChecklist.activeSelf == true)
        {
            ViewChecklist.SetActive(false);
        }
        else
        {
            ViewChecklist.SetActive(true);
        }
    }

    public void OnLanguageChange()
    {
        if (LocalizationSettings.SelectedLocale == localeCS)
        {
            LocalizationSettings.SelectedLocale = localeEN;
            LocaleImage.sprite = LocaleENImage;
            LocaleText.text = "EN";
        }
        else
        {
            LocalizationSettings.SelectedLocale = localeCS;
            LocaleImage.sprite = LocaleCSImage;
            LocaleText.text = "CS";
        }
        NavApi.languageChanged = true;
    }

    void detectLeftCorner()
    {
        SpawnObject.transform.position = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
        //SpawnObject = Camera.ScreenToWorldPoint(new Vector3(0, Screen.height, Camera.nearClipPlane);
    }

    // Detekce náklonu zařízení
    void DetectTilt()
    {
        //Check Gyroscopesupport

        bool gyroBool = SystemInfo.supportsGyroscope; //A failsafe in case gyroscope isnt supported

        if (phaze == "ar" && gyroBool)
        {
            //Enable Gyroscope
            Gyroscope gyroscope = Input.gyro;
            gyroscope.enabled = true;
            float tiltThreshold = 0.5F;

            //mightbreaktheprogram++; MightBreakTheProgram was orriginally used to check if the detecttilt is even being called at all, now that I know it's being call, it's essencially useless

            //string textTilt = "Current Tilt: X: " + gyroscope.gravity.x + " Y: " + gyroscope.gravity.y + " Z: " + gyroscope.gravity.z;

            //TiltText.text = textTilt;

            //TiltText.enabled = true;

            if (gyroscope.gravity.y > -tiltThreshold && gyroscope.gravity.y < tiltThreshold && gyroscope.gravity.x > -tiltThreshold && gyroscope.gravity.x < tiltThreshold && gyroscope.gravity.z < -0.75f)
            {
                PrimaryMark.SetActive(true);
            }
            else
            {
                PrimaryMark.SetActive(false);
            }
        }
        else
        {
            PrimaryMark.SetActive(false);
            //TiltText.enabled = false;
        }
    }

    //public IEnumerator SendMeasurementsData()
    //{
    //    KibanaApi.PostNewMeasuring(MeasuringUtils.GenerateMeasurementsData());
    //}
}
