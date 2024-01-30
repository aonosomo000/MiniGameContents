using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[Serializable]
public class DropData
{
    public int targetIndex;
    public List<int> secondGoalCountList;
    public List<DropCase> dropCases;

    public DropData(int secondGoalCount)
    {
        targetIndex = 0;
        secondGoalCountList = new List<int>();
        for (var sg = 0; sg < secondGoalCount; sg++)
        {
            secondGoalCountList.Add(0);
        }
        dropCases = new List<DropCase>();
    }
}


[Serializable]
public class DropCoord
{
    public string x;
    public string y;

    public DropCoord(string _x, string _y)
    {
        x = _x;
        y = _y;
    }
}


[Serializable]
public class DropCase
{
    public List<SecondGoalIndexList> secondGoalIndexList;
    public List<DropCoord> dropCoordList;

    public DropCase()
    {
        secondGoalIndexList = new List<SecondGoalIndexList>();
        dropCoordList = new List<DropCoord>();
    }
}

[Serializable]
public class SecondGoalIndexList
{
    public List<int> indexList;

    public SecondGoalIndexList()
    {
        indexList = new List<int>();
    }
}

[Serializable]
public class PinballData
{
    public List<DropDataList> allDropDataList = new List<DropDataList>();
}

[Serializable]
public class DropDataList
{
    public List<DropData> dropDataList = new List<DropData>();
}

public class DropMachine : MonoBehaviour
{
    public GameObject dropBall;
    public bool isTest = false;
    public Prediction2D prediction2d;
    public float checkDelay = 0f;
    private bool isCheat = true;

    public Shooter2D shooter;
    public List<Transform> startPosList;
    public List<GameObject> finalGoalList;
    public List<GameObject> secondGoalList;

    public float checkWidth = 0f;
    public float checkHeight = 0f;
    public float exceptionXwidth = 0f;


    public int maxCaseCount = 0;

    [SerializeField] private int maxXCount = 100;
    [SerializeField] private int maxYCount = 100;
    private int curXCount = 0;
    private int curYCount = 0;
    private int curIndex = 0;
    private List<DropData> prevDropDataList = new List<DropData>();
    [SerializeField] private int totalCount = 0;
    private float curXpos = 0f;
    private float curYpos = 0f;
    private bool isGoalCheck = false;

    [SerializeField] private DropData curDropData;
    private DropCase curDropCase;
    private DropCoord curDropCoord;

    private PinballData pinballData = new PinballData();

    private int cheatTargetIndex = 0;
    private int cheatSecondGoal1 = 0;
    private int cheatSecondGoal2 = 0;

    private DateTime startTime;

    [Space]
    #region UI 관련
    [Space]
    [SerializeField] private Button cheatBtn;
    [SerializeField] private TMPro.TextMeshProUGUI cheatBtnTmp;
    [SerializeField] List<Button> startButtonList;
    [SerializeField] private TMPro.TextMeshProUGUI warnCheatText;


    [Space]
    [SerializeField] private List<TMPro.TextMeshProUGUI> cCountTextList;
    [SerializeField] private Button getDataBtn;
    [SerializeField] private Slider getDataSlider;
    [SerializeField] private TMPro.TextMeshProUGUI getDataSliderText;
                     
    [SerializeField] private Button ballSelectBtn;
    [SerializeField] private Button obstacleSelectBtn;

    //public Slider widthSlider;
    //public Slider heightSlider;
    //public TMPro.TMP_InputField widthInput;
    //public TMPro.TMP_InputField heightInput;
    //public TMPro.TextMeshProUGUI widthSliderText;
    //public TMPro.TextMeshProUGUI heightSliderText;
    #endregion

    private void Start()
    {
        //Physics2D.simulationMode = SimulationMode2D.Script;
        //
        //currentScene = SceneManager.GetActiveScene();
        //currentPhysicsScene = currentScene.GetPhysicsScene2D();
        //lineRenderer = GetComponent<LineRenderer>();

        getDataSlider.gameObject.SetActive(false);

        warnCheatText.text = "";
        SwitchGetDataUI(false);
        InteractableBottomButton(false);

        startButtonList.ForEach(btn =>
        {
            btn.interactable = false;
        });

        StartCoroutine(LoadDataCoroutine());
    }
    void Update()
    {
        CheckNextDrop();
        //if (currentPhysicsScene.IsValid())
        //{
        //    currentPhysicsScene.Simulate(Time.fixedDeltaTime);
        //}
    }


