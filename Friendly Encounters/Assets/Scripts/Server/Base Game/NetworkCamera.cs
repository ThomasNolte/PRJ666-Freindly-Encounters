﻿using UnityEngine;

public class NetworkCamera : MonoBehaviour
{
    public static NetworkCamera instance = null;
    private float cameraSizeOffsetX;
    private float cameraSizeOffsetY;

    public Transform topLeft;
    public Transform bottomRight;

    Transform playerTransform;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        cameraSizeOffsetX = CameraExtension.OrthographicBounds(Camera.main.GetComponent<Camera>()).extents.x;
        cameraSizeOffsetY = CameraExtension.OrthographicBounds(Camera.main.GetComponent<Camera>()).extents.y;
        DontDestroyOnLoad(gameObject);
    }

    void LateUpdate()
    {
        //If the player is gone no need to move the camera
        if (playerTransform != null)
        {
            Vector3 pos = playerTransform.position + new Vector3(0, 0, -10);
            //Set the camera's transform to players
            //but plus the offset between camera and player
            if (DodgeWaterBalloonScoreBoard.IsDodgeWaterBalloon)
            {
                pos.x = Mathf.Clamp(pos.x, topLeft.position.x + cameraSizeOffsetX, bottomRight.position.x - cameraSizeOffsetX);
                pos.y = Mathf.Clamp(pos.y, bottomRight.position.y + cameraSizeOffsetY, topLeft.position.y - cameraSizeOffsetY);
            }
            pos.z = -10;

            transform.position = pos;
        }

    }

    public void setTarget(Transform target)
    {
        playerTransform = target;
    }

}