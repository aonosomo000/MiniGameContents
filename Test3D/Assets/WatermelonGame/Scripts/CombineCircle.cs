using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum CircleColorType
{
    YELLOW = 0, //FFE500
    GREEN ,      //60FF91,
    PURPLE,     //9700FF
}

[Serializable]
public class CircleInfo
{
    public int Id { get; private set; }
    public int Level { get; private set; }
    public CircleType Type { get; private set; }
    public CircleColorType Color { get; private set; }

    public CircleInfo(int id, int level, CircleType type, CircleColorType color)
    {
        Id = id;
        Level = level;
        Type = type;
        Color = color;
    }

    public void SetLevel(int value)
    {
        Level = value;
    }
}
public class CombineCircle : MonoBehaviour
{
    private readonly int MAX_LEVEL = 10;

    [SerializeField] private CircleDropMachine machine;

    [SerializeField] private CircleInfo info;

    [SerializeField] private bool isLinePass = false;

    [SerializeField] private float curValue = 0f;

    [SerializeField] private TMPro.TextMeshProUGUI valueText;

    private float originScaleX = 0f;
    private float originScaleY = 0f;

    [SerializeField] private GameObject maxLevelEffect;
    [SerializeField] private GameObject combineEffect;
    [SerializeField] private GameObject wildEffect;
    [SerializeField] private Collider2D collider;
    [SerializeField] private Rigidbody2D rigid;
    [SerializeField] private SpriteRenderer coinSprite;
    [SerializeField] private SpriteRenderer wildSprite;

    private bool isCombine = false;
    private bool isDie = false;

    private List<Transform> nearSameCircle = new List<Transform>();

    [SerializeField] private bool isInit = false;
    [SerializeField] private int levelT = 0;
    [SerializeField] private CircleType typeT = CircleType.Coin;
    [SerializeField] private CircleColorType colorT = CircleColorType.YELLOW;

    private void Start()
    {
        if (isInit)
        {
            originScaleX = 1f;
            originScaleY = 1f;
            info = new CircleInfo(0, levelT, typeT, colorT);
            isLinePass = true;
            SetValue();
        }
    }

    public void Init(CircleDropMachine _machine, CircleInfo _info)
    {
        originScaleX = transform.localScale.x;
        originScaleY = transform.localScale.y;
        machine = _machine;
        info = _info;
        isLinePass = false;
        SetValue();
    }

