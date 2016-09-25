// Unity derives Camera Input Component UI from this file
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Affdex
{
    /// <summary>
    /// Provides WebCam access to the detector.  Sample rate set per second.  Use
    /// </summary>
    [RequireComponent(typeof(Detector))]
    public class CameraInput : MonoBehaviour, IDetectorInput
    {
        /// <summary>
        /// Number of frames per second to sample.  Use 0 and call ProcessFrame() manually to run manually.
        /// Enable/Disable to start/stop the sampling
        /// </summary>
        public float sampleRate = 20;

        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_ANDROID
        /// <summary>
        /// List of WebCams accessible to Unity
        /// </summary>
        [HideInInspector]
        protected WebCamDevice[] devices;

        /// <summary>
        /// WebCam chosen to gather metrics from
        /// </summary>
        [HideInInspector]
        protected WebCamDevice device;
#endif
        /// <summary>
        /// Should the selected camera be front facing?
        /// </summary>
        public bool isFrontFacing = true;

        /// <summary>
        /// Desired width for capture
        /// </summary>
        public int targetWidth = 640;

        /// <summary>
        /// Desired height for capture
        /// </summary>
        public int targetHeight = 480;

        /// <summary>
        /// Web Cam texture
        /// </summary>
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_ANDROID
        [HideInInspector]
        private WebCamTexture webcamTexture;

        public float videoRotationAngle
        {
            get
            {
                return webcamTexture.videoRotationAngle;
            }
        }
#endif
        /// <summary>
        /// The detector that is on this game object
        /// </summary>
        public Detector detector
        {
            get; private set;
        }

        /// <summary>
        /// The texture that is being modified for processing
        /// </summary>
        public Texture Texture
        {
            get
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_ANDROID
                return webcamTexture;
#else
                return new Texture();
#endif
            }
        }

        void Start()
        {
            if (!AffdexUnityUtils.ValidPlatform())
                return;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_ANDROID
            detector = GetComponent<Detector>();
            devices = WebCamTexture.devices;
            if (devices.Length > 0)
            {
                SelectCamera(isFrontFacing);

                if (device.name != "Null")
                {
                    webcamTexture = new WebCamTexture(device.name, targetWidth, targetHeight, (int)sampleRate);
                    webcamTexture.Play();
                }
            }
#endif
        }

        /// <summary>
        /// Set the target device (by name or orientation)
        /// </summary>
        /// <param name="isFrontFacing">Should the device be forward facing?</param>
        /// <param name="name">The name of the webcam to select.</param>
        public void SelectCamera(bool isFrontFacing, string name = "")
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_ANDROID
            foreach (WebCamDevice d in devices)
            {
                if (d.name.Length > 1 && d.name == name)
                {
                        webcamTexture.Stop();
                        device = d;

                        webcamTexture = new WebCamTexture(device.name, targetWidth, targetHeight, (int)sampleRate);
                        webcamTexture.Play();
                }
                else if (d.isFrontFacing == isFrontFacing)
                {
                    device = d;
                }
            }
#endif
        }

        void OnEnable()
        {
            if (!AffdexUnityUtils.ValidPlatform())
                return;

            //get the selected camera!

            if (sampleRate > 0)
                StartCoroutine(SampleRoutine());
        }

        /// <summary>
        /// Coroutine to sample frames from the camera
        /// </summary>
        /// <returns></returns>
        private IEnumerator SampleRoutine()
        {
            while (enabled)
            {
                yield return new WaitForSeconds(1 / sampleRate);
                ProcessFrame();
            }
        }


        /// <summary>
        /// Sample an individual frame from the webcam and send to detector for processing.
        /// </summary>
        public void ProcessFrame()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_ANDROID
            if (webcamTexture != null)
            {
                if (detector.IsRunning)
                {
                    if (webcamTexture.isPlaying)
                    {

                        Frame.Orientation orientation = Frame.Orientation.Upright;

#if UNITY_ANDROID
                        // account for camera rotation on mobile devices
                        switch(webcamTexture.videoRotationAngle)
                        {
                            case 90:
                                orientation = Frame.Orientation.CW_90;
                                break;
                            case 180:
                                orientation = Frame.Orientation.CW_180;
                                break;
                            case 270:
                                orientation = Frame.Orientation.CW_270;
                                break;
                        }
#endif

                        Frame frame = new Frame(webcamTexture.GetPixels32(), webcamTexture.width, webcamTexture.height, orientation, Time.realtimeSinceStartup);
                        detector.ProcessFrame(frame);
                    }
                }
            }
#endif
        }

        void OnDestroy()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_ANDROID
            if (webcamTexture != null)
            {
                webcamTexture.Stop();
            }
#endif
        }
    }
}
