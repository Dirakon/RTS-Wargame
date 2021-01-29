using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum MSA_Type
{
    ShootAndMove,
    StopOnShoot,
    ShootAndAimOnlyIfStoped
}
public enum ShootType
{
    NoShoot,
    BulletShoot,
    LazerShoot
}
public enum ProjectileTargetType
{
    NoTargetting,
    PositionTargetting,
    UnitTargetting
}




public class Unit : MonoBehaviour
{
    // Start is called before the first frame update
    [HideInInspector]
    public Mover mover;
    public void FullUnsub()
    {

    }
    private void Awake()
    {
        bcollider = GetComponent<BoxCollider>();
        seenByWhom = new int[Team.teams.Length];
        orderOverwriteST = shootType;
        orderOverwriteMSA = msa;
        thinker = Thinker.Get(unitType, this);
        List<Ability> delete = new List<Ability>();
        foreach (var p in thinker.passives)
        {
            p.Start();
        }
        allUnits.Add(this);
        team = Team.Get(teamInit);
        //   meshAgent = GetComponent<NavMeshAgent>();
        //  meshAgent.speed = moveSpeed;
        //  meshAgent.acceleration = acceleration;
        //  meshAgent.angularSpeed = rotationSpeed;
        mover = GetComponent<Mover>();
        mover.SetSpeed(moveSpeed);
        mover.SetAcceleration(acceleration);
        mover.SetAngularSpeed(rotationSpeed);
        mover.SetStopingDistance(stoppingDistance);
        rb = GetComponent<Rigidbody>();
        ds.myTeam = team;
        FieldOfVision.myTeam = team;
    }
    MeshRenderer hpRend;
    Image tileRend;
    void Start()
    {
        myTile = Instantiate(GameMaster.singleton.tilePrefab, GameMaster.singleton.LUtile.transform);
        zosIcon = Instantiate(GameMaster.singleton.zosPrefab, GameMaster.singleton.RDtile.transform).GetComponent<Image>();
        Vector3 thing = transform.localScale.x * (FieldOfVision.zoneOfSight.transform.localScale);
        var safe = GameMaster.singleton.GetTileScale(new Vector2(thing.x, thing.z));
        zosIcon.transform.localScale = safe;
        FieldOfVision.onUnitAdded += OnUnitSeen;
        FieldOfVision.onUnitDeleted += OnUnitUnseen;
        startHP = hp;
        hpBar = Instantiate(GameMaster.singleton.hpBarPrefab, transform);
        hpBar.transform.position = transform.position + transform.up * 4;
        hpRend = hpBar.GetComponentInChildren<MeshRenderer>();
        tileRend = myTile.GetComponent<Image>();
        tileRend.color = team.color;
        if (isLocalPlayers())
        {
            MakeVisible();
        }
        else
        {
            MakeInvisible();
        }
        orderLine = Instantiate(GameMaster.singleton.orderLinePrefab, transform.position, transform.rotation, transform).GetComponent<LineScript>();
        orderLine.unit = this;

    }
    public bool isLocalPlayers()
    {
        foreach (var p in GameMaster.singleton.teamsToSee)
        {
            if (p == team.num)
            {
                return true;
            }
        }
        return false;
    }
    public void MakeVisible()
    {
        foreach (var p in actualThingsToSee)
        {
            p.enabled = true;
        }
        hpRend.enabled = true;
        tileRend.enabled = true;
        if (isLocalPlayers())
        {
            visVision.enabled = true;
            zosIcon.enabled = true;
        }
    }
    public void MakeInvisible()
    {
        foreach (var p in actualThingsToSee)
        {
            p.enabled = false;
        }
        hpRend.enabled = false;
        tileRend.enabled = false;
        visVision.enabled = false;
        zosIcon.enabled = false;
    }
    public void OnUnitSeen(object sender, EventArgs args)
    {
        var unit = ((Detector.unitSmthArgs)args).unit;
        unit.seenByWhom[team.num]++;
        if (unit.seenByWhom[team.num] == 1 && isLocalPlayers() && !unit.isLocalPlayers())
        {
            unit.MakeVisible();
        }
    }
    public void OnUnitUnseen(object sender, EventArgs args)
    {
        var unit = ((Detector.unitSmthArgs)args).unit;
        unit.seenByWhom[team.num]--;
        if (isLocalPlayers() && !unit.isLocalPlayers())
        {
            bool isUnseen = true;
            foreach (var p in GameMaster.singleton.teamsToSee)
            {
                if (unit.seenByWhom[p] != 0)
                {
                    isUnseen = false;
                    break;
                }
            }
            if (isUnseen)
            {
                unit.MakeInvisible();
            }
        }
    }
    public void GetHarm(float harm)
    {
        hp -= harm;
        ChangHPTo(hp);
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
    //gun
    [Header("Gun settings")]
    public Gun[] gun;
    public GameObject projectilePrefab;
    public float shootRate;
    public float gunAngularSpeed;
    public ShootType shootType;
    public bool gunRotationNeeded;
    public bool shootAtOnce;
    public float eachShotInterval;
    public bool noNeedForYGunRotation;
    public bool rotateBeforeShoot = true;
    public bool ballisticTrajectory;
    public float gravityModifierForBT;

    //bullet
    [Header("Projectile settings")]
    public float projectileSpeed;
    public bool canFriendlyHurt;
    public float projectileDamage;
    public float projectileLifeTime;
    public ProjectileTargetType targetType;
    public float angularSpeedOfProjectiles;
    public GameObject boomPrefab;
    public float AOE;
    public float randomizeBulletAngle;

    //laser
    public float laserDuration;
    public float laserReload;

    //mine
    [Header("Unit settings")]
    public float hp;
    public int teamInit;
    public Team team;
    public List<Order> subscribedOrders = new List<Order>();
    public Order currentOrder = null;
    public float moveSpeed;
    public float stoppingDistance;
    public float rotationSpeed;
    public float acceleration;
    public int unitType;
    public MSA_Type msa;    //Move-Shoot-Aim
    float startHP;
    public GameObject onDeath;
    LineScript orderLine;
    public MeshRenderer[] actualThingsToSee;
    [HideInInspector]
    public BoxCollider bcollider;
    public GameObject[] neccecaryHoles;

    GameObject myTile;
    Image zosIcon;
    [HideInInspector]
    public NavMeshAgent navMeshAgent;
    Rigidbody rb;
    float velStoper = 0.5f;
    public Detector ds;
    public Ability currentAbility = null;
    public Detector FieldOfVision;
    public static List<Unit> allUnits = new List<Unit>();
    public Thinker thinker;
    public int[] seenByWhom;
    public float prefferedHeight = 0f;
    public SpriteRenderer visVision;
    public SpriteRenderer metkaOfThis;

    [HideInInspector]
    public Unit target;
    public ShootType orderOverwriteST;
    public MSA_Type orderOverwriteMSA;
    public void ChangeAllTheOrdersToThisOne(Order order)
    {
        if (currentOrder != null && currentOrder.type == OrderType.UseAbility)
        {
            return; //Потому что способность не прервать
        }
        order.Subscribe(this);
        foreach (var p in subscribedOrders)
        {
            p.Unsubscribe(this);
        }
        if (currentOrder != null)
            currentOrder.Unsubscribe(this);
        subscribedOrders.Clear();
        FixOrderOverwrites();
        hasProceededOrder = false;
        currentOrder = order;
        mover.SetDestination(gameObject.transform.position);
    }
    public void FixOrderOverwrites()
    {
        msa = orderOverwriteMSA;
        shootType = orderOverwriteST;
    }
    public bool hasProceededOrder = false;
    void Update()
    {
        RaycastHit hit;
        Physics.Raycast(new Ray(transform.position, Vector3.down), out hit, Mathf.Infinity, GameMaster.singleton.groundOnly);
        if (mover.agent == null)
            transform.position = new Vector3(transform.position.x, hit.point.y + prefferedHeight + (bcollider.center.y + bcollider.bounds.size.y), transform.position.z);
        visVision.transform.position = new Vector3(transform.position.x, hit.point.y + 0.01f, transform.position.z);
        orderLine.Hide();
        myTile.transform.position = GameMaster.singleton.calculateTilePosition(gameObject);
        zosIcon.transform.position = myTile.transform.position;
        // bool move = true;
        if (msa == MSA_Type.StopOnShoot && (ds.closest != null || target != null) && shootType != ShootType.NoShoot)
        {
            mover.SetIsStopped(true);
        }
        else
        {
            mover.SetIsStopped(false);
        }
        if (ds.closest != null && (msa != MSA_Type.ShootAndAimOnlyIfStoped || mover.GetIsStopped() || mover.getVelocity() == 0 || (currentOrder == null && subscribedOrders.Count == 0)) && shootType != ShootType.NoShoot && ds.closest.seenByWhom[team.num] != 0)
        {
            target = ds.closest;
        }
        else
        {
            target = null;
        }
        //Велосити остановка

        if (rb.velocity.x != 0)
        {
            if (Mathf.Abs(rb.velocity.x) < velStoper)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, rb.velocity.z);
            }
            else
            {
                rb.velocity = new Vector3((Mathf.Abs(rb.velocity.x) - velStoper) * (rb.velocity.x / Mathf.Abs(rb.velocity.x)), rb.velocity.y, rb.velocity.z);
            }
        }
        if (rb.velocity.z != 0)
        {
            if (Mathf.Abs(rb.velocity.z) < velStoper)
            {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0);
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, (Mathf.Abs(rb.velocity.z) - velStoper) * (rb.velocity.z / Mathf.Abs(rb.velocity.z)));
            }
        }
        //Постараемся пополнять приказы по мере выполнения
        if (currentOrder == null && subscribedOrders.Count > 0)
        {
            currentOrder = subscribedOrders[0];
            subscribedOrders.Remove(currentOrder);
            //   meshAgent.st
        }

        //Если приказ есть
        if (currentOrder != null)
        {
            switch (currentOrder.type)
            {
                case OrderType.Move:
                    if (isSelected)
                        orderLine.Appear();
                    var act = (transform.position - currentOrder.positionTarget);
                    act.y = 0;
                    if (act.magnitude < mover.GetStoppingDistance())
                    {
                        currentOrder.Unsubscribe(this);
                        currentOrder = null;
                        hasProceededOrder = false;
                        FixOrderOverwrites();
                    }
                    else if (!hasProceededOrder)
                    {
                        hasProceededOrder = true;
                        orderLine.SecondPosition(currentOrder.thing.transform);
                        mover.SetDestination(currentOrder.positionTarget);
                    }
                    break;
                case OrderType.UseAbility:
                    if (currentAbility == null)
                    {
                        currentAbility = thinker.abilities[currentOrder.abilityId];
                        switch (currentAbility.type)
                        {
                            case AbilityType.Choose:
                                currentAbility.chooseTarget = currentOrder.unitTarget;
                                break;
                            case AbilityType.SkillShot:
                                currentAbility.skillShotTarget = currentOrder.positionTarget;
                                break;
                        }
                        currentAbility.Start();
                    }
                    else
                    {
                        if (currentAbility.canMoveOn)
                        {
                            currentAbility.canMoveOn = false;
                            hasProceededOrder = false;
                            currentOrder.Unsubscribe(this);
                            currentAbility = null;
                            currentOrder = null;
                            FixOrderOverwrites();
                        }
                    }
                    break;
                case OrderType.Attack:
                    if (isSelected)
                        orderLine.Appear();
                    if (!hasProceededOrder && currentOrder.unitTarget != null)
                    {
                        orderLine.SecondPosition(currentOrder.unitTarget.transform);
                        hasProceededOrder = true;
                        shootType = ShootType.NoShoot;
                        msa = MSA_Type.StopOnShoot;
                    }
                    else if (currentOrder.unitTarget == null)
                    {
                        currentOrder.Unsubscribe(this);
                        currentOrder = null;
                        hasProceededOrder = false;
                        FixOrderOverwrites();
                    }
                    else
                    {
                        var found = false;
                        mover.SetDestination(currentOrder.unitTarget.transform.position);
                        List<Unit> arr;
                        if (team.IsMyEnemy(currentOrder.unitTarget.team))
                            arr = ds.zoneOfSight.enemiesInSight;
                        else
                            arr = ds.zoneOfSight.friendsInSight;
                        foreach (var p in arr)
                        {
                            if (p == currentOrder.unitTarget)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            shootType = orderOverwriteST;
                            target = currentOrder.unitTarget;
                        }
                        else
                        {
                            shootType = ShootType.NoShoot;
                            target = null;
                        }
                    }
                    break;
                case OrderType.Chase:
                    if (isSelected)
                        orderLine.Appear();
                    if (!hasProceededOrder)
                    {
                        hasProceededOrder = true;
                        orderLine.SecondPosition(currentOrder.unitTarget.transform);
                    }
                    else
                    {
                        mover.SetDestination(currentOrder.unitTarget.transform.position);
                    }
                    break;
            }
        }
    }
    public void ChangHPTo(float newHP)
    {
        hp = newHP;
        float newScale = hp / startHP;
        hpBar.transform.localScale = new Vector3(newScale * GameMaster.singleton.hpBarPrefab.transform.localScale.x, hpBar.transform.localScale.y, hpBar.transform.localScale.z);
    }
    public GameObject hpBar;
    [HideInInspector]
    public bool isSelected = false;
    [HideInInspector]
    public List<int> groups = new List<int>();
    private void OnDestroy()
    {
        foreach (var p in FieldOfVision.zoneOfSight.friendsInSight)
        {
            OnUnitUnseen(this, new Detector.unitSmthArgs(p));
        }
        foreach (var p in FieldOfVision.zoneOfSight.enemiesInSight)
        {
            OnUnitUnseen(this, new Detector.unitSmthArgs(p));
        }
        FieldOfVision.onUnitDeleted -= OnUnitUnseen;
        FieldOfVision.onUnitAdded -= OnUnitSeen;
        if (isSelected)
        {
            GameMaster.singleton.selectedUnits.Remove(this);
            if (GameMaster.singleton.repType == unitType)
            {
                GameMaster.singleton.representative.Remove(this);
                if (GameMaster.singleton.representative.Count == 0)
                {
                    GameMaster.singleton.RedoRepresentative();
                }
            }
        }
        foreach (var p in groups)
        {
            GameMaster.singleton.binds[p].Remove(this);
        }
        allUnits.Remove(this);
        if (onDeath != null && hp <= 0)
        {
            Instantiate(onDeath, transform.position, transform.rotation);
            Destroy(myTile.gameObject);
            Destroy(zosIcon.gameObject);
        }
        foreach (var p in subscribedOrders)
        {
            p.Unsubscribe(this);
        }
        if (currentOrder != null)
            currentOrder.Unsubscribe(this);
    }
}
