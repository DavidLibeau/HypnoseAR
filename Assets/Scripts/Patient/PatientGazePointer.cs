using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PatientGazePointer : MonoBehaviour
{

    //[SerializeField]
    public float loadingTime;
    //[SerializeField]
    public float sliderIncrement;
    //[SerializeField]
    public Color activeColor;
    //[SerializeField]
    public AnimationCurve curve;
    public static PatientGazePointer instance;

    float endFocusTime;
    float progress;
    public PointerEventData pointerData;

    RectTransform indicatorFillRT;
    RawImage indicatorFillRawImage;
    RawImage centerRawImage;

    GameObject lastActivatedTarget;
    GameObject target;

    private bool mouseInput = true; // to be changed (for tests)
                                     // Use this for initialization
    void Start()
    {
        pointerData = new PointerEventData(EventSystem.current);

        indicatorFillRT = transform.Find("IndicatorFill").GetComponent<RectTransform>();
        indicatorFillRawImage = transform.Find("IndicatorFill").GetComponent<RawImage>();
        centerRawImage = transform.Find("Center").GetComponent<RawImage>();

        gameObject.SetActive(UnityEngine.XR.XRSettings.enabled);
        instance = this;
        endFocusTime = Time.time + loadingTime;
        //this.enabled = true;
        Debug.Log("start");
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseInput)
        {
            // rayDirection = Camera.main.ScreenPointToRay(Input.mousePosition);
            pointerData.position = Input.mousePosition;
            pointerData.button = PointerEventData.InputButton.Left;
            //indicatorFillRT.position = new Vector3(pointerData.position.x, pointerData.position.y,0);
        }
        else
        {
            //var headPosition = Camera.main.transform.position;
            //var gazeDirection = Camera.main.transform.forward;
            //rayDirection = new Ray(headPosition, gazeDirection);
            pointerData.button = PointerEventData.InputButton.Left; // trouver l'équivalent sur hololens
            pointerData.position = new Vector2(Screen.width / 2, Screen.height / 2);
        }
        //RaycastHit target;
        //if (Physics.Raycast(rayDirection, out target, raycastLayer))
        //{
        //    meshRend.enabled = true;
        //    //Debug.Log("meshrend true");
        //    transform.position = target.point;
        //    transform.rotation = Quaternion.FromToRotation(Vector3.up, target.normal);
        //}
        //else
        //{
        //    //Debug.Log("meshrend false" + rayDirection.ToString());
        //    meshRend.enabled = true;
        //}

        List<RaycastResult> result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, result);
        if (result.Count > 0)
        {
            this.enabled = true;
            //Debug.Log("true");
            indicatorFillRT.localScale = Vector3.one * (1f+0.2f*Mathf.Cos(5*Time.time));
            transform.position = result[0].gameObject.transform.position;
        }
        else
        {
            //Debug.Log("false");
            indicatorFillRT.localScale = Vector3.zero;
        }
    }
}
