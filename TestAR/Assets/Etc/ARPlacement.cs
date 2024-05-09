using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* 추가 */
using UnityEngine.XR.ARFoundation;

public class ARPlacement : MonoBehaviour
{
   public ARRaycastManager _raycastManager;

    /// <summary>
    /// 감지된 객체를 담을 리스트
    /// </summary>
    static List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

    public Camera arCamera;
    public GameObject gameobject;

    private void Awake()
    {
               
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("마우스 클릭");
            //Vector3 centerScreen = new Vector3(Screen.width / 2, Screen.height / 2);
            Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
            Debug.Log($"{ray}");

            if (_raycastManager.Raycast(ray, raycastHits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
            {
                Pose hitpose = raycastHits[0].pose;
                //Vector3 hitposition = hitpose.position;
                ///* 객체 위치를 변경함 */
                //gameobject.transform.position = hitposition;
                Instantiate(gameobject, hitpose.position, Quaternion.identity);

            }
            else
            {
                Debug.Log($"{raycastHits}");
                Debug.Log($"??? : {_raycastManager.Raycast(ray, raycastHits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon)}");
                Debug.Log($"Hits Count: {raycastHits.Count}");

            }
        }
    }
}
