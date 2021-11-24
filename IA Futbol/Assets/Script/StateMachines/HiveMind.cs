using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Ludiq;
using UnityEngine.UI;
public class HiveMind : MonoBehaviour
{
    Vector3[] posDefensas;
    [SerializeField] Text metric;
    Vector3[] posDelanteros;

    private void Update()
    {

    }

    public void init()
    {
        //Recogemos las variables necesarias
        Team MyTeam = Variables.Object(this.gameObject).Get("MyTeam").ConvertTo<Team>();
        Team EnemyTeam = Variables.Object(this.gameObject).Get("EnemyTeam").ConvertTo<Team>();
        List<FootBallPlayer> myTeam = GameManager.getInstance().getTeam(MyTeam);
        List<FootBallPlayer> enemyTeam = GameManager.getInstance().getTeam(EnemyTeam);
        Variables.Object(this.gameObject).Set("MyPlayers", myTeam);
        Variables.Object(this.gameObject).Set("EnemyPlayers", enemyTeam);

        List<FootBallPlayer> delanteros = new List<FootBallPlayer>();
        List<FootBallPlayer> centros = new List<FootBallPlayer>();
        List<FootBallPlayer> defensas = new List<FootBallPlayer>();

        foreach (FootBallPlayer player in myTeam)
        {
            if (player.getMyRol() == Rol.Delantero) delanteros.Add(player);
            else if (player.getMyRol() == Rol.Centro) centros.Add(player);
            else defensas.Add(player);
        }

        Variables.Object(this.gameObject).Set("Delanteros", delanteros);
        Variables.Object(this.gameObject).Set("Centros", centros);
        Variables.Object(this.gameObject).Set("Defensas", defensas);

        //Calculamos las posiciones que ocuparán los defensas durante el ataque, ya que serán siempre las mismas, por lo que 
        //no será necesario recalcularlas siempre
        if (defensas.Count != 0)
        {
            posDefensas = new Vector3[defensas.Count];
            float limitAttack = defensas[0].getLimitAttack();
            float goalX = GameManager.getInstance().getGoalZone(MyTeam).position.x;
            //La posición en x de los defensas es el punto medio entre la portería y su zona de ataque
            float defX = (limitAttack + goalX) / 2.0f;
            Collider campo = GameManager.getInstance().getCampo();
            //Repartimos a los defensas a lo largo del eje Z del campo para cubrir el máximo área posible
            float diffY = (campo.bounds.extents.z * 2) / (defensas.Count + 1);
            float campoMaxZ = campo.bounds.center.z + GameManager.getInstance().getCampo().bounds.extents.z;
            for (int i = 1; i <= defensas.Count; i++)
            {
                posDefensas[i - 1] = new Vector3(defX, defensas[0].transform.position.y, campoMaxZ - (i * diffY));
            }
            //Si hay más de dos defensas adelantamos al primero y al último para cubrir un poco más de zona y cubrir la parte delantera de la 
            //defensa
            if (defensas.Count >= 2)
            {
                posDefensas[0].x = defensas[0].getLimitAttack();
                posDefensas[defensas.Count - 1].x = defensas[0].getLimitAttack();
            }
        }

        //Hacemos lo mismo con los delanteros pero estas posiciones se usarán durante la etapa de defensa para cubrir la zona delantera con 
        //el fin de estar preparados para el contraataque
        if (delanteros.Count != 0)
        {
            posDelanteros = new Vector3[delanteros.Count];
            float limitDefense = delanteros[0].getLimitDefense();
            Collider campo = GameManager.getInstance().getCampo();
            //Separacion entre delanteros
            float diffY = (campo.bounds.extents.z * 2) / (delanteros.Count + 1);
            float campoMaxZ = campo.bounds.center.z + GameManager.getInstance().getCampo().bounds.extents.z;
            for (int i = 1; i <= delanteros.Count; i++)
            {
                posDelanteros[i - 1] = new Vector3(limitDefense, delanteros[0].transform.position.y, campoMaxZ - (i * diffY));
            }
        }
    }

    public FootBallPlayer NearestFootBallPlayer(Transform target, List<FootBallPlayer> team)
    {
        float targetDistance = float.MaxValue;
        FootBallPlayer nearest = null;
        foreach (FootBallPlayer player in team)
        {
            float distance = (target.position - player.transform.position).magnitude;
            if (distance < targetDistance)
            {
                targetDistance = distance;
                nearest = player;
            }
        }
        return nearest;
    }

    public bool hasPossession(List<FootBallPlayer> team)
    {

        bool hasPossession = false;
        int i = 0;
        while (i < team.Count && !hasPossession)
        {
            if (team[i].getHasBall())
                hasPossession = true;
            i++;
        }
        return hasPossession;
    }
    public bool NoPossession(List<FootBallPlayer> teamA, List<FootBallPlayer> teamB)
    {
        bool nopossession = !hasPossession(teamA) && !hasPossession(teamB);
        if (nopossession && metric.text != "SinBalon")
            metric.text = "SinBalon";
        return nopossession;
    }

