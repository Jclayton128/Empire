using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class InfluenceHandler : MonoBehaviour
{
    //refs

    //[SerializeField] List<SpriteRenderer> _segments = new List<SpriteRenderer>();
    [SerializeField] SpriteRenderer _ownerInfluence = null;
    [SerializeField] SpriteRenderer _secondplaceInfluence = null;
    [SerializeField] TileHandler _tileHandler = null;

    //settings
    int _influenceSlots_max = 10;
    [SerializeField] float _minCircleScale = 1.12f;
    [SerializeField] float _maxCircleScale = 2.4f;

    [SerializeField] float _timeBetweenInfluenceGrowths_Nominal = 10f;

    //state
    [SerializeField] float _timeBetweenInfluenceGrowths_Actual = 10f;
    [SerializeField] float _timeGrowingNewInfluence = 0;
    [SerializeField] List<int> _influenceSlots;
    [SerializeField] List<int> _influenceSlotsByFrequency;
    Tween _scaleTween;

    public void Initialize()
    {
        _influenceSlots = new List<int>();
        for (int i = 0; i < _influenceSlots_max; i++)
        {
            _influenceSlots.Add(-1);
        }

        ShowInfluence();
    }

    public int GetInfluenceForFaction(int faction)
    {
        int score = 0;

        foreach (var testFaction in _influenceSlots)
        {
            if (testFaction == faction)
            {
                score++;
            }
        }

        return score;

    }

    public void AddSingleInfluence(int newFaction)
    {
        //trash the next one
        _influenceSlots.RemoveAt(0);
        _influenceSlots.Add(newFaction);

        ShowInfluence();
    }

    public void AddInfluenceUntilCompletelyInfluenced(int newFaction)
    {
        for (int i = 0; i < _influenceSlots_max; i++)
        {
            AddSingleInfluence(newFaction);
        }

    }    


   
    private void ShowInfluence()
    {
        if (_tileHandler.FactionIndex < -1)
        {
            _ownerInfluence.color = Color.clear;
            _secondplaceInfluence.color = FactionController.Instance.GetFactionFillColor(-2);
        }

        int ownerInfluence = 0;

        foreach (var slot in _influenceSlots)
        {
            if (slot == _tileHandler.FactionIndex)
            {
                ownerInfluence++;
            }
        }

        if (ownerInfluence > 0)
        {
            float factor = (float)ownerInfluence / (float)_influenceSlots.Count;
            float mult = Mathf.Lerp(_minCircleScale, _maxCircleScale, factor);
            Vector3 scale = Vector3.one * mult;

            _scaleTween.Kill();
            _scaleTween = _ownerInfluence.transform.DOScale(scale, 0.5f).SetEase(Ease.OutBack);

            _ownerInfluence.color = FactionController.Instance.GetFactionFillColor(_tileHandler.FactionIndex);
        }
        else
        {
            Debug.Log("Owner has no influence!");
        }

        int second = GetSecondMostInfluentialFaction();
        _secondplaceInfluence.color = FactionController.Instance.GetFactionFillColor(second);

    }

    private int GetSecondMostInfluentialFaction()
    {
        _influenceSlotsByFrequency.Clear();
        _influenceSlotsByFrequency = new List<int>(_influenceSlots);

        _influenceSlotsByFrequency.GroupBy(n => n).OrderByDescending(g => g.Count());
        //.Skip(1)
        //.Select(g => (int?)g.Key)
        //.FirstOrDefault();

        //if (secondMostFrequent != null)
        //{
        //    return (int)secondMostFrequent;
        //}
        //else
        //{
        //    return _tileHandler.FactionIndex;
        //}

        return _influenceSlotsByFrequency[0];
    }

    private void Update()
    {
        if (_tileHandler)
        UpdateTimeBetweenInfluenceGrowths();
        UpdateInfluence();
    }



    private void UpdateTimeBetweenInfluenceGrowths()
    {
        _timeBetweenInfluenceGrowths_Actual = _timeBetweenInfluenceGrowths_Nominal +
            (TileController.Instance.GetDistanceFromPointToFactionBarycenter(transform.position, _tileHandler.FactionIndex) / 2f) -
            (_tileHandler.GetNeighborlyScore(_tileHandler.FactionIndex) / 2f); 

    }

    private void UpdateInfluence()
    {
        _timeGrowingNewInfluence += Time.deltaTime;
        if (_timeGrowingNewInfluence >= _timeBetweenInfluenceGrowths_Actual)
        {
            AddSingleInfluence(_tileHandler.FactionIndex);
            _timeGrowingNewInfluence = 0;
        }
    }
}
