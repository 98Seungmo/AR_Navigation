using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using Unity.XR.CoreUtils;

public class ScaleController : MonoBehaviour
{
    XROrigin _xrorigin;
    public Slider scaleSlider;

    private void Awake()
    {
        _xrorigin = GetComponent<XROrigin>();
    }

    private void Start()
    {
        scaleSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float arg0)
    {
        if (scaleSlider != null)
        {
            /* (1,1,1) 값을 나누어 슬라이더 핸들을 오른쪽으로 보내면, Scale 값은 적어진다. -> 3D 객체는 커진다. */
            _xrorigin.transform.localScale = Vector3.one / arg0;
        }
    }

    void Update()
    {

    }
}
