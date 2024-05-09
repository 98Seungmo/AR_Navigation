using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSpinner : MonoBehaviour
{
    public float rotationSpeed = 100f; // 회전 속도

    private void Update()
    {
        // 회전 속도에 따라 이미지를 시계방향으로 회전
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}