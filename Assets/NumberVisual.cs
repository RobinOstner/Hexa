using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberVisual : MonoBehaviour {

    public enum ValuesToShow { units, unitsAfterMovement, gridSize, missing, difficulty }
    public ValuesToShow valueToShow;

    public List<Sprite> sprites;

    public int value;

    private int oldValue;

    private Image image;

	// Use this for initialization
	void Start () {
        image = GetComponent<Image>();

        UpdateValue();
        UpdateVisual();
	}
	
	// Update is called once per frame
	void Update () {
        UpdateValue();

		if(value != oldValue)
        {
            UpdateVisual();
        }

        oldValue = value;
	}

    private void UpdateValue()
    {
        switch (valueToShow)
        {
            case ValuesToShow.gridSize:
                value = Settings.GridSize;
                break;
            case ValuesToShow.missing:
                value = Settings.MissingPercent/10;
                break;
            case ValuesToShow.difficulty:
                value = Settings.AIDifficulty/10;
                break;
            default:
                value = 0;
                break;
        }
    }

    void UpdateVisual()
    {
        if (value >= 0 && value < sprites.Count)
        {
            image.sprite = sprites[value];
        }
    }
}
