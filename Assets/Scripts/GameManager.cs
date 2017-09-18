﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    // Singleton
    public static GameManager current;

    // All Possible Teams
    public enum Teams { Null, Gold, Blue, Red };

    // Currently Active Team
    [SerializeField]
    public Teams activeTeam = Teams.Gold;

    // Total Game Rounds
    public int rounds;

    // GameObject Prefab for Player
    public GameObject playerPrefab;
    // List with all players
    public List<Player> players;
    public Player activePlayer
    {
        get
        {
            return players[(int)activeTeam-1];
        }
    }

    // Can the player do something?
    public bool playerControl;

    // NextTeamButton
    public GameObject nextTeamButton;

    // Next Team Screen
    public GameObject nextTeamScreen;
    private Text nextTeamText;

    // The text that appears when one person was defeated
    public GameObject endScreen;
    private Text endScreenText;

	// Use this for initialization
	void Start () {
        current = this;

        endScreenText = endScreen.GetComponent<Text>();
        nextTeamText = nextTeamScreen.GetComponent<Text>();

        InitializeGame();
	}
	
	// Update is called once per frame
	void Update () {
        nextTeamButton.SetActive(playerControl);
	}

    // Initializes The Game
    public void InitializeGame()
    {
        rounds = 0;

        while(players.Count > 0)
        {
            DestroyImmediate(players[0].gameObject);
            players.RemoveAt(0);
        }

        Player newPlayer = Instantiate(playerPrefab, transform).GetComponent<Player>();
        newPlayer.ID = players.Count + 1;
        players.Add(newPlayer);
        newPlayer = Instantiate(playerPrefab, transform).GetComponent<Player>();
        newPlayer.ID = players.Count + 1;
        players.Add(newPlayer);
    }

    // Adds the tile to the player and removes it from other players
    public void AddTileToPlayer(HexTile tile)
    {
        foreach (Player player in players)
        {
            player.tiles.Remove(tile);
        }

        players[(int)tile.team - 1].tiles.Add(tile);
    }

    // Resets the tile and removes it from all players
    public void ResetTile(HexTile tile)
    {
        foreach (Player player in players)
        {
            player.tiles.Remove(tile);
        }

        // Reset Team to Null
        tile.team = Teams.Null;
    }

    // Switches to the next Team
    public void NextTeam()
    {
        GridManager.current.FinishAttackForAllTiles(true);
        
        // Save Camera Position
        if (rounds >= 1)
        {
            activePlayer.lastCameraPos = CameraBehaviour.current.gameObject.transform.position;
        }

        // Cycle Through Teams
        activeTeam++;

        if ((int)activeTeam > players.Count)
        {
            current.activeTeam = Teams.Gold;
            rounds++;
        }

        activePlayer.FinishTurn();

        StartCoroutine(PlayerChange());
    }

    public IEnumerator PlayerChange()
    {
        playerControl = false;

        nextTeamText.text = activeTeam.ToString().ToUpper() + " IS NEXT";
        nextTeamScreen.SetActive(true);

        yield return new WaitForSeconds(1);

        yield return new WaitUntil(() => Input.touchCount > 0);

        nextTeamScreen.SetActive(false);
        playerControl = true;

        StartCoroutine(CameraBehaviour.current.MoveCameraToLast(activePlayer.lastCameraPos));
    }

    // Show the End Screens
    public IEnumerator Defeat(Player defeatedPlayer)
    {
        endScreenText.text = defeatedPlayer.team.ToString().ToUpper() + " WAS DEFEATED";

        endScreenText.gameObject.SetActive(true);

        GridManager.current.DisableHTDT();

        yield return null;

        yield return new WaitUntil(() => Input.touchCount == 0);

        yield return new WaitUntil(() => Input.anyKeyDown);

        endScreenText.gameObject.SetActive(false);

        SceneManager.LoadScene(0);

    }
}
