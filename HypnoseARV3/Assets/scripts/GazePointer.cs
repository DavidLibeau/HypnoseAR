using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GazePointer : MonoBehaviour
{
    public Color activeColor;
    public static GazePointer instance;
    public PointerEventData pointerData;

    RectTransform indicatorFillRT;
    RawImage indicatorFillRawImage;
    RawImage centerRawImage;

    GameObject lastActivatedTarget;
    GameObject target;

#if WINDOWS_UWP
    private bool mouseInput = false; // to be changed (for tests)
#else
    private bool mouseInput = true; // to be changed (for tests)
#endif
    void Start()
    {
        pointerData = new PointerEventData(EventSystem.current);
        Debug.Log(activeColor.ToString());
        indicatorFillRT = transform.Find("IndicatorFill").GetComponent<RectTransform>();
        indicatorFillRawImage = transform.Find("IndicatorFill").GetComponent<RawImage>();
        indicatorFillRawImage.color = activeColor;
        centerRawImage = transform.Find("Center").GetComponent<RawImage>();
        centerRawImage.color = activeColor;
        instance = this;
        Debug.Log("start");
    }

    // Update is called once per frame
    void Update()
    {
        // Session hasn't started, main panel is active
        if (!MainPanel.Instance.sessionPending)
        {
            if (mouseInput)
            {
                pointerData.position = Input.mousePosition;
                pointerData.button = PointerEventData.InputButton.Left;
            }
            else
            {
                pointerData.button = PointerEventData.InputButton.Left; // trouver l'équivalent sur hololens
                pointerData.position = new Vector2(Screen.width / 2, Screen.height / 2);
            }
            List<RaycastResult> result = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, result);
            if (result.Count > 0)
            {
                indicatorFillRT.localScale = Vector3.one * (1f + 0.2f * Mathf.Cos(5 * Time.time));
                transform.position = result[0].gameObject.transform.position;
            }
            else
            {
                indicatorFillRT.localScale = Vector3.zero;
            }
        }// session is pending, gaze pointer will be used to select objects
        else
        {

        }
    }
}
