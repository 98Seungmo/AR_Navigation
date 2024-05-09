using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;

public class FaceRegion : MonoBehaviour
{
    /// <summary>
    /// 코, 왼쪽/오른쪽
    /// </summary>
    public GameObject[] prefabs;

    private ARFaceManager _magFace;
    private XROrigin _origin;

    /// <summary>
    /// 얼굴 부위 정보
    /// </summary>
    private NativeArray<ARCoreFaceRegionData> faceRegions;

    public Vector3 _offset_Nose = Vector3.zero;

    void Start()
    {
        _magFace = GetComponent<ARFaceManager>();
        _origin = GetComponent<XROrigin>();

        for (int i = 0; i < prefabs.Length; i++)
        {
            prefabs[i] = Instantiate(prefabs[i], _origin.TrackablesParent);
        }
    }

    
    void Update()
    {
        ARCoreFaceSubsystem subsystem = (ARCoreFaceSubsystem)_magFace.subsystem;

        foreach (ARFace face in _magFace.trackables)
        {
            subsystem.GetRegionPoses(face.trackableId, Allocator.Persistent, ref faceRegions);

            /* faceRegion 에 저장되어 있는 부위의 위치에 배치 */
            foreach (ARCoreFaceRegionData faceRegion in faceRegions)
            {
                ARCoreFaceRegion regiontype = faceRegion.region;

                /* 활성화 된 부위의 위치, 회전 설정 */
                prefabs[(int)regiontype].transform.localPosition = faceRegion.pose.position + _offset_Nose;
                prefabs[(int)regiontype].transform.localRotation = faceRegion.pose.rotation;
            }
        }
    }
}
