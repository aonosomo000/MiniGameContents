using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GemCoord
{
    public int x;
    public int y;

    public GemCoord(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}
public enum GemMainType
{
    None = -1,
    Circle,
    Diamind,
    Pentagon,
    Square
}
public enum GemSubType
{
    Normal = -1,
    Horizontal,
    Vertical
}

[Serializable]
public class GemInfo
{
    public GemMainType MainType;
    public GemSubType SubType = GemSubType.Normal;
    public GemCoord Coord;

    public GemInfo(GemMainType _Type)
    {
        MainType = _Type;
    }
    
    public GemInfo(GemMainType _Type, GemCoord _Coord)
    {
        MainType = _Type;
        Coord = _Coord;
    }

    public void SetType(GemMainType _Type)
    {
        MainType = _Type;
    }

    public void SetCoord(GemCoord _coord)
    {
        Coord = _coord;
    }
}


public class Gem : MonoBehaviour
{

    private GemMatchManager manager;

    [SerializeField] public GemInfo Info;

    [SerializeField] private CurveMovement curveMovement;
    [SerializeField] private float swapDuration = 0f;
    [SerializeField] private AnimationCurve swapCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField] private float downAddY = 0f;
    [SerializeField] private float firstDownDuration = 0f;
    [SerializeField] private AnimationCurve firstDownCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField] private float downReturnDuration = 0f;
    [SerializeField] private AnimationCurve downReturnCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField] private SpriteRenderer highlightSprite;
    [SerializeField] private SpriteRenderer stripeSpriteRenderer;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Init(GemMatchManager _manager, GemInfo _Info)
    {
        manager = _manager;
        Info = _Info;
    }
    public void InitMainType(GemMainType _Type)
    {
        Info.SetType(_Type);
        SetVisual();
    }

    public void SetSubType(GemSubType _Type)
    {
        Info.SubType = _Type;
        var m = (int)Info.MainType * 2;
        var s = (int)_Type;
        if (_Type != GemSubType.Normal)
        {
            stripeSpriteRenderer.sprite = manager.gemStripeSpriteList[m + s];
            stripeSpriteRenderer.gameObject.SetActive(true);
        }
    }

    public void SetVisual()
    {
        highlightSprite.sprite = manager.gemHighlightSpriteList[(int)Info.MainType];
        spriteRenderer.sprite = manager.gemSpriteList[(int)Info.MainType];
        spriteRenderer.gameObject.SetActive(true);
    }

    public void PlayRemove(GemMainType _gemCustomType = GemMainType.None)
    {
        manager.curScore++;
        manager.scoreText.text = "Á¡¼ö: " + manager.curScore.ToString();
        StartCoroutine(PlayRemoveCoroutine(_gemCustomType));
    }

    public IEnumerator PlayRemoveCoroutine(GemMainType _gemCustomType = GemMainType.None)
    {
        highlightSprite.gameObject.SetActive(true);

        var alpha = 0f;

        while(alpha < 1f)
        {
            highlightSprite.color = new Color(1f, 1f, 1f, alpha);
            alpha += 0.1f;
            yield return new WaitForSeconds(0.01f);
        }

        highlightSprite.gameObject.SetActive(false);

        manager.effectManager.ShowRemoveEffect(gameObject.transform.position, _gemCustomType == GemMainType.None ? Info.MainType : _gemCustomType);

        Destroy(gameObject);
    }

    public void Move(Vector2 _targetPos, GemCoord _initCoord, Action _callBack = null)
    {
        Info.Coord = _initCoord;
        curveMovement.duration = swapDuration;
        curveMovement.animationCurve = swapCurve;
        curveMovement.Move(_targetPos, _callBack);
    }

    public void MoveDown(Vector2 _targetPos, GemCoord _initCoord, Action _callBack = null)
    {
        Info.Coord = _initCoord;
        curveMovement.duration = firstDownDuration;
        curveMovement.animationCurve = firstDownCurve;
        curveMovement.Move(new Vector3(_targetPos.x, _targetPos.y + downAddY), 
            ()=>
            {
                curveMovement.duration = downReturnDuration;
                curveMovement.animationCurve = downReturnCurve;
                curveMovement.Move(_targetPos,_callBack);
            });
    }
}
