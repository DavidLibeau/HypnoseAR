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

public class HoloObject
{
    private GameObject obj;
    private int selfId;
    private string sceneId;
    private string prefabType;

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
        obj = CreateObjectWithPrefab(desc.SelfId);
        obj.transform.position = desc.Position;
        obj.transform.rotation = desc.Rotation;
        obj.transform.localScale = desc.Scale;
        obj.transform.SetParent(parent.transform);
        obj.transform.hasChanged = false;
        obj.name = desc.SceneId;
        obj.AddComponent<Outline>();
        obj.AddComponent<ObjectManager>();
        obj.GetComponent<ObjectManager>().interactible = desc.Interactible;
        obj.SendMessage("OnDisable", SendMessageOptions.DontRequireReceiver);
        this.DealWithOtherParameters(desc.OtherContent);
    }

    public GameObject CreateObjectWithPrefab(int prefabId)
    {
        //GameObject newObject = Resources.Load(SessionManager.Instance.LocalPath + prefabId + ".prefab") as GameObject;
        Debug.Log("prefabId : " + prefabId);

        // to be changed
        GameObject model = Resources.Load<GameObject>("HoloLibrary/Prefab/" + prefabId);
        //Debug.Log(model);
        
        GameObject newObject = MonoBehaviour.Instantiate(model);
        //GameObject newObject = (GameObject)AssetBundle.LoadAsset("library/Prefab/" + prefabId + ".prefab", typeof(Ga));
        Debug.Log("newOjbect is null ? " + newObject.ToString());
        return newObject;
    }


    public void DealWithOtherParameters(XElement otherContent)
    {
        // to be completed
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
        output = "[OBJ|" + selfId + "|" + prefabType + "|" + SerializePosition() + "]";
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

public class SessionManager : MonoBehaviour
{

    public static SessionManager Instance;

    // miror structure reflecting the current scene
    public Seance currentSeance;

    // parent object containing the instanciated objects in the scene
    public GameObject parent;

    private List<HoloObject> activeObjects;
    private string localPath;
    private int nextId = 0;
    bool newSessionStatus; // true if pending / false else
    DateTime orig = new DateTime(1970, 1, 1, 0, 0, 0);

    // modified object
    public List<HoloObjectDescription> changedObjects;

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
        LocalPath = ""; // path where library is stored on the hololens
        activeObjects = new List<HoloObject>();
        changedObjects = new List<HoloObjectDescription>();
        currentSeance = new Seance
        {
            ActiveObjects = new List<HoloObjectDescription>()
        };
        parent = GameObject.Find("SceneObj");
    }

    private void Update()
    {
        OnObjectChange(); // try to see if this could be called from the Gesture Manager update function
    }
    // Converts the XML to a new scene decription
    public void ParseXML(string getContent)
    {
        if (!getContent.Equals(""))
        {
            // Creating equivalent representation of the scene
            //XmlDocument seanceXml = new XmlDocument();
            Debug.Log(getContent);
            XDocument seanceXml = XDocument.Parse(getContent);
            //seanceXml.LoadXml(getContent);
            //Debug.Log(getContent);

            // id
            //currentSeance.SeanceId = seanceXml.SelectSingleNode("seance/id").InnerText;
            string newSeanceId = seanceXml.Descendants("id").First<XElement>().Value;
            if(currentSeance.SeanceId != null && currentSeance.SeanceId != newSeanceId)
            {
                Debug.Log("Seance Id changed !! Previous " + currentSeance.SeanceId + " // New : " + newSeanceId);
            }
            currentSeance.SeanceId = newSeanceId;

            // create_date
            double secs;
            //double.TryParse(seanceXml.SelectSingleNode("seance/date_create").InnerText, out secs);
            double.TryParse(seanceXml.Descendants("date_create").First<XElement>().Value, out secs);
            currentSeance.SeanceCreationDate = orig.AddSeconds(secs);

            // creating holoobjevctsdescription
            //XmlNodeList objects = seanceXml.SelectNodes("seance/scene/object");
            IEnumerable<XElement> objects = seanceXml.Descendants("object");
            // reset
            currentSeance.ActiveObjects = new List<HoloObjectDescription>();
            // populate
            foreach (XElement node in objects)
            //for (uint i= 0; i<objects.Length; i++)
            {
                AddHoloObjectDescription(node);
                //Debug.Log(node.ToString());
            }
            Debug.Log(currentSeance.ToString());
            ModifyScene();

            // pending
            // to be implemented

            //fading ui
            //bool.TryParse(seanceXml.SelectSingleNode("seance").Attributes.GetNamedItem("isStarted").InnerText,out newSessionStatus);
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
        //int.TryParse(node.SelectSingleNode("selfId").InnerText, out sfid);
        int.TryParse(node.Element("selfId").Value, out sfid);

        // Fields that might not exist
        Vector3 rand = new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(2, 5));
        // position
        Vector3 pos = rand;
        //IXmlNode posNode = node.SelectSingleNode("position");
        XElement posNode = node.Element("position");
        if (posNode != null)
        {
            float x, y, z;
            //float.TryParse(posNode.SelectSingleNode("x").InnerText, out x);
            //float.TryParse(posNode.SelectSingleNode("y").InnerText, out y);
            //float.TryParse(posNode.SelectSingleNode("z").InnerText, out z);
            float.TryParse(posNode.Element("x").Value, out x);
            float.TryParse(posNode.Element("y").Value, out y);
            float.TryParse(posNode.Element("z").Value, out z);
            pos.x = x;
            pos.y = y;
            pos.z = z;
        }
        //rotation
        Quaternion rot = UnityEngine.Random.rotation;
        //IXmlNode rotNode = node.SelectSingleNode("rotation");
        XElement rotNode = node.Element("rotation");
        if (rotNode != null)
        {
            float x, y, z, w;
            //float.TryParse(posNode.SelectSingleNode("x").InnerText, out x);
            //float.TryParse(posNode.SelectSingleNode("y").InnerText, out y);
            //float.TryParse(posNode.SelectSingleNode("z").InnerText, out z);
            //float.TryParse(posNode.SelectSingleNode("w").InnerText, out w);
            float.TryParse(posNode.Element("x").Value, out x);
            float.TryParse(posNode.Element("y").Value, out y);
            float.TryParse(posNode.Element("z").Value, out z);
            float.TryParse(posNode.Element("w").Value, out w);
            rot.x = x;
            rot.y = y;
            rot.z = z;
            rot.w = w;
        }
        // scale
        Vector3 scale = Vector3.one;
        //IXmlNode sclNode = node.SelectSingleNode("scale");
        XElement sclNode = node.Element("scale");
        if (sclNode != null)
        {
            float x, y, z;
            //float.TryParse(posNode.SelectSingleNode("x").InnerText, out x);
            //float.TryParse(posNode.SelectSingleNode("y").InnerText, out y);
            //float.TryParse(posNode.SelectSingleNode("z").InnerText, out z);
            float.TryParse(posNode.Element("x").Value, out x);
            float.TryParse(posNode.Element("y").Value, out y);
            float.TryParse(posNode.Element("z").Value, out z);
            scale.x = x;
            scale.y = y;
            scale.z = z;
        }
        bool interactible = true; // default value
        XElement itrNode = node.Element("interactible");
        if(itrNode != null)
        {
            bool.TryParse(itrNode.Value, out interactible);
        }

        HoloObjectDescription currentobj = new HoloObjectDescription
        {
            //SceneId = node.SelectSingleNode("sceneId").InnerText,
            SceneId = node.Element("sceneId").Value,
            SelfId = sfid,
            Position = pos,
            Rotation = rot,
            Scale = scale,
            Interactible = interactible
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
            //if (currentObj != null)
            {
                // object is already there, maybe needs to be modified
                Debug.Log("Object found. Modification is possible");
                currentObj.Obj.transform.position = desc.Position;
                currentObj.Obj.transform.rotation = desc.Rotation;
                currentObj.Obj.transform.localScale = desc.Scale;
                currentObj.Obj.GetComponent<ObjectManager>().interactible = desc.Interactible;
                currentObj.DealWithOtherParameters(desc.OtherContent);
            }
            else
            {
                // new object
                Debug.Log("Object not found. Creating new object");
                activeObjects.Add(new HoloObject(desc, parent));
            }
        }
        // destuction
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
                Debug.Log("  // return : " + obj);
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
    public XDocument MkSceneXmlFeedback()
    {
        if (MainPanel.Instance.sessionPending && currentSeance.SeanceId != null)
        {
            XDocument xmlDoc = new XDocument();
            XElement seanceNode = new XElement("seance");
            //XmlNode seanceNode = xmlDoc.CreateElement("seance");
            //XmlAttribute isStartedAttr = xmlDoc.CreateAttribute("isStarted");
            //isStartedAttr.InnerText = "true";
            //seanceNode.Attributes.Append(isStartedAttr);
            //seanceNode.SetAttribute("isStarted", "true");
            seanceNode.SetAttributeValue("isStarted", "true");
            //xmlDoc.AppendChild(seanceNode);
            xmlDoc.Add(seanceNode);

            //XmlElement idNode = xmlDoc.CreateElement("id");
            XElement idNode = new XElement("id");
            //idNode.InnerText = currentSeance.SeanceId;
            idNode.SetValue(currentSeance.SeanceId);
            //seanceNode.AppendChild(idNode);
            seanceNode.Add(idNode);

            //XmlElement lastUpdateNode = xmlDoc.CreateElement("lastUpdate");
            XElement lastUpdateNode = new XElement("lastUpdate");
            int updateDate = (int)(DateTime.Now - orig).TotalSeconds;
            //lastUpdateNode.InnerText = updateDate.ToString();
            lastUpdateNode.SetValue(updateDate.ToString());
            //seanceNode.AppendChild(lastUpdateNode);
            seanceNode.Add(lastUpdateNode);

            //XmlElement sceneNode = xmlDoc.CreateElement("scene");
            XElement sceneNode = new XElement("scene");
            foreach (HoloObjectDescription desc in currentSeance.ActiveObjects)
            {
                //XmlElement node = xmlDoc.CreateElement("object");
                XElement node = new XElement("object");
                // selfId
                //XmlElement selfIdNode = xmlDoc.CreateElement("selfId");
                XElement selfIdNode = new XElement("selfId");
                //selfIdNode.InnerText = desc.SelfId.ToString();
                selfIdNode.SetValue(desc.SelfId.ToString());
                //node.AppendChild(selfIdNode);
                node.Add(selfIdNode);
                //sceneId
                //XmlElement sceneIdNode = xmlDoc.CreateElement("sceneId");
                XElement sceneIdNode = new XElement("sceneId");
                //sceneIdNode.InnerText = desc.SceneId;
                sceneIdNode.SetValue(desc.SceneId);
                //node.AppendChild(sceneIdNode);
                node.Add(sceneIdNode);
                // position (Vector3)
                //XmlElement positionNode = xmlDoc.CreateElement("position");
                XElement positionNode = new XElement("position");
                //XmlElement posXNode = xmlDoc.CreateElement("x");
                XElement posXNode = new XElement("x");
                //posXNode.InnerText = desc.Position.x.ToString();
                posXNode.SetValue(desc.Position.x.ToString());
                //XmlElement posYNode = xmlDoc.CreateElement("y");
                XElement posYNode = new XElement("y");
                //posYNode.InnerText = desc.Position.y.ToString();
                posYNode.SetValue(desc.Position.y.ToString());
                //XmlElement posZNode = xmlDoc.CreateElement("z");
                XElement posZNode = new XElement("z");
                //posZNode.InnerText = desc.Position.z.ToString();
                posZNode.SetValue(desc.Position.z.ToString());

                //positionNode.AppendChild(posXNode);
                //positionNode.AppendChild(posYNode);
                //positionNode.AppendChild(posZNode);
                positionNode.Add(posXNode);
                positionNode.Add(posYNode);
                positionNode.Add(posZNode);
                //node.AppendChild(positionNode);
                node.Add(positionNode);
                // rotation (Quaternion)
                //XmlElement rotationNode = xmlDoc.CreateElement("rotation");
                //XmlElement rotXNode = xmlDoc.CreateElement("x");
                //rotXNode.InnerText = desc.Rotation.x.ToString();
                //XmlElement rotYNode = xmlDoc.CreateElement("y");
                //rotYNode.InnerText = desc.Rotation.y.ToString();
                //XmlElement rotZNode = xmlDoc.CreateElement("z");
                //rotZNode.InnerText = desc.Rotation.z.ToString();
                //XmlElement rotWNode = xmlDoc.CreateElement("w");
                //rotWNode.InnerText = desc.Rotation.w.ToString();
                XElement rotationNode = new XElement("rotation");
                XElement rotXNode = new XElement("x");
                rotXNode.SetValue(desc.Rotation.x.ToString());
                XElement rotYNode = new XElement("y");
                rotYNode.SetValue(desc.Rotation.y.ToString());
                XElement rotZNode = new XElement("z");
                rotZNode.SetValue(desc.Rotation.z.ToString());
                XElement rotWNode = new XElement("w");
                rotWNode.SetValue(desc.Rotation.w.ToString());

                //rotationNode.AppendChild(rotXNode);
                //rotationNode.AppendChild(rotYNode);
                //rotationNode.AppendChild(rotZNode);
                //rotationNode.AppendChild(rotWNode);
                rotationNode.Add(rotXNode);
                rotationNode.Add(rotYNode);
                rotationNode.Add(rotZNode);
                rotationNode.Add(rotWNode);
                //node.AppendChild(rotationNode);
                node.Add(rotationNode);
                // scale (Vector3)
                //XmlElement scaleNode = xmlDoc.CreateElement("scale");
                //XmlElement sclXNode = xmlDoc.CreateElement("x");
                //sclXNode.InnerText = desc.Scale.x.ToString();
                //XmlElement sclYNode = xmlDoc.CreateElement("y");
                //sclYNode.InnerText = desc.Scale.y.ToString();
                //XmlElement sclZNode = xmlDoc.CreateElement("z");
                //sclZNode.InnerText = desc.Scale.z.ToString();
                XElement scaleNode = new XElement("scale");
                XElement sclXNode = new XElement("x");
                sclXNode.SetValue(desc.Scale.x.ToString());
                XElement sclYNode = new XElement("y");
                sclYNode.SetValue(desc.Scale.y.ToString());
                XElement sclZNode = new XElement("z");
                sclZNode.SetValue(desc.Scale.z.ToString());

                //scaleNode.AppendChild(sclXNode);
                //scaleNode.AppendChild(sclYNode);
                //scaleNode.AppendChild(sclZNode);
                //node.AppendChild(scaleNode);
                scaleNode.Add(sclXNode);
                scaleNode.Add(sclYNode);
                scaleNode.Add(sclZNode);
                node.Add(scaleNode);

                //sceneNode.AppendChild(node);
                sceneNode.Add(node);
            }
            //seanceNode.AppendChild(sceneNode);
            seanceNode.Add(sceneNode);
            return xmlDoc;
        }
        else
        {
            Debug.Log("Can't serialize scene objects since sessionPending == false");
            return null;
        }

    }

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
                    Debug.Log("Incoherence between HoloObjectDescription list and HoloObject list activeObjects.");
                }
            }

        }
    }

    //public Vector3 PositionFromString(string pos)
    //{
    //    // pos :"##.##/##.##/##.##" (x/y/z)
    //    Debug.Log(pos);
    //    string[] posData = pos.Split('/');
    //    float[] xyz = new float[3];
    //    for (int i = 0; i < 3; i++)
    //    {
    //        if (!float.TryParse(posData[i], out xyz[i]))
    //        {
    //            Debug.Log("Can't parse position " + i + " : " + posData[i] + " / Position set to 0");
    //            xyz[i] = 0;
    //        }
    //    }
    //    return new Vector3(xyz[0], xyz[1], xyz[2]);
    //}

    //public Quaternion RotationFromString(string rot)
    //{
    //    return Quaternion.Euler(PositionFromString(rot));
    //}

}
