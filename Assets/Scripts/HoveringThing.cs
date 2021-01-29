using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoveringThing : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    private void Awake()
    {
        singleton = this;
        panel = GetComponentInChildren<Image>();
        Disappear();
    }
    Image panel;
    [SerializeField]Text mainText;
    [SerializeField] Text descText;
    static public HoveringThing singleton;
    // Update is called once per frame
    void Update()
    {
        
    }
    public void Appear(string text, string desc = "")
    {
        transform.position = Input.mousePosition;
        panel.enabled = true;
        this.descText.enabled = true;
        this.mainText.enabled = true;
        this.mainText.text = text;
        this.descText.text = desc;
    }
    public void Disappear()
    {
        panel.enabled = false;
        mainText.enabled = false;
        descText.enabled = false ;
    }
}
