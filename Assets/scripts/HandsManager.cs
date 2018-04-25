using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;

//namespace HoloToolkit.Unity
//{
    /// <summary>
    /// HandsManager keeps track of the focused object when a hand is detected.
    /// </summary>
    public class HandsManager : Singleton<HandsManager>
    {
        [Tooltip("Audio clip to play when Finger Pressed.")]
        public AudioClip FingerPressedSound;
        private AudioSource audioSource;

        public Text testIndicator;

        /// <summary>
        /// Tracks the hand detected state.
        /// </summary>
        public bool HandDetected
        {
            get;
            private set;
        }

        // Keeps track of the GameObject that the hand is interacting with.
        public GameObject FocusedGameObject { get; private set; }
        public GameObject PrevFocusedGameOjbect { get; private set; }
        public GameObject SelectedGameOjbect { get; private set; }

        void Awake()
        {
            EnableAudioHapticFeedback();

            // register for all events
            InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
            InteractionManager.InteractionSourcePressed += InteractionManager_InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += InteractionManager_InteractionSourceReleased;
            
            //Initialize FocusedGameObject as null.
            FocusedGameObject = null;
        }

        private void Update()
        {
            if (testIndicator != null)
            {
                if (HandDetected)
                {
                    testIndicator.text = "Hands detected";
                }
                else
                {
                    testIndicator.text = "Nothing";
                }
            }
            FocusedGameObject = GazePointer.Instance.Target;
            if(FocusedGameObject != null)
            {
                Debug.Log(FocusedGameObject);
            }
            UpdateFocusOutline();
        }

        private void EnableAudioHapticFeedback()
        {
            // If this hologram has an audio clip, add an AudioSource with this clip.
            if (FingerPressedSound != null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }

                audioSource.clip = FingerPressedSound;
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1;
                audioSource.dopplerLevel = 0;
            }
        }

        private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs obj)
        {
            HandDetected = true;
            //Debug.Log("Source detected");
        }

        private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs obj)
        {
            HandDetected = false;
            if(SelectedGameOjbect != null)
            {
                SelectedGameOjbect.SendMessage("OnDeselect", SendMessageOptions.DontRequireReceiver);
            }
            FocusedGameObject = null;
            SelectedGameOjbect = null;
            //Debug.Log("Source lost");
        }

        private void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs hand)
        {
            if (GazePointer.Instance.Target != null)
            {
                if (audioSource != null && !audioSource.isPlaying)
                {
                    audioSource.Play();
                }
                FocusedGameObject = GazePointer.Instance.Target;
                Debug.Log(FocusedGameObject);
                SelectedGameOjbect = FocusedGameObject;
                SelectedGameOjbect.SendMessage("OnSelect", SendMessageOptions.DontRequireReceiver);
            }
           // Debug.Log("Source pressed");
        }

        private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs hand)
        {
            if (SelectedGameOjbect != null)
            {
                SelectedGameOjbect.SendMessage("OnDeselect", SendMessageOptions.DontRequireReceiver);
            }
            SelectedGameOjbect = null;
          //  Debug.Log("Source released");
        }

        public void UpdateFocusOutline()
        {
            if (SelectedGameOjbect == null && FocusedGameObject != PrevFocusedGameOjbect)
            {
                if(FocusedGameObject != null)
                {
                    FocusedGameObject.SendMessage("OnEnable", SendMessageOptions.DontRequireReceiver);
                }
                if(PrevFocusedGameOjbect != null)
                {
                    PrevFocusedGameOjbect.SendMessage("OnDisable", SendMessageOptions.DontRequireReceiver);
                }
            }
            //if(SelectedGameOjbect != null && FocusedGameObject == SelectedGameOjbect)
            PrevFocusedGameOjbect = FocusedGameObject;
        }

        void OnDestroy()
        {
            InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;
            InteractionManager.InteractionSourceReleased -= InteractionManager_InteractionSourceReleased;
            InteractionManager.InteractionSourcePressed -= InteractionManager_InteractionSourcePressed;
        }
    }
//}