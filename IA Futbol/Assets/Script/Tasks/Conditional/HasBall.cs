using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

[TaskCategory("IAFutbol")]
[TaskDescription("Comprueba si el jugador tiene la pelota en posesion")]
public class HasBall : Conditional
{
    private FootBallPlayer player;

    public override void OnAwake()
    {
        player = GetComponent<FootBallPlayer>();
    }
    public override TaskStatus OnUpdate()
    {
        if (player.getHasBall()) return TaskStatus.Success;
        else return TaskStatus.Failure;
    }
}
