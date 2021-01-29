using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoeField : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!p)
            p = true;
        else
            Destroy(gameObject);
    }
    bool p = false;
    Team team;
    float damage;
    public void CalculateAOEDamage(float radius, float damage, Team v)
    {
        transform.localScale *= radius;
        team = v;
        this.damage = damage;
        GetComponent<SphereCollider>().enabled = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        var v = other.GetComponent<Unit>();
        if (v != null && (team == null || team.IsMyEnemy(v.team)))
        {
            v.GetHarm(damage);
        }
    }
}
