using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    //refs

    [SerializeField] TextMeshProUGUI _timeScaleTMP = null;

    //settings
    [SerializeField] List<float> _timeScales = null;

    //state

    [SerializeField] int _timeIndex = 1;


    private void Awake()
    {
        Instance = this;

    }

    public void StartNewGame()
    {
        ActionController.Instance.ClearAllActions();
        FactionController.Instance.SetupFactions();
        FactionController.Instance.SetPlayerFaction(0);
        TileController.Instance.CreateNewWorld();
        TimeController.Instance.StartNewRun();
        _timeScaleTMP.text = _timeScales[_timeIndex].ToString();
    }

    public void IncreaseTimeScale()
    {
        _timeIndex++;
        _timeIndex = Mathf.Clamp(_timeIndex, 0, _timeScales.Count-1);

        Time.timeScale = _timeScales[_timeIndex];
        _timeScaleTMP.text = Time.timeScale.ToString();
    }

    public void DecreaseTimeScale()
    {
        _timeIndex--;
        _timeIndex = Mathf.Clamp(_timeIndex, 0, _timeScales.Count-1);

        Time.timeScale = _timeScales[_timeIndex];
        _timeScaleTMP.text = Time.timeScale.ToString();
    }
}
