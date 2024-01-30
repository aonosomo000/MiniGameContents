using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public enum CardType
{
    Stone = 0,
    Coin,
    SpecialCoin
}


public class MagicCard : MonoBehaviour
{
    public Animator anim;
    [SerializeField] private Transform cardT;
    [SerializeField] private Transform innerT;
    [SerializeField] private Transform spritesT;
    [SerializeField] private Camera effectCamera;
    [SerializeField] private Button cardBtn;
    [SerializeField] private Slider symbolSlider;

    [SerializeField] private Image originSymbol;
    [SerializeField] private Image changeSymbol;

    [SerializeField] private List<Sprite> spriteList;

    [SerializeField] private CardType type;

    public void Init(CardType _type)
    {
        type = _type;

        symbolSlider.value = 0f;

        SwapSymbol(_type);
    }

    public CardType GetCardType()
    {
        return type;
    }

    private void Update()
    {
        var cardXspace = cardT.position.x;
        var cardYspace = cardT.position.y;
        var zDiff = spritesT.position.z / effectCamera.gameObject.transform.position.z;

        var xAdd = -cardXspace * zDiff;
        var yAdd = -cardYspace * zDiff;

        innerT.position = new Vector3(
            cardXspace + xAdd,
            cardYspace + yAdd,
            innerT.position.z
            );
    }

    public void SetButton(bool interactable)
    {
        cardBtn.interactable = interactable;
    }

    public void SwapSymbol(CardType _type)
    {
        if (_type == CardType.Stone)
        {
            originSymbol.sprite = spriteList[0];
        }
        else if (_type == CardType.Coin)
        {
            originSymbol.sprite = spriteList[1];
        }
        else if (_type == CardType.SpecialCoin)
        {
            originSymbol.sprite = spriteList[2];
        }
    }
}
