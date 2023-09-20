using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

// Trida pro komunikaci s BE API - potreba sjednotit ziskavani destinaci na BE, aby se destinace ziskavaly stejnym zpusobem
public class NavApi : MonoBehaviour
{
    public static bool languageChanged;
    public MyGameManager myGameManager;

    private Dictionary<string, NavItem>
        navItems = new Dictionary<string, NavItem>();

    private Dictionary<string, string>
        qMapper = new Dictionary<string, string>();

    private List<string> dropdownList = new List<string>();

    public Dropdown dropdown;

    public string ori;

    public string target;

    public bool destionationsLoaded = false;

    [SerializeField]
    public static AppsConfigurator.BaseAppConfig appsConfig;

    public bool isNNF = false;

    public bool isDemo = false;

    public bool isDev = false;

    // DEV
    // API pro ziskavani cesty
    private string API_DEVTOOLS_PATH
    {
        get
        {
            return isDev
                ? "https://api01-b.hekate.club/devTools/path/"
                : "https://api.nnf.cginzs.cz/devTools/path/";
        }
    }

    // API pro ziskani informaci o ceduli
    private string API_DEVTOOLS_SIGN
    {
        get
        {
            return isDev
                ? "https://api01-b.hekate.club/devTools/navigationSign/"
                : "https://api.nnf.cginzs.cz/devTools/navigationSign/";
        }
    }

    // API pro ziskani moznych destinaci
    private string API_DEVTOOLS_DESTINATIONS
    {
        get
        {
            return isDev
                ? "https://api01-b.hekate.club/devTools/items/"
                : "https://api.nnf.cginzs.cz/devTools/items/";
        }
    }

    //Api pro ziskani configurace
    private string API_CONFIGURATION = "https://api01-b.hekate.club/devTools/config";

    // PROD
    // private const string API_DEVTOOLS_PATH = "https://api.nnf.cginzs.cz/devTools/path/";
    // private const string API_DEVTOOLS_SIGN = "https://api.nnf.cginzs.cz/devTools/navigationSign/";
    // private const string API_DEVTOOLS_DESTINATIONS = "https://api.nnf.cginzs.cz/devTools/items/";
    private const string SIGN_ID = "signId";

    private const string ORI = "ori";

    private const string ORIGIN = "origin";

    private const string DESTINATION = "destination";

    private const string ORIENTATION = "orientation";

    private const string ENCODER = "encoder";

    private const string FOOT = "FOOT";

    private const string LABEL = "label";

    private const string LABELEN = "label_en";

    private const string IDENTIFIER = "identifier";

    private const string CGI_OFFICE_LONDON = "CGI Office London";


    private const string NADCH = "National Institute of Paediatric Diseases main building";

    private const string PROPERTIES = "properties";

    private const string CONSISTS_OF = "consists of";

    private const string DESCRIPTION = "description";

    private const string DESTINATION_LIST = "Destination list for navin demo app";

    [HideInInspector]
    private LocalizedString
        SELECT_PATH =
            new LocalizedString()
            { TableReference = "Locales", TableEntryReference = "FindAPlace" };

    public bool NavItemHasKey(string key)
    {
        return navItems.ContainsKey(key);
    }

    public NavItem NavItemGetValue(string key)
    {
        return navItems[key];
    }

    public bool QMapperHasKey(string key)
    {
        return qMapper.ContainsKey(key);
    }

    public string QMapperGetValue(string key)
    {
        return qMapper[key];
    }

    void Start()
    {
        languageChanged = false;
        if (DeviceTypeChecker.GetDeviceType() == ENUM_Device_Type.Tablet)
        {
            List<GameObject> gamesObjects = MyGameManager.FindAllObjectsInScene();

            dropdown = MyGameManager.FindGameObjectsAll(gamesObjects, "TabletDropdown").GetComponent<Dropdown>();
        }

        PrepareDropdown();
        /*webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(GetNavigationList);
        webClient.DownloadStringAsync(new Uri("https://data01-b.hekate.club/nnf/query/v1/items?class=Q48,Q114&return-referring=false"));*/
    }

