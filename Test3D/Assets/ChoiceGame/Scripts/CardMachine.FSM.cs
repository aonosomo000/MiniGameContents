using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class CardMachineState
{
    public const string NONE = null;
    public const string ENTER = "EnterState";
    public const string IDLE = "IdleState";
    public const string FLIP_START = "FlipStartState";
    public const string FLIP_UPDATE = "FlipUpdateState";
    public const string DRAW_START = "DrawStartState";
    public const string DRAW_UPDATE = "DrawUpdateState";
    public const string DRAW_STOP = "DrawStopState";
    public const string HIT = "HitState";
    public const string RESULT_START = "ResultStartState";
    public const string RESULT_END = "ResultEndState";
    public const string BUY_FEATURE = "BuyFeatureState";
}
public partial class CardMachine : MonoBehaviour
{
    protected readonly float RESULT_SKIP_DELAY = 0f;
    public string CurrentState { get; private set; } = CardMachineState.ENTER;
    private string runningState;
    [HideInInspector] public bool pauseStateTransition;

    protected List<Coroutine> repeatWinningLinesCoroutineList;
    protected Coroutine hitSymbolsCoroutine;

    private bool isResultEnd = false;

    private void SetupFSM()
    {
        StartCoroutine(StartFSM());
    }

    // 현재 state 코루틴을 실행한다.
    private IEnumerator StartFSM()
    {
        yield return null;

        string prevState = CardMachineState.NONE;
        CurrentState = CardMachineState.ENTER;

        while (true)
        {
            while (pauseStateTransition == true)
            {
                yield return null;
            }

            Debug.LogFormat("[SlotMachine FSM] Change State : {0} -> {1}", prevState, CurrentState);
            prevState = CurrentState;
            runningState = CurrentState;

            if (OnStateChange != null)
            {
                OnStateChange.Invoke(CurrentState);
            }

            yield return StartCoroutine(CurrentState);

            if (OnStateEnd != null)
            {
                OnStateEnd.Invoke(prevState);
            }
        }
    }

    private void NextState(string nextState)
    {
        if (CurrentState != runningState)
        {
            return;
        }

        CurrentState = nextState;
    }

    // 슬롯머신 진입시 최초 한번 실행되어야 할 스테이트
    // 이 부분에 슬롯머신 진입 애니메이션 등을 구현해야함.
    private IEnumerator EnterState()
    {
        isResultEnd = false;

        yield return null;

        NextState(CardMachineState.DRAW_UPDATE);
    }

    // 기본 상태.
    // 기본적으로 유저의 입력을 기다리지만 프리스핀, 리스핀, 오토스핀등의 상황에선 자동으로 스핀 상태로 변경.
    private IEnumerator IdleState()
    {
        if (AutoSpin == true)
        {
            StartDraw();
        }

        skipResult = false;
        validSkipResult = false;
        IsForceStop = false;

        while (CurrentState == CardMachineState.IDLE)
        {
            //if (Info.buyFeaturePurchased)
            //{
            //    NextState(CardMachineState.BUY_FEATURE);
            //    break;
            //}

            yield return null;
        }
    }

    private IEnumerator FlipStartState()
    {
        choiceList.Add(curChoiceId);
        cardList[curChoiceId].SetButton(false);

        cardList[curChoiceId].Init(cardTypeList[choiceCount]);

        if(cardTypeList[choiceCount] == CardType.SpecialCoin)
        {
            spCount++;
        }

        if(spCount == 3)
        {
            cardList[curChoiceId].SwapSymbol(CardType.Coin);
            cardList[curChoiceId].anim.SetTrigger("SpecialFlip");
            spCount = 0;
            yield return new WaitForSeconds(2f);
        }
        else
        {
            cardList[curChoiceId].anim.SetTrigger("NormalChoice");
            yield return new WaitForSeconds(0.2f);
        }


        if (choiceCount == stopIndex)
        {
            NextState(CardMachineState.HIT);
        }
        else
        {
            NextState(CardMachineState.IDLE);
        }
        choiceCount++;

        yield break;
    }
    private IEnumerator DrawUpdateState()
    {
        for(var c = 0; c < cardList.Count; c++)
        {
            foreach (var param in cardList[c].anim.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                {
                    cardList[c].anim.ResetTrigger(param.name);
                }
            }
        }
        choiceList.Clear();
        cardTypeList.Clear();
        choiceCount = 0;
        stopIndex = 0;
        spCount = 0;

        //서버에서 카드타입리스트 배열로 가져와야함 (최대길이 9)
        //테스트

        var rand = UnityEngine.Random.Range(0, 100f);
        var addRand = 0f;

        for(var c = 0; c < cardTypeServerList.Count; c++)
        {
            if(rand <= cardTypeServerList[c].rate + addRand)
            {
                stopIndex = cardTypeServerList[c].stopIndex;
                var spl = cardTypeServerList[c].types.Split(",");
                for(var s = 0; s < spl.Length; s++)
                {
                    cardTypeList.Add((CardType)Enum.Parse(typeof(CardType), spl[s]));
                }
                break;
            }

            addRand += cardTypeServerList[c].rate;
        }

        yield return new WaitForSeconds(0.1f);

        NextState(CardMachineState.DRAW_START);

        yield break;
    }
    private IEnumerator DrawStartState()
    {
        if(isResultEnd)
        {
            for (var c = 0; c < cardList.Count; c++)
            {
                cardList[c].anim.SetTrigger("Out");
                yield return new WaitForSeconds(0.05f);
            }
        }

        yield return new WaitForSeconds(0.1f);

        for (var c = 0; c < cardList.Count; c++)
        {
            cardList[c].anim.SetTrigger("Draw");
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.25f);

        for(var c = 0; c < cardList.Count; c++)
        {
            cardList[c].SetButton(true);
        }

        NextState(CardMachineState.IDLE);
        yield break;
    }
    private IEnumerator HitState()
    {
        //매치된 심볼 연출
        var matchType = cardTypeList[stopIndex];

        if(matchType == CardType.Coin ||
           matchType == CardType.SpecialCoin)
        {
            for (var c = 0; c < cardList.Count; c++)
            {
                if (cardList[c].GetCardType() == matchType)
                {
                    if(matchType == CardType.SpecialCoin)
                    {
                        cardList[c].SwapSymbol(matchType);
                    }
                    cardList[c].anim.SetTrigger("Match");
                }
            }
            yield return new WaitForSeconds(1.5f);
        }

        NextState(CardMachineState.RESULT_START);
        yield break;
    }
    private IEnumerator ResultStartState()
    {
        NextState(CardMachineState.RESULT_END);
        yield break;
    }
    private IEnumerator ResultEndState()
    {
        isResultEnd = true;

        yield return null;

        var remainList = new List<int>();

        for (var c = 0; c < cardList.Count; c++)
        {
            cardList[c].SetButton(false);
            remainList.Add(c);
        }

        for (var r = remainList.Count - 1; r >= 0; r--)
        {
            for (var c = 0; c < choiceList.Count; c++)
            {
                if (remainList[r] == choiceList[c])
                {
                    remainList.Remove(choiceList[c]);
                    break;
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        for (var r = 0; r < remainList.Count; r++)
        {
            cardList[remainList[r]].Init(cardTypeList[r + choiceCount]);
            cardList[remainList[r]].anim.SetTrigger("NormalFlip");
            yield return new WaitForSeconds(0.1f);
        }

        NextState(CardMachineState.IDLE);

        drawBtn.interactable = true;

        yield break;
    }
    private IEnumerator BuyFeatureState()
    {
        yield return null;
    }
}
