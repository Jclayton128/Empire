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
    [SerializeField] TextMeshProUGUI _oddsTMP = null;

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
            }
            _oddsTMP.text = " ";
        }
        else
        {
            _centerSite.color = FactionController.Instance.GetFactionColor(centerTile.FactionIndex);

            for (int i = 0; i < _neighborSites.Count; i++)
            {
                if (centerTile.OrderedNeighborTiles[i] == null)
                {
                    _neighborSites[i].color = Color.clear;
                }
                else
                {
                    _neighborSites[i].color = FactionController.Instance.GetFactionColor(centerTile.OrderedNeighborTiles[i].FactionIndex);
                }

            }
            _oddsTMP.text = $"{odds.ToString()}%";
        }

           


    }
}
