using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FootBallPlayer : MonoBehaviour
{
    bool hasBall = false;
    Ball myBall;
    Color myColor;
    bool dangered = false;
    bool stunned = false;
    bool waitingForPass = false;
    List<FootBallPlayer> team;
    float limitAttackX, limitDefenseX;
    NavMeshAgent navmesh;

    [SerializeField] Team myTeam;
    [SerializeField] Rol myRol;
    [SerializeField] BoxCollider goalZone;
    [SerializeField] float ShootPower = 2;
    [SerializeField] float PassPower = .2f;
    [SerializeField] float spreadDistance = 1.2f;
    [SerializeField] float stunnedTime = 1;
    [SerializeField] float MaxWaitingTime = 2.25f;
    Vector3 backOffset = new Vector3(1.5f, 0, 0);
    float timeWaiting = 0;
    float timeSinceLast = 0;
    Vector3 initialPos;
    Vector3 shootDirection;

    //Fmod
    private FMOD.Studio.EventInstance instance;
    FMODUnity.StudioListener listener;
    FMODUnity.StudioEventEmitter emitter;


    private void Start()
    {
        emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        listener = GetComponent<FMODUnity.StudioListener>();
        initialPos = transform.position;
        team = GameManager.getInstance().getTeam(myTeam);
        limitAttackX = GameManager.getInstance().getAttackZone(myTeam, myRol);
        limitDefenseX = GameManager.getInstance().getDefenseZone(myTeam, myRol);
        navmesh = GetComponent<NavMeshAgent>();
        if (myTeam == Team.TeamA)
        {
            myColor = Color.red;
            backOffset *= -1;
        }
        else if (myTeam == Team.TeamB)
            myColor = Color.blue;

        emitter.Play();
    }


    private void OnEnable()
    {
        if(emitter!=null)emitter.Play();
    }

    private void OnDisable()
    {
        emitter.Stop();
    }

    public void reset(bool back)
    {
        transform.position = initialPos;
        hasBall = false;
        shootDirection = Vector3.zero;
        dangered = false;
        myBall = null;
        waitingForPass = false;
        timeWaiting = 0;
        stunned = false;
        timeSinceLast = 0;
        //al resetear al agente si han marcado gol a su equipo empieza un poco más atrás
        if (back)
            transform.position += backOffset;

    }
    public void goTo(Transform target)
    {
        if (Vector3.Distance(transform.position, target.position) > 0.05 && !waitingForPass)
        {
            navmesh.isStopped = false;
            navmesh.SetDestination(target.position);
        }
    }
    public void goTo(Vector3 target)
    {
        if (Vector3.Distance(transform.position, target) > 0.05 && !waitingForPass)
        {
            navmesh.isStopped = false;
            Debug.DrawLine(transform.position, target, myColor);
            navmesh.SetDestination(target);
        }
    }
    private void Update()
    {
        //Se coloca la pelota delante del jugador. No se hace mediante jerarquía padre-hijo porque da problemas con la escala
        if (hasBall)
        {
            Vector3 diff = transform.forward.normalized * GetComponent<Collider>().bounds.extents.z * 2.0f;
            myBall.transform.position = transform.position + diff;
        }
        //Timer para dejar de estar aturdido
        if (stunned)
        {
            timeSinceLast += Time.deltaTime;
            if (timeSinceLast >= stunnedTime)
            {
                stunned = false;
                timeSinceLast = 0;
            }
        }
        //Timer para dejar de esperar para el pase si pasa más de un tiempo determinado
        if (waitingForPass)
        {
            timeWaiting += Time.deltaTime;
            if (timeWaiting >= MaxWaitingTime)
            {
                timeWaiting = 0;
                waitingForPass = false;
            }
        }       
    }
    public void Shoot()
    {
        //Si el agente tiene la pelota y puede tirar aplicamos un empuje en la dirección de tiro de fuerza proporcional a la distancia y a una constante
        if (hasBall && shootDirection != Vector3.zero)
        {
            //Se ejecuta el sonido de tiro                                    
            instance = FMODUnity.RuntimeManager.CreateInstance("event:/SonidosPasesYTiros");
            instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
            instance.setParameterByName("Fuerza", shootDirection.magnitude / 30.0f);
            instance.start();
            instance.release();

            transform.LookAt(shootDirection + transform.position);
            myBall.transform.position = transform.position + transform.forward;
            Rigidbody ballrb = myBall.GetComponent<Rigidbody>();

            ballrb.AddForce(shootDirection.normalized * ShootPower * shootDirection.magnitude, ForceMode.Impulse);
            //Se avisa de que la pelota está en el aire para evitar pasar a estado sin balón
            myBall.setBallOnAir(true);
            myBall.setLastPlayer(this);
            setHasBall(null);
            //Avisamos a un portero para que se tire a un lugar aleatorio
            GameManager.getInstance().notifyGoalKeeper(myTeam);
        }
    }
    public void Pass(FootBallPlayer mate)
    {
        //Si el compañero es del equipo del agente
        if (mate != this && myTeam == mate.myTeam)
        {
            //Se avisa al agente para que espere
            mate.setWaitingForPass(true);
            Vector3 dir = mate.transform.position - transform.position;
            mate.navmesh.isStopped = true;
            //Si realmente se tiene la bola se mira hacia el compañero y se aplica un impulso proporcional a la distancia y a una constante
            if (hasBall)
            {
                //Se ejecuta el sonido de pase                
                instance = FMODUnity.RuntimeManager.CreateInstance("event:/SonidosPasesYTiros");
                instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
                instance.setParameterByName("Fuerza", dir.magnitude/30.0f);
                instance.start();
                instance.release();

                Rigidbody ballrb = myBall.GetComponent<Rigidbody>();
                transform.LookAt(mate.transform.position);
                myBall.transform.position = transform.position + transform.forward;
                //Se avisa de que la pelota está en el aire para evitar pasar a estado sin balón
                myBall.setBallOnAir(true);
                ballrb.AddForce(dir.normalized * dir.magnitude * PassPower, ForceMode.Impulse);
                myBall.setLastPlayer(this);
                setHasBall(null);
            }
        }
    }
    public Team getMyTeam() { return myTeam; }
    public Rol getMyRol() { return myRol; }
    public bool isInDanger() { return dangered; }
    public void setDangered(bool v) { dangered = v; }
    public BoxCollider getGoalZone() { return goalZone; }
    public void setShootDirection(Vector3 dir) { shootDirection = dir; }
    public Vector3 getShootDirection() { return shootDirection; }
    public List<FootBallPlayer> getTeam() { return team; }
    public float getLimitAttack() { return limitAttackX; }
    public float getLimitDefense() { return limitDefenseX; }
    public bool isStunned() { return stunned; }
    public void stun() { stunned = true; }
    public bool getWaitingForPass() { return waitingForPass; }
    public void setWaitingForPass(bool b)
    {
        if (b) GetComponent<Rigidbody>().velocity = Vector3.zero;
        waitingForPass = b;
    }
    public void setHasBall(Ball ball)
    {
        hasBall = ball != null;
        if (hasBall)
        {
            //si el agente consigue la pelota deja de esperar
            GameManager.getInstance().setBallOwner(this);
            timeWaiting = 0;
            waitingForPass = false;
            //listener.enabled = true;
        }
        else
        {
            GameManager.getInstance().setBallOwner(null);
            //listener.enabled = false;
        }
        myBall = ball;
    }
    public bool getHasBall() { return hasBall; }
    public void spread()
    {
        //si el agente no puede desmarcarse va a su límite de ataque por defecto
        Vector3 defaultPos = new Vector3(limitAttackX, transform.position.y, transform.position.z);
        FootBallPlayer owner = GameManager.getInstance().getBallOwner();
        //Se lanza un rayo en dirección al compañero con la pelota si es del equipo del agente y si colisiona con un contrario se dirige a un punto
        //en la línea perpendicular a este rayo.
        if (owner != null && owner.getMyTeam() == myTeam)
        {

            RaycastHit info;
            int layer = LayerMask.GetMask(LayerMask.LayerToName(this.gameObject.layer));
            Vector3 dir = owner.transform.position - transform.position;
            if (Physics.Raycast(transform.position, dir.normalized, out info, dir.magnitude, layer) &&
                info.collider.gameObject.GetComponent<FootBallPlayer>().myTeam != myTeam)
            {
                //Hay dos posibles vectores perpendiculares así que se elige aquel cuyo punto resultante deje al agente más cerca de la portería rival
                Vector3 orthogonal1 = transform.position + new Vector3(-dir.z, dir.y, dir.x).normalized * spreadDistance;
                Vector3 orthogonal2 = transform.position + new Vector3(dir.z, dir.y, -dir.x).normalized * spreadDistance;
                Vector3 closest = Vector3.Distance(orthogonal1, goalZone.transform.position) < Vector3.Distance(orthogonal2,
                    goalZone.transform.position) ? orthogonal1 : orthogonal2;
                Vector3 DirToClosest = closest - transform.position;
                //Si ya hay alguien en esa posición o alguien se interpondría en el camino del agente se desmarca hacia el otro lado
                if (Physics.Raycast(transform.position, DirToClosest.normalized, DirToClosest.magnitude, layer))
                    closest = closest == orthogonal1 ? orthogonal2 : orthogonal1;
                goTo(closest);
            }
            else goTo(defaultPos);

        }
        else goTo(defaultPos);
    }

    public void coverPlayer(Vector3 enemyPos)
    {
        //El agente se dirige al punto entre la pelota y el enemigo en el que está lo más cerca del enemigo posible
        Vector3 dir = (GameManager.getInstance().getBallPosition() - enemyPos).normalized;
        goTo(enemyPos + dir * (GetComponent<Collider>().bounds.extents.x * 2));
    }
}
