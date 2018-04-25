using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Linq;
//using System.Xml.Serialization;
//using Windows.Data.Xml.Dom;
//using Windows.Foundation;
//using Windows;
using System.IO;
//using System.Xml;
// outline
using cakeslice;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Diagnostics;

public class HoloObject
{
    private GameObject obj;
    private int selfId;
    private string sceneId;
    private string prefabType;
    public float rotRate = 0.5f;// rotation rate of objects;

    public GameObject Obj
    {
        get
        {
            return obj;
        }

        set
        {
            obj = value;
        }
    }
    public string PrefabType
    {
        get
        {
            return prefabType;
        }

        set
        {
            prefabType = value;
        }
    }
    public string SceneId
    {
        get
        {
            return sceneId;
        }

        set
        {
            sceneId = value;
        }
    }
    public int SelfId
    {
        get
        {
            return selfId;
        }

        set
        {
            selfId = value;
        }
    }

    public HoloObject(HoloObjectDescription desc, GameObject parent)
    {
        selfId = desc.SelfId;
        sceneId = desc.SceneId;
        prefabType = "undefined"; // to be changed
        //obj = GameObject.CreatePrimitive(PrimitiveType.Cube); // to be changed
        //obj = LibraryManager.Instance.CreateObjectWithPrefab(desc.SelfId);
        string fileInfoName = desc.SelfId + ".xml";
        //obj = LibraryManager.Instance.CreateObjectAsync(fileInfoName).Result;
        Task<GameObject> objectTask = LibraryManager.Instance.CreateObjectAsync(fileInfoName);
        UnityEngine.Debug.Log("objectTask status : " + objectTask.Status);
        objectTask.Wait();
        obj = objectTask.Result;
        //obj = LibraryManager.Instance.CreateObjectAsync(fileInfoName);
        obj.transform.position = desc.Position;
        obj.transform.rotation = desc.Rotation;
        obj.transform.localScale = desc.Scale;
        obj.transform.SetParent(parent.transform);
        obj.transform.hasChanged = false;
        obj.name = desc.SceneId;
        obj.AddComponent<Outline>();
        obj.AddComponent<ObjectManager>();
        obj.GetComponent<ObjectManager>().interactible = desc.Interactible;
        if (desc.Interactible)
        {
            obj.AddComponent<MeshCollider>();
            //obj.GetComponent<MeshCollider>().m
        }

        obj.GetComponent<ObjectManager>().rotationVect = RandomVector();

        this.DealWithOtherParameters(desc);

        obj.SendMessage("OnDisable", SendMessageOptions.DontRequireReceiver);
        
    }

    public Texture2D ColorToTexture2D(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        Color[] colArray = new Color[1];
        colArray[0] = color;
        tex.SetPixels(colArray);
        return tex;
    }

    public Vector3 RandomVector()
    {
        float x, y, z;
        x = UnityEngine.Random.Range(-rotRate, rotRate);
        y = UnityEngine.Random.Range(-rotRate, rotRate);
        z = UnityEngine.Random.Range(-rotRate, rotRate);
        return new Vector3(x, y, z);
    }

    public void DealWithOtherParameters(HoloObjectDescription desc)
    {
        // shader/material modifications
        if(desc.DefCtrl == null)
        {
            return;
        }
        obj.GetComponent<Renderer>().material = SessionManager.Instance.paramShaderMaterial;
        obj.GetComponent<Renderer>().material.SetTexture("_MainTex", ColorToTexture2D(desc.DefCtrl.color0));
        obj.GetComponent<Renderer>().material.SetTexture("_SecTex", ColorToTexture2D(desc.DefCtrl.color1));
        obj.GetComponent<Renderer>().material.SetFloat("_Dist", desc.DefCtrl.amplitude);
        obj.GetComponent<Renderer>().material.SetFloat("_Speed", desc.DefCtrl.waveVelocity);
        obj.GetComponent<Renderer>().material.SetFloat("_WNb", desc.DefCtrl.waveNumber);
        obj.GetComponent<Renderer>().material.SetFloat("_TransPercent", desc.DefCtrl.transition);
        obj.GetComponent<Renderer>().material.SetFloat("_AmbLight", desc.DefCtrl.ambientLight);
        obj.GetComponent<Renderer>().material.SetFloat("_DiffLight", desc.DefCtrl.diffuseLight);
        obj.GetComponent<Renderer>().material.SetFloat("_SpecLight", desc.DefCtrl.specularLight);
    }

