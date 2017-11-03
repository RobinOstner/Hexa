using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ChangedTile
{
    public ChangedTile(HexTile tileA, HexTile tileB, int unitChangeA, int unitChangeB)
    {
        marked = false;
        this.tileA = tileA;
        this.tileB = tileB;
        this.unitChangeA = unitChangeA;
        this.unitChangeB = unitChangeB;
        teamABefore = teamAAfter = teamBBefore = teamBAfter = GameManager.Teams.Null;
        unitAAfter = unitBAfter = 0;
    }

    public bool marked;
    public HexTile tileA, tileB;
    public int unitChangeA, unitChangeB;
    public int unitAAfter, unitBAfter;
    public GameManager.Teams teamABefore, teamAAfter, teamBBefore, teamBAfter;

    public void AddTeamBefore(GameManager.Teams aBefore,GameManager.Teams bBefore)
    {
        teamABefore = aBefore;
        teamBBefore = bBefore;
    }

    public void AddTeamAfter(GameManager.Teams aAfter, GameManager.Teams bAfter, int unitAAfter, int unitBAfter)
    {
        teamAAfter = aAfter;
        teamBAfter = bAfter;
        this.unitAAfter = unitAAfter;
        this.unitBAfter = unitBAfter;
    }

    public void Mark()
    {
        marked = true;
    }

    public void Undo()
    {
        tileA.units -= unitChangeA;
        tileB.units -= unitChangeB;
        tileA.team = teamABefore;
        tileB.team = teamBBefore;
        tileB.SetMoveLocked(false);
    }

    public void Apply()
    {
        tileA.units += unitChangeA;
        tileB.units += unitChangeB;
        tileA.team = teamAAfter;
        tileB.team = teamBAfter;
        if (teamABefore == teamBAfter)
        {
            tileB.SetMoveLocked(true);
        }
    }
}

public class Replay : MonoBehaviour {

    public List<ChangedTile> changes;

	// Use this for initialization
	void Start () {
        changes = new List<ChangedTile>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // Adds the tile to the changed tiles
    public void AddMovementToReplay(ChangedTile change)
    {
        changes.Add(change);
    }

    public IEnumerator ShowReplay()
    {
        CameraBehaviour.current.replayMode = true;

        //yield return new WaitUntil(() => Input.touchCount == 0);
        //yield return new WaitUntil(() => Input.touchCount > 0);

        // Undo All Changes
        for(int i=changes.Count-1; i>=0; i--)
        {
            ChangedTile change = changes[i];

            change.Undo();

            //yield return new WaitUntil(() => Input.touchCount == 0);
            //yield return new WaitUntil(() => Input.touchCount > 0);
        }

        HexTile focusTile = null;

        for(int i=0; i < changes.Count; i++)
        {
            ChangedTile change = changes[i]; 

            CameraBehaviour.current.focusTransform = change.tileB.transform;

            float waitTime = 0.25f;

            if(focusTile != change.tileB)
            {
                waitTime = .5f;
            }

            focusTile = change.tileB;

            yield return new WaitForSeconds(waitTime);

            change.Apply();


            //yield return new WaitUntil(() => Input.touchCount == 0);
            //yield return new WaitUntil(() => Input.touchCount > 0);

            //yield return new WaitForSeconds(.25f);

            if (i+1 < changes.Count && changes[i+1].tileB != focusTile)
            {
                focusTile = changes[i + 1].tileB;
                yield return new WaitForSeconds(.5f);
            }
            if(i+1 == changes.Count)
            {
                yield return new WaitForSeconds(.5f);
            }
        }

        changes = new List<ChangedTile>();

        //changes = new List<ChangedTile>();

        CameraBehaviour.current.focusTransform = null;
        CameraBehaviour.current.replayMode = false;

        StartCoroutine(CameraBehaviour.current.MoveCameraToLast(GameManager.current.activePlayer.lastCameraPos));
    }
}
