using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class GoalKeeper : MonoBehaviour
{
    [SerializeField] Collider goal;
    NavMeshAgent navmesh;
    Vector3 initialPos;

    //Fmod
    private FMOD.Studio.EventInstance instance;

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

    private void OnCollisionEnter(Collision collision)
    {
        //Si el portero choca con la pelota se ejecuta el sonido de parada
        if(collision.gameObject.GetComponent<Ball>()!=null)
        {
            instance = FMODUnity.RuntimeManager.CreateInstance("event:/SonidoPortero");
            instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
           
            instance.start();
            instance.release();
        }
    }
}
