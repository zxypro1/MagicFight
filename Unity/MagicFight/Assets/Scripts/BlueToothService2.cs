using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArduinoBluetoothAPI;
using System;
using System.Text;

public class BlueToothService2 : MonoBehaviour
{
    private BluetoothHelper bluetoothHelper2;
    private float timer;
    private Animator anim;
    private GameObject player;

    private void exps() {
        anim.SetTrigger("attack");
        Debug.Log("EXPS!");
    }

    private void avad() {
        anim.SetTrigger("attack");
        Debug.Log("AVA!");
    }

    private void pro() {
        anim.SetTrigger("attack");
        Debug.Log("PRO!");
    }
    void Awake()
    {
        player = GameObject.Find("Wizard2");
        anim = player.GetComponent<Animator>();
    }

    void ResetAnimation()
        {
            anim.SetBool("isLookUp", false);
            anim.SetBool("isRun", false);
            anim.SetBool("isJump", false);
        }

    void Start()
    {
        timer = 0;
        try{
            Debug.Log("HI 2");
            anim.SetTrigger("die");
            BluetoothHelper.BLE = true;  //use Bluetooth Low Energy Technology
            bluetoothHelper2 = BluetoothHelper.GetInstance();
            // bluetoothHelper2.setFixedLengthBasedStream(1);
            bluetoothHelper2.OnConnected += (helper) => {
                // helper.StartListening();
                anim.SetTrigger("idle");
                List<BluetoothHelperService> services = helper.getGattServices();
                foreach (BluetoothHelperService s in services)
                {
                    Debug.Log("Service : " + s.getName());
                    foreach (BluetoothHelperCharacteristic item in s.getCharacteristics())
                    {
                        Debug.Log(item.getName());
                    }
                }

                Debug.Log("Connected");
                BluetoothHelperCharacteristic c = new BluetoothHelperCharacteristic("5B23");
                c.setService("765B");
                bluetoothHelper2.Subscribe(c);
                // bluetoothHelper2.setTxCharacteristic(c);
                // bluetoothHelper2.setRxCharacteristic(c);
                // bluetoothHelper2.setFixedLengthBasedStream(1);
                // bluetoothHelper2.StartListening();
                //sendData();
            };
            bluetoothHelper2.OnConnectionFailed += (helper)=>{
                Debug.Log("Connection failed");
                anim.SetTrigger("die");
            };
            // bluetoothHelper2.OnDataReceived += (helper) => {
            //     // Debug.Log("0");
            //     byte[] a = bluetoothHelper2.ReadBytes();
            //     // Debug.Log(a[0]);
            //     switch (a[0]) {
            //         case 7: 
            //             pro();
            //             break;
            //         case 8: 
            //             avad();
            //             break;
            //         case 9: 
            //             exps();
            //             break;
            //         default: break;
            //     }
            // };
            bluetoothHelper2.OnScanEnded += OnScanEnded;
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
                // Debug.Log(characteristic.getName());
                byte a = value[0];
                // for (int i = 0; i < value.Length; i++) {
                //     a += value[i];
                // }
                // Debug.Log(a);
                switch (a) {
                    case 7: 
                        pro();
                        break;
                    case 8: 
                        avad();
                        break;
                    case 9: 
                        exps();
                        break;
                    default: break;
                }
                // bluetoothHelper2.WriteCharacteristic(characteristic, "0");
                // anim.SetTrigger("attack");
            };

            // BluetoothHelperService service = new BluetoothHelperService("FFE0");
            // service.addCharacteristic(new BluetoothHelperCharacteristic("FFE1"));
            // BluetoothHelperService service2 = new BluetoothHelperService("180A");
            // service.addCharacteristic(new BluetoothHelperCharacteristic("2A24"));
            // bluetoothHelper2.Subscribe(service);
            // bluetoothHelper2.Subscribe(service2);
            // bluetoothHelper2.ScanNearbyDevices();

            // BluetoothHelperService service = new BluetoothHelperService("19B10000-E8F2-537E-4F6C-D104768A1214");
            // service.addCharacteristic(new BluetoothHelperCharacteristic("19B10001-E8F2-537E-4F6C-D104768A1214"));
            // BluetoothHelperService service2 = new BluetoothHelperService("180A");
            // service.addCharacteristic(new BluetoothHelperCharacteristic("2A24"));
            // bluetoothHelper2.Subscribe(service);
            // bluetoothHelper2.Subscribe(service2);
            bluetoothHelper2.ScanNearbyDevices();

        }catch(Exception ex){
            Debug.Log(ex.StackTrace);
        }
    }

    private void OnScanEnded(BluetoothHelper helper, LinkedList<BluetoothDevice> devices){
        Debug.Log("Found " + devices.Count);
        if(devices.Count == 0){
            bluetoothHelper2.ScanNearbyDevices();
            return;
        }

        foreach(var d in devices)
        {
            Debug.Log(d.DeviceName);
        }
        try
        {
            bluetoothHelper2.setDeviceName("Nano 33 BLE Sense2");
            bluetoothHelper2.Connect();
            Debug.Log("Connecting");
        }catch(Exception ex)
        {
            bluetoothHelper2.ScanNearbyDevices();
            Debug.Log(ex.Message);
        }
    }

    void OnDestroy()
    {
        if (bluetoothHelper2 != null)
            bluetoothHelper2.Disconnect();
    }

    void Update(){
        if(bluetoothHelper2 == null)
            return;
        if(!bluetoothHelper2.isConnected())
            return;
        timer += Time.deltaTime;

        if(timer < 1)
            return;
        timer = 0;
        // sendData();
        // read();
    }

    void sendData(){
        // Debug.Log("Sending");
        // BluetoothHelperCharacteristic ch = new BluetoothHelperCharacteristic("FFE1");
        // ch.setService("FFE0"); //this line is mandatory!!!
        // bluetoothHelper2.WriteCharacteristic(ch, new byte[]{0x44, 0x55, 0xff});

        Debug.Log("Sending");
        BluetoothHelperCharacteristic ch = new BluetoothHelperCharacteristic("2A57");
        ch.setService("180A"); //this line is mandatory!!!
        bluetoothHelper2.WriteCharacteristic(ch, "2");
    }

    void read(){
        BluetoothHelperCharacteristic ch = new BluetoothHelperCharacteristic("2A57");
        ch.setService("180A");//this line is mandatory!!!
        bluetoothHelper2.ReadCharacteristic(ch);
    }

    /// <summary>
/// 字符串转16进制
/// </summary>
/// <param name="_str">字符串</param>
/// <param name="encode">编码格式</param>
/// <returns></returns>
    private static string StrToHex(string Text)
    {
        byte[] buffer = Encoding.Default.GetBytes(Text);
        string result = string.Empty;
        foreach (char c in buffer)
        {
        result += Convert.ToString(c, 16);
        }
        return result.ToUpper();
    }
//本文来自：谢文Cor，原地址：http://www.c-or.cn/?id=209

}