    public HoloObject(int id, string sceneId, string prefabType, GameObject obj)
    {
        this.selfId = id;
        this.sceneId = sceneId;
        this.prefabType = prefabType;
        this.obj = obj;
    }

    public string SerializePosition()
    {
        Vector3 pos = this.obj.transform.position;
        string output = pos.x.ToString() + "/" + pos.y.ToString() + "/" + pos.z.ToString();
        return output;
    }

    // to be able to send a object and its status
    public string SerializeObject()
    {
        // returning its id, prefab, position
        String output;
        output = "[OBJ|" + selfId + "|" + "|" + SerializePosition() + "]";
        return output;
    }

    override
    public string ToString()
    {
        return this.SerializeObject();
    }

    //public static void DebugLog(string str)
    //{
    //    Debug.Log("Thread : " + System.Threading.Thread.CurrentThread.ManagedThreadId +" // " + str);
    //}
}

public class HoloObjectDescription
{
    private int selfId;
    private string sceneId;
    private bool interactible;
    private Vector3 position;
    private Quaternion rotation;
    private Vector3 scale;
    private DeformableController defCtrl;
    private XElement otherContent; // to be parsed upon creation of the holoObject

    public int SelfId
    {
        get
        {
            return selfId;
        }

        set
        {
            selfId = value;
        }
    }
    public string SceneId
    {
        get
        {
            return sceneId;
        }

        set
        {
            sceneId = value;
        }
    }
    public Vector3 Position
    {
        get
        {
            return position;
        }

        set
        {
            position = value;
        }
    }
    public Quaternion Rotation
    {
        get
        {
            return rotation;
        }

        set
        {
            rotation = value;
        }
    }
    public Vector3 Scale
    {
        get
        {
            return scale;
        }

        set
        {
            scale = value;
        }
    }
    public XElement OtherContent
    {
        get
        {
            return otherContent;
        }

        set
        {
            otherContent = value;
        }
    }
    public bool Interactible
    {
        get
        {
            return interactible;
        }

        set
        {
            interactible = value;
        }
    }

    public DeformableController DefCtrl
    {
        get
        {
            return defCtrl;
        }

        set
        {
            defCtrl = value;
        }
    }

    override
    public string ToString()
    {
        return "selfId : " + SelfId + " / sceneID : " + SceneId + " / interactible : " + interactible + " / position : " + position.ToString() + " / rotation : " + rotation.ToString() + " / scale : " + scale.ToString();
    }
}

public class Seance
{
    private string seanceId;
    private DateTimeOffset seanceCreationDate;
    private List<HoloObjectDescription> activeObjects;

    public string SeanceId
    {
        get
        {
            return seanceId;
        }

        set
        {
            seanceId = value;
        }
    }
    public DateTimeOffset SeanceCreationDate
    {
        get
        {
            return seanceCreationDate;
        }

        set
        {
            seanceCreationDate = value;
        }
    }
    public List<HoloObjectDescription> ActiveObjects
    {
        get
        {
            return activeObjects;
        }

        set
        {
            activeObjects = value;
        }
    }

    override
    public string ToString()
    {
        return seanceId + " | " + seanceCreationDate.ToString() + " | " + ActiveObjectsToString();
    }

    public string ActiveObjectsToString()
    {
        string output = "[";
        foreach (HoloObjectDescription h in activeObjects)
        {
            output += h.ToString() + "|";
        }
        output = output.Remove(output.Length - 1) + "]";
        if (output == "]")
        {
            return "[EMPTY SET]";
        }
        else
        {
            return output;
        }
    }
}

public class DeformableController
{
    public Color color0;
    public Color color1;
    public float amplitude;
    public float waveVelocity;
    public float waveNumber;
    public float transition;
    public float ambientLight;
    public float diffuseLight;
    public float specularLight;

    public DeformableController(Color col0, Color col1, float amplitude, float waveVelocity, float waveNumber, float transition, float ambientLight, float diffuseLight, float specularLight)
    {
        this.color0 = col0;
        this.color1 = col1;
        this.amplitude = amplitude;
        this.waveVelocity = waveVelocity;
        this.waveNumber = waveNumber;
        this.transition = transition;
        this.ambientLight = ambientLight;
        this.diffuseLight = diffuseLight;
        this.specularLight = specularLight;
    }

    override
    public string ToString()
    {
        return "DeformableController : " + color0 + "/" + color1 + "/" + amplitude + "/" + waveVelocity + "/" + waveNumber + "/" + transition + "/" + ambientLight + "/" + diffuseLight + "/" + specularLight;
    }

}

