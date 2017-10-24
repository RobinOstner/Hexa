using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerMenu : MonoBehaviour {

    public int Size;
    public int Missing;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Size = Settings.GridSize;
        Missing = Settings.MissingPercent;
    }

    public void StartGame()
    {
        Settings.p1IsAI = false;
        Settings.p2IsAI = true;
        // Should be able to be changed in settings
        Settings.ShowAIMovement = false;

        StartCoroutine(StartNewGameFade());
    }

    public IEnumerator StartNewGameFade()
    {
        StartCoroutine(Fades.current.FadeOut());

        yield return new WaitUntil(() => Fades.current.finished);

        SceneManager.LoadScene(2);
    }

    public void IncreasDifficulty()
    {
        Settings.AIDifficulty += 10;
        Settings.AIDifficulty = Mathf.Clamp(Settings.AIDifficulty, 0, 100);
    }

    public void DecreaseDifficulty()
    {
        Settings.AIDifficulty -= 10;
        Settings.AIDifficulty = Mathf.Clamp(Settings.AIDifficulty, 0, 100);
    }

    public void IncreaseSize()
    {
        Settings.GridSize++;
        Settings.GridSize = Mathf.Clamp(Settings.GridSize, 3, 20);
    }

    public void DecreaseSize()
    {
        Settings.GridSize--;
        Settings.GridSize = Mathf.Clamp(Settings.GridSize, 3, 20);
    }

    public void IncreaseMissing()
    {
        Settings.MissingPercent = Mathf.Clamp(Settings.MissingPercent + 10, 0,70);
    }

    public void DecreaseMissing()
    {
        Settings.MissingPercent = Mathf.Clamp(Settings.MissingPercent - 10, 0, 70);
    }
}
