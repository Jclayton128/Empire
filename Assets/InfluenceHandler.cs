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
    int _influenceSlots_max = 9;
    [SerializeField] float _minCircleScale = 1.12f;
    [SerializeField] float _maxCircleScale = 2.4f;

    [SerializeField] float _timeBetweenInfluenceGrowths_Nominal = 10f;

    //state
    [SerializeField] float _timeBetweenInfluenceGrowths_Actual = 10f;
    [SerializeField] float _timeGrowingNewInfluence = 0;
    [SerializeField] List<int> _influenceSlots;
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
        for (int i = 0; i < _influenceSlots.Count; i++)
        {
            if (_influenceSlots[i] != newFaction)
            {
                _influenceSlots[i] = newFaction;
                break;
            }
        }

        //_influenceSlots.RemoveAt(0);
        //_influenceSlots.Add(newFaction);

        //if (_tileHandler.CurrentTileType.TType != TileType.TileTypes.Water)
        //{
        //    int testFaction = GetFirstMostFrequentFaction(_influenceSlots);
        //    if (testFaction != _tileHandler.FactionIndex)
        //    {
        //        //swap ownership
        //        TileController.Instance.ChangeTileFaction(_tileHandler, _tileHandler.FactionIndex, testFaction);
        //        _tileHandler.AssignFactionToTile(testFaction, false);
        //    }
        //}

        int ownerInfluence = GetOwnerInfluence();
        int mostInfluentialFaction = GetFirstMostFrequentFaction(_influenceSlots);
        if (ownerInfluence == 0)
        {
            //swap ownership
            TileController.Instance.ChangeTileFaction(_tileHandler, _tileHandler.FactionIndex, mostInfluentialFaction);
            _tileHandler.AssignFactionToTile(mostInfluentialFaction, false);
        }

        ShowInfluence();
    }

    public void AddInfluenceUntilCompletelyInfluenced(int newFaction)
    {
        for (int i = 0; i < _influenceSlots_max; i++)
        {
            AddSingleInfluence(newFaction);
        }

    }    

    private int GetOwnerInfluence()
    {

        int ownerInfluence = 0;

        foreach (var slot in _influenceSlots)
        {
            if (slot == _tileHandler.FactionIndex)
            {
                ownerInfluence++;
            }
        }

        return ownerInfluence;
    }
   
    private void ShowInfluence()
    {
        if (_tileHandler.FactionIndex < -1)
        {
            _ownerInfluence.color = Color.clear;
            _secondplaceInfluence.color = FactionController.Instance.GetFactionFillColor(-2);
        }

        int ownerInfluence = GetOwnerInfluence();
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

        int? second = GetSecondMostFrequent(_influenceSlots);

        if (second != null)
        {
            _secondplaceInfluence.color = FactionController.Instance.GetFactionFillColor((int)second);
        }
        else
        {
            int first = GetFirstMostFrequentFaction(_influenceSlots);
            _secondplaceInfluence.color = FactionController.Instance.GetFactionFillColor(first);
            //if (second == _tileHandler.FactionIndex)
            //{
            //    //show the firstplace person then
            //    int first = GetFirstMostFrequentFaction(_influenceSlots);
            //    _secondplaceInfluence.color = FactionController.Instance.GetFactionFillColor(first);
            //}
            //else
            //{
            //    _secondplaceInfluence.color = FactionController.Instance.GetFactionFillColor(_tileHandler.FactionIndex);
            //}

        }

    }

    //private int GetSecondMostInfluentialFaction()
    //{
    //    _influenceSlotsByFrequency.Clear();
    //    _influenceSlotsByFrequency = new List<int>(_influenceSlots);

    //    int? secondMostFrequent = _influenceSlotsByFrequency.GroupBy(n => n).OrderByDescending(g => g.Count()).Skip(1).Select(g => (int?)g.Key).FirstOrDefault();

    //    if (secondMostFrequent != null)
    //    {
    //        return (int)secondMostFrequent;
    //    }
    //    else
    //    {
    //        return _tileHandler.FactionIndex;
    //    }

    //    return _influenceSlotsByFrequency[0];
    //}

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

    private int GetFirstMostFrequentFaction(List<int> numbers)
    {
        if (numbers == null || numbers.Count < 2) return _tileHandler.FactionIndex;

        // 1. Count occurrences
        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (int num in numbers)
        {
            if (counts.ContainsKey(num))
                counts[num]++;
            else
                counts[num] = 1;
        }

        // 2. Identify top two frequencies
        int maxFreq = 0;
        int secondMaxFreq = 0;

        foreach (int freq in counts.Values)
        {
            if (freq > maxFreq)
            {
                secondMaxFreq = maxFreq;
                maxFreq = freq;
            }
            else if (freq > secondMaxFreq && freq < maxFreq)
            {
                secondMaxFreq = freq;
            }
        }

        foreach (var pair in counts)
        {
            if (pair.Value == maxFreq)
                return pair.Key;
        }

        return _tileHandler.FactionIndex;

    }

    public int? GetSecondMostFrequent(List<int> numbers)
    {
        if (numbers == null || numbers.Count < 2) return null;

        // 1. Count occurrences
        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (int num in numbers)
        {
            if (counts.ContainsKey(num))
                counts[num]++;
            else
                counts[num] = 1;
        }

        // 2. Identify top two frequencies
        int maxFreq = 0;
        int secondMaxFreq = 0;

        foreach (int freq in counts.Values)
        {
            if (freq > maxFreq)
            {
                secondMaxFreq = maxFreq;
                maxFreq = freq;
            }
            else if (freq > secondMaxFreq && freq < maxFreq)
            {
                secondMaxFreq = freq;
            }
        }

        // 3. Find the integer with the second highest frequency
        if (secondMaxFreq == 0) return null;

        foreach (var pair in counts)
        {
            if (pair.Value == secondMaxFreq)
                return pair.Key;
        }

        return null;
    }
}
