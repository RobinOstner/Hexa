using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour {

    // Dynamic Background
    public Color goldBackground, blueBackground, redBackground;
    public bool dynamic;

    public bool vibrateMainMenu;
    public Gradient vibeGradient;
    public float value;
    public float vibeRate;
    private float startValue;
    private float goalValue;
    private float delta;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (dynamic)
        {
            DynamicBackground();
        }
        if (vibrateMainMenu)
        {
            VibrateColor();
        }
	}

    // Adjust the background color to the active team
    void DynamicBackground()
    {
        Color active = Color.black;

        /*
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
        */

        active = ColorManager.current.background;

        GetComponent<Renderer>().material.color = active;
    }

    // Makes The Background Color Vibrate
    void VibrateColor()
    {
        delta += Time.deltaTime;
        value = Mathf.Lerp(startValue, goalValue, vibeRate*delta);

        if(Mathf.Abs(value-goalValue) <= 0.001f)
        {
            if (goalValue == 1) { goalValue = 0; startValue = 1; delta = 0; }
            else { goalValue = 1; startValue = 0; delta = 0; }
        }

        GetComponent<Renderer>().material.color = vibeGradient.Evaluate(value);
    }
}
