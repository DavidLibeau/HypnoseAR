using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace HoloToolkit.Unity
{
    public class GestureManager : Singleton<GestureManager>
    {
        // Tap and Navigation gesture recognizer. For the safe place
        public GestureRecognizer NavigationRecognizer { get; private set; }
        public bool IsNavigating { get; private set; }
        public Vector3 NavigationPosition { get; private set; }

        // Manipulation gesture recognizer. For the hologram phase add Tap
        public GestureRecognizer ManipulationRecognizer { get; private set; }
        public bool IsManipulating { get; private set; }
        public Vector3 ManipulationPosition { get; private set; }

        // Currently active gesture recognizer.
        public GestureRecognizer ActiveRecognizer { get; private set; }

        void Awake()
        {
            NavigationRecognizer = new GestureRecognizer();
            NavigationRecognizer.SetRecognizableGestures( GestureSettings.Tap | GestureSettings.NavigationX);
            //NavigationRecognizer.Tapped += NavigationRecognizer_Tapped;
            //NavigationRecognizer.NavigationStarted += NavigationRecognizer_NavigationStarted;
            //NavigationRecognizer.NavigationUpdated += NavigationRecognizer_NavigationUpdated;
            //NavigationRecognizer.NavigationCompleted += NavigationRecognizer_NavigationCompleted;
            //NavigationRecognizer.NavigationCanceled += NavigationRecognizer_NavigationCanceled;

            ManipulationRecognizer = new GestureRecognizer();
            ManipulationRecognizer.SetRecognizableGestures(GestureSettings.ManipulationTranslate);
            ManipulationRecognizer.ManipulationStarted += ManipulationRecognizer_ManipulationStarted;
            ManipulationRecognizer.ManipulationUpdated += ManipulationRecognizer_ManipulationUpdated;
            ManipulationRecognizer.ManipulationCompleted += ManipulationRecognizer_ManipulationCompleted;
            ManipulationRecognizer.ManipulationCanceled += ManipulationRecognizer_ManipulationCanceled;

            //Select the navigation mode / Transition to manipulation
            Transition(ManipulationRecognizer);
        }

        void OnDestroy()
        {
            // Unregister the Tapped and Navigation events on the NavigationRecognizer.
            //NavigationRecognizer.Tapped -= NavigationRecognizer_Tapped;
            //NavigationRecognizer.NavigationStarted -= NavigationRecognizer_NavigationStarted;
            //NavigationRecognizer.NavigationUpdated -= NavigationRecognizer_NavigationUpdated;
            //NavigationRecognizer.NavigationCompleted -= NavigationRecognizer_NavigationCompleted;
            //NavigationRecognizer.NavigationCanceled -= NavigationRecognizer_NavigationCanceled;

            // Unregister the Manipulation events on the ManipulationRecognizer.
            ManipulationRecognizer.ManipulationStarted -= ManipulationRecognizer_ManipulationStarted;
            ManipulationRecognizer.ManipulationUpdated -= ManipulationRecognizer_ManipulationUpdated;
            ManipulationRecognizer.ManipulationCompleted -= ManipulationRecognizer_ManipulationCompleted;
            ManipulationRecognizer.ManipulationCanceled -= ManipulationRecognizer_ManipulationCanceled;
        }

        //public void ResetGestureRecognizers()
        //{
        //    // Default to the navigation gestures.
        //    Transition(NavigationRecognizer);
        //}

        /// <summary>
        /// Transition to a new GestureRecognizer.
        /// </summary>
        /// <param name="newRecognizer">The GestureRecognizer to transition to.</param>
        public void Transition(GestureRecognizer newRecognizer)
        {
            if (newRecognizer == null)
            {
                return;
            }

            if (ActiveRecognizer != null)
            {
                if (ActiveRecognizer == newRecognizer)
                {
                    return;
                }

                ActiveRecognizer.CancelGestures();
                ActiveRecognizer.StopCapturingGestures();
            }
            newRecognizer.StartCapturingGestures();
            ActiveRecognizer = newRecognizer;
        }
        // Navigation events
        //private void NavigationRecognizer_NavigationStarted(NavigationStartedEventArgs obj)
        //{
        //    IsNavigating = true;
        //    NavigationPosition = Vector3.zero;
        //}

        //private void NavigationRecognizer_NavigationUpdated(NavigationUpdatedEventArgs obj)
        //{
        //    IsNavigating = true;
        //    NavigationPosition = obj.normalizedOffset;
        //}

        //private void NavigationRecognizer_NavigationCompleted(NavigationCompletedEventArgs obj)
        //{
        //    IsNavigating = false;
        //}

        //private void NavigationRecognizer_NavigationCanceled(NavigationCanceledEventArgs obj)
        //{
        //    IsNavigating = false;
        //}

        // Manipulation events
        private void ManipulationRecognizer_ManipulationStarted(ManipulationStartedEventArgs obj)
        {
            if (HandsManager.Instance.SelectedGameOjbect != null)
            {
                IsManipulating = true;
                ManipulationPosition = Vector3.zero;
               // Debug.Log("Manipulation started. Ojbect : " + HandsManager.Instance.SelectedGameOjbect.name);
                //HandsManager.Instance.FocusedGameObject.SendMessageUpwards("OnSelect", SendMessageOptions.DontRequireReceiver);
                HandsManager.Instance.SelectedGameOjbect.SendMessageUpwards("PerformManipulationStart", ManipulationPosition);
            }
        }

        private void ManipulationRecognizer_ManipulationUpdated(ManipulationUpdatedEventArgs obj)
        {
            if (HandsManager.Instance.SelectedGameOjbect != null)
            {
                IsManipulating = true;
                ManipulationPosition = obj.cumulativeDelta;
                HandsManager.Instance.SelectedGameOjbect.SendMessageUpwards("PerformManipulationUpdate", ManipulationPosition);
                //SessionManager.Instance.OnObjectChange();
                //Debug.Log("OnObjectChange");
                //Debug.Log("Manipulation update Ojbect : " + HandsManager.Instance.SelectedGameOjbect.name);
            }
        }

        private void ManipulationRecognizer_ManipulationCompleted(ManipulationCompletedEventArgs obj)
        {
            if (HandsManager.Instance.SelectedGameOjbect != null)
            {
                HandsManager.Instance.SelectedGameOjbect.SendMessageUpwards("PerformManipulationComplete", ManipulationPosition);
            }
            //HandsManager.Instance.FocusedGameObject.SendMessageUpwards("OnDeselect", SendMessageOptions.DontRequireReceiver);
            //Debug.Log("Manipulation completed");
            IsManipulating = false;
        }

        private void ManipulationRecognizer_ManipulationCanceled(ManipulationCanceledEventArgs obj)
        {
            if (HandsManager.Instance.SelectedGameOjbect != null)
            {
                HandsManager.Instance.SelectedGameOjbect.SendMessageUpwards("PerformManipulationCancel", ManipulationPosition);
            }
            //HandsManager.Instance.FocusedGameObject.SendMessageUpwards("OnDeselect", SendMessageOptions.DontRequireReceiver);
            //Debug.Log("Manipulation canceled");
            IsManipulating = false;
        }

        //private void NavigationRecognizer_Tapped(TappedEventArgs obj)
        //{
        //    GameObject focusedObject = GazePointer.Instance.Target;
        //    if (focusedObject != null)
        //    {
        //        focusedObject.SendMessageUpwards("OnSelect");
        //    }
        //}
    }
}