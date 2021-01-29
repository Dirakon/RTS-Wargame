using System.Collections.Generic;
using UnityEngine;

public class Team
{
    public int num;
    public List<Team> enemies = new List<Team>();
    public static int teamOverride = -1;
    public bool IsMyEnemy(Team team)
    {
        return IsMyEnemy(team.num);
    }
    public bool IsMyEnemy(int team)
    {
        return team != num;
        /*
        foreach (var p in enemies)
        {
            if (p.num == team)
                return true;
        }
        return false;*/
    }
    private Team(int num)
    {
        this.num = num;
        switch (num)
        {
            case 0:
                color = Color.blue;
                break;
            case 1:
                color = Color.red;
                break;
            case 2:
                color = Color.green;
                break;
            case 3:
                color = Color.yellow;
                break;
            default:
                color = new Color(Random.Range(0,255)/255f, Random.Range(0, 255) / 255f, Random.Range(0, 255) / 255f);
                break;
        }
    }
    public Color color;
    public static Team[] teams = new Team[0];
    public static Team Get(int num)
    {
        if (teamOverride != -1)
        {
            num = teamOverride;
            teamOverride = -1;
        }
        if (num >= teams.Length)
        {
            Team[] newTeams = new Team[num + 1];
            for (int i = 0; i < teams.Length; ++i)
            {
                newTeams[i] = teams[i];
            }
            teams = newTeams;
            teams[num] = new Team(num);
            foreach(var unit in Unit.allUnits)
            {
                var newSeenBy = new int[teams.Length];
                for(int i = 0; i < unit.seenByWhom.Length; ++i)
                {
                    newSeenBy[i] = unit.seenByWhom[i];
                }
                unit.seenByWhom = newSeenBy;
            }
        }
        else if (teams[num] == null)
        {
            teams[num] = new Team(num);
        }
        return teams[num];
    }
}