public class SessionManager : MonoBehaviour
{

    public static SessionManager Instance;
    // miror structure reflecting the current scene
    public Seance currentSeance;

    // parent object containing the instanciated objects in the scene
    public GameObject parent;

    public Mutex modificationPending = new Mutex(); // locks the access if a thread/async function already modifying the scene
    private List<HoloObject> activeObjects;
    private string localPath;
    bool newSessionStatus; // true if pending / false else
    DateTime orig = new DateTime(1970, 1, 1, 0, 0, 0);
    public Material paramShaderMaterial;

    // modified object
    public List<HoloObjectDescription> changedObjects;

    //defual values
    public Vector3 defaultPos;
    public Quaternion defaultRot;


    public string LocalPath
    {
        get
        {
            return localPath;
        }

        set
        {
            localPath = value;
        }
    }
    
    // Use this for initialization
    void Start()
    {
        Instance = this;
#if WINDOWS_UWP
        localPath = ""; // path where library is stored on the hololens
#else
        localPath = "HoloLibrary/";
#endif
        activeObjects = new List<HoloObject>();
        changedObjects = new List<HoloObjectDescription>();
        currentSeance = new Seance
        {
            ActiveObjects = new List<HoloObjectDescription>()
        };
        parent = GameObject.Find("SceneObj");
        // deformation material
        //DirContent(Application.dataPath +"/Resources/"+ localPath);
        Material[] materials = Resources.LoadAll<Material>(LocalPath + "paramDef").ToArray();
        foreach(Material mat in materials)
        {
            UnityEngine.Debug.Log(mat);
        }
        paramShaderMaterial = Resources.Load<Material>(localPath + "paramDef/YParamDef");

        defaultPos = new Vector3(0, 0, 5);
        defaultRot = UnityEngine.Random.rotation;
    }

    public static void DirContent(string path)
    {
        var info = new DirectoryInfo(path);
        var fileInfo = info.GetFiles();
        foreach (FileInfo file in fileInfo)
        {
            UnityEngine.Debug.Log(file);
        }
    }

    private void Update()
    {
        if (!MainPanel.Instance.isSafePlace)
        {
            OnObjectChange(); // try to see if this could be called from the Gesture Manager update function
        }
    }


    // Converts the XML to a new scene decription
    public void ParseXML(string getContent)
    {
        if (!getContent.Equals(""))
        {
            // Creating equivalent representation of the scene
            UnityEngine.Debug.Log(getContent);
            XDocument seanceXml = XDocument.Parse(getContent);
            
            // id
            string newSeanceId = seanceXml.Descendants("id").First<XElement>().Value;
            if(currentSeance.SeanceId != null && currentSeance.SeanceId != newSeanceId)
            {
                UnityEngine.Debug.Log("Seance Id changed !! Previous " + currentSeance.SeanceId + " // New : " + newSeanceId);
            }
            currentSeance.SeanceId = newSeanceId;

            // create_date
            double secs;
            double.TryParse(seanceXml.Descendants("date_create").First<XElement>().Value, out secs);
            currentSeance.SeanceCreationDate = orig.AddSeconds(secs);

            // safe place mode?
            if (SceneSwicth(seanceXml))
            {
                return;
            }
            
            // creating holoobjevctsdescription
            IEnumerable<XElement> objects = seanceXml.Descendants("object");
            //locking access via mutex
            modificationPending.WaitOne();
            // reset
            currentSeance.ActiveObjects = new List<HoloObjectDescription>();
            // populate
            foreach (XElement node in objects)
            {
                AddHoloObjectDescription(node);
            }
            UnityEngine.Debug.Log(currentSeance.ToString());
            ModifyScene();
            //unlocking access
            modificationPending.ReleaseMutex();

            // pending
            // to be implemented

            //fading ui
            bool.TryParse(seanceXml.Element("seance").Attribute("isStarted").Value, out newSessionStatus);
            if (MainPanel.Instance.sessionPending != newSessionStatus && newSessionStatus == true) // beginning session
            {
                MainPanel.Instance.FadeOutPanel(MainPanel.Instance.mainPanel);
            }
            if (MainPanel.Instance.sessionPending != newSessionStatus && newSessionStatus == false) // ending session
            {
                MainPanel.Instance.FadeInPanel(MainPanel.Instance.mainPanel);
            }
            MainPanel.Instance.sessionPending = newSessionStatus;
        }
    }

