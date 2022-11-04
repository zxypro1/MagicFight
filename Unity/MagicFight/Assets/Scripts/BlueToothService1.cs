using System.Collections.Generic;
using UnityEngine;
using ArduinoBluetoothAPI;
using System;
using UnityEngine.UIElements;

public class BlueToothService1 : MonoBehaviour
{
    private BluetoothHelper bluetoothHelper;
    private BluetoothHelper bluetoothHelper2;
    //private GameObject player1;
    private GameObject player2;
    //private string deviceName = "Nano 33 BLE Sense";
    //private string characteristicName = "65C1";
    //private string serviceName = "708A";
    private string deviceName2 = "Nano 33 BLE Sense2";
    private string characteristicName2 = "5B23";
    private string serviceName2 = "765B";

    void Awake()
    {
        //player1 = GameObject.Find("Wizard");
        player2 = GameObject.Find("Wizard2");
    }

    void Start()
    {
        //player1 = GameObject.Find("Wizard");
        player2 = GameObject.Find("Wizard2");

        //player1.GetComponent<Player1>().die();
        player2.GetComponent<Player2>().die();
        try
        {
            Debug.Log("HI");
            BluetoothHelper.BLE = true;  //use Bluetooth Low Energy Technology
            //bluetoothHelper = BluetoothHelper.GetInstance(deviceName);
            bluetoothHelper2 = BluetoothHelper.GetNewInstance(deviceName2);

            // player1 connect
            //bluetoothHelper.OnConnected += (helper) => {
            //    List<BluetoothHelperService> services = helper.getGattServices();
            //    player1.GetComponent<Player1>().idle();
            //    foreach (BluetoothHelperService s in services)
            //    {
            //        Debug.Log("Service : " + s.getName());
            //        foreach (BluetoothHelperCharacteristic item in s.getCharacteristics())
            //        {
            //            Debug.Log(item.getName());
            //        }
            //    }

            //    Debug.Log("Connected");
            //    BluetoothHelperCharacteristic c = new BluetoothHelperCharacteristic(characteristicName);
            //    c.setService(serviceName);
            //    bluetoothHelper.Subscribe(c);
            //};
            //bluetoothHelper.OnConnectionFailed += (helper) => {
            //    Debug.Log("Connection failed");
            //    player1.GetComponent<Player1>().die();
            //};
            //bluetoothHelper.OnScanEnded += OnScanEnded;
            //bluetoothHelper.OnServiceNotFound += (helper, serviceName) =>
            //{
            //    Debug.Log(serviceName);
            //};
            //bluetoothHelper.OnCharacteristicNotFound += (helper, serviceName, characteristicName) =>
            //{
            //    Debug.Log(characteristicName);
            //};
            //bluetoothHelper.OnCharacteristicChanged += (helper, value, characteristic) =>
            //{
            //    byte a = value[0];
            //    Debug.Log(a);
            //    switch (a)
            //    {
            //        case 7:
            //            player1.GetComponent<Player1>().pro();
            //            break;
            //        case 8:
            //            player1.GetComponent<Player1>().avad();
            //            break;
            //        case 9:
            //            player1.GetComponent<Player1>().exps();
            //            break;
            //        case 10:
            //            player1.GetComponent<Player1>().block();
            //            break;
            //        default: break;
            //    }
            //};

            // player2 connect
            bluetoothHelper2.OnConnected += (helper) => {
                List<BluetoothHelperService> services2 = helper.getGattServices();
                player2.GetComponent<Player2>().idle();
                foreach (BluetoothHelperService s in services2)
                {
                    Debug.Log("Service : " + s.getName());
                    foreach (BluetoothHelperCharacteristic item in s.getCharacteristics())
                    {
                        Debug.Log(item.getName());
                    }
                }

                Debug.Log("Connected");
                BluetoothHelperCharacteristic d = new BluetoothHelperCharacteristic(characteristicName2);
                d.setService(serviceName2);
                bluetoothHelper2.Subscribe(d);
            };
            bluetoothHelper2.OnConnectionFailed += (helper) => {
                Debug.Log("Connection failed");
                player2.GetComponent<Player2>().die();
            };
            bluetoothHelper2.OnScanEnded += OnScanEnded2;
            bluetoothHelper2.OnServiceNotFound += (helper, serviceName) =>
            {
                Debug.Log(serviceName);
            };
            bluetoothHelper2.OnCharacteristicNotFound += (helper, serviceName, characteristicName) =>
            {
                Debug.Log(characteristicName);
            };
            bluetoothHelper2.OnCharacteristicChanged += (helper, value, characteristic) =>
            {
                byte a = value[0];
                Debug.Log(a);
                switch (a)
                {
                    case 7:
                        player2.GetComponent<Player2>().pro();
                        break;
                    case 8:
                        player2.GetComponent<Player2>().avad();
                        break;
                    case 9:
                        player2.GetComponent<Player2>().exps();
                        break;
                    case 10:
                        player2.GetComponent<Player2>().block();
                        break;
                    default: break;
                }
            };
            //bluetoothHelper.ScanNearbyDevices();
            bluetoothHelper2.ScanNearbyDevices();

        }
        catch (Exception ex)
        {
            Debug.Log(ex.StackTrace);
        }
    }

