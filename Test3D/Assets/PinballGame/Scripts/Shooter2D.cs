using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter2D : MonoBehaviour
{
    public DropMachine dropMachine;
    public GameObject firePoint;
    public GameObject ballPrefab;
    public float power;

    Vector3 currentPosition;
    Quaternion currentRotation;


    public void Init()
    {
        currentPosition = transform.position;
        currentRotation = transform.rotation;
    }

    public Vector2 calculateForce()
    {
        return new Vector2(0f, 1f*power);
    }

    void shoot()
    {
        GameObject ball = Instantiate(ballPrefab, firePoint.transform.position, Quaternion.identity);
        ball.GetComponent<Rigidbody2D>().AddForce(calculateForce(), ForceMode2D.Impulse);
    }

    public void MoveShooter(float x, float y)
    {
        transform.position = new Vector3(x, y, 0f);

        predict();
    }

    private void FixedUpdate()
    {
        if(!dropMachine.isTest)
        {
            predict();
        }
    }
    private void predict()
    {
        Prediction2D.instance.predict(ballPrefab, firePoint.transform.position, calculateForce());
    }
}
