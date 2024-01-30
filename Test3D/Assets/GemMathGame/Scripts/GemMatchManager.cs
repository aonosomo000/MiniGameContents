using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Test
{
    public static int FindIndex<T>(this T[] array, Func<T, bool> converter)
    {
        if (array == null) throw new ArgumentNullException("array");
        if (converter == null) throw new ArgumentNullException("converter");

        int index = 0;
        foreach (var item in array)
        {
            if (converter(item))
            {
                break;
            }
            index++;
        }
        return index;
    }
    public static T[] Push<T>(this T[] array, T value)
    {
        if (array == null) throw new ArgumentNullException("array");
        if (value == null) throw new ArgumentNullException("value");

        Array.Resize(ref array, array.Length + 1);
        array[array.GetUpperBound(0)] = value;
        return array;
    }
}

public enum NearDirection
{
    Left=0,
    Right,
    Up,
    Down
}
public class NearGemTypeInfo
{
    public GemMainType type;
    public GemCoord coord;
    public NearDirection direction;
    public int count;

    public NearGemTypeInfo(GemMainType _type, GemCoord _coord, NearDirection _direction, int _count)
    {
        type = _type;
        coord = _coord;
        direction = _direction;
        count = _count;
    }
}

public class GemTypeCount
{
    public GemMainType type;
    public int count;

    public GemTypeCount(GemMainType _type, int _count)
    {
        type = _type;
        count = _count;
    }
}

[Serializable]
public class GemSubTypeRand
{
    public GemSubType subType;
    public int rand;
}


public class GemMatchManager : MonoBehaviour
{
    public GMEffectManager effectManager;

    public TMPro.TextMeshProUGUI scoreText;

    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private Transform gemGroup;

    [SerializeField] private int maxPointInit;
    [SerializeField] private int minPointInit;

    [SerializeField] private int gemXCount;
    [SerializeField] private int gemYCount;

    [SerializeField] private float width;
    [SerializeField] private float height;

    public List<Sprite> gemSpriteList;
    public List<Sprite> gemStripeSpriteList;
    public List<Sprite> gemHighlightSpriteList;
    [SerializeField] private List<GemMainType> typeList;
    private List<GemInfo> curMatchPointList = new List<GemInfo>();

    private Gem[,] gemList;
    private Gem[,] newGemList; //위에서 새로 생성될 젬 리스트
    private Vector2[,] gemPosList;
    private Vector2[,] newGemPosList;

    private bool canMove = false;

    private bool canDrag = false;
    private Gem curSelectGem;

    public int curScore = 0;
    public int remainDownCount = 0;

    [SerializeField] private List<GemSubTypeRand> subTypeRandList;
    private int remainSpecialRemoveCount = 0;

