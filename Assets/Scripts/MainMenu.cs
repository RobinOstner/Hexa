using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    // Singleton
    public static MainMenu current;

    // The GridSize of a new game
    public int GridSize = 3;
    // The Text Object for the Grid Size Display
    public Text GridSizeDisplay;

    public int MissingPercent = 0;
    public Text MissingPercentDisplay;

	// Use this for initialization
	void Start () {
        ToggleShowAIMovement(false);
	}
	
	// Update is called once per frame
	void Update () {

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            UpdateGridSizeDisplay();
            UpdateMissingPercentDisplay();
        }
        SyncSettings();
	}

    // Changes to the Start New Game Menu
    public void ShowNewGameMenu()
    {
        Debug.Log("Load Scene 1! Should load the New Game Menu!");
        SceneManager.LoadScene(1);
        GridSize = 2;
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

    // Syncs the Settings of the Menu with the actual Settings values
    private void SyncSettings()
    {
        Settings.GridSize = GridSize;
        Settings.MissingPercent = MissingPercent;
    }

    public void ToggleShowAIMovement(bool value)
    {
        Settings.ShowAIMovement = value;
    }
}
