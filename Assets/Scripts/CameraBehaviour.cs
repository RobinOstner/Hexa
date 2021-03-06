﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour {

    public static CameraBehaviour current;

    // Where the Camera should be
    public Vector3 targetPosition;
    // Camera Height
    public int cameraHeight;
    // Camera Speed
    public float cameraSpeed;

    // Zoom/Move Mode
    public bool pinchZoomMoveEnabled;

    // REPLAYS
    public bool replayMode;
    public Transform focusTransform;

    // PinchZoom/Move
    // Is Pinching?
    public bool isPinching;
    // Is Dragging?
    public bool isDragging;
    // Movement Speed when dragging with 2 fingers
    public float moveSpeed;
    // How fast does the Camera zoom in/out when pinching
    public float zoomFactor;
    // How does the zoom react to the Cameras height
    public float zoomHeightFactor;
    // How fast does the constraint react
    public float constraintSpeed;

    public float shakeDuration;
    public float shakeIntensity;

    // Alternate Zoom/Move

	// Use this for initialization
	void Start () {
        current = this;
	}
	
	// Update is called once per frame
	void Update () {

        if (replayMode)
        {
            FocusReplay();
        }
        else
        {
            // Camera can only be controlled if Player is actually in Control
            if (GameManager.current.playerControl && !GameManager.current.activePlayer.isAI)
            {
                if (pinchZoomMoveEnabled)
                {
                    PinchMoveZoom();
                }
                else
                {
                    AlternateMoveZoom();
                }

                MoveZoomConstraints();
            }
            else
            {
                MoveCameraToMiddle();
            }
        }
	}

    void FocusReplay()
    {
        if (focusTransform != null)
        {
            targetPosition = focusTransform.position + new Vector3(0.5f, 0, 0.5f) + Vector3.up * 2;

            if ((targetPosition - transform.position).magnitude >= 0.01f)
            {
                // Slerping of Position
                Vector3 movedPosition = Vector3.Slerp(transform.position, targetPosition, cameraSpeed);

                // Assign Position
                transform.position = movedPosition;
            }
        }
        else
        {
            MoveCameraToMiddle();
        }
    }

    // Moves Camera to the Middle of the Grid
    void MoveCameraToMiddle()
    {
        // Get the Actual Grid Size
        //int gridSize = GridManager.current.GetActualGridSize();

        // Calculate Position
        /*
        float posX = gridSize / 2f;
        float posY = posX;
        targetPosition = Vector3.right * posX + Vector3.forward * posY + Vector3.up*cameraHeight/2f + Vector3.up*gridSize;
        */

        targetPosition = GridManager.current.GetMidTile().transform.position + new Vector3( 0.5f, 0, 0.5f) + Vector3.up * cameraHeight + Vector3.up * Vector3.Magnitude(GridManager.current.GetMidTile().transform.position);

        // Slerping of Position
        Vector3 movedPosition = Vector3.Slerp(transform.position, targetPosition, cameraSpeed);

        // Assign Position
        transform.position = movedPosition;
    }

    public IEnumerator MoveCameraToLast(Vector3 targetPos)
    {
        if (!GameManager.current.activePlayer.isAI)
        {
            bool touching = true;

            while ((transform.position - targetPos).magnitude >= .01f)
            {
                if (Input.touchCount == 0)
                {
                    touching = false;
                }
                if (!touching && Input.touchCount > 0)
                {
                    break;
                }

                transform.position = Vector3.Slerp(transform.position, targetPos, cameraSpeed);
                yield return null;
            }
        }
    }

    public IEnumerator Shake()
    {

        float elapsed = 0.0f;

        Vector3 originalCamPos = Camera.main.transform.position;

        while (elapsed < shakeDuration)
        {

            elapsed += Time.deltaTime;

            float percentComplete = elapsed / shakeDuration;
            float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

            // map value to [-1, 1]
            float x = Random.value * 2.0f - 1.0f;
            float z = Random.value * 2.0f - 1.0f;
            x *= originalCamPos.y * shakeIntensity * damper;
            z *= originalCamPos.y * shakeIntensity * damper;

            x += originalCamPos.x;
            z += originalCamPos.z;

            Camera.main.transform.position = new Vector3(x, originalCamPos.y, z);

            yield return null;
        }

        Camera.main.transform.position = originalCamPos;
    }

    // Move the Camera with 2 Fingers
    void PinchMoveZoom()
    {
        isPinching = Input.touchCount == 2;

        if (isPinching)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 delta1 = touch1.deltaPosition;
            Vector2 delta2 = touch2.deltaPosition;

            float deltaX = delta1.x + delta2.x;
            deltaX *= transform.position.y;
            float deltaY = delta1.y + delta2.y;
            deltaY *= transform.position.y;


            transform.position -= new Vector3(deltaX, 0, deltaY)/100 * moveSpeed;

            Vector2 posOld1 = touch1.position - delta1;
            Vector2 posOld2 = touch2.position - delta2;

            float diff = (posOld1 - posOld2).magnitude - (touch1.position - touch2.position).magnitude;
            float newHeight = diff * zoomFactor/1000 * transform.position.y*zoomHeightFactor + transform.position.y;
            
            transform.position = new Vector3(transform.position.x, newHeight, transform.position.z);
        }
    }

    // Constraints Movement and Zoom to specific values
    void MoveZoomConstraints()
    {
        float posX = transform.position.x;
        float height = transform.position.y;
        float posZ = transform.position.z;

        //newHeight = Mathf.Max(newHeight, 2);
        if (height < 3)
        {
            height = Mathf.Lerp(height, 3, Time.deltaTime*constraintSpeed);
        }
        if(height > GridManager.current.gridSize)
        {
            height = Mathf.Min(Mathf.Lerp(height, GridManager.current.gridSize, Time.deltaTime * constraintSpeed), GridManager.current.gridSize*2f);
        }

        if(posX < 0)
        {
            posX = Mathf.Lerp(posX, 2, Time.deltaTime * constraintSpeed);
        }
        if (posX > GridManager.current.gridSize)
        {
            posX = Mathf.Lerp(posX, GridManager.current.gridSize-2, Time.deltaTime * constraintSpeed);
        }
        if (posZ < 0)
        {
            posZ = Mathf.Lerp(posZ, 1, Time.deltaTime * constraintSpeed);
        }
        if (posZ > GridManager.current.gridSize)
        {
            posZ = Mathf.Lerp(posZ, GridManager.current.gridSize-1, Time.deltaTime * constraintSpeed);
        }

        transform.position = new Vector3(posX, height, posZ);
    }

    // Alternative Way to Zoom and Move the Camera
    void AlternateMoveZoom()
    {
        isPinching = Input.touchCount == 2;

        if (isPinching)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 delta1 = touch1.deltaPosition;
            Vector2 delta2 = touch2.deltaPosition;

            
            Vector2 posOld1 = touch1.position - delta1;
            Vector2 posOld2 = touch2.position - delta2;

            float diff = (posOld1 - posOld2).magnitude - (touch1.position - touch2.position).magnitude;
            float newHeight = diff * zoomFactor / 1000 * transform.position.y * zoomHeightFactor + transform.position.y;

            transform.position = new Vector3(transform.position.x, newHeight, transform.position.z);
        }

        isDragging = Input.touchCount == 1;

        if (isDragging)
        {
            Touch touch1 = Input.GetTouch(0);

            Vector2 delta1 = touch1.deltaPosition;


            float deltaX = delta1.x;
            deltaX *= transform.position.y;
            float deltaY = delta1.y;
            deltaY *= transform.position.y;


            transform.position -= new Vector3(deltaX, 0, deltaY) / 50 * moveSpeed;
        }
    }
}
