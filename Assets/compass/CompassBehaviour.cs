using System;
using System.Collections;
using UnityEngine;

public class CompassBehaviour : MonoBehaviour
{
    private bool startTracking = false;
    void Start()
    {
        Input.compass.enabled = true;
        Input.location.Start();
        StartCoroutine(InitializeCompass());
    }

    public void SetToTrueNorth() {
        transform.rotation = Quaternion.Euler(0, -Input.compass.trueHeading, 0);
    }

    IEnumerator InitializeCompass()
    {
        yield return new WaitForSeconds(1f);
        startTracking |= Input.compass.enabled;
        if (startTracking)
        {
            SetToTrueNorth();
        }
    }

    private static string DegreesToCardinalDetailed(double degrees)
    {
        string[] caridnals = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW", "N" };
        return caridnals[(int)Math.Round(((double)degrees * 10 % 3600) / 225)];
    }
}
