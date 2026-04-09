using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ToolController : MonoBehaviour
{

    public static ToolController Instance {get; private set;}
    public enum Tools { Attack, Defend, Strike, Invest}

    //refs
    [SerializeField] BattlePanelDriver _bpd = null;
    [SerializeField] TextMeshProUGUI _selectedToolTMP = null;

    //state
    [SerializeField] Tools _selectedTool = Tools.Attack;
    public Tools SelectedTool => _selectedTool;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SelectTool(Tools.Attack);
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
            case Tools.Attack:
                HandleProspectiveAttack();
                break;


        }
    }

    private void HandleProspectiveAttack()
    {
        Debug.Log("Tick");
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

        float defensiveHelp = 0;
        float offensiveHelp = 0;
        float neutrals = 0;

        foreach (var tile in selectedTile.ShuffledNeighborTiles)
        {
            if (tile == null) continue;

            if (tile.FactionIndex == FactionController.Instance.PlayerFaction)
            {
                offensiveHelp++;
            }
            else if (tile.FactionIndex == selectedTile.FactionIndex)
            {
                defensiveHelp++;
            }
            else
            {
                neutrals++;
            }
        }

        float odds = (offensiveHelp) / (defensiveHelp + offensiveHelp);
        int oddsAsInt = Mathf.RoundToInt(odds * 100f);
        _bpd.DepictBattle(TileController.Instance.TileUnderCursor, oddsAsInt);
    }

    public void SelectTool(Tools newTool)
    {
        _selectedTool = newTool;
        _selectedToolTMP.text = _selectedTool.ToString();
    }
}
