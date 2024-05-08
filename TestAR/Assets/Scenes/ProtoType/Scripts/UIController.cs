using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region Variables
    public static UIController Instance {  get; private set; } ///< UIController.cs 싱글톤 패턴


    [Header("UI")]
    public GameObject searchPanel; ///< search UI
    public GameObject destinationPanel; ///< destination UI
    public GameObject routePanel; ///< route UI
    public GameObject navigationPanel; ///<navigation UI
    public GameObject placeDetailsPanel; ///< place Details UI

    [Header("Auto Complete & Search History")]
    public TMP_InputField inputField; ///< 검색기록 InputField
    public GameObject buttonPrefabs; ///< 자동완성 검색어 프리팹
    public Transform autocompletePanel; ///< 자동완성 리스트 Panel
    public GameObject historyButtonPrefabs; ///< 검색기록 프리팹
    public Transform historyPanel; ///< 검색기록 리스트 Panel
    public GameObject historyBackground; ///< 검색기록 관련 Panel 
    public GameObject autocompleteBackground; ///< 자동완성 관련 Panel

    [Header("Place Details Text")]
    public TMP_Text placeName; ///< 장소 이름
    public TMP_Text phoneNumber; ///< 전화번호
    public TMP_Text addressComponent; ///< 주소
    public TMP_Text businessStatus; ///< 영업 여부
    public TMP_Text rating; ///< 평점
    public TMP_Text UserRatingsTotal; ///< 평점에 참여한 사용자 수
    public TMP_Text website; ///< 해당 장소의 웹사이트

    private Coroutine _fetchPlaceInfoCoroutine; ///< 장소에 대한 지리적 정보 관리 코루틴
    private Coroutine _fetchPlaceDetailsCoroutine; ///< 장소의 상세정보에 대한 관리 코루틴
    private Coroutine _updateDestinationCoroutine; ///< 목적지 업데이트 관리 코루틴

    private bool _isNaviPanel = false; ///< NaviPanel 활성화 여부
    /**
     * @brief 다른 스크립트에서 참조할 수 있도록 설정
     */
    public bool IsNaviPanel
    {
        get { return _isNaviPanel; }
        set { _isNaviPanel = value; }
    }
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

    /**
     * @brief Inputfield 입력에 따라 함수 실행
     */
    void Start()
    {
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }
    #endregion

    #region InputField Autocomplete
    /**
     * @brief InputField의 글자수에 따라 검색기록 및 자동완성 호출
     * @param[in] input InputField 값(글자)
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
    #endregion

    #region UI Onclick Button
    /* UI에 할당할 버튼들 */
    /**
     * @brief 지도를 현재 위치로 돌아가게 함
     */
    public void ReturnLocation()
    {
        MapView.Instance.currentCenter = Vector2d.zero;
        StartCoroutine(MapView.Instance.GetGoogleMap());
    }
    /**
     * @brief 목적지 이동 버튼
     */
    public void OpenDestination()
    {
        SetActiveSearchPanel(false);
        SetActiveDestinationPanel(true);
        StartCoroutine(CoroutineOpenDestiantion());
    }
    IEnumerator CoroutineOpenDestiantion()
    {
        /* 장소의 지리적 위치값 가져옴 */
        yield return _fetchPlaceInfoCoroutine = StartCoroutine(NaviData.Instance.FetchPlaceInfo(NaviData.Instance.PlaceId));
        yield return null;
        /* 장소의 상세정보 가져옴 */
        yield return _fetchPlaceDetailsCoroutine = StartCoroutine(NaviData.Instance.FetchPlaceDetails(NaviData.Instance.PlaceId));
        yield return null;
        /* 목적지 업데이트 */
        yield return _updateDestinationCoroutine = StartCoroutine(MapView.Instance.UpdateDestination(MapView.Instance._currentLocation));
        yield return null;
    }

    /**
     * @brief 경로 찾는 버튼
     */
    public void StartFindDirection()
    {
        SetActiveDestinationPanel(false);
        SetActiveRoutePanel(true);
        StartCoroutine(CoroutineStartFindDirection());
    }
    IEnumerator CoroutineStartFindDirection()
    {
        /* 현재 위치 */
        string currentLocation = $"{Input.location.lastData.latitude},{Input.location.lastData.longitude}";
        /* 경로 (PolyLine) 가져옴 */
        yield return StartCoroutine(NaviData.Instance.GetDirection(currentLocation, NaviData.Instance.destinationLocation));
        /* 지도에 경로 표시 */
        yield return StartCoroutine(MapView.Instance.RouteMap(NaviData.Instance.directionStaticMapUrl));
    }

    /**
     * @brief 길안내 시작 버튼
     * @details NavigationPanel이 활성화 되면 이전 RoutePanel 비활성화 (정보 제대로 전달받기 위함)
     */
    public void StartNavigation()
    {
        SetActiveNavigationPanel(true);
        /* RoutePanel 에서의 경로와 현재위치를 토대로 경로 설정 후 AR Navigation 시작 */
        StartCoroutine(MapView.Instance.UseNavigation(NaviData.Instance.naviMapUrl));

        if (_isNaviPanel == true)
        {
            SetActiveRoutePanel(false);
        }
    }

    /**
     * @brief 뒤로가기 버튼 (DestinationPanel -> SearchPanel)
     */
    public void ReturnSearch()
    {
        /* 코루틴 멈춤 */
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

    /**
     * @brief 뒤로가기 버튼 (NavigationPanel -> SearchPanel)
     */
    public void ReturnHome()
    {
        /* 코루틴 멈춤 */
        StopCoroutine(MapView.Instance.UseNavigation(NaviData.Instance.naviMapUrl));
        SetActiveNavigationPanel(false);
        SetActiveSearchPanel(true);
    }
    
    /**
     * @brief 장소에서 Website 의 링크 생성
     */
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
                //Debug.Log("place_id" + prediction.place_id);
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
    /**
     * @brief SearchPanel 활성화 여부
     * @param[in] isActive True/false
     */
    public void SetActiveSearchPanel(bool isActive)
    {
        searchPanel.SetActive(isActive);
    }

    /**
     * @brief DestinationPanel 활성화 여부
     * @param[in] isActive True/false
     */
    public void SetActiveDestinationPanel(bool isActive)
    {
        destinationPanel.SetActive(isActive);
    }

    /**
     * @brief RoutePanel 활성화 여부
     * @param[in] isActive True/false
     */
    public void SetActiveRoutePanel(bool isActive)
    {
        routePanel.SetActive(isActive);
    }

    /**
     * @brief NavigationPanel 활성화 여부
     * @param[in] isActive True/false
     */
    public void SetActiveNavigationPanel(bool isActive)
    {
        navigationPanel.SetActive(isActive);
    }

    /**
     * @brief 장소 상세보기 누를시 나오는 팝업창
     */
    public void OpenPlaceDetailsPanel()
    {
        placeDetailsPanel.SetActive(true);
        /* 텍스트 정보 */
        placeName.text = NaviData.Instance.placeInfoResponse.result.name; ///< 장소 이름
        phoneNumber.text = NaviData.Instance.placeDetailsResponse.result.formatted_phone_number; ///< 전화번호
        addressComponent.text = NaviData.Instance.addressComponentsDescription; ///< 주소
        businessStatus.text = NaviData.Instance.placeDetailsResponse.result.business_status; ///< 운영 여부
        rating.text = NaviData.Instance.placeDetailsResponse.result.rating.ToString(); ///< 평점
        UserRatingsTotal.text = NaviData.Instance.placeDetailsResponse.result.user_ratings_total.ToString(); ///< 평점에 참여한 사용자 수
        website.text = NaviData.Instance.placeDetailsResponse.result.website; ///< 웹사이트
    }

    /**
     * @brief 팝업창 닫기
     */
    public void ClosePlaceDetailsPanel()
    {
        placeDetailsPanel.SetActive(false);
    }
    #endregion
}
