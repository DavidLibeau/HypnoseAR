using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterController : MonoBehaviour {

    public float speed = 10.0f;
	// Use this for initialization
	void Start () {
        Cursor.lockState = CursorLockMode.Locked;
	}
	
	// Update is called once per frame
	void Update () {
#if WINDOWS_UWP
      
#else
        float translation = Input.GetAxis("Vertical") * speed;
        float straffe = Input.GetAxis("Horizontal") * speed;
        translation *= Time.deltaTime;
        Debug.Log("vertical " + Input.GetAxis("Vertical") + "// translation " + Input.GetAxis("Vertical") * speed + "// speed" + speed);
        straffe *= Time.deltaTime;
        transform.Translate(straffe, 0, translation);
        if (Input.GetKeyDown("escape"))
        {
            Cursor.lockState = CursorLockMode.None;
        }
#endif
    }
}
