using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CurveMovement : MonoBehaviour
{
    public readonly float MIN_CURVE = -1.0f;
    public readonly float MAX_CURVE = 1.0f;

    public enum MovementType
    {
        Curve = 0,
        Linear = 1
    }

    public float duration = 0.6f;

    [Space]
    public MovementType movementType = MovementType.Curve;
    public float min = -0.5f;
    public float max = 0.5f;
    public AnimationCurve animationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Space]
    public bool useScale = false;
    public Vector3 startScale = Vector3.one;
    public Vector3 endScale = Vector3.one;
    public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private Transform cachedTransform;
    public Transform CachedTransfom
    {
        get
        {
            if (cachedTransform == null)
            {
                cachedTransform = GetComponent<Transform>();
            }

            return cachedTransform;
        }
    }

    public Vector3 CurrentTargetPosition { get; private set; }
    public Vector3 CurrentStartPosition { get; private set; }
    public bool IsLocalPosition { get; private set; }

    public void Move(Vector3 position, Action onComplete = null)
    {
        Move(position, false, onComplete);
    }

    public void LocalMove(Vector3 localPosition, Action onComplete = null)
    {
        Move(localPosition, true, onComplete);
    }

    private void Move(Vector3 position, bool isLocalPosition, Action onComplete)
    {
        Stop();
        StartCoroutine(UpdateCoroutine(position, isLocalPosition, onComplete));
    }

    public IEnumerator WaitForMoveComplete(Vector3 position)
    {
        yield return WaitForMoveComplete(position, false);
    }

    public IEnumerator WaitForLocalMoveComplete(Vector3 position)
    {
        yield return WaitForMoveComplete(position, true);
    }

    private IEnumerator WaitForMoveComplete(Vector3 position, bool isLocalPosition)
    {
        bool isDone = false;

        Move(position, isLocalPosition, () => isDone = true);

        while (isDone == false)
        {
            yield return null;
        }
    }

    public void Stop()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdateCoroutine(Vector3 targetPosition, bool isLocalPosition, Action onComplete)
    {
        var startPosition = isLocalPosition == true ? CachedTransfom.localPosition : CachedTransfom.position;
        targetPosition.z = startPosition.z;
        float currentTime = 0;

        IsLocalPosition = isLocalPosition;
        CurrentStartPosition = startPosition;
        CurrentTargetPosition = targetPosition;

        var min = Mathf.Clamp(this.min, MIN_CURVE, MAX_CURVE);
        var max = Mathf.Clamp(this.max, MIN_CURVE, MAX_CURVE);

        float curveValue = 0;

        if (max < min)
        {
            curveValue = min;
        }
        else
        {
            curveValue = UnityEngine.Random.Range(min, max);
        }

        if (movementType == MovementType.Linear)
        {
            curveValue = 0.0f;
        }
        else
        {
            curveValue = curveValue < 0 ? MIN_CURVE - curveValue : MAX_CURVE - curveValue;
            curveValue *= 10.0f;
        }

        Vector3 center = (startPosition + targetPosition) * 0.5F;
        center.y -= curveValue;

        Vector3 riseRelCenter = startPosition - center;
        Vector3 setRelCenter = targetPosition - center;

        if (useScale)
        {
            UpdateScale(0.0f);
        }

        while (true)
        {
            float t = currentTime / duration;

            UpdateMove(t, riseRelCenter, setRelCenter, center, isLocalPosition);

            if (useScale)
            {
                UpdateScale(t);
            }

            if (currentTime > duration)
            {
                break;
            }

            currentTime += Time.smoothDeltaTime;

            yield return null;
        }

        if (onComplete != null)
        {
            onComplete();
        }
    }

    private void UpdateMove(float time, Vector3 riseRelCenter, Vector3 setRelCenter, Vector3 center, bool isLocalPosition)
    {
        var targetTime = animationCurve.Evaluate(time);

        Vector3 movePosition = Vector3.zero;

        if (movementType == MovementType.Linear)
        {
            movePosition = Vector3.Lerp(riseRelCenter, setRelCenter, targetTime) + center;
        }
        else
        {
            movePosition = Vector3.Slerp(riseRelCenter, setRelCenter, targetTime) + center;
        }

        if (isLocalPosition == true)
        {
            CachedTransfom.localPosition = movePosition;
        }
        else
        {
            CachedTransfom.position = movePosition;
        }
    }

    private void UpdateScale(float time)
    {
        var targetTime = animationCurve.Evaluate(time);
        var targetScale = Vector3.Lerp(startScale, endScale, targetTime);
        CachedTransfom.localScale = targetScale;
    }
}
