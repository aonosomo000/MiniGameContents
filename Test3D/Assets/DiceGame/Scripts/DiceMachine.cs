using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Random = UnityEngine.Random;

[Serializable]
public class DiceDropDataList
{
    public List<DiceDropData> diceDropDataList = new List<DiceDropData>();
}


[Serializable]
public class DiceDropData
{
    public List<int> topNumList;
    public List<DiceDropCase> dropCases;

    public DiceDropData()
    {
        topNumList = new List<int>();
        dropCases = new List<DiceDropCase>();
    }
}

[Serializable]
public class DiceDropCase
{
    public List<DropVector3> dropPosRotList;

    public DiceDropCase()
    {
        dropPosRotList = new List<DropVector3>();
    }
}

[Serializable]
public class DropVector3
{
    public string px;
    public string py;
    public string pz;
    public string rx;
    public string ry;
    public string rz;

    public DropVector3(Vector3 pos, Vector3 rot)
    {
        px = pos.x.ToString("0.000;-0.000");
        py = pos.y.ToString("0.000;-0.000");
        pz = pos.z.ToString("0.000;-0.000");
        rx = rot.x.ToString("0.000;-0.000");
        ry = rot.y.ToString("0.000;-0.000");
        rz = rot.z.ToString("0.000;-0.000");
    }
}

public class DiceMachine : MonoBehaviour
{
    public Prediction3D prediction;
    public GameObject dicePrefab;
    public bool isTest = false;

    [SerializeField] private int diceCount = 0;
    private List<GameObject> diceList = new List<GameObject>();

    [SerializeField] private float power;

    [SerializeField] private List<GameObject> firePointList;

    private int finishedCount = 0;

    [SerializeField] private Button cheatButton;
    [SerializeField] private Button diceButton;

    [SerializeField] private DiceDropDataList diceDropDataList = new DiceDropDataList();

    private DiceDropData curDiceDropData = new DiceDropData();
    private List<DiceDropCase> curDiceDropCases = new List<DiceDropCase>();

    private int cheatTotalNum = 2;

    private void Start()
    {
        Init();
        for (var d = 0; d < diceCount; d++)
        {
            var newDice = Instantiate(dicePrefab, firePointList[d].transform);
            diceList.Add(newDice);
            newDice.SetActive(false);
        }
    }

    public void Init()
    {
        StartCoroutine(LoadDataCoroutine());
    }

    public void PlayDice()
    {
        finishedCount = 0;

        StopAllCoroutines();

        LockAllButton(true);

        if (isTest)
        {
            curDiceDropData = new DiceDropData();
            curDiceDropCases = new List<DiceDropCase>();

            StartCoroutine(MoveDicePrediction());
        }
        else
        {
            StartCoroutine(MoveDiceCoroutine());
        }
    }

    public void PlayDiceCheat()
    {
        finishedCount = 0;

        StopAllCoroutines();

        LockAllButton(true);

        StartCoroutine(MoveDiceCoroutine(true));
    }

