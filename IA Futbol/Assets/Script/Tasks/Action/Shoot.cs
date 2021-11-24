using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("IAFutbol")]
[TaskDescription("Chuta a puerta")]

public class Shoot : Action
{
    private FootBallPlayer player;
    public override void OnAwake()
    {
        player = GetComponent<FootBallPlayer>();
    }
    public override TaskStatus OnUpdate()
    {
        player.Shoot();
        return TaskStatus.Success;
    }
}
