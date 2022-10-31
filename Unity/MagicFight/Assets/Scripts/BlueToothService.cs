using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArduinoBluetoothAPI;
using System;
using System.Text;

public class BlueToothService : MonoBehaviour
{
    private BluetoothHelper bluetoothHelper;
    private Animator anim;
    private GameObject player;
    private GameObject fireball;
    private GameObject fireball2;
    public bool isDead = false;
    public string deviceName;
    public string characteristicName;
    public string serviceName;

    private Vector3 getInitPosition()
    {
        if (name == "Wizard")
        {
            return new Vector3(2.5f, 2.5f, 0);
        }
        else
        {
            return new Vector3(-2.5f, 2.5f, 0);
        }
    }

    private void exps() {
        anim.SetTrigger("attack");
        fireball.SetActive(true);
        fireball.transform.position = transform.position;
        fireball.transform.position += getInitPosition();
        Debug.Log("EXPS!");
    }

    private void avad() {
        anim.SetTrigger("attack");
        fireball2.SetActive(true);
        fireball2.transform.position = transform.position;
        fireball2.transform.position += getInitPosition();
        Debug.Log("AVA!");
    }

    private void pro() {
        anim.SetTrigger("attack");
        Debug.Log("PRO!");
    }
    void Awake()
    {
        //fireball = GameObject.Find("fireball-e");
        //fireball.transform.position = transform.position;
        //fireball.transform.position += new Vector3(2.5f, 2.5f, 0);
        anim = GetComponent<Animator>();
    }

    void ResetAnimation()
        {
            anim.SetBool("isLookUp", false);
            anim.SetBool("isRun", false);
            anim.SetBool("isJump", false);
        }

    void Start()
    {
        if (name == "Wizard")
        {
            fireball = GameObject.Find("fireball-e");
            fireball2 = GameObject.Find("fireball-a");
        }
        else
        {
            fireball = GameObject.Find("fireball-e-2");
            fireball2 = GameObject.Find("fireball-a-2");
        }

        try
        {
            Debug.Log("HI");
            anim.SetTrigger("die");
            BluetoothHelper.BLE = true;  //use Bluetooth Low Energy Technology
            bluetoothHelper = BluetoothHelper.GetInstance();
            // bluetoothHelper.setFixedLengthBasedStream(1);
            bluetoothHelper.OnConnected += (helper) => {
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
                //BluetoothHelperCharacteristic c = new BluetoothHelperCharacteristic("2A57");
                //c.setService("180A");
                BluetoothHelperCharacteristic c = new BluetoothHelperCharacteristic(characteristicName);
                c.setService(serviceName);
                bluetoothHelper.Subscribe(c);
                // bluetoothHelper.setTxCharacteristic(c);
                // bluetoothHelper.setRxCharacteristic(c);
                // bluetoothHelper.setFixedLengthBasedStream(1);
                // bluetoothHelper.StartListening();
                //sendData();
            };
            bluetoothHelper.OnConnectionFailed += (helper)=>{
                Debug.Log("Connection failed");
                anim.SetTrigger("die");
            };
            bluetoothHelper.OnScanEnded += OnScanEnded;
            bluetoothHelper.OnServiceNotFound += (helper, serviceName) =>
            {
                Debug.Log(serviceName);
            };
            bluetoothHelper.OnCharacteristicNotFound += (helper, serviceName, characteristicName) =>
            {
                Debug.Log(characteristicName);
            };
            bluetoothHelper.OnCharacteristicChanged += (helper, value, characteristic) =>
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
                // bluetoothHelper.WriteCharacteristic(characteristic, "0");
                // anim.SetTrigger("attack");
            };

            // BluetoothHelperService service = new BluetoothHelperService("FFE0");
            // service.addCharacteristic(new BluetoothHelperCharacteristic("FFE1"));
            // BluetoothHelperService service2 = new BluetoothHelperService("180A");
            // service.addCharacteristic(new BluetoothHelperCharacteristic("2A24"));
            // bluetoothHelper.Subscribe(service);
            // bluetoothHelper.Subscribe(service2);
            // bluetoothHelper.ScanNearbyDevices();

            // BluetoothHelperService service = new BluetoothHelperService("19B10000-E8F2-537E-4F6C-D104768A1214");
            // service.addCharacteristic(new BluetoothHelperCharacteristic("19B10001-E8F2-537E-4F6C-D104768A1214"));
            // BluetoothHelperService service2 = new BluetoothHelperService("180A");
            // service.addCharacteristic(new BluetoothHelperCharacteristic("2A24"));
            // bluetoothHelper.Subscribe(service);
            // bluetoothHelper.Subscribe(service2);
            bluetoothHelper.ScanNearbyDevices();

        }catch(Exception ex){
            Debug.Log(ex.StackTrace);
        }
    }

    private void OnScanEnded(BluetoothHelper helper, LinkedList<BluetoothDevice> devices){
        Debug.Log("Found " + devices.Count);
        if(devices.Count == 0){
            bluetoothHelper.ScanNearbyDevices();
            return;
        }

        foreach(var d in devices)
        {
            Debug.Log(d.DeviceName);
        }
        try
        {
            bluetoothHelper.setDeviceName(deviceName);
            bluetoothHelper.Connect();
            Debug.Log("Connecting");
        }catch(Exception ex)
        {
            bluetoothHelper.ScanNearbyDevices();
            Debug.Log(ex.Message);
        }
    }

    void OnDestroy()
    {
        if (bluetoothHelper != null)
            bluetoothHelper.Disconnect();
    }

    void Update(){
        if(bluetoothHelper == null)
            return;
        if(!bluetoothHelper.isConnected())
            return;
        //timer += Time.deltaTime;

        //if(timer < 1)
        //    return;
        //timer = 0;
        // sendData();
        // read();
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

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.name == "fireball-e" || collision.name == "fireball-a")
        {
            Debug.Log("is trigger!");
            Debug.Log(collision.name);
            isDead = true;
            anim.SetTrigger("die");
        }
    }
}
