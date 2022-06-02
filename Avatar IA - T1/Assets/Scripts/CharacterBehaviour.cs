using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector3 originalPos;
    public float period;
    public float amplitude;
    private float currentTime;
    private bool isSlow;

    public float lineInterval = 1.0f;
    public float xOffset;
    public float zOffset;
    private Vector3 linePos;
    void Start()
    {
        originalPos = transform.localPosition;
        currentTime = 0.0f;
        isSlow = false;
        linePos = new Vector3(xOffset, originalPos.y, zOffset);
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        int timeCost = 1;
        if (MapManager.Instance.follower.hasPath())
            timeCost = MapManager.Instance.follower.getCurrentTimeCost();
        float yOffset = amplitude * Mathf.Sin(currentTime * period * 3.0f);
        
        if (timeCost >= 15)
        {
            isSlow = true;
            yOffset = 0.0f;
            currentTime = 0.0f;
        }
        else if (isSlow)
        {
            isSlow = false;
            currentTime = 0.0f;
        }
            
        if (yOffset < 0.0f)
            yOffset = 0.0f;

        transform.localPosition = Vector3.Lerp(originalPos, linePos, currentTime / lineInterval);
        transform.localPosition += new Vector3(0, yOffset, 0);
        MapManager.Instance.objectLookAtEvent(transform);
    }
}
