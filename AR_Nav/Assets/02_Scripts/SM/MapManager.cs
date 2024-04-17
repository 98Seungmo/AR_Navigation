using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public RawImage mapRawImage;
    public GoogleMapsAPI googleMapsAPI;

    [Header("맵 정보 설정")]
    public string strBaseURL = "https://maps.googleapis.com/maps/api/staticmap?";
    public double latitude = 37.7136;
    public double longitude = 126.7435;
    public int zoom = 14;
    public int mapWidth;
    public int mapHeight;
    public string APIKey;

    // 드래그 관련 변수
    private bool isDragging = false;
    private Vector2 dragStartPosition;

    void Start()
    {
        mapRawImage = GetComponent<RawImage>();
        StartCoroutine(LoadMap());
    }

    IEnumerator LoadMap()
    {
        string url = strBaseURL + "center=" + latitude + "," + longitude +
            "&zoom=" + zoom + "&size=" + mapWidth + "x" + mapHeight +
            "&key=" + APIKey;

        Debug.Log("URL =" + url);

        url = UnityWebRequest.UnEscapeURL(url);
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);

        yield return req.SendWebRequest();

        mapRawImage.texture = DownloadHandlerTexture.GetContent(req);

        // 터치 이벤트 리스너 등록
        mapRawImage.gameObject.AddComponent<EventTrigger>();
        EventTrigger trigger = mapRawImage.GetComponent<EventTrigger>();

        // 드래그 시작 이벤트 등록
        EventTrigger.Entry dragStartEntry = new EventTrigger.Entry();
        dragStartEntry.eventID = EventTriggerType.BeginDrag;
        dragStartEntry.callback.AddListener((eventData) => { OnPointerDown(); });
        trigger.triggers.Add(dragStartEntry);

        // 드래그 종료 이벤트 등록
        EventTrigger.Entry dragEndEntry = new EventTrigger.Entry();
        dragEndEntry.eventID = EventTriggerType.EndDrag;
        dragEndEntry.callback.AddListener((eventData) => { OnPointerUp(); });
        trigger.triggers.Add(dragEndEntry);
    }

    void Update()
    {
        // 줌 인/아웃 처리
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            // 터치 이동량에 따라 줌 인/아웃
            zoom -= (int)Touchscreen.current.primaryTouch.delta.ReadValue().y;
            StartCoroutine(LoadMap());
        }

        // 드래그 이동 처리
        if (isDragging && Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 currentTouchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            Vector2 dragDelta = currentTouchPosition - dragStartPosition;
            dragStartPosition = currentTouchPosition;

            // 맵 위치 조정
            latitude -= dragDelta.y * 0.001;
            longitude += dragDelta.x * 0.001;

            StartCoroutine(LoadMap());
        }
    }


    /// <summary>
    /// 드래그 시작 메서드
    /// </summary>
    public void OnPointerDown()
    {
        isDragging = true;
        dragStartPosition = Touchscreen.current.primaryTouch.position.ReadValue();
    }

    /// <summary>
    /// 드래그 종료 메서드
    /// </summary>
    public void OnPointerUp()
    {
        isDragging = false;
    }
}
