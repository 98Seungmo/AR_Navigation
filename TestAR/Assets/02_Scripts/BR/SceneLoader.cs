using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// 게임오프젝트 인스펙터 창에 전환할 씬의 이름을 넣도록 지정
    /// 로딩타임 간격
    /// </summary>
    public string nextSceneName = "HomeScene";
    public float loadingTime = 5f;

    void Start()
    {
        // Invoke 함수를 사용하여 일정 시간 후에 LoadNextScene 함수를 호출합니다.
        Invoke("LoadNextScene", loadingTime);
    }

    void LoadNextScene()
    {
        // 다음 씬으로 전환합니다.
        SceneManager.LoadScene(nextSceneName);
    }
}
