using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lazer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        actualMeshRenderer.material = materials[ptr];
    }
    public void SecondPosition(Vector3 pos)
    {
        sPos = pos;
        var mag = (pos - transform.position).magnitude;
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, mag / fatherScale);
        transform.rotation =    Quaternion.LookRotation( (pos - transform.position).normalized);
    }
    Vector3 sPos = Vector3.negativeInfinity;
    public void ChangeMaterial()
    {
        ptr = (ptr + 1) % materials.Length;
        actualMeshRenderer.material = materials[ptr];
    }
    public Gun gun;
    public float fatherScale=1;
    int ptr = 0;
    public Material[] materials;
    public void Appear()
    {
        actualMeshRenderer.enabled = true;
    }

    public void Hide()
    {
        actualMeshRenderer.enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (!gun.doIShoot || gun.body.target == null)
        {
            Hide();
        }else
        {
         //   Appear();
        }
    }
    public void ManageStart(Gun gun)
    {
        StartCoroutine(manager(gun));
    }
    IEnumerator manager(Gun gun)
    {
        while (true)
        {
            yield return null;
            if (gun.doIShoot && gun.body.target != null)
            {
                gun.lasersActive = true;
                yield return new WaitForSeconds(gun.body.laserDuration);
                gun.lasersActive = false;
                if (gun.body.laserReload != 0f)
                {
                    yield return new WaitForSeconds(gun.body.laserReload);
                }
                gun.lasersActive = true;
            }
        }
    }
    public GameObject actualLazerBody;
    public MeshRenderer actualMeshRenderer;
}
