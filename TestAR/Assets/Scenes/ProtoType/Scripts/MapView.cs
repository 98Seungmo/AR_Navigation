using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MapView : MonoBehaviour
{
    #region Variables
    public static MapView Instance { get; private set; }


    [Header("RawImage")]
    public RawImage staticMapImage;
    public RawImage destinationMapImage;
    public RawImage routeMapImage;
    public RawImage useNavigation;

    [Header("Basic Zoom Level")]
    private int _zoom = 17; ///< ���� Zoom = 17�ܰ�

    [Header("Min/Max Zoom Level")]
    private int _maxZoom = 18;  ///< Ȯ�밡���� �ִ� zoom ����
    private int _minZoom = 15;  ///< ��Ұ����� �ּ� zoom ����

    [Header("Drag & Zoom speed")]
    private double _orthoZoomSpeed = 0.05; ///< 2D Ȯ��/��� �ӵ�
    public RawImage MapTouchRange;  ///< �巡��, Ȯ��/��� ��� ����
    private RectTransform _mapRectTransform;

    public Location _currentLocation { get; private set; } ///< geometry ��

    private Vector2d _previousDistance;
    public Vector2d currentCenter;

    [Header("Map Move")]
    private static int _zoomLevel;
    private static double _circumference;
    private static double _radius;
    private static Vector2d _centre;
    private static bool _mplslnit = false;

    [Header("Mouse Position")]
    public RawImage rawImage;
    private Vector3 touchPosition;
    public int imageSize = 640;
    Vector2d shiftedCentre = new Vector2d(0, 0);
    private Vector2d startLatLng;
    private Vector2d endLatLng;
    private Vector2d currentLatLng;

    #endregion

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
    private Coroutine _getgooglemap = null;


    /**
     * @brief �� ��ġ ���� ���ϱ� ���� RectTransform ������Ʈ �Ҵ�, 
     */
    void Start()
    {
        _mapRectTransform = MapTouchRange.GetComponent<RectTransform>();
        InitializeMercatorProjection();
        StartGoogleMap();
    }

    void Update()
    {
        TestDrag();
        OnTouchZoom();
    }

    #region Mercator
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

    public static void MercatorProjection_Setting(int zoomLevel)
    {
        _zoomLevel = zoomLevel;
        _circumference = 256 * Mathf.Pow(2, zoomLevel);

        _radius = (_circumference / (2 * Mathf.PI));
        _centre = new Vector2d(_circumference / 2, _circumference / 2);

    }

    public static double GetXFromLongitude(double longDegrees)
    {
        double x = 0;
        double longInRadians = longDegrees * Mathf.PI / 180;

        x = _radius * longInRadians;
        x = _centre.x + x;

        return x;
    }

    public static double GetLongitudeFromX(double xValue)
    {
        double longitude = 0;

        xValue = xValue - _centre.x;

        longitude = xValue / _radius;

        longitude = longitude * 180 / Mathf.PI;

        return longitude;
    }

    public static double GetYFromLatitude(double latDegrees)
    {
        double y = 0;
        double latInRadians = latDegrees * Mathf.PI / 180;

        double logVal = Math.Log(((1 + Math.Sin(latInRadians)) / (1 - Math.Sin(latInRadians))), Math.E);

        y = _radius * 0.5 * logVal;
        y = _centre.y - y;

        return y;
    }
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
    public void TestDrag() 
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
                    Debug.Log("��ġ ��ġ : " + n);

                    currentLatLng = new Vector2d(GetLongitudeFromX(-n.x + shiftedCentre.x),
                                                 GetLatitudeFromY(-n.y + shiftedCentre.y));
                    Debug.Log($"currentLatLng ���� �� " + currentLatLng.y);

                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            startLatLng = currentLatLng;
                            Debug.Log($"�浵 : {startLatLng.x}, ���� : {startLatLng.y}");
                            break;
                        case TouchPhase.Moved:
                            Vector2d intermDelta = currentLatLng - startLatLng;
                            Debug.Log($"�̵� �� �浵 : {intermDelta.x}, ���� : {intermDelta.y}");
                            break;
                        case TouchPhase.Ended:
                            endLatLng = currentLatLng;

                            Vector2d delta = endLatLng - startLatLng;
                            Debug.Log($"Delta : �浵 {delta.x}, ���� {delta.y}");
                            if (delta.x != 0 || delta.y != 0)
                            {
                                Debug.Log($"�� �̵� �Ÿ� : �浵 {delta.x}, ���� {delta.y}");
                                Debug.Log($"currentCenterX : {currentCenter.x},currentCenterY : {currentCenter.y}");
                                currentCenter += delta;
                                StartCoroutine(GetGoogleMap());
                            }
                            break;
                    }
                }
            }
        }
    }

    /* Ȯ�� ��� */
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

    public void StartGoogleMap()
    {
        currentCenter = Vector2d.zero;
        StartCoroutine(GetGoogleMap());
    }

    /**
     * @brief ���� Map API�� URL�� �̿��ؼ� ���� ��ġ�� ����� �������� ����
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

        Debug.Log($"center �� üũ : {currentCenter.x}, {currentCenter.y}");

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
     * @brief PlaceAPI�� geometry �� �޾Ƽ� ������ ��Ŀ�� ǥ��
     * @param[in] location �ش� ����� geometry(lat, lng)��
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
            Debug.LogError("www ����" + www.error);
            Debug.Log(Configuration.BaseUrl + query);
        }
        else
        {
            destinationMapImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }

        yield break;
    }
    #endregion

    #region DrawFindDirectionMap

    public IEnumerator RouteMap(string url)
    {
        var www = UnityWebRequestTexture.GetTexture(url);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("www ����" + www.error);
            Debug.Log(url);
        }
        else
        {
            routeMapImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }

    }
    #endregion

    #region DrawNavigation
    public IEnumerator UseNavigation(string url)
    {
        var www = UnityWebRequestTexture.GetTexture(Configuration.BaseUrl + url);
        
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(url);
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
