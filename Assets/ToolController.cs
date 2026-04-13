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
        TileHandler selectedTile = TileController.Instance.TileUnderCursor;
        if (selectedTile.FactionIndex == FactionController.Instance.PlayerFaction)
        {   
            //No point in depicting a battle against yourself, right?
            _bpd.HideBattleDepiction();
            return;
        }

        //bool isAdjacent = false;
        //foreach (var tile in selectedTile.OrderedNeighborTiles)
        //{
        //    if (tile.FactionIndex == FactionController.Instance.PlayerFaction)
        //    {
        //        isAdjacent = true;
        //        break;
        //    }
        //}

        //if (isAdjacent == false)
        //{
        //    //No point in depicting a battle that isn't adjacent to player territory, right?
        //    _bpd.HideBattleDepiction();
        //    return;
        //}

        _defensiveHelp = 0;
        _offensiveHelp = 0;
        _neutrals = 0;

        foreach (var tile in selectedTile.ShuffledNeighborTiles)
        {
            if (tile == null) continue;

            if (tile.FactionIndex == FactionController.Instance.PlayerFaction)
            {
                
                _offensiveHelp++;
                //also need to account for 
            }
            else if (tile.FactionIndex == selectedTile.FactionIndex)
            {
                _defensiveHelp += tile.DefendBonus + 1;
            }
            else
            {
                _neutrals++;
            }
        }

        float odds = ((float)_offensiveHelp) / ((float)_defensiveHelp + (float)_offensiveHelp);
        int oddsAsInt = Mathf.RoundToInt(odds * 100f);
        _bpd.DepictBattle(TileController.Instance.TileUnderCursor, oddsAsInt);
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

    private void AttemptAttack(TileHandler clickedTile)
    {
        if (clickedTile.FactionIndex == FactionController.Instance.PlayerFaction)
        {
            //cannot attack ownself
            return;
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

        if (isAdjacent == false)
        {
            //cannot attack non-adjacent tiles
            return;
        }


        int totalOdds = _offensiveHelp + _defensiveHelp;
        int rand = UnityEngine.Random.Range(1, totalOdds+1);
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
    }

    public void SelectTool(Tools newTool)
    {
        _selectedTool = newTool;
        _selectedToolTMP.text = _selectedTool.ToString();
    }
}