    private IEnumerator LoadDataCoroutine()
    {
        // 데이터를 불러올 경로 지정
        string path = Path.Combine(Application.dataPath, "DropData.json");

        if (File.Exists(path))
        {
            string jsonData = "";
            // 파일의 텍스트를 string으로 저장
            yield return jsonData = File.ReadAllTextAsync(path).Result;
            // 이 Json데이터를 역직렬화하여 playerData에 넣어줌
            yield return pinballData = JsonUtility.FromJson<PinballData>(jsonData);


            startButtonList.ForEach(btn =>
            {
                btn.interactable = true;
            });

            InteractableTopButton(true);
        }
        else
        {
            InteractableTopButton(false);
            warnCheatText.text = "데이터가 없습니다.\n[데이터 수집 필요]";
        }

        InteractableBottomButton(true);
    }

    #region 치트 관련
    public void SetCheatOn()
    {
        isCheat = isCheat != true;

        if(isCheat)
        {
            cheatBtnTmp.text = "치트\n적용\nON";
        }
        else
        {
            cheatBtnTmp.text = "치트\n적용\nOFF";
        }
    }
    public void SetTargetIndex(TMPro.TMP_Dropdown select)
    {
        cheatTargetIndex = select.value;
    }
    public void SetC1(TMPro.TMP_Dropdown select)
    {
        cheatSecondGoal1 = select.value;
    }
    public void SetC2(TMPro.TMP_Dropdown select)
    {
        cheatSecondGoal2 = select.value;
    }
    #endregion

    public void DropBall(int startIndex)
    {
        warnCheatText.text = "";
        curDropData = new DropData(secondGoalList.Count);
        var dropCase = new DropCase();
        var randDropCoord = new DropCoord("0", "0");

        getDataBtn.interactable = false;
        getDataSlider.value = 0;
        InteractableBottomButton(false);

        startButtonList.ForEach(btn =>
        {
            btn.interactable = false;
        });

        cCountTextList.ForEach(text =>
        {
            text.text = "";
        });
        isGoalCheck = true;
        dropBall.SetActive(true);

        if (isCheat)
        {
            dropCase = GetDropCase(startIndex, cheatTargetIndex, cheatSecondGoal1, cheatSecondGoal2);
            if (dropCase == null)
            {
                Debug.Log("치트에 해당하는 케이스가 없음 (더 많은 데이터 필요)");
                warnCheatText.text = "치트에 부합하는 데이터가 없습니다.";
                getDataSlider.gameObject.SetActive(false);
            }
            else
            {
                StartCoroutine(MoveDropBall(dropCase));
                return;
            }
        }
        getDataBtn.interactable = true;
        getDataSlider.value = 0;
        InteractableBottomButton(true);

        startButtonList.ForEach(btn =>
        {
            btn.interactable = true;
        });

        cCountTextList.ForEach(text =>
        {
            text.text = "";
        });
        isGoalCheck = false;
        dropBall.SetActive(false);
    }

    public void DropRandomCase()
    {
        var randStart = Random.Range(0, 5);
        var ddl = pinballData.allDropDataList[randStart];
        var randData = Random.Range(0, ddl.dropDataList.Count);
        var dcs = ddl.dropDataList[randData].dropCases;
        var randCase = Random.Range(0, dcs.Count);

        warnCheatText.text = "";
        curDropData = new DropData(secondGoalList.Count);
        var dropCase = new DropCase();
        var randDropCoord = new DropCoord("0", "0");

        getDataBtn.interactable = false;
        getDataSlider.value = 0;
        InteractableBottomButton(false);

        startButtonList.ForEach(btn =>
        {
            btn.interactable = false;
        });

        cCountTextList.ForEach(text =>
        {
            text.text = "";
        });
        isGoalCheck = true;
        dropBall.SetActive(true);

        dropCase = ddl.dropDataList[randData].dropCases[randCase];

        StartCoroutine(MoveDropBall(dropCase));

    }

    private IEnumerator MoveDropBall(DropCase dropCase)
    {
        var col = dropBall.GetComponent<CircleCollider2D>();
        dropBall.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        col.isTrigger = true;
        int mvIndex = 0;
        while(isGoalCheck)
        {
            if(dropCase.dropCoordList.Count < mvIndex + 1)
            {
                InteractableBottomButton(true);

                dropBall.SetActive(false);

                startButtonList.ForEach(btn =>
                {
                    btn.interactable = true;
                });

                isGoalCheck = false;
                break;
            }

            //for(var sg = 0; sg < dropCase.secondGoalIndexList.Count; sg++)
            //{
            //    for(var i = 0; i < dropCase.secondGoalIndexList[sg].indexList.Count; i++)
            //    {
            //        if (mvIndex == dropCase.secondGoalIndexList[sg].indexList[i])
            //        {
            //            CheckSecondGoal(sg);
            //        }
            //    }
            //    
            //    
            //}

            var cx = float.Parse(dropCase.dropCoordList[mvIndex].x);
            var cy = float.Parse(dropCase.dropCoordList[mvIndex].y);
            dropBall.transform.position = new Vector3(cx, cy, dropBall.transform.position.z);
            yield return new WaitForFixedUpdate();
            mvIndex++;
        }
    }

