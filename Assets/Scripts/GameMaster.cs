using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public float moveSpeed, scroolSpeed;
    public static GameMaster singleton;
    public List<Unit> selectedUnits;
    public List<Unit>[] binds = new List<Unit>[9];
    public List<Unit> representative = new List<Unit>();
    [HideInInspector] public int repType = 0;
    [HideInInspector] public int pendingSkillshot = -1, pendingChoose = -1;
    private void Awake()
    {
        singleton = this;
        for (int i = 0; i < binds.Length; ++i)
        {
            binds[i] = new List<Unit>();
        }
    }
    public GameObject hpBarPrefab;
    public GameObject AOEFieldPrefab;
    public GameObject[] unitPrefabs;
    public void SetNewMinCDToAbility(int abilityId)
    {
        Ability minCd = representative[0].thinker.abilities[abilityId];
        for (int i = 1; i < representative.Count; ++i)
        {
            if (representative[i].thinker.abilities[abilityId].currentReloadProcess < minCd.currentReloadProcess)
            {
                minCd = representative[i].thinker.abilities[abilityId];
            }
        }
        AbilityPanel.singleton.SetAbility(minCd);
    }
    Vector2 startScreenPosition;
    public void FindNewRep()
    {
        //Ищем новый тип юнита
        int dif = 9999;
        foreach (var p in selectedUnits)
        {
            if (p.unitType > repType && p.unitType - repType < dif)
            {
                dif = p.unitType - repType;
            }
        }
        if (dif == 9999)
        {
            repType = -1;
            FindNewRep();
        }
        else
        {
            repType += dif;
            AbilityPanel.singleton.ClearAbilities();
            representative.Clear();
            foreach (var p in selectedUnits)
            {
                if (p.unitType == repType)
                {
                    representative.Add(p);
                }
            }
            if (representative.Count != 0)
            {
                for (int i = 0; i < representative[0].thinker.abilities.Length; ++i)
                {
                    SetNewMinCDToAbility(i);
                }
            }
        }
    }
    public GameObject tilePrefab;
    public GameObject LUtile, LDtile, RUtile, RDtile;
    public GameObject orderLinePrefab;
    public GameObject orderThingPrefab;
    public float minX, maxX, minZ, maxZ;
    public List<int> teamsToSee;
    public Image minimap;
    public GameObject zosPrefab;
    public Vector3 calculateTilePosition(GameObject obj)
    {/*
        var x = (obj.transform.position.x - minX) / (maxX-minX) * (RUtile.transform.position - LUtile.transform.position);
        var z = (obj.transform.position.z - minZ) / (maxZ - minZ) * (LDtile.transform.position - LUtile.transform.position);
        return LDtile.transform.position + (x - z);*/
     //   var rastFromLU = (hit.point - LUtile.transform.position).magnitude / (RUtile.transform.position - LDtile.transform.position).magnitude;
     //   Debug.Log(rastFromLU);
     //  var dirFromLU = (hit.point - LUtile.transform.position).normalized;
        var vecRealLU = new Vector3(minX, obj.transform.position.y, maxZ);
        var vecRealRD = new Vector3(maxX, obj.transform.position.y, minZ);

        //  vecRealLU += dirFromLU * (rastFromLU * (vecRealLU - vecRealRD).magnitude);
        //   transform.position = new Vector3(vecRealLU.x, transform.position.y, vecRealLU.z);
        var rastFromLU = (obj.transform.position - vecRealLU).magnitude / (vecRealLU - vecRealRD).magnitude;
        var dirFromLU = (obj.transform.position - vecRealLU).normalized;
        dirFromLU.y = dirFromLU.z;
        dirFromLU.z = 0;
        float xSize, zSize;
        xSize = (GameMaster.singleton.maxX - GameMaster.singleton.minX);
        zSize = (GameMaster.singleton.maxZ - GameMaster.singleton.minZ);
        var ans = LUtile.transform.position;
        //  Vector3 mapLU=, mapRD=;
        ans += dirFromLU * (rastFromLU * (LUtile.transform.position - RDtile.transform.position).magnitude);
        return ans;
    }
    public Vector3 GetTileScale(Vector2 scaleInUnits)
    {
        float xSize, zSize;
        Rect rect = GameMaster.singleton.minimap.rectTransform.rect;
        xSize = (GameMaster.singleton.maxX - GameMaster.singleton.minX);
        zSize = (GameMaster.singleton.maxZ - GameMaster.singleton.minZ);
        return new Vector3(rect.width * (scaleInUnits.x / xSize) / 10, rect.height * (scaleInUnits.y / zSize) / 10, 1);
    }
    public LayerMask groundOnly;
    public void RedoRepresentative()
    {
        pendingChoose = pendingSkillshot = -1;
        if (selectedUnits.Count == 0)
        {
            AbilityPanel.singleton.ClearAbilities();
            representative.Clear();
        }
        else
        {
            FindNewRep();
        }
    }
    public void ActivateAbilityInReps(Ability ability)
    {
        switch (ability.type)
        {
            case AbilityType.Choose:
                pendingChoose = ability.id;
                break;
            case AbilityType.SkillShot:
                pendingSkillshot = ability.id;
                break;
            case AbilityType.JustActivate:
                var order = new Order(ability.id);
                var arr = representative;
                if (ability.activationKey == KeyCode.H)
                {
                    //Если холдфайр
                    arr = selectedUnits;
                }
                foreach (var unit in arr)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        order.Subscribe(unit);
                        unit.subscribedOrders.Add(order);
                    }
                    else
                    {
                        unit.ChangeAllTheOrdersToThisOne(new Order(ability.id));
                    }
                }
                break;
        }

    }
    void Update()
    {

        //ScroolMove
        transform.position += Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime * transform.right;
        transform.position += Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime * transform.up;
        transform.position += Input.GetAxis("Mouse ScrollWheel") * scroolSpeed * Time.deltaTime * transform.forward;

        if (miniMode)
        {
            Vector3 interestingPos = Input.mousePosition;
            //  RaycastHit hit;
            //     Debug.Log("Mouse: " +Input.GetMouseButton(0).ToString());
            //    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
            //    Debug.Log("Name: " + (hit.collider.name == "Minimap").ToString());
            //      Debug.Log(hit.collider.name);
            if (Input.GetMouseButton(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    Vector3 hit = new Vector3(interestingPos.x, interestingPos.y, LUtile.transform.position.z);
                    var x = (maxX * Vector3.right - minX * Vector3.right);
                    var z = (minZ * Vector3.forward - maxZ * Vector3.forward);

                    var rastFromRD = (hit - RDtile.transform.position).magnitude / (RUtile.transform.position - LDtile.transform.position).magnitude;
                    var dirFromRD = (hit - RDtile.transform.position).normalized;
                    dirFromRD.z = dirFromRD.y;
                    var vecRealLU = new Vector3(minX, 0, maxZ);
                    var vecRealRD = new Vector3(maxX, 0, minZ);
                    //     Gizmos.DrawSphere(vecRealRD, 5);
                    vecRealRD += dirFromRD * (rastFromRD * (vecRealLU - vecRealRD).magnitude);
                    transform.position = new Vector3(vecRealRD.x, transform.position.y, vecRealRD.z);
                }
            }
            else
            {
                miniMode = false;
            }

            //  Debug.Log(Input.mousePosition);
            //Debug.DrawRay(transform.position, hit, Color.red, 20f);
            //Debug.DrawRay(transform.position, hit, Color.red, 20f);
        }

        //Бинды
        for (int i = 0; i < 9; ++i)
        {
            if (Input.GetKeyDown((KeyCode)(i + '1')))
            {
                if (Input.GetKey(KeyCode.CapsLock))
                {
                    foreach (var p in binds[i])
                    {
                        p.groups.Remove(i + 1);
                    }
                    binds[i].Clear();
                    foreach (var p in selectedUnits)
                    {
                        p.groups.Add(i + 1);
                        binds[i].Add(p);
                    }

                }
                else
                {
                    DoSelectStuffWithUnits(binds[i]);
                }
            }
        }

        //Изменить представителя
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            RedoRepresentative();
        }
        //Выделение юнитов или выборки-скилшоты 
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.name == "Minimap")
                {
                    miniMode = true;
                }
                EventSystem.current.SetSelectedGameObject(null);
            }
            else if (pendingSkillshot != -1)
            {
                //Скилшотим!
                RaycastHit hit;
                if (selectedUnits.Count != 0 && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    var order = new Order(pendingSkillshot, hit.point);
                    foreach (var p in representative)
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            order.Subscribe(p);
                            p.subscribedOrders.Add(order);
                        }
                        else
                        {
                            p.ChangeAllTheOrdersToThisOne(order);
                        }
                    }
                    pendingSkillshot = -1;
                }
            }
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {

                    var unit = hit.collider.GetComponent<Unit>();
                    if (unit != null)
                    {
                        if (pendingChoose == -1)
                        {
                            //И выборка не жаждет чего-то
                            List<Unit> list = new List<Unit>();
                            list.Add(unit);
                            GameMaster.singleton.DoSelectStuffWithUnits(list);
                        }
                        else if (selectedUnits.Count != 0)
                        {
                            //Если нажали по юниту
                            var order = new Order(pendingChoose, unit);

                            var arr = representative;
                            if (pendingChoose == 0)
                            {
                                //Если форс атака 
                                arr = selectedUnits;
                            }
                            //И выборка жаждет инфы о попадании
                            foreach (var p in arr)
                            {
                                if (Input.GetKey(KeyCode.LeftShift))
                                {
                                    order.Subscribe(p);
                                    p.subscribedOrders.Add(order);
                                }
                                else
                                {
                                    p.ChangeAllTheOrdersToThisOne(order);
                                }
                            }
                            pendingChoose = -1;
                        }
                    }
                }
            }
        }

        //Приказ о движении
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count != 0)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
            }
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    var fund = hit.collider.GetComponent<Unit>();
                    Order newOrder;
                    if (fund != null && selectedUnits.Count > 0)
                    {
                        if (selectedUnits[0].team.IsMyEnemy(fund.team))
                        {
                            //Если враг, то атака
                            newOrder = new Order(fund, OrderType.Attack);
                        }
                        else
                        {
                            //Если друг, то следование
                            newOrder = new Order(fund, OrderType.Chase);
                        }
                    }
                    else
                    {
                        //Если не юнит, то движение
                        newOrder = new Order(hit.point);
                    }
                    foreach (var unit in selectedUnits)
                    {
                        if (unit == newOrder.unitTarget)
                            continue;
                        //Потом заменить на шифт
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            newOrder.Subscribe(unit);
                            unit.subscribedOrders.Add(newOrder);
                        }
                        else
                        {
                            unit.ChangeAllTheOrdersToThisOne(newOrder);
                        }
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            foreach (var unit in selectedUnits)
            {
                unit.ChangeAllTheOrdersToThisOne(unit.currentOrder);
                unit.currentOrder.Unsubscribe(unit);
                unit.currentOrder = null;
            }
        }

        //Способности представителей
        if (representative.Count != 0)
        {
            foreach (var p in representative[0].thinker.abilities)
            {
                if (Input.GetKeyDown(p.activationKey))
                {
                    ActivateAbilityInReps(p);
                    break;
                }
            }
        }
    }
    private void LateUpdate()
    {
    }
    public bool miniMode = false;
    public GUISkin skin;
    public const float rastNotToHold = 10f;
    bool rectActivated = false;
    private void OnGUI()
    {

        if (miniMode || EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        //Массовое выделение
        if (Input.GetMouseButtonDown(0))
        {
            startScreenPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            if (!rectActivated && (startScreenPosition - (Vector2)Input.mousePosition).magnitude > rastNotToHold)
            {
                rectActivated = true;
            }
            if (rectActivated)
            {
                Rect rect = new Rect(startScreenPosition.x, Screen.height - startScreenPosition.y, -startScreenPosition.x + Input.mousePosition.x, startScreenPosition.y - Input.mousePosition.y);

                GUI.Box(rect, "");
            }
        }
        else if (Input.GetMouseButtonUp(0) && rectActivated)
        {
            rectActivated = false;
            Rect rect = new Rect(startScreenPosition.x, Screen.height - startScreenPosition.y, -startScreenPosition.x + Input.mousePosition.x, startScreenPosition.y - Input.mousePosition.y);
            List<Unit> units = new List<Unit>();
            foreach (var unit in Unit.allUnits)
            {
                var vec = Camera.main.WorldToScreenPoint(unit.transform.position);
                vec.y = Screen.height - vec.y;
                if (rect.Contains(vec, true))
                {
                    units.Add(unit);
                }
            }
            DoSelectStuffWithUnits(units);
        }
    }
    public void DoSelectStuffWithUnits(List<Unit> list)
    {
        OrderThing.HideAll();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            foreach (var unit in list)
            {
                if (unit.isSelected)
                {
                    if (list.Count == 1)
                    {
                        selectedUnits.Remove(unit);
                        unit.metkaOfThis.enabled = false;
                        unit.isSelected = false;
                    }
                }
                else
                {
                    selectedUnits.Add(unit);
                    unit.metkaOfThis.enabled = true;
                    unit.isSelected = true;
                }
            }
        }
        else
        {
            foreach (var unit in selectedUnits)
            {
                unit.metkaOfThis.enabled = false;
                unit.isSelected = false;
            }
            selectedUnits.Clear();
            foreach (var unit in list)
            {
                selectedUnits.Add(unit);
                unit.metkaOfThis.enabled = true;
                unit.isSelected = true;
            }
        }
        RedoRepresentative();
        foreach (var p in selectedUnits)
        {
            if (p.currentOrder != null && p.currentOrder.thing != null)
            {
                p.currentOrder.thing.mr.enabled = true;
            }
            foreach (var ord in p.subscribedOrders)
            {

                if (ord != null && ord.thing != null)
                {
                    ord.thing.mr.enabled = true;
                }
            }
        }
    }
}