    public void attack(List<FootBallPlayer> delanteros, List<FootBallPlayer> centros, List<FootBallPlayer> defensas)
    {
        if (metric.text != "Ataque")
            metric.text = "Ataque";
        //Defensas
        //Distribuimos los defensas repartiendolos por su zona, cubriendo el mayor area posible preparados para defender
        for (int i = 0; i < defensas.Count; i++)
        {
            if (!defensas[i].getHasBall())
            {
                defensas[i].goTo(posDefensas[i]);
            }
        }
        int numCentrosSinPelota = 0;
        foreach (FootBallPlayer centro in centros)
        {
            if (!centro.getHasBall())
            {
                numCentrosSinPelota++;
            }
        }
        //la mitad de los centros adoptarán su posición defensiva (el centro del campo) para responder rápidamente en caso de que la posesión cambie
        Collider campo = GameManager.getInstance().getCampo();
        float diffY = (campo.bounds.extents.z * 2) / ((numCentrosSinPelota / 2) + 1);
        float campoMaxZ = campo.bounds.center.z + GameManager.getInstance().getCampo().bounds.extents.z;

        for (int i = 1; i <= numCentrosSinPelota / 2; i++)
        {
            if (!centros[i - 1].getHasBall())
                centros[i - 1].goTo(new Vector3(centros[0].getLimitDefense(), centros[i - 1].transform.position.y, campoMaxZ - (i * diffY)));
        }
        //El resto de centros se desmarcará con el fin de ayudar al delantero o al centro que tenga la pelota a subirla
        for (int i = numCentrosSinPelota / 2; i < centros.Count; i++)
        {
            if (!centros[i].getHasBall())
                centros[i].spread();
        }
        //Los delanteros se desmarcarán para poder tener un tiro claro
        foreach (FootBallPlayer delantero in delanteros)
        {
            if (!delantero.getHasBall())
                delantero.spread();
        }

    }

    public void defend(List<FootBallPlayer> delanteros, List<FootBallPlayer> centros, List<FootBallPlayer> defensas, List<FootBallPlayer> enemyPlayers)
    {
        if (metric.text != "Defensa")
            metric.text = "Defensa";
        //Los delanteros esperan al contraataque en su zona
        for (int i = 0; i < delanteros.Count; i++) delanteros[i].goTo(posDelanteros[i]);

        //La mitad de los centros esperan tambíen al contraataque
        for (int i = 0; i < centros.Count / 2; i++)
        {
            centros[i].goTo(new Vector3(centros[0].getLimitDefense(), centros[i].transform.position.y, centros[i].transform.position.z));
        }

        //El resto de jugadores irán a cubrir a los enemigos más cercanos a ellos, excepto el que esté más cerca del enemigo con el
        //balón, que se dirigirá a él para robarlo
        List<FootBallPlayer> otherPlayers = new List<FootBallPlayer>();
        for (int i = centros.Count / 2; i < centros.Count; i++) otherPlayers.Add(centros[i]);
        foreach (FootBallPlayer defensa in defensas) otherPlayers.Add(defensa);

        //Se busca al más cercano al enemigo con el balón y se le saca de la lista de jugadores que van a cubrir
        FootBallPlayer owner = GameManager.getInstance().getBallOwner();
        if (owner != null)
        {
            float minDist;
            Vector3 ballPos = GameManager.getInstance().getBallPosition();

            minDist = float.MaxValue;
            FootBallPlayer closest = null;
            foreach (FootBallPlayer player in otherPlayers)
            {
                float dist = Vector3.Distance(ballPos, player.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = player;
                }
            }
            otherPlayers.Remove(closest);
            //Se le ordena ir a por el balón
            closest.goTo(ballPos);
        }

        //El resto van a por el jugador mas cercano sin balon y lo cubren
        List<FootBallPlayer> enemies = new List<FootBallPlayer>(enemyPlayers);
        if (owner != null) enemies.Remove(owner);

        for (int i = 0; i < otherPlayers.Count; i++)
        {
            float min = float.MaxValue;
            FootBallPlayer enemyToCover = null;
            for (int j = 0; j < enemies.Count; j++)
            {
                float distance = Vector3.Distance(enemies[j].transform.position, otherPlayers[i].transform.position);
                if (distance < min)
                {
                    min = distance;
                    enemyToCover = enemies[j];
                }
            }
            otherPlayers[i].coverPlayer(new Vector3(enemyToCover.transform.position.x, enemyToCover.transform.position.y, enemyToCover.transform.position.z));
            enemies.Remove(enemyToCover);
        }
    }
}