    private void Update()
    {
        if (languageChanged)
        {
            languageChanged = false;
            PrepareDropdown();
        }
    }

    // Pripravime dropdown s moznymi destinacemi
    public void PrepareDropdown()
    {
        dropdownList.Clear();
        dropdownList.Add(LocaleUtils.GetLocalizetString(SELECT_PATH));

        dropdown.ClearOptions();
        dropdown.AddOptions (dropdownList);

        if (isDemo)
            GetConfiguration();
        else
            GetItemData();
    }

    // Ziskani destinaci
    private void GetItemData()
    {
        WebClient webClient = new WebClient();
        string credentials =
            Convert
                .ToBase64String(Encoding
                    .ASCII
                    .GetBytes(myGameManager.BASIC_AUTH_USERNAME +
                    ":" +
                    myGameManager.BASIC_AUTH_PASSWORD));
        webClient.Headers[HttpRequestHeader.Authorization] =
            string.Format("Basic {0}", credentials);
        webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
        if (LocalizationSettings.SelectedLocale.Identifier == "cs")
        {
            webClient.Headers[HttpRequestHeader.AcceptLanguage] = "cs-CZ";
        }
        else
        {
            webClient.Headers[HttpRequestHeader.AcceptLanguage] = "en-US";
        }

        // NNF vs DEMO vs NUDCH/Primatorky/NIS
        string classIds = isNNF ? "Q48,Q114" : isDemo ? "Q8662" : "Q9";

        webClient.QueryString.Add("class", classIds);
        webClient.DownloadStringCompleted +=
            new DownloadStringCompletedEventHandler(GetNavigationList);
        webClient.DownloadStringAsync(new Uri(API_DEVTOOLS_DESTINATIONS));
    }

    private void GetConfiguration()
    {
        WebClient webClient = new WebClient();
        string credentials =
            Convert
                .ToBase64String(Encoding
                    .ASCII
                    .GetBytes(myGameManager.BASIC_AUTH_USERNAME +
                    ":" +
                    myGameManager.BASIC_AUTH_PASSWORD));
        webClient.Headers[HttpRequestHeader.Authorization] =
            string.Format("Basic {0}", credentials);
        webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
        if (LocalizationSettings.SelectedLocale.Identifier == "cs")
        {
            webClient.Headers[HttpRequestHeader.AcceptLanguage] = "cs-CZ";
        }
        else
        {
            webClient.Headers[HttpRequestHeader.AcceptLanguage] = "en-US";
        }

        webClient.DownloadStringCompleted +=
            new DownloadStringCompletedEventHandler(GetDemoNavigationList);
        webClient.DownloadStringAsync(new Uri(API_CONFIGURATION));
    }


    // Spustit pri nacteni QR kodu a zjisteni informace o originu
    public void StartApi(String qrCode)
    {
        RequestParameters requestParameters = new RequestParameters();
        requestParameters.SetRequestParameters (qrCode);
        ori = requestParameters.GetValue(ORI);
        GetOrigin (ori);
    }


    public void StartApiId(String q)
    {
        ori = q;
        GetOrigin(q);
    }

    // Ziskani destinace pro CGI_OFFICE_LONDON a DEMO
    public List<string> ParseJsonData(string signJson)
    {
        char[] parameterDelimiters = new char[] { '/' };
        JArray objects = JArray.Parse(signJson);
        List<string> destinations = new List<string>();

        foreach (JObject itemObj in objects)
        {
            JToken labelEn = itemObj["label_en"];
            if (labelEn.Value<string>() == CGI_OFFICE_LONDON || isDemo)
            {
                foreach (JObject prop in itemObj[PROPERTIES])
                {
                    if (prop["label_en"].Value<string>() == CONSISTS_OF)
                    {
                        string identifier =
                            prop["value"][IDENTIFIER].ToString();
                        string label = prop["value"][LABEL].ToString();
                        string description =
                            prop["value"][DESCRIPTION].Value<string>();

                        string[] splittedString =
                            identifier
                                .Split(parameterDelimiters,
                                StringSplitOptions.RemoveEmptyEntries);
                        string finalIdentifier =
                            splittedString[splittedString.Length - 1];
                        NavItem navItem =
                            new NavItem(finalIdentifier, label, description);
                        navItems.Add (label, navItem);
                        qMapper.Add (finalIdentifier, label);
                        destinations.Add (label);
                    }
                }
            }
        }

        return destinations;
    }

