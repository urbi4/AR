using System;


[System.Serializable]
public class Beacon
{
	public String name;
	public String address;
	public int rssi;
    public double distance;

    public Beacon(string name, string address, int rssi, double distance)
    {
        this.name = name;
        this.address = address;
        this.rssi = rssi;
        this.distance = distance;
    }

    public Beacon() { }

}
