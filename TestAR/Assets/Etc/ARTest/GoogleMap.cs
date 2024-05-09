using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Globalization;
using System;

public class GoogleMap : MonoBehaviour
{
    public string apiKey; ///< 사용자의 Google API Key
    public float lat = 0.0f; ///< 위도 초기값 
    public float lng = 0.0f; ///< 경도 초기값 
    public int zoom = 14; ///< 지도 Zoom 초기값

    public enum resloution { low = 0, high = 2 } ///< 지도 해상도 enum 타입
    public resloution mapResolution = resloution.low; ///< 해상도 초기값

    public enum type { roadmap, satellite, hybrid, terrain }; ///< 지도 유형 enum 타입
    public type mapType = type.roadmap; ///< 지도 초기 유형 roadmap 

    private string url = string.Empty; ///< URL 초기화
    private int mapWidth = 640; ///< 지도 너비
    private int mapHeight = 640; ///< 지도 높이
    private bool mapIsLoading = false; ///< 지도 로딩 상태
    private Rect rect; ///< Raw Image 의 Rectransform 담을 변수
    private string apiKeyLast; ///< 마지막으로 사용한 API Key 저장

    private float latLast = 33.85660f; ///< 마지막으로 사용한 위도 저장
    private float lonLast = 151.21500f; ///< 마지막으로 사용한 경도 저장
    private int zoomLast = 14; ///< 마지막으로 사용한 지도 Zoom 저장
    private resloution mapResolutionLast = resloution.low; ///< 마지막으로 사용한 해상도 초기값 저장
    private type mapTypeLast = type.roadmap; ///< 마지막으로 사용한 지도 유형 저장
    private bool updateMap = true; ///< 지도 업데이트 필요성 (설정을 변경했을 떄 새로운 지도 이미지 요청 및 로드할 상황)

    private void Start()
    {
        /* 시작할 때 GetGoogleMap 코루틴 실행 */
        StartCoroutine(GetGoogleMap());
        /* Raw Image 컴포넌트 크기 가져오기 */
        rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
        /* rect 의 너비, 높이를 mapWidth, mapHeight 에 할당 */
        mapWidth = (int)Mathf.Round(rect.width);
        mapHeight = (int)Mathf.Round(rect.height);

        /* Test */
        StartCoroutine(GetDirections("37.5665, 126.97801", "37.5656, 126.97511"));
    }

    private void Update()
    {
        /* 설정이 변경될 때마다 새로운 지도 불러옴 */
        if (updateMap && (apiKeyLast != apiKey || !Mathf.Approximately(latLast, lat) || !Mathf.Approximately(lonLast, lng) ||
                          zoomLast != zoom || mapResolutionLast != mapResolution || mapTypeLast != mapType))
        {
            rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
            mapWidth = (int)Mathf.Round(rect.width);
            mapHeight = (int)Mathf.Round(rect.height);

            StartCoroutine(GetGoogleMap());
            /* 새로운 지도를 불러왔으니 업데이트 필요성 X */
            updateMap = false;
        }
    }

    /**
     * @brief Google Maps Static API URL 구성 하고 이미지를 불러와서 유니티에 표시
     */
    private IEnumerator GetGoogleMap()
    {
        //url = "https://maps.googleapis.com/maps/api/staticmap?center=" + lat + "," + lng +
        //                                                              "&zoom=" + zoom +
        //                                                              "&size=" + mapWidth +
        //                                                              "x" + mapHeight +
        //                                                              "&scale=" + mapResolution +
        //                                                              "&maptype=" + mapType +
        //                                                              "&key=" + apiKey;
        ///* 로딩 상태 */
        //mapIsLoading = true;
        ///* url 에 있는 텍스처를 가져오기 위해 요청 */
        //UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        ///* 요청 대기 */
        //yield return www.SendWebRequest();

        ///* 실패 */
        //if (www.result != UnityWebRequest.Result.Success)
        //{
        //    Debug.LogError("www error" + www.error);
        //    Debug.LogError(url);
        //}
        //else
        //{
        //    /* 성공 */
        //    mapIsLoading = false;
        //    /* Raw Image 에 다운로드 요청한 텍스처를 적용 */
        //    gameObject.GetComponent<RawImage>().texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

        //    apiKeyLast = apiKey;
        //    latLast = lat;
        //    lonLast = lng;
        //    zoomLast = zoom;
        //    mapResolutionLast = mapResolution;
        //    mapTypeLast = mapType;
        //    updateMap = true;
        //}

        /* 코루틴 종료  */
        yield break;
    }

    private IEnumerator GetDirections(string startLocation, string endLocation)
    {

        /*
         * driving: 도로 사용하여 거리를 계산
         * walking: 보행자 경로와 보도를 사용하여 거리를 계산
         * bicycling: 자전거 경로를 사용하여 거리를 계산
         * transit: 대중교통을 사용하여 거리를 계산
         */

        string url = "https://maps.googleapis.com/maps/api/directions/json?" +
            $"origin={startLocation}" +
            $"&destination={endLocation}" +
            $"&region=KR" +
            $"&mode=transit" +
            $"&key={apiKey}";

        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error :" + webRequest.error);
        }
        else
        {
            Debug.Log(webRequest.downloadHandler.text);

            //Root r = JsonConvert.DeserializeObject<Root>(webRequest.downloadHandler.text);
        }
    }
}

