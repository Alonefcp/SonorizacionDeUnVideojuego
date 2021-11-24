using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("IAFutbol")]
[TaskDescription("Se la pasa al compañero óptimo (el más cercano a la portería)")]
public class Pass : Action
{
    private FootBallPlayer player;
    public LayerMask PlayersLayer;

    List<FootBallPlayer> team;
    public override void OnAwake()
    {
        player = GetComponent<FootBallPlayer>();
    }

    public override void OnStart()
    {
        team = player.getTeam();
    }

    public override TaskStatus OnUpdate()
    {
        List<FootBallPlayer> canPass = new List<FootBallPlayer>();
        RaycastHit info;
        //Se lanza un raycast a todo el equipo del agente para ver si el primero con el que colisiona es aliado o enemigo
        foreach (FootBallPlayer canPassPlayer in team)
        {
            if (canPassPlayer != player && !canPassPlayer.isInDanger())
            {
                Vector3 direction = canPassPlayer.transform.position - player.transform.position;
                bool collides = Physics.Raycast(player.transform.position, direction.normalized, out info, direction.magnitude, PlayersLayer);
                Debug.DrawRay(player.transform.position, direction, Color.yellow);

                //si no hay nadie en medio o si lo que hay es alguien del equipo del agente lo añadimos a la lista de posibles pases
                if (info.collider != null)
                {
                    Team myTeam = info.collider.GetComponent<FootBallPlayer>().getMyTeam();
                    if (!collides || (collides && myTeam == player.getMyTeam()))
                    {
                        canPass.Add(canPassPlayer);
                    }
                }
            }
        }
        FootBallPlayer furthest = null;
        float targetDistance = float.MaxValue;
        //si no hay posibles pases porque todos están cubiertos se devuelve failure
        if (canPass.Count == 0) return TaskStatus.Failure;

        //en caso contrario se pasa al más cercano a la portería contraria y por tanto el más adelantado
        foreach (FootBallPlayer footBallPlayer in canPass)
        {
            float distance = (footBallPlayer.getGoalZone().transform.position - footBallPlayer.transform.position).magnitude;
            if (distance < targetDistance)
            {
                furthest = footBallPlayer;
                targetDistance = distance;
            }
        }
        player.Pass(furthest);
        return TaskStatus.Success;
    }
}
