using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Trida pro deeplink z webove stranky, automaticky nacte pocatek a destinaci z linku (nyni se nepouziva)
public class ProcessDeepLinkMngr : MonoBehaviour
{
    public MyGameManager myGameManager;

    public static ProcessDeepLinkMngr Instance { get; private set; }
    public string deeplinkURL = null;
    private const string ORIGIN_DESTINATION = "originDestination";
    private const string TARGET_DESTINATION = "targetDestination";
    public string origin = null;
    public string target = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Application.deepLinkActivated += onDeepLinkActivated;
            //onDeepLinkActivated("unitydl://mylink?originDestination=Q407&targetDestination=Q178://scan/");


            if (!String.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                onDeepLinkActivated(Application.absoluteURL);
            }
            // Initialize DeepLink Manager global variable.
            else
            {
                deeplinkURL = null;
                myGameManager.DefaultPhaze();
            } 
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void onDeepLinkActivated(string url)
    {
        // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
        deeplinkURL = url;

        RequestParameters requestParameters = new RequestParameters();
        requestParameters.SetRequestParameters(deeplinkURL);
        origin = requestParameters.GetValue(ORIGIN_DESTINATION);
        target = requestParameters.GetValue(TARGET_DESTINATION);
        myGameManager.navApi.ori = origin;
        myGameManager.navApi.target = target;

        myGameManager.navApi.GetOrigin(origin);
        myGameManager.ARPhaze();
        
        /*
        // Decode the URL to determine action. 
        // In this example, the app expects a link formatted like this:
        // unitydl://mylink?scene1
        string sceneName = url.Split("?"[0])[1];
        bool validScene;
        switch (sceneName)
        {
            case "scene1":
                validScene = true;
                break;
            case "scene2":
                validScene = true;
                break;
            default:
                validScene = false;
                break;
        }
        if (validScene) SceneManager.LoadScene(sceneName);*/
    }
}
