using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ActionController : MonoBehaviour, ActionCommander
{


    public static ActionController Instance {get; private set;}
    public enum ActionTypes {Undefined, Attack, Invest, Extract, Trade, Count}

    //refs
    [SerializeField] BattlePanelDriver _bpd = null;
    [SerializeField] ActionDriver _actionDriver_left = null;
    [SerializeField] ActionDriver _actionDriver_right = null;
    [SerializeField] ActionHandler _actionPrefab = null;
    //settings
    [Header("Attack")]
    [SerializeField] Sprite _actionIcons_Attack = null;
    [SerializeField] float _duration_Attack = 3f;
    [SerializeField] int _cost_Attack = 2;
    public int Cost_Attack => _cost_Attack;
    public float Duration_Attack => _duration_Attack;

    [Header("Invest")]
    [SerializeField] Sprite _actionIcons_Invest = null;
    [SerializeField] float _duration_Invest = 12f;
    [SerializeField] int _cost_Invest = 3;
    public int Cost_Invest => _cost_Invest;
    public float Duration_Invest => _duration_Invest;

    [Header("Extract")]
    [SerializeField] Sprite _actionIcons_Extract = null;
    [SerializeField] float _duration_Extract = 5f;
    [SerializeField] int _cost_Extract = 0;
    public int Cost_Extract => _cost_Extract;
    public float Duration_Extract => _duration_Extract;

    [Header("Trade")]
    [SerializeField] Sprite _actionIcons_Trade = null;
    [SerializeField] float _duration_Trade = 15f;
    [SerializeField] int _cost_Trade = 4;
    public int Cost_Trade => _cost_Trade;
    public float Duration_Trade => _duration_Trade;

    [Header("Attack Values")]
    //[SerializeField] int _offensiveHelp_player = 0;
    //[SerializeField] int _defensiveHelp_player = 0;
    //[SerializeField] int _neutrals_player = 0;
    //[SerializeField] TileHandler _tileToAttackFromForWaterAttacks;


    //state
    [SerializeField] List<ActionHandler> _actionsInPlayForPlayer = new List<ActionHandler>();

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
            _actionDriver_left.SetCost(_cost_Extract);
            _actionDriver_right.SetName("Defend");
            _actionDriver_right.SetCost(_cost_Invest);

            _bpd.HideBattleDepiction();
        }
        else
        {
            _actionDriver_left.SetName("Attack");
            _actionDriver_left.SetCost(_cost_Attack);
            _actionDriver_right.SetName("Trade");
            _actionDriver_right.SetCost(_cost_Trade);

            if (CheckIfAttackIsPossibleAtTileUnderCursor())
                //||TileController.Instance.TileUnderCursor.TileActionHandler.AssignedAction == ActionTypes.Attack)
            {
                DepictProspectiveProspectivePlayerAttack();
            }
            else
            {
                _bpd.HideBattleDepiction();
            }
        }        
    }

    private ActionHandler CreateNewPlayerAction()
    {
        var newAction = CreateNewAction();
        _actionsInPlayForPlayer.Add(newAction);
        return newAction;
    }

    public ActionHandler CreateNewAction()
    {
        ActionHandler newAction = Instantiate(_actionPrefab);
        return newAction;
    }

    public void CompleteAction(ActionHandler completedAction)
    {
        _actionsInPlayForPlayer.Remove(completedAction);
    }

    public void ClearAllActions()
    {
        if (_actionsInPlayForPlayer.Count > 0)
        {
            for (int i = _actionsInPlayForPlayer.Count - 1; i >= 0; i--)
            {
                Destroy(_actionsInPlayForPlayer[i].gameObject);
            }
            _actionsInPlayForPlayer.Clear();
        }
    }

    #region Player UI responses

    public bool CheckIfFactionHasAlreadyAssignedActionToTile(TileHandler targetTile, int attemptingFaction)
    {
        foreach (var action in _actionsInPlayForPlayer)
        {
            if (action.TargetTile == targetTile && action.AttemptingFaction == attemptingFaction)
            {
                return true;
            }
        }

        return false;
    }

    public void HandleLMBClickOnTile(TileHandler clickedTile)
    {
        if (CheckIfFactionHasAlreadyAssignedActionToTile(clickedTile, FactionController.Instance.PlayerFaction) == true)
        {
            //cannot assign any new action to a place already performing an action.
            return;
        }

        //check if clickedtile is in or out

        if (clickedTile.FactionIndex == FactionController.Instance.PlayerFaction)
        {
            if (CheckIfExtractIsPossibleAtTileUnderCursor() &&
                FactionController.Instance.CheckIfAffordable(_cost_Extract, FactionController.Instance.PlayerFaction))
            {
                ActionHandler ah = CreateNewPlayerAction();
                ah.AssignAction(ActionTypes.Extract,
                    TileController.Instance.TileUnderCursor, FactionController.Instance.PlayerFaction,
                    _duration_Extract, false, _actionIcons_Extract, this);
                FactionController.Instance.AdjustResources(-_cost_Extract, FactionController.Instance.PlayerFaction);
            }
        }
        else
        {
            if (CheckIfAttackIsPossibleAtTileUnderCursor() &&
                FactionController.Instance.CheckIfAffordable(_cost_Attack, FactionController.Instance.PlayerFaction))
            {
                ActionHandler ah = CreateNewPlayerAction();
                ah.AssignAction(ActionTypes.Attack,
                    TileController.Instance.TileUnderCursor, FactionController.Instance.PlayerFaction, 
                    _duration_Attack, true, _actionIcons_Attack, this);
                FactionController.Instance.AdjustResources(-_cost_Attack, FactionController.Instance.PlayerFaction);
            }
        }
    }

    public void HandleRMBClickOnTile(TileHandler clickedTile)
    {
        if (CheckIfFactionHasAlreadyAssignedActionToTile(clickedTile, FactionController.Instance.PlayerFaction) == true)
        {
            //cannot assign any new action to a place already performing an action.
            return;
        }

        if (clickedTile.FactionIndex == FactionController.Instance.PlayerFaction)
        {
            if (CheckIfInvestIsPossibleAtTileUnderCursor() &&
                FactionController.Instance.CheckIfAffordable(_cost_Invest, FactionController.Instance.PlayerFaction))
            {
                ActionHandler ah = CreateNewPlayerAction();
                ah.AssignAction(ActionTypes.Invest,
                    TileController.Instance.TileUnderCursor, FactionController.Instance.PlayerFaction, 
                    _duration_Invest, false, _actionIcons_Invest, this);
                FactionController.Instance.AdjustResources(-_cost_Invest, FactionController.Instance.PlayerFaction);
            }
        }
        else
        {
            //Diplomacy

            if (CheckIfTradeIsPossibleAtTileUnderCursor())
            {
                ActionHandler ah = CreateNewPlayerAction();
                ah.AssignAction(ActionTypes.Trade,
                    TileController.Instance.TileUnderCursor, FactionController.Instance.PlayerFaction,
                    FindDurationForTradeActionAtTileUnderCursor(), true, _actionIcons_Trade, this);
            }
        }
    }

    #endregion

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

            //if (_tileToAttackFromForWaterAttacks == null)
            //{
            //    //Debug.Log("No attack possible");
            //    return false;
            //}
        }


        return false;


    }

    /// <summary>
    /// X: offensive help, Y: defensive help, Z: neutral
    /// </summary>
    /// <param name="clickedTile"></param>
    /// <param name="attemptingFaction"></param>
    /// <returns></returns>
    private Vector3Int FindAttackOdds_Land(TileHandler clickedTile, int attemptingFaction,
        out List<int> neighborValues, out List<int> neighborFactions)
    {
        neighborValues = new List<int> { -1, -1, -1, -1, -1, -1 };
        neighborFactions = new List<int> { -1, -1, -1, -1, -1, -1 };

        int defensiveHelp = clickedTile.DefendBonus;
        int offensiveHelp = 0;
        int neutrals = 0;

        for (int i = 0; i < clickedTile.OrderedNeighborTiles.Count; i++)
        {
            if (clickedTile.OrderedNeighborTiles[i] == null) continue;

            if (clickedTile.OrderedNeighborTiles[i].FactionIndex == attemptingFaction)
            {
                offensiveHelp += clickedTile.OrderedNeighborTiles[i].AttackBonus;
                neighborValues[i] = clickedTile.OrderedNeighborTiles[i].AttackBonus;
                neighborFactions[i] = FactionController.Instance.ActiveFaction;
            }
            else if (clickedTile.OrderedNeighborTiles[i].FactionIndex != -1 &&
                clickedTile.OrderedNeighborTiles[i].FactionIndex == clickedTile.FactionIndex)
            {
                defensiveHelp += clickedTile.OrderedNeighborTiles[i].DefendBonus;
                neighborValues[i] = clickedTile.OrderedNeighborTiles[i].DefendBonus;
                neighborFactions[i] = clickedTile.FactionIndex;
            }
            else
            {
                neutrals++;
                neighborValues[i] = 0;
                neighborFactions[i] = clickedTile.OrderedNeighborTiles[i].FactionIndex;
            }
        }

        return new Vector3Int(offensiveHelp, defensiveHelp, neutrals);

    }

    public Vector3Int FindAttackOdds_Land(TileHandler clickedTile, int attemptingFaction)
    {
        return (FindAttackOdds_Land(clickedTile, attemptingFaction, out _, out _));
    }

    private Vector3Int FindAttackOdds_Water(TileHandler originTileForAttack, TileHandler targetTile, int attemptingFaction,
        out List<int> neighborValues, out List<int> neighborFactions)
    {
        neighborValues = new List<int> { -1, -1, -1, -1, -1, -1 };
        neighborFactions = new List<int> { -1, -1, -1, -1, -1, -1 };

        int defensiveHelp = targetTile.DefendBonus;
        int offensiveHelp = 0; //water attacks are not buffed in same way as land.
        int neutrals = 0;

        for (int i = 0; i < targetTile.OrderedNeighborTiles.Count; i++)
        {
            if (targetTile.OrderedNeighborTiles[i] == null) continue;

            if (targetTile.OrderedNeighborTiles[i].CurrentTileType.TType == TileType.TileTypes.Water)
            {
                int c = TileController.Instance.FindDistanceThroughWater(targetTile.OrderedNeighborTiles[i], originTileForAttack);
                if (c > 0)
                {
                    //This set up means that water attacks improve against islands and peninsulas
                    offensiveHelp += 1;
                    neighborValues[i] = 1;
                }
                else
                {
                    offensiveHelp += 0;
                    neighborValues[i] = 0;
                }

                neighborFactions[i] = targetTile.OrderedNeighborTiles[i].FactionIndex;
            }
            else if (targetTile.OrderedNeighborTiles[i].FactionIndex != -1 &&
                targetTile.OrderedNeighborTiles[i].FactionIndex == targetTile.FactionIndex)
            {
                defensiveHelp += targetTile.OrderedNeighborTiles[i].DefendBonus;
                neighborValues[i] = targetTile.OrderedNeighborTiles[i].DefendBonus;
                neighborFactions[i] = targetTile.FactionIndex;
            }
            else
            {
                neutrals++;
                neighborValues[i] = 0;
                neighborFactions[i] = targetTile.OrderedNeighborTiles[i].FactionIndex;
            }
        }



        return new Vector3Int(offensiveHelp, defensiveHelp, neutrals);
    }

    public Vector3Int FindAttackOdds_Water(TileHandler originTileForAttack, TileHandler targetTile, int attemptingFaction)
    {
        return FindAttackOdds_Water(originTileForAttack, targetTile, attemptingFaction, out _, out _);
    }

 

    private void DepictProspectiveProspectivePlayerAttack()
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
            //Find Odds for land attack
            List<int> neighborValues;
            List<int> neighborFactions;
            var oddsVec = FindAttackOdds_Land(selectedTile, FactionController.Instance.PlayerFaction, out neighborValues, out neighborFactions);

            int offensiveHelp = oddsVec.x;
            int defensiveHelp = oddsVec.y;
            int neutrals = oddsVec.z;

            float odds = ((float)offensiveHelp) / ((float)defensiveHelp + (float)offensiveHelp);
            int oddsAsInt = Mathf.RoundToInt(odds * 100f);

            _bpd.DepictBattle(selectedTile.FactionIndex, selectedTile.DefendBonus, neighborValues, neighborFactions,
                offensiveHelp, defensiveHelp, oddsAsInt);
        }
        else
        {
            //detect if water attack is possible

            TileHandler tileToAttackFromForWaterAttacks = FindClosestOriginTileForWaterAttack(selectedTile, FactionController.Instance.PlayerFaction);
            //foreach (var tile in attackerTiles)
            //{
            //    waterDist = TileController.Instance.FindDistanceThroughWater(tile, TileController.Instance.TileUnderCursor);
            //    if (waterDist > 0)
            //    {
            //        tileToAttackFromForWaterAttacks = tile;
            //        //a water pathway was found. Stop looking.
            //        break;
            //    }
            //}

            if (tileToAttackFromForWaterAttacks == null)
            {
                Debug.Log("No attack possible");
                return;
            }

            List<int> neighborValues;
            List<int> neighborFactions;

            Vector3Int oddsVec = FindAttackOdds_Water(tileToAttackFromForWaterAttacks, selectedTile, FactionController.Instance.PlayerFaction, out neighborValues, out neighborFactions);

            int offensiveHelp = oddsVec.x;
            int defensiveHelp = oddsVec.y;
            int neutrals = oddsVec.z;
            


            float odds = ((float)offensiveHelp) / ((float)defensiveHelp + (float)offensiveHelp);
            int oddsAsInt = Mathf.RoundToInt(odds * 100f);
           
            _bpd.DepictBattle(selectedTile.FactionIndex, selectedTile.DefendBonus, neighborValues, neighborFactions,
                offensiveHelp, defensiveHelp, oddsAsInt);

        }

        
    }

    private TileHandler FindClosestOriginTileForWaterAttack(TileHandler targetTile, int attemptingFaction)
    {
        TileHandler tileToAttackFromForWaterAttacks = null;
        int waterDist = 0;
        List<TileHandler> attackerTiles = TileController.Instance.GetFactionTileList(attemptingFaction);
        foreach (var tile in attackerTiles)
        {
            waterDist = TileController.Instance.FindDistanceThroughWater(tile, TileController.Instance.TileUnderCursor);
            if (waterDist > 0)
            {
                tileToAttackFromForWaterAttacks = tile;
                //a water pathway was found. Stop looking.
                break;
            }
        }

        return tileToAttackFromForWaterAttacks;
    }

    public bool ResolveAttackAttempt(TileHandler targetTile, int attemptingFaction)
    {
        if (targetTile.FactionIndex == attemptingFaction)
        {
            //cannot attack ownself
            return false;
        }
        if (targetTile.CurrentTileType.TType == TileType.TileTypes.Water)
        {
            //cannot attack or own water tiles
            return false;
        }

        bool isAdjacent = false;
        foreach (var tile in targetTile.ShuffledNeighborTiles)
        {
            if (tile == null) continue;
            if (tile.FactionIndex == attemptingFaction)
            {
                isAdjacent = true;
                break;
            }
        } 


        if (!isAdjacent)
        {
            TileHandler originTile = FindClosestOriginTileForWaterAttack(targetTile, attemptingFaction);

            bool didAttackResolve = AttemptWaterAttack(originTile, targetTile, attemptingFaction);
            return didAttackResolve;
        }
        else
        {
            bool didAttackResolve = AttemptLandAttack(targetTile, attemptingFaction);
            return didAttackResolve;
        }


    }

    private bool AttemptLandAttack(TileHandler clickedTile, int attemptingFaction)
    {
        var odds = FindAttackOdds_Land(clickedTile, attemptingFaction);


        int offensiveHelp = odds.x;
        int defensiveHelp = odds.y;
        int neutrals = odds.z;


        int lopside = Mathf.Abs(offensiveHelp - defensiveHelp) + 1;
        int damageRand = UnityEngine.Random.Range(0, lopside + 1);
        //Debug.Log($"rolled {damageRand} / {lopside}");
        if ( damageRand == 0)
        {
            //damage tile;

            clickedTile.GetComponent<PopulationHandler>().DecrementFillableNode();
        }

        int totalOdds = offensiveHelp + defensiveHelp;
        int rand = UnityEngine.Random.Range(1, totalOdds + 1);
        if (rand > defensiveHelp)
        {
            //attacks succeeds
            //Debug.Log($"Attack succeeds: {rand}/{totalOdds}");

            int winningFaction = attemptingFaction;

            TileController.Instance.ChangeTileFaction(clickedTile, clickedTile.FactionIndex, winningFaction);
            clickedTile.AssignFactionToTile(winningFaction, false);
            clickedTile.TileInfluenceHandler.AddInfluence(winningFaction, 5);
            //TileController.Instance.HighlightFaction(winningFaction);
        }
        else
        {
            //attack fails
            //Debug.Log($"Attack fails: {rand}/{totalOdds}");
        }

        return true;
    }

    private bool AttemptWaterAttack(TileHandler originTileForAttack, TileHandler clickedTile, int attemptingFaction)
    {

        int c = TileController.Instance.FindDistanceThroughWater(originTileForAttack, clickedTile);
        if (c <= 0)
        {

            //no path from tile to attack from to clicked tile
            return false;
        }

        var odds = FindAttackOdds_Water(originTileForAttack, clickedTile, attemptingFaction);

        int offensiveHelp = odds.x;
        int defensiveHelp = odds.y;
        int neutrals = odds.z;


        int totalOdds = offensiveHelp + defensiveHelp;
        int rand = UnityEngine.Random.Range(1, totalOdds + 1);
        if (rand > defensiveHelp)
        {
            //attacks succeeds
            //Debug.Log($"Attack succeeds: {rand}/{totalOdds}");

            int winningFaction = attemptingFaction;
            int oldFaction = clickedTile.FactionIndex;

            if (clickedTile.FactionIndex > 0)
            {
                TileController.Instance.ChangeTileFaction(clickedTile, clickedTile.FactionIndex, -1);
                clickedTile.AssignFactionToTile(-1, true);
                clickedTile.DehighlightBorders();
                TileController.Instance.RefreshBorderHighlights();
                //TileController.Instance.HighlightFaction(oldFaction);
            }
            else if (clickedTile.FactionIndex == -1)
            {
                TileController.Instance.ChangeTileFaction(clickedTile, clickedTile.FactionIndex, winningFaction);
                clickedTile.AssignFactionToTile(winningFaction, false);
                clickedTile.TileInfluenceHandler.AddInfluence(winningFaction, 5);
                TileController.Instance.RefreshBorderHighlights();
                //TileController.Instance.HighlightFaction(winningFaction);
            }

        }
        else
        {
            //attack fails
            //Debug.Log($"Attack fails: {rand}/{totalOdds}");
        }

        return true;
    }


    #endregion

    #region Invest Action

    private bool CheckIfInvestIsPossibleAtTileUnderCursor()
    {
        //if (TileController.Instance.TileUnderCursor.TileActionHandler.AssignedAction != ActionTypes.Undefined)
        //{
        //    //cannot defend a place already being attacked.
        //    return false;
        //}

        if ((TileController.Instance.TileUnderCursor.CurrentTileType.TType == TileType.TileTypes.Plain ||
            TileController.Instance.TileUnderCursor.CurrentTileType.TType == TileType.TileTypes.Capitol) &&
            TileController.Instance.TileUnderCursor.FactionIndex == FactionController.Instance.PlayerFaction)
        {
            return true;
        }
        else return false;
    }

    public void ResolveInvestStart(TileHandler targetTile, int attemptingFaction)
    {
        targetTile.AttemptDefendTile();
        targetTile.TileInfluenceHandler.AddInfluence(attemptingFaction, 3);
    }

    public void ResolveInvestCompletion(TileHandler targetTile, int attemptingFaction)
    {
        targetTile.UndefendTile();
        targetTile.TilePopulationHandler.IncrementFillableNode();
    }

    #endregion

    #region Extract Action

    private bool CheckIfExtractIsPossibleAtTileUnderCursor()
    {
        if ((TileController.Instance.TileUnderCursor.CurrentTileType.TType == TileType.TileTypes.Plain ||
            TileController.Instance.TileUnderCursor.CurrentTileType.TType == TileType.TileTypes.Capitol) &&
            TileController.Instance.TileUnderCursor.FactionIndex == FactionController.Instance.PlayerFaction)
        {
            return true;
        }
        else return false;
    }

    public void ResolveExtractAttempt(TileHandler targetTile, int attemptingFaction)
    {
        int amount = targetTile.HarvestNode();
        FactionController.Instance.AdjustResources(amount, attemptingFaction);
        int randomUnrest = UnityEngine.Random.Range(0, 3);
        targetTile.TileInfluenceHandler.AddInfluence(-1, randomUnrest);
    }

    #endregion
    
    #region Diplomacy Action

    private bool CheckIfTradeIsPossibleAtTileUnderCursor()
    {
        if (TileController.Instance.TileUnderCursor.CurrentTileType.TType == TileType.TileTypes.Water)
        {
            //cannot attack water
            return false;
        }

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

        return false;
    }

    private float FindDurationForTradeActionAtTileUnderCursor()
    {
        int alliedProductionAtSurroundingTiles = 0;

        foreach (var tile in TileController.Instance.TileUnderCursor.OrderedNeighborTiles)
        {
            if (tile.FactionIndex == FactionController.Instance.PlayerFaction)
            {
                alliedProductionAtSurroundingTiles += tile.ResourceBonus;
            }
        }

        float pFactor = alliedProductionAtSurroundingTiles / 9f;
        float tradeMult = Mathf.Lerp( 1f, 0.2f, pFactor);
        //Debug.Log($"anp: {alliedProductionAtSurroundingTiles}, pfac: {pFactor}, trademult: {tradeMult}");
        return (_duration_Trade * tradeMult);
    }

    public void ResolveAttemptAtTrade(TileHandler targetTile, int attemptingFaction)
    {
        int randomAmountOfInfluenceToAdd = UnityEngine.Random.Range(1, 6);

        targetTile.TileInfluenceHandler.AddInfluence(attemptingFaction, randomAmountOfInfluenceToAdd);
    }

    #endregion

}