    // Byli na pevno pro Primatorky/NUDCH, ted uz se bere z Q1 z wikibase
    public List<string> ParseDemoData(string signJson)
    {
        char[] parameterDelimiters = new char[] { '/' };
        JObject jObject = JObject.Parse(signJson);
        JArray objects = FilterByLabelEn(DESTINATION_LIST, jObject);
        List<string> destinations = new List<string>();


        // TODO FIX THIS WHEN CRYPTO WILL BE DONE

        /* NUDCH
        if (LocalizationSettings.SelectedLocale.Identifier == "cs")
        {
            String label1 = "Urologická ambulance 1";
            String finalIdentifier1 = "Q14198";
            String description1 = "NÚDCH";
            NavItem navItem1 = new NavItem(finalIdentifier1, label1, description1);
            navItems.Add(label1, navItem1);
            qMapper.Add(finalIdentifier1, label1);
            destinations.Add(label1);

            String label2 = "Urodynamická a urologická ambulance 2";
            String finalIdentifier2 = "Q14199";
            String description2 = "NÚDCH";
            NavItem navItem2 = new NavItem(finalIdentifier2, label2, description2);
            navItems.Add(label2, navItem2);
            qMapper.Add(finalIdentifier2, label2);
            destinations.Add(label2);
        }
        else
        {
            String label1 = "Urological outpatient clinic 1";
            String finalIdentifier1 = "Q14198";
            String description1 = "NÚDCH";
            NavItem navItem1 = new NavItem(finalIdentifier1, label1, description1);
            navItems.Add(label1, navItem1);
            qMapper.Add(finalIdentifier1, label1);
            destinations.Add(label1);

            String label2 = "Urodynamic and urological outpatient clinic 2";
            String finalIdentifier2 = "Q14199";
            String description2 = "NÚDCH";
            NavItem navItem2 = new NavItem(finalIdentifier2, label2, description2);
            navItems.Add(label2, navItem2);
            qMapper.Add(finalIdentifier2, label2);
            destinations.Add(label2);
        }

        foreach (JObject itemObj in objects)
        {
            string identifier =
                         itemObj[IDENTIFIER].ToString();
            string label = itemObj[LABEL].ToString();
            string description =
                itemObj[DESCRIPTION].Value<string>();

            string[] splittedString =
                identifier
                    .Split(parameterDelimiters,
                    StringSplitOptions.RemoveEmptyEntries);
            string finalIdentifier =
                splittedString[splittedString.Length - 1];
            NavItem navItem =
                new NavItem(finalIdentifier, label, description);
            navItems.Add(label, navItem);
            qMapper.Add(finalIdentifier, label);
            destinations.Add(label);

        }
        */

        // Primatorky
        /*if (LocalizationSettings.SelectedLocale.Identifier == "cs")
        {
            String label1 = "Toalety ČEZ Primátorky";
            String finalIdentifier1 = "Q14500";
            String description1 = "ČEZ Primátorky";
            NavItem navItem1 = new NavItem(finalIdentifier1, label1, description1);
            navItems.Add(label1, navItem1);
            qMapper.Add(finalIdentifier1, label1);
            destinations.Add(label1);

            String label2 = "Východ 001 - ČEZ Primátorky areál";
            String finalIdentifier2 = "Q14508";
            String description2 = "ČEZ Primátorky areál";
            NavItem navItem2 = new NavItem(finalIdentifier2, label2, description2);
            navItems.Add(label2, navItem2);
            qMapper.Add(finalIdentifier2, label2);
            destinations.Add(label2);

            String label3 = "Východ 002 - ČEZ Primátorky areál";
            String finalIdentifier3 = "Q14509";
            String description3 = "ČEZ Primátorky areál";
            NavItem navItem3 = new NavItem(finalIdentifier3, label3, description3);
            navItems.Add(label3, navItem3);
            qMapper.Add(finalIdentifier3, label3);
            destinations.Add(label3);

            String label4 = "Cíl";
            String finalIdentifier4 = "Q14512";
            String description4 = "ČEZ Primátorky";
            NavItem navItem4 = new NavItem(finalIdentifier4, label4, description4);
            navItems.Add(label4, navItem4);
            qMapper.Add(finalIdentifier4, label4);
            destinations.Add(label4);

            String label5 = "Tribuna";
            String finalIdentifier5 = "Q14513";
            String description5 = "ČEZ Primátorky";
            NavItem navItem5 = new NavItem(finalIdentifier5, label5, description5);
            navItems.Add(label5, navItem5);
            qMapper.Add(finalIdentifier5, label5);
            destinations.Add(label5);

            String label6 = "Občerstvení";
            String finalIdentifier6 = "Q14514";
            String description6 = "ČEZ Primátorky";
            NavItem navItem6 = new NavItem(finalIdentifier6, label6, description6);
            navItems.Add(label6, navItem6);
            qMapper.Add(finalIdentifier6, label6);
            destinations.Add(label6);

            String label7 = "CGI Stánek";
            String finalIdentifier7 = "Q14515";
            String description7 = "ČEZ Primátorky";
            NavItem navItem7 = new NavItem(finalIdentifier7, label7, description7);
            navItems.Add(label7, navItem7);
            qMapper.Add(finalIdentifier7, label7);
            destinations.Add(label7);
        }
        else
        {
            String label1 = "Toilets ČEZ Primátorky";
            String finalIdentifier1 = "Q14500";
            String description1 = "ČEZ Primátorky";
            NavItem navItem1 = new NavItem(finalIdentifier1, label1, description1);
            navItems.Add(label1, navItem1);
            qMapper.Add(finalIdentifier1, label1);
            destinations.Add(label1);

            String label2 = "Entrance 001 - ČEZ Primátorky areal";
            String finalIdentifier2 = "Q14508";
            String description2 = "ČEZ Primátorky areal";
            NavItem navItem2 = new NavItem(finalIdentifier2, label2, description2);
            navItems.Add(label2, navItem2);
            qMapper.Add(finalIdentifier2, label2);
            destinations.Add(label2);

            String label3 = "Entrance 002 - ČEZ Primátorky areal";
            String finalIdentifier3 = "Q14509";
            String description3 = "ČEZ Primátorky areal";
            NavItem navItem3 = new NavItem(finalIdentifier3, label3, description3);
            navItems.Add(label3, navItem3);
            qMapper.Add(finalIdentifier3, label3);
            destinations.Add(label3);

            String label4 = "Finish";
            String finalIdentifier4 = "Q14512";
            String description4 = "ČEZ Primátorky";
            NavItem navItem4 = new NavItem(finalIdentifier4, label4, description4);
            navItems.Add(label4, navItem4);
            qMapper.Add(finalIdentifier4, label4);
            destinations.Add(label4);

            String label5 = "Tribune";
            String finalIdentifier5 = "Q14513";
            String description5 = "ČEZ Primátorky";
            NavItem navItem5 = new NavItem(finalIdentifier5, label5, description5);
            navItems.Add(label5, navItem5);
            qMapper.Add(finalIdentifier5, label5);
            destinations.Add(label5);

            String label6 = "Refreshment";
            String finalIdentifier6 = "Q14514";
            String description6 = "ČEZ Primátorky";
            NavItem navItem6 = new NavItem(finalIdentifier6, label6, description6);
            navItems.Add(label6, navItem6);
            qMapper.Add(finalIdentifier6, label6);
            destinations.Add(label6);

            String label7 = "CGI Booth";
            String finalIdentifier7 = "Q14515";
            String description7 = "ČEZ Primátorky";
            NavItem navItem7 = new NavItem(finalIdentifier7, label7, description7);
            navItems.Add(label7, navItem7);
            qMapper.Add(finalIdentifier7, label7);
            destinations.Add(label7);
        }*/
        /* NIS
        if (LocalizationSettings.SelectedLocale.Identifier == "cs")
        {
            String label1 = "Urologická ambulance 1";
            String finalIdentifier1 = "Q14198";
            String description1 = "NÚDCH";
            NavItem navItem1 = new NavItem(finalIdentifier1, label1, description1);
            navItems.Add(label1, navItem1);
            qMapper.Add(finalIdentifier1, label1);
            destinations.Add(label1);

            String label2 = "Urodynamická a urologická ambulance 2";
            String finalIdentifier2 = "Q14199";
            String description2 = "NÚDCH";
            NavItem navItem2 = new NavItem(finalIdentifier2, label2, description2);
            navItems.Add(label2, navItem2);
            qMapper.Add(finalIdentifier2, label2);
            destinations.Add(label2);

            String label3 = "Urodynamická a urologická ambulance 2";
                String finalIdentifier3 = "Q14199";
                String description3 = "NÚDCH";
                NavItem navItem3 = new NavItem(finalIdentifier3, label3, description3);
                navItems.Add(label3, navItem3);
                qMapper.Add(finalIdentifier3, label3);
                destinations.Add(label3);

            String label4 = "Urodynamická a urologická ambulance 2";
                String finalIdentifier4 = "Q14199";
                String description4 = "NÚDCH";
                NavItem navItem4 = new NavItem(finalIdentifier4, label4, description4);
                navItems.Add(label4, navItem4);
                qMapper.Add(finalIdentifier4, label4);
                destinations.Add(label4);

            String label5 = "Urodynamická a urologická ambulance 2";
                String finalIdentifier5 = "Q14199";
                String description5 = "NÚDCH";
                NavItem navItem5 = new NavItem(finalIdentifier5, label5, description5);
                navItems.Add(label5, navItem5);
                qMapper.Add(finalIdentifier5, label5);
                destinations.Add(label5);
        }
        else
        {
            String label1 = "Urological outpatient clinic 1";
            String finalIdentifier1 = "Q14198";
            String description1 = "NÚDCH";
            NavItem navItem1 = new NavItem(finalIdentifier1, label1, description1);
            navItems.Add(label1, navItem1);
            qMapper.Add(finalIdentifier1, label1);
            destinations.Add(label1);

            String label2 = "Urodynamic and urological outpatient clinic 2";
            String finalIdentifier2 = "Q14199";
            String description2 = "NÚDCH";
            NavItem navItem2 = new NavItem(finalIdentifier2, label2, description2);
            navItems.Add(label2, navItem2);
            qMapper.Add(finalIdentifier2, label2);
            destinations.Add(label2);

            String label3 = "Urodynamická a urologická ambulance 2";
                String finalIdentifier3 = "Q14199";
                String description3 = "NÚDCH";
                NavItem navItem3 = new NavItem(finalIdentifier3, label3, description3);
                navItems.Add(label3, navItem3);
                qMapper.Add(finalIdentifier3, label3);
                destinations.Add(label3);

            String label4 = "Urodynamická a urologická ambulance 2";
                String finalIdentifier4 = "Q14199";
                String description4 = "NÚDCH";
                NavItem navItem4 = new NavItem(finalIdentifier4, label4, description4);
                navItems.Add(label4, navItem4);
                qMapper.Add(finalIdentifier4, label4);
                destinations.Add(label4);

            String label5 = "Urodynamická a urologická ambulance 2";
                String finalIdentifier5 = "Q14199";
                String description5 = "NÚDCH";
                NavItem navItem5 = new NavItem(finalIdentifier5, label5, description5);
                navItems.Add(label5, navItem5);
                qMapper.Add(finalIdentifier5, label5);
                destinations.Add(label5);
        }
        */

        foreach (JObject itemObj in objects)
        {
            string identifier =
                         itemObj[IDENTIFIER].ToString();
            string label = itemObj[LABELEN].ToString();
            string description =
                itemObj[DESCRIPTION].Value<string>();

            string[] splittedString =
                identifier
                    .Split(parameterDelimiters,
                    StringSplitOptions.RemoveEmptyEntries);
            string finalIdentifier =
                splittedString[splittedString.Length - 1];
            NavItem navItem =
                new NavItem(finalIdentifier, label, description);
            navItems.Add(label, navItem);
            qMapper.Add(finalIdentifier, label);
            destinations.Add(label);

        }

        return destinations;
    }

