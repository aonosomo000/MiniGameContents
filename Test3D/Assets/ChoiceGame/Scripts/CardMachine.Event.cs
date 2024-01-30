using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class CardMachine : MonoBehaviour
{
    [Serializable] public class CommonEvent : UnityEvent { }
    [Serializable] public class StateEvent : UnityEvent<string> { }
    [Serializable] public class CurrencyEvent : UnityEvent<long> { }
    [Serializable] public class BoolEvent : UnityEvent<bool> { }
    [Serializable] public class StringEvent : UnityEvent<string> { }
    [Serializable] public class ErrorEvent : UnityEvent<string, string> { } //title, message

    [HideInInspector] public StringEvent OnSkinChange;
    [HideInInspector] public StateEvent OnStateChange;
    [HideInInspector] public StateEvent OnStateEnd;
    [HideInInspector] public CurrencyEvent OnPayCoins;
    [HideInInspector] public CurrencyEvent OnEarnCoins;
    [HideInInspector] public CurrencyEvent OnEarnShield;
    [HideInInspector] public CurrencyEvent OnEarnStar;
    [HideInInspector] public CommonEvent OnMoreCoins;
    [HideInInspector] public BoolEvent OnAutoSpin;
    [HideInInspector] public CurrencyEvent OnTotalBetChange;
    [HideInInspector] public CurrencyEvent OnTotalBetRequest;
    [HideInInspector] public BoolEvent OnEnableSpinButton;
    [HideInInspector] public BoolEvent OnEnableAutoSpinButton;
    [HideInInspector] public CommonEvent OnEnableSkip;
    [HideInInspector] public CommonEvent OnForceStop;
    [HideInInspector] public ErrorEvent OnError;



    protected virtual void DispatchSkinChange(string skin)
    {
        if (OnSkinChange != null)
        {
            OnSkinChange.Invoke(skin);
        }
    }

    protected virtual void DispatchPayCoins(long coins)
    {
        if (OnPayCoins != null)
        {
            OnPayCoins.Invoke(coins);
        }
    }

    protected virtual void DispatchEarnCoins(long coins)
    {
        if (OnEarnCoins != null)
        {
            OnEarnCoins.Invoke(coins);
        }
    }

    protected virtual void DispatchEarnShield(long coins)
    {
        if (OnEarnCoins != null)
        {
            OnEarnCoins.Invoke(coins);
        }
    }

    protected virtual void DispatchEarnStart(long coins)
    {
        if (OnEarnCoins != null)
        {
            OnEarnCoins.Invoke(coins);
        }
    }

    protected virtual void DispatchMoreCoins()
    {
        if (OnMoreCoins != null)
        {
            OnMoreCoins.Invoke();
        }
    }
    protected virtual void DispatchChangeAutoSpin(bool isOn)
    {
        if (OnAutoSpin != null)
        {
            OnAutoSpin.Invoke(isOn);
        }
    }
    protected virtual void DispatchChangeTotalBet(long totalBet)
    {
        if (OnTotalBetChange != null)
        {
            OnTotalBetChange.Invoke(totalBet);
        }
    }
    protected virtual void DispatchTotalBetRequest(long totalBet)
    {
        if (OnTotalBetRequest != null)
        {
            OnTotalBetRequest.Invoke(totalBet);
        }
    }
    protected virtual void DispatchEnableSpinButton(bool value)
    {
        if (OnEnableSpinButton != null)
        {
            OnEnableSpinButton.Invoke(value);
        }
    }
    protected virtual void DispatchEnableAutoSpinButton(bool value)
    {
        if (OnEnableAutoSpinButton != null)
        {
            OnEnableAutoSpinButton.Invoke(value);
        }
    }

    protected virtual void DispatchEnableSkip()
    {
        if (OnEnableSkip != null)
        {
            OnEnableSkip.Invoke();
        }
    }

    protected virtual void DisaptchForceStop()
    {
        if (OnForceStop != null)
        {
            OnForceStop.Invoke();
        }
    }        
    
    /// <summary> 에러 로그 </summary>
    /// <param name="title"> 제목 </param>
    /// <param name="message"> 내용 </param>
    /// <param name="debugOnly"> GGDEV 전처리기 활성화 시에만 처리. ex) 게임 진행 불가 시 false로 항상 뜨도록, 디버그 및 QA용은 true </param>
    protected virtual void DispatchError(string title, string message, bool debugOnly)
    {
#if GGDEV == false
        if (debugOnly)
        {
            return;
        }
#endif
        if (OnError != null)
        {
            OnError.Invoke(title, message);
        }
    }
}
