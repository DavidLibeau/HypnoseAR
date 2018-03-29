using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Windows;
using System.Xml;
using System.IO;
using System.Xml.Linq;

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
    public GameObject sessionPointer;
    public Text info;
    public Text sessionIdText;

    public DateTime lastUpdate = DateTime.Now; // to be changed
    public String nextPatient = "Marcel Dupont"; // to be changed

    private String getContent = ""; // contains the text output from the GET operations
    private String prevGetContent = ""; // contains the output from the previous GET operations (to compare with current)
    private String putContent = ""; // contains the text output representing the status of the scene to be send to the server
    private String prevPutContent = ""; // contains the text output representing the previous status of the scene sent to the server
    private String url = "dav.li/HypnoseAR/";
    private bool putPending = false;

    private float nextGetCall;
    public float refreshingFreq = 5f;
    public bool sessionPending;
    public static MainPanel Instance;

    // test
    public Text modeTest;
    public Text recievedTest;

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
        getContent = "";
        nextGetCall = Time.time;
        Instance = this;

	}
	//***** ENSURE THAT THERE IS NO COLLISION BETWEEN RECIEVED CONTENTS AND UPDATED CONTENTS DUE TO PATIENT*****//
	// Update is called once per frame
	void Update () {
        // ask for starting message
        if (!sessionPending)
        {
            sessionPointer.SetActive(false);
            modeTest.text = "UI mode";
        }
        else
        {
            modeTest.text = "Session running";
            sessionPointer.SetActive(true);
        }
        // every x seconds make a get and send scene status:
        if (Time.time > nextGetCall)
        {
            nextGetCall += refreshingFreq;
            prevGetContent = getContent;
            prevPutContent = putContent;

            string parameters = "id=" + testSeanceId + ""; // replace by by sessionId
            //StartCoroutine(GetHTTPRequest("getSeance", parameters, ParseXMLandReply)); // replace by get or whatever -> get updates from server
          
          // test
          //StartCoroutine(GetHTTPRequest("test", ParseXMLandReply)); // replace by get or whatever -> get updates from server
          getContent = "<?xml version=\"1.0\"?><seance isStarted=\"true\"><id>XX1SI</id><date_create>1521033192</date_create><scene><object><selfId>1</selfId><sceneId>o46</sceneId></object><object><selfId>1</selfId><sceneId>o24</sceneId></object></scene></seance>";
          ParseXMLandReply();
        }
    }

    public void ParseXMLandReply()
    {
        recievedTest.text = getContent;
        // ParseXML, porcess the info on the scene via the SessionManager
        if (!prevGetContent.Equals(getContent))
        {
            SessionManager.Instance.ParseXML(getContent);
            //Debug.Log(getContent);
        }
        else
        {
            Debug.Log("same content");
        }

        // Reply with the new status of the status in case the patient moved something
        //Send objects status to server
        if (sessionPending)
        {
            //XmlDocument feedBack = SessionManager.Instance.MkXmlFeedback();
            XDocument feedBack = SessionManager.Instance.MkObjXmlFeedback(SessionManager.Instance.changedObjects);
            //putContent = feedBack.OuterXml;
            if (feedBack != null)
            {
                putContent = feedBack.ToString(); //to be checked
                //Debug.Log(SessionManager.Instance.changedObject.ToString());
                //Debug.Log("putContent : " + putContent);
                //Debug.Log("prevPutContent : " + prevPutContent);

                if (!prevPutContent.Equals(putContent))
                {
                    Debug.Log("Latest feedback : " + putContent);
                    StartCoroutine(PutHTTPRequest("putTest", putContent));
                }
                else
                {
                    Debug.Log("same feedback content");
                }
            }
            else
            {
                Debug.Log("No feedback content");
            }
        }
    }

    public void UpdateLib()
    {
        // to be completed
        //callback : update info
    }

    public void NewSession()
    {
        StartCoroutine(GetHTTPRequest("test", UpdateSessionId)); // to be removed
      //  StartCoroutine(GetHTTPRequest("newSeance", UpdateSessionId));
    }

    public void UpdateSessionId()
    {
        sessionId = getContent;
        Debug.Log("sessionId : " + sessionId);
        sessionIdText.text = "Identifiant de session : " + sessionId;
    }

    IEnumerator GetHTTPRequest(string function, string parameters, Action Callback)
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

        if(request.responseCode != 200) //request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
            yield break;
        }
        else
        {
            getContent = request.downloadHandler.text;
           //Debug.Log("Message : "+ getContent);
        }
        if (Callback != null)
        {
            //Debug.Log("Callback");
            Callback();
        }
    }

    IEnumerator GetHTTPRequest(string function, Action callback)
    {
        string completeUrl = Url + function + ".php";
        Debug.Log(completeUrl);
        UnityWebRequest request = UnityWebRequest.Get(completeUrl);
        yield return request.SendWebRequest();

        if (request.responseCode != 200) // request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            getContent = request.downloadHandler.text;
            Debug.Log("Message : " + getContent);
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
        Debug.Log(Url + function + ".php");
        UnityWebRequest request = UnityWebRequest.Put(Url + function + ".php", data);
        yield return request.SendWebRequest();

        if (request.responseCode != 200)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("Upload complete!");
        }
       // ConfirmPut();
    }

    //public void ConfirmPut()
    //{
    //    putPending = false;
    //}

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
        StartCoroutine(GetHTTPRequest("test", DisplayInfo)); // to be changed to logInfo        
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
            info.text = getContent;
            Debug.Log("There is something wrong with the log file or the GET content: " + getContent);
        }
    }

    private void UpdateLogInfo()
    {
        string newInfo = TimeToString(lastUpdate.Day) + "|" + TimeToString(lastUpdate.Month) + "|" + TimeToString(lastUpdate.Year) + "|" + TimeToString(lastUpdate.Hour) + "|" + TimeToString(lastUpdate.Minute) + "|" + TimeToString(lastUpdate.Second) + "|" + nextPatient; // to be changed
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
