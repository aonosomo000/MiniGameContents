using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMEffectManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> removeEffectList;

    public void ShowMoveDownEffect()
    {

    }
    
    public void ShowRemoveEffect(Vector2 _gemPos, GemMainType _type)
    {
        var effect = Instantiate(removeEffectList[(int)_type], transform);
        effect.transform.position = _gemPos;
        effect.SetActive(true);
    }


}
