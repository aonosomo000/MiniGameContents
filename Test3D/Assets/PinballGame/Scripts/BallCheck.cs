using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCheck : MonoBehaviour
{
    public DropMachine dropMachine;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("SecondGoal"))
        {
            dropMachine.CheckSecondGoal(collision.gameObject.GetComponent<SecondGoalObject>().secondGoalIndex, dropMachine.prediction2d.curIterationIndex);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("FinalGoal"))
        {
            dropMachine.CheckFinalGoal(collision.gameObject.GetComponent<GoalBox>().goalIndex);
            gameObject.SetActive(false);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("TeleportCol"))
        {
            collision.gameObject.GetComponent<TeleportObject>().TeleportBall(transform);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("SecondGoal"))
        {
            dropMachine.CheckSecondGoal(collision.gameObject.GetComponent<SecondGoalObject>().secondGoalIndex, dropMachine.prediction2d.curIterationIndex);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("TeleportCol"))
        {
            collision.gameObject.GetComponent<TeleportObject>().PlayTeleportEffect();
        }
    }
}
