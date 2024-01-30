using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TH_EffectManager : MonoBehaviour
{
    [SerializeField] private int poolCount = 0;
    [SerializeField] private GameObject destroyEffectPrefab;

    private List<GameObject> destroyEffectPool = new List<GameObject>();

    private void Awake()
    {
        for(var p = 0; p < poolCount; p++)
        {
            var effect = Instantiate(destroyEffectPrefab, transform);
            destroyEffectPool.Add(effect);
        }
    }

    public GameObject GetDestoryEffect()
    {
        for(var p = 0; p < destroyEffectPool.Count; p++)
        {
            if(!destroyEffectPool[p].activeInHierarchy)
            {
                return destroyEffectPool[p];
            }
        }
        var effect = Instantiate(destroyEffectPrefab, transform);
        destroyEffectPool.Add(effect);
        return effect;
    }
}
