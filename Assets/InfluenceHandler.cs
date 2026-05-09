using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceHandler : MonoBehaviour
{
    //refs

    [SerializeField] List<SpriteRenderer> _segments = new List<SpriteRenderer>();
    [SerializeField] TileHandler _tileHandler = null;

    //state

    [SerializeField] List<int> _currentFactionInfluences;

    public void Initialize()
    {
       
        _currentFactionInfluences = new List<int>();
        for (int i = 0; i < _segments.Count; i++)
        {
            _currentFactionInfluences.Add(i);
            _currentFactionInfluences[i] = -1;
        }
        SetSegmentGraphics();
    }

    public int GetInfluenceScoreForFaction(int faction)
    {
        int score = 0;

        foreach (var testFaction in _currentFactionInfluences)
        {
            if (testFaction == faction)
            {
                score++;
            }
        }

        return score;

    }

    public void SetInfluenceTotal(int newFaction)
    {
        if (newFaction >= 0)
        {
            for (int i = 0; i < _segments.Count; i++)
            {
                _currentFactionInfluences[i] = newFaction;
            }
            SetSegmentGraphics();
        }
        else
        {
            for (int segment = 0; segment < _segments.Count; segment++)
            {
                _segments[segment].color = Color.clear;
            }
        }



    }

    public void SetInfluenceSingle(int newFaction)
    {
        int segmentToBeat = -1;
        int scoreToBeat = 0;

        for (int i = 0; i < _tileHandler.OrderedNeighborTiles.Count; i++)
        {

            if (_currentFactionInfluences[i] != newFaction && 
                _tileHandler.OrderedNeighborTiles[i].TileInfluenceHandler.GetInfluenceScoreForFaction(newFaction) > scoreToBeat)
            {
                scoreToBeat = _tileHandler.OrderedNeighborTiles[i].TileInfluenceHandler.GetInfluenceScoreForFaction(newFaction);
                segmentToBeat = i;
                //great, we found an adjacent influence segment taht matches the new owner.

            }
        }

        if (segmentToBeat >= 0)
        {
            _currentFactionInfluences[segmentToBeat] = newFaction;
            SetSegmentGraphics();
            return;
        }
        
        List<int> nonconformingSegments = new List<int>();

        for (int i = 0; i < _currentFactionInfluences.Count; i++)
        {
            if (_currentFactionInfluences[i] != newFaction)
            {
                nonconformingSegments.Add(i);
            }
        }

        if (nonconformingSegments.Count == 0)
        {
            Debug.Log($"This faction is already fully influenced to {newFaction}");
            return;
        }


        int rand = UnityEngine.Random.Range(0, nonconformingSegments.Count);
        _currentFactionInfluences[rand] = newFaction;

        SetSegmentGraphics();
    }

    private void SetSegmentGraphics()
    {
        for (int segment = 0; segment < _segments.Count; segment++)
        {
            _segments[segment].color = FactionController.Instance.GetFactionFillColor(_currentFactionInfluences[segment]);
        }
    }
}