    public CircleInfo GetInfo()
    {
        return info;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(isDie)
        {
            return;
        }

        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Combine"))
        {
            var otherCircle = collision.transform.GetComponent<CombineCircle>();

            if (info.Type == CircleType.Wild)
            {
                ToCombine(otherCircle.transform);
                otherCircle.FromCombine();
            }
            else if(info.Type == CircleType.Coin)
            {
                if (isLinePass)
                {
                    if (otherCircle.isCombine || isCombine)
                    {
                        return;
                    }


                    else if (otherCircle.info.Level == info.Level && otherCircle.info.Color == info.Color)
                    {
                        ToCombine(otherCircle.transform);
                        otherCircle.FromCombine();
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (info.Type == CircleType.Destroyer)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Combine"))
            {
                var otherCircle = collision.transform.GetComponent<CombineCircle>();
                machine.effectManager.ShowEffect(collision.transform.position, CircleEffectType.DestroyerCol);
                otherCircle.DestroyCircle();

                machine.SetScoreText(1);

                if (!isDie)
                {
                    StartCoroutine(DestroyerLifeCoroutine());
                }
            }
        }
        else if(info.Type == CircleType.Coin || info.Type == CircleType.Wild)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("DeadZone"))
            {
                if (!isLinePass)
                {
                    isLinePass = true;
                }
                else
                {
                    machine.GameEnd();
                }
            }
        }
    }

    public void DestroyCircle()
    {
        isDie = true;
        Destroy(gameObject);
    }

    public void FromCombine()
    {
        StartCoroutine(FromCombineCoroutine());
    }
    public void ToCombine(Transform target)
    {
        valueText.text = "";
        rigid.velocity = new Vector2(0f, 0f);
        rigid.freezeRotation = true;
        rigid.isKinematic = true;
        collider.isTrigger = true;

        StartCoroutine(ToCombineCoroutine(target));
    }

    public IEnumerator FromCombineCoroutine()
    {
        isCombine = true;
        info.SetLevel(info.Level + 1);

        machine.SetScoreText(info.Level);

        SetValue();

        combineEffect.SetActive(false);
        combineEffect.SetActive(true);

        yield return new WaitForSeconds(0.1f);


        if(info.Level == MAX_LEVEL)
        {
            yield return PlayMaxLevel();
        }
        else
        {
            GetComponent<Collider2D>().enabled = false;
            GetComponent<Collider2D>().enabled = true;
            isCombine = false;
        }

    }
    public IEnumerator ToCombineCoroutine(Transform target)
    {
        isCombine = true;
        GetComponent<Collider2D>().enabled = false;
        var maxStep = 10f;
        var curStep = 0f;

        while (curStep < maxStep)
        {
            transform.position = new Vector3(
                transform.position.x + (target.position.x - transform.position.x) * curStep / maxStep,
                transform.position.y + (target.position.y - transform.position.y) * curStep / maxStep,
                0f
                );
            transform.localScale = new Vector3(
                transform.localScale.x + (target.localScale.x - transform.localScale.x) * curStep / maxStep,
                transform.localScale.y + (target.localScale.y - transform.localScale.y) * curStep / maxStep,
                transform.localScale.z + (target.localScale.z - transform.localScale.z) * curStep / maxStep
                );
            coinSprite.color = new Color(
                1f,
                1f,
                1f,
                coinSprite.color.a * (maxStep - curStep) / maxStep
                );
            curStep++;
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(gameObject);
    }


    public IEnumerator PlayMaxLevel()
    {
        machine.SetScoreText(100);

        maxLevelEffect.SetActive(false);
        maxLevelEffect.SetActive(true);
        yield return new WaitForSeconds(0.52f);
        valueText.text = "";
        coinSprite.enabled = false;
        rigid.simulated = false;

        GetComponent<Collider2D>().enabled = false;


        yield return new WaitForSeconds(0.5f);

        Destroy(gameObject);
    }

    private IEnumerator DestroyerLifeCoroutine()
    {
        isDie = true;
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private void SetValue()
    {
        var resScaleMulti = 1f;

        if (info.Type == CircleType.Wild)
        {
            resScaleMulti = 1.5f;
            wildEffect.SetActive(true);
            wildSprite.gameObject.SetActive(true);

            valueText.color = new Color(0f, 0f, 0f);
            valueText.text ="";
            coinSprite.color = new Color(1f,1f,1f);
        }
        else if(info.Type == CircleType.Coin)
        {
            resScaleMulti = machine.circleEditList[info.Level].scale;
            valueText.color = new Color(0f, 0f, 0f);
            valueText.text = SetStringMulti();
            machine.SetMaxLevel(info.Level);

            if(info.Color == CircleColorType.YELLOW)
            {
                coinSprite.color = new Color(1f, 229f/255f, 0f);
            }
            else if (info.Color == CircleColorType.PURPLE)
            {
                coinSprite.color = new Color(151f/255f, 0f, 1f);
            }
            else if (info.Color == CircleColorType.GREEN)
            {
                coinSprite.color = new Color(96f/255f, 1f, 145f/255f);
            }
        }
        else if(info.Type == CircleType.Destroyer)
        {
            
        }

        transform.localScale = new Vector3(originScaleX * resScaleMulti, originScaleY * resScaleMulti, originScaleY * resScaleMulti);

    }

    private string SetStringMulti()
    {
        return info.Level.ToString();
    }

    private string SetKMB(float value)
    {
        var k = 1000f * 1000f;
        var m = k * 1000f;
        var b = m * 1000f;

        var kd = 0.001f;
        var md = kd * 0.001f;
        var bd = md * 0.001f;

        if (value < 1000)
        {
            return value.ToString("0");
        }
        else if (value < k) //K
        {
            return (value * kd).ToString("0") + "K";
        }
        else if (value < m) //M
        {
            return (value * md).ToString("0") + "M";
        }
        else if (value < b) //B
        {
            return (value * bd).ToString("0") + "B";
        }
        else
        {
            return "0";
        }
    }
}
