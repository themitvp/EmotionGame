using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;
using System.Collections;

namespace Affdex
{
    internal class iOSNativePlatform : NativePlatform
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void ImageResults(IntPtr i);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void FaceResults(Int32 i);

        IntPtr cWrapperHandle;

        [DllImport("affdex-native")]
        private static extern int registerListeners( IntPtr handle,
            [MarshalAs(UnmanagedType.FunctionPtr)] ImageResults imageCallback,
            [MarshalAs(UnmanagedType.FunctionPtr)] FaceResults foundCallback, 
            [MarshalAs(UnmanagedType.FunctionPtr)] FaceResults lostCallback);

        [DllImport("affdex-native")]
        private static extern int processFrame(IntPtr handle, IntPtr rgba, Int32 w, Int32 h, Int32 orientation, float timestamp);

        [DllImport("affdex-native")]
        private static extern int start(IntPtr handle);

        [DllImport("affdex-native")]
        private static extern void release(IntPtr handle);

        [DllImport("affdex-native")]
        private static extern int stop(IntPtr handle);

        [DllImport("affdex-native")]
        private static extern void setExpressionState(IntPtr handle, int expression, int state);

        [DllImport("affdex-native")]
        private static extern void setEmotionState(IntPtr handle, int emotion, int state);

        [DllImport("affdex-native")]
        private static extern IntPtr initialize(int discrete, string affdexDataPath);

        public override IEnumerator Initialize(Detector detector, int discrete)
        {
            OSXNativePlatform.detector = detector;
            String adP = Application.streamingAssetsPath;
            String affdexDataPath = Path.Combine(adP, "affdex-data-osx"); 
            int status = 0;

            try 
            {
                cWrapperHandle = initialize(discrete, affdexDataPath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            Debug.Log("Initialized detector: " + status);

            FaceResults faceFound = new FaceResults(this.onFaceFound);
            FaceResults faceLost = new FaceResults(this.onFaceLost);
            ImageResults imageResults = new ImageResults(this.onImageResults);

            h1 = GCHandle.Alloc(faceFound, GCHandleType.Pinned);
            h2 = GCHandle.Alloc(faceLost, GCHandleType.Pinned);
            h3 = GCHandle.Alloc(imageResults, GCHandleType.Pinned);

            status = registerListeners(cWrapperHandle, imageResults, faceFound, faceLost);
            Debug.Log("Registered listeners: " + status);
            yield break;
        }

        public override void ProcessFrame(byte[] rgba, int w, int h, Frame.Orientation orientation, float timestamp)
        {
            try
            {
                IntPtr addr = Marshal.AllocHGlobal(rgba.Length);

                Marshal.Copy(rgba, 0, addr, rgba.Length);

                processFrame(cWrapperHandle, addr, w, h, (int)orientation, Time.realtimeSinceStartup);

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
            setExpressionState(cWrapperHandle, expression, intState);
            // Debug.Log("Expression " + expression + " set to " + state);
        }

        public override void SetEmotionState(int emotion, bool state)
        {
            int intState = (state) ? 1 : 0;
            setEmotionState(cWrapperHandle, emotion, intState);
            //  Debug.Log("Emotion " + emotion + " set to " + state);
        }

        public override int StartDetector()
        {
            return start(cWrapperHandle);
        }


        public override void StopDetector()
        {
            stop(cWrapperHandle);
        }

        public override void Release()
        {
            release(cWrapperHandle);
            h1.Free();
            h2.Free();
            h3.Free();
        }
    }
}

