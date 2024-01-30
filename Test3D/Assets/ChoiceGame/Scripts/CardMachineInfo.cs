using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMachineInfo : MonoBehaviour
{
    public int TotalBetIndex { get; private set; }
    private int maxTotalBetIndex;
    private List<long> totalBets;
    public int MaxTotalbetIndex
    {
        get
        {
            return maxTotalBetIndex;
        }
        set
        {
            if (value >= GetTotalBetCount() - 1)
            {
                maxTotalBetIndex = GetTotalBetCount() - 1;
            }
            else
            {
                maxTotalBetIndex = value;
            }
        }
    }
    public virtual int GetTotalBetCount()
    {
        return totalBets.Count;
    }
    public DrawData drawData { get; protected set; }
    public bool isPaid;
    public CardMachineInfo()
    {
        drawData = new DrawData();
    }
}