    public JArray FilterByLabelEn(string labelEn, JObject objects)
    {
        JArray array = new JArray();
        foreach (JObject prop in objects[PROPERTIES])
        {
            if (prop["label_en"] != null)
            {
                JToken jLabelEn = prop["label_en"];
                if (jLabelEn.Value<string>() == labelEn && prop["value"] != null)
                    array.Add(prop["value"]);
            }
        }
        return array;
    }


    public JArray FilterByLocationLabel(string location, JArray objects)
    {
        JArray array = new JArray();


        foreach (JObject itemObj in objects)
        {
            foreach (JObject prop in itemObj[PROPERTIES])
            {
                if(prop["value"]["label_en"] != null) {

                    JToken labelEn = prop["value"]["label_en"];
                    if (labelEn.Value<string>() == location)
                    {
                        array.Add(itemObj);
                    }
                }

              
            }
        }
        return array;
    }
    public List<string> filterClassIds(string signJson)
    {
        char[] parameterDelimiters = new char[] { '/' };
        List<string> classIds = new List<string>();
        JArray objects = JArray.Parse(signJson);


        foreach (JObject itemObj in objects)
        {
            string[] test = itemObj["identifier"].ToString().Split(parameterDelimiters, StringSplitOptions.RemoveEmptyEntries);
            classIds.Add(test.Last<string>());
        }
        return classIds;
    }

