using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum OrderType
{
    Move,
    UseAbility,
    Attack,
    Chase
}

public class Order
{
    public int abilityId;
    public Unit unitTarget;
    public Vector3 positionTarget;
    public OrderThing thing;
   public void Subscribe(Unit unit)
    {
        if (thing != null)
        {
            thing.OnSubscribe(unit);
        }
    }
    public void Unsubscribe(Unit unit)
    {
        if (thing != null)
        {
            thing.OnUnsubscribe(unit);
        }
    }
    public Order(int abilityCode, Unit choose = null)//Ability
    {
        type = OrderType.UseAbility;
        this.abilityId = abilityCode;
        unitTarget = choose;
    }
    public Order(int abilityCode, Vector3 shot )//Ability
    {
        type = OrderType.UseAbility;
        this.abilityId = abilityCode;
        positionTarget = shot;

    }
    public OrderType type;
    public Transform target;
    public Order(Vector3 b)//Move
    {
        thing = GameObject.Instantiate(GameMaster.singleton.orderThingPrefab).GetComponent<OrderThing>();
        thing.transform.position = b;
        type = OrderType.Move;
        positionTarget = b;
    }
    public Order( Unit tar,OrderType ot)//Attack and chase
    {
        type = ot;
        unitTarget = tar;
    }
}