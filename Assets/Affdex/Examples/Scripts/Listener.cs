﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Affdex;

public class Listener : ImageResultsListener
{ 

    public Text textArea;
    public override void onFaceFound(float timestamp, int faceId)
    {
        Debug.Log("Found the face");
    }

    public override void onFaceLost(float timestamp, int faceId)
    {
        Debug.Log("lost the face");
    }
    
    public override void onImageResults(Dictionary<int, Face> faces)
    {
        Debug.Log("Got results.  Face count=" + faces.Count);
        if (faces.Count > 0)
        {
            DebugFeatureViewer dfv = GameObject.FindObjectOfType<DebugFeatureViewer>();

            if (dfv != null)
                dfv.ShowFace(faces[0]);

            // make the font bigger on Android, otherwise it's too small to read
            if (Application.platform == RuntimePlatform.Android)
                textArea.fontSize = 36;

            textArea.text = faces[0].ToString();
            textArea.CrossFadeColor(Color.white, 0.2f, true, false);
        }
        else
        {
            textArea.CrossFadeColor(new Color(1, 0.7f, 0.7f), 0.2f, true, false);
        }
    }

    
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}