using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HexTileDisplay : MonoBehaviour {
    
    // Sprite GameObject Reference
    public GameObject hexagonSprite;

    // Hexagon UI Display
    public GameObject hexDisplayPrefab;     // Prefab
    private GameObject hexDisplay;          // Actual GameObject
    private RectTransform hexDisplayTransform;  // Rect Transform of Display Object
    private Text hexDisplayText;            // Text Component of Display Object
    public HexTile hexTile;

    // Reference to the material
    private Material spriteMaterial;

    // Which Team?
    public GameManager.Teams team;
    // Neighbour Tile Indices
    public List<HexTileDisplay> neighbourTiles;
    // Is the tile neighbour to one of the active Tiles
    public bool isNeighbourToActive;

    // Is invisible/blank
    private bool isBlank;

    // Is mouse hovering?
    public bool hovering;
    public float hoveringShine;

    // Is Tile selected?
    public bool selected;

    // Is the Tile being highlighted?
    public bool highlighted;

    // Position of the Display
    public float canvasPosX, canvasPosY;

	// Use this for initialization
	void Start () {
        // Set the reference to the Material
        spriteMaterial = hexagonSprite.GetComponent<Renderer>().material;

        // Receive HexTile Component for future use
        hexTile = GetComponent<HexTile>();

        InitializeHexDisplay();
    }
    
    // Initializes everything needed for the Display
    public void InitializeHexDisplay()
    {
        // Instantiate the new Display Object
        hexDisplay = Instantiate(hexDisplayPrefab, FindObjectOfType<Canvas>().transform);
        // Receive the Rect Transform for future use
        hexDisplayTransform = hexDisplay.GetComponent<RectTransform>();
        // Receive the Text Component for future use
        hexDisplayText = hexDisplay.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update ()
    {
        team = hexTile.team;
        CheckNeighbouring();
        if (highlighted || isVisible())
        {
            isBlank = false;
            CalculateTextCanvasPos();
            UpdateTextDisplay();
            SetColor();
        }
        else
        {
            if (!isBlank) { SetBlank(); }
        }
    }

    void OnDestroy()
    {
        DestroyImmediate(hexDisplay);
    }

    // Changes The Color of The Material
    public void SetHovering()
    {
        hovering = true;
    }

    public void SetSelected(bool value) { selected = value; }

    // Set the color according to current state
    private void SetColor()
    {
        // Reset Color to normal
        Color normalColor = GridManager.current.tileColors.normal;
        Color activeColor = GetColorBasedOnTeam(false);

        if (GameManager.current.playerControl)
        {
            // Active Team
            if (team == GameManager.current.activeTeam)
            {
                normalColor = activeColor;
            }

            // Other Team but Neighbouring
            if (team != GameManager.current.activeTeam && team != GameManager.Teams.Null && isNeighbourToActive)
            {
                normalColor = activeColor / 2;
            }

            // Tile is Selected
            if (selected)
            {
                normalColor *= 4f;
            }
            // Tile is Highlighted && team != GameManager.current.activePlayer.team
            if (highlighted)
            {
                normalColor *= 2f;
            }

            if (hexTile.moveLocked)
            {
                normalColor /= 2f;
            }
        }
        
        // Make sure it is displayed correctly because of alpha channel
        normalColor = new Color(normalColor.r, normalColor.g, normalColor.b, 1);

        // Set the color
        spriteMaterial.color = normalColor;
    }

    // No Text & No Color
    public void SetBlank()
    {
        isBlank = true;
        spriteMaterial.color = GridManager.current.tileColors.normal;
        hexDisplayText.text = "";
    }

    // Returns the Color of the Active Team
    public Color GetColorBasedOnTeam(bool defaultOption)
    {
        if (!defaultOption)
        {
            switch (team)
            {
                case GameManager.Teams.Gold:
                    return GridManager.current.tileColors.selectedGold;
                case GameManager.Teams.Blue:
                    return GridManager.current.tileColors.selectedBlue;
                case GameManager.Teams.Red:
                    return GridManager.current.tileColors.selectedRed;
                default:
                    switch (GameManager.current.activeTeam)
                    {
                        case GameManager.Teams.Gold:
                            return GridManager.current.tileColors.selectedGold;
                        case GameManager.Teams.Blue:
                            return GridManager.current.tileColors.selectedBlue;
                        case GameManager.Teams.Red:
                            return GridManager.current.tileColors.selectedRed;
                        default:
                            return Color.white;
                    }
            }
        }
        else
        {
            switch (GameManager.current.activeTeam)
            {
                case GameManager.Teams.Gold:
                    return GridManager.current.tileColors.selectedGold;
                case GameManager.Teams.Blue:
                    return GridManager.current.tileColors.selectedBlue;
                case GameManager.Teams.Red:
                    return GridManager.current.tileColors.selectedRed;
                default:
                    return Color.white;
            }
        }
    }

    // Calculates The UIPosition
    public void CalculateTextCanvasPos()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        Vector3 posCorner = Camera.main.WorldToViewportPoint(transform.position - Vector3.right/2);
        float diff = (pos - posCorner).x * Screen.width/2;
        canvasPosX = pos.x * Screen.width + diff;
        canvasPosY = pos.y * Screen.height + diff;

        hexDisplayTransform.position = new Vector3(canvasPosX, canvasPosY, 0);
        hexDisplayTransform.sizeDelta = new Vector2(diff * 2, diff * 2);
    }
    
    // Changes the values of the Display
    public void UpdateTextDisplay()
    {
        // Is the Tile even visible?
        if (isVisible()) {
            if (highlighted && GameManager.current.playerControl)
            {
                string filler = hexTile.team == GameManager.current.activeTeam ? " + " : " VS ";     
                hexDisplayText.text = hexTile.units + filler + hexTile.unitsAfterMovement;
            }
            else
            {
                hexDisplayText.text = "" + hexTile.units;
            }
        }
        else
        {
            if (highlighted && GameManager.current.playerControl)
            {
                hexDisplayText.text = "" + hexTile.unitsAfterMovement;
            }
            else
            {
                hexDisplayText.text = "";
            }
        }

        hexDisplayText.text += hexTile.attacking ? " -> " + hexTile.unitsAfterMovement : "";
        
        if (isVisible() && highlighted && hexTile.attacking)
        {
            hexDisplayText.text = hexTile.units + " -> " + hexTile.units;
        }

        // Set the Color accordingly
        Color newColor = hexTile.isBaseTile && !highlighted ? Color.black : GetColorBasedOnTeam(highlighted) / 3f;
        // Adjust for Base Color
        if (hexTile.isBaseTile)
        {
            hexDisplayText.fontSize += 20;
        }

        // Set the Color
        hexDisplayText.color = new Color(newColor.r, newColor.g, newColor.b, 1);
    }


    // Check the surroundings
    public void CheckNeighbouring()
    {
        isNeighbourToActive = false;
        foreach (HexTileDisplay otherTile in neighbourTiles)
        {
            if (otherTile.team == GameManager.current.activeTeam)
            {
                isNeighbourToActive = true;
            }
        }
    }

    // Is the tile color visible for the currently active player
    private bool isVisible()
    {
        return GameManager.current.playerControl && ((isNeighbourToActive && team != GameManager.Teams.Null) || team == GameManager.current.activeTeam);
    }

    // Disable the Text Display
    public void DisableText()
    {
        hexDisplayText.gameObject.SetActive(false);
    }
}
