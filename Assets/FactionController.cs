using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FactionController : MonoBehaviour
{
    public static FactionController Instance { get; private set; }

    //refs
    [SerializeField] FactionDriver _factionDriver = null;
    [SerializeField] TextMeshProUGUI _playerEmpireTMP = null;


    //settings

    [SerializeField] int _factionCount = 4;
    public int FactionCount => _factionCount;
    [SerializeField] List<Color> _factionColors = new List<Color>();

    [SerializeField] float _productionPerHex_Base = 0.2f;
    [SerializeField] float _minProduction = 1;


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
            Debug.LogWarning("this faction doesn't exist");
            return Color.gray;
        }
    }

    public Color GetFactionColor_Desaturated(int factionIndex)
    {
        if (factionIndex < _factionColors.Count)
        {
            Color col = _factionColors[factionIndex];
            col.r *= 0.5f;
            col.g *= 0.5f;
            col.b *= 0.5f;
            return col;
        }
        else
        {
            Debug.LogWarning("this faction doesn't exist");
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
            int territory = TileController.Instance.GetFactionTerritoryCount(faction);
            float production = territory * _productionPerHex_Base;
            production = Mathf.Clamp(production, _minProduction, 99f);
            _factionDriver.SetFaction(faction, territory, production, 0);
        }
        else
        {
            _factionDriver.ClearFaction();
        }

    }
}
