using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ActionController : MonoBehaviour
{


    public static ActionController Instance {get; private set;}
    public enum ActionTypes {Undefined, Attack, Defend, Research, Exploit, Scout, Count}

    //refs
    [SerializeField] BattlePanelDriver _bpd = null;
    [SerializeField] ActionDriver _actionDriver = null;

    //settings
    [Header("Attack")]
    [SerializeField] Sprite _actionIcons_Attack = null;
    [SerializeField] float _duration_Attack = 3f;
    [SerializeField] int _costResource_Attack = 2;
    [SerializeField] int _costPop_Attack = 1;

    [Header("Defend")]
    [SerializeField] Sprite _actionIcons_Defend = null;
    [SerializeField] float _duration_Defend = 10f;
    [SerializeField] int _costResource_Defend = 3;
    [SerializeField] int _costPop_Defend = 2;

    [Header("other")]
    [SerializeField] Sprite _actionIcons_Research = null;
    [SerializeField] Sprite _actionIcons_Exploit = null;
    [SerializeField] Sprite _actionIcons_Scout = null;


    ActionTypes _selectedAction = ActionTypes.Undefined;
    public ActionTypes SelectedAction => _selectedAction;

    [Header("Attack Values")]
    [SerializeField] int _offensiveHelp = 0;
    [SerializeField] int _defensiveHelp = 0;
    [SerializeField] int _neutrals = 0;
    [SerializeField] TileHandler _tileToAttackFromForWaterAttacks;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SelectTool(ActionTypes.Undefined);
        TileController.Instance.TileUnderCursorChanged += HandleTileUnderCursorChanged;
    }

    private void HandleTileUnderCursorChanged()
    {
        if (TileController.Instance.TileUnderCursor == null)
        {
            _bpd.HideBattleDepiction();
            return;
        }


        switch (_selectedAction)
        {
            //case ActionTypes.Undefined:
            //    DepictUndefinedTool();
            //    break;

            case ActionTypes.Attack:
                if (CheckIfAttackIsPossibleAtTileUnderCursor())
                {
                    DepictProspectiveAttack();
                }
                else
                {
                    _bpd.HideBattleDepiction();
                }
                break;


        }
    }

    private void DepictUndefinedTool()
    {
        _bpd.HideBattleDepiction();
    }

    public void HandleClickOnTile(TileHandler clickedTile)
    {
        if (TileController.Instance.TileUnderCursor.TileActionHandler.AssignedAction != ActionTypes.Undefined)
        {
            //cannot assign a new action to a place already performing an action.
            return;
        }

        switch (_selectedAction)
        {
            case ActionTypes.Attack:

                if (FactionController.Instance.CheckIfAffordable(
                    _costPop_Attack, _costPop_Attack, FactionController.Instance.PlayerFaction) == false) return;
                else if(CheckIfAttackIsPossibleAtTileUnderCursor())
                {
                    FactionController.Instance.AdjustResources(-_costResource_Attack, FactionController.Instance.PlayerFaction);
                    FactionController.Instance.AdjustPopulation(-_costPop_Attack, FactionController.Instance.PlayerFaction);
                    clickedTile.GetComponent<ActionHandler>().AssignAction(ActionTypes.Attack, _duration_Attack, true, _actionIcons_Attack);
                }
                else
                {
                    //Can't attack here for some reason
                }
                break;

            case ActionTypes.Defend:

                if (FactionController.Instance.CheckIfAffordable(
                    _costResource_Defend, _costPop_Defend, FactionController.Instance.PlayerFaction) == false) return;
                else if (CheckIfDefendIsPossibleAtTileUnderCursor())
                {
                    FactionController.Instance.AdjustResources(-_costResource_Defend, FactionController.Instance.PlayerFaction);
                    FactionController.Instance.AdjustPopulation(-_costPop_Defend, FactionController.Instance.PlayerFaction);
                    clickedTile.GetComponent<ActionHandler>().AssignAction(ActionTypes.Defend, _duration_Defend, false, _actionIcons_Defend);
                }
                else
                {
                    //Can't defend here for some reason
                }

                break;


        }
    }

    #region Attack Action

    private bool CheckIfAttackIsPossibleAtTileUnderCursor()
    {
        if (TileController.Instance.TileUnderCursor.CurrentTileType.TType == TileType.TileTypes.Water)
        {
            //cannot attack water
            return false;
        }

        //if (TileController.Instance.TileUnderCursor.TileActionHandler.AssignedAction != ActionTypes.Undefined)
        //{
        //    //cannot attack a place already being attacked.
        //    return false;
        //}

        TileHandler selectedTile = TileController.Instance.TileUnderCursor;
        if (selectedTile.FactionIndex == FactionController.Instance.PlayerFaction)
        {
            //No point in depicting a battle against yourself, right?

            return false;
        }

        bool isAdjacent = false;
        foreach (var tile in selectedTile.OrderedNeighborTiles)
        {
            if (tile.FactionIndex == FactionController.Instance.PlayerFaction)
            {
                isAdjacent = true;
                return true;
                //break;
            }
        }

        if (!isAdjacent)
        {
            //detect if water attack is possible
            int waterDist = 0;
            List<TileHandler> attackerTiles = TileController.Instance.GetFactionTileList(FactionController.Instance.ActiveFaction);
            foreach (var tile in attackerTiles)
            {
                waterDist = TileController.Instance.FindDistanceThroughWater(tile, TileController.Instance.TileUnderCursor);
                if (waterDist > 0)
                {
                    return true;
                }
            }

            if (_tileToAttackFromForWaterAttacks == null)
            {
                //Debug.Log("No attack possible");
                return false;
            }
        }


        return false;


    }

    private void DepictProspectiveAttack()
    {

        TileHandler selectedTile = TileController.Instance.TileUnderCursor;
        bool isAdjacent = false;
        foreach (var tile in selectedTile.OrderedNeighborTiles)
        {
            if (tile.FactionIndex == FactionController.Instance.PlayerFaction)
            {
                isAdjacent = true;
                break;
            }
        }


        if (isAdjacent)
        {
            //Set up a land battle
            List<int> neighborValues = new List<int> { -1, -1, -1, -1, -1, -1 };
            List<int> neighborFactions = new List<int> { -1, -1, -1, -1, -1, -1 };

            _defensiveHelp = selectedTile.DefendBonus;
            _offensiveHelp = 0;
            _neutrals = 0;

            for (int i = 0; i < selectedTile.OrderedNeighborTiles.Count; i++)
            {
                if (selectedTile.OrderedNeighborTiles[i] == null) continue;

                if (selectedTile.OrderedNeighborTiles[i].FactionIndex == FactionController.Instance.ActiveFaction)
                {
                    _offensiveHelp += selectedTile.OrderedNeighborTiles[i].AttackBonus;
                    neighborValues[i] = selectedTile.OrderedNeighborTiles[i].AttackBonus;
                    neighborFactions[i] = FactionController.Instance.ActiveFaction;
                }
                else if (selectedTile.OrderedNeighborTiles[i].FactionIndex != -1 &&
                    selectedTile.OrderedNeighborTiles[i].FactionIndex == selectedTile.FactionIndex)
                {
                    _defensiveHelp += selectedTile.OrderedNeighborTiles[i].DefendBonus;
                    neighborValues[i] = selectedTile.OrderedNeighborTiles[i].DefendBonus;
                    neighborFactions[i] = selectedTile.FactionIndex;
                }
                else
                {
                    _neutrals++;
                    neighborValues[i] = 0;
                    neighborFactions[i] = selectedTile.OrderedNeighborTiles[i].FactionIndex;
                }
            }


            float odds = ((float)_offensiveHelp) / ((float)_defensiveHelp + (float)_offensiveHelp);
            int oddsAsInt = Mathf.RoundToInt(odds * 100f);

            _bpd.DepictBattle(selectedTile.FactionIndex, selectedTile.DefendBonus, neighborValues, neighborFactions,
                _offensiveHelp, _defensiveHelp, oddsAsInt);
        }
        else
        {
            //detect if water attack is possible
            int waterDist = 0;
            List<TileHandler> attackerTiles = TileController.Instance.GetFactionTileList(FactionController.Instance.ActiveFaction);
            _tileToAttackFromForWaterAttacks = null;
            foreach (var tile in attackerTiles)
            {
                waterDist = TileController.Instance.FindDistanceThroughWater(tile, TileController.Instance.TileUnderCursor);
                if (waterDist > 0)
                {
                    _tileToAttackFromForWaterAttacks = tile;
                    break;
                }
            }

            if (_tileToAttackFromForWaterAttacks == null)
            {
                Debug.Log("No attack possible");
                return;
            }

            List<int> neighborValues = new List<int> { -1, -1, -1, -1, -1, -1 };
            List<int> neighborFactions = new List<int> { -1, -1, -1, -1, -1, -1 };

            _defensiveHelp = selectedTile.DefendBonus;
            _offensiveHelp = 0; //water attacks are not buffed in same way as land.
            _neutrals = 0;

            for (int i = 0; i < selectedTile.OrderedNeighborTiles.Count; i++)
            {
                if (selectedTile.OrderedNeighborTiles[i] == null) continue;

                if (selectedTile.OrderedNeighborTiles[i].CurrentTileType.TType == TileType.TileTypes.Water)
                {
                    int c = TileController.Instance.FindDistanceThroughWater(selectedTile.OrderedNeighborTiles[i], _tileToAttackFromForWaterAttacks);
                    if (c > 0)
                    {
                        //This set up means that water attacks improve against islands and peninsulas
                        _offensiveHelp += 1;
                        neighborValues[i] = 1;
                    }
                    else
                    {
                        _offensiveHelp += 0;
                        neighborValues[i] = 0;
                    }

                    neighborFactions[i] = selectedTile.OrderedNeighborTiles[i].FactionIndex;
                }
                else if (selectedTile.OrderedNeighborTiles[i].FactionIndex != -1 &&
                    selectedTile.OrderedNeighborTiles[i].FactionIndex == selectedTile.FactionIndex)
                {
                    _defensiveHelp += selectedTile.OrderedNeighborTiles[i].DefendBonus;
                    neighborValues[i] = selectedTile.OrderedNeighborTiles[i].DefendBonus;
                    neighborFactions[i] = selectedTile.FactionIndex;
                }
                else
                {
                    _neutrals++;
                    neighborValues[i] = 0;
                    neighborFactions[i] = selectedTile.OrderedNeighborTiles[i].FactionIndex;
                }
            }


            float odds = ((float)_offensiveHelp) / ((float)_defensiveHelp + (float)_offensiveHelp);
            int oddsAsInt = Mathf.RoundToInt(odds * 100f);
           
            _bpd.DepictBattle(selectedTile.FactionIndex, selectedTile.DefendBonus, neighborValues, neighborFactions,
                _offensiveHelp, _defensiveHelp, oddsAsInt);

        }

        
    }

    public bool ResolveAttackAttempt(TileHandler clickedTile)
    {
        if (clickedTile.FactionIndex == FactionController.Instance.PlayerFaction)
        {
            //cannot attack ownself
            return false;
        }
        if (clickedTile.CurrentTileType.TType == TileType.TileTypes.Water)
        {
            //cannot attack or own water tiles
            return false;
        }

        bool isAdjacent = false;
        foreach (var tile in clickedTile.ShuffledNeighborTiles)
        {
            if (tile == null) continue;
            if (tile.FactionIndex == FactionController.Instance.PlayerFaction)
            {
                isAdjacent = true;
                break;
            }
        }


        if (!isAdjacent)
        {
            bool didAttackResolve = AttemptWaterAttack(clickedTile);
            return didAttackResolve;
        }
        else
        {
            bool didAttackResolve = AttemptLandAttack(clickedTile);
            return didAttackResolve;
        }


    }

    private bool AttemptLandAttack(TileHandler clickedTile)
    {


        int totalOdds = _offensiveHelp + _defensiveHelp;
        int rand = UnityEngine.Random.Range(1, totalOdds + 1);
        if (rand > _defensiveHelp)
        {
            //attacks succeeds
            Debug.Log($"Attack succeeds: {rand}/{totalOdds}");

            int winningFaction = FactionController.Instance.PlayerFaction;

            TileController.Instance.ChangeTileFaction(clickedTile, clickedTile.FactionIndex, winningFaction);
            clickedTile.AssignFactionToTile(winningFaction);
            TileController.Instance.HighlightFaction(winningFaction);
        }
        else
        {
            //attack fails
            Debug.Log($"Attack fails: {rand}/{totalOdds}");
        }

        return true;
    }

    private bool AttemptWaterAttack(TileHandler clickedTile)
    {

        int c = TileController.Instance.FindDistanceThroughWater(_tileToAttackFromForWaterAttacks, clickedTile);
        if (c <= 0)
        {

            //no path from tile to attack from to clicked tile
            return false;
        }


        int totalOdds = _offensiveHelp + _defensiveHelp;
        int rand = UnityEngine.Random.Range(1, totalOdds + 1);
        if (rand > _defensiveHelp)
        {
            //attacks succeeds
            Debug.Log($"Attack succeeds: {rand}/{totalOdds}");

            int winningFaction = FactionController.Instance.PlayerFaction;
            int oldFaction = clickedTile.FactionIndex;

            if (clickedTile.FactionIndex > 0)
            {
                TileController.Instance.ChangeTileFaction(clickedTile, clickedTile.FactionIndex, -1);
                clickedTile.AssignFactionToTile(-1);
                clickedTile.DehighlightBorders();
                TileController.Instance.HighlightFaction(oldFaction);
            }
            else if (clickedTile.FactionIndex == -1)
            {
                TileController.Instance.ChangeTileFaction(clickedTile, clickedTile.FactionIndex, winningFaction);
                clickedTile.AssignFactionToTile(winningFaction);
                TileController.Instance.HighlightFaction(winningFaction);
            }

        }
        else
        {
            //attack fails
            Debug.Log($"Attack fails: {rand}/{totalOdds}");
        }

        return true;
    }


    #endregion

    #region Defend Action

    private bool CheckIfDefendIsPossibleAtTileUnderCursor()
    {
        //if (TileController.Instance.TileUnderCursor.TileActionHandler.AssignedAction != ActionTypes.Undefined)
        //{
        //    //cannot defend a place already being attacked.
        //    return false;
        //}

        if (TileController.Instance.TileUnderCursor.CurrentTileType.TType == TileType.TileTypes.Plain &&
            TileController.Instance.TileUnderCursor.FactionIndex == FactionController.Instance.PlayerFaction)
        {
            return true;
        }
        else return false;
    }



    #endregion
    
    

    #region Action Selection

    public void SelectTool(ActionTypes newTool)
    {
        _selectedAction = newTool;
        _actionDriver.SetName(_selectedAction.ToString());
        ShowActionCost();
    }

    public void IncrementToolSelection()
    {
        if ((int)_selectedAction == (int)ActionTypes.Count-1)
        {
            _selectedAction = (ActionTypes)1;
        }
        else 
        {
            _selectedAction++;
        }
        _actionDriver.SetName(_selectedAction.ToString());
        ShowActionCost();
    }

    public void DecrementToolSelection()
    {
        if ((int)_selectedAction <= 1)
        {
            _selectedAction = (ActionTypes)ActionTypes.Count-1;
        }
        else
        {
            _selectedAction--;
        }
        _actionDriver.SetName(_selectedAction.ToString());
        ShowActionCost();
    }

    private void ShowActionCost()
    {
        switch (_selectedAction)
        {
            case ActionTypes.Attack:
                _actionDriver.SetCost(_costResource_Attack, _costPop_Attack);
                break;

            case ActionTypes.Defend:
                _actionDriver.SetCost(_costResource_Defend, _costPop_Defend);
                break;

        }
    }

    #endregion
}
