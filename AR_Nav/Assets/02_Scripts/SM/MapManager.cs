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

    [Header("�� ���� ����")]
    public string strBaseURL = "https://maps.googleapis.com/maps/api/staticmap?";
    public double latitude = 37.7136;
    public double longitude = 126.7435;
    public int zoom = 14;
    public int mapWidth;
    public int mapHeight;
    public string APIKey;

    // �巡�� ���� ����
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

        // ��ġ �̺�Ʈ ������ ���
        mapRawImage.gameObject.AddComponent<EventTrigger>();
        EventTrigger trigger = mapRawImage.GetComponent<EventTrigger>();

        // �巡�� ���� �̺�Ʈ ���
        EventTrigger.Entry dragStartEntry = new EventTrigger.Entry();
        dragStartEntry.eventID = EventTriggerType.BeginDrag;
        dragStartEntry.callback.AddListener((eventData) => { OnPointerDown(); });
        trigger.triggers.Add(dragStartEntry);

        // �巡�� ���� �̺�Ʈ ���
        EventTrigger.Entry dragEndEntry = new EventTrigger.Entry();
        dragEndEntry.eventID = EventTriggerType.EndDrag;
        dragEndEntry.callback.AddListener((eventData) => { OnPointerUp(); });
        trigger.triggers.Add(dragEndEntry);
    }

    void Update()
    {
        // �� ��/�ƿ� ó��
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            // ��ġ �̵����� ���� �� ��/�ƿ�
            zoom -= (int)Touchscreen.current.primaryTouch.delta.ReadValue().y;
            StartCoroutine(LoadMap());
        }

        // �巡�� �̵� ó��
        if (isDragging && Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 currentTouchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            Vector2 dragDelta = currentTouchPosition - dragStartPosition;
            dragStartPosition = currentTouchPosition;

            // �� ��ġ ����
            latitude -= dragDelta.y * 0.001;
            longitude += dragDelta.x * 0.001;

            StartCoroutine(LoadMap());
        }
    }


    /// <summary>
    /// �巡�� ���� �޼���
    /// </summary>
    public void OnPointerDown()
    {
        isDragging = true;
        dragStartPosition = Touchscreen.current.primaryTouch.position.ReadValue();
    }

    /// <summary>
    /// �巡�� ���� �޼���
    /// </summary>
    public void OnPointerUp()
    {
        isDragging = false;
    }
}