    private DropCase GetDropCase(int startIndex, int targetIndex = 0, int sg1 = 0, int sg2 = 0)
    {
        var randDropDataList = new List<DropData>();

        var curDropDataList = pinballData.allDropDataList[startIndex].dropDataList;
        for(var d = 0; d < curDropDataList.Count; d++)
        {
            if(curDropDataList[d].targetIndex == targetIndex)
            {
                if (curDropDataList[d].secondGoalCountList[0] == sg1 &&
                    curDropDataList[d].secondGoalCountList[1] == sg2)
                {
                    randDropDataList.Add(curDropDataList[d]);
                }

                //var totalSecondGoalCount = 0;
                //for (var s = 0; s < curDropDataList[d].secondGoalCountList.Count; s++)
                //{
                //    totalSecondGoalCount += curDropDataList[d].secondGoalCountList[s];
                //}

                //if(totalSecondGoalCount == sg1 + sg2)
                //{
                //    randDropDataList.Add(curDropDataList[d]);
                //}
            }
        }

        if(randDropDataList.Count > 0)
        {
            var randDdl = Random.Range(0, randDropDataList.Count);

            var randCase = Random.Range(0, randDropDataList[randDdl].dropCases.Count);

            return randDropDataList[randDdl].dropCases[randCase];
        }
        else
        {
            Debug.LogWarning("DropData Null");
            return null;
        }
    }


    #region 데이터 샘플링용

    public void GetData()
    {
        startTime = DateTime.Now.ToLocalTime();
        Debug.Log("Get Data Start");
        warnCheatText.text = "";
        totalCount = 0;

        pinballData = new PinballData();

        SetSliderText();

        for (var i = 0; i < startPosList.Count; i++)
        {
            pinballData.allDropDataList.Add(new DropDataList());
        }

        getDataSlider.maxValue = maxXCount * maxYCount * startButtonList.Count;

        curIndex = 0;

        curXCount = 0;
        curYCount = 0;

        curXpos = -checkWidth * 0.5f;
        curYpos = -checkHeight * 0.5f;

        isTest = true;

        SwitchGetDataUI(isTest);
        InteractableTopButton(false);
        InteractableBottomButton(false);

        //CheckNextDrop();
    }

    public void CheckNextDrop()
    {
        if (isTest && !isGoalCheck)
        {
            isGoalCheck = true;

            shooter.gameObject.SetActive(true);

            curDropCase = new DropCase();
            curDropData = new DropData(secondGoalList.Count);
            for(var sg = 0; sg < secondGoalList.Count; sg++)
            {
                curDropCase.secondGoalIndexList.Add(new SecondGoalIndexList());
            }


            curDropCoord = new DropCoord("", "");
            curDropCoord.x = curXpos.ToString("0.00000000;-0.00000000");
            curDropCoord.y = curYpos.ToString("0.00000000;-0.00000000");


            getDataSlider.value = totalCount;

            SetSliderText();

            if (curIndex >= startPosList.Count)
            {
                isTest = false;
            }
            else
            {
                shooter.MoveShooter(
                    startPosList[curIndex].transform.position.x + float.Parse(curDropCoord.x),
                    startPosList[curIndex].transform.position.y + float.Parse(curDropCoord.y));
            }
        }
    }

    public void CheckSecondGoal(int index, int curIterationIndex = 0)
    {
        curDropData.secondGoalCountList[index]++;
        if (!isTest)
        {
            cCountTextList[index].text = curDropData.secondGoalCountList[index].ToString();
        }
        else
        {
            curDropCase.secondGoalIndexList[index].indexList.Add(curIterationIndex);
        }
    }

