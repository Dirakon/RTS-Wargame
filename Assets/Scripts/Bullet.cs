using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
        StartCoroutine(ImmenentDemise());

    }
    IEnumerator ImmenentDemise()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
    // Update is called once per frame
    public bool canSelfHarm;
    public float speed;
    public Team team;
    public float lifeTime;
    public float damage;
    public Transform target;
    public Vector3 posTarget;
    public float gravity;
    public bool posTargeting = false;
    Rigidbody rb;
    private void OnCollisionEnter(Collision collision)

    {

        var unit = collision.gameObject.GetComponent<Unit>();

        var cont = collision.GetContact(0).point;
        if (AOEradius != 0)
        {
            var aoe = Instantiate(GameMaster.singleton.AOEFieldPrefab, cont, new Quaternion());
            aoe.GetComponent<AoeField>().CalculateAOEDamage(AOEradius, damage, canSelfHarm ? null : team);
        }
        if (boomPrefab != null)
        {
            var v = Instantiate(boomPrefab);
            v.transform.position = cont;
            GameMaster.singleton.StartCoroutine(dontForgetToKillIt(v));
        }
        if (unit != null && AOEradius == 0 &&(canSelfHarm || team.IsMyEnemy(unit.team)))
        {
            unit.GetHarm(damage);
        }
        Destroy(gameObject);
    }
    IEnumerator dontForgetToKillIt(GameObject what)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(what);
    }
    public float AOEradius;
    public float angularSpeed;
    public GameObject boomPrefab;
    private void FixedUpdate()
    {
        if (gravity != 0)
        {
            rb.AddForce(0, -gravity, 0, ForceMode.Acceleration);
        }
    }
    void Update()
    {
        if (target != null)
        {
            posTarget = target.transform.position;
        }
        if (posTargeting || target)
        {
            var idealRotation = Quaternion.LookRotation((posTarget - transform.position).normalized);
            var movHow = (idealRotation.eulerAngles - transform.rotation.eulerAngles);
            for (int i = 0; i < 3; ++i)
            {
                movHow[i] = movHow[i] % 360;
                if (movHow[i] < 0)
                    movHow[i] = 360 + movHow[i];

                if (movHow[i] > 180)
                {
                    movHow[i] -= 360;
                }
            }
            var move = movHow.normalized * Time.deltaTime * angularSpeed;
            //if (movHow.magnitude <= move.magnitude)
            if (movHow.magnitude < angularSpeed / 100f)
            {
                transform.rotation = idealRotation;
            }
            else
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + move);
            }
            rb.velocity = transform.forward * speed;
        }
    }
}
