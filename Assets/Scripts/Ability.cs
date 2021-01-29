using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public enum AbilityType
{
    SkillShot,
    Choose,
    JustActivate,
    Passive //Activated ONCE on spawn of unit
}


public class Ability
{
    public KeyCode activationKey;
    public AbilityType type;
    public bool canMoveOn = false, reloaded = true;
    public Unit activator, chooseTarget;
    public Vector3 skillShotTarget;
    public float reloadTime;
    public Func<Ability, IEnumerator> routine;
    public AbilitySprite abilitySprite;
    public int id;
    public float currentReloadProcess;//0f..1f
    public string description;
    public string aName;
    public event EventHandler OnSpriteChange;
    public void ChangeSpriteTo(AbilitySprite ass)
    {
        abilitySprite = ass;
        OnSpriteChange?.Invoke(this,EventArgs.Empty);
    }
    public IEnumerator ResetTimer()
    {
        if (reloadTime != 0)
        {

            currentReloadProcess = 1f;

            //Если реп
            if (activator.isSelected && activator.unitType == GameMaster.singleton.repType)
            {
                //Ищем новый минимальный кд на способность
                GameMaster.singleton.SetNewMinCDToAbility(id);
            }



            while (currentReloadProcess > 0)
            {
                currentReloadProcess -= Time.deltaTime / reloadTime;
                yield return null;
            }
            currentReloadProcess = 0f;
        }
        reloaded = true;
    }
    public void Start()
    {
        if (!reloaded)
        {
            canMoveOn = true;
        }
        else
        {
            reloaded = false;
            activator.StartCoroutine(ResetTimer());
            activator.StartCoroutine(routine(this));
        }
    }
    public Ability(KeyCode activationKey, AbilityType type, Unit activator, Func<Ability, IEnumerator> routine, float reloadTime, AbilitySprite abilitySprite, int id, string aName, string description = "")
    {
        this.reloadTime = reloadTime;
        this.type = type;
        this.activationKey = activationKey;
        this.activator = activator;
        this.routine = routine;
        this.abilitySprite = abilitySprite;
        this.id = id;
        this.description = description;
        this.aName = aName;
    }
}
