using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour {

    public bool interactible;
    public float multiplyingFactor = 1.0f;

    public void PerformManipulationUpdate(Vector3 manipulationPostion)
    {
        this.gameObject.transform.position += multiplyingFactor * manipulationPostion;
        this.gameObject.transform.hasChanged = true;
        //Debug.Log("Perfomring move...");
    }
}
