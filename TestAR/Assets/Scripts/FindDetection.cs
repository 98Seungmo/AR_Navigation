using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;

public class FindDetection : MonoBehaviour
{
    public ARFaceManager magFace;
    public GameObject cube;
    public Text text;

    List<GameObject> cubes = new List<GameObject>();
    ARCoreFaceSubsystem subsys;
    NativeArray<ARCoreFaceRegionData> regionData;

    void Start()
    {
            
    }

    void Update()
    {
        
    }
}
