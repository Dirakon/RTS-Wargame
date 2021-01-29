using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    // Start is called before the first frame update

    private void Awake()
    {
    }
    void Start()
    {

      //  foreach (var p in zoneOfSight)
       // {
            zoneOfSight.myTeam = myTeam;
        zoneOfSight.ds = this;
       // }
    }
    public void AddUnit(Unit unit)
    {
        if (onUnitAdded != null)
        {
            onUnitAdded.Invoke(this, new unitSmthArgs(unit));
        }
    }
    public class unitSmthArgs : EventArgs
    {
        public Unit unit;
        public unitSmthArgs(Unit unit)
        {
            this.unit = unit;
        }
    }
    public void DeleteUnit(Unit unit)
    {
        if (onUnitDeleted != null)
        {
            onUnitDeleted.Invoke(this, new unitSmthArgs(unit));
        }
    }
    public event EventHandler onUnitAdded;
    public event EventHandler onUnitDeleted;
    public Team myTeam;
    public Unit closest = null;
    public Zone zoneOfSight;
    // Update is called once per frame
    void Update()
    {
        closest = null;

       // foreach (var p in zoneOfSight)
       // {
            List<Unit> dead = new List<Unit>();
            foreach (var obj in zoneOfSight.enemiesInSight)
            {
                if (obj == null)
                {
                    dead.Add(obj);
                    continue;
                }
                if (closest == null || (obj.transform.position - transform.position).magnitude > closest.transform.position.magnitude)
                {
                    closest = obj;
                }
        }
        foreach (var d in dead)
        {
            zoneOfSight.enemiesInSight.Remove(d);
        }
        dead.Clear();
        foreach (var obj in zoneOfSight.friendsInSight)
        {
            if (obj == null)
            {
                dead.Add(obj);
                continue;
            }
        }
        foreach (var d in dead)
        {
            zoneOfSight.friendsInSight.Remove(d);
        }
        dead.Clear();
        //}
    }
}