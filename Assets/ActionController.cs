using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ActionController : MonoBehaviour
{


    public static ActionController Instance {get; private set;}
    public enum ActionTypes {Undefined, Attack, Defend, Research, Mine, Scout, Count}

    //refs
    [SerializeField] BattlePanelDriver _bpd = null;
    [SerializeField] ActionDriver _actionDriver_left = null;
    [SerializeField] ActionDriver _actionDriver_right = null;

    //settings
    [Header("Attack")]
    [SerializeField] Sprite _actionIcons_Attack = null;
    [SerializeField] float _duration_Attack = 3f;
    [SerializeField] int _cost_Attack = 2;

    [Header("Defend")]
    [SerializeField] Sprite _actionIcons_Defend = null;
    [SerializeField] float _duration_Defend = 10f;
    [SerializeField] int _cost_Defend = 3;


    [Header("Mine")]
    [SerializeField] Sprite _actionIcons_Mine = null;
    [SerializeField] float _duration_Mine = 3f;
    [SerializeField] int _cost_Mine = 0;

    [Header("Trade")]
    [SerializeField] Sprite _actionIcons_Trade = null;
    [SerializeField] float _duration_Trade = 3f;
    [SerializeField] int _cost_Trade = 0;


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
        TileController.Instance.TileUnderCursorChanged += HandleTileUnderCursorChanged;
    }

    private void HandleTileUnderCursorChanged()
    {
        if (TileController.Instance.TileUnderCursor == null)
        {
            _bpd.HideBattleDepiction();
            return;
        }

        if (TileController.Instance.TileUnderCursor.FactionIndex == FactionController.Instance.PlayerFaction)
        {
            _actionDriver_left.SetName("Mine");
            _actionDriver_left.SetCost(_cost_Mine);
            _actionDriver_right.SetName("Defend");
            _actionDriver_right.SetCost(_cost_Defend);

            _bpd.HideBattleDepiction();
        }
        else
        {
            _actionDriver_left.SetName("Attack");
            _actionDriver_left.SetCost(_cost_Attack);
            _actionDriver_right.SetName("Trade");
            _actionDriver_right.SetCost(_cost_Trade);

            if (CheckIfAttackIsPossibleAtTileUnderCursor() ||
                TileController.Instance.TileUnderCursor.TileActionHandler.AssignedAction == ActionTypes.Attack)
            {
                DepictProspectiveAttack();
            }
            else
            {
                _bpd.HideBattleDepiction();
            }
        }        
    }

    public void HandleLMBClickOnTile(TileHandler clickedTile)
    {
        if (TileController.Instance.TileUnderCursor.TileActionHandler.AssignedAction != ActionTypes.Undefined)
        {
            //cannot assign any new action to a place already performing an action.
            return;
        }

        //check if clickedtile is in or out

        if (clickedTile.FactionIndex == FactionController.Instance.PlayerFaction)
        {
            if (CheckIfMineIsPossibleAtTileUnderCursor() &&
                FactionController.Instance.CheckIfAffordable(_cost_Mine, FactionController.Instance.PlayerFaction))
            {
                clickedTile.GetComponent<ActionHandler>().AssignAction(ActionTypes.Mine, _duration_Mine, false, _actionIcons_Mine);
                FactionController.Instance.AdjustResources(-_cost_Mine, FactionController.Instance.PlayerFaction);
            }
        }
        else
        {
            if (CheckIfAttackIsPossibleAtTileUnderCursor() &&
                FactionController.Instance.CheckIfAffordable(_cost_Attack, FactionController.Instance.PlayerFaction))
            {
                clickedTile.GetComponent<ActionHandler>().AssignAction(ActionTypes.Attack, _duration_Attack, true, _actionIcons_Attack);
                FactionController.Instance.AdjustResources(-_cost_Attack, FactionController.Instance.PlayerFaction);
            }
        }
    }

    public void HandleRMBClickOnTile(TileHandler clickedTile)
    {
        if (TileController.Instance.TileUnderCursor.TileActionHandler.AssignedAction != ActionTypes.Undefined)
        {
            //cannot assign any new action to a place already performing an action.
            return;
        }

        if (clickedTile.FactionIndex == FactionController.Instance.PlayerFaction)
        {
            if (CheckIfDefendIsPossibleAtTileUnderCursor() &&
                FactionController.Instance.CheckIfAffordable(_cost_Defend, FactionController.Instance.PlayerFaction))
            {
                clickedTile.GetComponent<ActionHandler>().AssignAction(ActionTypes.Defend, _duration_Defend, false, _actionIcons_Defend);
                FactionController.Instance.AdjustResources(-_cost_Defend, FactionController.Instance.PlayerFaction);
            }
        }
        else
        {
            //Diplomacy

            //if (CheckIfAttackIsPossibleAtTileUnderCursor())
            //{
            //    clickedTile.GetComponent<ActionHandler>().AssignAction(ActionTypes.Attack, _duration_Attack, true, _actionIcons_Attack);
            //}
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
        //Debug.Log("depicting updated attack odds");
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
        int lopside = Mathf.Abs(_offensiveHelp - _defensiveHelp) + 1;
        int damageRand = UnityEngine.Random.Range(0, lopside + 1);
        //Debug.Log($"rolled {damageRand} / {lopside}");
        if ( damageRand == 0)
        {
            //damage tile;

            clickedTile.GetComponent<NodeHandler>().DamageNode();
        }

        int totalOdds = _offensiveHelp + _defensiveHelp;
        int rand = UnityEngine.Random.Range(1, totalOdds + 1);
        if (rand > _defensiveHelp)
        {
            //attacks succeeds
            //Debug.Log($"Attack succeeds: {rand}/{totalOdds}");

            int winningFaction = FactionController.Instance.PlayerFaction;

            TileController.Instance.ChangeTileFaction(clickedTile, clickedTile.FactionIndex, winningFaction);
            clickedTile.AssignFactionToTile(winningFaction);
            TileController.Instance.HighlightFaction(winningFaction);
        }
        else
        {
            //attack fails
            //Debug.Log($"Attack fails: {rand}/{totalOdds}");
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

    #region Mine Action

    private bool CheckIfMineIsPossibleAtTileUnderCursor()
    {
        if ((TileController.Instance.TileUnderCursor.CurrentTileType.TType == TileType.TileTypes.Plain ||
            TileController.Instance.TileUnderCursor.CurrentTileType.TType == TileType.TileTypes.Capitol) &&
            TileController.Instance.TileUnderCursor.FactionIndex == FactionController.Instance.PlayerFaction)
        {
            return true;
        }
        else return false;
    }

    #endregion


    
}