    private IEnumerator MoveDiceCoroutine(bool isCheat = false)
    {
        var randDropDataIndex = Random.Range(0, diceDropDataList.diceDropDataList.Count);

        var diceDropData = diceDropDataList.diceDropDataList[randDropDataIndex];

        var targetNumberList = new List<int>();

        if (isCheat)
        {
            while (true)
            {
                var remainValue = cheatTotalNum;

                for (var d = 0; d < diceCount; d++)
                {
                    var randNum = Random.Range(1, 7);
                    targetNumberList.Add(randNum);
                    remainValue -= randNum;
                }

                if (remainValue == 0)
                {
                    break;
                }

                targetNumberList.Clear();
            }
        }
        else
        {
            for (var d = 0; d < diceCount; d++)
            {
                var randNum = Random.Range(1, 7);
                targetNumberList.Add(randNum);
            }
        }

        for (var d = 0; d < diceCount; d++)
        {
            RotateDice(d, targetNumberList[d], diceDropData.topNumList[d]);
            var col = diceList[d].GetComponent<BoxCollider>();
            diceList[d].GetComponent<Rigidbody>().isKinematic = true;
            col.isTrigger = true;
            diceList[d].SetActive(true);
        }

        int mvIndex = 0;
        while (finishedCount < diceCount)
        {
            finishedCount = 0;
            for (var d = 0; d < diceCount; d++)
            {
                if (diceDropData.dropCases[d].dropPosRotList.Count < mvIndex + 1)
                {
                    finishedCount++;
                }
            }

            if (finishedCount >= diceCount)
            {
                LockAllButton(false);
                break;
            }

            for (var d = 0; d < diceCount; d++)
            {
                var px = float.Parse(diceDropData.dropCases[d].dropPosRotList[mvIndex].px);
                var py = float.Parse(diceDropData.dropCases[d].dropPosRotList[mvIndex].py);
                var pz = float.Parse(diceDropData.dropCases[d].dropPosRotList[mvIndex].pz);
                var rx = float.Parse(diceDropData.dropCases[d].dropPosRotList[mvIndex].rx);
                var ry = float.Parse(diceDropData.dropCases[d].dropPosRotList[mvIndex].ry);
                var rz = float.Parse(diceDropData.dropCases[d].dropPosRotList[mvIndex].rz);
                diceList[d].transform.position = new Vector3(px, py, pz);
                diceList[d].transform.eulerAngles = new Vector3(rx, ry, rz);
            }
            yield return new WaitForFixedUpdate();
            mvIndex++;
        }
    }
    private Vector3[,] nextNumRot = new Vector3[6,6]
    {
        {new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, -90f), new Vector3(90f, 0f, 0f), new Vector3(0f, 0f, 90f), new Vector3(-90f, 0f, 0f), new Vector3(0f, 0f, -180f)},
        {new Vector3(0f, 0f, 90f), new Vector3(0f, 0f, 0f), new Vector3(0f, 90f, 0f), new Vector3(0f, 0f, 180f), new Vector3(0f, -90f, 0f), new Vector3(0f, 0f, -90f)},
        {new Vector3(-90f, 0f, 0f), new Vector3(0f, -90f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 90f, 0f), new Vector3(180f, 0f, 0f), new Vector3(90f, 0f, 0f)},
        {new Vector3(0f, 0f, -90f), new Vector3(0f, 180f, 0f), new Vector3(0f, -90f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 90f, 0f), new Vector3(0f, 0f, 90f)},
        {new Vector3(90f, 0f, 0f), new Vector3(0f, 90f, 0f), new Vector3(180f, 0f, 0f), new Vector3(0f, -90f, 0f), new Vector3(0f, 0f, 0f), new Vector3(-90f, 0f, 0f)},
        {new Vector3(180f, 0f, 0f), new Vector3(0f, 0f, 90f), new Vector3(-90f, 0f, 0f), new Vector3(0f, 0f, -90f), new Vector3(90f, 0f, 0f), new Vector3(0f, 0f, 0f)}
    };
    //원하는 값이 위로 오도록 내부 오브젝트 회전
    private void RotateDice(int diceIndex, int targetNumber, int topNum)
    {
        var targetAngle = nextNumRot[topNum,targetNumber - 1];

        diceList[diceIndex].transform.GetChild(0).localEulerAngles = targetAngle;
    }

    private IEnumerator MoveDicePrediction()
    {
        for(var d = 0; d < diceCount; d++)
        {
            var col = diceList[d].GetComponent<BoxCollider>();
            diceList[d].GetComponent<Rigidbody>().isKinematic = true;
            col.isTrigger = true;
            diceList[d].SetActive(true);
        }

        int mvIndex = 0;
        while (finishedCount < diceCount)
        {
            finishedCount = 0;
            for (var d = 0; d < diceCount; d++)
            {
                if (prediction.linePositionsList[d].Count < mvIndex + 1)
                {
                    finishedCount++;
                }
            }

            if (finishedCount >= diceCount)
            {
                diceList.ForEach(dice =>
                {
                    curDiceDropData.topNumList.Add(dice.GetComponent<DiceCheck>().CheckFinalResult());
                });

                for(var d = 0; d < diceCount; d++)
                {
                    var diceDropCase = new DiceDropCase();
                    prediction.linePositionsList[d].ForEach(posrot =>
                    {
                        var dropVec = new DropVector3(posrot.Item1, posrot.Item2);
                        diceDropCase.dropPosRotList.Add(dropVec);
                    });

                    curDiceDropCases.Add(diceDropCase);
                }

                curDiceDropData.dropCases = curDiceDropCases;

                LockAllButton(false);
                break;
            }

            for (var d = 0; d < diceCount; d++)
            {


                var px = prediction.linePositionsList[d][mvIndex].Item1.x;
                var py = prediction.linePositionsList[d][mvIndex].Item1.y;
                var pz = prediction.linePositionsList[d][mvIndex].Item1.z;
                var rx = prediction.linePositionsList[d][mvIndex].Item2.x;
                var ry = prediction.linePositionsList[d][mvIndex].Item2.y;
                var rz = prediction.linePositionsList[d][mvIndex].Item2.z;
                diceList[d].transform.position = new Vector3(px, py, pz);
                diceList[d].transform.eulerAngles = new Vector3(rx, ry, rz);
            }
            yield return new WaitForFixedUpdate();
            mvIndex++;
        }
    }
    public void SetTotalNum(TMPro.TMP_Dropdown select)
    {
        cheatTotalNum = select.value + 2;
    }

    public void GoalCheck()
    {

    }
    public void CheckDrop()
    {
    }

    public void CaptureCurrent()
    {
        diceDropDataList.diceDropDataList.Add(curDiceDropData);
    }

    public void ResetData()
    {
        diceDropDataList.diceDropDataList.Clear();
    }

    public void SaveData()
    {
        if (isTest)
        {
            diceDropDataList.diceDropDataList = diceDropDataList.diceDropDataList.Distinct().ToList();

            // ToJson을 사용하면 JSON형태로 포멧팅된 문자열이 생성된다  
            string jsonData = JsonUtility.ToJson(diceDropDataList, true);
            // 데이터를 저장할 경로 지정   
            string path = Path.Combine(Application.dataPath, "DiceDropData.json");
            // 파일 생성 및 저장
            File.WriteAllTextAsync(path, jsonData).Wait();

            Debug.Log("Saved");
        }
    }
    private IEnumerator LoadDataCoroutine()
    {
        // 데이터를 불러올 경로 지정
        string path = Path.Combine(Application.dataPath, "DiceDropData.json");

        if (File.Exists(path))
        {
            string jsonData = "";
            // 파일의 텍스트를 string으로 저장
            yield return jsonData = File.ReadAllTextAsync(path).Result;
            // 이 Json데이터를 역직렬화하여 playerData에 넣어줌
            yield return diceDropDataList = JsonUtility.FromJson<DiceDropDataList>(jsonData);
        }
        else
        {
            Debug.LogError("저장된 데이터가 없습니다");
        }
    }

    private void LockAllButton(bool isLock)
    {
        cheatButton.interactable    = !isLock;
        diceButton.interactable     = !isLock;
    }
}
