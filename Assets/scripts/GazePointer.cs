using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;
using HoloToolkit.Unity;
/// <summary>
/// Gaze Pointer keeps track of gazing direction and focused objects () // coupled with hands manager, enables interactions with objects
/// </summary>
public class GazePointer : MonoBehaviour
{

    public static GazePointer Instance;

    // UI pointer
    public PointerEventData pointerData;
    public Color activeColor;
    RectTransform indicatorFillRT;
    RawImage indicatorFillRawImage;
    RectTransform centerRT;
    RawImage centerRawImage;
    // raycast targets
    //public GazeStabilizer gazeStabilizer;
    public GameObject Target { get; private set; }
    public GameObject PrevTarget { get; private set; }
    public GameObject SelectedTarget { get; private set; }
    public float MaxGazeDistance = float.MaxValue; //5.0f;

    // scene manipulations
    public GestureRecognizer recognizer;
    public bool objectSelected = false;

#if WINDOWS_UWP
    private bool mouseInput = false;
#else
    private bool mouseInput = true;
#endif
    // Use this for initialization
    void Awake()
    {
        Instance = this;
       // gazeStabilizer = new GazeStabilizer(); // necessary ?
    }

    void Start()
    {
        pointerData = new PointerEventData(EventSystem.current);
        //Debug.Log(activeColor.ToString());
        if (!MainPanel.Instance.isSafePlace)
        {
            indicatorFillRT = transform.Find("IndicatorFill").GetComponent<RectTransform>();
            indicatorFillRawImage = transform.Find("IndicatorFill").GetComponent<RawImage>();
            indicatorFillRawImage.color = activeColor;
            centerRT = transform.Find("Center").GetComponent<RectTransform>();
            centerRawImage = transform.Find("Center").GetComponent<RawImage>();
            centerRawImage.color = activeColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // ** UI ** // Session hasn't started, main panel is active
        if (!MainPanel.Instance.isSafePlace)
        {
            if (!MainPanel.Instance.sessionPending)
            {
                if (mouseInput)
                {
                    pointerData.position = Input.mousePosition;
                    pointerData.button = PointerEventData.InputButton.Left;
                }
                else
                {
                    //pointerData.button = PointerEventData.InputButton.Left; // trouver l'équivalent sur hololens
                    pointerData.position = new Vector2(Screen.width / 2, Screen.height / 2);
                }
                List<RaycastResult> result = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, result);
                if (result.Count > 0)
                {
                    indicatorFillRT.localScale = Vector3.one * (1f + 0.2f * Mathf.Cos(5 * Time.time));
                    transform.position = result[0].gameObject.transform.position;
                    centerRT.localScale = Vector3.one;
                    //centerRawImage.color = activeColor;
                }
                else
                {
                    indicatorFillRT.localScale = Vector3.zero;
                    centerRT.localScale = Vector3.zero;
                    //centerRawImage.color = Color.clear;
                }
            }
            // ** RUNNING MODE ** // session is pending, gaze pointer will be used to select objects
            else
            {
                indicatorFillRT.localScale = Vector3.zero;
                centerRT.localScale = Vector3.zero;
                RaycastHit hitInfo;
                Ray ray;
                if (mouseInput)
                {
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                }
                else
                {
                    Vector3 headPosition = Camera.main.transform.position;
                    Vector3 gazeDirection = Camera.main.transform.forward;
#if WINDOWS_UWP
                    //if(gazeStabilizer != null)
                    //{
                    //    gazeStabilizer.UpdateHeadStability(headPosition, Camera.main.transform.rotation);
                    //    headPosition = gazeStabilizer.StableHeadPosition;
                    //    ray = gazeStabilizer.StableHeadRay;

                    //}
                    //else
                    //{
                    ray = new Ray(headPosition, gazeDirection);
                    //}
#else
                ray = new Ray(headPosition, gazeDirection);
#endif
                }

                if (Physics.Raycast(ray, out hitInfo, MaxGazeDistance))
                {
                    // If the raycast hit a hologram, use that as the focused object.
                    Target = hitInfo.collider.gameObject;
                    //Debug.Log("Raycast hit : " + Target.name);
                    //if (!objectSelected && Target != null && !Target.Equals(PrevTarget)) // && !target.selected !!!!
                    //{
                    //    Target.SendMessage("OnEnable", SendMessageOptions.DontRequireReceiver);
                    //}
                    //if (mouseInput)
                    //{
                    //    if (Input.GetMouseButtonDown(0)) // for test purposes
                    //    {
                    //        TryToSelect();
                    //    }
                    //    if (Input.GetMouseButtonUp(0))
                    //    {

                    //    }
                    //}
                }
                else
                {
                    //if (!objectSelected && PrevTarget != null) // 
                    //{
                    //    PrevTarget.SendMessage("OnDisable", SendMessageOptions.DontRequireReceiver);
                    //}
                    Target = null;
                }

                //if (Target != PrevTarget)
                //{
                //    recognizer.CancelGestures();
                //    recognizer.StartCapturingGestures();
                //}

                //PrevTarget = Target;
            }
        }
    }

    //public void TryToSelect()
    //{
    //    if ((objectSelected && SelectedTarget == Target) || (!objectSelected))
    //    {
    //        objectSelected = !objectSelected;
    //        Debug.Log("Button down. Object selected : " + objectSelected);
    //        Target.SendMessage("OnToggleSelect", SendMessageOptions.DontRequireReceiver);
    //        SelectedTarget = Target;
    //    }
    //}
}
