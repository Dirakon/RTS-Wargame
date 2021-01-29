using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Thinker
{
    public Unit father;
    virtual protected void CreateAbilities()
    {

        abilities = new Ability[2];
    } 
    virtual protected void CreatePassives()
    {
        passives = new Ability[0];
    }
    public Thinker(Unit otec)
    {
        father = otec;
        CreateAbilities();
        CreatePassives();
        //STANDART ABILITIES
        //ForceAttack
        abilities[0]=(new Ability(KeyCode.A, AbilityType.Choose, father, forceAttack, 0,AbilitySprite.Attack,0, "Force attack"));
        //ForceHoldFire
        abilities[1] =(new Ability(KeyCode.H, AbilityType.JustActivate, father, forceHoldFire, 0,AbilitySprite.HoldFire,1, "Hold Fire"));

    }
    IEnumerator forceAttack(Ability irl)
    {
        // Debug.Log(1);
        irl.canMoveOn = true;
        //  Debug.Log(father.orders.Count);
        //  Debug.Log(irl.chooseTarget);
        var p = new Order(irl.chooseTarget, OrderType.Attack);
        p.Subscribe(father);
        father.subscribedOrders.Insert(0,p );
        //  Debug.Log(father.orders.Count);
        yield return null;
    }
    ShootType remember = ShootType.NoShoot;
    IEnumerator forceHoldFire(Ability irl)
    {
        irl.canMoveOn = true;
        var v = father.shootType;
        father.shootType = remember;
        father.orderOverwriteST = remember;
        remember = v;
        if (father.shootType == ShootType.NoShoot)
        {
            irl.ChangeSpriteTo(AbilitySprite.Explosion);
        }else
        {
            irl.ChangeSpriteTo(AbilitySprite.HoldFire);
        }
        yield return null;
    }
    protected void GoBuild(Ability irl, string[] tags, int unitType, float buildingRadius)
    {
        var dot = irl.skillShotTarget;
        dot.y = father.transform.position.y;
        if ((father.transform.position - dot).magnitude > buildingRadius)
        {
            if (father.moveSpeed != 0.0f)
            {
                //Надо приказать подъехать ближе
                Order o1 = new Order(irl.id, irl.skillShotTarget), o2 = new Order(irl.skillShotTarget);
                o1.Subscribe(father);
                o2.Subscribe(father);
                father.subscribedOrders.Insert(0, o1);
                father.subscribedOrders.Insert(0, o2);
            }
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(irl.skillShotTarget.x, GameMaster.singleton.transform.position.y, irl.skillShotTarget.z), Physics.gravity.normalized, out hit))
            {
                bool found = false;
                foreach (var p in tags)
                {
                    if (hit.collider.CompareTag(p))
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    //Да
                    Team.teamOverride = father.team.num;
                    GameObject.Instantiate(GameMaster.singleton.unitPrefabs[unitType]).transform.position = hit.point;
                }
            }
        }
    }
    protected void GoBuild(int unitType, int holeNum) //На шут поинте
    {
        Team.teamOverride = father.team.num;
        GameObject.Instantiate(GameMaster.singleton.unitPrefabs[unitType], father.neccecaryHoles[holeNum].transform.position, father.neccecaryHoles[holeNum].transform.rotation);
    }
    public static Thinker Get(int thinkerName, Unit father)
    {
        switch (thinkerName)
        {
            default:
                return new Thinker(father);
            case 0:
                return new FirstUnit(father);
            case 1:
                return new Spawner(father);
            case 2:
                return new MissileGuy(father);
            case 3:
                return new Builder(father);
            case 4:
                return new GroundFactory(father);
            case 5:
                return new Airport(father);
            case 6:
                return new Navalyard(father);
            case 7:
                return new Bike(father);
            case 8:
                return new Mine(father);
        }
    }

    public Ability[] abilities;
    public Ability[] passives;

}

//public class EmptyBoy

public class FirstUnit : Thinker
{

