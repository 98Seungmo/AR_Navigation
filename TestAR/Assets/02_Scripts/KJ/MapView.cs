using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MapView : MonoBehaviour
{
    #region Variables
    public static MapView Instance { get; private set; } ///< MapView.cs 싱글톤 패턴


    [Header("RawImage")]
    public RawImage staticMapImage; ///< 기본 static Map 
    public RawImage destinationMapImage; ///< 목적지 설정 지도 
    public RawImage routeMapImage; ///< 경로 설정 지도
    public RawImage useNavigation; ///< 길안내 지도

    [Header("Basic Zoom Level")]
    private int _zoom = 17; ///< 지도 Zoom = 17단계

    [Header("Min/Max Zoom Level")]
    private int _maxZoom = 18;  ///< 확대가능한 최대 zoom 레벨
    private int _minZoom = 15;  ///< 축소가능한 최소 zoom 레벨

    [Header("Drag & Zoom speed")]
    private double _orthoZoomSpeed = 0.05; ///< 2D 확대/축소 속도
    public RawImage MapTouchRange;  ///< 드래그, 확대/축소 허용 범위
    private RectTransform _mapRectTransform; ///< 터치 범위 설정

    public Location _currentLocation { get; private set; } ///< geometry 값

    public Vector2d currentCenter; ///< 현재 center 값

    [Header("Map Move")]
    private static int _zoomLevel; ///< 드래그 및 확대 축소시 Zoom Level
    private static double _circumference; ///< 둘레
    private static double _radius; ///< 반지름
    private static Vector2d _centre; ///< 중앙

    [Header("Mouse Position")]
    public RawImage rawImage; ///< 터치 제한할 범위
    public int imageSize = 640; ///< 지도 사이즈
    Vector2d shiftedCentre = new Vector2d(0, 0); ///< vector2d (0, 0) 으로 설정
    private Vector2d startLatLng; ///< 터치 시작 위도 경도
    private Vector2d endLatLng; ///< 터치 끝 위도 경도
    private Vector2d currentLatLng; ///< 터치 중 위도 경도

    #endregion

    #region Awake, Start, Update
    /**
     * @brief 싱글톤 패턴
     */
    void Awake()
    {
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
    private Coroutine _getgooglemap = null; ///< 구글맵 관리하는 코루틴


    /**
     * @brief 맵 터치 범위 정하기 위해 RectTransform 컴포넌트 할당
     *        MercatorProjection 적용
     *        구글맵 불러옴
     */
    void Start()
    {
        _mapRectTransform = MapTouchRange.GetComponent<RectTransform>();
        InitializeMercatorProjection();
        StartGoogleMap();
    }

    /**
     * @brief 드래그 및 줌 인아웃
     */
    void Update()
    {
        OnTouchDrag();
        OnTouchZoom();
    }
    #endregion

    #region Mercator
    /**
     * @brief Mercator 투영법 초기화 및 현재 위치를 지도에 맞게 설정
     */
    public void InitializeMercatorProjection()
    {
        MercatorProjection_Setting(_zoom);
        double currentLongitude = Input.location.lastData.longitude;
        double currentLatitude = Input.location.lastData.latitude;
        double x = GetXFromLongitude(currentLongitude);
        double y = GetYFromLatitude(currentLatitude);

        x = x - imageSize / 2;
        y = y - imageSize / 2;

        shiftedCentre = new Vector2d(x, y);
    }

    /**
     * @brief 지도의 확대 레벨에 따라 Mercator 투영의 설정 초기화
     * @param[in] zoomLevel 줌 레벨
     */
    public static void MercatorProjection_Setting(int zoomLevel)
    {
        _zoomLevel = zoomLevel;
        _circumference = 256 * Mathf.Pow(2, zoomLevel);

        _radius = (_circumference / (2 * Mathf.PI));
        _centre = new Vector2d(_circumference / 2, _circumference / 2);

    }

    /**
     * @brief 경도를 x 좌표로 변환
     * @param[in] longDegrees 경도
     */
    public static double GetXFromLongitude(double longDegrees)
    {
        double x = 0;
        double longInRadians = longDegrees * Mathf.PI / 180;

        x = _radius * longInRadians;
        x = _centre.x + x;

        return x;
    }

    /**
     * @brief x 좌표를 경도로 변환
     * @param[in] xValue x 값
     */
    public static double GetLongitudeFromX(double xValue)
    {
        double longitude = 0;

        xValue = xValue - _centre.x;

        longitude = xValue / _radius;

        longitude = longitude * 180 / Mathf.PI;

        return longitude;
    }

    /**
     * @brief 위도를 y 좌표로 변환
     * @param[in] latDegrees 위도
     */
    public static double GetYFromLatitude(double latDegrees)
    {
        double y = 0;
        double latInRadians = latDegrees * Mathf.PI / 180;

        double logVal = Math.Log(((1 + Math.Sin(latInRadians)) / (1 - Math.Sin(latInRadians))), Math.E);

        y = _radius * 0.5 * logVal;
        y = _centre.y - y;

        return y;
    }
    
    /**
     * @brief x 좌표를 위도로 변환
     * @param[in] yValue y 값
     */
    public static double GetLatitudeFromY(double yValue)
    {
        double latitude = 0;

        yValue = _centre.y - yValue;

        double lnvLog = yValue / (_radius * 0.5);

        lnvLog = Math.Pow(Math.E, lnvLog);

        latitude = Math.Asin((lnvLog - 1) / (lnvLog + 1));

        latitude = latitude * 180 / Math.PI;

        return latitude;

    }

    #endregion

    #region Drag & Zoom
    /**
     * @brief 드래그
     */
    public void OnTouchDrag() 
    {
        MercatorProjection_Setting(_zoom);
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            Vector2 localCursor;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_mapRectTransform, touch.position, null, out localCursor))
            {
                if (_mapRectTransform.rect.Contains(localCursor))
                {
                    Vector3 n = new Vector3(
                              (localCursor.x - _mapRectTransform.rect.min.x) / _mapRectTransform.rect.width * imageSize,
                               imageSize - ((localCursor.y - _mapRectTransform.rect.min.y) / _mapRectTransform.rect.height * imageSize), 0.0f);

                    currentLatLng = new Vector2d(GetLongitudeFromX(-n.x + shiftedCentre.x),
                                                 GetLatitudeFromY(-n.y + shiftedCentre.y));

                    /* 각 터치 마다 경도 위도값 구해서 (마지막 터치 - 처음 터치) 그 이동만큼 center 값에 더함*/
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            startLatLng = currentLatLng;
                            break;
                        case TouchPhase.Moved:
                            Vector2d intermDelta = currentLatLng - startLatLng;
                            break;
                        case TouchPhase.Ended:
                            endLatLng = currentLatLng;

                            Vector2d delta = endLatLng - startLatLng;
                            if (delta.x != 0 || delta.y != 0)
                            {
                                currentCenter += delta;
                                StartCoroutine(GetGoogleMap());
                            }
                            break;
                    }
                }
            }
        }
    }

    /**
     * @brief 확대 축소
     */
    public void OnTouchZoom()
    {
        MercatorProjection_Setting(_zoom);

        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (RectTransformUtility.RectangleContainsScreenPoint(_mapRectTransform, touchZero.position, null) &&
                RectTransformUtility.RectangleContainsScreenPoint(_mapRectTransform, touchOne.position, null))
            {
                if (_getgooglemap != null) return;

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                double prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                double touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                double deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                _zoom = Mathf.Clamp(_zoom - (int)(deltaMagnitudeDiff * _orthoZoomSpeed), _minZoom, _maxZoom);

                StartCoroutine(GetGoogleMap());
            }
        }
    }

    #endregion

    #region DrawGetStaticMap
    /**
     * @brief 처음 시작할때 currentCenter 값 초기화하고 맵을 불러옴 => 현재 위치의 지도가 불러와짐
     */
    public void StartGoogleMap()
    {
        currentCenter = Vector2d.zero;
        StartCoroutine(GetGoogleMap());
    }

    /**
     * @brief 구글 Map API의 URL를 이용해서 현재 위치에 기반한 구글지도 생성
     */
    public IEnumerator GetGoogleMap()
    {
        string markers = $"&markers=color:purple|label:U|{UnityWebRequest.UnEscapeURL(string.Format("{0}, {1}", Input.location.lastData.latitude, Input.location.lastData.longitude))}";

        var query =
            $"&center={UnityWebRequest.UnEscapeURL(string.Format("{0}, {1}", Input.location.lastData.latitude + currentCenter.y, Input.location.lastData.longitude + currentCenter.x))}" +
            $"&zoom={_zoom}" +
            $"&size=640x640" +
            $"&scale=1" +
            $"&maptype=roadmap" +
            markers +
            $"&key={Configuration.ApiKey}";

        //Log($"center 값 체크 : {currentCenter.x}, {currentCenter.y}");

        var www = UnityWebRequestTexture.GetTexture(Configuration.BaseUrl + query);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("www error" + www.error);
            Debug.LogError(Configuration.BaseUrl + query);
        }
        else
        {
            staticMapImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
        _getgooglemap = null;
        yield break;
    }
    #endregion

    #region DrawDestination
    /**
     * @brief PlaceAPI로 geometry 값 받아서 지도와 마커로 표시
     * @param[in] location 해당 장소의 geometry(lat, lng)값
     */
    public IEnumerator UpdateDestination(Location location)
    {
        _currentLocation = location;

        string markers = $"&markers=color:red|label:D|{UnityWebRequest.UnEscapeURL(string.Format("{0},{1}", location.lat, location.lng))}";

        var query =
            $"&center={UnityWebRequest.UnEscapeURL(string.Format("{0},{1}", location.lat, location.lng))}" +
            $"&zoom={_zoom}" +
            $"&size=640x640" +
            $"&scale=1" +
            $"&maptype=roadmap" +
            markers +
            $"&key={Configuration.ApiKey}";

        var www = UnityWebRequestTexture.GetTexture(Configuration.BaseUrl + query);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("www 에러" + www.error);
            //Debug.Log(Configuration.BaseUrl + query);
        }
        else
        {
            destinationMapImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }

        yield break;
    }
    #endregion

    #region DrawFindDirectionMap
    /**
     * @brief 경로를 찾은 데이터를 지도로 표시
     * @param[in] url 해당 구글 API URL
     */
    public IEnumerator RouteMap(string url)
    {
        var www = UnityWebRequestTexture.GetTexture(url);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("www 에러" + www.error);
            //Debug.Log(url);
        }
        else
        {
            routeMapImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
    #endregion

    #region DrawNavigation
    /**
     * @brief 길안내시 나오는 Google Static Map
     * @param[in] url 해당 구글 API URL
     */
    public IEnumerator UseNavigation(string url)
    {
        var www = UnityWebRequestTexture.GetTexture(Configuration.BaseUrl + url);
        
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            //Debug.Log(url);
            UIController.Instance.IsNaviPanel = false;
        }
        else
        {
            UIController.Instance.IsNaviPanel = true;
            useNavigation.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
    #endregion

}
