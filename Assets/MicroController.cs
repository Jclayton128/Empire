using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicroController : MonoBehaviour
{
    public static MicroController Instance { get; private set; }

    //settings

    [SerializeField] MicroHandler _microPrefab = null;

    //state
    [SerializeField] List<MicroHandler> _microsInAction = new List<MicroHandler>();

    private void Awake()
    {
        Instance = this;
    }

    public MicroHandler SpawnMicro()
    {
        var newMicro = Instantiate(_microPrefab);
        _microsInAction.Add(newMicro);
        return newMicro;
    }
}