    IEnumerator speedBoost(Ability irl)
    {
        irl.canMoveOn = true;
        father.mover.SetSpeed(father.mover.GetSpeed() + 1000f);
        yield return new WaitForSeconds(3f);
        father.mover.SetSpeed(father.mover.GetSpeed() - 1000f);
    }
    public FirstUnit(Unit father) : base(father)
    {
    }
    protected override void CreateAbilities()
    {
        abilities = new Ability[3];
        abilities[2]= new Ability(KeyCode.F, AbilityType.JustActivate, father, speedBoost, 5f, AbilitySprite.Speed,2, "Speed Boost","Increase speed by 1000 for 3 seconds.");
    }
}

public class Spawner : Thinker
{
    public Spawner(Unit father) : base(father)
    {
    }
    protected override void CreateAbilities()
    {
        abilities = new Ability[4];
        abilities[2] = new Ability(KeyCode.R, AbilityType.Choose, father, Kill, 0, AbilitySprite.Stop,2, "Kill", "Immediatly kill a unit.");
        abilities[3]= new Ability(KeyCode.F, AbilityType.JustActivate, father, ChangeShrunkMode, 0, AbilitySprite.LazerMode,3, "Change Mode", "Changes lazer mode (shrink, unshrink).");
    }
    protected override void CreatePassives()
    {
        passives = new Ability[1];
        passives[0] = new Ability(KeyCode.LeftWindows, AbilityType.Passive, father, FirstPassive, 0, AbilitySprite.LazerMode, 0, "Passive");
    }
    IEnumerator Kill(Ability irl)
    {
        irl.canMoveOn = true;
        GameObject.Destroy(irl.chooseTarget.gameObject);
        yield return null;
    }

    IEnumerator ChangeShrunkMode(Ability irl)
    {
        irl.canMoveOn = true;
        mode *= -1;
        if (father.gun[0].lazer != null)
            father.gun[0].lazer[0].ChangeMaterial();
        yield return null;
    }
    const float shrunkK = 0.1f;
    int mode = -1;
    IEnumerator FirstPassive(Ability irl)
    {
        while (true)
        {
            if (father.gun[0].doIShoot && father.shootType == ShootType.LazerShoot && father.target != null)
            {
                father.target.transform.localScale += mode * new Vector3(1, 1, 1) * shrunkK * Time.deltaTime;
                if (father.target.transform.localScale.x <= 0)
                {
                    GameObject.Destroy(father.target.gameObject);
                }
            }
            yield return null;
        }
    }
}

public class MissileGuy : Thinker
{

    IEnumerator MissileOverload(Ability irl)
    {
        irl.canMoveOn = true;
        father.shootAtOnce = true;
        father.shootRate = 0.5f;
        yield return new WaitForSeconds(3f);
        father.shootAtOnce = false;
        father.shootRate = 1f;
    }
    public MissileGuy(Unit father) : base(father)
    {
    }
    protected override void CreateAbilities()
    {
        abilities = new Ability[3];
        abilities[2] = new Ability(KeyCode.F, AbilityType.JustActivate, father, MissileOverload, 10f, AbilitySprite.MissileSwarm, 2, "Missile Swarm", "Increase shoot rate massively for 3 seconds.");
    }
}
public class Builder : Thinker
{
    //Строит 0 - наземную фабрику, 1 - корабельную фабрику, 2 - воздушную фабрику.
    IEnumerator GroundFact(Ability irl)
    {//0
        irl.canMoveOn = true;
        GoBuild(irl, new string[1] { "Ground" }, 0, father.stoppingDistance + 1f);
        yield return null;
    }
    IEnumerator AirFact(Ability irl)
    {//2
        irl.canMoveOn = true;
        GoBuild(irl, new string[2] { "Water", "Ground" }, 2, father.stoppingDistance + 1f);
        yield return null;
    }
    IEnumerator WaterFact(Ability irl)
    {//1
        irl.canMoveOn = true;
        GoBuild(irl, new string[1] { "Water" }, 1, father.stoppingDistance + 1f);
        yield return null;
    }
    public Builder(Unit father) : base(father)
    {
    }
    protected override void CreateAbilities()
    {
        abilities = new Ability[5];
        abilities[2] = (new Ability(KeyCode.G, AbilityType.SkillShot, father, GroundFact, 0, AbilitySprite.Ground, 2, "Ground Factory"));
        abilities[3] = (new Ability(KeyCode.N, AbilityType.SkillShot, father, WaterFact, 0, AbilitySprite.Ship, 3, "Navalyard"));
        abilities[4] = (new Ability(KeyCode.P, AbilityType.SkillShot, father, AirFact, 0, AbilitySprite.Plane, 4,"Airport"));
    }
}
public class GroundFactory : Thinker
{
    IEnumerator MachineGun(Ability irl)
    {//3
        irl.canMoveOn = true;
        GoBuild(3, 0);
        yield return null;
    }
    IEnumerator Artillery(Ability irl)
    {//9
        irl.canMoveOn = true;
        GoBuild(9, 0);
        yield return null;
    }
    IEnumerator Rocketeer(Ability irl)
    {//4
        irl.canMoveOn = true;
        GoBuild(4,0);
        yield return null;
    }
    IEnumerator Freezer(Ability irl)
    {//5
        irl.canMoveOn = true;
        GoBuild(5, 0);
        yield return null;
    }
    public GroundFactory(Unit father) : base(father)
    {
    }
    protected override void CreateAbilities()
    {
        abilities = new Ability[6];
        abilities[2] = (new Ability(KeyCode.R, AbilityType.JustActivate, father, Rocketeer, 0, AbilitySprite.Ground, 2, "Rocketeer"));
        abilities[3] = (new Ability(KeyCode.F, AbilityType.JustActivate, father, Freezer, 0, AbilitySprite.Ground, 3, "Freezer"));
        abilities[4] = (new Ability(KeyCode.M, AbilityType.JustActivate, father, MachineGun, 0, AbilitySprite.Ground, 4, "Machine Gunner"));
        abilities[5] = (new Ability(KeyCode.Y, AbilityType.JustActivate, father, Artillery, 0, AbilitySprite.Ground, 5, "Artillery"));
    }
}

