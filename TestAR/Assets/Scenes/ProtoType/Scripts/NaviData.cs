using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.Android;
using System;

#region Autocomplete
/**
 * @brief PlaceAPI의 Autocomplete에서 Json 파일의 클래스들을 정리 
 */
[System.Serializable]
public class PlaceAutocompleteResponse
{
    public Prediction[] predictions;
    public string status;
}

[System.Serializable]
public class Prediction
{
    public string description;
    public string place_id;
    public MatchedSubstrings[] matched_substrings;
    public StructuredFormatting structured_formatting;
    public Term[] terms;
    public string[] types;
}

public class MatchedSubstrings
{
    public int length;
    public int offset;
}

[System.Serializable]
public class StructuredFormatting
{
    public string main_text;
    public string secondary_text;
}

[System.Serializable]
public class Term
{
    public int offset;
    public string value;
}
#endregion

#region PlaceDetails
/**
 * @brief PlaceAPI의 PlaceDetails에서 Json 파일들의 클래스를 정의 
 */
[System.Serializable]
public class PlaceDetailsResponse
{
    public List<object> html_attributions;
    public Result result;
    public string status;
}

[System.Serializable]
public class Result
{
    public Geometry geometry;
    public string formatted_address;
    public string name;
    public List<AddressComponent> address_components;
    public string business_status;
    public string formatted_phone_number;
    public double rating;
    public int user_ratings_total;
    public string website;
}

[System.Serializable]
public class Geometry
{
    public Location location;
}

[System.Serializable]
public class Location
{
    public double lat;
    public double lng;
}

[System.Serializable]
public class AddressComponent
{
    public string long_name;
    public string short_name;
    public List<string> types;
}
#endregion

#region DirectionAPI
/**
 * @brief DirectionAPI 에서 Json 파일들의 클래스를 정의
 */
[System.Serializable]
public class DirectionsResponse
{
    public List<GeocodedWaypoint> geocoded_waypoints;
    public List<Route> routes;
    public string status;
}

[System.Serializable]
public class GeocodedWaypoint
{
    public string geocoder_status;
    public string place_id;
    public List<string> types;
}

[System.Serializable]
public class Route
{
    public Bounds bounds;
    public string copyrights;
    public List<Leg> legs;
    public OverviewPolyline overview_polyline;
    public string summary;
    public List<string> warnings;
    public List<object> waypoint_order;
}

[System.Serializable]
public class Bounds
{
    public Northeast northeast;
    public Southwest southwest;
}

[System.Serializable]
public class Northeast
{
    public double lat;
    public double lng;
}

[System.Serializable]
public class Southwest
{
    public double lat;
    public double lng;
}

[System.Serializable]
public class Leg
{
    public ArrivalTime arrival_time;
    public DepartureTime departure_time;
    public Distance distance;
    public Duration duration;
    public string end_address;
    public EndLocation end_location;
    public string start_address;
    public StartLocation start_location;
    public List<Step> steps;
    public List<object> traffic_speed_entry;
    public List<object> via_waypoint;
}

[System.Serializable]
public class ArrivalTime
{
    public string text;
    public string time_zone;
    public int value;
}

[System.Serializable]
public class DepartureTime
{
    public string text;
    public string time_zone;
    public int value;
}

[System.Serializable]
public class Distance
{
    public string text;
    public int value;
}

[System.Serializable]
public class Duration
{
    public string text;
    public int value;
}

[System.Serializable]
public class EndLocation
{
    public double lat;
    public double lng;
}

[System.Serializable]
public class StartLocation
{
    public double lat;
    public double lng;
}

[System.Serializable]
public class Step
{
    public Distance distance;
    public Duration duration;
    public EndLocation end_location;
    public string html_instructions;
    public Polyline polyline;
    public StartLocation start_location;
    //public List<Step> steps;
    public string travel_mode;
    public TransitDetails transit_details;
}

[System.Serializable]
public class Polyline
{
    public string points;
}