    // Ziskani destinaci pro NNF
    public List<string> ParseNNFData(string signJson)
    {
        JArray coordsSearch = JArray.Parse(signJson);

        char[] parameterDelimiters = new char[] { '/' };
        List<string> destinations = new List<string>();

        foreach (JObject destination in coordsSearch.Children<JObject>())
        {
            string identifier = destination[IDENTIFIER].ToString();
            string label = destination[LABEL].ToString();

            String description = "";
            for (int i = 0; i < destination["properties"].Count(); i++)
            {
                if (
                    destination["properties"][i]["label_en"].ToString() ==
                    "placement"
                )
                {
                    var properties =
                        destination["properties"][i]["value"]["label"];
                    if (properties != null)
                    {
                        if (description == "")
                        {
                            description = properties.ToString();
                        }
                        else
                        {
                            description += " | " + properties.ToString();
                        }
                    }
                }
            }

            string[] splittedString =
                identifier
                    .Split(parameterDelimiters,
                    StringSplitOptions.RemoveEmptyEntries);
            string finalIdentifier = splittedString[splittedString.Length - 1];
            if (
                finalIdentifier == "Q168" ||
                finalIdentifier == "Q1905" ||
                finalIdentifier == "Q11294"
            )
            {
                NavItem navItem =
                    new NavItem(finalIdentifier, label, description);
                navItems.Add (label, navItem);
                qMapper.Add (finalIdentifier, label);
                destinations.Add (label);
            }
        }
        return destinations;
    }

