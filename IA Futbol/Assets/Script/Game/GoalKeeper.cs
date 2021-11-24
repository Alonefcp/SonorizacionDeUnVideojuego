using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class GoalKeeper : MonoBehaviour
{
    [SerializeField] Collider goal;
    NavMeshAgent navmesh;
    Vector3 initialPos;

    private void Start()
    {
        goal = GetComponent<Collider>();
        navmesh = GetComponent<NavMeshAgent>();
        initialPos = transform.position;
    }
    public void Catch()
    {
        int rand = Random.Range(0, 2);
        Vector3 randomPoint;
       //Se dirige de forma aleatoria a la izquierda de la portería o a la derecha
        if (rand == 0)
            randomPoint = new Vector3(transform.position.x, transform.position.y, goal.bounds.center.z + goal.bounds.extents.z);
        else
            randomPoint = new Vector3(transform.position.x, transform.position.y, goal.bounds.center.z - goal.bounds.extents.z);
        navmesh.SetDestination(randomPoint);
    }

    public void reset()
    {
        transform.position = initialPos;
    }
}
