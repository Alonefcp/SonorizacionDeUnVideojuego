using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    //si un jugador del equipo contrario al del agente entra en el cono de visión el agente está en peligro
    private void OnTriggerEnter(Collider other)
    {
        FootBallPlayer footBall = other.gameObject.GetComponent<FootBallPlayer>();
        FootBallPlayer playerFootball = GetComponentInParent<FootBallPlayer>();
        if (footBall && playerFootball && footBall.getMyTeam() != playerFootball.getMyTeam())
        {
            playerFootball.setDangered(true);
        }
    }
    //si un jugador del equipo contrario al del agente sale del cono de visión el agente deja de estar en peligro
    private void OnTriggerExit(Collider other)
    {
        FootBallPlayer footBall = other.gameObject.GetComponent<FootBallPlayer>();
        FootBallPlayer playerFootball = GetComponentInParent<FootBallPlayer>();
        if (footBall && playerFootball && footBall.getMyTeam() != playerFootball.getMyTeam())
        {
            playerFootball.setDangered(false);
        }
    }
}
