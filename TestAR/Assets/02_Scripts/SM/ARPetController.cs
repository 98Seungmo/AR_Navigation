using UnityEngine;
using System.Collections;

public class ARPetController : MonoBehaviour
{
    private Animator animator; // 애니메이터
    private bool isMoving = false; // 캐릭터가 움직이고 있는지 여부를 나타내는 변수
    private float lat;
    private float lon;
    private float currentLat;
    private float currentLon;

    void Start()
    {
        animator = GetComponent<Animator>();
        // 시작할 때 초기 위치 기록
        lat = Input.location.lastData.latitude;
        lon = Input.location.lastData.longitude;
    }

    void Update()
    {
        // 현재 위치 업데이트
        StartCoroutine(PositionUpdate());

        // 현재 위치와 마지막 위치가 다른지 확인하여 움직임 여부 판단
        if (Mathf.Abs(lon - currentLon) > 0.000005 || Mathf.Abs(lat - currentLat) > 0.000005)
        {
            // 캐릭터가 이동 중일 때
            isMoving = true;
        }
        else
        {
            // 캐릭터가 이동 중이 아닐 때
            isMoving = false;
        }

        // 움직임 상태에 따라 애니메이터 업데이트
        UpdateAnimator();

        // 현재 위치를 마지막 위치로 업데이트
        lat = currentLat;
        lon = currentLon;
    }

    void UpdateAnimator()
    {
        // 움직임 상태에 따라 애니메이터 매개변수 업데이트
        animator.SetBool("IsMoving", isMoving);
    }

    IEnumerator PositionUpdate()
    {
        currentLat = Input.location.lastData.latitude;
        currentLon = Input.location.lastData.longitude;
        yield return new WaitForSeconds(0.1f);
    }
}