[System.Serializable]
public class TransitDetails
{
    public ArrivalStop arrival_stop;
    public ArrivalTime arrival_time;
    public DepartureStop departure_stop;
    public DepartureTime departure_time;
    public string headsign;
    public int headway;
    public Line line;
    public int num_stops;
}

[System.Serializable]
public class ArrivalStop
{
    public Location location;
    public string name;
}

[System.Serializable]
public class DepartureStop
{
    public Location location;
    public string name;
}

[System.Serializable]
public class Line
{
    public List<Agency> agencies;
    public string color;
    public string name;
    public string short_name;
    public string text_color;
    public Vehicle vehicle;
}

[System.Serializable]
public class Agency
{
    public string name;
    public string url;
}

[System.Serializable]
public class Vehicle
{
    public string icon;
    public string name;
    public string type;
}

[System.Serializable]
public class OverviewPolyline
{
    public string points;
}
#endregion

#region SearchHistoryData
/** 
 * @brief 검색기록에 관련된 데이터 저장 */
[System.Serializable]
public class SearchHistoryData
{
    public List<SearchItem> searches = new List<SearchItem>();
}

[System.Serializable]
public class SearchItem
{
    public string searchText;
    public string placeId;
}
#endregion

#region Vector2d
/**
 * @brief double 타입 필드를 통해 Vector2를 사용하기 위한 클래스
 * Vector간 수학적 계산을 위해 연산자 오버로딩 사용
 */
public class Vector2d
{
    public double x;
    public double y;

    public static readonly Vector2d zero = new Vector2d(0.0, 0.0);

    public Vector2d(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    /* 연산자 오버로딩 */
    public static Vector2d operator * (Vector2d vector, double scalar)
    {
        return new Vector2d(vector.x * scalar, vector.y * scalar);
    }
    public static Vector2d operator + (Vector2d a, Vector2d b)
    {
        return new Vector2d(a.x + b.x, a.y + b.y);
    }

    public static Vector2d operator - (Vector2d a, Vector2d b)
    {
        return new Vector2d(a.x - b.x, a.y - b.y);
    }

    public Vector2d(Vector2 vector)
    {
        this.x = vector.x;
        this.y = vector.y;
    }
}
#endregion

#region Configuration ApiKey, BaseUrl
/**
 * @brief URL 및 APIKEY를 모든 스크립트에서 접근할 수 있게 해줌 
 */
public static class Configuration
{
    public static string ApiKey => "AIzaSyAy-uxK5GceTZKIqh3kwlhlzzFn8wlZga4";
    public static string BaseUrl => "https://maps.googleapis.com/maps/api/staticmap?";
}
#endregion

/**
 * @brief AR Navigation 에 필요한 데이터 및 Json 파싱
 */
public class NaviData : MonoBehaviour
{
    #region Variables
    public static NaviData Instance { get; private set; }  ///< NaviData.cs 싱글톤 패턴

    public string PlaceId { get; private set; } ///< Place_ID
    public string addressComponentsDescription { get; private set; } ///< 주소컴포넌트를 string으로 변환한 변수

    [Header("Location")]
    private bool _waitingForLocationService = false;  ///< 위치서비스 허용여부
    private LocationInfo _lastLocation;  ///< Android 위치 서비스

    /* 검색기록 */
    private SearchHistoryData _searchHistoryData = new SearchHistoryData(); ///< 검색기록 데이터 초기화

    public SearchHistoryData searchHistoryData ///< private 접근 가능하게 해줌
    {
        get { return _searchHistoryData; }
        set { _searchHistoryData = value; }
    }
    private string filePath; ///< 파일 경로

    /* 참조 */
    public PlaceAutocompleteResponse autocompleteResponse { get; set; } ///< Autocomplete 참조
    public PlaceDetailsResponse placeInfoResponse { get; set; } ///< PlaceDetails Location 참조
    public PlaceDetailsResponse placeDetailsResponse { get; set; } ///< PlaceDetails 의 상세정보 참조

    public Location destinationLocation { get; set; } ///< 목적지 위치 저장

    private int _zoom = 17; ///< Main 화면 지도 Zoom Level
    private int _routeZoom = 15; ///< 경로 탐색시 Zoom Level
    private int _naviZoom = 20; ///< 길안내시 ZoomLevel

