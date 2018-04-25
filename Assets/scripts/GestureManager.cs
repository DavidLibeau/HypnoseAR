using UnityEngine;
using UnityEngine.XR.WSA.Input;

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
            NavigationRecognizer.NavigationStarted += NavigationRecognizer_NavigationStarted;
            NavigationRecognizer.NavigationUpdated += NavigationRecognizer_NavigationUpdated;
            NavigationRecognizer.NavigationCompleted += NavigationRecognizer_NavigationCompleted;
            NavigationRecognizer.NavigationCanceled += NavigationRecognizer_NavigationCanceled;

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
            NavigationRecognizer.NavigationStarted -= NavigationRecognizer_NavigationStarted;
            NavigationRecognizer.NavigationUpdated -= NavigationRecognizer_NavigationUpdated;
            NavigationRecognizer.NavigationCompleted -= NavigationRecognizer_NavigationCompleted;
            NavigationRecognizer.NavigationCanceled -= NavigationRecognizer_NavigationCanceled;

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
        private void NavigationRecognizer_NavigationStarted(NavigationStartedEventArgs obj)
        {

            Debug.Log("Navigation started. isSafePlace : "+ MainPanel.Instance.isSafePlace);
            if (MainPanel.Instance.isSafePlace)
            {
                IsNavigating = true;
                NavigationPosition = Vector3.zero;
            
            }
        }

        private void NavigationRecognizer_NavigationUpdated(NavigationUpdatedEventArgs obj)
        {
            if (MainPanel.Instance.isSafePlace)
            {
                IsNavigating = true;
                // juste avant arrière pour la main et rotation de la tête pour la direction.
                NavigationPosition += obj.normalizedOffset;
                camMouseLook.Instance.PerformNavigation(NavigationPosition);
                Debug.Log("Navigation updated");
            }
        }

        private void NavigationRecognizer_NavigationCompleted(NavigationCompletedEventArgs obj)
        {
            if (MainPanel.Instance.isSafePlace)
            {
                IsNavigating = false;
                Debug.Log("Navigation completed");
            }
        }

        private void NavigationRecognizer_NavigationCanceled(NavigationCanceledEventArgs obj)
        {
            if (MainPanel.Instance.isSafePlace)
            {
                IsNavigating = false;
                Debug.Log("Navigation canceled");
            }
        }

        //private void NavigationRecognizer_Tapped(TappedEventArgs obj)
        //{
        //    GameObject focusedObject = GazePointer.Instance.Target;
        //    if (focusedObject != null)
        //    {
        //        focusedObject.SendMessageUpwards("OnSelect");
        //    }
        //}

        // Manipulation events
        private void ManipulationRecognizer_ManipulationStarted(ManipulationStartedEventArgs obj)
        {
            if (HandsManager.Instance.SelectedGameOjbect != null && !MainPanel.Instance.isSafePlace)
            {
                IsManipulating = true;
                ManipulationPosition = Vector3.zero;
                Debug.Log("Manipulation started. Ojbect : " + HandsManager.Instance.SelectedGameOjbect.name);
                //HandsManager.Instance.FocusedGameObject.SendMessageUpwards("OnSelect", SendMessageOptions.DontRequireReceiver);
                HandsManager.Instance.SelectedGameOjbect.SendMessageUpwards("PerformManipulationStart", ManipulationPosition);
            }
        }

        private void ManipulationRecognizer_ManipulationUpdated(ManipulationUpdatedEventArgs obj)
        {
            if (HandsManager.Instance.SelectedGameOjbect != null && !MainPanel.Instance.isSafePlace)
            {
                IsManipulating = true;
                ManipulationPosition = obj.cumulativeDelta - ManipulationPosition;
                Debug.Log("ManipulationPosition : " + ManipulationPosition);
                HandsManager.Instance.SelectedGameOjbect.SendMessageUpwards("PerformManipulationUpdate", ManipulationPosition);
                SessionManager.Instance.OnObjectChange();
                //Debug.Log("OnObjectChange");
                Debug.Log("Manipulation update Ojbect : " + HandsManager.Instance.SelectedGameOjbect.name);
            }
        }

        private void ManipulationRecognizer_ManipulationCompleted(ManipulationCompletedEventArgs obj)
        {
            if (HandsManager.Instance.SelectedGameOjbect != null && !MainPanel.Instance.isSafePlace)
            {
                HandsManager.Instance.SelectedGameOjbect.SendMessageUpwards("PerformManipulationComplete", ManipulationPosition);
                // HandsManager.Instance.FocusedGameObject.SendMessageUpwards("OnDeselect", SendMessageOptions.DontRequireReceiver);
                Debug.Log("Manipulation completed");
            }
            
            IsManipulating = false;
        }

        private void ManipulationRecognizer_ManipulationCanceled(ManipulationCanceledEventArgs obj)
        {
            if (HandsManager.Instance.SelectedGameOjbect != null && !MainPanel.Instance.isSafePlace)
            {
                HandsManager.Instance.SelectedGameOjbect.SendMessageUpwards("PerformManipulationCancel", ManipulationPosition);
                Debug.Log("Manipulation canceled");
            }
           // HandsManager.Instance.FocusedGameObject.SendMessageUpwards("OnDeselect", SendMessageOptions.DontRequireReceiver);
            
            IsManipulating = false;
        }
    }