using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleMapsAPI : MonoBehaviour
{
    public string apiKey;

    // Unity에서 호출할 함수. 월드 좌표를 받아서 지리적 좌표로 변환하고 출력
    public void ConvertWorldToGeoLocation(Vector3 worldPosition)
    {
        StartCoroutine(ConvertCoordinates(worldPosition));
    }

    // 구글 맵 API를 통해 월드 좌표를 지리적 좌표로 변환하는 메서드
    private IEnumerator ConvertCoordinates(Vector3 worldPosition)
    {
        string url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={worldPosition.x},{worldPosition.y}&key={apiKey}";

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonResult = www.downloadHandler.text;
            GoogleGeocodeResponse response = JsonUtility.FromJson<GoogleGeocodeResponse>(jsonResult);

            if (response.status == "OK")
            {
                // 첫 번째 결과의 지리적 좌표를 반환
                GoogleGeocodeResult result = response.results[0];
                Vector2 geoLocation = new Vector2(result.geometry.location.lat, result.geometry.location.lng);
                Debug.Log("지리적 좌표: " + geoLocation);
            }
            else
            {
                Debug.LogError("구글 맵 API 오류: " + response.status);
            }
        }
        else
        {
            Debug.LogError("구글 맵 API 요청 실패: " + www.error);
        }
    }
}

// 구글 맵 API 응답을 파싱하기 위한 클래스들
[System.Serializable]
public class GoogleGeocodeResponse
{
    public string status;
    public GoogleGeocodeResult[] results;
}

[System.Serializable]
public class GoogleGeocodeResult
{
    public GoogleGeocodeGeometry geometry;
}

[System.Serializable]
public class GoogleGeocodeGeometry
{
    public GoogleGeocodeLocation location;
}

[System.Serializable]
public class GoogleGeocodeLocation
{
    public float lat;
    public float lng;
}
