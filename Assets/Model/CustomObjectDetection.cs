using System;
using Newtonsoft.Json;

[System.Serializable]
public class CustomObjectDetection
{
    [JsonProperty("objectDetection")]
    public string ObjectDetection { get; set; }

}

