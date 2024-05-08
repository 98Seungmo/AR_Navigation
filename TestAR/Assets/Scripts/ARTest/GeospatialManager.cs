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
    private TextMeshProUGUI _geospatialStatusText; ///< �پ��� ������ ���� �ؽ�Ʈ UI �� ������Ʈ

    [SerializeField]
    private AREarthManager _earthManager; ///< AREarthManager

    [SerializeField]
    private ARCoreExtensions _arCoreExtensions; ///< ARCoreExtensions 

    private bool _waitingForLocationService = false; ///< ������� ��ġ ���񽺰� ���۵ǰ� ��ġ ������ �޾ƿ��� ���� ��� �������� Ȯ��

    private Coroutine _locationServiceLauncher; ///< �ڷ�ƾ

    /**
     * @brief �������� 60���� �������Ѽ� �����ϰ� ����, �ε巯�� �ִϸ��̼ǰ� �������̽� ��ȣ�ۿ��� ����
     */
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    /**
     * @brief AR ���� ���¿� �������� ����� ���� ���� Ȯ�� �� �����Ǹ� �ش� ���� �Ÿ� ���� ��� Ȱ��ȭ
     */
    void Update()
    {
        /* �ش� ��Ȳ�� ���� �ߴ� */
        if (!Debug.isDebugBuild || _earthManager == null) return;
        /* �ش� ��Ȳ�� ���� �ߴ� (AR ������ �ʱ�ȭ ���̰ų� ���� ���� �ƴϸ�) */
        if (ARSession.state != ARSessionState.SessionInitializing && ARSession.state != ARSessionState.SessionTracking) return;

        /* ��� ���� �Ǵ��� Ȯ���� ������ �ش� ��� Ȱ��ȭ */
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
        /* AR ȯ�濡�� ������� ���� �������� ��ġ�� ���� ������ ��� ���� 
         * ���� ���¿� ���� ���� ���¸� Ȯ���ؼ� ���������� ��ġ�� ������ ����
         * �������� ��� _earthManager �κ��� pose �� ������
         * �ƴ� ��� ���ο� geospatialPose ��ü�� ���� */
        var pose = _earthManager.EarthState == EarthState.Enabled &&
            _earthManager.EarthTrackingState == TrackingState.Tracking ? _earthManager.CameraGeospatialPose : new GeospatialPose();

        var supported = _earthManager.IsGeospatialModeSupported(GeospatialMode.Enabled);

        /* AR ������ ����, ��ġ ���� ����, �������� ��� ���� ����, ���� ������ ���¿� ���� ����
         * �׸��� ������� �������� ��ġ(����, �浵), ��, ��Ȯ��, ���⼺�� ������ �پ��� ���� �� �����ؼ� UI �� ������ */
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
     * @brief ������� ��ġ���񽺸� �����ϴ� ������ ����
     * @detail �ȵ���̵� ����ڿ��� ���� ��ġ ������ ��û�ϰ� ����ڰ� Ȱ��ȭ���� ������ �ߴ�
     * Ȱ��ȭ�� ��� Input.location.Start(); ȣ���Ͽ� ��ġ ���� ����
     * �� ������ ���� ���� ������� �ǽð� ��ġ �����͸� �����ϰ� ���� ����
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
