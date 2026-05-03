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
    NodeHandler _nh;
    [SerializeField] float _initialDuration = 0;
    [SerializeField] bool _countsUp = true;
    [SerializeField] float _timeBuildup = 0;

    [SerializeField] ActionController.ActionTypes _assignedAction = ActionController.ActionTypes.Undefined;
    public ActionController.ActionTypes AssignedAction => _assignedAction;

    private void Awake()
    {
        _th = GetComponent<TileHandler>();
        _nh = GetComponent<NodeHandler>();
    }

    public void AssignAction(ActionController.ActionTypes action, float actionDuration, bool countsUp, Sprite actionIcon)
    {
        //can't have multiple actions in a hex, or overwrite existing actions.
        if (_assignedAction != ActionController.ActionTypes.Undefined) return;

        _assignedAction = action;
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
        if (_assignedAction == ActionController.ActionTypes.Undefined) return;

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
            _assignedAction = ActionController.ActionTypes.Undefined;
            _actionFillBar.fillAmount = 0;
            _actionIcon.enabled = false;
            _countsUp = true;
                    TileController.Instance.PushChangesFromTileUnderCursorChanged();
        }
    }

    private void ResolveAssignedActionAtStart()
    {
        switch (_assignedAction)
        {
            case ActionController.ActionTypes.Invest:
                _th.AttemptDefendTile();
                break;
        }

        TileController.Instance.PushChangesFromTileUnderCursorChanged();
    }

    private void ResolveAssignedActionAtEnd()
    {
        switch (_assignedAction)
        {
            case ActionController.ActionTypes.Attack:
                ActionController.Instance.ResolveAttackAttempt(_th);
                break;

            case ActionController.ActionTypes.Invest:
                _th.UndefendTile();
                _nh.HealAllDamagedNodes();
                break;

            case ActionController.ActionTypes.Extract:
                int amount = _th.HarvestNode();
                FactionController.Instance.AdjustResources(amount, FactionController.Instance.PlayerFaction);
                break;
        }


    }
}
