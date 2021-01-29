using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderThing : MonoBehaviour
{
    // Start is called before the first frame update
    public void OnSubscribe(Unit unit)
    {
        mr.enabled = true;
        subs++;
    }
    public static void HideAll()
    {
        foreach(var p in all)
        {
            p.mr.enabled = false;
        }
    }
    private void Awake()
    {
        all.Add(this);
    }
    static List<OrderThing> all = new List<OrderThing>();
    public void OnUnsubscribe(Unit unit)
    {
        //subs.Remove(unit);
        subs--;
        if (subs <= 0)
        {
            all.Remove(this);
            Destroy(gameObject);

        }
    }
    int subs = 0;
   // List<Unit> subs = new List<Unit>();
    void Start()
    {
        
    }
    public Order order;
    public MeshRenderer mr;
    // Update is called once per frame
    void Update()
    {
        
    }
}