    private void OnScanEnded(BluetoothHelper helper, LinkedList<BluetoothDevice> devices)
    {
        Debug.Log("Found " + devices.Count);
        foreach (var d in devices)
        {
            Debug.Log(d.DeviceName);
        }
        try
        {
            //bluetoothHelper.setDeviceName(deviceName);
            bluetoothHelper.Connect();
            Debug.Log("Connecting");
        }
        catch (Exception ex)
        {
            //bluetoothHelper.ScanNearbyDevices();
            Debug.Log(ex.Message);
        }
    }

    private void OnScanEnded2(BluetoothHelper helper, LinkedList<BluetoothDevice> devices)
    {
        Debug.Log("Found " + devices.Count);

        foreach (var d in devices)
        {
            Debug.Log(d.DeviceName);
        }
        try
        {
            //bluetoothHelper2.setDeviceName(deviceName2);
            bluetoothHelper2.Connect();
            Debug.Log("Connecting");
        }
        catch (Exception ex)
        {
            //bluetoothHelper2.ScanNearbyDevices();
            Debug.Log(ex.Message);
        }
    }

    void OnDestroy()
    {
        if (bluetoothHelper != null)
            bluetoothHelper.Disconnect();
        if (bluetoothHelper2 != null)
            bluetoothHelper2.Disconnect();
    }

    void Update()
    {
        //if (bluetoothHelper == null)
        //    return;
        //if (!bluetoothHelper.isConnected())
        //    bluetoothHelper.ScanNearbyDevices();
        if (bluetoothHelper2 == null)
            return;
        if (!bluetoothHelper2.isConnected())
            bluetoothHelper2.ScanNearbyDevices();
    }

    //void sendData(){
    //    // Debug.Log("Sending");
    //    // BluetoothHelperCharacteristic ch = new BluetoothHelperCharacteristic("FFE1");
    //    // ch.setService("FFE0"); //this line is mandatory!!!
    //    // bluetoothHelper.WriteCharacteristic(ch, new byte[]{0x44, 0x55, 0xff});

    //    Debug.Log("Sending");
    //    BluetoothHelperCharacteristic ch = new BluetoothHelperCharacteristic("2A57");
    //    ch.setService("180A"); //this line is mandatory!!!
    //    bluetoothHelper.WriteCharacteristic(ch, "2");
    //}

    //void read(){
    //    BluetoothHelperCharacteristic ch = new BluetoothHelperCharacteristic("2A57");
    //    ch.setService("180A");//this line is mandatory!!!
    //    bluetoothHelper.ReadCharacteristic(ch);
    //}
}
