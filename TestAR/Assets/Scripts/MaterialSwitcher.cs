using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class MaterialSwitcher : MonoBehaviour
{

    public Material[] _materials;
    private ARFaceManager _magFace;
    private int _index;

    void Start()
    {
        _magFace = GetComponent<ARFaceManager>();
        _magFace.facePrefab.GetComponent<MeshRenderer>().material = _materials[0];  
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            _index = (_index + 1) % _materials.Length;

            foreach (ARFace face in _magFace.trackables)
            {
                face.GetComponent<MeshRenderer>().material = _materials[_index];
            }
        }
    }
}
