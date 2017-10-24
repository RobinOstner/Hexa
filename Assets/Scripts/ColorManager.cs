using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour {

    public static ColorManager current;

    public Color background,
        emptyNormal, emptySelected, emptyHighlighted, emptyFont,
        goldNormal, goldSelected, goldHighlighted, goldFont, goldSelectedFont, 
        blueNormal, blueSelected, blueHighlighted, blueFont, blueSelectedFont;

	// Use this for initialization
	void Start () {
        current = this;	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Color SetColor(HexTileDisplay tile)
    {
        if (!tile.isVisible())
        {
            if (tile.selected) { return emptySelected; }
            if (tile.highlighted) { return emptyHighlighted; }
            return emptyNormal;
        }
        switch (tile.team)
        {
            case GameManager.Teams.Gold:
                if (tile.selected) { return goldSelected; }
                if (tile.highlighted) { return goldHighlighted; }
                return goldNormal;
            case GameManager.Teams.Blue:
                if (tile.selected) { return blueSelected; }
                if (tile.highlighted) { return blueHighlighted; }
                return blueNormal;
            default:
                if (tile.selected) { return emptySelected; }
                if (tile.highlighted) { return emptyHighlighted; }
                return emptyNormal;
        }
    }

    public Color SetFontColor(HexTileDisplay tile)
    {
        if (!tile.isVisible())
        {
            return emptyFont;
        }
        if (tile.selected || tile.highlighted)
        {
            switch (tile.team)
            {
                case GameManager.Teams.Gold:
                    return goldSelectedFont;
                case GameManager.Teams.Blue:
                    return blueSelectedFont;
                default:
                    return emptyFont;
            }
        }
        else
        {
            switch (tile.team)
            {
                case GameManager.Teams.Gold:
                    return goldFont;
                case GameManager.Teams.Blue:
                    return blueFont;
                default:
                    return emptyFont;
            }
        }
    }
}
