using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportObject : MonoBehaviour
{
    [SerializeField] private float power = 0;
    [SerializeField] private Transform teleportT;

    [SerializeField] private GameObject teleportEnterEffect;
    [SerializeField] private GameObject teleportExitEffect;

    public void TeleportBall(Transform ballT)
    {
        var rigid = ballT.GetComponent<Rigidbody2D>();
        rigid.velocity = new Vector2(0f,0f);

        ballT.localEulerAngles = new Vector3(0f, 0f, 0f);
        ballT.position = new Vector3(teleportT.position.x,
            teleportT.position.y, ballT.position.z);

        rigid.AddForce(teleportT.up * power, ForceMode2D.Impulse);
    }

    public void PlayTeleportEffect()
    {
        teleportEnterEffect.SetActive(false);
        teleportExitEffect.SetActive(false);
        teleportEnterEffect.SetActive(true);
        teleportExitEffect.SetActive(true);
    }
}