    public void AddHoloObjectDescription(XElement node)
    {
        //Debug.Log(node.Value);
        int sfid;
        int.TryParse(node.Element("selfId").Value, out sfid);

        // Fields that might not exist
        // position
        Vector3 pos = defaultPos;
        XElement posNode = node.Element("position");
        if (posNode != null)
        {
            float x, y, z;
#if WINDOWS_UWP
            float.TryParse(posNode.Element("x").Value , out x);
            float.TryParse(posNode.Element("y").Value , out y);
            float.TryParse(posNode.Element("z").Value , out z);
#else
            float.TryParse(posNode.Element("x").Value.Replace(".", ","), out x);
            float.TryParse(posNode.Element("y").Value.Replace(".", ","), out y);
            float.TryParse(posNode.Element("z").Value.Replace(".", ","), out z);
#endif
            pos.x = x;
            pos.y = y;
            pos.z = z;
        }
        //rotation
        Quaternion rot = defaultRot;
        XElement rotNode = node.Element("rotation");
        if (rotNode != null)
        {
            float x, y, z, w;
#if WINDOWS_UWP
            float.TryParse(rotNode.Element("x").Value , out x);
            float.TryParse(rotNode.Element("y").Value , out y);
            float.TryParse(rotNode.Element("z").Value , out z);
            float.TryParse(rotNode.Element("w").Value , out w);
#else
            float.TryParse(rotNode.Element("x").Value.Replace(".", ","), out x);
            float.TryParse(rotNode.Element("y").Value.Replace(".", ","), out y);
            float.TryParse(rotNode.Element("z").Value.Replace(".", ","), out z);
            float.TryParse(rotNode.Element("w").Value.Replace(".", ","), out w);
#endif
            rot.x = x;
            rot.y = y;
            rot.z = z;
            rot.w = w;
        }
        // scale
        Vector3 scale = Vector3.one;
        XElement sclNode = node.Element("scale");
        if (sclNode != null)
        {
            float x, y, z;
#if WINDOWS_UWP
            float.TryParse(sclNode.Element("x").Value , out x);
            float.TryParse(sclNode.Element("y").Value , out y);
            float.TryParse(sclNode.Element("z").Value , out z);
#else
            float.TryParse(sclNode.Element("x").Value.Replace(".", ","), out x);
            float.TryParse(sclNode.Element("y").Value.Replace(".", ","), out y);
            float.TryParse(sclNode.Element("z").Value.Replace(".", ","), out z);
#endif
            scale.x = x;
            scale.y = y;
            scale.z = z;
        }

        // interactible
        bool interactible = true; // default value
        XElement itrNode = node.Element("interactible");
        if(itrNode != null)
        {
            bool.TryParse(itrNode.Value, out interactible);
        }

        // deformable
        XElement defNode = node.Element("deformable");
        DeformableController defCtrl;
        if (defNode != null)
        {
            // colors
            XElement col0Node = defNode.Element("color0");
            XElement col1Node = defNode.Element("color1");
            Color col0 = Color.black; // default
            Color col1 = Color.white; // default
#if WINDOWS_UWP
            float.TryParse(col0Node.Element("r").Value , out col0.r);
            float.TryParse(col0Node.Element("g").Value , out col0.g);
            float.TryParse(col0Node.Element("b").Value , out col0.b);
            float.TryParse(col0Node.Element("a").Value , out col0.a);
            float.TryParse(col1Node.Element("r").Value , out col1.r);
            float.TryParse(col1Node.Element("g").Value , out col1.g);
            float.TryParse(col1Node.Element("b").Value , out col1.b);
            float.TryParse(col1Node.Element("a").Value , out col1.a);
#else
            float.TryParse(col0Node.Element("r").Value.Replace(".", ","), out col0.r);
            float.TryParse(col0Node.Element("g").Value.Replace(".", ","), out col0.g);
            float.TryParse(col0Node.Element("b").Value.Replace(".", ","), out col0.b);
            float.TryParse(col0Node.Element("a").Value.Replace(".", ","), out col0.a);
            float.TryParse(col1Node.Element("r").Value.Replace(".", ","), out col1.r);
            float.TryParse(col1Node.Element("g").Value.Replace(".", ","), out col1.g);
            float.TryParse(col1Node.Element("b").Value.Replace(".", ","), out col1.b);
            float.TryParse(col1Node.Element("a").Value.Replace(".", ","), out col1.a);
#endif
            // normalizing
            col0.r = Mathf.Clamp(col0.r, 0, 1);
            col0.g = Mathf.Clamp(col0.g, 0, 1);
            col0.b = Mathf.Clamp(col0.b, 0, 1);
            col0.a = Mathf.Clamp(col0.a, 0, 1);
            col1.r = Mathf.Clamp(col1.r, 0, 1);
            col1.g = Mathf.Clamp(col1.g, 0, 1);
            col1.b = Mathf.Clamp(col1.b, 0, 1);
            col1.a = Mathf.Clamp(col1.a, 0, 1);
            //other parameters
            float ampl, wVel, wNb, trans, amb, dif, spec; //wave amplitude, velocity, number, transition between both colors, ambient, diffuse and specular lighting
#if WINDOWS_UWP
            float.TryParse(defNode.Element("amplitude").Value , out ampl);
            float.TryParse(defNode.Element("waveVel").Value , out wVel);
            float.TryParse(defNode.Element("waveNb").Value , out wNb);
            float.TryParse(defNode.Element("transition").Value , out trans);
            float.TryParse(defNode.Element("ambient").Value , out amb);
            float.TryParse(defNode.Element("diffuse").Value , out dif);
            float.TryParse(defNode.Element("specular").Value , out spec);
#else
            float.TryParse(defNode.Element("amplitude").Value.Replace(".", ","), out ampl);
            float.TryParse(defNode.Element("waveVel").Value.Replace(".", ","), out wVel);
            float.TryParse(defNode.Element("waveNb").Value.Replace(".", ","), out wNb);
            float.TryParse(defNode.Element("transition").Value.Replace(".", ","), out trans);
            float.TryParse(defNode.Element("ambient").Value.Replace(".", ","), out amb);
            float.TryParse(defNode.Element("diffuse").Value.Replace(".", ","), out dif);
            float.TryParse(defNode.Element("specular").Value.Replace(".", ","), out spec);

#endif
            defCtrl = new DeformableController(col0, col1, ampl, wVel, wNb, trans, amb, dif, spec);
        }
        else
        {
            defCtrl = null;
        }
        HoloObjectDescription currentobj = new HoloObjectDescription
        {
            SceneId = node.Element("sceneId").Value,
            SelfId = sfid,
            Position = pos,
            Rotation = rot,
            Scale = scale,
            Interactible = interactible,
            DefCtrl = defCtrl
        };
        currentSeance.ActiveObjects.Add(currentobj);
    }

