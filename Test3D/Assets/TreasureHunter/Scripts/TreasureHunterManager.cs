using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TreasureHunterManager : MonoBehaviour
{
    [Serializable]
    public class Block
    {
        public DestroyType type;
        public int randNum;
    }

    public enum FirstCoordType
    { 
        Open=0,
        Closed,
        Destroyed
    }

    [SerializeField] private Camera camera;
    [SerializeField] private TH_EffectManager effectManager;
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private Transform blockGroup;
    [SerializeField] private Transform treasureGroup;

    [SerializeField] private int blockXCount;
    [SerializeField] private int blockZCount;

    [SerializeField] private float width;
    [SerializeField] private float height;

    private FirstCoordType[,] firstCoordList; //트레져 배치 좌표 리스트
    private List<List<THBlock>> blockList = new List<List<THBlock>>();
    private int treasureCount = 3;
    [SerializeField] private List<GameObject> treasurePrefabList = new List<GameObject>();
    [SerializeField] private List<GameObject> treasureSmallPrefabList = new List<GameObject>();
    private List<GameObject> treasureList = new List<GameObject>();

    [SerializeField] private List<Block> destroyRandList = new List<Block>();

    private bool isPlayBlock = false;

    private THBlock curBlock = null;

    [SerializeField] private float startHighlightAlpha;
    [SerializeField] private float highlightAlphaMulti;

    [SerializeField] private TMPro.TextMeshProUGUI remainText;
    [SerializeField] private Button createBtn;

    private void Init()
    {
        for (var z = 0; z < blockList.Count; z++)
        {
            for (var x = 0; x < blockList[z].Count; x++)
            {
                Destroy(blockList[z][x].gameObject);
            }
            blockList[z].Clear();
        }
        blockList.Clear();

        firstCoordList = new FirstCoordType[blockXCount, blockZCount];
        treasureList.Clear();
    }

    public void SetField()
    {
        Init();

        createBtn.interactable = false;

        var startX = width / (blockXCount * 2f) - width * 0.5f;
        var startZ = height / (blockZCount * 2f) - height * 0.5f;

        //블록 생성
        for (var z = 0; z < blockZCount; z++)
        {
            var xBlockList = new List<THBlock>();
            var zPos = startZ + (z * height / blockZCount);
            for (var x = 0; x < blockXCount; x++)
            {
                var xPos = startX + (x * width / blockXCount);
                var block = Instantiate(blockPrefab, blockGroup);
                block.transform.localPosition = new Vector3(xPos,0.5f, zPos);
                var thBlock = block.GetComponent<THBlock>();
                thBlock.coord = new BlockCoord(x, z);

                var typeRand = Random.Range(0, 100);
                DestroyType type = DestroyType.Normal;
                var randomTotal = 0;
                for (var r = 0; r < destroyRandList.Count; r++)
                {
                    randomTotal += destroyRandList[r].randNum;
                    if (typeRand < randomTotal)
                    {
                        type = destroyRandList[r].type;
                        thBlock.type = type;
                        break;
                    }
                }

                xBlockList.Add(thBlock);
            }
            blockList.Add(xBlockList);
        }

        //Treasure 배치
        treasureCount = 3;

        for (var x = 0; x < blockXCount; x++)
        {
            for (var z = 0; z < blockZCount; z++)
            {
                firstCoordList[x, z] = FirstCoordType.Open;
            }
        }

        for (var t = 0; t < treasureCount; t++)
        {
            if(blockXCount < 7 || blockZCount < 7)
            {
                var treasure = Instantiate(treasureSmallPrefabList[t], treasureGroup);
                treasureList.Add(treasure);
                SetTreasureValid(treasure.GetComponent<Treasure>());
            }
            else
            {
                var treasure = Instantiate(treasurePrefabList[t], treasureGroup);
                treasureList.Add(treasure);
                SetTreasureValid(treasure.GetComponent<Treasure>());
            }
        }

        remainText.text = "남은 보물 " + treasureCount.ToString() + "개";

        SetCameraYpos();
    }

    private void SetCameraYpos()
    {
        var baseY = 30f;

        if(blockXCount >= blockZCount)
        {
            baseY += (blockXCount - 4) * 7f;
        }
        else
        {
            baseY += (blockZCount - 4) * 4f;
        }

        camera.transform.position = new Vector3(0f, baseY, 0f);
    }

    private void SetTreasureValid(Treasure treasure)
    {
        var isInvalid = true;
        while(isInvalid)
        {
            var randStartCoord = new BlockCoord(Random.Range(0, blockXCount), Random.Range(0, blockZCount));

            var coordList = treasure.GetPartList();

            var correctCount = 0;
            var correctPosisionList = new List<Vector3>();
            List<(int, int)> openCheckList = new List<(int, int)>();

            for (var c = 0; c < coordList.Count; c++)
            {
                var x = coordList[c].coord.x + randStartCoord.x;
                var z = coordList[c].coord.z + randStartCoord.z;
                if (x < 0 || blockXCount <= x ||
                    z < 0 || blockZCount <= z)
                {
                    break;
                }

                if (firstCoordList[x, z] == FirstCoordType.Closed)
                {
                    break;
                }

                openCheckList.Add((x, z));

                correctPosisionList.Add(blockList[z][x].transform.position);
                correctCount++;
            }

            if(correctCount == coordList.Count)
            {
                for(var o = 0; o < openCheckList.Count; o++)
                {
                    firstCoordList[openCheckList[o].Item1, openCheckList[o].Item2] = FirstCoordType.Closed; 
                }

                treasure.SetTreasure(correctPosisionList, new BlockCoord(openCheckList[0].Item1, openCheckList[0].Item2), this);
                treasure.gameObject.SetActive(true);
                isInvalid = false;
            }
        }
    }
    public void SetX(TMPro.TMP_Dropdown select)
    {
        blockXCount = select.value+4;
        width = blockXCount;
    }
    public void SetZ(TMPro.TMP_Dropdown select)
    {
        blockZCount = select.value+4;
        height = blockZCount;
    }
    private void FixedUpdate()
    {
        if (isPlayBlock)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int layerMask = 1 << LayerMask.NameToLayer("BoxCol");  // BoxCol 레이어만 충돌 체크함
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, layerMask))
        {
            for(var z = 0; z < blockList.Count; z++)
            {
                for(var x = 0; x < blockList[z].Count; x++)
                {
                    if (blockList[z][x] == hit.transform.GetComponent<THBlock>())
                    {
                        blockList[z][x].ShowHighLight(0.1f);

                        curBlock = blockList[z][x];
                    }
                    else
                    {
                        blockList[z][x].ShowHighLight(0f);
                    }
                }
            }
        }

        if (curBlock != null)
        {
            if (Input.GetMouseButton(0))
            {
                StartCoroutine(PlayBlockCoroutine());
            }
        }

        curBlock = null;
    }

    private IEnumerator PlayBlockCoroutine()
    {
        isPlayBlock = true;

        var destroyList = new List<THBlock>();

        destroyList.Add(curBlock);

        if (curBlock.type == DestroyType.CrossX)
        {
            var coordList = new List<(int, int)>();

            coordList.Add((1, -1));
            coordList.Add((2, -2));
            coordList.Add((-1, -1));
            coordList.Add((-2, -2));

            coordList.Add((1, 1));
            coordList.Add((2, 2));
            coordList.Add((-1, 1));
            coordList.Add((-2, 2));
            
            for (var c = 0; c < coordList.Count; c++)
            {
                var zIndex = curBlock.coord.z + coordList[c].Item1;
                var xIndex = curBlock.coord.x + coordList[c].Item2;

                if (0 <= zIndex && zIndex < blockZCount &&
                    0 <= xIndex && xIndex < blockXCount)
                {
                    destroyList.Add(blockList[zIndex][xIndex]);
                }
            }
        }
        else if(curBlock.type == DestroyType.Horizontal)
        {
            var coordList = new List<(int, int)>();

            coordList.Add((0, -1));
            coordList.Add((0, -2));
            coordList.Add((0, 1));
            coordList.Add((0, 2));

            for (var c = 0; c < coordList.Count; c++)
            {
                var zIndex = curBlock.coord.z + coordList[c].Item1;
                var xIndex = curBlock.coord.x + coordList[c].Item2;

                if (0 <= zIndex && zIndex < blockZCount &&
                    0 <= xIndex && xIndex < blockXCount)
                {
                    destroyList.Add(blockList[zIndex][xIndex]);
                }
            }
        }
        else if (curBlock.type == DestroyType.Vertical)
        {
            var coordList = new List<(int, int)>();

            coordList.Add((-1, 0));
            coordList.Add((-2, 0));
            coordList.Add((1, 0));
            coordList.Add((2, 0));

            for (var c = 0; c < coordList.Count; c++)
            {
                var zIndex = curBlock.coord.z + coordList[c].Item1;
                var xIndex = curBlock.coord.x + coordList[c].Item2;

                if (0 <= zIndex && zIndex < blockZCount &&
                    0 <= xIndex && xIndex < blockXCount)
                {
                    destroyList.Add(blockList[zIndex][xIndex]);
                }
            }
        }

        for(var d = 0; d < destroyList.Count; d++)
        {
            firstCoordList[destroyList[d].coord.x, destroyList[d].coord.z] = FirstCoordType.Destroyed;
        }

        var opacity = startHighlightAlpha;
        var addAlpha = startHighlightAlpha;
        while(opacity < 1f)
        {
            addAlpha *= highlightAlphaMulti;
            opacity += addAlpha;

            yield return new WaitForSeconds(0.01f);

            for(var d = 0; d < destroyList.Count; d++)
            {
                destroyList[d].ShowHighLight(opacity);
            }
        }

        yield return new WaitForSeconds(0.1f);

        var effectCacheList = new List<GameObject>();

        for (var d = 0; d < destroyList.Count; d++)
        {
            destroyList[d].ShowHighLight(startHighlightAlpha);

            if (destroyList[d].gameObject.activeInHierarchy)
            {
                var effect = effectManager.GetDestoryEffect();

                effect.transform.position = new Vector3(
                    destroyList[d].transform.position.x,
                    destroyList[d].transform.position.y + 1f,
                    destroyList[d].transform.position.z
                    );

                effect.SetActive(true);
                effectCacheList.Add(effect);
            }
            destroyList[d].DestroyBlock();
            destroyList[d].ShowHighLight(0f);
        }

        yield return new WaitForSeconds(0.7f);

        for (var e = 0; e < effectCacheList.Count; e++)
        {
            effectCacheList[e].SetActive(false);
        }

        for(var t = 0; t < treasureList.Count; t++)
        {
            var treasure = treasureList[t].GetComponent<Treasure>();
            if (!treasure.isFind)
            {
                yield return treasure.CheckTreasureFind(blockList, firstCoordList);
            }
        }


        isPlayBlock = false;
    }

    public void TreasureFound()
    {
        treasureCount--;

        if(treasureCount == 0)
        {
            createBtn.interactable = true;

            remainText.text = "클리어";
            return;
        }

        remainText.text = "남은 보물 " + treasureCount.ToString() + "개";
    }
}
