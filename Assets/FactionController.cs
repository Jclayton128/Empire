using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionController : MonoBehaviour
{
    public static FactionController Instance { get; private set; }

    //settings

    [SerializeField] int _factionCount;
    [SerializeField] List<Color> _factionColors = new List<Color>();



    private void Awake()
    {
         Instance = this;
    }

    public Color GetFactionColor(int factionIndex)
    {
        if (factionIndex < _factionColors.Count)
        {
            return _factionColors[factionIndex];
        }
        else
        {
            Debug.LogWarning("this faction doesn't exist");
            return Color.gray;
        }
    }
}
