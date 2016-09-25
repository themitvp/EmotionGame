using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;
using System.Collections;

namespace Affdex
{
    internal class WindowsNativePlatform : NativePlatform
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void ImageResults(IntPtr i);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void FaceResults(Int32 i);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void LogCallback(IntPtr l);
       
        [DllImport("AffdexNativeWrapper")]
        private static extern int registerListeners([MarshalAs(UnmanagedType.FunctionPtr)] ImageResults imageCallback,
            [MarshalAs(UnmanagedType.FunctionPtr)] FaceResults foundCallback, 
            [MarshalAs(UnmanagedType.FunctionPtr)] FaceResults lostCallback);

        [DllImport("AffdexNativeWrapper")]
        private static extern int registerLog([MarshalAs(UnmanagedType.FunctionPtr)] LogCallback log);

        [DllImport("AffdexNativeWrapper")]
        private static extern int processFrame(IntPtr rgba, Int32 w, Int32 h, float timestamp);

        [DllImport("AffdexNativeWrapper")]
        private static extern int start();

        [DllImport("AffdexNativeWrapper")]
        private static extern void release();

        [DllImport("AffdexNativeWrapper")]
        private static extern int stop();

        [DllImport("AffdexNativeWrapper")]
        private static extern void setExpressionState(int expression, int state);

        [DllImport("AffdexNativeWrapper")]
        private static extern void setEmotionState(int emotion, int state);

        [DllImport("AffdexNativeWrapper")]
        private static extern int initialize(int discrete, string affdexDataPath);

        static void onLogCallback(IntPtr logPtr)
        {
            try
            {
                string lInfo = Marshal.PtrToStringAnsi(logPtr);
                Debug.Log("Native: " + lInfo);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + " " + e.StackTrace);
            }
        }

        public override IEnumerator Initialize(Detector detector, int discrete)
        {
            WindowsNativePlatform.detector = detector;

            //load our lib!

            String adP = Application.streamingAssetsPath;
            
            String affdexDataPath = Path.Combine(adP, "affdex-data-2"); // Application.streamingAssetsPath + "/affdex-data";
            //String affdexDataPath = Application.dataPath + "/affdex-data";
            affdexDataPath = affdexDataPath.Replace('/', '\\');
            int status = initialize(discrete, affdexDataPath);
            Debug.Log("Initialized detector: " + status);
             
            FaceResults faceFound = new FaceResults(this.onFaceFound);
            FaceResults faceLost = new FaceResults(this.onFaceLost);
            ImageResults imageResults = new ImageResults(this.onImageResults);

            h1 = GCHandle.Alloc(faceFound, GCHandleType.Pinned);
            h2 = GCHandle.Alloc(faceLost, GCHandleType.Pinned);
            h3 = GCHandle.Alloc(imageResults, GCHandleType.Pinned);

            status = registerListeners(imageResults, faceFound, faceLost);
            Debug.Log("Registered listeners: " + status);
            yield break;
        }

        public override void ProcessFrame(byte[] rgba, int w, int h, Frame.Orientation orientation, float timestamp)
        {
            try
            {
                IntPtr addr = Marshal.AllocHGlobal(rgba.Length);

                Marshal.Copy(rgba, 0, addr, rgba.Length);
                processFrame(addr, w, h, Time.realtimeSinceStartup);
                
                Marshal.FreeHGlobal(addr);
                addr = IntPtr.Zero;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + " " + e.StackTrace);
            }
		}

        public override void SetExpressionState(int expression, bool state)
        {
            int intState = (state) ? 1 : 0;
            setExpressionState(expression, intState);
           // Debug.Log("Expression " + expression + " set to " + state);
        }

        public override void SetEmotionState(int emotion, bool state)
        {
            int intState = (state) ? 1 : 0;
            setEmotionState(emotion, intState);
          //  Debug.Log("Emotion " + emotion + " set to " + state);
        }

        public override int StartDetector()
        {
            return start();
        }

        public override void StopDetector()
        {
            stop();
        }

        public override void Release()
        {
            release();
            h1.Free();
            h2.Free();
            h3.Free();
        }
    }
}