    // Destinace pro DEMO
    void PrepareDataForDropDown(string signJson)
    {
        List<string> classIds = filterClassIds(signJson);
        WebClient webClient = new WebClient();
        string credentials =
            Convert
                .ToBase64String(Encoding
                    .ASCII
                    .GetBytes(myGameManager.BASIC_AUTH_USERNAME +
                    ":" +
                    myGameManager.BASIC_AUTH_PASSWORD));
        webClient.Headers[HttpRequestHeader.Authorization] =
            string.Format("Basic {0}", credentials);
        webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
        if (LocalizationSettings.SelectedLocale.Identifier == "cs")
        {
            webClient.Headers[HttpRequestHeader.AcceptLanguage] = "cs-CZ";
        }
        else
        {
            webClient.Headers[HttpRequestHeader.AcceptLanguage] = "en-US";
        }
        List<string> uniqueList = classIds.Distinct().ToList();
        webClient.QueryString.Add("class", string.Join(",", uniqueList));
        webClient.QueryString.Add("return-reffering", "true");
        webClient.DownloadStringCompleted +=
           new DownloadStringCompletedEventHandler(GetDemoNavigationList);
        webClient.DownloadStringAsync(new Uri(API_DEVTOOLS_DESTINATIONS));
    }


    void FillDropdown(string signJson)
    {
        navItems.Clear();
        dropdownList.Clear();
        qMapper.Clear();
        dropdownList.Add(LocaleUtils.GetLocalizetString(SELECT_PATH));
        List<string> destinations =
            isNNF ? ParseNNFData(signJson) : isDemo ? ParseDemoData(signJson) : ParseJsonData(signJson);
        destinations.Sort();
        dropdownList.AddRange (destinations);
        dropdown.ClearOptions();
        dropdown.AddOptions (dropdownList);

        destionationsLoaded = true;
        DrawNavigationPath();
    }

