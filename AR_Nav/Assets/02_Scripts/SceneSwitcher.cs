using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    public string targetSceneName; // �̵��� ���� �̸��� �����ϴ� ����


    //public string sceneName; 
    public void SwitchScene()
    {

        //targetSceneName = "BR_UI 5(Journey Information)";
        SceneManager.LoadSceneAsync(targetSceneName); // ������ ������ ��ȯ�ϴ� �Լ� ȣ��
    }
}

