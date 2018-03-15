using UnityEngine;
#if WINDOWS_UWP
using UnityEngine.XR.WSA.Input;
#endif

namespace Academy.HoloToolkit.Unity
{
    /// <summary>
    /// GestureManager contains event handlers for subscribed gestures.
    /// </summary>
    public class GestureManager : MonoBehaviour
    {
#if WINDOWS_UWP
        private UnityEngine.XR.WSA.Input.GestureRecognizer gestureRecognizer;

        void Start()
        {
            gestureRecognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
            gestureRecognizer.SetRecognizableGestures(UnityEngine.XR.WSA.Input.GestureSettings.Tap);

            gestureRecognizer.Tapped += (args) =>
            {
                GameObject focusedObject = InteractibleManager.Instance.FocusedGameObject;

                if (focusedObject != null)
                {
                    focusedObject.SendMessage("OnSelect");
                }
            };

            gestureRecognizer.StartCapturingGestures();
        }

        void OnDestroy()
        {
            gestureRecognizer.StopCapturingGestures();

        }
#endif
    }
}