using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CircleEffectType
{ 
    DestroyerCol = 0,
}


public class EffectManager_CoinGame : MonoBehaviour
{
    [SerializeField] private GameObject destroyerColEffect;



    public void ShowEffect(Vector3 _worldPos, CircleEffectType _type, float _scaleMulti = 1f)
    { 
        if(_type == CircleEffectType.DestroyerCol)
        {
            StartCoroutine(PlayEffectCoroutine_DestroyerCol(_worldPos, _scaleMulti));
        }
    }

    private IEnumerator PlayEffectCoroutine_DestroyerCol(Vector3 _worldPos, float _scaleMulti)
    {
        var effect = Instantiate(destroyerColEffect, transform);
        effect.transform.localScale = effect.transform.localScale * _scaleMulti;
        effect.transform.position = _worldPos;

        yield return new WaitForSeconds(0.5f);

        Destroy(effect);
    }
}
