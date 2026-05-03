using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeHandler : MonoBehaviour
{

    [SerializeField] TileHandler _tileHandler = null;
    [SerializeField] ActionHandler _actionHandler = null;
    [SerializeField] SpriteRenderer[] _nodes = null;

    //settings
    [SerializeField] Sprite[] _nodeSprites = null;
    [SerializeField] Sprite _damagedNodeSprite = null;
    [SerializeField] Sprite _unbuildableNodeSprite = null;

    [Header("Resource Parameters")]
    [SerializeField] float _timeBetweenNodeGrowths = 5f;
    [SerializeField] float _node0_growth;
    [SerializeField] float _node1_growth;
    [SerializeField] float _node2_growth;

    [SerializeField] int _harvestableNodes_Max = 3;
    [SerializeField] int _harvestableNodes_Damaged = 0;




    public int GetHarvestableNodeAmount()
    {
        int count = 0;
        if (_node0_growth >= 1) count++;
        if (_node1_growth >= 1) count++;
        if (_node2_growth >= 1) count++;

        return count;
    }

    public void ClearNodesUponHarvest()
    {
        if (_node0_growth >= 0) _node0_growth = 0;
        if (_node1_growth >= 0) _node1_growth = 0;
        if (_node2_growth >= 0) _node2_growth = 0;

        DepictNodes();
    }

    public void SetMaxNodes(int maxNodes)
    {
        if (maxNodes == 0)
        {
            //must be a water hex that'll never have usable nodes
            _nodes[0].enabled = false;
            _nodes[1].enabled = false;
            _nodes[2].enabled = false;
            _node0_growth = -1f;
            _node1_growth = -1f;
            _node2_growth = -1f;
        }

        if (maxNodes == 1)
        {
            _node0_growth = 0f;
            _node1_growth = -1f;
            _node2_growth = -1f;
        }
        else if (maxNodes == 2)
        {
            _node0_growth = 0f;
            _node1_growth = 0f;
            _node2_growth = -1f;
        }
        else if (maxNodes == 3)
        {
            _node0_growth = 0f;
            _node1_growth = 0f;
            _node2_growth = 0f;
        }


        DepictNodes();
    }

    [ContextMenu("Damage Node")]
    public void DamageNode()
    {
        if (_node0_growth >= 0)
        {
            _node0_growth = -2f;
        }
        else if (_node1_growth >= 0)
        {
            _node1_growth = -2f;
        }
        else if (_node2_growth >= 0)
        {
            _node2_growth = -2f;
        }
        else
        {
            Debug.Log("can't damage this hex anymore");
        }

        DepictNodes();
    }

    private void DepictNodes()
    {
        if (_node0_growth <= -2f)
        {
            _nodes[0].sprite = _damagedNodeSprite;
        }
        if (_node1_growth <= -2f)
        {
            _nodes[1].sprite = _damagedNodeSprite;
        }
        if(_node2_growth <= -2f)
        {
            _nodes[2].sprite = _damagedNodeSprite;
        }

        if (_node0_growth > -2f && _node0_growth < 0)
        {
            _nodes[0].sprite = _unbuildableNodeSprite;
        }
        if(_node1_growth > -2f && _node1_growth < 0)
        {
            _nodes[1].sprite = _unbuildableNodeSprite;
        }
        if(_node2_growth > -2f && _node2_growth < 0)
        {
            _nodes[2].sprite = _unbuildableNodeSprite;
        }

        if (_node0_growth >= 0) _nodes[0].sprite = ConvertGrowthIntoStage(_node0_growth);
        if (_node1_growth >= 0) _nodes[1].sprite = ConvertGrowthIntoStage(_node1_growth);
        if (_node2_growth >= 0) _nodes[2].sprite = ConvertGrowthIntoStage(_node2_growth);


        foreach (var node in _nodes)
        {
            node.color = FactionController.Instance.GetFactionBorderColor(_tileHandler.FactionIndex);
        }
    }

    private Sprite ConvertGrowthIntoStage(float timeSpentGrowing)
    {
        int stage = 0;

        float factor = (timeSpentGrowing / _timeBetweenNodeGrowths) * _nodeSprites.Length;
       //Debug.Log($"{timeSpentGrowing}/{_timeBetweenNodeGrowths} * {_nodeSprites.Length} = {factor}. F2I: {}")
        stage = Mathf.FloorToInt(factor);
        stage = Mathf.Clamp(stage, 0, _nodeSprites.Length-1);
        return _nodeSprites[stage];
    }


    private void Update()
    {
        //no growth if an action is happening here.
        if (_actionHandler.AssignedAction != ActionController.ActionTypes.Undefined) return;

        if (_node0_growth >= 0 && _node0_growth < _timeBetweenNodeGrowths)
        {
            _node0_growth += Time.deltaTime;
        }
        else if (_node1_growth >= 0 && _node1_growth < _timeBetweenNodeGrowths)
        {
            _node1_growth += Time.deltaTime;
        }
        else if (_node2_growth >= 0 && _node2_growth < _timeBetweenNodeGrowths)
        {
            _node2_growth += Time.deltaTime;
        }

        DepictNodes();

    }

}
