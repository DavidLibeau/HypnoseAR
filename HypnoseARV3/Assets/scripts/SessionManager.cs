using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

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

    public HoloObject(HoloObjectDescription desc)
    {
        selfId = desc.SelfId;
        sceneId = desc.SceneId;
        prefabType = "undefined"; // to be changed
        obj = GameObject.CreatePrimitive(PrimitiveType.Cube); // to be changed
       // obj = CreateObjectWithPrefab(desc.SelfId);
        obj.transform.position = desc.Position;
        obj.transform.rotation = desc.Rotation;
        obj.transform.localScale = desc.Scale;
        this.DealWithOtherParameters(desc.OtherContent);
    }

    public GameObject CreateObjectWithPrefab(int prefabId)
    {
        GameObject newObject = Resources.Load(SessionManager.Instance.LocalPath + prefabId + ".prefab") as GameObject;
        return newObject;
    }

    public void DealWithOtherParameters(XmlNode otherContent)
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
}

public class HoloObjectDescription
{
    private int selfId;
    private string sceneId;
    private Vector3 position;
    private Quaternion rotation;
    private Vector3 scale;
    private XmlDocument otherContent; // to be parsed upon creation of the holoObject


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
    public XmlDocument OtherContent
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

    override
    public string ToString()
    {
        return "selfId : " + SelfId + " / sceneID : " + SceneId + " / transform : ";
    }
}

public class Seance
{

