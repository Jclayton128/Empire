using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static TimeController Instance { get; private set; }

    //ref
    [SerializeField] TimeDriver _timeDriver = null;

    //settings

    [SerializeField] float _timePerTurn = 30f;

    //state
    float _timeInCurrentTurn = 0;
    float _timeFactor;
    int _currentTurn = 0;
    public int CurrentTurn => _currentTurn;
    bool _isInTurn = false;

    private void Awake()
    {
        Instance = this;
    }

    public void StartNewRun()
    {
        _currentTurn = 0;
        _timeDriver.SetTurn(CurrentTurn);
        _isInTurn = true;   
        StartNewTurn();
    }

    public void StartNewTurn()
    {
        _timeInCurrentTurn = 0;
        _timeFactor = 0;
        _timeDriver.SetTimeFactor(_timeFactor);
    }

    private void AdvanceTurn()
    {
        _currentTurn++;
        StartNewTurn();
        _timeDriver.SetTurn(CurrentTurn);
        FactionController.Instance.GatherProductionAtEndOfTurn();
    }

    private void Update()
    {
        if (!_isInTurn) return;

        _timeInCurrentTurn += Time.deltaTime;
        _timeFactor = _timeInCurrentTurn / _timePerTurn;
        if (_timeFactor >= 1)
        {
            AdvanceTurn();
        }
        else
        {
            _timeDriver.SetTimeFactor(_timeFactor);
        }
    }
}
