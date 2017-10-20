using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    // Singleton
    public static MainMenu current;

    // The GridSize of a new game
    public int GridSize = 5;
    // The Text Object for the Grid Size Display
    public Text GridSizeDisplay;

    public int MissingPercent = 20;
    public Text MissingPercentDisplay;

    public int Difficulty = 20;
    public Text DifficultyDisplay;

	// Use this for initialization
	void Start () {
        ToggleShowAIMovement(false);
        MissingPercent = 20;
        GridSize = 5;
        Difficulty = 20;
	}
	
	// Update is called once per frame
	void Update () {

        CheckInput();

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            UpdateGridSizeDisplay();
            UpdateMissingPercentDisplay();
            UpdateDifficultyDisplay();
        }
        if(SceneManager.GetActiveScene().buildIndex == 3)
        {
            UpdateGridSizeDisplay();
            UpdateMissingPercentDisplay();
        }
        SyncSettings();
	}

    void CheckInput()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Debug.Log("Return to Previous Screen!");

            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                SceneManager.LoadScene(0);
            }
            else if (SceneManager.GetActiveScene().buildIndex == 3)
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    // Changes to the Start New Singleplayer Game Menu
    public void ShowSingleplayerMenu()
    {
        Debug.Log("Load Scene 1! Should load the Singleplayer Menu!");
        SceneManager.LoadScene(1);
    }

    public void ShowVersusMenu()
    {
        Debug.Log("Load Scene 3! Should load the Versus Menu!");
        Settings.p1IsAI = false;
        Settings.p2IsAI = false;
        SceneManager.LoadScene(3);
    }

    // Starts a new Game
    public void StartNewGame()
    {
        Debug.Log("Load Scene 2! Should load New Game!");
        SceneManager.LoadScene(2);
    }

    // Changes the Grid Size to the value of the slider
    public void ChangeGridSize(float value)
    {
        GridSize = (int)value;
    }
    
    // Changes the Missing Percent value to the value of the slider
    public void ChangeMissingPercent(float value)
    {
        MissingPercent = (int)value;
    }

    // Changes the GridSize Display according to the value
    private void UpdateGridSizeDisplay()
    {
        GridSizeDisplay.text = "GRID SIZE: " + Settings.GridSize;
    }

    // Changes the Missing Percent Display according to the value
    private void UpdateMissingPercentDisplay()
    {
        MissingPercentDisplay.text = "MISSING: " + Settings.MissingPercent;
    }

    public void ChangeDifficulty(float value)
    {
        Difficulty = (int)value;
    }

    private void UpdateDifficultyDisplay()
    {
        DifficultyDisplay.text = "DIFFICULTY: " + Settings.AIDifficulty;
    }

    // Syncs the Settings of the Menu with the actual Settings values
    private void SyncSettings()
    {
        Settings.GridSize = GridSize;
        Settings.MissingPercent = MissingPercent;
        Settings.AIDifficulty = Difficulty;
    }

    public void ToggleShowAIMovement(bool value)
    {
        Settings.ShowAIMovement = value;
    }
}
