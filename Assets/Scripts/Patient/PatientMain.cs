using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PatientMain : MonoBehaviour
{
    //****** Network objects ******//
    private int port = 5000;
    private int hostId;
    private int videoChannel; //video feed back from HoloLens
    private int statChannel;    //recieve satistics about HoloLens
    private int instChannel; //send instructions to HoloLens

    private int connectionId;
    private string IPAddress;

    private bool pendingConnection = false;
    private bool establishedConnection = false;
    private float connectionTime;

    private byte error;


    //****** Gameplay objects ******//
    private GameObject UICanvas;
    private GameObject startSessionPanel;
    private GameObject testPanel;
    public InputField HostIP;
    public Text Status;
    // to be removed
    public Text TestStatus;

    //****** Script objects ******//
    public static PatientMain instance;

    private void Start()
    {
        UICanvas = GameObject.Find("UICanvas");
        startSessionPanel = UICanvas.transform.Find("StartPanel").gameObject;
        startSessionPanel.SetActive(true);
        testPanel = UICanvas.transform.Find("TestPanel").gameObject;
        testPanel.SetActive(false);
        instance = this;
        InitConnection();
    }

        // creates the socket
    public void InitConnection()
    {
        if (!establishedConnection)
        {
            //RecievedMsg.text = "Trying to connect to " + IPAddress;
            NetworkTransport.Init();
            ConnectionConfig cc = new ConnectionConfig();

            // TCP connection
            instChannel = cc.AddChannel(QosType.Reliable);

            // UDP connection for video Feed and stats
            statChannel = cc.AddChannel(QosType.Unreliable);
            videoChannel = cc.AddChannel(QosType.Unreliable);

            // only 1 connection max
            HostTopology htopo = new HostTopology(cc, 1);
            // Accept host on port with the HostTopology on any IP address (null)
            hostId = NetworkTransport.AddHost(htopo, 0);

            Debug.Log("Ready to connect to computer");
        }
    }

    // called by connect button
    private void Connect()
    {
        if (pendingConnection)
        {
            Status.text = "Device has already a pending connection"; // should never occur
            return;
        }
        if (HostIP.text == "")
        {
            Status.text = "Please enter the host IPv4";
            return;
        }
        IPAddress = HostIP.text;
        Status.text = "Trying to connect to " + IPAddress;
        connectionId = NetworkTransport.Connect(hostId, IPAddress, port, 0, out error); //IP address to be changed
        Debug.Log("connectionId : " + connectionId + " // error : " + (NetworkError)error);
        if (connectionId != 0)
        {
            Debug.Log("Connection sequence successfull, connection id : " + connectionId);
            pendingConnection = true;
        }
        else
        {
            Debug.Log("Facing an issue with connection to " + IPAddress);
            pendingConnection = false;
        }
    }

    private void Update()
    {
        if (!pendingConnection && !establishedConnection)
        {
            return;
        }

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.Nothing:         //1
                break;
            case NetworkEventType.ConnectEvent:    //2 normally unused
                break;
            case NetworkEventType.DataEvent:       //3
                Debug.Log("StatChannel ?" + (channelId == statChannel).ToString());
                Debug.Log("VideoChannel ?" + (channelId == videoChannel).ToString());
                Debug.Log("InstChannel ?" + (channelId == instChannel).ToString());
                // parse data according to channel id
                if (channelId == statChannel)
                {
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    // first message recieved to confirm connection
                    if (msg == "CONNECT")
                    {
                        Status.text = msg;
                        establishedConnection = true;
                        StartCoroutine(FadeTo(1.0f, 1.0f, testPanel));
                        StartCoroutine(FadeTo(0.0f, 1.0f, startSessionPanel));
                        break;
                    }

                    if (startSessionPanel.activeSelf)
                    {
                        Status.text = msg;
                        Debug.Log("Message : " + msg);
                    }
                    else
                    {
                        TestStatus.text = msg;
                        Debug.Log("Message : " + msg);
                        PatientManager.instance.ParseInstruction(msg); // to be modified later
                    }
                    break;
                }
                if (channelId == videoChannel)
                {
                    break;
                }
                if (channelId == instChannel)
                {
                    break;
                }
                break;
            case NetworkEventType.DisconnectEvent: //4 normally not used
                StartCoroutine(FadeTo(0.0f, 1.0f, testPanel));
                StartCoroutine(FadeTo(1.0f, 1.0f, startSessionPanel));
                PatientGazePointer.instance.enabled = false;
                establishedConnection = false;
                pendingConnection = false;
                Status.text = "Disconnected";
                break;
        }
    }



    public void Dial()
    {
        string button = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text;
        Debug.Log("Dial " + button);
        switch (button)
        {
            case "Back":
                if (HostIP.text.Length >= 1)
                {
                    HostIP.text = HostIP.text.Remove(HostIP.text.Length - 1);
                }
                break;
            default:
                HostIP.text += button;
                break;
        }
    }

    IEnumerator FadeTo(float aValue, float aTime, GameObject obj)
    {
        Debug.Log("Fading to " + aValue);
        CanvasGroup objCanvasGroup;
        objCanvasGroup = obj.GetComponent<CanvasGroup>();
        float prevAlpha = objCanvasGroup.alpha;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            objCanvasGroup.alpha = (1.0f - t) * prevAlpha + t * aValue;
            yield return null;
        }
        if (aValue == 0)
        {
            obj.SetActive(false);
            Debug.Log("Unactive canvas");
        }
        else
        {
            obj.SetActive(true);
            Debug.Log("Active canvas");
        }
    }
}
