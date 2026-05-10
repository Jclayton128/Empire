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

    public void AddInfluence(int newFaction, int influenceToAdd)
    {
        //trash the next one

        for (int times = 0; times < influenceToAdd; times++)
        {
            for (int i = 0; i < _influenceSlots.Count; i++)
            {
                if (_influenceSlots[i] != newFaction)
                {
                    _influenceSlots[i] = newFaction;
                    break;
                }
            }
        }

        CheckForOwnershipSwapViaInfluence();
        ShowInfluence();
    }
    public void AddInfluenceUntilCompletelyInfluenced(int newFaction)
    {
        AddInfluence(newFaction, _influenceSlots_max);

    }

    private void CheckForOwnershipSwapViaInfluence()
    {
        int ownerInfluence = GetOwnerInfluence();
        int mostInfluentialFaction = GetFactionRankings().x;
        if (_tileHandler.FactionIndex != mostInfluentialFaction)
        {
            //swap ownership
            TileController.Instance.ChangeTileFaction(_tileHandler, _tileHandler.FactionIndex, mostInfluentialFaction);
            _tileHandler.AssignFactionToTile(mostInfluentialFaction, false);
            TileController.Instance.RefreshBorderHighlights();
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
            return;
        }

        Vector3Int influenceRanking = GetFactionRankings();

        float factor = (float)influenceRanking.y / (float)_influenceSlots.Count;
        float mult = Mathf.Lerp(_minCircleScale, _maxCircleScale, factor);
        Vector3 scale = Vector3.one * mult;

        _scaleTween.Kill();
        _scaleTween = _ownerInfluence.transform.DOScale(scale, 0.5f).SetEase(Ease.OutBack);

        _ownerInfluence.color = FactionController.Instance.GetFactionFillColor(influenceRanking.x);
        _secondplaceInfluence.color = FactionController.Instance.GetFactionFillColor(influenceRanking.z);

    }

    

    private void Update()
    {
        if (_tileHandler)
        {
            UpdateTimeBetweenInfluenceGrowths();
            UpdateInfluence();
        }

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
            AddInfluence(_tileHandler.FactionIndex, 1);
            _timeGrowingNewInfluence = 0;
        }
    }

    private Vector3Int GetFactionRankings()
    {
        //x: plurality faction index
        //y: plurality faction influence count
        //z: second faction index
        Vector3Int output = Vector3Int.zero;

        //setup tracking
        int independentInfluence = 0;
        List<int> influencePerFaction = new List<int>();
        for (int faction = 0; faction < FactionController.Instance.FactionCount; faction++)
        {
            influencePerFaction.Add(0);
        }

        //populate tracking with influence counts
        for (int slot = 0; slot < _influenceSlots.Count; slot++)
        {
            int factionInSlot = _influenceSlots[slot];
            if (factionInSlot == -1)
            {
                independentInfluence++;
            }
            else if (factionInSlot == -2)
            {
                //do nothing with ocean
            }
            else
            {
                influencePerFaction[factionInSlot]++;
            }
            //Debug.Log($"fis: {factionInSlot}");

            
        }

        //review tracking to identify plurality and second factions


        int firstPlaceFaction = -1;
        int influenceOfFirstPlaceFaction = independentInfluence;
        //determine first place stats
        for (int faction = 0; faction <influencePerFaction.Count; faction++)
        {
            if (influencePerFaction[faction] > influenceOfFirstPlaceFaction)
            {
                firstPlaceFaction = faction;
                influenceOfFirstPlaceFaction = influencePerFaction[firstPlaceFaction];
            }
        }

        //second pass to identify secondplace stats
        int secondPlaceFaction = -1;
        int influenceOfSecondPlaceFaction = 0;
        for (int faction = 0; faction < influencePerFaction.Count; faction++)
        {
            if (faction != firstPlaceFaction && influencePerFaction[faction] > influenceOfSecondPlaceFaction)
            {
                secondPlaceFaction = faction;
                influenceOfSecondPlaceFaction = influencePerFaction[secondPlaceFaction];
            }
        }

        //TODO next: have this vec3int output get read. x: color/ownership of tile, y: relative hold/scale of owner, z: color of background.
        //This will be a "plurality owns" model. Attacking needs to immediately grant a plurality of influence upon conquering.

        output.x = firstPlaceFaction;
        output.y = influenceOfFirstPlaceFaction;
        output.z = secondPlaceFaction;
        //Debug.Log($"{output}");

        return output;

    }

    
}
