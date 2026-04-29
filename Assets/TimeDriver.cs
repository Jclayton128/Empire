using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeDriver : MonoBehaviour
{
    //ref

    [SerializeField] Slider _timeRemainingSlider = null;

    [SerializeField] TextMeshProUGUI _daysElapsedTMP = null;


    public void SetTimeFactor(float factor)
    {
        _timeRemainingSlider.value = 1- factor;
    }

    public void SetTurn(int turn)
    {
        _daysElapsedTMP.text = $"{turn}";
    }
}
