using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

public class Done_GameController : MonoBehaviour
{
	public GameObject[] hazards;
	public Vector3 spawnValues;
	public int hazardCount; // Number of enemies in a spawn
	public float spawnWait; // How much time between each spawn of enemy or asteroid
	public float startWait; // When the first wave should come
	public float waveWait; // The waiting period until next wave
	public float enemySpeed; // The speed of the enemies

	int baseHazardCount = 10;
	float baseSpawnWait = 0.75f;
	float baseWaveWait = 4;
	float baseScrollSpeed = -0.25f;
	int baseLevel = 1;
	float baseEnemySpeed = -5f;
	int wave = 1;


	public GUIText highscoreText;
	public GUIText scoreText;
	public GUIText restartText;
	public GUIText gameOverText;
	public GUIText emotionText;
	public GUIText levelText;
	
	private bool gameOver;
	private bool restart;
	public int score;
	public static int highscore;
	public static bool emotionModeActivated;
	int previousLevel;
	public int Level;
	public static List<float> LevelValence = new List<float> ();
	int bgScrollInterval = 5;
	public static bool hasJustDied;

	Transform player;
	Listener playerEmotions;
	Done_BGScroller bgScroller;

	Stopwatch stopwatch;
	Stopwatch stopwatchWave;

	void Awake ()
	{
		
	}
	
	void Start ()
	{
		gameOver = false;
		restart = false;
		restartText.text = "";
		gameOverText.text = "";

		Level = Mathf.Max(PlayerPrefs.GetInt ("level", Level), baseLevel);
		previousLevel = Level - 1;

		wave = PlayerPrefs.GetInt ("wave", wave);

		UnityEngine.Debug.Log ("Hash: " + LevelValence.GetHashCode ());

		score = 0;
		highscore = PlayerPrefs.GetInt ("highscore", highscore);
		levelText.text = Level.ToString ();


		UpdateScore ();
		AdjustParameters ();
		StartCoroutine (SpawnWaves ());

		player = GameObject.FindGameObjectWithTag("MainCamera").transform;
		playerEmotions = player.GetComponent<Listener>();

		bgScroller = GameObject.FindObjectOfType<Done_BGScroller>();
		bgScroller.scrollSpeed = Mathf.Min(PlayerPrefs.GetFloat ("scrollSpeed", bgScroller.scrollSpeed), baseScrollSpeed);

		enemySpeed = Mathf.Min(PlayerPrefs.GetFloat ("enemySpeed", enemySpeed), baseEnemySpeed);

		stopwatch = new Stopwatch ();
		stopwatch.Start ();
		stopwatchWave = new Stopwatch ();
		stopwatchWave.Start ();
	}
	
	void Update ()
	{
		if (restart) {
			if (Input.GetKeyDown (KeyCode.R)) {
				SceneManager.LoadScene (Application.loadedLevel);
			}
		}

		if (Input.GetKeyDown (KeyCode.H)) {
			emotionModeActivated = !emotionModeActivated;
			Level = baseLevel;
			bgScroller.scrollSpeed = baseScrollSpeed;
			SceneManager.LoadScene (Application.loadedLevel);
			hasJustDied = false;
			wave = 1;
		}

		if (Input.GetKeyDown (KeyCode.T)) {
			Level = baseLevel;
			bgScroller.scrollSpeed = baseScrollSpeed;
			SceneManager.LoadScene (Application.loadedLevel);
			hasJustDied = false;
			wave = 1;
		}

		levelText.text = Level.ToString ();

		if (stopwatch.Elapsed.TotalSeconds > bgScrollInterval) {
			bgScroller.scrollSpeed += previousLevel < Level ? -0.0001f : 0.0001f;
			stopwatch.Reset ();
			stopwatch.Start ();
		}

		if (stopwatchWave.Elapsed.TotalSeconds > 2) {
			gameOverText.text = "";
			stopwatchWave.Reset ();
		}

		if (playerEmotions != null && emotionModeActivated) {
			emotionText.text = playerEmotions.currentValence.ToString();

			// This ensures that the player is actually looking at the game.
			if (playerEmotions.currentAttention > 70) {
				LevelValence.Add (playerEmotions.currentValence);
			}
		}
	}

	void AdjustParameters () {
		var factor = Level - 1;
		hazardCount = baseHazardCount + factor * 5;
		spawnWait = baseSpawnWait - factor * 0.01f;
		waveWait = baseWaveWait - factor * 0.15f;	
		enemySpeed += previousLevel < Level ? -0.2f : 0.2f;
	}
	
	IEnumerator SpawnWaves ()
	{
		yield return new WaitForSeconds (startWait);
		if (Level == 1) {
			LevelValence.Clear ();
		}
		while (true)
		{
			UnityEngine.Debug.Log ("level count" + LevelValence.Count);

			if (!emotionModeActivated) {
				LevelValence.Clear ();
				var fakeValence = hasJustDied ? -30f : 20f;
				LevelValence.Add (fakeValence);
				if (Level == 1) {
					LevelValence.Clear ();
				}
			}

			if (LevelValence.Count > 0) {
				var avg = LevelValence.Average ();
				UnityEngine.Debug.Log ("level average" + avg);
				UnityEngine.Debug.Log ("hasJustDied " + hasJustDied);
				previousLevel = Level;

				if (hasJustDied) {
					hasJustDied = false;

					if (avg < -20 && Level > 1) {
						Level--;
					}
				} else {
					if (avg > -20) {
						Level++;
					} else if (Level > 1) {
						Level--;
					}
				}
			}

			LevelValence.Clear ();
			AdjustParameters ();
			gameOverText.text = "Wave " + wave + "!";
			stopwatchWave.Start ();
			wave++;

			for (int i = 0; i < hazardCount; i++)
			{
				GameObject hazard = hazards [Random.Range (0, hazards.Length)];
				hazard.GetComponent<Done_Mover> ().speed = enemySpeed;
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				yield return new WaitForSeconds (spawnWait);
			}
			yield return new WaitForSeconds (waveWait);
			
			if (gameOver)
			{
				break;
			}
		}
	}
	
	public void AddScore (int newScoreValue)
	{
		score += newScoreValue;

		if (score > highscore)
			highscore = score;

		UpdateScore ();
	}
	
	void UpdateScore ()
	{
		highscoreText.text = "Highscore: " + highscore;
		scoreText.text = "Score: " + score;

		if (emotionModeActivated == false) {
			highscoreText.fontStyle = FontStyle.Bold;
		}
	}
	
	public void GameOver ()
	{
		gameOverText.text = "Game Over!";
		gameOver = true;
		restartText.text = "Press 'R' for Restart";
		restart = true;
		hasJustDied = true;
	}

	public bool IsPlayerDead ()
	{
		return gameOver;
	}

	public bool IsEmotionModeActivated ()
	{
		return emotionModeActivated;
	}

	void OnDestroy() {
		PlayerPrefs.SetInt ("highscore", highscore);
		PlayerPrefs.SetInt ("level", Level);
		PlayerPrefs.SetInt ("wave", wave);
		PlayerPrefs.SetFloat ("scrollSpeed", bgScroller.scrollSpeed);
		PlayerPrefs.SetFloat ("enemySpeed", enemySpeed);
		PlayerPrefs.Save();
	}
}