    // Keeps the coherence between HoloObject list and HoloObjectDescription list
    public void ModifyScene()
    {
        // creation or modification
        foreach (HoloObjectDescription desc in currentSeance.ActiveObjects)
        {
            HoloObject currentObj = CheckSceneIdInActive(desc.SceneId);
            if(currentObj != null)
            {
                // object is already there, maybe needs to be modified
                UnityEngine.Debug.Log("Object found. Modification is possible");
                currentObj.Obj.transform.position = desc.Position;
                currentObj.Obj.transform.rotation = desc.Rotation;
                currentObj.Obj.transform.localScale = desc.Scale;
                currentObj.Obj.GetComponent<ObjectManager>().interactible = desc.Interactible;
                currentObj.DealWithOtherParameters(desc);
            }
            else
            {
                // new object
                UnityEngine.Debug.Log("Object not found. Creating new object");
                activeObjects.Add(new HoloObject(desc, parent));
            }
        }
        // destruction
        foreach (HoloObject obj in activeObjects)
        {
            HoloObjectDescription currDesc = CheckSceneIdInDesc(obj.SceneId);
            if (currDesc == null)
            {
                // no description corresponds to the active object. It shall be deleted
                Destroy(obj.Obj);
                activeObjects.Remove(obj);
            }
        }

    }
    // checks if the active object can be linked to a desc
    public HoloObjectDescription CheckSceneIdInDesc(string sceneId)
    {
        foreach (HoloObjectDescription desc in currentSeance.ActiveObjects)
        {
            if (sceneId.Equals(desc.SceneId))
            {
                return desc;
            }
        }
        return null;
    }
    // checks if the desc can be linked to an active object
    public HoloObject CheckSceneIdInActive(string sceneId)
    {
        //DebugLog("number of active objects : " + activeObjects.Count + " // required sceneId : '" + sceneId+ "'");
        foreach (HoloObject obj in activeObjects)
        {
            //DebugLog("current active object : '" + obj.SceneId+ "'");
            if (sceneId.Equals(obj.SceneId))
            {
                UnityEngine.Debug.Log("  // return : " + obj);
                return obj;
            }
            //DebugLog("/" + obj.SceneId + "//" + sceneId + "/");
        }
        return null;
    }

