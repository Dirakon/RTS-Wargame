using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        fatherScale = 1;
        Hide();
    }

    // Update is called once per frame
    public void Hide()
    {
        mr.enabled = false;
    }
    public void Appear()
    {
        mr.enabled = true;
    }
    public MeshRenderer mr;
    Transform sec;
    public float fatherScale;
    public Unit unit;
    public void SecondPosition(Transform pos)
    {
        transform.localScale *=fatherScale;
        fatherScale = unit.transform.localScale.x;
        transform.localScale /= fatherScale;
        sec = pos;
    }
    void Update()
    {
        if (mr.enabled && sec != null)
        {
            var mag = (sec.transform.position - transform.position).magnitude;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, mag / fatherScale);
            transform.rotation = Quaternion.LookRotation((sec.transform.position - transform.position).normalized);
        }
    }
}
