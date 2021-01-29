using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        bc = GetComponent<Collider>();
        bc.enabled = true;
    }
    public Team myTeam;

    public List<Unit> enemiesInSight = new List<Unit>();
    public List<Unit> friendsInSight = new List<Unit>();
    public Detector ds;
    // Update is called once per frame

    private void OnTriggerEnter(Collider other)
    {
        var d = other.GetComponent<Unit>();
        if (myTeam == null)
        {

        }
        if (d != null)
        {
            if (myTeam.IsMyEnemy(d.team))
                enemiesInSight.Add(d);
            else
                friendsInSight.Add(d);

            ds.AddUnit(d);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        var d = other.GetComponent<Unit>();
        if (d != null)
        {
            if (myTeam.IsMyEnemy(d.team))
                enemiesInSight.Remove(d);
            else
                friendsInSight.Remove(d);
            ds.DeleteUnit(d);
        }
    }
    public Collider bc;
    void Update()
    {

    }
}
