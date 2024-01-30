using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceCheck : MonoBehaviour
{
    [SerializeField] private List<Transform> checkSideList;

    public int CheckFinalResult()
    {
        var y = -9999f;
        var res = 0;

        for(var c = 0; c < checkSideList.Count; c++)
        {
            if(checkSideList[c].position.y > y)
            {
                y = checkSideList[c].position.y;
                res = c;
            }
        }

        return res;
    }
    
}
