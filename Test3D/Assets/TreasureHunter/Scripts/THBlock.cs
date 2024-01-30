using System;
using UnityEngine;

public enum DestroyType
{
    Normal = 0,
    CrossX,
    Vertical,
    Horizontal
}

[Serializable]
public struct BlockCoord
{
    public int x;
    public int z;

    public BlockCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }
}

public class THBlock : MonoBehaviour
{
    public DestroyType type = DestroyType.Normal;
    [SerializeField] private BoxCollider col;
    public GameObject meshObject;
    public GameObject highLightSprite;

    public BlockCoord coord;

    public void DestroyBlock()
    {
        col.enabled = false;
        meshObject.SetActive(false);
    }

    public void ShowHighLight(float alpha)
    {
        highLightSprite.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, alpha);
    }
}