    // Vykresleni cesty
    public void DrawNavigationPath()
    {
        if (destionationsLoaded && target != null)
        {
            int? dropdownValue = GetQIndexInDictionary(target);
            if (dropdownValue != null)
            {
                dropdown.value = dropdownValue.Value;
                ChangeDropdownValue (target);
            }
        }
    }

    public int? GetQIndexInDictionary(String target)
    {
        foreach (KeyValuePair<string, NavItem> pair in navItems)
        {
            if (pair.Value.FinalIdentifier.Equals(target))
            {
                var listAvailableStrings =
                    dropdown.options.Select(option => option.text).ToList();
                return listAvailableStrings.IndexOf(pair.Key);
            }
        }

        return null;
    }

    // Ziskani informaci o pocatku
    public void GetOrigin(String ori)
    {
        try
        {
            Debug.Log("ScannerBLE "+ ori);
            WebClient webClient = new WebClient();
            string credentials =
                Convert
                    .ToBase64String(Encoding
                        .ASCII
                        .GetBytes(myGameManager.BASIC_AUTH_USERNAME +
                        ":" +
                        myGameManager.BASIC_AUTH_PASSWORD));
            webClient.Headers[HttpRequestHeader.Authorization] =
                string.Format("Basic {0}", credentials);
            webClient.QueryString.Add (SIGN_ID, ori);
            webClient.DownloadStringCompleted +=
                new DownloadStringCompletedEventHandler(GetOriginHandler);
            webClient.DownloadStringAsync(new Uri(API_DEVTOOLS_SIGN));
        }
        catch (WebException e)
        {
            Debug.Log("GetOrigin API ERROR " + e);
        }
    }

    // Handler pro ziskani informaci o pocatku
    void GetOriginHandler(object sender, DownloadStringCompletedEventArgs json)
    {
        if (json.Error != null)
        {
            Debug.Log("GetOriginHandler Error - " + json.Error.Message);
            return;
        }

        JObject coordsSearch = JObject.Parse(json.Result);
        myGameManager.origin.text = coordsSearch[LABEL].ToString();
        myGameManager.signOrientation.transform.eulerAngles =
            new Vector3(0, -(float) coordsSearch[ORIENTATION], 0);
    }

    // Event handler pro ziskani destinaci u DEMA
    void GetDemoNavigationList(object sender, DownloadStringCompletedEventArgs json)
    {
        if (json.Error != null)
        {
            Debug.Log("GetNavigationList Error - " + json.Error.Message);
            return;
        }
        FillDropdown(json.Result);
    }

    // Event handler pro ziskani destinaci
    void GetNavigationList(object sender, DownloadStringCompletedEventArgs json)
    {
        if (json.Error != null)
        {
            Debug.Log("GetNavigationList Error - " + json.Error.Message);
            StartCoroutine(TryAgainGetItemData());   // Animace nacitani
            return;
        }
        if (isDemo) PrepareDataForDropDown(json.Result); else FillDropdown(json.Result);
    }

    // Pokud nastala chyba - napr. nebyl dostupny internet - opakuje se ziskavani destinaci
    IEnumerator TryAgainGetItemData()
    {
        yield return new WaitForSeconds(1f);
        GetItemData();
    }

    // Pri vyberu destinace z dropdownu
    public void DropdownValueChanged(Dropdown change)
    {
        if (change.value == 0)
        {
            return;
        }

        ChangeDropdownValue (change);
    }