    // creates a Xml document representing the modify object
    public XDocument MkObjXmlFeedback(List<HoloObjectDescription> hListObj)
    {
        if (hListObj.Any())
        {
            XDocument xmlDoc = new XDocument();
            XElement xmlRoot = new XElement("root");
            foreach (HoloObjectDescription hObj in hListObj)
            {
                XElement xmlObj = new XElement("object");
                XElement selfIdNode = new XElement("selfId");
                selfIdNode.SetValue(hObj.SelfId.ToString());
                xmlObj.Add(selfIdNode);

                XElement sceneIdNode = new XElement("sceneId");
                sceneIdNode.SetValue(hObj.SceneId);
                xmlObj.Add(sceneIdNode);

                // position (Vector3)
                XElement positionNode = new XElement("position");
                XElement posXNode = new XElement("x");
                posXNode.SetValue(hObj.Position.x.ToString());
                XElement posYNode = new XElement("y");
                posYNode.SetValue(hObj.Position.y.ToString());
                XElement posZNode = new XElement("z");
                posZNode.SetValue(hObj.Position.z.ToString());
                positionNode.Add(posXNode);
                positionNode.Add(posYNode);
                positionNode.Add(posZNode);
                xmlObj.Add(positionNode);

                // rotation (Quaternion)
                XElement rotationNode = new XElement("rotation");
                XElement rotXNode = new XElement("x");
                rotXNode.SetValue(hObj.Rotation.x.ToString());
                XElement rotYNode = new XElement("y");
                rotYNode.SetValue(hObj.Rotation.y.ToString());
                XElement rotZNode = new XElement("z");
                rotZNode.SetValue(hObj.Rotation.z.ToString());
                XElement rotWNode = new XElement("w");
                rotWNode.SetValue(hObj.Rotation.w.ToString());
                rotationNode.Add(rotXNode);
                rotationNode.Add(rotYNode);
                rotationNode.Add(rotZNode);
                rotationNode.Add(rotWNode);
                xmlObj.Add(rotationNode);

                // scale (Vector3)
                XElement scaleNode = new XElement("scale");
                XElement sclXNode = new XElement("x");
                sclXNode.SetValue(hObj.Scale.x.ToString());
                XElement sclYNode = new XElement("y");
                sclYNode.SetValue(hObj.Scale.y.ToString());
                XElement sclZNode = new XElement("z");
                sclZNode.SetValue(hObj.Scale.z.ToString());
                scaleNode.Add(sclXNode);
                scaleNode.Add(sclYNode);
                scaleNode.Add(sclZNode);
                xmlObj.Add(scaleNode);

                //deformation
                if(hObj.DefCtrl != null)
                {
                    DeformableController current = hObj.DefCtrl;
                    XElement defNode = new XElement("deformable");
                    XElement col0Node = new XElement("color0");
                    XElement red0 = new XElement("r");
                    red0.SetValue(current.color0.r);
                    col0Node.Add(red0);
                    XElement green0 = new XElement("g");
                    green0.SetValue(current.color0.g);
                    col0Node.Add(green0);
                    XElement blue0 = new XElement("b");
                    blue0.SetValue(current.color0.b);
                    col0Node.Add(blue0);
                    XElement alpha0 = new XElement("a");
                    alpha0.SetValue(current.color0.a);
                    col0Node.Add(alpha0);

                    XElement col1Node = new XElement("color1");
                    XElement red1 = new XElement("r");
                    red1.SetValue(current.color0.r);
                    col0Node.Add(red1);
                    XElement green1 = new XElement("g");
                    green1.SetValue(current.color0.g);
                    col0Node.Add(green1);
                    XElement blue1 = new XElement("b");
                    blue1.SetValue(current.color0.b);
                    col0Node.Add(blue1);
                    XElement alpha1 = new XElement("a");
                    alpha1.SetValue(current.color0.a);
                    col0Node.Add(alpha1);

                    XElement ampNode = new XElement("amplitude");
                    ampNode.SetValue(current.amplitude);

                    XElement waveVelNode = new XElement("waveVel");
                    waveVelNode.SetValue(current.waveVelocity);

                    XElement waveNbNode = new XElement("waveNb");
                    waveNbNode.SetValue(current.waveNumber);

                    XElement transNode = new XElement("transition");
                    transNode.SetValue(current.transition);

                    XElement ambNode = new XElement("ambient");
                    ambNode.SetValue(current.ambientLight);

                    XElement difNode = new XElement("diffuse");
                    difNode.SetValue(current.diffuseLight);

                    XElement specNode = new XElement("specular");
                    specNode.SetValue(current.specularLight);
                    defNode.Add(col0Node);
                    defNode.Add(col1Node);
                    defNode.Add(ampNode);
                    defNode.Add(waveVelNode);
                    defNode.Add(waveNbNode);
                    defNode.Add(transNode);
                    defNode.Add(ambNode);
                    defNode.Add(difNode);
                    defNode.Add(specNode);

                    xmlObj.Add(defNode);
                }
                xmlRoot.Add(xmlObj);
            }
            xmlDoc.Add(xmlRoot);
            //Debug.Log("xmlDoc : " + xmlDoc.ToString());
            hListObj = new List<HoloObjectDescription>();
            return xmlDoc;
        }
        else
        {
            return null;
        }
    }