    public void CheckFinalGoal(int index)
    {
        if(!isTest)
        {
            return;
        }
        if(isGoalCheck)
        {
            curDropData.targetIndex = index;


            prevDropDataList.Clear();

            var isSame = false;

            var dropDataListCheck = pinballData.allDropDataList[curIndex].dropDataList;

            for(var l = 0; l < prediction2d.linePositions.Count; l++)
            {
                var dc = new DropCoord(prediction2d.linePositions[l].x.ToString("0.00000000;-0.00000000"), prediction2d.linePositions[l].y.ToString("0.00000000;-0.00000000"));
                curDropCase.dropCoordList.Add(dc);
            }


            for (var d = 0; d < dropDataListCheck.Count; d++)
            {
                if (dropDataListCheck[d].targetIndex == curDropData.targetIndex)
                {
                    var sgCheckCount = 0;
                    for(var s = 0; s < dropDataListCheck[d].secondGoalCountList.Count; s++)
                    {
                        if (dropDataListCheck[d].secondGoalCountList[s] == curDropData.secondGoalCountList[s])
                        {
                            sgCheckCount++;
                        }
                    }
                    if(sgCheckCount == dropDataListCheck[d].secondGoalCountList.Count)
                    {
                        isSame = true;
                        if (dropDataListCheck[d].dropCases.Count < maxCaseCount)
                        {
                            dropDataListCheck[d].dropCases.Add(curDropCase);
                        }
                        break;
                    }
                }
            };

            if (!isSame)
            {
                curDropData.dropCases.Add(curDropCase);
                dropDataListCheck.Add(curDropData);
            }

            CheckNextCoord();

            isGoalCheck = false;
        }
    }

    public void CheckNextCoord()
    {
        do
        {
            totalCount++;
            curXCount++;
            if (curXCount >= maxXCount)
            {
                curXCount = 0;
                curYCount++;
            }

            if (curYCount >= maxYCount)
            {
                curIndex++;
                curXpos = -checkWidth * 0.5f;
                curYpos = -checkHeight * 0.5f;
                curYCount = 0;
                if (curIndex > 4)
                {
                    SaveDataCoroutine();
                    break;
                }
            }

            var xt = (curXCount / (float)maxXCount);
            var yt = (curYCount / (float)maxYCount);

            curXpos = -checkWidth * 0.5f + checkWidth * xt;
            curYpos = -checkHeight * 0.5f + checkHeight * yt;
        }
        while (-exceptionXwidth * 0.5f <= curXpos && curXpos <= exceptionXwidth * 0.5f);
    }

    //최대 이동거리 제한
    public void CheckMaxIteration()
    {
        if (isGoalCheck)
        {
            if (!isTest)
            {
                return;
            }

            CheckNextCoord();

            isGoalCheck = false;
        }
    }

    public void SaveDataCoroutine()
    {
        if(isTest)
        {
            isTest = false;

            shooter.gameObject.SetActive(false);
            prediction2d.ClearLineRenderer();

            SwitchGetDataUI(isTest);

            // ToJson을 사용하면 JSON형태로 포멧팅된 문자열이 생성된다  
            string jsonData = JsonUtility.ToJson(pinballData, true);
            // 데이터를 저장할 경로 지정   
            string path = Path.Combine(Application.dataPath, "DropData.json");
            // 파일 생성 및 저장
            File.WriteAllTextAsync(path, jsonData).Wait();

            Debug.Log("CheckEnd");

            var span = DateTime.Now.ToLocalTime() - startTime;
            int timestamp = (int)span.TotalSeconds;

            getDataSliderText.text = "데이터 수집 완료 [" + timestamp + "초 소요]";
            getDataSlider.value = getDataSlider.maxValue;

            startButtonList.ForEach(btn =>
            {
                btn.interactable = true;
            });
            InteractableTopButton(true);
            InteractableBottomButton(true);
        }
    }
    #endregion

    #region UI 관련 함수
    private void SwitchGetDataUI(bool isStart)
    {
        if(isStart)
        {
            getDataSlider.value = 0;
            getDataSlider.gameObject.SetActive(true);
        }
    }

    private void InteractableTopButton(bool isInteractable)
    {
        cheatBtn.interactable = isInteractable;
        startButtonList.ForEach(btn =>
        {
            btn.interactable = isInteractable;
        });
    }

    private void InteractableBottomButton(bool isInteractable)
    {
        getDataBtn.interactable = isInteractable;
        ballSelectBtn.interactable = isInteractable;
        obstacleSelectBtn.interactable = isInteractable;
    }

    private void SetSliderText()
    {
        getDataSliderText.text = "데이터 수집중 (" + totalCount + "/" + maxXCount * maxYCount * startPosList.Count + ")";
    }

    //public void SetWidthSlider(Slider slider)
    //{
    //    checkWidth = slider.value;
    //    widthSliderText.text = "Width: " + checkWidth; 
    //}
    //
    //public void SetHeightSlider(Slider slider)
    //{
    //    checkHeight = slider.value;
    //    heightSliderText.text = "Height: " + checkHeight;
    //}
    #endregion
}
