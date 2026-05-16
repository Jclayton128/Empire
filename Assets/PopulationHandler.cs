using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PopulationHandler : MonoBehaviour
{
    //private enum PopulationStatus { Unbuildable, Damaged, Healthy}

    [SerializeField] TileHandler _tileHandler = null;
    [SerializeField] List<SpriteRenderer> _nodeSprites = null;

    //settings
    [SerializeField] Sprite _fillableNodeSprite = null;
    [SerializeField] Sprite _filledNodeSprite = null;

    [Header("Resource Parameters")]
    [SerializeField] float _timeBetweenNodeGrowths_nominal = 10f;
    [SerializeField] float _timeBetweenNodeGrowths_randomAdd = 10f;
    [SerializeField] float _timeBetweenNodeGrowths_actual = 10f;

    [SerializeField] int _filledPopulationNodes = 0;
    [SerializeField] int _fillablePopulationNodes = 3;

    [SerializeField] float _currentNodeGrowth = 0;

    //[SerializeField] float _node0_growth;
    //[SerializeField] float _node1_growth;
    //[SerializeField] float _node2_growth;
    //[SerializeField] PopulationStatus _node0_status;
    //[SerializeField] PopulationStatus _node1_status;
    //[SerializeField] PopulationStatus _node2_status;

    //[SerializeField] int _harvestableNodes_Max = 3;
    //public int MaxNodes => _harvestableNodes_Max;
    //[SerializeField] int _harvestableNodes_Damaged = 0;

    public void Initialize()
    {
        _nodeSprites = Shuffle(_nodeSprites);
    }

    public static List<SpriteRenderer> Shuffle(List<SpriteRenderer> list)
    {
        System.Random rnd = new System.Random();
        List<SpriteRenderer> output = new List<SpriteRenderer>(list);
        int n = output.Count;
        while (n > 1)
        {
            n--;
            int k = rnd.Next(n + 1);
            SpriteRenderer value = output[k];
            output[k] = output[n];
            output[n] = value;
        }

        return output;
    }

    public int GetFillableFilledDelta()
    {
        return (_filledPopulationNodes - _filledPopulationNodes);
    }

    public int GetHarvestableNodeAmount()
    {
        //harvest is directly connected to population.
        return _filledPopulationNodes;
    }

    public void SetFillableNodes(int maxNodes)
    {
        _fillablePopulationNodes = maxNodes;
        DepictNodes();      
    }

    [ContextMenu("Damage Node")]
    public void DecrementFillableNode()
    {
        _fillablePopulationNodes--;

        _fillablePopulationNodes = Mathf.Clamp(_fillablePopulationNodes, 0, 6);
        _filledPopulationNodes = Mathf.Clamp(_filledPopulationNodes, 0, _fillablePopulationNodes);

        DepictNodes();
    }

    public void IncrementFillableNode()
    {
        _fillablePopulationNodes++;

        _fillablePopulationNodes = Mathf.Clamp(_fillablePopulationNodes, 0, 6);
        _filledPopulationNodes = Mathf.Clamp(_filledPopulationNodes, 0, _fillablePopulationNodes);

        DepictNodes();
    }

    public void DepictNodes()
    {
        for (int node = 0; node < _nodeSprites.Count; node++)
        {
            if (node < _filledPopulationNodes)
            {

                if (_nodeSprites[node].sprite != _filledNodeSprite)
                {
                    _nodeSprites[node].transform.localScale = Vector3.one * 0.5f;
                    _nodeSprites[node].transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack, 2.0f);
                }
                _nodeSprites[node].sprite = _filledNodeSprite;
            }
            else if (node < _fillablePopulationNodes)
            {
                _nodeSprites[node].sprite = _fillableNodeSprite;
            }
            else
            {
                //if not filled and not fillable, then must be unfillable
                _nodeSprites[node].sprite = null;
            }

        }

        foreach (var node in _nodeSprites)
        {
            node.color = FactionController.Instance.GetFactionBorderColor(_tileHandler.FactionIndex);
        }
    }

    //private Sprite ConvertGrowthIntoStage(float timeSpentGrowing)
    //{
    //    int stage = 0;

    //    float factor = (timeSpentGrowing / _timeBetweenNodeGrowths_actual) * _nodeSprites.Length;
    //    //Debug.Log($"{timeSpentGrowing}/{_timeBetweenNodeGrowths_actual} * {_nodeSprites.Length} = {factor}. F2I: {Mathf.FloorToInt(factor)}", this);
    //    stage = Mathf.FloorToInt(factor);
    //    stage = Mathf.Clamp(stage, 0, _nodeSprites.Length-1);



    //    return _nodeSprites[stage];
    //}


    private void Update()
    {

        ////no growth if an action is happening here.
        //if (_actionHandler.AssignedAction != ActionController.ActionTypes.Undefined) return;

        UpdateNominalTimeBetweenGrowths();

        _currentNodeGrowth += Time.deltaTime;
        if (_currentNodeGrowth >= _timeBetweenNodeGrowths_nominal)
        {
            _currentNodeGrowth = 0;
            _filledPopulationNodes++;
            _filledPopulationNodes = Mathf.Clamp(_filledPopulationNodes, 0, _fillablePopulationNodes);
            DepictNodes();
        }
    }

    private void UpdateNominalTimeBetweenGrowths()
    {
        if (_tileHandler.FactionIndex >= 0)
        {
            _timeBetweenNodeGrowths_actual =
                _timeBetweenNodeGrowths_nominal +
                (TileController.Instance.GetDistanceFromPointToFactionBarycenter(transform.position, _tileHandler.FactionIndex) / 2f);
        }
    }
}
