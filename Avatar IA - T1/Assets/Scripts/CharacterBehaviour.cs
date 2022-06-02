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
    void Start()
    {
        originalPos = transform.localPosition;
        currentTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        float offset = amplitude * Mathf.Sin(currentTime * period);
        if (offset < 0.0f)
            offset = 0.0f;

        transform.localPosition = new Vector3(originalPos.x, originalPos.y + offset, originalPos.z);
    }
}
