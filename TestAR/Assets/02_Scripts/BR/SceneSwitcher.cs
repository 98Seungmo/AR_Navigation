using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    public string targetSceneName; // �̵��� ���� �̸��� �����ϴ� ����


    
    public void SwitchScene()
    {

        
        SceneManager.LoadSceneAsync(targetSceneName); // ������ ������ ��ȯ�ϴ� �Լ� ȣ��
    }
}

