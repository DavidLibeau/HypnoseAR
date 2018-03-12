using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PatientObject
{
    private string name;
    private GameObject obj;
    private int id;

    public int Id { get { return id; } set { id = value; } }
    public string Name { get { return name; } set { name = value; } }
    public GameObject Obj { get { return obj; } set { obj = value; } }
        
    public PatientObject(string name, GameObject obj, int id)
    {
        this.name = name;
        this.obj = obj;
        this.id = id;
    }
}


public class PatientManager: MonoBehaviour {

    public List<PatientObject> currObjects;
    public int objCount = 0;
    public static PatientManager instance;


	void Start () {
        currObjects = new List<PatientObject>();// Use this for initialization
        instance = this;
    }

    // decides what to do depending on the message
    public void ParseInstruction(string msg)
    {
        string[] splitMsg = msg.Split('|');
        //clientManager.ModObj();
    }

    int AddObj(string name, Vector3 position, Vector3 scale, Vector3 rotation)
    {
        int id = objCount++;
        PrimitiveType type = PrimitiveType.Cube;// what do i want to instantiate to be changed
        GameObject newObj = GameObject.CreatePrimitive(type);
        newObj.transform.localPosition = position;
        newObj.transform.localRotation = Quaternion.Euler(0f,0f,0f); // to be changed
        PatientObject pObj = new PatientObject(name, newObj, id);
        currObjects.Add(pObj);
        return id;
    }

    void ModObj(int id)
    {

        PatientObject curr = findObj(id);
        if(curr == null)
        {
            Debug.Log("Object not found");
            return;
        }
        //blabla
        //throw new System.Exception("Object was not found or modification was impossible");
    }

    PatientObject findObj(int id)
    {
        foreach(PatientObject obj in currObjects)
        {
            if(obj.Id == id)
            {
                return obj;
            }
        }
        return null;
    }


	// Update is called once per frame
	void Update () {
		
	}
}
