﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Affdex;
using System;

public class Listener : ImageResultsListener
{ 

    public Text textArea;

	public float currentInterocularDistance;
	public float currentContempt;
	public float currentValence;
	public float currentAnger;
	public float currentFear;
	public float currentEngagement;
	public float currentDisgust;
	public float currentJoy;
	public float currentSadness;
	public float currentSurprise;

	public float currentSmile;
	public float currentAttention;
	public float currentBrowFurrow;
	public float currentBrowRaise;
	public float currentChinRaise;
	public float currentEyeClosure;
	public float currentInnerBrowRaise;
	public float currentLipCornerDepressor;
	//public float currentBrowFurrow;
	//public float currentBrowFurrow;
	public FileOutput fileOutput;

	public FeaturePoint[] featurePointsList;

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

            //textArea.text = faces[0].ToString();
            textArea.CrossFadeColor(Color.white, 0.2f, true, false);

			foreach (KeyValuePair<int, Face> pair in faces) {
				int FaceId = pair.Key;  // The Face Unique Id.
				Face face = pair.Value;    // Instance of the face class containing emotions, and facial expression values.

				//Retrieve the Emotions Scores
				face.Emotions.TryGetValue (Emotions.Contempt, out currentContempt);
				face.Emotions.TryGetValue (Emotions.Valence, out currentValence);
				face.Emotions.TryGetValue (Emotions.Anger, out currentAnger);
				face.Emotions.TryGetValue (Emotions.Fear, out currentFear);
				face.Emotions.TryGetValue (Emotions.Engagement, out currentEngagement);
				face.Emotions.TryGetValue (Emotions.Disgust, out currentDisgust);
				face.Emotions.TryGetValue (Emotions.Joy, out currentJoy);
				face.Emotions.TryGetValue (Emotions.Sadness, out currentSadness);
				face.Emotions.TryGetValue (Emotions.Surprise, out currentSurprise);

				//Retrieve the Smile Score
				face.Expressions.TryGetValue (Expressions.Smile, out currentSmile);
				face.Expressions.TryGetValue (Expressions.Attention, out currentAttention);
				face.Expressions.TryGetValue (Expressions.BrowFurrow, out currentBrowFurrow);
				face.Expressions.TryGetValue (Expressions.BrowRaise, out currentBrowRaise);
				face.Expressions.TryGetValue (Expressions.ChinRaise, out currentChinRaise);
				face.Expressions.TryGetValue (Expressions.EyeClosure, out currentEyeClosure);
				face.Expressions.TryGetValue (Expressions.InnerBrowRaise, out currentInnerBrowRaise);
				face.Expressions.TryGetValue (Expressions.LipCornerDepressor, out currentLipCornerDepressor);
				/*face.Expressions.TryGetValue (Expressions.LipPress, out currentAttention);
				face.Expressions.TryGetValue (Expressions.LipPucker, out currentAttention);
				face.Expressions.TryGetValue (Expressions.LipCornerDepressor, out currentAttention);
				face.Expressions.TryGetValue (Expressions.LipCornerDepressor, out currentAttention);
				face.Expressions.TryGetValue (Expressions.LipCornerDepressor, out currentAttention);
				face.Expressions.TryGetValue (Expressions.LipCornerDepressor, out currentAttention);*/




				//Retrieve the Interocular distance, the distance between two outer eye corners.
				currentInterocularDistance = face.Measurements.interOcularDistance;

				fileOutput.LogFace (face);
				//Retrieve the coordinates of the facial landmarks (face feature points)
				featurePointsList = face.FeaturePoints;
			}
        }
        else
        {
            textArea.CrossFadeColor(new Color(1, 0.7f, 0.7f), 0.2f, true, false);
			//fileOutput.Close ();
        }
    }

    
    // Use this for initialization
    void Start () {
		Debug.Log("Started");
		fileOutput = new FileOutput ("output-" + System.DateTime.Now.Ticks + ".csv");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}