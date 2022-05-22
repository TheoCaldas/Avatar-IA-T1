using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public GameObject topDownCamera;
    public GameObject thirdPersonCamera;

    private int selectedCamera = 1;

    private void Update() 
    {
        if (Input.GetKey("1") && selectedCamera == 2)
        {
            selectedCamera = 1;
            topDownCamera.SetActive(true);
            thirdPersonCamera.SetActive(false);
        }

        if (Input.GetKey("2") && selectedCamera == 1)
        {
            selectedCamera = 2;
            topDownCamera.SetActive(false);
            thirdPersonCamera.SetActive(true);
        }
    }
}
