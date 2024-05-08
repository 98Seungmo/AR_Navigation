using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    public float magneticHeading;

    public float trueHeading;

    public GameObject compass;

    private void OnEnable()
    {
        Input.location.Start();
        Input.compass.enabled = true;
    }
    private void OnDisable()
    {
        Input.compass.enabled = false;
        Input.location.Stop();
    }

    private void LateUpdate()
    {
        if (Input.compass.headingAccuracy == 0 || Input.compass.headingAccuracy > 0)
        {
            magneticHeading = Input.compass.magneticHeading;
            trueHeading = Input.compass.trueHeading;
        }

        if (trueHeading != 0)
        {
            compass.transform.rotation = Quaternion.Euler(0, 0, trueHeading);
        }
    }
}