public class Airport : Thinker
{

    IEnumerator WorkerDrone(Ability irl)
    {//7
        
        irl.canMoveOn = true;
        GoBuild(7, 0);
        yield return null;
    }
    IEnumerator AvaPlane(Ability irl)
    {//8

        irl.canMoveOn = true;
        GoBuild(8, 0);
        yield return null;
    }
    public Airport(Unit father) : base(father)
    {
    }
    protected override void CreateAbilities()
    {
        abilities = new Ability[4];
        abilities[2] = (new Ability(KeyCode.P, AbilityType.JustActivate, father, AvaPlane, 0, AbilitySprite.Plane, 2,"Rocket Plane"));
        abilities[3] = (new Ability(KeyCode.W, AbilityType.JustActivate, father, WorkerDrone, 0, AbilitySprite.Plane, 3, "Worker Drone"));
    }
}

public class Navalyard : Thinker
{

    IEnumerator BigShip(Ability irl)
    {//6
        //BIGSHIP!
        irl.canMoveOn = true;
        GoBuild(6, 0);
        yield return null;
    }
    public Navalyard(Unit father) : base(father)
    {
    }
    protected override void CreateAbilities()
    {
        abilities = new Ability[3];
        abilities[2] = (new Ability(KeyCode.B, AbilityType.JustActivate, father, BigShip, 0, AbilitySprite.Ship, 2, "Big Ship"));
    }
}
public class Bike : Thinker
{

    IEnumerator Mine(Ability irl)
    {//6
        //BIGSHIP!
        irl.canMoveOn = true;
        GoBuild(10, 0);
        yield return null;
    }
    public Bike(Unit father) : base(father)
    {
    }
    protected override void CreateAbilities()
    {
        abilities = new Ability[3];
        abilities[2] = (new Ability(KeyCode.M, AbilityType.JustActivate, father, Mine, 0, AbilitySprite.Explosion, 2, "Plant Mine"));
    }
}



public class Mine : Thinker
{
    public Mine(Unit father) : base(father)
    {
    }
    protected override void CreatePassives()
    {
        passives = new Ability[1];
        passives[0] = new Ability(KeyCode.LeftWindows, AbilityType.Passive, father, FirstPassive, 0, AbilitySprite.Explosion, 0, "Me Explode");
    }
    const float shrunkK = 0.1f;
    int mode = -1;
    IEnumerator FirstPassive(Ability irl)
    {
        while (true)
        {
            if (father.ds.closest != null)
            {
                father.ds.closest.GetHarm(100);
                father.GetHarm(father.hp);
            }
            yield return null;
        }
    }
}