using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalZone : MonoBehaviour
{

    [SerializeField] Team Team;
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.GetComponent<Ball>())
            GameManager.getInstance().Goal(Team);
    }
}
