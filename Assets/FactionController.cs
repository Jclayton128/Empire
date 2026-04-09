using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FactionController : MonoBehaviour
{
    public static FactionController Instance { get; private set; }

    //refs
    [SerializeField] TextMeshProUGUI _playerFactionTMP = null;


    //settings

    [SerializeField] int _factionCount;
    [SerializeField] List<Color> _factionColors = new List<Color>();

    //state

    [SerializeField] int _playerFaction = 0;
    public int PlayerFaction => _playerFaction;

    private void Awake()
    {
         Instance = this;
    }

    private void Start()
    {
        SetPlayerFaction(0);
    }

    public Color GetFactionColor(int factionIndex)
    {
        if (factionIndex < _factionColors.Count)
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
        _playerFactionTMP.text = $"Player: {_playerFaction}";
    }
}
