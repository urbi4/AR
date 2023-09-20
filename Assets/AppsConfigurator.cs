using System;
using System.Collections.Generic; // Import the System.Collections.Generic class to give us access to List<>
using System.IO;
using UnityEditor;
using UnityEngine;

public class AppsConfigurator : MonoBehaviour
{
    [Serializable]
    public class BaseAppConfig
    {
        [Header("Basic configuration")]
        public string CompanyName = "CGI";

        public string AppName = "";

        public string BuildVersion = "";

        public int BundleVersion = 1;

        public string PackageName = "";

        [Header("Permisions")]
        public string
            IosCameraDescription =
                "Tato aplikace potřebuje použít kameru pro účel interaktivní navigace.";

        public string
            IosLocationDescription =
                "Tato aplikace potřebuje přístup k poloze pro účel interaktivní navigace.";

        public void ChangeConfiguration()
        {

#if UNITY_EDITOR
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = AppName;
            PlayerSettings.bundleVersion = BuildVersion;
            PlayerSettings
                .SetApplicationIdentifier(BuildTargetGroup.Android,
                PackageName);
            PlayerSettings
                .SetApplicationIdentifier(BuildTargetGroup.iOS, PackageName);
            PlayerSettings.Android.bundleVersionCode = BundleVersion;
            PlayerSettings.iOS.buildNumber = BundleVersion.ToString();
            PlayerSettings.iOS.cameraUsageDescription = IosCameraDescription;
            PlayerSettings.iOS.locationUsageDescription =
                IosLocationDescription;
#endif
        }
    }

    public bool isDev = false;

    [SerializeField]
    public List<BaseAppConfig> listOfConfigs = new List<BaseAppConfig>(1);

    [SerializeField]
    public int choosedConfig = 0;

    void AddNew()
    {
        //Add a new index position to the end of our list
        listOfConfigs.Add(new BaseAppConfig());
    }

    void Remove(int index)
    {
        //Remove an index position from our list at a point in our list array
        listOfConfigs.RemoveAt (index);
    }

    // [PostProcessBuild]
    // public static void ChangeXcodePlist(
    //     BuildTarget buildTarget,
    //     string pathToBuiltProject,
    //     string usageLocationDescription,
    //     string usageCameraDescription
    // )
    // {
    //     if (buildTarget == BuildTarget.iOS)
    //     {
    //         // Get plist
    //         string plistPath = pathToBuiltProject + "/Info.plist";
    //         PlistDocument plist = new PlistDocument();
    //         plist.ReadFromString(File.ReadAllText(plistPath));
    //         // Get root
    //         PlistElementDict rootDict = plist.root;
    //         // background location useage key (new in iOS 8)
    //         rootDict
    //             .SetString("NSLocationAlwaysUsageDescription",
    //             usageLocationDescription);
    //         rootDict
    //             .SetString("NSCameraUsageDescription", usageCameraDescription);
    //         // Write to file
    //         File.WriteAllText(plistPath, plist.WriteToString());
    //     }
    // }
    public void Start()
    {
    }
}
