using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicroHandler : MonoBehaviour
{

    //settings

    [SerializeField] GameObject _bulletPrefab = null;


    //state
    [SerializeField] ActionController.ActionTypes _actionType = ActionController.ActionTypes.Undefined;
    [SerializeField] float _timeRemaining;
    List<GameObject> _spawnedObjects = new List<GameObject>();
    Vector3 _sourcePos;
    Vector3 _targetPos;
    bool _isRunning = false;

    public void StartMicro(ActionController.ActionTypes actionToDepict, Vector3 sourcePos, Vector3 targetPos, float duration)
    {
        _actionType = actionToDepict;
        _sourcePos = sourcePos;
        _targetPos = targetPos;
        _timeRemaining = duration;
        _isRunning = true;
    }

    private void Update()
    {
        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining < 0 )
        {
            _isRunning = false;
        }

        if (_isRunning)
        {
            UpdateAction();
        }
    }

    private void UpdateAction()
    {
        switch (_actionType)
        {
            case ActionController.ActionTypes.Attack:
                UpdateDepictAttack();
                break;
        }
    }

    private void UpdateDepictAttack()
    {
        
    }
}
