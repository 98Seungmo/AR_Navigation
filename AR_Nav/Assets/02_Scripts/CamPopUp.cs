using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class CamPopUp : MonoBehaviour
{
    public Button allowButton;
    public Button denyButton;

    void Start()
    {
        // 버튼 클릭 이벤트 연결
        allowButton.onClick.AddListener(AllowCameraAccess);
        denyButton.onClick.AddListener(DenyCameraAccess);

        // 카메라 접근 권한이 없는 경우
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            // 팝업 활성화
            gameObject.SetActive(true);
        }
        else
        {
            // 팝업 비활성화
            gameObject.SetActive(false);
        }
    }

    // 카메라 접근 허용 버튼 클릭 시 호출되는 함수
    void AllowCameraAccess()
    {
        // 카메라 접근 권한 요청
        Permission.RequestUserPermission(Permission.Camera);

        // 팝업 비활성화
        gameObject.SetActive(false);
    }

    // 카메라 접근 거부 버튼 클릭 시 호출되는 함수
    void DenyCameraAccess()
    {
        // 앱 종료
        Application.Quit();
    }
}
