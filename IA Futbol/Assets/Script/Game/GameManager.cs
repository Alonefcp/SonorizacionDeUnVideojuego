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

    //Fmod
    private FMOD.Studio.EventInstance instance;

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
        instance = FMODUnity.RuntimeManager.CreateInstance("event:/Pitido");
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));       
        instance.start();
        instance.release();

        //Al marcar la bola no está en el aire
        ball.setBallOnAir(false);

        uint teamN = (uint)team;
        //Se castea el equipo al que han marcado a entero y se le suma gol
        goals[teamN]++;
        teams[teamN].text = goals[teamN].ToString();
        //Si a uno de los equipos le han marcado el máximo de goles ha ganado el contrario. Se desactivan los jugadores y las IA multicapa
        if (goals[teamN] >= MaxGoals)
        {
            WinnerText.text = "Gana " + names[1 - teamN];
            teamA.gameObject.SetActive(false);
            teamB.gameObject.SetActive(false);
            foreach (HiveMind mind in hiveMinds)
            {
                mind.gameObject.SetActive(false);
                mind.gameObject.SetActive(false);
            }
            
        }
        //en caso de seguir el juego se resetean las posiciones y estado de los agentes y la bola
        else
            resetGame(team);
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
    void resetGame(Team t)
    {
        foreach (FootBallPlayer player in TeamAPlayers)
        {
            //se le manda al jugador como bool para aparecer retrasado que el equipo que ha marcado no ha sido el suyo
            player.reset(t != Team.Nobody && Team.TeamA != t);
        }
        foreach (FootBallPlayer player in TeamBPlayers)
        {
            player.reset(t != Team.Nobody && Team.TeamB != t);
        }
        
        goalKeeperTeamA.reset();
        goalKeeperTeamB.reset();
        ball.reset();
      //usaremos nobody como valor especial para indicar que se resetee el estado del juego en general
        if (t == Team.Nobody)
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
        resetGame(Team.Nobody);
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
