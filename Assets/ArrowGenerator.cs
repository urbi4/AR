using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Trida pro generovani cesty pomoci 3D sipek + pomocne funkce
public class ArrowGenerator : MonoBehaviour
{
    public MyGameManager myGameManager;

    public GameObject arrow0;
    public GameObject arrow1;

    public GameObject completeWay;
    
    //CompassBehaviour compassBehaviour;

    private const float SCALE = 0.3F; // 5
    private const float SPACE = 0.5F; // 7.5
    private const string GEO_JSON = "geoJson";
    private const string FEATURES = "features";
    private const string GEOMETRY = "geometry";
    private const string TYPE = "type";
    private const string LINESTRING = "LineString";
    private const string COORDINATES = "coordinates";


    int arrowId = 0;
    List<GameObject> listOfArrows = new List<GameObject>();
    float lastPositionX = 0;
    float lastPositionY = 0;

    double completeWayStartAlt = 0;

    // Vypocet uhlu mezi dvemi souradnicemi
    double GetAngle(double startLat, double startLng, double destLat, double destLng)
    {
        startLat = ToRadians(startLat);
        startLng = ToRadians(startLng);
        destLat = ToRadians(destLat);
        destLng = ToRadians(destLng);

        double y = Math.Sin(destLng - startLng) * Math.Cos(destLat);
        double x = Math.Cos(startLat) * Math.Sin(destLat) - Math.Sin(startLat) * Math.Cos(destLat) * Math.Cos(destLng - startLng);
        double angle = Math.Atan2(y, x);
        angle = (ToDegrees(angle) + 360) % 360;

        return Math.Round(angle * 10) / 10;
    }

    // Vypocet vzdalenosti mezi dvemi souradnicemi
    double GetDistance(double startLat, double startLng, double destLat, double destLng)
    {
        int R = 6371; // km
        var dLat = ToRadians(destLat - startLat);
        var dLon = ToRadians(destLng - startLng);
        var lat1 = ToRadians(startLat);
        var lat2 = ToRadians(destLat);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = R * c;
        // km to meters
        return (float)Math.Round(distance * 10000) / 10;
    }

    // Prevod stupmnu na radiany
    double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    // Prevod radianu na stupne
    double ToDegrees(double radians)
    {
        return radians * 180 / Math.PI;
    }

    // Vytvoreni jednoho useku cesty z 3D sipek mezi dvema POI
    void CreateSingleWay(float angle, float distance, float altDiff, float completeAltDiff)
    {
        var finalDistance = ((distance - SPACE) / SPACE);
        int roundedFinalDistance = (int)finalDistance;
        
        var zStep = (roundedFinalDistance == 0) ? 0 : altDiff / roundedFinalDistance;

        if (arrowId == 0)
        {
            listOfArrows.Add(CreateArrow(angle, lastPositionX - 1 * SPACE * (float)Math.Cos(ToRadians(angle)), lastPositionY - 1 * SPACE * (float)Math.Sin(ToRadians(angle)), (zStep * -1) + completeAltDiff));
        }

        for (int i = 0; i < finalDistance; i++)
        {
            listOfArrows.Add(CreateArrow(angle, lastPositionX + i * SPACE * (float)Math.Cos(ToRadians(angle)), lastPositionY + i * SPACE * (float)Math.Sin(ToRadians(angle)), (zStep * i) + completeAltDiff));
        }
        lastPositionX += (distance / SPACE) * SPACE * (float)Math.Cos(ToRadians(angle));
        lastPositionY += (distance / SPACE) * SPACE * (float)Math.Sin(ToRadians(angle));
    }

    // Vytvoreni 3D sipky a jeji napozicovani
    GameObject CreateArrow(float angle, float positionX, float positionY, float positionZ)
    {
        GameObject arrow = null;
        arrowId = arrowId + 1;

        // Stridani sipek svetle modra/tmava modra
        if (arrowId % 2 == 0)
        {
            arrow = Instantiate(arrow0, new Vector3(0, 0, 0), Quaternion.identity);
        }
        if (arrowId % 2 == 1)
        {
            arrow = Instantiate(arrow1, new Vector3(0, 0, 0), Quaternion.identity);
        }

        // Rodice nastavime, abychom mohli pote celou cestu vyrotovat dle signOrientation
        arrow.transform.SetParent(myGameManager.completeWay.transform);
        arrow.transform.localRotation = Quaternion.Euler(0, angle + 180, 0);
        arrow.transform.localScale = new Vector3(SCALE, SCALE, SCALE);
        arrow.transform.localPosition = new Vector3(positionY, positionZ, positionX);

        return arrow;
    }

    // Smazani kompletni cesty pri zadani nove cesty
    public void DestroyCompleteWay() {
        foreach (GameObject arrow in listOfArrows)
        {
            Destroy(arrow);
        }

        lastPositionX = 0;
        lastPositionY = 0;
        arrowId = 0;
    }

    // Zmena obrazku podle typu primary mark
    private void ChangePrimaryMarkType(String type) {
        switch (type)
        {
            case "B":
                myGameManager.PrimaryMarkTypeImage.sprite = myGameManager.BuildingImage;
                break;
            case "E":
                myGameManager.PrimaryMarkTypeImage.sprite = myGameManager.ExitImage;
                break;
            case "D":
                myGameManager.PrimaryMarkTypeImage.sprite = myGameManager.DoorImage;
                break;
            case "S":
                myGameManager.PrimaryMarkTypeImage.sprite = myGameManager.StairsImage;
                break;
            case "V":
                myGameManager.PrimaryMarkTypeImage.sprite = myGameManager.ElevatorImage;
                break;
            case "C":
                myGameManager.PrimaryMarkTypeImage.sprite = myGameManager.CorridorImage;
                break;
            default:
                myGameManager.PrimaryMarkTypeImage.sprite = myGameManager.DestinationImage;
                break;
        }
    }


