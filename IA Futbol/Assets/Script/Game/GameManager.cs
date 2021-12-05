using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Team : uint { TeamA = 0, TeamB = 1, Nobody = 2 };
public enum Zone : uint { Goal, Attack, Center, Defense };
public enum Rol : uint { Delantero = 0, Centro = 1, Defensa = 2 };
public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;

    private Team winner;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            initTeams();
            teamANameText.text = names[0];
            teamBNameText.text = names[1];
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void initTeams()
    {
        //se añaden los jugadores al vector desde el padre común del equipo
        foreach (Transform player in teamA)
        {
            FootBallPlayer football = player.gameObject.GetComponent<FootBallPlayer>();
            if (football)
                TeamAPlayers.Add(football);
            else
            {
                GoalKeeper goalKeeper = player.gameObject.GetComponent<GoalKeeper>();
                if (goalKeeper)
                    goalKeeperTeamA = goalKeeper;
            }
        }
        foreach (Transform player in teamB)
        {
            FootBallPlayer football = player.gameObject.GetComponent<FootBallPlayer>();
            if (football)
                TeamBPlayers.Add(football);
            else
            {
                GoalKeeper goalKeeper = player.gameObject.GetComponent<GoalKeeper>();
                if (goalKeeper)
                    goalKeeperTeamB = goalKeeper;
            }
        }
    }

    const int MaxGoals = 3;
    int[] goals = new int[2];
    [SerializeField] Text WinnerText;
    [SerializeField] Text[] teams = new Text[2];
    [SerializeField] Text teamANameText, teamBNameText;
    [SerializeField] string[] names = new string[2];
    [SerializeField] Transform teamA, teamB;
    [SerializeField] Transform zonaA, zonaB, zonaMedio, goalZoneA, goalZoneB, ATKTeamA, ATKTeamB;
    List<FootBallPlayer> TeamAPlayers = new List<FootBallPlayer>();
    List<FootBallPlayer> TeamBPlayers = new List<FootBallPlayer>();
    [SerializeField] Ball ball;
    [SerializeField] Collider campo;
    [SerializeField] HiveMind[] hiveMinds = new HiveMind[2];
    GoalKeeper goalKeeperTeamA;
    GoalKeeper goalKeeperTeamB;
    FootBallPlayer ballOwner;
    [SerializeField] Transform gradasEquipoAzul;
    [SerializeField] Transform gradasEquipoRojo;

    float currentTime = 0f;
    float maxTime = 70f;
    float minTime = 40f;
    

    private void Update()
    {
        currentTime += Time.deltaTime;
        if(currentTime >= Random.Range(minTime,maxTime))
        {
            currentTime = 0;
            Vector3 pos = new Vector3(Random.Range(-10, 10), 27.88f, Random.Range(-5, 5));
            FMODUnity.RuntimeManager.PlayOneShot("event:/Canticos", pos);
        }
    }

    public void notifyGoalKeeper(Team shooterTeam)
    {
        //Se avisa al portero del equipo contrario al recibido que tiene que lanzarse
        if (shooterTeam == Team.TeamA)
            goalKeeperTeamB.Catch();
        else goalKeeperTeamA.Catch();
    }
    public static GameManager getInstance()
    {
        if (_instance == null)
        {
            throw new System.Exception("Singleton not created");
        }
        return _instance;
    }

    public void Goal(Team team)
    {       
        //Al marcar la bola no está en el aire
        ball.setBallOnAir(false);
     
        //Se castea el equipo al que han marcado a entero y se le suma gol
        uint teamN = (uint)team;
        if(teamN==0) //equipo azul
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/GradasGol",gradasEquipoAzul.position);           
          
        }
        else if(teamN==1) //equipo rojo
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/GradasGol", gradasEquipoRojo.position);
        }
        goals[teamN]++;
        //Se ejecuta el sonido de cuando se marca un gol
        if (goals[teamN] < MaxGoals) FMODUnity.RuntimeManager.PlayOneShot("event:/PitidoGol");
        teams[teamN].text = goals[teamN].ToString();
        //Si a uno de los equipos le han marcado el máximo de goles ha ganado el contrario. Se desactivan los jugadores y las IA multicapa
        if (goals[teamN] >= MaxGoals)
        {
            //Se ejecuta el sonido de cuando se acaba el partido
            FMODUnity.RuntimeManager.PlayOneShot("event:/PitidoFinalPartido");

            WinnerText.text = "Gana " + names[1 - teamN];
            teamA.gameObject.SetActive(false);
            teamB.gameObject.SetActive(false);
            foreach (HiveMind mind in hiveMinds)
            {
                mind.gameObject.SetActive(false);
            }
            
        }
        //en caso de seguir el juego se resetean las posiciones y estado de los agentes y la bola
        else
        {
            teamA.gameObject.SetActive(false);
            teamB.gameObject.SetActive(false);
            foreach (HiveMind mind in hiveMinds)
            {
                mind.gameObject.SetActive(false);
            }

            winner = team;
            Invoke("resetMatch", 3);
        }
    }

    public List<FootBallPlayer> getTeam(Team team)
    {
        if (team == Team.TeamA)
            return TeamAPlayers;
        else return TeamBPlayers;
    }

    public float getDefenseZone(Team team, Rol role)
    {
        if (role == Rol.Centro)
            return zonaMedio.position.x;
        else if (team == Team.TeamA)
        {
            if (role == Rol.Defensa) return zonaA.position.x;
            else return zonaB.position.x;
        }
        else
        {
            if (role == Rol.Defensa) return zonaB.position.x;
            else return zonaA.position.x;
        }
    }
    public Ball getBall() { return ball; }
    public float getCenterZone()
    {
        return zonaMedio.position.x;
    }
    public float getAttackZone(Team team, Rol role)
    {
        if (team == Team.TeamA)
        {
            if (role == Rol.Centro)
                return zonaB.position.x;
            else if (role == Rol.Defensa) return zonaA.position.x;
            else return ATKTeamA.position.x;
        }
        else
        {
            if (role == Rol.Centro)
                return zonaA.position.x;
            else if (role == Rol.Defensa) return zonaB.position.x;
            else return ATKTeamB.position.x;
        }

    }
    void resetMatch()
    {
        teamA.gameObject.SetActive(true);
        teamB.gameObject.SetActive(true);
        foreach (HiveMind mind in hiveMinds)
        {
            mind.gameObject.SetActive(true);
        }

        foreach (FootBallPlayer player in TeamAPlayers)
        {
            //se le manda al jugador como bool para aparecer retrasado que el equipo que ha marcado no ha sido el suyo
            player.reset(winner != Team.Nobody && Team.TeamA != winner);
        }
        foreach (FootBallPlayer player in TeamBPlayers)
        {
            player.reset(winner != Team.Nobody && Team.TeamB != winner);
        }
        
        goalKeeperTeamA.reset();
        goalKeeperTeamB.reset();
        ball.reset();
      //usaremos nobody como valor especial para indicar que se resetee el estado del juego en general
        if (winner == Team.Nobody)
        {
            for (int i = 0; i < 2; i++)
            {
                goals[i] = 0;
                teams[i].text = "0";
            }
            WinnerText.text = "";
        }
    }

    /// <summary>
    /// Se reactiva todo lo que se desactivó al ganar y se resetea el juego
    /// </summary>
    public void resetGame()
    {
        teamA.gameObject.SetActive(true);
        teamB.gameObject.SetActive(true);
        foreach (HiveMind hiveMind in hiveMinds)
        {
            if (!hiveMind.gameObject.activeSelf)
                hiveMind.gameObject.SetActive(true);
        }
        winner = Team.Nobody;
        resetMatch();
     }

    public Collider getCampo() { return campo; }

    public Transform getGoalZone(Team team)
    {
        if (team == Team.TeamA)
        {
            return goalZoneA;
        }
        else
        {
            return goalZoneB;
        }
    }



    public Vector3 getBallPosition() { return ball.transform.position; }

    public void setBallOwner(FootBallPlayer player) { ballOwner = player; }
    public FootBallPlayer getBallOwner() { return ballOwner; }
}
