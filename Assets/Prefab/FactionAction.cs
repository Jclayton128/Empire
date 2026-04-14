using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Faction Action")]
public class FactionAction : ScriptableObject
{
    public enum Actions { Attack, Fortify}


    [SerializeField] Sprite _actionIcon = null;
    public Sprite ActionIcon => _actionIcon;

    
}