    private void ChangePrimaryMarkBuilding(String building)
    {
        switch (building)
        {
            case "A":
                myGameManager.PrimaryMarkBuildingImage.sprite = myGameManager.BuildingA;
                break;
            case "B":
                myGameManager.PrimaryMarkBuildingImage.sprite = myGameManager.BuildingB;
                break;
            case "C":
                myGameManager.PrimaryMarkBuildingImage.sprite = myGameManager.BuildingC;
                break;
            default:
                myGameManager.PrimaryMarkBuildingImage.sprite = myGameManager.BuildingNone;
                break;
        }
    }

    // Vytvoreni cele cesty od originu do destinace z jednotlivych useku
    public void CreateCompleteWay(String navJson)
    {
        DestroyCompleteWay();
        var lineStringCounter = 0;
        float totalDistance = 0;
        float totalTime = 0;

        JObject coordsSearch = JObject.Parse(navJson);

        if (coordsSearch["instructions"][0]["primaryMark"] != null)
        {
            String distance = "";
            String name = "";
            String type = "";
            String building = "";

            JToken distanceToken = coordsSearch["instructions"][0]["distance"];
            JToken nameToken = coordsSearch["instructions"][0]["primaryMark"]["name"];
            JToken typeToken = coordsSearch["instructions"][0]["primaryMark"]["type"];
            JToken buildingToken = coordsSearch["instructions"][0]["primaryMark"]["building"];

            if (distanceToken != null)
            {
                distance = Mathf.Round((float)distanceToken).ToString();
            }
            if (nameToken != null)
            {
                name = nameToken.ToString();
            }
            if (typeToken != null)
            {
                type = typeToken.ToString();
            }
            if (buildingToken != null)
            {
                building = buildingToken.ToString();
            }

            myGameManager.PrimaryMark.SetActive(true);
            myGameManager.PrimaryMarkText.text = "<b>" + name + "</b> | " + distance + "m";
            ChangePrimaryMarkType(type);
            ChangePrimaryMarkBuilding(building);
        }
        else {
            myGameManager.PrimaryMark.SetActive(false);
        }

        for (int i = 0; i < coordsSearch[GEO_JSON][FEATURES].Count(); i++)
        {
            if (coordsSearch[GEO_JSON][FEATURES][i][GEOMETRY][TYPE].ToString() == LINESTRING)
            {
                // Koordinaty jednotlivych bodu, pozor prvni vraci API lon a druhy lat
                double startLat = (double)coordsSearch[GEO_JSON][FEATURES][i][GEOMETRY][COORDINATES][0][1];
                double startLng = (double)coordsSearch[GEO_JSON][FEATURES][i][GEOMETRY][COORDINATES][0][0];
                double endLat = (double)coordsSearch[GEO_JSON][FEATURES][i][GEOMETRY][COORDINATES][1][1];
                double endLng = (double)coordsSearch[GEO_JSON][FEATURES][i][GEOMETRY][COORDINATES][1][0];
                double startAlt = 0;
                double endAlt = 0;

                if (coordsSearch[GEO_JSON][FEATURES][i][GEOMETRY][COORDINATES][0].Count() >= 3 &&
                     coordsSearch[GEO_JSON][FEATURES][i][GEOMETRY][COORDINATES][1].Count() >= 3)
                {
                    startAlt = (double)coordsSearch[GEO_JSON][FEATURES][i][GEOMETRY][COORDINATES][0][2];
                    endAlt = (double)coordsSearch[GEO_JSON][FEATURES][i][GEOMETRY][COORDINATES][1][2];
                }

                double distance = GetDistance(startLat, startLng, endLat, endLng);
                double angle = GetAngle(startLat, startLng, endLat, endLng);


                if (i == 0)
                {
                    completeWayStartAlt = startAlt;
                }
                double completeAltDiff = startAlt - completeWayStartAlt;
                double altDiff = endAlt - startAlt;

                CreateSingleWay((float)angle, (float)distance, (float)altDiff, (float)completeAltDiff);

                //totalDistance += (float)distance;
                lineStringCounter++;
            }

            // Nevykreslujeme celou cestu, ale jen cast
            //TODO: Zjistit jak nejdelší cestu můžeme zobrazovat bez memory leaku
            if (lineStringCounter >= 7)
            {
                break;
            }
        }

        for (int i = 0; i < coordsSearch["instructions"].Count(); i++)
        {
            double distance = (double)coordsSearch["instructions"][i]["distance"];
            double time = (double)coordsSearch["instructions"][i]["time"];
            totalDistance += (float)distance;
            totalTime += (float)time;
        }

        myGameManager.PathPanel.SetActive(true);
        myGameManager.DestinationInfo.GetComponent<Text>().text = myGameManager.navApi.NavItemGetValue(myGameManager.navApi.QMapperGetValue(myGameManager.navApi.target)).Description;
        myGameManager.Distance.GetComponent<Text>().text = MsToMinutes(totalTime).ToString() + " min " + "(" + (Mathf.Round(totalDistance)).ToString() + " m)";
    }

    public float MsToMinutes(float ms) {
        float minutes = ms / 60000;
        if (minutes < 1)
        {
            return 1;
        }
        return Mathf.Round(minutes);
    }
}
