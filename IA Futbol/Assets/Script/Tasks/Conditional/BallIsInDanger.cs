using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("IAFutbol")]
[TaskDescription("Comprueba si el jugador se siente amenazado por un contrario")]
public class BallIsInDanger : Conditional
{
    private FootBallPlayer player;

    public override void OnAwake()
    {
        player = GetComponent<FootBallPlayer>();
    }
    public override TaskStatus OnUpdate()
    {
        if (player.isInDanger()) return TaskStatus.Success;
        else return TaskStatus.Failure;
    }

}
