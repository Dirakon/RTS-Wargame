using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (emptyParentY == null)
        {
            emptyParentY = emptyParentXZ;
        }
        if (body.shootType == ShootType.LazerShoot)
        {
            lazer = new Lazer[shootingPlace.Length];
            InstantiateLasers();
            lazer[0].ManageStart(this);
        }
        StartCoroutine(autoShooter());
    }

    // Update is called once per frame

    public void InstantiateLasers()
    {

        for (int i = 0; i < shootingPlace.Length; ++i)
        {
            lazer[i] = Instantiate(body.projectilePrefab, shootingPlace[i].transform.position, shootingPlace[i].transform.rotation, transform).GetComponent<Lazer>();
            lazer[i].gun = this;
        }
    }
    public bool lasersActive = false;
    IEnumerator autoShooter()
    {
        while (true)
        {
            if (doIShoot && body.target != null && (lasersActive || body.shootType != ShootType.LazerShoot))
            {
                if (body.shootType == ShootType.BulletShoot)
                {
                    foreach (var curShoot in shootingPlace)
                    {
                        if (body.target == null)
                            break;
                        var bul = Instantiate(body.projectilePrefab, curShoot.transform.position, Quaternion.RotateTowards(curShoot.transform.rotation, Random.rotation, body.randomizeBulletAngle)).GetComponent<Bullet>();
                        Physics.IgnoreCollision(bul.GetComponent<Collider>(), body.GetComponent<Collider>());
                        bul.damage = body.projectileDamage;
                        if (body.ballisticTrajectory)
                        {
                            var dir = body.target.transform.position - curShoot.transform.position;
                            float y = dir.y;
                            var xy = dir;
                            xy.y = 0;
                            float x = xy.magnitude;
                            float angle = 360-curShoot.transform.eulerAngles.x;
                            Debug.Log(angle);
                            var angleInRads = angle * Mathf.Deg2Rad;
                            float speed = Mathf.Sqrt((Physics.gravity.y*body.gravityModifierForBT*x*x)/(2*(y-Mathf.Tan(angleInRads) * x)*Mathf.Pow(Mathf.Cos(angleInRads),2)));
                            Debug.Log(speed);
                            bul.speed = speed;
                            bul.gravity = -Physics.gravity.y * body.gravityModifierForBT;
                        }
                        else
                        {
                            bul.speed = body.projectileSpeed;
                        }
                        bul.lifeTime = body.projectileLifeTime;
                        bul.canSelfHarm = body.canFriendlyHurt;
                        bul.team = body.team;
                        bul.boomPrefab = body.boomPrefab;
                        bul.AOEradius = body.AOE;
                        if (body.targetType == ProjectileTargetType.UnitTargetting)
                        {
                            bul.target = body.target.transform;
                            bul.angularSpeed = body.angularSpeedOfProjectiles;
                        }
                        else if (body.targetType == ProjectileTargetType.PositionTargetting)
                        {
                            bul.posTarget = body.target.transform.position;
                            bul.posTargeting = true;
                            bul.angularSpeed = body.angularSpeedOfProjectiles;
                        }
                        if (shootingPlace.Length > 1 && !body.shootAtOnce)
                            yield return new WaitForSeconds(body.eachShotInterval);
                    }
                    yield return new WaitForSeconds(body.shootRate);
                }
                else if (body.shootType == ShootType.LazerShoot)
                {   //Размещаем лазер
                    foreach (var p in lazer)
                    {
                        p.Appear();
                        p.transform.localScale *= p.fatherScale == 0 ? 1 : p.fatherScale;
                        p.fatherScale = body.transform.localScale.x;
                        p.transform.localScale /= p.fatherScale;
                        p.SecondPosition(body.target.transform.position);
                        body.target.GetHarm(Time.deltaTime * body.projectileDamage);
                    }
                }
            }
            else if (body.shootType == ShootType.LazerShoot)
            {
                //Убираем лазеры
                foreach (var las in lazer)
                {
                    las.Hide();
                }
            }
            yield return null;
        }
    }
    //public GameObject lastShotProjectile;
    public GameObject[] shootingPlace;
    public Lazer[] lazer = null;
    public bool doIShoot = false;
    void Update()
    {
        targetAcquired = false;
        doIShoot = false;
        Quaternion idealRotation;
        if (body.shootType == ShootType.NoShoot)
        {
            body.target = null;
        }
        if (body.target != null && body.gunRotationNeeded)
        {
            idealRotation = Quaternion.LookRotation((body.target.transform.position - emptyParentY.transform.position).normalized);
        }
        else
        {
            idealRotation = body.transform.rotation;
        }
        var idealRotationXZ = Quaternion.Euler(transform.rotation.x, idealRotation.eulerAngles.y, idealRotation.eulerAngles.z);
        if (body.noNeedForYGunRotation)
        {
            idealRotation = idealRotationXZ;
            var p = idealRotation.eulerAngles;
            p.x = emptyParentY.transform.eulerAngles.x;
            idealRotation.eulerAngles = p;
        }
        else
        {

        }
        var movHow = (idealRotation.eulerAngles - emptyParentY.transform.rotation.eulerAngles);
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
        var move = movHow.normalized * Time.deltaTime * body.gunAngularSpeed;
        if (movHow.magnitude < body.gunAngularSpeed / 100f)
        { //Ставим вручную
            var eul = idealRotation.eulerAngles;
            emptyParentXZ.transform.rotation = idealRotationXZ;
            emptyParentY.transform.rotation = Quaternion.Euler(eul);
            if (body.target != null)
            {
                targetAcquired = true;
            }
        }
        else
        {
            emptyParentXZ.transform.rotation = Quaternion.Euler(emptyParentXZ.transform.rotation.eulerAngles + new Vector3(0, move.y, move.z));
            emptyParentY.transform.rotation = Quaternion.Euler(emptyParentY.transform.rotation.eulerAngles + new Vector3(move.x, 0, 0));
        }
        if (targetAcquired || !body.rotateBeforeShoot)
        {
            if ((body.msa == MSA_Type.ShootAndMove || (body.msa == MSA_Type.StopOnShoot || (body.currentOrder == null && body.subscribedOrders.Count == 0) || body.mover.getVelocity() == 0)))
            {
                doIShoot = true;
            }
        }
    }
    public GameObject emptyParentY, emptyParentXZ;
    public Unit body;
    public bool targetAcquired = false;
}
