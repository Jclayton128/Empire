using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.Http.Headers;

public class FactionController : MonoBehaviour
{
    public static FactionController Instance { get; private set; }

    //refs
    [SerializeField] FactionDriver _factionDriver = null;
    [SerializeField] TextMeshProUGUI _playerEmpireTMP = null;


    //settings

    [SerializeField] int _factionCount = 4;
    [SerializeField] int _factionStartingSize = 4;
    public int FactionCount => _factionCount;
    public int FactionStartingSize => _factionStartingSize;
    [SerializeField] List<Color> _factionColors = new List<Color>();

    [SerializeField] float _productionPerHex_Base = 0.2f;
    public float ProductionPerHex => _productionPerHex_Base;
    [SerializeField] float _minProduction = 1;

    public int ActiveFaction => _playerFaction; //This will eventually be updated to reflect whatever's faction has the turn priority.

    //state

    [SerializeField] int _playerFaction = 0;
    public int PlayerFaction => _playerFaction;

    List<float> _bankedProductions = new List<float>();

    private void Awake()
    {
         Instance = this;
    }


    public Color GetFactionColor(int factionIndex)
    {
        if (factionIndex >= 0 && factionIndex < _factionColors.Count)
        {
            return _factionColors[factionIndex];
        }
        else
        {
            //Debug.LogWarning("this faction doesn't exist");

            //neutral or rebel factions don't have borders
            return Color.clear;
        }
    }

    public Color GetFactionColor_Desaturated(int factionIndex)
    {
        if (factionIndex >= 0 && factionIndex < _factionColors.Count)
        {
            Color col = _factionColors[factionIndex];
            col.r *= 0.5f;
            col.g *= 0.5f;
            col.b *= 0.5f;
            return col;
        }
        else
        {
            //Debug.LogWarning("this faction doesn't exist");
            //neutral or rebel faction fill
            return Color.gray;
        }
    }

    public void SetPlayerFaction(int playerFaction)
    {
        _playerFaction = playerFaction;
        _playerEmpireTMP.text = $"Player: {_playerFaction}";
    }

    public void DisplayFaction(int faction)
    {
        if (faction >= 0 && faction < _factionCount)
        {
            int territory = TileController.Instance.GetFactionTerritory(faction);
            float production = TileController.Instance.GetFactionProduction(faction);
            _factionDriver.SetFaction(faction, territory, production, 0);
        }
        else
        {
            _factionDriver.ClearFaction();
        }

    }
}
