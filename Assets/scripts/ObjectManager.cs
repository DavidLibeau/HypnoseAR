using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour {

    public bool interactible;
    public bool deformable;
    public float multiplyingFactor = 1.0f;
    public Vector3 rotationVect;

    public void PerformManipulationUpdate(Vector3 manipulationPostion)
    {
        if (interactible)
        {
            this.gameObject.transform.position += multiplyingFactor * manipulationPostion;
            this.gameObject.transform.hasChanged = true;
            //Debug.Log("Perfomring move...");
        }
    }

    private void Update()
    {
        this.gameObject.transform.Rotate(rotationVect);
    }
}