    /* URL 전달 */
    public string directionStaticMapUrl { get; private set; } ///< 경로 안내를 위한 구글 API URL
    public string naviMapUrl { get; private set; } ///< 길안내를 위한 구글 API URL

    /**
     * @brief 장소 Place_Id를 저장
     * @param[in] placeId 장소 ID
     */
    public void SetPlaceId(string placeId)
    {
        PlaceId = placeId;
    }
    #endregion

    #region Awake, Start, Update
    /**
     * @brief searchHistory.json 으로 파일 저장, 싱글톤 패턴, 위치 서비스 허용
     * @details 파일 저장 = App이 실행되는 기기에 따라 다른 데이터 저장용 경로(여기선 searchHistory.json) 제공
     */
    void Awake()
    {
        StartCoroutine(StartLocationService());
        /* App이 실행되는 기기에 따라 다른 데이터 저장용 경로 제공
         * searchHistory.json 파일의 전체 경로를 생성 */
        filePath = Path.Combine(Application.persistentDataPath, "searchHistory.json");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Search History 
    /**
     * @brief 검색기록 추가
     * @details 해당 검색어를 placeId와 같이 리스트에 넣고 저장 Method 사용
     * @param[in] searchQuery 검색기록 이름 텍스트
     * @param[in] placeId 장소 ID
     */
    public void AddSearchToHistory(string searchQuery, string placeId)
    {
        if (!_searchHistoryData.searches.Any(item => item.searchText == searchQuery))
        {
            var newSearchItem = new SearchItem { searchText = searchQuery, placeId = placeId };
            _searchHistoryData.searches.Insert(0, newSearchItem);
            SaveSearchHistory();
        }
    }

    /**
     * @brief JsonData로 검색기록 저장
     * @details 파일을 Json 데이터로 저장
     */
    public void SaveSearchHistory()
    {
        string jsonData = JsonUtility.ToJson(_searchHistoryData);
        File.WriteAllText(filePath, jsonData);
    }

    /**
     * @brief 저장된 검색기록 로드
     * @details 기존에 저장된 기록들 (검색 버튼을 통해서 누른 장소들) 찾아서 UI로 보여줌
     */
    public void LoadSearchHistory()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            _searchHistoryData = JsonUtility.FromJson<SearchHistoryData>(json);
            UIController.Instance.DisplaySearchHistory(); 
        }
        else
        {
            Debug.Log("찾을수 없음 새로 만들어야 함");
            _searchHistoryData = new SearchHistoryData();
        }
    }
    #endregion

