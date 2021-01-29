using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyed : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ImmenentDemise());
    }


    IEnumerator ImmenentDemise()
    {
        yield return new WaitForSeconds(deathTime);
        Destroy(gameObject);
    }
    // Update is called once per frame
    public float deathTime;
    void Update()
    {
        
    }
}
