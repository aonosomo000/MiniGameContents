using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class CardMachine : MonoBehaviour
{
    [Serializable]
    public class CardServerType
    {
        public float rate;
        public int stopIndex;
        public string types;
    }


    public enum ReelMode
    {
        None,
        Regular,
    }
    public enum HoldType
    {
        On,
        Off,
        Auto
    }
    public enum MaxbetType
    {
        Index //설정 된 max index에 해당하는 토탈벳
    }
    public enum MachineItemsStopType
    {
        All,
        Sequence
    }
    public sealed class BuyFeatureOptions
    {
        public sealed class CoinDisplay
        {
            public Func<long, string> price;
            public Func<long, string> bet;
            public Func<long, string> popup;
        }

        public CoinDisplay coinDisplay = new CoinDisplay();
        public Texture2D coinIconTexture;
        public Action onNeedMoreCoins;
    }

    [Header("* Hit Time")]
    [SerializeField, Tooltip("전체 플립 후 히트 연출 딜레이")] protected float hitDelayTime = 0.0f;

    [Header("* Win Time")]
    [SerializeField, Tooltip("코인 획득 연출 시간")] protected float winCoinDuration = 0.0f;

    [HideInInspector] public HoldType holdType = HoldType.Off;

    private bool isInitialize;
    public CardMachineInfo Info { get; private set; }
    protected bool skipResult;
    private bool validSkipResult;
    public ReelMode CurrentReelMode { get; private set; }

    private bool autoSpin;
    public bool AutoSpin
    {
        get { return autoSpin; }
        set
        {
            autoSpin = value;
            DispatchChangeAutoSpin(autoSpin);
        }
    }

    private string skin = null;
    public string Skin
    {
        get
        {
            return skin;
        }
        set
        {
            if (skin != value)
            {
                skin = value;
                DispatchSkinChange(skin);
            }
        }
    }
    public bool IsForceStop { get; protected set; }
    private MaxbetType maxbetType;

    private int pauseCount;

    [SerializeField] private List<MagicCard> cardList = new List<MagicCard>();
    [SerializeField] private List<CardServerType> cardTypeServerList = new List<CardServerType>(); //서버미구현으로 테스트용도
    private List<CardType> cardTypeList = new List<CardType>();
    private int stopIndex = 0;
    private int curChoiceId = -1;
    private List<int> choiceList = new List<int>();
    private int choiceCount = 0;
    private int spCount = 0;

    [SerializeField] private Button drawBtn;


    private void Start()
    {
        Initialize();
    }

    private void ShuffleCards()
    {

    }


    public void Restart()
    {
        choiceCount = 0;
    }

    public void Initialize()
    {
        if (isInitialize == true)
        {
            return;
        }

        //테스트
        SetupFSM();

        //holdType = holdFeature;

        //SetupInfo(enterData, betData, slotPreset.general.orientation);

        isInitialize = true;

        Skin = skin;
        
        CurrentReelMode = ReelMode.None;
        
        //SetupUI(machineUiContainer, slotPreset.general.machineUiType, machineUiLayoutIndex, orientationType);
    }

    public bool IsInitialized()
    {
        if (isInitialize == false)
        {
            Debug.Log("[SlotMachine] Initialization is required.");
            return false;
        }

        if (CurrentState == CardMachineState.ENTER)
        {
            return false;
        }

        return true;
    }

    private void SetupCards()
    {
    }

    private void SetupInfo()
    {
        //int reelColumn = reelGroup.Column;
        //int reelRow = reelGroup.Row;
        //
        //Info = CreateSlotMachineInfo(reelColumn, reelRow, slotOrientationType, jackpotWinType);
        //Info.SetData(enterData);
        //Info.SetData(betData);
    }


    public void StartDraw()
    {
        if (CurrentState == CardMachineState.IDLE)
        {
            StartDraw(true);
        }
        else
        {
            SkipResult();
        }
    }

    public void StartChoice(int id)
    {
        if(CurrentState == CardMachineState.IDLE)
        {
            curChoiceId = id;

            NextState(CardMachineState.FLIP_START);
        }
    }

    private void StartDraw(bool isPaid)
    {
        if (IsInitialized() == false)
        {
            return;
        }

        //if (CurrentState != CardMachineState.IDLE && CurrentState != CardMachineState.BUY_FEATURE)
        //{
        //    Debug.Log("[SlotMachine] This machine is busy.");
        //    return;
        //}

        //if (isPaid && (Info.IsFreespin || Info.IsRespin || Info.IsLinkRespin))
        //{
        //    Debug.Log("[SlotMachine] 유료 스핀은 프리스핀, 리스핀, 링크리스핀 상황에선 불가능 함.");
        //    return;
        //}

        //Info.isPaid = isPaid;

        if (isPaid == true)
        {
            //if (HasEnoughCoinsForSpin() == false)
            //{
            //    Debug.Log("[SlotMachine] Not enough coins.");
            //    AutoSpin = false;
            //    MachineUI.SetAutoSpin(false);
            //    DispatchMoreCoins();
            //    return;
            //}
        }
        drawBtn.interactable = false;
        Debug.Log("Start Draw");
        //Debug.LogFormat("[SlotMachine] Pay for spin : {0} Coins", (isPaid ? StringUtils.ToComma(Info.CurrentTotalBet) : "free"));
        //Debug.LogFormat("[SlotMachine] My coins : {0} Coins", Info.coins);

        //machineGroup.ForEach(item =>
        //{
        //    item.ReelGroupHelper.SetTimeRateAll(defaultReelTimeRate);
        //});
        //
        NextState(CardMachineState.DRAW_UPDATE);
    }

    public void StopDraw(DrawData drawData)
    {
        if (IsInitialized() == false)
        {
            return;
        }

        if (CurrentState != CardMachineState.DRAW_START)
        {
            Debug.Log("[SlotMachine] wrong state.");
            return;
        }

        //if (Info.buyFeaturePurchased)
        //{
        //    Info.buyFeaturePurchased = false;
        //    PayCoins(Info.BuyFeaturePrice);
        //}
        //else if (Info.isPaid == true)
        //{
        //    PayCoins(Info.CurrentTotalBet);
        //}
        //
        //if (updateNextReelType == UpdateNextReelType.SpinStop)
        //{
        //    Info.UpdateNextReels();
        //}
        //
        //Info.SetData(drawData, machineGroup.EnabledCount, MachineGroup.MainItem.reelGroup.Column, MachineGroup.MainItem.reelGroup.Row, includeFeatureToLineWin);
        //
        //var jackpotCoins = Info.GetSuggestJackpotCoins();
        //DispatchChangeJackpotCoins(jackpotCoins);

        NextState(CardMachineState.DRAW_UPDATE);
    }
    public void SkipResult()
    {
        if (IsResultSkipPossible() == true)
        {
            skipResult = true;
        }
    }
    private bool IsResultSkipPossible()
    {
        return validSkipResult == true
                && CurrentState != CardMachineState.IDLE
                && CurrentState != CardMachineState.ENTER
                && CurrentState != CardMachineState.HIT;
    }
    
    //public void SetBet(BetData betData)
    //{
    //    Info.SetData(betData);
    //
    //    if (Info.IsReelChanged)
    //    {
    //        DispatchReelStripChanged();
    //    }
    //
    //    if (changeSymbolsByTotalbet && betData.syms != null)
    //    {
    //        machineGroup.MainItem.ChangeSymbols(betData.syms);
    //    }
    //
    //    UpdateLineBetSummary();
    //
    //    DispatchChangeTotalBet(betData.bet);
    //
    //    var jackpotCoins = Info.HasAverageTotalBet && Info.averageFinalJackpotCoins.Count > 0 ? Info.averageFinalJackpotCoins : Info.finalJackpotCoins;
    //    DispatchChangeJackpotCoins(jackpotCoins, true);
    //}
    //public long GetCurrentTotalBet()
    //{
    //    return Info.CurrentTotalBet;
    //}
    //public long GetMaxTotalBet()
    //{
    //    return maxbetType == GetMaxTotalBetForIndex();
    //}
    //private long GetMaxTotalBetForIndex()
    //{
    //    long maxTotalBet = Info.GetTotalBet(Info.MaxTotalbetIndex);
    //    return maxTotalBet;
    //}
    //public bool IsFirstTotalBet()
    //{
    //    return Info.TotalBetIndex == 0;
    //}
    //public bool IsLastTotalBet()
    //{
    //    return Info.TotalBetIndex == (Info.GetTotalBetCount() - 1);
    //}
    //private bool IsMaxTotalBetForIndex()
    //{
    //    return Info.CurrentTotalBet >= GetMaxTotalBet();
    //}
}
