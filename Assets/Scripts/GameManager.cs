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
    private bool finished;
    public GameObject endScreen;
    private Text endScreenText;

    // Text Object showing the Tile Count
    public Text tileCountText;

	// Use this for initialization
	void Start () {
        current = this;

        endScreenText = endScreen.GetComponent<Text>();
        nextTeamText = nextTeamScreen.GetComponent<Text>();

        InitializeGame();
	}
	
	// Update is called once per frame
	void Update () {
        nextTeamButton.SetActive(playerControl && !CameraBehaviour.current.replayMode);
        UpdateUIInfo();
	}

    // Initializes The Game
    public void InitializeGame()
    {
        finished = false;
        rounds = 0;

        while(players.Count > 0)
        {
            DestroyImmediate(players[0].gameObject);
            players.RemoveAt(0);
        }

        Player newPlayer = Instantiate(playerPrefab, transform).GetComponent<Player>();
        newPlayer.ID = players.Count + 1;
        if (!Settings.p1IsAI)
        {
            newPlayer.isAI = false;
        }
        newPlayer.aiComponent.difficulty = Settings.AIDifficulty;
        players.Add(newPlayer);
        newPlayer = Instantiate(playerPrefab, transform).GetComponent<Player>();
        newPlayer.ID = players.Count + 1;
        if (!Settings.p2IsAI)
        {
            newPlayer.isAI = false;
        }
        newPlayer.aiComponent.difficulty = Settings.AIDifficulty;
        players.Add(newPlayer);
    }

    // Adds the tile to the player and removes it from other players
    public void AddTileToPlayer(HexTile tile, Player player)
    {
        foreach (Player otherPlayers in players)
        {
            otherPlayers.tiles.Remove(tile);
        }

        tile.team = player.team;
        player.tiles.Add(tile);
    }

    // Returns the player according to the team
    public Player GetPlayerByTeam(Teams team)
    {
        foreach(Player p in players)
        {
            if (p.team == team)
            {
                return p;
            }
        }

        return null;
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
        tile.SetMoveLocked(false);
        tile.isBaseTile = false;
    }

    // Switches to the next Team
    public void NextTeam()
    {
        GridManager.current.FinishAttackForAllTiles(InputManager.current.movements);
        GridManager.current.FinishAllTiles();

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

        
        if (activePlayer.tiles.Count == 0)
        {
            if (current.rounds > 0)
            {
                activeTeam--;

                if((int)activeTeam <= 0)
                {
                    current.activeTeam = Teams.Blue;
                }

                Debug.Log("Active: " + activePlayer);

                if (!activePlayer.isAI)
                {
                    StartCoroutine(activePlayer.replays.ShowReplay());
                }

                activeTeam++;

                if ((int)activeTeam > players.Count)
                {
                    current.activeTeam = Teams.Gold;
                    rounds++;
                }

                StartCoroutine(current.Defeat(activePlayer));
            }
        }
        else
        {
            StartCoroutine(PlayerChange());
        }
    }

    public IEnumerator PlayerChange()
    {
        if (!finished)
        {
            playerControl = false;

            HexTile selected = InputManager.current.selectedHexTile;
            if (selected != null)
            {
                InputManager.current.selectedHexTile.SetSelected(false);
                InputManager.current.selectedHexTile = null;
            }
            nextTeamText.text = activeTeam.ToString().ToUpper() + " IS NEXT";
            nextTeamScreen.SetActive(true);

            if (activePlayer.isAI)
            {
                activePlayer.StartTurn();
            }
            else
            {

                yield return new WaitUntil(() => Input.touchCount == 0);
                yield return new WaitUntil(() => Input.touchCount > 0);

                if (!activePlayer.isAI)
                {
                    activePlayer.StartTurn();
                }

                playerControl = true;

                nextTeamScreen.SetActive(false);
            }
        }

    }

    // Show the End Screens
    public IEnumerator Defeat(Player defeatedPlayer)
    {
        yield return new WaitUntil(() => !CameraBehaviour.current.replayMode);

        finished = true;

        playerControl = false;

        endScreenText.text = defeatedPlayer.team.ToString().ToUpper() + " WAS DEFEATED";

        endScreenText.gameObject.SetActive(true);

        nextTeamScreen.gameObject.SetActive(false);

        GridManager.current.DisableHTDT();


        yield return new WaitForSeconds(1);

        yield return new WaitUntil(() => Input.touchCount == 0);

        yield return new WaitUntil(() => Input.anyKeyDown);

        endScreenText.gameObject.SetActive(false);

        StartCoroutine(Fades.current.FadeOut());

        yield return new WaitUntil(() => Fades.current.finished);

        SceneManager.LoadScene(0);

    }

    // Shows all necessary info
    private void UpdateUIInfo()
    {
        tileCountText.text = "TILES: " + activePlayer.tiles.Count;
    }
}
