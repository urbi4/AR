using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ListViewScript : MonoBehaviour
{
    public MyGameManager myGameManager;
    

    public GameObject buttonTemplate;
    GameObject g;
    public Dictionary<string, string> informationList = new Dictionary<string, string>();

    
    
    // Start is called before the first frame update
    void Start()
    {
        // metoda pro ziskani informacei
        
        InitList();
    }
    private void Update()
    {
        
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
             g.transform.GetChild(1).GetComponent<Text>().text = information.Value.ToString();
        }

    }

    
}
