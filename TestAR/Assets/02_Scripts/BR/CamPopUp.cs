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
        // ��ư Ŭ�� �̺�Ʈ ����
        allowButton.onClick.AddListener(AllowCameraAccess);
        denyButton.onClick.AddListener(DenyCameraAccess);

        // ī�޶� ���� ������ ���� ���
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            // �˾� Ȱ��ȭ
            gameObject.SetActive(true);
        }
        else
        {
            // �˾� ��Ȱ��ȭ
            gameObject.SetActive(false);
        }
    }

    // ī�޶� ���� ��� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    void AllowCameraAccess()
    {
        // ī�޶� ���� ���� ��û
        Permission.RequestUserPermission(Permission.Camera);

        // �˾� ��Ȱ��ȭ
        gameObject.SetActive(false);
    }

    // ī�޶� ���� �ź� ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    void DenyCameraAccess()
    {
        // �� ����
        Application.Quit();
    }
}
