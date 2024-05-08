using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region Variables
    public static UIController Instance {  get; private set; }


    [Header("UI")]
    public GameObject searchPanel;
    public GameObject destinationPanel;
    public GameObject routePanel;
    public GameObject navigationPanel;
    public GameObject placeDetailsPanel;

    [Header("Auto Complete & Search History")]
    public TMP_InputField inputField; ///< 검색기록 InputField
    public GameObject buttonPrefabs; ///< 자동완성 검색어 프리팹
    public Transform autocompletePanel; ///< 자동완성 리스트 Panel
    public GameObject historyButtonPrefabs; ///< 검색기록 프리팹
    public Transform historyPanel; ///< 검색기록 리스트 Panel
    public GameObject historyBackground; ///< 검색기록 관련 Panel 
    public GameObject autocompleteBackground; ///< 자동완성 관련 Panel

    [Header("Place Details Text")]
    public TMP_Text placeName;
    public TMP_Text phoneNumber;
    public TMP_Text addressComponent;
    public TMP_Text businessStatus;
    public TMP_Text rating;
    public TMP_Text UserRatingsTotal;
    public TMP_Text website;


    private Coroutine _fetchPlaceInfoCoroutine;
    private Coroutine _fetchPlaceDetailsCoroutine;
    private Coroutine _updateDestinationCoroutine;

    private bool _isNaviPanel = false;

    public bool IsNaviPanel
    {
        get { return _isNaviPanel; }
        set { _isNaviPanel = value; }
    }
    #endregion

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

    void Start()
    {
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    void Update()
    {
    }

    /**
 * @brief InputField의 글자수에 따라 검색기록 및 자동완성 호출
 * @param[in] input InputField 값
 */
    public void OnInputValueChanged(string input)
    {
        if (input.Length == 0)
        {
            NaviData.Instance.LoadSearchHistory();
            autocompleteBackground.SetActive(false);
            historyBackground.SetActive(true);

        }
        else if (input.Length >= 2)
        {
            StartCoroutine(NaviData.Instance.GetPlaceAutocompleteData(input));
            historyBackground.SetActive(false);
            autocompleteBackground.SetActive(true);
        }
        else
        {
            ClearButton();
        }
    }

    /**
     * @brief 버튼 초기화 함수
     */
    void ClearButton()
    {
        foreach (Transform child in autocompletePanel)
        {
            Destroy(child.gameObject);
        }
    }

    #region UI Onclick Button

    public void ReturnLocation()
    {
        MapView.Instance.currentCenter = Vector2d.zero;
        StartCoroutine(MapView.Instance.GetGoogleMap());
    }
    public void StartFindDirection()
    {
        SetActiveDestinationPanel(false);
        SetActiveRoutePanel(true);
        StartCoroutine(CoroutineStartFindDirection());
    }
    IEnumerator CoroutineStartFindDirection()
    {
        string currentLocation = $"{Input.location.lastData.latitude},{Input.location.lastData.longitude}";
        yield return StartCoroutine(NaviData.Instance.GetDirection(currentLocation, NaviData.Instance.destinationLocation));
        yield return StartCoroutine(MapView.Instance.RouteMap(NaviData.Instance.directionStaticMapUrl));
    }

    public void OpenDestination()
    {
        SetActiveSearchPanel(false);
        SetActiveDestinationPanel(true);
        StartCoroutine(CoroutineOpenDestiantion());
    }
    IEnumerator CoroutineOpenDestiantion()
    {
        yield return _fetchPlaceInfoCoroutine = StartCoroutine(NaviData.Instance.FetchPlaceInfo(NaviData.Instance.PlaceId));
        yield return null;
        yield return _fetchPlaceDetailsCoroutine = StartCoroutine(NaviData.Instance.FetchPlaceDetails(NaviData.Instance.PlaceId));
        yield return null;
        yield return _updateDestinationCoroutine = StartCoroutine(MapView.Instance.UpdateDestination(MapView.Instance._currentLocation));
        yield return null;
    }

    public void StartNavigation()
    {
        SetActiveNavigationPanel(true);
        StartCoroutine(MapView.Instance.UseNavigation(NaviData.Instance.naviMapUrl));

        if (_isNaviPanel == true)
        {
            SetActiveRoutePanel(false);
        }
    }

    public void ReturnSearch()
    {
        if (_fetchPlaceInfoCoroutine != null)
        {
            StopCoroutine(_fetchPlaceInfoCoroutine);
            _fetchPlaceInfoCoroutine = null;
        }
        if (_updateDestinationCoroutine != null)
        {
            StopCoroutine(_updateDestinationCoroutine);
            _updateDestinationCoroutine = null;
        }
        if (_fetchPlaceDetailsCoroutine != null)
        {
            StopCoroutine(_fetchPlaceDetailsCoroutine);
            _fetchPlaceDetailsCoroutine = null;
        }
        SetActiveDestinationPanel(false);
        SetActiveSearchPanel(true);
    }

    public void ReturnHome()
    {
        StopCoroutine(MapView.Instance.UseNavigation(NaviData.Instance.naviMapUrl));
        SetActiveNavigationPanel(false);
        SetActiveSearchPanel(true);
    }

    public void CLickURL()
    {
        Application.OpenURL(NaviData.Instance.placeDetailsResponse.result.website);
    }
    #endregion

    #region Autocomplete Dynamic List & EventListener
    /**
     * @brief 자동완성 데이터를 버튼에 할당 및 동적으로 생성
     * @details InputField 에 입력시 자동완성 함수에 의해 나온 데이터들을 버튼에 할당 및 버튼 동적 생성후 
     * 버튼 Text에 표시 그리고 해당 버튼 클릭시 InputField에 해당 main_text 값 할당 및 place_id 데이터 보냄
     * @param[in] jsonResponse 자동완성 데이터 Json 파일
     */
    public void UpdateButton(string jsonResponse)
    {
        ClearButton();
        NaviData.Instance.autocompleteResponse = JsonUtility.FromJson<PlaceAutocompleteResponse>(jsonResponse);

        foreach (var prediction in NaviData.Instance.autocompleteResponse.predictions)
        {
            GameObject newButton = Instantiate(buttonPrefabs, autocompletePanel);
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();

            buttonText.text = prediction.structured_formatting.main_text;
            Button button = newButton.GetComponent<Button>();

            button.onClick.AddListener(() =>
            {
                inputField.text = prediction.structured_formatting.main_text;
                NaviData.Instance.SetPlaceId(prediction.place_id);
                Debug.Log("place_id" + prediction.place_id);
                NaviData.Instance.AddSearchToHistory(prediction.structured_formatting.main_text, prediction.place_id);
            });
        }
    }
    #endregion

    #region Search History Dynamic List & EventListener
    /**
     * @brief 검색기록 동적 생성 및 데이터 버튼에 할당
     * @details InputField 에서 아무것도 입력하지 않거나 백스페이스 입력시 검색을 했던 장소들 표시
     * 버튼 누르면 자동완성과 마찬가지로 InputField에 main_text 할당 및 place_id 저장
     */
    public void DisplaySearchHistory()
    {
        ClearHistoryDisplay();

        foreach (var historyItem in NaviData.Instance.searchHistoryData.searches)
        {
            GameObject newButton = Instantiate(historyButtonPrefabs, historyPanel);
            TextMeshProUGUI historyText = newButton.GetComponentInChildren<TextMeshProUGUI>();

            historyText.text = historyItem.searchText;
            Button button2 = newButton.GetComponent<Button>();
            button2.onClick.AddListener(() =>
            {
                inputField.text = historyItem.searchText;
                NaviData.Instance.SetPlaceId(historyItem.placeId);
            });

            Button button3 = newButton.transform.Find("Remove").GetComponent<Button>();
            button3.onClick.AddListener(() => DeleteSearchHistory(historyItem));
        }

    }

    /**
     * @brief 버튼으로 검색기록에서 해당 검색기록 삭제
     * @param[in] itemToDelete 삭제해야할 검색기록
     */
    private void DeleteSearchHistory(SearchItem itemToDelete)
    {
        if (NaviData.Instance.searchHistoryData.searches.Contains(itemToDelete))
        {
            NaviData.Instance.searchHistoryData.searches.Remove(itemToDelete);
            NaviData.Instance.SaveSearchHistory();
            DisplaySearchHistory();
        }
    }

    /**
     * @brief 만약 다른 검색어 입력시 검색기록 초기화 
     */
    private void ClearHistoryDisplay()
    {
        foreach (Transform child in historyPanel)
        {
            Destroy(child.gameObject);
        }
    }
    #endregion

    #region Panel SetActive
    public void SetActiveSearchPanel(bool isActive)
    {
        searchPanel.SetActive(isActive);
    }

    public void SetActiveDestinationPanel(bool isActive)
    {
        destinationPanel.SetActive(isActive);
    }

    public void SetActiveRoutePanel(bool isActive)
    {
        routePanel.SetActive(isActive);
    }

    public void SetActiveNavigationPanel(bool isActive)
    {
        navigationPanel.SetActive(isActive);
    }

    public void OpenPlaceDetailsPanel()
    {
        placeDetailsPanel.SetActive(true);
        /* 해당 텍스트 정보들 입력해야 함 */
        placeName.text = NaviData.Instance.placeInfoResponse.result.name;
        phoneNumber.text = NaviData.Instance.placeDetailsResponse.result.formatted_phone_number;
        addressComponent.text = NaviData.Instance.addressComponentsDescription;
        businessStatus.text = NaviData.Instance.placeDetailsResponse.result.business_status;
        rating.text = NaviData.Instance.placeDetailsResponse.result.rating.ToString();
        UserRatingsTotal.text = NaviData.Instance.placeDetailsResponse.result.user_ratings_total.ToString();
        website.text = NaviData.Instance.placeDetailsResponse.result.website;
    }

    public void ClosePlaceDetailsPanel()
    {
        placeDetailsPanel.SetActive(false);
    }
    #endregion
}
