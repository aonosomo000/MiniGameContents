using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothRotating : MonoBehaviour
{
    [SerializeField] private Transform targetT;
    void FixedUpdate()
    {
        transform.localEulerAngles = new Vector3(0f, 0f, -targetT.localEulerAngles.z);
    }
}
