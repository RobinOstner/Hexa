using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour {

    public Color goldBackground, blueBackground, redBackground;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        DynamicBackground();
	}

    // Adjust the background color to the active team
    void DynamicBackground()
    {
        Color active = Color.black;

        switch (GameManager.current.activeTeam)
        {
            case GameManager.Teams.Gold:
                active = goldBackground;
                break;
            case GameManager.Teams.Blue:
                active = blueBackground;
                break;
            case GameManager.Teams.Red:
                active = redBackground;
                break;
        }

        GetComponent<Renderer>().material.color = active;
    }
}
