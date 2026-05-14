using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public interface ActionCommander
{
    void CompleteAction(ActionHandler completedAction);
}

public class ActionHandler : MonoBehaviour
{

    //refs
    [SerializeField] Image _actionFillBar = null;
    [SerializeField] Image _actionIcon = null;

    //state
    TileHandler _targetTile;
    public TileHandler TargetTile => _targetTile;
    int _attemptingFaction;
    public int AttemptingFaction => _attemptingFaction; 

    float _initialDuration = 0;
    bool _countsUp = true;
    float _timeBuildup = 0;

    [SerializeField] ActionController.ActionTypes _assignedAction = ActionController.ActionTypes.Undefined;
    public ActionController.ActionTypes AssignedAction => _assignedAction;
    ActionCommander _actionCommander;

    public void AssignAction(ActionController.ActionTypes action, TileHandler targetTile, int attemptingFaction,
        float actionDuration, bool countsUp, Sprite actionIcon, ActionCommander actionCommander)
    {
        //can't have multiple actions in a hex, or overwrite existing actions.
        if (_assignedAction != ActionController.ActionTypes.Undefined) return;

        _actionCommander = actionCommander;

        _assignedAction = action;
        _targetTile = targetTile;
        _attemptingFaction = attemptingFaction;

        transform.position = _targetTile.transform.position;

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

        //Check to see if Invest is no longer relevant due to target tile changing ownership.
        if ((_assignedAction == ActionController.ActionTypes.Invest)
            && _targetTile.FactionIndex != _attemptingFaction)
        {
            ActionController.Instance.ResolveInvestCompletion(_targetTile, _attemptingFaction);
            _actionCommander.CompleteAction(this);
            Destroy(this.gameObject);
            return;
        }

        //Check to see if Extract is no longer relevant due to target tile changing ownership.
        if ((_assignedAction == ActionController.ActionTypes.Extract)
            && _targetTile.FactionIndex != _attemptingFaction)
        {    
            _actionCommander.CompleteAction(this);
            Destroy(this.gameObject);
            return;
        }


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

            _actionCommander.CompleteAction(this);
            Destroy(this.gameObject);
        }
    }

    private void ResolveAssignedActionAtStart()
    {
        switch (_assignedAction)
        {
            case ActionController.ActionTypes.Attack:
                //var micro = MicroController.Instance.SpawnMicro();
                //micro.StartMicro(ActionController.ActionTypes.Attack, )
                break;

            case ActionController.ActionTypes.Invest:
                ActionController.Instance.ResolveInvestStart(_targetTile, _attemptingFaction);
                //_th.AttemptDefendTile();
                //_th.TileInfluenceHandler.AddInfluence(_th.FactionIndex, 3);
                break;

            case ActionController.ActionTypes.Extract:

                break;

            case ActionController.ActionTypes.Trade:

                break;

        }

        TileController.Instance.PushChangesFromTileUnderCursorChanged();
    }

    private void ResolveAssignedActionAtEnd()
    {
        switch (_assignedAction)
        {
            case ActionController.ActionTypes.Attack:
                ActionController.Instance.ResolveAttackAttempt(_targetTile, _attemptingFaction);
                break;

            case ActionController.ActionTypes.Invest:
                ActionController.Instance.ResolveInvestCompletion(_targetTile, _attemptingFaction);
                //_th.UndefendTile();
                //_nh.HealAllDamagedNodes();
                break;

            case ActionController.ActionTypes.Extract:
                ActionController.Instance.ResolveExtractAttempt(_targetTile, _attemptingFaction);
                //int amount = _th.HarvestNode();
                //FactionController.Instance.AdjustResources(amount, FactionController.Instance.PlayerFaction);
                //int randomUnrest = UnityEngine.Random.Range(0, 5);
                //_th.TileInfluenceHandler.AddInfluence(-1, randomUnrest);
                break;

            case ActionController.ActionTypes.Trade:
                ActionController.Instance.ResolveAttemptAtTrade(_targetTile, _attemptingFaction);
                break;
        }


    }
}
