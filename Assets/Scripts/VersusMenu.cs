using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VersusMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void StartGame()
    {
        Settings.p1IsAI = false;
        Settings.p2IsAI = false;
        // Should be able to be changed in settings
        Settings.ShowAIMovement = false;

        Debug.Log("Start New Singleplayer Game: Size: " + Settings.GridSize + " Mix: " + Settings.MissingPercent + " Diff: " + Settings.AIDifficulty);

        StartCoroutine(StartNewGameFade());
    }

    public IEnumerator StartNewGameFade()
    {
        StartCoroutine(Fades.current.FadeOut());

        yield return new WaitUntil(() => Fades.current.finished);

        SceneManager.LoadScene(2);
    }
}
