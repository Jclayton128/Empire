using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionBrain : MonoBehaviour, ActionCommander
{
    //settings

    [SerializeField] float _timeBetweenThinks = 2.1f;

    //state

    [SerializeField] bool _isExtant;
    [SerializeField] int _factionIndex;
    int _attackCost;
    float _attackDuration;
    int _investCost;
    float _investDuration;
    int _extractCost;
    float _extractDuration;
    [SerializeField] float _timeToNextThink = 0;

    [SerializeField] List<TileHandler> _allTiles = new List<TileHandler>();
    [SerializeField] List<TileHandler> _frontierTiles = new List<TileHandler>();
    [SerializeField] List<TileHandler> _interiorTiles = new List<TileHandler>();

    [SerializeField] List<ActionHandler> _actionsInPlay = new List<ActionHandler>();

    public void SetupFactionIndex(int index)
    {
        _isExtant = true;
        _timeToNextThink = (float)index / 10f;
        _factionIndex = index;
        _attackCost = ActionController.Instance.Cost_Attack;
        _attackDuration = ActionController.Instance.Duration_Attack;
        _investCost = ActionController.Instance.Cost_Invest;
        _investDuration = ActionController.Instance.Duration_Invest;
        _extractCost = ActionController.Instance.Cost_Extract;
        _extractDuration = ActionController.Instance.Duration_Extract;       

    }

    private void Update()
    {
        if (!_isExtant) return;

        _timeToNextThink -= Time.deltaTime;
        if (_timeToNextThink < 0)
        {
            _timeToNextThink = _timeBetweenThinks;

            UpdateTileState();
            UpdateAttack();
            UpdateGenerateResources();
        }

    }

    private void UpdateTileState()
    {
        _frontierTiles.Clear();
        _interiorTiles.Clear();

        _allTiles = new List<TileHandler>(TileController.Instance.GetFactionTileList(_factionIndex));

        foreach (var tile in _allTiles)
        {
            bool tileIsFrontier = false;
            foreach (var nt in tile.OrderedNeighborTiles)
            {
                if (nt.FactionIndex != _factionIndex)
                {
                    _frontierTiles.Add(tile);
                    tileIsFrontier = true;
                    break;
                }
            }
            if (tileIsFrontier == false) _interiorTiles.Add(tile);
        }
    }

    private void UpdateAttack()
    {

    }

    private void UpdateGenerateResources()
    {
        if (FactionController.Instance.CheckIfAffordable(_attackCost, _factionIndex))
        {
            Debug.Log("extracting. Could attack.");

            //mine rich, secure interiors only
            //foreach (var tile in _interiorTiles)
            foreach (var tile in _allTiles)
            {
                //do not consider a tile that already has an action
                if (CheckIfTargetTileAlreadyHasAnAction(tile)) return;

                //do not consider extracting from tiles that could rebel
                if (tile.TileInfluenceHandler.GetInfluenceForFaction(_factionIndex) < 9) return;

                if (tile.ResourceBonus > 0)
                {
                    ActionHandler action = ActionController.Instance.CreateNewAction();
                    action.AssignAction(ActionController.ActionTypes.Extract, tile, _factionIndex, _extractDuration, false, null, this);
                    _actionsInPlay.Add(action);
                }
                else
                {
                    //Debug.Log("Not mining this one due to zero resources", tile);
                }

            }

            //if (tilesToExtract > 0)
            //{
            //    foreach (var tile in _frontierTiles)
            //    {
            //        //do not consider a tile that already has an action
            //        if (CheckIfTargetTileAlreadyHasAnAction(tile)) return;

            //        //do not consider extracting from tiles that could rebel
            //        if (tile.TileInfluenceHandler.GetInfluenceForFaction(_factionIndex) < 9) return;

            //        if (tile.ResourceBonus >= (tile.MaxResource - 1f))
            //        {
            //            ActionHandler action = ActionController.Instance.CreateNewAction();
            //            action.AssignAction(ActionController.ActionTypes.Extract, tile, _factionIndex, _extractDuration, false, null, this);
            //            _actionsInPlay.Add(action);
            //            tilesToExtract--;
            //        }

            //        if (tilesToExtract <= 0) break;
            //    }
            //}

        }
        else
        {
            //mine secure frontiers and interiors
            //mine rich, secure interiors only
            foreach (var tile in _allTiles)
            {
                //do not consider a tile that already has an action
                if (CheckIfTargetTileAlreadyHasAnAction(tile)) return;

                //do not consider extracting from tiles that could rebel
                if (tile.TileInfluenceHandler.GetInfluenceForFaction(_factionIndex) < 9) return;

                if (tile.ResourceBonus > 0)
                {
                    ActionHandler action = ActionController.Instance.CreateNewAction();
                    action.AssignAction(ActionController.ActionTypes.Extract, tile, _factionIndex, _extractDuration, false, null, this);
                    _actionsInPlay.Add(action);
                }
            }
        }
    }

    private bool CheckIfTargetTileAlreadyHasAnAction(TileHandler targetTile)
    {
        foreach (var action in _actionsInPlay)
        {
            if (action.TargetTile == targetTile)
            {
                return true;
            }
        }

        return false;
    }

    public void CompleteAction(ActionHandler completedAction)
    {
        if (_actionsInPlay.Contains(completedAction))
        {
            _actionsInPlay.Remove(completedAction);
        }
    }
}
