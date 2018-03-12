using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TherapeuteSession : MonoBehaviour {

    private bool sessionStatus = false;
    private float time = 0;
    public Text timer;

    public static TherapeuteSession instance;

    //to be removed
    public InputField Msg;

    // Use this for initialization
    private void Start()
    {
        instance = this;
    }

    public void SendOrder()
    {
        if (Msg.text != "")
        {
            TherapeuteMain.instance.SendMsg(Msg.text);
        }
        else
        {
            Debug.Log("Nothing to send");
        }
    }

    void StartSession () {
        sessionStatus = true;
	}
	
	// Update is called once per frame
	void Update () {
        //update timer
        switch (sessionStatus)
        {
            case false: // paused
                //gameObject.GetComponent("GameStage").gameObject.SetActive(false);
                break;
            case true: // playing 
                time += Time.deltaTime;
                TimeSpan t = TimeSpan.FromSeconds(time);
                timer.text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);
                break;
        }
    }

    public void PlayPause()
    {
        sessionStatus = !sessionStatus;

    }
}
