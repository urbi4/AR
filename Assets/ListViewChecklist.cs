using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ButtonExtension
{
    public static void AddEventListener<T>(this Button button, T param, Action<T> OnClick)
    {
        button.onClick.AddListener(delegate () {
            OnClick(param);
        });
    }
}

public class ListViewChecklist : MonoBehaviour
{
    public MyGameManager myGameManager;


    public GameObject buttonTemplate;
    GameObject g;
    public Dictionary<string, string> informationList = new Dictionary<string, string>();


    

    // Start is called before the first frame update
    void Start()
    {
        InitList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ItemClicked(GameObject g)
    {
        //Debug.Log("------------item " + name + " clicked---------------");
        //Beacon item = scannedItems.Find((x) => x.name == g.transform.GetChild(0).GetComponent<Text>().text);
        
        var colors = g.GetComponent<Button>().colors;
        colors.normalColor = Color.green;
        GetComponent<Button>().colors = colors;
        

        /*if (item.name.Contains("Q"))
        {
            myGameManager.receivedQ(item.name);
        }
        else
        {
            myGameManager.errorBLEModal.SetActive(true);
        }*/


    }

    public void InitList()
    {
        informationList.Add("Temperature", "One");
        informationList.Add("Velocity", "Two");
        foreach (var information in informationList)
        {
            g = Instantiate(buttonTemplate, transform);
            g.name = information.Key.ToString();
            g.transform.GetChild(0).GetComponent<Text>().text = information.Key.ToString();
            //g.transform.GetChild(1).GetComponent<Button>().text = information.Value.ToString();
        }

    }
}
