using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileType")]
public class TileType : ScriptableObject
{
    public enum TileTypes { Plain, Water, Mountain, Fortified, Resourced}

    [SerializeField] TileTypes _tileType = TileTypes.Plain;
    public TileTypes TType => _tileType;

    [TextArea(1,3)]
    [SerializeField] string _typeDescription = "This is a default description;";
    public string TypeDescription => _typeDescription;


    [SerializeField] Sprite _tileIcon = null;
    public Sprite TileIcon => _tileIcon;
}
