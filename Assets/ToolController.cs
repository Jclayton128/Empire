using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ToolController : MonoBehaviour
{

    public static ToolController Instance {get; private set;}
    public enum Tools {Undefined, Attack, Defend, Strike, Invest}

    //refs
    [SerializeField] BattlePanelDriver _bpd = null;
    [SerializeField] TextMeshProUGUI _selectedToolTMP = null;

    //state
    Tools _selectedTool = Tools.Undefined;
    public Tools SelectedTool => _selectedTool;

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
        SelectTool(Tools.Undefined);
        TileController.Instance.TileUnderCursorChanged += HandleTileUnderCursorChanged;
    }

    private void HandleTileUnderCursorChanged()
    {
        if (TileController.Instance.TileUnderCursor == null)
        {
            _bpd.HideBattleDepiction();
            return;
        }


        switch (_selectedTool)
        {
            case Tools.Undefined:
                DepictUndefinedTool();
                break;

            case Tools.Attack:
                DepictProspectiveAttack();
                break;


        }
    }

    private void DepictUndefinedTool()
    {
        _bpd.HideBattleDepiction();
    }

    private void DepictProspectiveAttack()
    {
        if (TileController.Instance.TileUnderCursor.CurrentTileType.TType == TileType.TileTypes.Water)
        {
            //cannot attack water
            return;
        }

        TileHandler selectedTile = TileController.Instance.TileUnderCursor;
        if (selectedTile.FactionIndex == FactionController.Instance.PlayerFaction)
        {   
            //No point in depicting a battle against yourself, right?
            _bpd.HideBattleDepiction();
            return;
        }

        bool isAdjacent = false;
        foreach (var tile in selectedTile.OrderedNeighborTiles)
        {
            if (tile.FactionIndex == FactionController.Instance.PlayerFaction)
            {
                isAdjacent = true;
                break;
            }
        }

        //if (isAdjacent == false)
        //{
        //    //No point in depicting a battle that isn't adjacent to player territory, right?
        //    _bpd.HideBattleDepiction();
        //    return;
        //}

        if (isAdjacent)
        {
            //Set up a land battle
            List<int> neighborValues = new List<int> { -1, -1, -1, -1, -1, -1 };
            List<int> neighborFactions = new List<int> { -1, -1, -1, -1, -1, -1 };

            _defensiveHelp = 1 + selectedTile.DefendBonus;
            _offensiveHelp = 0;
            _neutrals = 0;

            for (int i = 0; i < selectedTile.OrderedNeighborTiles.Count; i++)
            {
                if (selectedTile.OrderedNeighborTiles[i] == null) continue;

                if (selectedTile.OrderedNeighborTiles[i].FactionIndex == FactionController.Instance.ActiveFaction)
                {
                    _offensiveHelp += selectedTile.OrderedNeighborTiles[i].AttackBonus + 1;
                    neighborValues[i] = selectedTile.OrderedNeighborTiles[i].AttackBonus + 1;
                    neighborFactions[i] = FactionController.Instance.ActiveFaction;
                }
                else if (selectedTile.OrderedNeighborTiles[i].FactionIndex != -1 &&
                    selectedTile.OrderedNeighborTiles[i].FactionIndex == selectedTile.FactionIndex)
                {
                    _defensiveHelp += selectedTile.OrderedNeighborTiles[i].DefendBonus + 1;
                    neighborValues[i] = selectedTile.OrderedNeighborTiles[i].DefendBonus + 1;
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

            _bpd.DepictBattle(selectedTile.FactionIndex, selectedTile.DefendBonus + 1, neighborValues, neighborFactions, oddsAsInt);
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

            _defensiveHelp = 1 + selectedTile.DefendBonus;
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
                    _defensiveHelp += selectedTile.OrderedNeighborTiles[i].DefendBonus + 1;
                    neighborValues[i] = selectedTile.OrderedNeighborTiles[i].DefendBonus + 1;
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
           
            _bpd.DepictBattle(selectedTile.FactionIndex, selectedTile.DefendBonus + 1, neighborValues, neighborFactions, oddsAsInt);

        }

        
    }

    public void HandleClickOnTile(TileHandler clickedTile)
    {
        switch (_selectedTool)
        {
            case Tools.Attack:
                AttemptAttack(clickedTile);
                break;

            case Tools.Defend:
                clickedTile.AttemptFortifyTile();
                break;


        }
    }

    private bool AttemptAttack(TileHandler clickedTile)
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

    public void SelectTool(Tools newTool)
    {
        _selectedTool = newTool;
        _selectedToolTMP.text = _selectedTool.ToString();
    }
}
