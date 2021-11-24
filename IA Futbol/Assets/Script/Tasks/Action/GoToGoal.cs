using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

[TaskCategory("IAFutbol")]
[TaskDescription("Se va hacia la portería")]
public class GoToGoal : Action
{
    NavMeshAgent navMeshAgent;
    Transform goalZone;
    Vector3 target;
    public override void OnAwake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        goalZone = GetComponent<FootBallPlayer>().getGoalZone().transform;
    }

    public override void OnStart()
    {
        navMeshAgent.isStopped = false;
        SetDestination(target);
        target = goalZone.position;
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