    // Pri vyberu destinace z dropdownu, provolej API a nacti cestu k cili
    public void ChangeDropdownValue(Dropdown change)
    {
        //ori = "Q407";
        try
        {
            myGameManager.arrowGenerator.DestroyCompleteWay();
            myGameManager.PrimaryMark.SetActive(false);
            this.target =
                navItems[change.options[change.value].text].FinalIdentifier;
            WebClient webClient = new WebClient();
            string credentials =
                Convert
                    .ToBase64String(Encoding
                        .ASCII
                        .GetBytes(myGameManager.BASIC_AUTH_USERNAME +
                        ":" +
                        myGameManager.BASIC_AUTH_PASSWORD));
            webClient.Headers[HttpRequestHeader.Authorization] =
                string.Format("Basic {0}", credentials);
            webClient.QueryString.Add (ORIGIN, ori);
            webClient
                .QueryString
                .Add(DESTINATION,
                navItems[change.options[change.value].text].FinalIdentifier);
            webClient.QueryString.Add (ENCODER, FOOT);
            webClient.DownloadStringCompleted +=
                new DownloadStringCompletedEventHandler(GetNavigationPath);
            webClient.DownloadStringAsync(new Uri(API_DEVTOOLS_PATH));

            myGameManager.DestinationInfo.GetComponent<Text>().text =
                LocaleUtils.GetLocalizetString(myGameManager.LOADING_PATH);
            myGameManager
                .NavigationLabel
                .GetComponent<RectTransform>()
                .sizeDelta =
                new Vector2(myGameManager.NavigationLabelDefaultWidth,
                    myGameManager.NavigationLabelDefaultHeight);
            myGameManager.PathPanel.SetActive(false);
        }
        catch (WebException e)
        {
            Debug.Log("DropdownValueChanged API ERROR " + e);
            change.value = 0;
        }
    }

    // Pri vyberu destinace pres string target
    public void ChangeDropdownValue(String target)
    {
        try
        {
            myGameManager.arrowGenerator.DestroyCompleteWay();
            this.target = target;
            WebClient webClient = new WebClient();
            string credentials =
                Convert
                    .ToBase64String(Encoding
                        .ASCII
                        .GetBytes(myGameManager.BASIC_AUTH_USERNAME +
                        ":" +
                        myGameManager.BASIC_AUTH_PASSWORD));
            webClient.Headers[HttpRequestHeader.Authorization] =
                string.Format("Basic {0}", credentials);
            webClient.QueryString.Add (ORIGIN, ori);
            webClient.QueryString.Add (DESTINATION, target);
            webClient.QueryString.Add (ENCODER, FOOT);
            webClient.DownloadStringCompleted +=
                new DownloadStringCompletedEventHandler(GetNavigationPath);
            webClient.DownloadStringAsync(new Uri(API_DEVTOOLS_PATH));

            myGameManager.DestinationInfo.GetComponent<Text>().text =
                LocaleUtils.GetLocalizetString(myGameManager.LOADING_PATH);
            myGameManager
                .NavigationLabel
                .GetComponent<RectTransform>()
                .sizeDelta =
                new Vector2(myGameManager.NavigationLabelDefaultWidth,
                    myGameManager.NavigationLabelDefaultHeight);
            myGameManager.PathPanel.SetActive(false);
        }
        catch (WebException e)
        {
            Debug.Log("DropdownValueChanged API ERROR " + e);
            dropdown.value = 0;
        }
    }

    // Handler pro ziskani cesty
    void GetNavigationPath(object sender, DownloadStringCompletedEventArgs json)
    {
        if (json.Error != null)
        {
            Debug.Log("GetNavigationPath Error - " + json.Error.Message);
            return;
        }

        myGameManager.arrowGenerator.CreateCompleteWay(json.Result);
        myGameManager.NavigationLabel.GetComponent<RectTransform>().sizeDelta =
            new Vector2(myGameManager.NavigationLabelDefaultWidth,
                myGameManager.NavigationLabelNavigatingHeight);
    }

    // public static void SetChoosedConfig(
    //     AppsConfigurator.BaseAppConfig config,
    //     bool isDev
    // )
    // {
    //     baseAppConfig = config;
    //     isDev = isDev;
    // }
}
