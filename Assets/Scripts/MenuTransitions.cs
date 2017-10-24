using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuTransitions : MonoBehaviour {

    // The Transform of this object
    RectTransform transform;

    public enum Menus { main, singleplayer, versus, settings }
    public Menus currentMenu;
    public Vector3 main, singleplayer, versus, settings;
    public float transitionSpeed;
    public float minDist;

    public bool inTransition;

	// Use this for initialization
	void Start () {
        transform = GetComponent<RectTransform>();

        StartCoroutine(Fades.current.FadeIn());
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.Escape) && !inTransition)
        {
            StartCoroutine(ShowMainTransition());
        }
	}

    IEnumerator ShowMainTransition()
    {
        yield return new WaitUntil(() => !inTransition);

        currentMenu = Menus.main;

        inTransition = true;

        while (Vector3.Magnitude(transform.localPosition - main) >= minDist)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, main, transitionSpeed);
            yield return null;
        }

        inTransition = false;
    }

    public void ShowSingleplayer()
    {
        if (!inTransition)
        {
            StartCoroutine(ShowSingleplayerTransition());
        }
    }

    IEnumerator ShowSingleplayerTransition()
    {
        yield return new WaitUntil(() => !inTransition);

        currentMenu = Menus.singleplayer;

        inTransition = true;

        while(Vector3.Magnitude(transform.localPosition-singleplayer) >= minDist)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, singleplayer, transitionSpeed);
            yield return null;
        }

        inTransition = false;
    }

    public void ShowVersus()
    {
        if (!inTransition)
        {
            StartCoroutine(ShowVersusTransition());
        }
    }

    IEnumerator ShowVersusTransition()
    {
        yield return new WaitUntil(() => !inTransition);

        currentMenu = Menus.versus;

        inTransition = true;

        while (Vector3.Magnitude(transform.localPosition - versus) >= minDist)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, versus, transitionSpeed);
            yield return null;
        }

        inTransition = false;
    }

    public void ShowSettings()
    {
        if (!inTransition)
        {
            StartCoroutine(ShowSettingsTransition());
        }
    }

    IEnumerator ShowSettingsTransition()
    {
            currentMenu = Menus.settings;

            inTransition = true;

            while (Vector3.Magnitude(transform.localPosition - settings) >= minDist)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, settings, transitionSpeed);
                yield return null;
            }

            inTransition = false;
    }
}
