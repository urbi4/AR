using UnityEngine;
using System.Collections.Generic;
using System;

public class ScannerBLE : MonoBehaviour
{

	// IBKS C1 D0:57:74:EB:A3:BA Q407
	// IBKS C2 CD:86:3F:1C:81:18 Q407
	// IBKS C3 CC:0D:8E:63:E2:D3 Q13719
	// IBKS C5 D4:D9:D6:6D:6C:41 Q13719
	public MyGameManager myGameManager;

	private float timeout;
	private float startScanTimeout = 10f;
	private float startScanDelay = 0.5f;
	private bool startScan = true;
	public List<Beacon> scannedItems;
	public string loadedItem;
	private List<Beacon> loadedItems;

	private Beacon lastBeacon = null;
	private BluetoothDeviceScript bluetoothScript = null;
    public void OnStopScanning()
	{
		Debug.Log ("BLE stopping");
		BluetoothLEHardwareInterface.StopScan ();
	}

	// Use this for initialization
	void Start ()
	{
		scannedItems = new List<Beacon>();
		loadedItems = new List<Beacon>();
		loadedItem = null;

		//Beacon beacon1 = new Beacon("Q407", "Q407address", -70);
		//Beacon beacon5 = new Beacon("Q13719", "Q13719address", -70);
		//loadedItems.Add(beacon1);
		//loadedItems.Add(beacon5);


		//name, adress, rssi

		
	}

	public void InitBLE()
	{
        bluetoothScript = BluetoothLEHardwareInterface.Initialize(true, false, () => {

            timeout = startScanDelay;
        },
        (error) => {

            Debug.Log("Error: " + error);

            if (error.Contains("Bluetooth LE Not Enabled"))
                BluetoothLEHardwareInterface.BluetoothEnable(true);
        });
    }
	
	// Update is called once per frame
	void Update ()
	{
		if (timeout > 0f && bluetoothScript != null)
		{
			timeout -= Time.deltaTime;
			if (timeout <= 0f)
			{
				if (startScan)
				{
					startScan = false;
					timeout = startScanTimeout;

					BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (null, null, (address, name, rssi, bytes) => {
						
						var foundedItem = scannedItems.Find((obj) => obj.address == address);
						if (foundedItem != null)
						{
							var scannedItem = foundedItem;
							scannedItem.rssi = rssi;
							scannedItem.distance = ToDistanceRSSI(rssi);
                        }

                        if (!name.Equals("No Name") && name.StartsWith("Q") && loadedItem == null) //!name.Equals("No Name") &&  name.StartsWith("iTAG") 
                        {
                            double rssiDistance = ToDistanceRSSI(rssi);
							if (foundedItem == null) {
                                var beacon = new Beacon(name, address, rssi, rssiDistance);
                                scannedItems.Add(beacon);
                            }
                          
                            rssiDistance = ToDistanceRSSI(rssi);
                            if (rssiDistance <= (double)10)
                            {
                                BluetoothLEHardwareInterface.Log("ScannerBLE Already used " + name);
                                loadedItem = name;
                                myGameManager.receivedQ(name);
                            }
                            else
                            {
                                BluetoothLEHardwareInterface.Log("ScannerBLE Q is not in distance" + name);
                                loadedItem = null;
                            }
                        }

                        //Priprava na prechod pres beacny pri ceste
                        //var loadedItem = loadedItems.Find((obj)=> obj.name == name);
                        //if (loadedItem != null)
                        //{
                        //	if (lastBeacon != foundedItem && foundedItem.rssi >= loadedItem.rssi)
                        //	{
                        //                          BluetoothLEHardwareInterface.Log("receivedQ");
                        //                          lastBeacon = foundedItem;
                        //		myGameManager.receivedQ(foundedItem.name);
                        //	}
                        //}
                    }, true);
				}
				else
				{
					BluetoothLEHardwareInterface.StopScan ();
					startScan = true;
					timeout = startScanDelay;
				}
			}
		}
	}
    double ToDistanceRSSI(double rssi)
    {
		double rssiDistance = Math.Pow(((double)10),((-69 - rssi) / (10 * 2)));
        return rssiDistance *100;
    }
}
