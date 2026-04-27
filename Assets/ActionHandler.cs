using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;


public class ActionHandler : MonoBehaviour
{

    //refs
    [SerializeField] Image _actionFillBar = null;
    [SerializeField] Image _actionIcon = null;

    //state
    TileHandler _th;
    [SerializeField] float _initialDuration = 0;
    [SerializeField] bool _countsUp = true;
    [SerializeField] float _timeBuildup = 0;

    [SerializeField] ActionController.ActionTypes _action = ActionController.ActionTypes.Undefined;

    private void Awake()
    {
        _th = GetComponent<TileHandler>();
    }

    public void AssignAction(ActionController.ActionTypes action, float actionDuration, bool countsUp, Sprite actionIcon)
    {
        _action = action;
        ResolveAssignedActionAtStart();

        _initialDuration = actionDuration;

        _actionIcon.sprite = actionIcon;
        _actionIcon.enabled = true;
        _countsUp = countsUp;
        if (!countsUp)
        {
            _timeBuildup = actionDuration;
        }
        else if (countsUp)
        {
            _timeBuildup = 0;
        }

        SetFillBar();
    }

    private void Update()
    {
        if (_action == ActionController.ActionTypes.Undefined) return;

        if (_countsUp)
        {
            _timeBuildup += Time.deltaTime;
        }
        else
        {
            _timeBuildup -= Time.deltaTime;
        }
        SetFillBar();
    }

    private void SetFillBar()
    {
        _actionFillBar.fillAmount = Mathf.Abs(_timeBuildup / _initialDuration);

        if ((_countsUp && (_timeBuildup/_initialDuration) >= 1f) ||
            (!_countsUp && (_timeBuildup / _initialDuration) <= 0))
        {
            //resolve action
            ResolveAssignedActionAtEnd();
            _action = ActionController.ActionTypes.Undefined;
            _actionFillBar.fillAmount = 0;
            _actionIcon.enabled = false;
            _countsUp = true;
        }
    }

    private void ResolveAssignedActionAtStart()
    {
        switch (_action)
        {
            case ActionController.ActionTypes.Defend:
                _th.AttemptDefendTile();
                break;
        }
    }

    private void ResolveAssignedActionAtEnd()
    {
        switch (_action)
        {
            case ActionController.ActionTypes.Attack:
                ActionController.Instance.ResolveAttackAttempt(_th);
                break;

            case ActionController.ActionTypes.Defend:
                _th.UndefendTile();
                break;
        }
    }
}
