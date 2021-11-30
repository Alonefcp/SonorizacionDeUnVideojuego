using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ball : MonoBehaviour
{
    Rigidbody rb;
    Vector3 initialPosition;
    [SerializeField] LayerMask campoLayer;
    const float maxBallOnAirTime = 1;
    float timeOnAir = 0;
    bool ballOnAir = true;
    public void setBallOnAir(bool ballMoving)
    {
        ballOnAir = ballMoving;
    }

    public bool getBallOnAir()
    {
        return ballOnAir;
    }
    private void Update()
    {
        //Ponemos un tope al tiempo que puede estar el balón en el aire para que no sea una "bala perdida" hasta que colisione
        if (ballOnAir)
        {
            timeOnAir += Time.deltaTime;
            if (timeOnAir >= maxBallOnAirTime)
            {
                timeOnAir = 0;
                ballOnAir = false;
            }
        }
    }
    private void Awake()
    {
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }
    public void reset()
    {
        transform.position = initialPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        ballOnAir = false;
        timeOnAir = 0;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (LayerMask.GetMask(LayerMask.LayerToName(collision.gameObject.layer)) != campoLayer)
        {
            //el balón ya no está en el aire
            ballOnAir = false;
            FootBallPlayer player = collision.gameObject.GetComponent<FootBallPlayer>();
            //si el jugador receptor no está aturdido cogerá la pelota
            if (player && !player.isStunned())
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                //Si ya tenía la pelota otro jugador se la quitamos y lo aturdimos
                FootBallPlayer owner = GameManager.getInstance().getBallOwner();
                if (owner != null)
                {                  
                    owner.setHasBall(null);
                    owner.stun();
                }

                player.setHasBall(this);
                //si estábamos esperando un pase dejamos de esperar
                if (player.getWaitingForPass())
                    player.setWaitingForPass(false);
            }
        }
    }
}
