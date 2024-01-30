using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum CircleType
{ 
    Coin = 0,
    Wild,
    Bomb,
    Destroyer
}


[Serializable]
public class CircleEditInfo
{
    public int level;
    public float scale;
}

public class CircleDropMachine : MonoBehaviour
{
    public EffectManager_CoinGame effectManager;
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private GameObject destroyerPrefab;
    [SerializeField] private List<GameObject> obstaclePrefabList;

    public List<CircleEditInfo> circleEditList;

    [SerializeField] public Transform spawnPos;
    [SerializeField] private GameObject curDropCircle;
    [SerializeField] private GameObject nextDropCircle;

    [SerializeField] private TMPro.TextMeshProUGUI remainText;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMPro.TextMeshProUGUI endText;

    private int curCircleCount = 0;
    private int curMaxLevel = 1;
    private int curScore = 0;

    [SerializeField] private List<TypeIndex> typeIndexList;
    private List<TypeIndex> originTypeIndexList = new List<TypeIndex>();

    [SerializeField] private List<Sprite> nextCircleImageList;
    [SerializeField] private Image nextCircleImage;
    [SerializeField] private TMPro.TextMeshProUGUI nextCircleText;

    [Serializable]
    public struct LevelRand
    {
        public int level;
        public int rand;
    }
    [Serializable]
    public struct TypeRand
    {
        public CircleType type;
        public int rand;
    }
    [Serializable]
    public class TypeIndex
    {
        public CircleType type;
        public int index;

        public TypeIndex(CircleType _type, int _index)
        {
            type = _type;
            index = _index;
        }
    }

    [SerializeField] private List<LevelRand> levelRandList;
    [SerializeField] private List<TypeRand> typeRandList;
    private int curIndex = 0;

    private Vector2 recentMousePos;

    private void Start()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1f;
        recentMousePos = spawnPos.position;

        for(var t = 0; t < typeIndexList.Count; t++)
        {
            originTypeIndexList.Add(new TypeIndex(typeIndexList[t].type, typeIndexList[t].index));
        }

        InstantiateCircle();
    }

    public void SetMaxLevel(int value)
    {
        if(value > curMaxLevel)
        {
            curMaxLevel = value;
        }
    }

    public void MoveCircleOnMouse(Vector2 point)
    {
        curDropCircle.transform.position = new Vector3(point.x, point.y, 0f);
        recentMousePos = new Vector3(point.x, point.y, 0f);
    }

    public void PlayDrop(Vector2 point)
    {
        curCircleCount++;

        curDropCircle.GetComponent<Rigidbody2D>().simulated = true;

        InstantiateCircle();
    }

    private void InstantiateCircle()
    {
        var nextType = GetNextCircleType();

        if (nextType == CircleType.Destroyer)
        {
            nextDropCircle = Instantiate(destroyerPrefab);
            nextDropCircle.transform.position = recentMousePos;
        }
        else if (nextType == CircleType.Wild)
        {
            nextDropCircle = Instantiate(circlePrefab);
            nextDropCircle.transform.position = recentMousePos;
        }
        else
        {
            nextDropCircle = Instantiate(circlePrefab);
            nextDropCircle.transform.position = recentMousePos;
        }

        var randLevel = Random.Range(0, 100);

        var maxColorIndex = 1;

        if(curScore > 200)
        {
            maxColorIndex = 2;
        }

        var colorType = Random.Range(0, maxColorIndex);

        if (nextType == CircleType.Coin)
        {
            for (var l = 0; l < levelRandList.Count; l++)
            {
                if (randLevel < levelRandList[l].rand)
                {
                    randLevel = levelRandList[l].level;
                    break;
                }
            }
        }
        else
        {
            randLevel = 3;
        }

        nextDropCircle.GetComponent<CombineCircle>().Init(this,
            new CircleInfo(
                curIndex,
                randLevel,
                nextType,
                (CircleColorType)colorType
                )
            );


        curIndex++;
        nextDropCircle.SetActive(true);

        curDropCircle = nextDropCircle;
    }
    public void SetScoreText(int value)
    {
        curScore += value;
        remainText.text = "점수: " + curScore.ToString();
    }
    public void GameEnd()
    {
        Time.timeScale = 0f;
        endPanel.SetActive(true);
        endText.text = "점수: " + curScore.ToString(); 
    }

    public void SceneReload()
    {
        SceneManager.LoadScene(0);
    }

    private CircleType GetNextCircleType()
    {
        var nearDis = 99999;
        var resRemain = 0;
        var nearType = CircleType.Coin;
        var nextType = CircleType.Coin;

        for(var t = 0; t < typeIndexList.Count; t++)
        {
            var remainCount = typeIndexList[t].index - curCircleCount;

            if (remainCount == 0)
            {
                typeIndexList[t].index += originTypeIndexList[t].index;
                resRemain = typeIndexList[t].index - curCircleCount;
                nextType = typeIndexList[t].type;
            }
        }

        for(var n = 0; n < typeIndexList.Count; n++)
        {
            var remainCount = typeIndexList[n].index - curCircleCount;

            if (0 < remainCount && remainCount <= nearDis)
            {
                resRemain = remainCount;
                nearType = typeIndexList[n].type;
                nearDis = remainCount;
            }
        }

        if(nearType == CircleType.Wild)
        {
            nextCircleImage.sprite = nextCircleImageList[0];
        }
        else if (nearType == CircleType.Destroyer)
        {
            nextCircleImage.sprite = nextCircleImageList[1];
        }

        nextCircleText.text = "남은 수: " + resRemain;

        Debug.Log("NextSpecialType: " + nearType.ToString() + " Remain: " + resRemain);

        return nextType;
    }
}
