using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleMapsAPI : MonoBehaviour
{
    public string apiKey;

    // Unity���� ȣ���� �Լ�. ���� ��ǥ�� �޾Ƽ� ������ ��ǥ�� ��ȯ�ϰ� ���
    public void ConvertWorldToGeoLocation(Vector3 worldPosition)
    {
        StartCoroutine(ConvertCoordinates(worldPosition));
    }

    // ���� �� API�� ���� ���� ��ǥ�� ������ ��ǥ�� ��ȯ�ϴ� �޼���
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
                // ù ��° ����� ������ ��ǥ�� ��ȯ
                GoogleGeocodeResult result = response.results[0];
                Vector2 geoLocation = new Vector2(result.geometry.location.lat, result.geometry.location.lng);
                Debug.Log("������ ��ǥ: " + geoLocation);
            }
            else
            {
                Debug.LogError("���� �� API ����: " + response.status);
            }
        }
        else
        {
            Debug.LogError("���� �� API ��û ����: " + www.error);
        }
    }
}

// ���� �� API ������ �Ľ��ϱ� ���� Ŭ������
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
