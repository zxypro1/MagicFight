using System.Collections.Generic;
using UnityEngine;
using ArduinoBluetoothAPI;
using System;

public class BlueToothService : MonoBehaviour
{
    private BluetoothHelper bluetoothHelper;
    private Animator anim;
    private GameObject player;
    private GameObject fireball;
    private GameObject fireball2;
    private GameObject fireball_other;
    private GameObject shield;
    public bool isDead = false;
    public string deviceName;
    public string characteristicName;
    public string serviceName;
    public int waitTime = 1; // shield show time (second)

    /// <summary>
    /// Set init position of fireballs.
    /// </summary>
    /// <returns>Vector3</returns>
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

    private void block()
    {
        if (fireball_other.activeSelf)
        {
            fireball_other.GetComponent<fireballs>().isblocked = true;
        }
    }

    /// <summary>
    /// Spell Expelliamas
    /// </summary>
    private void exps() {
        anim.SetTrigger("attack");
        fireball.GetComponent<fireballs>().onInit();
        fireball.SetActive(true);
        fireball.transform.position = transform.position;
        fireball.transform.position += getInitPosition();
        Debug.Log("EXPS!");
    }

    /// <summary>
    /// Spell Avada Kedavra
    /// </summary>
    private void avad() {
        anim.SetTrigger("attack");
        fireball2.SetActive(true);
        fireball2.transform.position = transform.position;
        fireball2.transform.position += getInitPosition();
        Debug.Log("AVA!");
    }

    /// <summary>
    /// Spell Protego
    /// </summary>
    private void pro() {
        Debug.Log("PRO!");
        anim.SetTrigger("attack");
        shield.SetActive(true);
        Invoke("closeShield", waitTime);
    }

    private void closeShield()
    {
        Debug.Log("shield close");
        shield.SetActive(false);
    }

    void Awake()
    {
        //fireball = GameObject.Find("fireball-e");
        //fireball.transform.position = transform.position;
        //fireball.transform.position += new Vector3(2.5f, 2.5f, 0);
        shield = transform.GetChild(1).gameObject;
        shield.SetActive(false);
        anim = GetComponent<Animator>();
        if (name == "Wizard")
        {
            fireball = GameObject.Find("fireball-e");
            fireball2 = GameObject.Find("fireball-a");
            fireball_other = GameObject.Find("fireball-e-2");
        }
        else
        {
            fireball = GameObject.Find("fireball-e-2");
            fireball2 = GameObject.Find("fireball-a-2");
            fireball_other = GameObject.Find("fireball-e");
        }
    }

    /// <summary>
    /// Demo usage of fireballs. Only for test.
    /// Disable this after testing
    /// </summary>
    void test()
    {
        anim.SetTrigger("idle");
        fireball.SetActive(true);
        fireball.transform.position = transform.position;
        fireball.transform.position += getInitPosition();
        fireball2.SetActive(true);
        fireball2.transform.position = transform.position;
        fireball2.transform.position += getInitPosition();
        //pro();
        block();
        //fireball.transform.rotation = Quaternion.Euler(0, 30, 30);
    }

    void ResetAnimation()
        {
            anim.SetBool("isLookUp", false);
            anim.SetBool("isRun", false);
            anim.SetBool("isJump", false);
        }

    void Start()
    {
        try
        {
            Debug.Log("HI");
            anim.SetTrigger("die");
            test(); // only for test
            BluetoothHelper.BLE = true;  //use Bluetooth Low Energy Technology
            bluetoothHelper = BluetoothHelper.GetInstance();
            bluetoothHelper.OnConnected += (helper) => {
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
                BluetoothHelperCharacteristic c = new BluetoothHelperCharacteristic(characteristicName);
                c.setService(serviceName);
                bluetoothHelper.Subscribe(c);
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
                byte a = value[0];
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
            };
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
            if (shield.activeSelf == false)
            {
                isDead = true;
                anim.SetTrigger("die");
            }
        }
    }
}
