using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeHandler : MonoBehaviour
{

    [SerializeField] SpriteRenderer[] _nodes = null;

    //settings

    [SerializeField] float _jitterRange = 0.2f;
    [SerializeField] Sprite[] _nodeSprites = null;

    private void Awake()
    {
        RandomizeNodeLocations();
    }

    private void RandomizeNodeLocations()
    {
        foreach (var node in _nodes)
        {
            Vector3 jit = UnityEngine.Random.insideUnitCircle.normalized * _jitterRange;
            node.transform.localPosition += jit;
        }
    }

    public void SetNodes(int node0, int node1, int node2)
    {
        _nodes[0].sprite = _nodeSprites[node0];
        _nodes[1].sprite = _nodeSprites[node1];
        _nodes[2].sprite = _nodeSprites[node2];
    }
    
}
