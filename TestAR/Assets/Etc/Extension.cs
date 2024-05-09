using Google.XR.ARCoreExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Extension : MonoBehaviour
{
    AREarthManager _mag = new AREarthManager();
    

    void Start()
    {
        if (_mag.IsGeospatialModeSupported(GeospatialMode.Enabled) == FeatureSupported.Unsupported)
        {
            Debug.Log("OK");
        }    
        else
        {
            Debug.Log("Unsupported");
        }
    }

    void Update()
    {
        
    }
    
}