    #region PlaceAPI Autocomplete
    /**
     * @brief 현재 위치를 기반으로 검색어와 비슷한 장소들을 자동완성하고 그 값을 main_text,  place_id를 받아옴
     * @param[in] input 입력값(글자)
     */
    public IEnumerator GetPlaceAutocompleteData(string input)
    {

        double latitude = Input.location.lastData.latitude;
        double longitude = Input.location.lastData.longitude;
        int radius = 5000;

        string autoUrl = $"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={input}&location={latitude},{longitude}&radius={radius}&language=ko&key={Configuration.ApiKey}&components=country:kr";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(autoUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("AutoWebError : " + webRequest.error);
            }
            else
            {
                //Debug.Log("json" + webRequest.downloadHandler.text);
                UIController.Instance.UpdateButton(webRequest.downloadHandler.text);
            }
        }
    }
    #endregion

    #region PlaceAPI PlaceDetails
    /**
     * @brief 장소 ID 값을 이용해서 해당 장소의 위도, 경도, 주소, 이름 값 받아옴
     * @details PlaceAPI PlaceDetails 이용
     * @param[in] placeId 장소 ID
     */
    public IEnumerator FetchPlaceInfo(string placeId)
    {
        string detailsUrl = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields=geometry,formatted_address,name&key={Configuration.ApiKey}";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(detailsUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                //Debug.Log("Place Details : " + webRequest.downloadHandler.text);
                placeInfoResponse = JsonUtility.FromJson<PlaceDetailsResponse>(webRequest.downloadHandler.text);
                destinationLocation = placeInfoResponse.result.geometry.location;
                yield return MapView.Instance.UpdateDestination(destinationLocation);
            }
            else
            {
                Debug.LogError("DetailsWebError : " + webRequest.error);
            }
        }
    }

    /**
     * @brief 장소 ID 값을 이용해서 해당 장소의 상세정보를 가져옴
     * @details 가게 오픈 여부, 주소, 가게 번호, 웹사이트, 평점 등 제공, 특히 해당 주소는 string 형식으로 바꾸고 한국식 주소로 정렬 
     */
    public IEnumerator FetchPlaceDetails(string placeId)
    {
        string detailsInfoUrl = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}" +
                                $"&fields=address_components,business_status,formatted_phone_number,website,rating,user_ratings_total" +
                                $"&language=ko" +
                                $"&key={Configuration.ApiKey}";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(detailsInfoUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                //Debug.Log("Place Info : " + webRequest.downloadHandler.text);
                placeDetailsResponse = JsonUtility.FromJson<PlaceDetailsResponse>(webRequest.downloadHandler.text);
                var addressComponents = placeDetailsResponse.result.address_components
                    .Where(ac => ac.types.Contains("administrative_area_level_1") ||
                                 ac.types.Contains("locality") ||
                                 ac.types.Contains("sublocality_level_4") ||
                                 ac.types.Contains("premise") ||
                                 ac.types.Contains("postal_code"))
                    .OrderBy(ac => ac.types.Contains("postal_code") ? 5 :
                                   ac.types.Contains("premise") ? 4 :
                                   ac.types.Contains("sublocality_level_4") ? 3 :
                                   ac.types.Contains("locality") ? 2 :
                                   ac.types.Contains("administrative_area_level_1") ? 1 : 6)
                    .Select(ac => ac.long_name)
                    .ToList();
                addressComponentsDescription = string.Join(" ", addressComponents);
            }
            else
            {
                Debug.LogError("PlaceInfoError : " + webRequest.error);
            }
        }
    }

    #endregion

    #region DirectionAPI
    /**
     * @brief 출발지와 목적지 값을 받아와서 경로 표시
     * @param[in] origin 출발지(현재위치)
     * @param[in] destination 목적지 (목적지 설정한 장소)
     */
    public IEnumerator GetDirection(string origin, Location destinantion)
    {
        string dest = string.Format("{0},{1}", destinantion.lat, destinantion.lng);
        string directionUrl = $"https://maps.googleapis.com/maps/api/directions/json?origin={origin}&destination={dest}&mode=transit&key={Configuration.ApiKey}";
        using (UnityWebRequest www = UnityWebRequest.Get(directionUrl))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("DirectionAPI 오류" + www.error);
            }
            else
            {
                //Debug.Log("경로 : " +  www.downloadHandler.text);
                /* 해당 함수 불러옴 */
                ProcessDirections(www.downloadHandler.text);
            }
        }
    }
    
    /**
     * @brief PolyLine을 이용하여 경로 생성
     * @details DirectionAPI 에서 받은 PolyLine 들을 Decode 하여 Vector 목록으로 변환 후 각 좌표들을 문자열로 변환하고
     * 하나의 문자열로 결합
     */
    void ProcessDirections(string jsonData)
    {
        DirectionsResponse directionsResponse = JsonUtility.FromJson<DirectionsResponse>(jsonData);
        if (directionsResponse.routes.Count > 0)
        {
            string encodedPolyline = directionsResponse.routes[0].overview_polyline.points;
            List<Vector2d> pathPoints = DecodePolyline(encodedPolyline);
            string pathString = string.Join("|", pathPoints.Select(p => $"{p.x},{p.y}"));
            /* 경로 탐색시 사용되는 URL */
            directionStaticMapUrl =
                $"https://maps.googleapis.com/maps/api/staticmap?size=640x640" +
                $"&maptype=roadmap" +
                $"&zoom={_routeZoom}" +
                $"&path={pathString}" +
                $"&markers=color:purple|label:O|{Input.location.lastData.latitude},{Input.location.lastData.longitude}" +
                $"&markers=color:red|label:D|{destinationLocation.lat},{destinationLocation.lng}" +
                $"&key={Configuration.ApiKey}";
            /* 길안내시 사용되는 URL */
            naviMapUrl =
                $"&center={UnityWebRequest.UnEscapeURL(string.Format("{0}, {1}", Input.location.lastData.latitude, Input.location.lastData.longitude))}" +
                $"&size=640x640" +
                $"&zoom={_naviZoom}" +
                $"&scale=1" +
                $"&path={pathString}" +
                $"&markers=color:purple|label:U|{Input.location.lastData.latitude}, {Input.location.lastData.longitude}" +
                $"&markers=color:red|label:D|{destinationLocation.lat}, {destinationLocation.lng}" +
                $"&key={Configuration.ApiKey}";
        }
    }

    /**
     * @brief 인코딩된 PolyLine 문자열을 디코딩하여 Vector2d 목록으로 변환
     * @details 
     * @param[in] encodedPoints 지리적 포인트를 포함하는 인코딩된 문자열
     */
    List<Vector2d> DecodePolyline(string encodedPoints)
    {
        if (string.IsNullOrEmpty(encodedPoints)) return null;

        List<Vector2d> poly = new List<Vector2d>();
        int index = 0, len = encodedPoints.Length;
        int lat = 0, lng = 0;

        while (index < len)
        {
            int b, shift = 0, result = 0;
            do
            {
                b = encodedPoints[index++] - 63;
                result |= (b & 31) << shift;
                shift += 5;
            } while (b >= 0x20);
            int dlat = ((result & 1) != 0 ? -(result >> 1) : (result >> 1));
            lat += dlat;

            shift = 0;
            result = 0;
            do
            {
                b = encodedPoints[index++] - 63;
                result |= (b & 31) << shift;
                shift += 5;
            } while (b >= 0x20);
            int dlng = ((result & 1) != 0 ? -(result >> 1) : ( result >> 1));
            lng += dlng;

            Vector2d p = new Vector2d(Convert.ToSingle(lat) / 1E5f, Convert.ToSingle(lng) / 1E5f);
            poly.Add(p);
        }

        return poly;
    }
    #endregion

    #region Location Service
    /** 
     * @brief 사용자 위치 정보 동의 
     */
    private IEnumerator StartLocationService()
    {
        _waitingForLocationService = true;
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitForSeconds(5.0f);

            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
#if UNITY_ANDROID
                OpenAppSettings();
#endif
                Application.Quit(); // UI 이용해서 좀더 매끄럽게 다듬을 수 있음 (04.19)
            }
        }

        if (!Input.location.isEnabledByUser)
        {
            _waitingForLocationService = false;
            Debug.Log("GPS가 준비되지 않음");
            yield break;
        }

        Input.location.Start();

        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            yield return new WaitForSeconds(1);
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogError("위치 서비스를 사용할 수 없음");
            _waitingForLocationService = false;
            yield break;
        }
        else
        {
            _waitingForLocationService = false;
            Debug.Log("구글맵 불러오는중");
            //StartCoroutine(GetGoogleMap());
        }
#endif
    }

    /**
     * @brief 위치 접근 권한이 없는 경우 설정 페이지로 이동하여 권한을 활성화 하도록 요청
     */
#if UNITY_ANDROID
    void OpenAppSettings()
    {
        /* 권한 확인 거부시 false 반환 */
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            /* Android Java 클래스 */
            AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            /* 현재 실행중인 Android Activity */
            AndroidJavaObject currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            /* Package Manager */
            AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            /* 설정 페이지를 위한 Intent 생성 */
            AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);
            /* 호출 1 -> 사용자에게 권한 요청 */
            currentActivity.Call("startActivity", launchIntent);

            /* Uri 클래스 */
            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", Application.identifier, null);
            /* Intent 생성 */
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject);
            /* Intent 수정 */
            intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
            intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
            currentActivity.Call("startActivity", intentObject);
        }
    }
#endif

    #endregion
}

