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
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

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

    private String prevGetContent = ""; // contains the output from the previous GET operations (to compare with current)
    private String putContent = ""; // contains the text output representing the status of the scene to be send to the server
    private String prevPutContent = ""; // contains the text output representing the previous status of the scene sent to the server
    private String url = "dav.li/HypnoseAR/";

    private float nextGetCall;
    public float refreshingFreq = 5f;
    public bool sessionPending;
    public bool downloadPending;
    public bool isSafePlace;
    public static MainPanel Instance;

    // test
    public Text modeTest;
    public Text recievedTest;
    public bool noServerTest;
    //string testString = "<?xml version=\"1.0\"?><seance isStarted=\"true\"><id>XX1SI</id><mode>true</mode><date_create>1521033192</date_create><scene><object><selfId>1</selfId><sceneId>58</sceneId><position><x>-0.51</x><y>1.31</y><z>1</z></position></object></scene></seance>";

    ////#if WINDOWS_UWP
    //    Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings; //for saving log info
    //    Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
    ////#endif

    //string path = "info.txt"; // pb là
    //string path = "C:/Users/Romain/Documents/Unity/HypnoseARV3/Assets/test.txt";
    public string sessionId;

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
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(GameObject.Find("White Pointer"));
    }


    // Use this for initialization
    void Start () {
        sessionPending = false;
        downloadPending = false;
        nextGetCall = Time.time;
        Instance = this;
        sessionId = ""; // to be changed MI5F6
        noServerTest = false;
        isSafePlace = false;
	}
	// Update is called once per frame
	void Update () {
        // ask for starting message
        if (!sessionPending)
        {
            if(sessionPointer != null)
            {
                sessionPointer.SetActive(false);
            }
            if(modeTest != null)
            {
                modeTest.text = "UI mode";
            }

        }
        else
        {
            if(modeTest != null)
            {
                modeTest.text = "Session running";
            }
            if (sessionPointer != null)
            {
                sessionPointer.SetActive(true);
            }
        }
        // every x seconds make a get and send scene status:
        if (Time.time > nextGetCall && !sessionId.Equals(""))
        {
            nextGetCall += refreshingFreq;
            prevPutContent = putContent;
            string parameters = "id=" + sessionId + ""; // replace by by sessionId
            StartCoroutine(GetHTTPRequest("getSeance", parameters, ParseXMLandReply)); // replace by get or whatever -> get updates from server
            //UnityEngine.Debug.Log(testString);
           // ParseXMLandReply(testString);
        }
    }

    public void ParseXMLandReply(string content)
    {
        recievedTest.text = content;
        if (content.Equals("Error:404"))
        {
            Debug.Log(content);
            return;
        }
        // ParseXML, porcess the info on the scene via the SessionManager
        if (!prevGetContent.Equals(content))
        {
            SessionManager.Instance.ParseXML(content);
            //Debug.Log(getContent);
        }
        else
        {
            Debug.Log("same content");
        }
        prevGetContent = content;

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
                    //Debug.Log("Latest feedback : " + putContent);
                    StartCoroutine(PutHTTPRequest("putTest", putContent));
                }
                else
                {
                    //Debug.Log("same feedback content");
                }
            }
            else
            {
                Debug.Log("No feedback content");
            }
        }
    }

    public void UpdateLibCall()
    {
        if (!downloadPending && !sessionPending)
        {
            string function = "";
            string parameters = "db/3d/downloadList.xml";
            StartCoroutine(GetHTTPRequest(function, parameters, DownloadFiles));
        }
        else
        {
            Debug.Log("Download or session pending");
        }
        LibraryManager.Instance.PutUpdateLibInfo();
        UpdateLibInfo();
    }

    public void DownloadFiles(string instructionFile)
    {
        Debug.Log("DownloadFiles received content : " + instructionFile);
        string parameters = "";
        string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
        if (instructionFile.StartsWith(_byteOrderMarkUtf8))
        {
            instructionFile = instructionFile.Remove(0, _byteOrderMarkUtf8.Length);
        }
        XDocument instructionXml = XDocument.Parse(instructionFile);
        XElement rootNode = instructionXml.Descendants("downloadList").First<XElement>();
        IEnumerable<XElement> objList = rootNode.Elements("object");
        foreach(XElement objItem in objList)
        {
            string selfId = "", meshPath = "", texPath = "";
            XElement selfIdNode = objItem.Element("id");
            if (selfIdNode != null)
            {
                selfId = selfIdNode.Value;

            }
            XElement meshNode = objItem.Element("mesh");
            if(meshNode != null)
            {
                meshPath = meshNode.Value;
            }
            XElement texNode = objItem.Element("texture");
            if (texNode != null)
            {
                texPath = texNode.Value;
            }
            
            if (!selfId.Equals("") && !meshPath.Equals("") && !texPath.Equals(""))
            {
                string meshName = meshPath.Split('/').Last();
                string texName = texPath.Split('/').Last();
                StartCoroutine(LibraryManager.Instance.DownloadLibFile(parameters+ meshPath, meshName));
                StartCoroutine(LibraryManager.Instance.DownloadLibFile(parameters+texPath, texName));
                XElement infoRootNode = new XElement("root",selfIdNode,meshNode,texNode);
                XDocument infoFileXml = new XDocument();
                infoFileXml.Add(infoRootNode);
                byte[] data = Encoding.ASCII.GetBytes(infoFileXml.ToString());
                LibraryManager.Instance.WriteData(data, selfId + ".xml");
            }

        }
        
    }

    public void NewSession()
    {
        if (!downloadPending)
        {
            StartCoroutine(GetHTTPRequest("newSeance","", UpdateSessionId));
        }
        else
        {
            Debug.Log("Download pending");
        }

    }

    public void UpdateSessionId(string content)
    {
        sessionId = content;
        Debug.Log("sessionId : " + sessionId);
        sessionIdText.text = "Identifiant de session : " + sessionId;
    }

    IEnumerator GetHTTPRequest(string function, string parameters, Action<string> Callback)
    {
        if (!noServerTest)
        {
            string completeUrl = Url;
            if(function.Equals(""))
            {
                completeUrl += parameters;
            }
            if (parameters.Equals("") && !function.Equals(""))
            {
                completeUrl += function + ".php";
            }
            if(!parameters.Equals("") && !function.Equals(""))
            {
                completeUrl += function + ".php?" + parameters;
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
                //Debug.Log("Message : "+ getContent);
            }
            if (Callback != null)
            {
                //Debug.Log("Callback");
                Callback(request.downloadHandler.text);   
            }
        }
        else
        {
            //Callback(testString);
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
           // Debug.Log("Upload complete!");
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

    public void DisplayToConsole(string content)
    {
        Debug.Log("getContent :" + content);
    }

    public void UpdateLibInfo()
    {
        DateTime lastUpdate = LibraryManager.Instance.GetUpdateLibInfo();
        info.text = "Dernière mise à jour : " + TimeToString(lastUpdate.Day) + "/" + TimeToString(lastUpdate.Month) + "/" + TimeToString(lastUpdate.Year) + " à " + TimeToString(lastUpdate.Hour) + ":" + TimeToString(lastUpdate.Minute) + ":" + TimeToString(lastUpdate.Second); 
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