    // creates a Xml document representing the complete scene
    //public XDocument MkSceneXmlFeedback()
    //{
    //    if (MainPanel.Instance.sessionPending && currentSeance.SeanceId != null)
    //    {
    //        XDocument xmlDoc = new XDocument();
    //        XElement seanceNode = new XElement("seance");
    //        seanceNode.SetAttributeValue("isStarted", "true");
    //        xmlDoc.Add(seanceNode);

    //        XElement idNode = new XElement("id");
    //        idNode.SetValue(currentSeance.SeanceId);
    //        seanceNode.Add(idNode);
            
    //        XElement lastUpdateNode = new XElement("lastUpdate");
    //        int updateDate = (int)(DateTime.Now - orig).TotalSeconds;
    //        lastUpdateNode.SetValue(updateDate.ToString());
    //        seanceNode.Add(lastUpdateNode);

    //        XElement sceneNode = new XElement("scene");
    //        foreach (HoloObjectDescription desc in currentSeance.ActiveObjects)
    //        {
    //            XElement node = new XElement("object");
    //            XElement selfIdNode = new XElement("selfId");
    //            selfIdNode.SetValue(desc.SelfId.ToString());
    //            node.Add(selfIdNode);
    //            XElement sceneIdNode = new XElement("sceneId");
    //            sceneIdNode.SetValue(desc.SceneId);
    //            node.Add(sceneIdNode);
    //            XElement positionNode = new XElement("position");
    //            XElement posXNode = new XElement("x");
    //            posXNode.SetValue(desc.Position.x.ToString());
    //            XElement posYNode = new XElement("y");
    //            posYNode.SetValue(desc.Position.y.ToString());
    //            XElement posZNode = new XElement("z");
    //            posZNode.SetValue(desc.Position.z.ToString());

    //            positionNode.Add(posXNode);
    //            positionNode.Add(posYNode);
    //            positionNode.Add(posZNode);
    //            node.Add(positionNode);
    //            XElement rotationNode = new XElement("rotation");
    //            XElement rotXNode = new XElement("x");
    //            rotXNode.SetValue(desc.Rotation.x.ToString());
    //            XElement rotYNode = new XElement("y");
    //            rotYNode.SetValue(desc.Rotation.y.ToString());
    //            XElement rotZNode = new XElement("z");
    //            rotZNode.SetValue(desc.Rotation.z.ToString());
    //            XElement rotWNode = new XElement("w");
    //            rotWNode.SetValue(desc.Rotation.w.ToString());

    //            rotationNode.Add(rotXNode);
    //            rotationNode.Add(rotYNode);
    //            rotationNode.Add(rotZNode);
    //            rotationNode.Add(rotWNode);
    //            node.Add(rotationNode);
    //            XElement scaleNode = new XElement("scale");
    //            XElement sclXNode = new XElement("x");
    //            sclXNode.SetValue(desc.Scale.x.ToString());
    //            XElement sclYNode = new XElement("y");
    //            sclYNode.SetValue(desc.Scale.y.ToString());
    //            XElement sclZNode = new XElement("z");
    //            sclZNode.SetValue(desc.Scale.z.ToString());

    //            scaleNode.Add(sclXNode);
    //            scaleNode.Add(sclYNode);
    //            scaleNode.Add(sclZNode);
    //            node.Add(scaleNode);

