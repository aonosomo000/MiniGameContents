using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TreasureHunterManager;

public class Treasure : MonoBehaviour
{
    [Serializable]
    public class TreasurePart
    {
        public BlockCoord coord;
        public GameObject partObject;
    }

    [SerializeField] private List<TreasurePart> partList;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject totalPartObj;

    private TreasureHunterManager manager;

    private bool isAnimationPlay = false;
    private BlockCoord startCoord;

    public bool isFind { get; private set; }


    public void SetTreasure(List<Vector3> _correctPosisionList, BlockCoord _startCoord, TreasureHunterManager _manager)
    {
        manager = _manager;
        startCoord = _startCoord;

        var centerPos = new Vector3(0f, 0f, 0f);

        for (var c = 0; c < _correctPosisionList.Count; c++)
        {
            centerPos += new Vector3(
                    _correctPosisionList[c].x,
                    0,
                    _correctPosisionList[c].z
                    );
        }

        totalPartObj.transform.position = new Vector3(
            centerPos.x / (float)_correctPosisionList.Count,
            - 0.25f,
            centerPos.z / (float)_correctPosisionList.Count
            );

        for (var p = 0; p < partList.Count; p++)
        {
            partList[p].partObject.transform.position =
                new Vector3(
                    _correctPosisionList[p].x,
                    _correctPosisionList[p].y - 0.25f,
                    _correctPosisionList[p].z
                    );
        }
    }

    public IEnumerator CheckTreasureFind(List<List<THBlock>> blockList, FirstCoordType[,] firstCoordList)
    {
        if(!isFind)
        {
            var correctCount = 0;
            
            for(var z = 0; z < blockList.Count; z++)
            {
                for(var x = 0; x < blockList[z].Count; x++)
                {
                    if (firstCoordList[x, z] == FirstCoordType.Destroyed)
                    {
                        for (var c = 0; c < partList.Count; c++)
                        {
                            if (x == partList[c].coord.x + startCoord.x &&
                                z == partList[c].coord.z + startCoord.z)
                            {

                                correctCount++;
                            }
                        }
                    }
                }
            }

            if(correctCount == partList.Count)
            {
                Debug.Log("Find!");
                isFind = true;
                yield return PlayFindTreasure();
            }
        }
    }

    public List<TreasurePart> GetPartList()
    {
        return partList;
    }

    public IEnumerator PlayFindTreasure()
    {
        isAnimationPlay = true;

        anim.SetTrigger("Find");

        while (isAnimationPlay)
        {
            yield return new WaitForFixedUpdate();
        }
        manager.TreasureFound();
    }

    public void AnimationFinished()
    {
        isAnimationPlay = false;
    }
}
