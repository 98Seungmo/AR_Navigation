using Google.XR.ARCoreExtensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GeospatialManager : MonoBehaviour
{
    [Header("Core Features")]
    [SerializeField]
    private TextMeshProUGUI _geospatialStatusText; ///< 다양한 정보를 담은 텍스트 UI 에 업데이트

    [SerializeField]
    private AREarthManager _earthManager; ///< AREarthManager

    [SerializeField]
    private ARCoreExtensions _arCoreExtensions; ///< ARCoreExtensions 

    private bool _waitingForLocationService = false; ///< 사용자의 위치 서비스가 시작되고 위치 정보를 받아오는 동안 대기 상태인지 확인

    private Coroutine _locationServiceLauncher; ///< 코루틴

    /**
     * @brief 프레임을 60으로 고정시켜서 실행하게 만듬, 부드러운 애니메이션과 인터페이스 상호작용을 위해
     */
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    /**
     * @brief AR 세션 상태와 지리공간 모드의 지원 여부 확인 후 지원되면 해당 모드와 거리 추적 모드 활성화
     */
    void Update()
    {
        /* 해당 상황시 실행 중단 */
        if (!Debug.isDebugBuild || _earthManager == null) return;
        /* 해당 상황시 실행 중단 (AR 세션이 초기화 중이거나 추적 중이 아니면) */
        if (ARSession.state != ARSessionState.SessionInitializing && ARSession.state != ARSessionState.SessionTracking) return;

        /* 모드 지원 되는지 확인이 끝나면 해당 모드 활성화 */
        var featureSupport = _earthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);

        switch (featureSupport)
        {
            case FeatureSupported.Unknown:
                break;
            case FeatureSupported.Unsupported:
                break;

            case FeatureSupported.Supported:
                if (_arCoreExtensions.ARCoreExtensionsConfig.GeospatialMode == GeospatialMode.Disabled)
                {
                    _arCoreExtensions.ARCoreExtensionsConfig.GeospatialMode = GeospatialMode.Enabled;

                    _arCoreExtensions.ARCoreExtensionsConfig.StreetscapeGeometryMode = StreetscapeGeometryMode.Enabled;
                }
                break;

            default:
                break;
        }
        /* AR 환경에서 사용자의 현재 지리공간 위치와 방향 정보를 얻기 위함 
         * 지구 상태와 지구 추적 상태를 확인해서 지리공간의 위치와 방향을 결정
         * 추적중인 경우 _earthManager 로부터 pose 를 가져옴
         * 아닌 경우 새로운 geospatialPose 객체를 생성 */
        var pose = _earthManager.EarthState == EarthState.Enabled &&
            _earthManager.EarthTrackingState == TrackingState.Tracking ? _earthManager.CameraGeospatialPose : new GeospatialPose();

        var supported = _earthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);

        /* AR 세션의 상태, 위치 서비스 상태, 지리공간 모드 지원 여부, 현재 지구의 상태와 추적 상태
         * 그리고 사용자의 지리공간 위치(위도, 경도), 고도, 정확도, 방향성을 포함한 다양한 정보 를 포함해서 UI 에 보여줌 */
        if (_geospatialStatusText != null)
        {
            _geospatialStatusText.text =
                $"SessionState : {ARSession.state}\n" +
                $"LocationServiceStatus : {Input.location.status}\n" +
                $"FeatureSupported : {supported}\n" +
                $"EarthState : {_earthManager.EarthState}\n" +
                $"EarthTrackingState : {_earthManager.EarthTrackingState}\n" +
                $"LAT/LON : {pose.Latitude:F6}, {pose.Longitude:F6}\n" +
                $"HorizontalAcc : {pose.HorizontalAccuracy:F6}\n" +
                $"ALT : {pose.Altitude:F2}\n" +
                $"VerticalAcc : {pose.VerticalAccuracy:F2}\n" +
                $"EunRotation : {pose.EunRotation:F2}\n" +
                $"OrientationYawAcc : {pose.OrientationYawAccuracy:F2}\n";
        }
    }

    private void OnEnable()
    {
        _locationServiceLauncher = StartCoroutine(StartLocationService());
    }

    private void OnDisable()
    {
        if(_locationServiceLauncher != null)
            StopCoroutine(_locationServiceLauncher);

        _locationServiceLauncher = null;
        Input.location.Stop();
    }

    /**
     * @brief 사용자의 위치서비스를 시작하는 과정을 관리
     * @detail 안드로이드 사용자에게 정밀 위치 권한을 요청하고 사용자가 활성화하지 않으면 중단
     * 활성화된 경우 Input.location.Start(); 호출하여 위치 추적 시작
     * 이 과정을 통해 앱은 사용자의 실시간 위치 데이터를 안전하게 접근 가능
     */
    private IEnumerator StartLocationService()
    {
        _waitingForLocationService = true;
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitForSeconds(3.0f);
        }
#endif
        
        if (!Input.location.isEnabledByUser)
        {
            _waitingForLocationService = false;
            yield break;
        }

        Input.location.Start();

        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            yield return null;
        }

        _waitingForLocationService = false;
        if (Input.location.status != LocationServiceStatus.Running)
        {
            Input.location.Stop();
        }
    }
}