    //            sceneNode.Add(node);
    //        }
    //        seanceNode.Add(sceneNode);
    //        return xmlDoc;
    //    }
    //    else
    //    {
    //        Debug.Log("Can't serialize scene objects since sessionPending == false");
    //        return null;
    //    }

    //}

    // to be called everytime the patient moves objects to preserve coherence between HoloObject list and HoloObjectDescription listand returns the list of the changed Objects
    public void OnObjectChange()
    {
        foreach (HoloObject obj in activeObjects)
        {
            //searches for the changed object
            if (obj.Obj.transform.hasChanged)
            {
                HoloObjectDescription currentDesc = CheckSceneIdInDesc(obj.SceneId);
                if (currentDesc != null)
                {
                    currentDesc.Position = obj.Obj.transform.position;
                    currentDesc.Rotation = obj.Obj.transform.rotation;
                    currentDesc.Scale = obj.Obj.transform.localScale;
                    obj.Obj.transform.hasChanged = false;
                    changedObjects.Remove(currentDesc);
                    changedObjects.Add(currentDesc);
                }
                else
                {
                    UnityEngine.Debug.Log("Incoherence between HoloObjectDescription list and HoloObject list activeObjects.");
                }
            }

        }
    }

    IEnumerator BlackScreen(bool opaque, float aTime)
    {
        Texture2D texture = new Texture2D(1, 1);
        Color[] colors = new Color[1];
        float alpha;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            if (opaque)
            {
                // fadeIn
                alpha = 1.0f - t;
            }
            else
            {
                //fadeOut
                alpha = t;
            }
            colors[0] = new Color(0, 0, 0, alpha);
            texture.SetPixels(colors);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
            yield return null;
        }
    }


    public bool SceneSwicth(XDocument seanceXml)
    {
        UnityEngine.Debug.Log("sceneSwitch");
        bool previousIsSafePlace = MainPanel.Instance.isSafePlace;
        try
        {
            bool.TryParse(seanceXml.Descendants("mode").First<XElement>().Value, out MainPanel.Instance.isSafePlace);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("no mode node found: " + e);
        }

        if (MainPanel.Instance.isSafePlace)
        {
            if (previousIsSafePlace != MainPanel.Instance.isSafePlace)
            {
                // vider la liste des hologrammes et charger la scène
                Stopwatch elapsedSec = new Stopwatch();
                long timeOut = 3000L; // 3 sec 
                elapsedSec.Start();
                //StartCoroutine(BlackScreen(true, 1.0f));
                string sceneName = "holoSafePlace"; // to be changed if several safe places
                StartCoroutine(LoadAsyncScene(sceneName));
                Singleton<GestureManager>.Instance.Transition(Singleton<GestureManager>.Instance.NavigationRecognizer);
                foreach (HoloObject obj in activeObjects)
                {
                    Destroy(obj.Obj);
                }
                currentSeance.ActiveObjects = new List<HoloObjectDescription>();
                activeObjects = new List<HoloObject>();
                elapsedSec.Stop();
                //StartCoroutine(BlackScreen(false, 1.0f));
            }
            return true;
        }

        // hologram mode
        if (previousIsSafePlace != MainPanel.Instance.isSafePlace)
        {
            // unload scene and reload original scene
            Stopwatch elapsedSec = new Stopwatch();
            long timeOut = 3000L; // 3 sec 
            elapsedSec.Start();
            //StartCoroutine(BlackScreen(true, 1.0f));
            string sceneName = "holoLensScene"; // to be changed if several safe places
            StartCoroutine(LoadAsyncScene(sceneName));
            Singleton<GestureManager>.Instance.Transition(Singleton<GestureManager>.Instance.NavigationRecognizer);
            //StartCoroutine(BlackScreen(false, 1.0f));
        }
        return false;
    }

    IEnumerator LoadAsyncScene(string sceneName)
    {
        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneName);
        loading.allowSceneActivation = false;
        //while (loading.progress <= 0.89f)
        //{
        //    UnityEngine.Debug.Log("Loading : " + loading.progress * 100 + "%");
        //    yield return null;
        //}
        while (!loading.isDone)
        {
            UnityEngine.Debug.Log("Loading : " + loading.progress * 100 + "%");
            if (loading.progress == 0.9f)
            {
                UnityEngine.Debug.Log("scene activation");
                loading.allowSceneActivation = true;
            }
            yield return null;
        }
        UnityEngine.Debug.Log("Done");
        yield return null;
    }
}
