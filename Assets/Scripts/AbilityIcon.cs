using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (mouseOver)
        {
            timer += Time.deltaTime;
            if (timer >= timeToShow)
            {
                mouseOver = false;
                HoveringThing.singleton.Appear(attachedAbility.aName,attachedAbility.description);
            }
        }
        image.color = new Color(1 - attachedAbility.currentReloadProcess, 1 - attachedAbility.currentReloadProcess, 1 - attachedAbility.currentReloadProcess, 1);
    }
    public Image image;
    public Ability attachedAbility;
    [SerializeField] Image panelImage;
    [SerializeField] Text panelText;
    public void Instantiate()
    {
        image = GetComponent<Image>();
        SetInactive();
    }
    bool mouseOver = false;
    float timer = 0f;
    const float timeToShow = 1f;
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
        timer = 0f;
        HoveringThing.singleton.Disappear();
    }
    public void SetActive()
    {
        enabled = true;
        image.color = new Color(1, 1, 1, 1);
        panelText.enabled = true;
        panelImage.enabled = true;
        Update();
    }
    public void AbilitySpriteChange(object obj, EventArgs args)
    {
        image.sprite = AbilityPanel.singleton.GetSprite(attachedAbility);
    }
    public void SetInactive()
    {
        if (attachedAbility!=null)
        {
            attachedAbility.OnSpriteChange -= AbilitySpriteChange;
        }
        enabled = false;
        image.color = new Color(1, 1, 1, 0);
        panelText.enabled = false;
        panelImage.enabled = false;
    }
    //  public bool isActive;
    public void AttachAbility(Ability ability)
    {
        if (attachedAbility != null)
        {
            attachedAbility.OnSpriteChange -= AbilitySpriteChange;
        }
        attachedAbility = ability;
        SetActive();
        image.sprite = AbilityPanel.singleton.GetSprite(attachedAbility);
        attachedAbility.OnSpriteChange += AbilitySpriteChange;
        panelText.text = ((char)ability.activationKey).ToString().ToUpper();
    }
    public void Clicked()
    {
        GameMaster.singleton.ActivateAbilityInReps(attachedAbility);
    }
}
