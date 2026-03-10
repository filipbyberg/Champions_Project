using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LockToGrid : MonoBehaviour
{
    public int tileSize = 1;
    public Vector3 tileOffset = Vector3.zero;


    // Update is called once per frame
    void Update()
    {
        if(!EditorApplication.isPlaying)
        {
            Vector3 currentPosition = transform.position;

            float snappedX = Mathf.Round(currentPosition.x / tileSize) * tileSize + tileOffset.x;
            float snappedZ = Mathf.Round(currentPosition.z / tileSize) * tileSize + tileOffset.z;
            float snappedY = tileOffset.y;

            Vector3 snappedPosition = new Vector3(snappedX, snappedY, snappedZ);
            transform.position = snappedPosition;
        }
        
    }
}
