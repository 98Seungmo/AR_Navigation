using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    public string targetSceneName; // 이동할 씬의 이름을 저장하는 변수


    
    public void SwitchScene()
    {

        
        SceneManager.LoadSceneAsync(targetSceneName); // 지정된 씬으로 전환하는 함수 호출
    }
}