    private void Start()
    {
        SetGems();
        canMove = true;
    }
    private void Update()
    {
        if (!canMove)
            return;

        if(Input.GetMouseButton(0))// && canDrag
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, Vector3.forward * 1000);
            if (Physics.Raycast(ray, out hit, 1000))
            {
                if(hit.transform.gameObject.layer == LayerMask.NameToLayer("BoxCol"))
                {
                    var gem = hit.transform.GetComponent<Gem>();
                    
                    if(curSelectGem == null)
                    {
                        curSelectGem = gem;
                    }
                    else
                    {
                        if(curSelectGem != gem)
                        {
                            if(Math.Abs(curSelectGem.Info.Coord.x - gem.Info.Coord.x) +
                               Math.Abs(curSelectGem.Info.Coord.y - gem.Info.Coord.y) < 2)
                            {
                                canMove = false;
                                canDrag = false;
                                SwapGems(curSelectGem, gem);
                                return;
                            }
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            //canDrag = true;
            curSelectGem = null;
        }
    }

    private void SetGems()
    {
        gemList = new Gem[gemXCount, gemYCount];
        newGemList = new Gem[gemXCount, gemYCount];
        gemPosList = new Vector2[gemXCount, gemYCount];
        newGemPosList = new Vector2[gemXCount, gemYCount];
        var startX = width / (gemXCount * 2f) - width * 0.5f;
        var startY = height / (gemYCount * 2f) - height * 0.5f;

        //젬 위치 세팅
        for (var y = 0; y < gemYCount; y++)
        {
            var yPos = startY + (y * height / gemYCount);
            var yPosNew = startY + ((y + gemYCount) * height / gemYCount);

            for (var x = 0; x < gemXCount; x++)
            {
                var xPos = startX + (x * width / gemXCount);

                var gemObj = Instantiate(gemPrefab, gemGroup);
                gemObj.transform.localPosition = new Vector3(xPos, yPos, 0f);
                gemPosList[x, y] = gemObj.transform.localPosition;

                newGemPosList[x, y] = new Vector3(xPos, yPosNew, 0f);

                var gem = gemObj.GetComponent<Gem>();

                gem.Init(this, new GemInfo(GemMainType.None, new GemCoord(x, y)));

                gemList[x, y] = gem;
            }
        }

        var unavailable = true;
        while (unavailable)
        {
            var matchPointList = new List<GemInfo>();
            
            //젬 타입 세팅 (3개 연속 방지)
            for (var y = 0; y < gemYCount; y++)
            {
                for (var x = 0; x < gemXCount; x++)
                {
                    var xGemCheckList = new List<GemInfo>();
                    var yGemCheckList = new List<GemInfo>();
                    var prevGemInfo = new GemInfo(GemMainType.None);
                    for (var yc = -2; yc < 0; yc++)
                    {
                        var yCheck = y + yc;
                        if (0 <= yCheck && yCheck < gemYCount)
                        {
                            if (gemList[x, yCheck].Info.MainType == prevGemInfo.MainType)
                            {
                                yGemCheckList.Add(prevGemInfo);
                                yGemCheckList.Add(new GemInfo(gemList[x, yCheck].Info.MainType, new GemCoord(x, yCheck)));
                            }
                            else
                            {
                                prevGemInfo = new GemInfo(gemList[x, yCheck].Info.MainType, new GemCoord(x, yCheck));
                            }
                        }
                    }

                    prevGemInfo = new GemInfo(GemMainType.None);
                    for (var xc = -2; xc < 0; xc++)
                    {
                        var xCheck = x + xc;

                        if (0 <= xCheck && xCheck < gemXCount)
                        {
                            if (gemList[xCheck, y].Info.MainType == prevGemInfo.MainType)
                            {
                                xGemCheckList.Add(prevGemInfo);
                                xGemCheckList.Add(new GemInfo(gemList[xCheck, y].Info.MainType, new GemCoord(xCheck, y)));
                            }
                            else
                            {
                                prevGemInfo = new GemInfo(gemList[xCheck, y].Info.MainType, new GemCoord(xCheck, y));
                            }
                        }
                    }

                    var exceptTypeList = new List<GemMainType>();
                    var randTypeList = new List<GemMainType>();

                    if (yGemCheckList.Count == 2)
                    {
                        exceptTypeList.Add(yGemCheckList[0].MainType);
                    }
                    if (xGemCheckList.Count == 2)
                    {
                        exceptTypeList.Add(xGemCheckList[0].MainType);
                    }

                    for (var t = 0; t < typeList.Count; t++)
                    {
                        var same = false;
                        for (var e = 0; e < exceptTypeList.Count; e++)
                        {
                            if (exceptTypeList[e] == typeList[t])
                            {
                                same = true;
                                break;
                            }
                        }
                        if (!same)
                        {
                            randTypeList.Add(typeList[t]);
                        }
                    }

                    var rand = Random.Range(0, randTypeList.Count);

                    gemList[x, y].InitMainType(randTypeList[rand]);

                    var randSub = Random.Range(0, 100);
                    var curRand = 0;
                    for(var t = 0; t < subTypeRandList.Count; t++)
                    {
                        curRand += subTypeRandList[t].rand;
                        if (randSub < curRand)
                        {
                            gemList[x, y].SetSubType(subTypeRandList[t].subType);
                            break;
                        }
                    }
                }
            }

            matchPointList = FindAnyThreeMatch();

            if (matchPointList.Count > 0)
            {
                ////보드에서 젬을 한번 옮겼을때 얻을 수 있는 최대 포인트 계산 (위에서 내려올 다른 젬들에 대한 계산은 젬을 옮기고 나서 서버와 통신 후 결정)
                //
                //var pointInit = GetMaxPointInit(matchPointList);
                //
                ////최대포인트 합산이 원하는 값과 같을경우
                //if(minPointInit <= pointInit && pointInit <= maxPointInit)
                //{
                //    unavailable = false;
                //    curMatchPointList = matchPointList;
                //}
                curMatchPointList = matchPointList;
                unavailable = false;
            }
        }
    }

    private List<GemInfo> FindAnyThreeMatch()
    {
        var matchPointList = new List<GemInfo>();
        //3매치 가능한 경우가 하나도 없을경우 재생성
        for (var y = 0; y < gemYCount; y++)
        {
            for (var x = 0; x < gemXCount; x++)
            {
                var nearTypeList = new List<NearGemTypeInfo>();
                //주위에 붙어있는 젬 확인
                for (var yc = -1; yc <= 1; yc++)
                {
                    var yCheck = y + yc;
                    for (var xc = -1; xc <= 1; xc++)
                    {
                        var xCheck = x + xc;
                        if (0 <= yCheck && yCheck < gemYCount &&
                           0 <= xCheck && xCheck < gemXCount)
                        {
                            if (yc * xc == 0 && yc + xc != 0)
                            {
                                if (gemList[xCheck, yCheck].Info.MainType != gemList[x, y].Info.MainType)
                                {
                                    var direction = NearDirection.Left;
                                    if (xc < 0)
                                    {
                                        direction = NearDirection.Left;
                                    }
                                    else if (xc > 0)
                                    {
                                        direction = NearDirection.Right;
                                    }
                                    if (yc < 0)
                                    {
                                        direction = NearDirection.Down;
                                    }
                                    else if (yc > 0)
                                    {
                                        direction = NearDirection.Up;
                                    }

                                    nearTypeList.Add(new NearGemTypeInfo(
                                        gemList[xCheck, yCheck].Info.MainType,
                                        new GemCoord(xCheck, yCheck),
                                        direction,
                                        1)
                                        );
                                }
                            }
                        }
                    }
                }

                //붙어있는젬의 붙어있는젬 확인
                for (var n = 0; n < nearTypeList.Count; n++)
                {
                    var xs = nearTypeList[n].coord.x;
                    var ys = nearTypeList[n].coord.y;
                    var direction = nearTypeList[n].direction;
                    if (direction == NearDirection.Left)
                    {
                        xs--;
                    }
                    else if (direction == NearDirection.Right)
                    {
                        xs++;
                    }
                    else if (direction == NearDirection.Down)
                    {
                        ys--;
                    }
                    else
                    {
                        ys++;
                    }
                    if (0 <= ys && ys < gemYCount &&
                           0 <= xs && xs < gemXCount)
                    {
                        if (gemList[xs, ys].Info.MainType == nearTypeList[n].type)
                        {
                            nearTypeList[n].count += 1;
                        }
                    }
                }

                var checkTypeList = new List<GemTypeCount>();

                for (var n = 0; n < nearTypeList.Count; n++)
                {
                    var sameExist = false;
                    for (var c = 0; c < checkTypeList.Count; c++)
                    {
                        if (nearTypeList[n].type == checkTypeList[c].type)
                        {
                            checkTypeList[c].count += nearTypeList[n].count;
                            sameExist = true;
                        }
                    }
                    if (!sameExist)
                    {
                        checkTypeList.Add(new GemTypeCount(nearTypeList[n].type, nearTypeList[n].count));
                    }
                }

                var checkCount = 0;
                for (var c = 0; c < checkTypeList.Count; c++)
                {
                    if (checkTypeList[c].count > checkCount)
                    {
                        checkCount = checkTypeList[c].count;
                    }
                }

                if (checkCount > 2)
                {
                    var matchPoint = new GemInfo(gemList[x,y].Info.MainType, new GemCoord(x, y));
                    matchPointList.Add(matchPoint);
                }
            }
        }
        return matchPointList;
    }

    //매치연결된 좌표리스트 가져오기
    private List<Gem> GetMatchedGemList(Gem _startGem)
    {
        var matchedGemList = new List<Gem>();
        var startGemInfo = _startGem;//new GemInfo(_startGemInfo.Type, _startGemInfo.Coord);
        var startCoord = startGemInfo.Info.Coord;
        var startType = startGemInfo.Info.MainType;
        var dx = new int[4] { 1, 0, -1, 0 };
        var dy = new int[4] { 0, 1, 0, -1 };
        var dirCount = 0;
        var dirCountArr = new List<List<Gem>>();

        matchedGemList.Add(_startGem);

        while (dirCount < 4)
        {
            for (var dir = 0; dir < 4; dir++)
            {
                int nx = startCoord.x;
                int ny = startCoord.y;

                var dirGemList = new List<Gem>();

                for(var count = 0; count < 2; count++)
                {
                    nx += dx[dir];
                    ny += dy[dir];

                    if(IsOverRange(nx, ny))
                    {
                        continue;
                    }

                    if (startType == gemList[nx, ny].Info.MainType)
                    {
                        dirGemList.Add(gemList[nx, ny]);//new GemInfo(_predictGemList[nx, ny].Type, new GemCoord(nx, ny)));
                    }
                    else
                    {
                        break;
                    }
                }

                dirCountArr.Add(dirGemList);
                dirCount++;
            }
        }

        if(dirCountArr[0].Count + dirCountArr[2].Count > 1)
        {
            matchedGemList.AddRange(dirCountArr[0]);
            matchedGemList.AddRange(dirCountArr[2]);
        }
        if (dirCountArr[1].Count + dirCountArr[3].Count > 1)
        {
            matchedGemList.AddRange(dirCountArr[1]);
            matchedGemList.AddRange(dirCountArr[3]);
        }

        return matchedGemList;
    }

    private void DFS(GemInfo _startGemInfo, GemInfo[,] _predictGemList)
    {
        var matchedCoordList = new List<GemCoord>();
        var startCoord = _startGemInfo.Coord;
        var startType = _startGemInfo.MainType;
        var visited = new bool[gemXCount,gemYCount];
        var dx = new int[4]{ 1, 0, -1, 0 };
        var dy = new int[4]{ 0, 1, 0, -1 };

        matchedCoordList.Add(startCoord);

        visited[startCoord.x, startCoord.y] = true;

        var q = new Queue<GemCoord>();
        q.Enqueue(startCoord);

        while(q.Count>0)
        {
            GemCoord coord = q.Peek();
            q.Dequeue();

            for(var dir = 0; dir < 4; dir++)
            {
                int nx = coord.x + dx[dir];
                int ny = coord.y + dy[dir];

                if (IsOverRange(coord.x, coord.y)) //범위 초과했는지 확인
                    continue;
                if (visited[nx, ny]) //방문했는지 확인
                    continue;
                if (startType != _predictGemList[nx, ny].MainType)
                    continue;
                visited[nx, ny] = true;
                q.Enqueue(coord);
                matchedCoordList.Add(coord);
            }
        }
    }

    //유저 플레이 케이스

    //젬 스왑
    public void SwapGems(Gem _moveGem, Gem _targetGem)
    {
        //if (IsOverRange(_moveGem.Info.Coord.x + _targetGem.Info.coo, _moveGem.Info.Coord.y + _targetGem.y))
        //    return;

        StartCoroutine(SwapGemCoroutine(_moveGem, _targetGem));
    }

    private List<Gem> FindAnyMatchList()
    {
        var matchedList = new List<Gem>();
        //매치 확인 CheckMatch
        for (var y = 0; y < gemYCount; y++)
        {
            for (var x = 0; x < gemXCount; x++)
            {
                if (gemList[x, y].Info.MainType == GemMainType.None)
                    continue;

                var matchedGemList = GetMatchedGemList(gemList[x, y]);

                if (matchedGemList.Count > 2)
                {
                    matchedList.AddRange(matchedGemList);
                }
            }
        }

        return matchedList;
    }

    private IEnumerator SwapGemCoroutine(Gem _moveGem, Gem _targetGem)
    {
        var originPos = _moveGem.gameObject.transform.position;
        var targetPos = _targetGem.gameObject.transform.position;

        var originMove = true;
        var targetMove = true;

        var originCoord = _moveGem.Info.Coord;
        var targetCoord = _targetGem.Info.Coord;

        //이동 연출
        _moveGem.Move(targetPos, targetCoord, () => { 
            originMove = false; 
        });
        _targetGem.Move(originPos, originCoord, () => { 
            targetMove = false; 
            //gemList[_moveGem.Info.Coord.x, _moveGem.Info.Coord.y].InitType(targetType);
        });

        while(originMove || targetMove)
        {
            yield return null;
        }

        gemList[targetCoord.x, targetCoord.y] = _moveGem;
        gemList[originCoord.x, originCoord.y] = _targetGem;

        //매치 확인
        var matchedGemList = FindAnyMatchList().Distinct().ToList();

        //매치일 경우
        if (matchedGemList.Count > 0)
        {
            //모든 매치 종료까지 반복
            while (true)
            {
                matchedGemList = FindAnyMatchList().Distinct().ToList();
                if (matchedGemList.Count < 1)
                {
                    break;
                }

                Debug.Log("Match!");

                matchedGemList.Sort((x, y) => x.Info.Coord.y.CompareTo(y.Info.Coord.y));


                //매치된 젬 블럭 위치 저장 및 젬 블럭 제거
                for (var m = 0; m < matchedGemList.Count; m++)
                {
                    if (gemList[matchedGemList[m].Info.Coord.x, matchedGemList[m].Info.Coord.y] == null)
                        continue;

                    if(matchedGemList[m].Info.SubType == GemSubType.Normal)
                    {
                        gemList[matchedGemList[m].Info.Coord.x, matchedGemList[m].Info.Coord.y].PlayRemove();
                        gemList[matchedGemList[m].Info.Coord.x, matchedGemList[m].Info.Coord.y] = null;
                    }
                    else
                    {
                        StartCoroutine(PlayRemoveSpecial(matchedGemList[m].Info.Coord));
                    }
                    matchedGemList[m] = null;
                }

                while (remainSpecialRemoveCount > 0)
                {
                    yield return null;
                }

                newGemList = new Gem[gemXCount, gemYCount];

                //제거된 y축 블럭 수만큼 맨위에 새로운 젬 블럭 추가
                for (var x = 0; x < gemXCount; x++)
                {
                    yield return CreateNewGems(x);
                }
                
                yield return new WaitForSeconds(0.1f);

                //빈 공간에 젬 블럭 내리기
                for (var x = 0; x < gemXCount; x++)
                {
                    yield return MoveDownVertical(x);
                }

                yield return new WaitForSeconds(0.5f);

            }

        }
        else
        {
            yield return new WaitForSeconds(0.1f);
            //다시 돌아감
            originMove = true;
            targetMove = true;

            _moveGem.Move(originPos, originCoord, () =>
            {
                originMove = false;
            });
            _targetGem.Move(targetPos, targetCoord, () =>
            {
                targetMove = false;
            });

            while (originMove || targetMove)
            {
                yield return null;
            }

            gemList[originCoord.x, originCoord.y] = _moveGem;
            gemList[targetCoord.x, targetCoord.y] = _targetGem;

        }

        //전체 필드 매치 가능여부 확인
        //불가능할 경우 필드 재생성

        curSelectGem = null;
        canMove = true;
    }

    private IEnumerator PlayRemoveSpecial(GemCoord _specialGemCoord)
    {
        remainSpecialRemoveCount++;

        var mainType = gemList[_specialGemCoord.x, _specialGemCoord.y].Info.MainType;
        var subType = gemList[_specialGemCoord.x, _specialGemCoord.y].Info.SubType;

        gemList[_specialGemCoord.x, _specialGemCoord.y].PlayRemove();
        gemList[_specialGemCoord.x, _specialGemCoord.y] = null;

        if(subType == GemSubType.Horizontal)
        {
            for (var x = 0; x < gemXCount; x++)
            {
                for(var i = -1; i < 3; i+=2)
                {
                    var xc = _specialGemCoord.x + x * i;

                    if (IsOverRange(xc, _specialGemCoord.y))
                        continue;

                    if (gemList[xc, _specialGemCoord.y] == null)
                    {
                        effectManager.ShowRemoveEffect(gemPosList[xc, _specialGemCoord.y], mainType);
                    }
                    else
                    {
                        if (gemList[xc, _specialGemCoord.y].Info.SubType == GemSubType.Normal)
                        {
                            gemList[xc, _specialGemCoord.y].PlayRemove(mainType);
                            gemList[xc, _specialGemCoord.y] = null;
                        }
                        else
                        {
                            StartCoroutine(PlayRemoveSpecial(new GemCoord(xc, _specialGemCoord.y)));
                        }
                    }
                }
                yield return new WaitForSeconds(0.05f);
            }
        }
        else if (subType == GemSubType.Vertical)
        {
            for (var y = 0; y < gemYCount; y++)
            {
                for (var i = -1; i < 3; i += 2)
                {
                    var yc = _specialGemCoord.y + y * i;

                    if (IsOverRange(_specialGemCoord.x, yc))
                        continue;

                    if (gemList[_specialGemCoord.x, yc] == null)
                    {
                        effectManager.ShowRemoveEffect(gemPosList[_specialGemCoord.x, yc], mainType);
                    }
                    else
                    {
                        if (gemList[_specialGemCoord.x, yc].Info.SubType == GemSubType.Normal)
                        {
                            gemList[_specialGemCoord.x, yc].PlayRemove(mainType);
                            gemList[_specialGemCoord.x, yc] = null;
                        }
                        else
                        {
                            StartCoroutine(PlayRemoveSpecial(new GemCoord(_specialGemCoord.x, yc)));
                        }
                    }
                }
                yield return new WaitForSeconds(0.05f);
            }
        }
        remainSpecialRemoveCount--;
    }

    private IEnumerator MoveDownVertical(int xCoord)
    {
        for(var y = 0; y < gemYCount; y++)
        {
            if (gemList[xCoord, y] != null)
                continue;

            for (var yc = 1; yc <= gemYCount; yc++)
            {
                var yCheck = y + yc;

                if (yCheck > gemYCount - 1) //위에 가려진 새로 생성된 젬블럭
                {
                    if (newGemList[xCoord, yCheck - gemYCount] != null)
                    {
                        gemList[xCoord, y] = newGemList[xCoord, yCheck - gemYCount];
                        newGemList[xCoord, yCheck - gemYCount] = null;
                        gemList[xCoord, y].SetVisual();
                        gemList[xCoord, y].MoveDown(gemPosList[xCoord, y], new GemCoord(xCoord, y), () => { remainDownCount--; });
                        break;
                    }
                }
                else
                {
                    if (gemList[xCoord, yCheck] != null)
                    {
                        gemList[xCoord, y] = gemList[xCoord, yCheck];
                        gemList[xCoord, yCheck] = null;
                        gemList[xCoord, y].MoveDown(gemPosList[xCoord, y], new GemCoord(xCoord, y), () => { remainDownCount--; });
                        break;
                    }
                }
            }
        }
        yield return null;
        //while(moveCount > 0)
        //{
        //    yield return null;
        //}
    }

    //매치 후 떨어지는 젬을 추가
    private IEnumerator CreateNewGems(int xCoord)
    {
        var yc = 0;
        var prevType = GemMainType.None;
        var sameCount = 0;
        for (var y = 0; y < gemYCount; y++)
        {
            if (gemList[xCoord, y] == null)
            {
                var randType = Random.Range(0, typeList.Count);
                var type = typeList[randType];
                if (type != prevType)
                {
                    prevType = type;
                }
                else
                {
                    sameCount++;
                }

                while (sameCount > 1)
                {
                    randType = Random.Range(0, typeList.Count);
                    type = typeList[randType];
                    if (type != prevType)
                    {
                        sameCount = 0;
                        break;
                    }
                }

                GameObject newGemObj = null;
                yield return newGemObj = Instantiate(gemPrefab, gemGroup);
                var newGem = newGemObj.GetComponent<Gem>();

                newGem.Init(this, new GemInfo(type, new GemCoord(xCoord, y)));
                newGem.InitMainType(type);
                var randSub = Random.Range(0, 100);
                var curRand = 0;
                for (var t = 0; t < subTypeRandList.Count; t++)
                {
                    curRand += subTypeRandList[t].rand;
                    if (randSub < curRand)
                    {
                        newGem.SetSubType(subTypeRandList[t].subType);
                        break;
                    }
                }
                newGemList[xCoord, yc] = newGem;
                newGem.transform.position = newGemPosList[xCoord, yc];

                yc++;
            }
        }
        yield return null;
    }


    private bool IsOverRange(int x, int y)
    {
        if (x < 0 || y < 0 || x >= gemXCount || y >= gemYCount) return true;
        else return false;
    }
}
