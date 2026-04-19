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
    [SerializeField] TextMeshProUGUI _centerTMP = null;
    [SerializeField] TextMeshProUGUI _oddsTMP = null;
    [SerializeField] TextMeshProUGUI _attackCountTMP = null;
    [SerializeField] TextMeshProUGUI _defendCountTMP = null;


    private void Awake()
    {
        HideBattleDepiction();
    }

    public void HideBattleDepiction()
    {
        gameObject.SetActive(false);
    }

    public void DepictBattle(int defendingFactionIndex,
        int centerTileValue, List<int> orderedNeighborTileValues, List<int> orderedNeighborTileFactions,
        int attackCount, int defendCount, int odds)
    {
        gameObject.SetActive(true);

        if (centerTileValue == -1)
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
            _centerSite.color = FactionController.Instance.GetFactionFillColor(defendingFactionIndex);
            _centerTMP.text = centerTileValue.ToString();

            for (int i = 0; i < _neighborSites.Count; i++)
            {
                if (orderedNeighborTileValues[i] == -1)
                {
                    _neighborSites[i].color = Color.clear;
                    _neighborBonusTMP[i].text = " ";
                }
                else
                {
                    _neighborSites[i].color = FactionController.Instance.GetFactionFillColor(orderedNeighborTileFactions[i]);
                    _neighborBonusTMP[i].text = orderedNeighborTileValues[i].ToString();                   
                }

            }
            _oddsTMP.text = $"{odds.ToString()}%";
            _attackCountTMP.text = attackCount.ToString();
            _defendCountTMP.text = defendCount.ToString();
        }

           


    }
}
