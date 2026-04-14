using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattlePanelDriver : MonoBehaviour
{
    //ref
    [SerializeField] Image _centerSite = null;
    [SerializeField] List<Image> _neighborSites = null;
    [SerializeField] List<TextMeshProUGUI> _neighborBonusTMP = null;
    [SerializeField] TextMeshProUGUI _oddsTMP = null;
    [SerializeField] TextMeshProUGUI _centerTMP = null;

    private void Awake()
    {
        HideBattleDepiction();
    }

    public void HideBattleDepiction()
    {
        gameObject.SetActive(false);
    }

    public void DepictBattle(TileHandler centerTile, int odds)
    {
        gameObject.SetActive(true);

        if (centerTile == null)
        {
            _centerSite.color = Color.clear;
            for (int i = 0; i < _neighborSites.Count; i++)
            {
                _neighborSites[i].color = Color.clear;
                _neighborBonusTMP[i].text = " ";
            }
            _oddsTMP.text = " ";
        }
        else
        {
            _centerSite.color = FactionController.Instance.GetFactionColor(centerTile.FactionIndex);
            _centerTMP.text = $"{1 + centerTile.DefendBonus}";

            for (int i = 0; i < _neighborSites.Count; i++)
            {
                if (centerTile.OrderedNeighborTiles[i] == null)
                {
                    _neighborSites[i].color = Color.clear;
                    _neighborBonusTMP[i].text = " ";
                }
                else
                {
                    _neighborSites[i].color = FactionController.Instance.GetFactionColor(centerTile.OrderedNeighborTiles[i].FactionIndex);

                    if (centerTile.FactionIndex != -1 && centerTile.OrderedNeighborTiles[i].FactionIndex == centerTile.FactionIndex)
                    {
                        int db = centerTile.OrderedNeighborTiles[i].DefendBonus;
                        _neighborBonusTMP[i].text = $"{db + 1}";
                    }
                    else if (centerTile.OrderedNeighborTiles[i].FactionIndex == FactionController.Instance.PlayerFaction)
                    {
                        int ab = centerTile.OrderedNeighborTiles[i].AttackBonus;
                        _neighborBonusTMP[i].text = $"{ab + 1}";
                    }
                    else
                    {
                        _neighborBonusTMP[i].text = " ";
                    }


                }

            }
            _oddsTMP.text = $"{odds.ToString()}%";
        }

           


    }
}
