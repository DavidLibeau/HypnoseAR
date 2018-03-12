using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TherapeuteMain : MonoBehaviour
{

    private TherapeuteSession newSession;

    //  ***** Communication layer defs ***** //
    private int port = 5000;
    private int hostId;
    private int videoChannel; //video feed back from HoloLens
    private int statChannel;	//recieve satistics about HoloLens
    private int instChannel; //send instructions to HoloLens
    private bool isStarted = false;
    private byte error;

    // connectionId of the client
    int connectionId;

    // IP data
    private IPAddress[] iplist;
    private string localIP;

    // Handlers //
    public Text status;
    private GameObject UICanvas;
    private GameObject startSessionPanel;
    private GameObject sessionPanel;
    private GameObject primaryPanel;
    public static TherapeuteMain instance;

    private void Start()
    {
        
        // ***** to be changed ***** //
        // get handler on canvas
        UICanvas = GameObject.Find("UICanvas");
        startSessionPanel = UICanvas.transform.Find("StartingSessionPanel").gameObject;
        //startSessionPanel.SetActive(false);
        startSessionPanel.SetActive(true);
        sessionPanel = UICanvas.transform.Find("SessionPanel").gameObject;
        sessionPanel.SetActive(false);
        primaryPanel = UICanvas.transform.Find("PrimaryPanel").gameObject;
        //primaryPanel.SetActive(true);
        primaryPanel.SetActive(false);

        instance = this;
        InitiateConnection();
    }

    void InitiateConnection()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();

        // TCP connection
        instChannel = cc.AddChannel(QosType.Reliable);
        // UDP connection for video Feed and stats
        statChannel = cc.AddChannel(QosType.Unreliable);
        videoChannel = cc.AddChannel(QosType.Unreliable);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        iplist = Dns.GetHostAddresses(Dns.GetHostName());
        if(iplist.Length < 2)
        {
            status.text = "You might not be connected to internet";
            localIP = iplist[iplist.Length - 1].ToString();
        }
        else
        {
            localIP = iplist[1].ToString();
            status.text = "Host IPv4 : " + localIP + "\n waiting for connection";
        }
#endif

        // only 1 connection max
        HostTopology htopo = new HostTopology(cc, 1);

        // Accept host on port with the HostTopology on any IP address (null)
        hostId = NetworkTransport.AddHost(htopo, port, null);
        Debug.Log("Open socket, waiting for clients, socket id is : "+ hostId);
        isStarted = true;
    }

    private void Update()
    {
        if (!isStarted) {
            return;
        }
        int recHostId;
        //int connectionId;
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
            case NetworkEventType.ConnectEvent:    //2
                Debug.Log("Device " + connectionId + " is connected");
                status.text = "Client is connected";
                SendMsg("CONNECT"); //first message to confirm connection
                SendMsg("Client is connected to " + localIP);
                StartCoroutine(FadeTo(0.0f, 0.5f, startSessionPanel));
                StartCoroutine(FadeTo(1.0f, 0.5f, sessionPanel));
                break;
            case NetworkEventType.DataEvent:
                Debug.Log("StatChannel ?" + (channelId == statChannel).ToString());
                Debug.Log("VideoChannel ?" + (channelId == videoChannel).ToString());
                Debug.Log("InstChannel ?" + (channelId == instChannel).ToString());
                if(channelId == instChannel)
                {
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    status.text = msg;
                    Debug.Log("Device " + connectionId + " has sent " + msg);//3
                    break;
                }
                if (channelId == statChannel)
                {

                }
                if(channelId == videoChannel)
                {

                }
                    break;
            case NetworkEventType.DisconnectEvent: //4
                Debug.Log("Device " + connectionId + " has disconnected");
                status.text = "Client has disconnected \n Host IPv4 : " + localIP;
                StartCoroutine(FadeTo(0.0f, 0.5f, sessionPanel));
                StartCoroutine(FadeTo(1.0f, 0.5f, startSessionPanel));
                break;
        }
    }

    public void SendMsg(string message)
    {
        Debug.Log("Sending to " + connectionId + " : " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostId, connectionId, statChannel, msg, message.Length * sizeof(char), out error);
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

    public void BackToPrimary()
    {
        Debug.Log("Back to primary panel");
        FadeTo(0.0f, 1.0f, startSessionPanel);
        FadeTo(1.0f, 1.0f, primaryPanel);
    }

}