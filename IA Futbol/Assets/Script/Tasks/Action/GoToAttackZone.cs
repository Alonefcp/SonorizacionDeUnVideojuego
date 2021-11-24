using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;
[TaskCategory("IAFutbol")]
[TaskDescription("Se va hacia la zona de ataque")]
public class GoToAttackZone : Action
{
    FootBallPlayer player;
    NavMeshAgent navMeshAgent;
    Transform attackZone;
    Vector3 target;
    public override void OnAwake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GetComponent<FootBallPlayer>();

    }
    public override void OnStart()
    {
        navMeshAgent.isStopped = false;
        target = new Vector3(player.getLimitAttack(), transform.position.y, transform.position.z);
        SetDestination(target);
    }

    public override TaskStatus OnUpdate()
    {
        if (HasArrived())
        {
            return TaskStatus.Success;
        }

        SetDestination(target);

        return TaskStatus.Running;
    }



   
    private bool SetDestination(Vector3 destination)
    {
        navMeshAgent.isStopped = false;
        return navMeshAgent.SetDestination(destination);
    }
    private bool HasArrived()
    {
  
        float remainingDistance;
        if (navMeshAgent.pathPending)
        {
            remainingDistance = float.PositiveInfinity;
        }
        else
        {
            remainingDistance = navMeshAgent.remainingDistance;
        }

        return remainingDistance <= 0;
    }


    private void Stop()
    {
        if (navMeshAgent.hasPath)
        {
            navMeshAgent.isStopped = true;
        }
    }


    public override void OnEnd()
    {
        Stop();
    }


    public override void OnBehaviorComplete()
    {
        Stop();
    }


}