    private string seanceId;
    private DateTime seanceCreationDate;
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
    public DateTime SeanceCreationDate
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
        return seanceId + " | " + seanceCreationDate.ToShortDateString() + " | " + ActiveObjectsToString();
    }

    public string ActiveObjectsToString()
    {
        string output = "[";
        foreach (HoloObjectDescription h in activeObjects)
        {
            output += h.ToString() + "|";
        }
        output = output.Remove(output.Length-1) + "]";
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

public class SessionManager : MonoBehaviour {

    public static SessionManager Instance;
    public Seance currentSeance;
    private List<HoloObject> activeObjects;
    private string localPath;
    private int nextId = 0;
    bool newSessionStatus; // true if pending / false else

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
    void Start () {
        Instance = this;
        LocalPath = ""; // path where library is stored on the hololens
        activeObjects = new List<HoloObject>();
        currentSeance = new Seance
        {
            ActiveObjects = new List<HoloObjectDescription>()
        };
	}

    public void ParseXML(string getContent)
    {
        // Creating equivalent representation of the scene
        XmlDocument seanceXml = new XmlDocument();
        seanceXml.LoadXml(getContent);
        Debug.Log(getContent);
        // pending
        bool.TryParse(seanceXml.SelectSingleNode("seance").Attributes["isStarted"].Value, out newSessionStatus);
        if (MainPanel.Instance.sessionPending != newSessionStatus && newSessionStatus == true) // beginning session
        {
            MainPanel.Instance.FadeOutPanel(MainPanel.Instance.mainPanel);
        }
        if (MainPanel.Instance.sessionPending != newSessionStatus && newSessionStatus == false) // ending session
        {
            MainPanel.Instance.FadeInPanel(MainPanel.Instance.mainPanel);
        }
        MainPanel.Instance.sessionPending = newSessionStatus;
        // id
        currentSeance.SeanceId = seanceXml.SelectSingleNode("seance/id").InnerText;

        // create_date
        double secs;
        double.TryParse(seanceXml.SelectSingleNode("seance/date_create").InnerText, out secs);
        DateTime orig= new DateTime(1970,1,1,0,0,0);
        currentSeance.SeanceCreationDate = orig.AddSeconds(secs);

        // creating holoobjevctsdescription
        XmlNodeList objects = seanceXml.SelectNodes("seance/scene/object");
        // reset
        currentSeance.ActiveObjects = new List<HoloObjectDescription>();
        foreach (XmlNode node in objects)
        {
            AddHoloObjectDescription(node);
        }
        Debug.Log(currentSeance.ToString());
        ModifyScene();
    }

    public void AddHoloObjectDescription(XmlNode node)
    {
        Debug.Log(node.InnerXml);
        int sfid;
        int.TryParse(node.SelectSingleNode("selfId").InnerText, out sfid);

        // Fields that might not exist
        Vector3 rand = new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(0, 5));
        // position
        Vector3 pos = rand;
        XmlNode posNode = node.SelectSingleNode("position");
        if(posNode != null)
        {
            float x, y, z;
            float.TryParse(posNode.SelectSingleNode("x").InnerText, out x);
            float.TryParse(posNode.SelectSingleNode("y").InnerText, out y);
            float.TryParse(posNode.SelectSingleNode("z").InnerText, out z);
            pos.x = x;
            pos.y = y;
            pos.z = z;
        }
        //rotation
        Vector3 rotVect = rand;
        XmlNode rotNode = node.SelectSingleNode("rotation");
        if (posNode != null)
        {
            float x, y, z;
            float.TryParse(posNode.SelectSingleNode("x").InnerText, out x);
            float.TryParse(posNode.SelectSingleNode("y").InnerText, out y);
            float.TryParse(posNode.SelectSingleNode("z").InnerText, out z);
            rotVect.x = x;
            rotVect.y = y;
            rotVect.z = z;
        }
        Quaternion rot = Quaternion.Euler(rotVect);
        // scale
        Vector3 scale = Vector3.one;
        XmlNode sclNode = node.SelectSingleNode("scale");
        if (posNode != null)
        {
            float x, y, z;
            float.TryParse(posNode.SelectSingleNode("x").InnerText, out x);
            float.TryParse(posNode.SelectSingleNode("y").InnerText, out y);
            float.TryParse(posNode.SelectSingleNode("z").InnerText, out z);
            scale.x = x;
            scale.y = y;
            scale.z = z;
        }

        HoloObjectDescription currentobj = new HoloObjectDescription
        {
            SceneId = node.SelectSingleNode("sceneId").InnerText,
            SelfId = sfid,
            Position = pos,
            Rotation = rot,
            Scale = scale
        };
        currentSeance.ActiveObjects.Add(currentobj);
    }

    public void ModifyScene()
    {
        // creation or modification
        foreach (HoloObjectDescription desc in currentSeance.ActiveObjects)
        {
            HoloObject currentObj = CheckSceneIdInActive(desc.SceneId);
            if (currentObj != null)
            {
                // object is already there, maybe needs to be modified
                Debug.Log("Object found. Modification is possible");
                currentObj.Obj.transform.position = desc.Position;
                currentObj.Obj.transform.rotation = desc.Rotation;
                currentObj.Obj.transform.localScale = desc.Scale;
                currentObj.DealWithOtherParameters(desc.OtherContent);
            }
            else
            {
                // new object
                Debug.Log("Object not found. Creating new object");
                activeObjects.Add(new HoloObject(desc));
            }
        }
        // destuction
        foreach(HoloObject obj in activeObjects)
        {
            HoloObjectDescription currDesc = CheckSceneIdInDesc(obj.SceneId);
            if(currDesc == null)
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
        foreach(HoloObjectDescription desc in currentSeance.ActiveObjects)
        {
            if(sceneId.Equals(desc.SceneId))
            {
                return desc;
            }
        }
        return null;
    }
    // checks if the desc can be linked to an active object
    public HoloObject CheckSceneIdInActive(string sceneId)
    {
        foreach(HoloObject obj in activeObjects)
        {
            if (sceneId.Equals(obj.SceneId))
            {
                return obj;
            }
        }
        return null;
    }


    //public void CreateObj(string[] data)
    //{
    //    //data format prefabtype|(position)##.##/##.##/##.##|(rotation)##.##/##.##/##.##|otherParamName|otherParamValue
    //    int id;
    //    int.TryParse(data[0], out id);
    //    if (!CheckId(id))
    //    {
    //        Debug.Log("The required id is already used by an object");
    //        return;
    //    }
    //    string pos = data[2]; // to be changed
    //    string rot = data[3]; // to be changed
    //    Vector3 position = PositionFromString(pos);
    //    Quaternion rotation = RotationFromString(rot);
    //    string prefabType = data[1]; // = data[?];
    //    GameObject prefab = Resources.Load(localPath + prefabType) as GameObject;
    //    //GameObject obj = Instantiate(prefab, position, rotation) as GameObject;  // to be uncommented
    //    //Destroy(prefab);


    //    // testing
    //    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //    obj.transform.position = position;
    //    obj.transform.rotation = rotation;
    //    //end testing

    //    if(data.Length > 3)
    //    {
    //        // deal with other parameters
    //        Debug.Log("Extra parameters : " + data.ToString());
    //    }

    //    //HoloObject newHO = new HoloObject(id, prefabType, obj);
    //    //activeObjects.Add(newHO);
    //    //Debug.Log("Object created : " + data.ToString());
    //}

    //public void ModifyObj(string[] data)
    //{
    //    // data format id|(new position)##.##/##.##/##.##|(new rotation)##.##/##.##/##.##|otherParamName|otherParamValue
    //    int id;
    //    int.TryParse(data[0], out id);
    //    HoloObject currentObj = FindObjById(id);
    //    if(currentObj != null)
    //    {
    //        Vector3 pos = PositionFromString(data[1]);
    //        Quaternion rot = RotationFromString(data[2]);
    //        currentObj.Obj.transform.position = pos;
    //        currentObj.Obj.transform.rotation = rot;
    //        if (data.Length > 3)
    //        {
    //            // deal with other parameters
    //            Debug.Log("Extra parameters : " + data.ToString());
    //        }
    //        return;
    //    }
    //    return;
    //}

    //public void DeleteObj(string[] data)
    //{
    //    // data format : id
    //    int id;
    //    int.TryParse(data[0], out id);
    //    if (data.Length > 1)
    //    {
    //        Debug.Log("can't delete obj, message issue : " + data.ToString());
    //    }
    //    else
    //    {
    //        HoloObject currObject = FindObjById(id);
    //        if(currObject != null)
    //        {
    //            //activeObjects.Remove(currObject);
    //            //Destroy(currObject.Obj);
    //            return;
    //        }
    //        Debug.Log("The object wasn't found");
    //    }
    //}

    //public HoloObject FindObjById(int id)
    //{
    //foreach(HoloObject h in activeObjects)
    //{
    //    if(h.Id == id)
    //    {
    //        return h;
    //    }
    //}
    //Debug.Log("HoloObject was not found!");
    //return null;
    //}

    public Vector3 PositionFromString(string pos)
    {
        // pos :"##.##/##.##/##.##" (x/y/z)
        Debug.Log(pos);
        string[] posData = pos.Split('/');
        float[] xyz = new float[3];
        for (int i = 0; i < 3; i++)
        {
            if (!float.TryParse(posData[i], out xyz[i]))
            {
                Debug.Log("Can't parse position " + i + " : " + posData[i] + " / Position set to 0");
                xyz[i] = 0;
            }
        }
        return new Vector3(xyz[0], xyz[1], xyz[2]);
    }

    public Quaternion RotationFromString(string rot)
    {
        return Quaternion.Euler(PositionFromString(rot));
    }



    //public bool CheckId(int id)
    //{
    //foreach(HoloObject h in activeObjects)
    //{
    //    if(h.Id == id)
    //    {
    //        return false;
    //    }
    //}
    //return true;
    //}

}
