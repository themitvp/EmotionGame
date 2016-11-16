using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Affdex;
using System;

public class Listener : ImageResultsListener
{ 
	public float currentInterocularDistance;
	public float currentContempt;
	public float currentValence;
	public float currentAnger;
	public float currentEngagement;
	public float currentDisgust;
	public float currentJoy;
	public float currentSurprise;

	public float currentSmile;
	public float currentAttention;
	public FileOutput fileOutput;

	public FeaturePoint[] featurePointsList;

    public override void onFaceFound(float timestamp, int faceId)
    {
		Debug.Log("Found the face at: " + timestamp);
    }

    public override void onFaceLost(float timestamp, int faceId)
    {
		Debug.Log("lost the face at: " + timestamp);
    }
    
    public override void onImageResults(Dictionary<int, Face> faces)
    {
        Debug.Log("Got results.  Face count=" + faces.Count);

		var game = GameObject.FindObjectOfType<Done_GameController>();
		var player = GameObject.FindGameObjectWithTag("Player");

		if (faces.Count > 0 && game.HasStartedGame())
        {
            var isPlayerDead = game.IsPlayerDead() ? 1 : 0;
			var isEmotionModeActivated = game.IsEmotionModeActivated() ? 1 : 0;

			foreach (KeyValuePair<int, Face> pair in faces) {
				int FaceId = pair.Key;  // The Face Unique Id.
				Face face = pair.Value;    // Instance of the face class containing emotions, and facial expression values.

				//Retrieve the Emotions Scores
				face.Emotions.TryGetValue (Emotions.Contempt, out currentContempt);
				face.Emotions.TryGetValue (Emotions.Valence, out currentValence);
				face.Emotions.TryGetValue (Emotions.Anger, out currentAnger);
				face.Emotions.TryGetValue (Emotions.Engagement, out currentEngagement);
				face.Emotions.TryGetValue (Emotions.Disgust, out currentDisgust);
				face.Emotions.TryGetValue (Emotions.Joy, out currentJoy);
				face.Emotions.TryGetValue (Emotions.Surprise, out currentSurprise);

				//Retrieve the Smile Score
				face.Expressions.TryGetValue (Expressions.Smile, out currentSmile);
				face.Expressions.TryGetValue (Expressions.Attention, out currentAttention);

				//Retrieve the Interocular distance, the distance between two outer eye corners.
				currentInterocularDistance = face.Measurements.interOcularDistance;

				var enemies = GameObject.FindGameObjectsWithTag ("Enemy");

				fileOutput.LogFace (face, enemies.Length, isPlayerDead, game.Level, game.hazardCount, game.spawnWait, game.waveWait, isEmotionModeActivated, game.score);

				//Retrieve the coordinates of the facial landmarks (face feature points)
				featurePointsList = face.FeaturePoints;
			}
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