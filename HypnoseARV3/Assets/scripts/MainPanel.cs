using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Windows;

//#if WINDOWS_UWP
//using Windows.Storage;
//using Windows.System;
//using System.Threading.Tasks;
//using Windows.Security.Credentials.UI;
//using Windows.Data.Xml.Dom;
//using Windows.Storage.Streams;
//using System.IO;
//#endif

public class MainPanel : MonoBehaviour {

    public GameObject mainPanel;
    public Text info;
    public Text sessionIdText;
    public DateTime date = DateTime.Now;
    public String nextPatient = "Marcel Dupont";

    private String getContent; // contains the text output from the GET operations
    private String prevGetContent; // contains the output from the  previous GET operations (to compare with current)
    private String url = "dav.li/HypnoseAR/";

    bool coroutineRunning;
    private float nextGetCall;
    public float refreshingFreq = 5f;
    public bool sessionPending;
    public static MainPanel Instance;

////#if WINDOWS_UWP
//    Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings; //for saving log info
//    Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
////#endif

    //string path = "info.txt"; // pb là
    //string path = "C:/Users/Romain/Documents/Unity/HypnoseARV3/Assets/test.txt";
    string sessionId;

    public string Url
    {
        get
        {
            return url;
        }

        set
        {
            url = value;
        }
    }
    // URL : dav.li/HypnoseAR/test.php
    // id for the test seance: XX1SI
    public string testSeanceId = "XX1SI";
    // Use this for initialization
    void Start () {
        RequestInfo();
        sessionPending = false;
        coroutineRunning = false;
        getContent = "";
        nextGetCall = Time.time;
        Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
        // ask for starting message
        if (!sessionPending)
        {

        }
        // every x seconds make a get and send scene status:
        if (Time.time > nextGetCall)
        {
            nextGetCall += refreshingFreq;
            prevGetContent = getContent;
            string parameters = "id=" + testSeanceId + ""; // replace by by sessionId
            StartCoroutine(GetHTTPRequest("getSeance", parameters, ParseXML)); // replace by get or whatever -> get updates from server

            //Send objects status to server
            if(sessionPending)
            {
                //string activeObjectSerial = SessionManager.Instance.SerializeActiveObjects();
                //Debug.Log(activeObjectSerial);
                //StartCoroutine(PutHTTPRequest("updateSeance", activeObjects)); // send the serialized list of all active objects in XML!!
            }

        }
    }

    public void ParseXML()
    {
        
        if (!prevGetContent.Equals(getContent))
        {
            SessionManager.Instance.ParseXML(getContent);
            //Debug.Log(getContent);
        }
        else
        {
            Debug.Log("same content");
        }
    }

    public void UpdateLib()
    {

    }

    public void UpdateSeanceButton()
    {
        //string parameters = "id='"+ seanceId + "'&data='scene'";
        //StartCoroutine(GetHTTPRequest("updateSeance",parameters, DisplayToConsole));
        string parameters = "id=" + testSeanceId + "";
        StartCoroutine(GetHTTPRequest("getSeance", parameters, ParseXML));
    }

    public void NewSession()
    {
        string parameters = "id=" + testSeanceId + "";
        StartCoroutine(GetHTTPRequest("test", parameters, UpdateSessionId)); // to be changed by init
      //  StartCoroutine(GetHTTPRequest("newSeance", UpdateSessionId));
    }

    public void UpdateSessionId()
    {
        SessionManager.Instance.ParseXML(getContent);
        Debug.Log("sessionId : " + sessionId);
        sessionIdText.text = "Identifiant de session : " + sessionId;
    }

    IEnumerator GetHTTPRequest(string function, string parameters, Action callback)
    {
        string completeUrl;
        if (parameters == "")
        {
            completeUrl = Url + function + ".php";
        }
        else
        {
            completeUrl = Url + function + ".php?" + parameters;
        }
        Debug.Log(completeUrl);
        UnityWebRequest request = UnityWebRequest.Get(completeUrl);
        yield return request.SendWebRequest();

        if(request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            getContent = request.downloadHandler.text;
           // Debug.Log("Message : "+ getContent);
        }
        if (callback != null)
        {
            //Debug.Log("Callback");
            callback();
        }
    }

    IEnumerator GetHTTPRequest(string function, Action callback)
    {
        string completeUrl = Url + function + ".php";
        Debug.Log(completeUrl);
        UnityWebRequest request = UnityWebRequest.Get(completeUrl);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            getContent = request.downloadHandler.text;
            // Debug.Log("Message : "+ getContent);
        }
        if (callback != null)
        {
            //Debug.Log("Callback");
            callback();
        }
    }

    IEnumerator PutHTTPRequest(string function, string message)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        UnityWebRequest request = UnityWebRequest.Put(Url + function + ".php", data);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("Upload complete!");
        }
    }

    public void FadeInPanel(GameObject obj)
    {
        StartCoroutine(FadeTo(1.0f, 1.0f, obj));
    }

    public void FadeOutPanel(GameObject obj)
    {
        StartCoroutine(FadeTo(0.0f, 1.0f, obj));
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

    public void RequestInfo()
    {
        //StartCoroutine(GetHTTPRequest("test", DisplayInfo)); // to be changed to logInfo        
    }

    public void DisplayToConsole()
    {
        Debug.Log("getContent :" + getContent);
    }

    public void DisplayInfo()
    {
        string[] dataArray = getContent.Split('|');
        if (dataArray.Length == 7)
        {
            info.text = "Dernière mise à jour : " + dataArray[0] + "/" + dataArray[1] + "/" + dataArray[2] + " à " + dataArray[3] + ":" + dataArray[4] + ":" + dataArray[5] + "\n Prochain patient : " + dataArray[6];
        }
        else
        {
            Debug.Log("There is something wrong with the log file or the GET content: " + getContent);
        }
    }

    private void UpdateLogInfo()
    {
        string newInfo = TimeToString(date.Day) + "|" + TimeToString(date.Month) + "|" + TimeToString(date.Year) + "|" + TimeToString(date.Hour) + "|" + TimeToString(date.Minute) + "|" + TimeToString(date.Second) + "|" + nextPatient;
        StartCoroutine(PutHTTPRequest("test", newInfo)); // to be changed to logInfo
    }

    private static string TimeToString(int num)
    {
        if(num < 10)
        {
            return "0" + num.ToString();
        }
        return num.ToString();
    }
}
