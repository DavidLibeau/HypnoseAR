using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camMouseLook : MonoBehaviour {

    Vector2 mouseLook;
    Vector2 smoothV;
    float smoothZ;

    public float sensitivity = 5.0f;
    public float smoothing = 2.0f;
    GameObject character;

    public static camMouseLook Instance;
    public RigidbodyConstraints constraints;

	// Use this for initialization
	void Start () {
        character = this.transform.parent.gameObject;
        Instance = this;      
        constraints = this.GetComponent<Rigidbody>().constraints;
        constraints = RigidbodyConstraints.FreezeRotation;
	}
	
	// Update is called once per frame
	void Update () {
#if WINDOWS_UWP
#else
        var md = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        md = Vector2.Scale(md, new Vector2(sensitivity * smoothing, sensitivity * smoothing));
        smoothV.x = Mathf.Lerp(smoothV.x, md.x, 1f / smoothing);
        smoothV.y = Mathf.Lerp(smoothV.y, md.y, 1f / smoothing);
        mouseLook += smoothV;
        transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        character.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, character.transform.up);
#endif
    }

    public void PerformNavigation(Vector3 speedVector)
    {
        //float rotationRate = speedVector.x*sensitivity;
        //character.transform.localRotation *= Quaternion.AngleAxis(rotationRate, Vector3.up); // to be checked in use
        smoothZ = Mathf.Lerp(smoothZ, speedVector.x-smoothZ, 1f/smoothing);
        Debug.Log("Moving : speedVector " + speedVector +" // smoothZ " + smoothZ);
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        character.transform.position -= smoothZ*forward;
        Debug.Log("Character position : " + character.transform.position);
    }
}
