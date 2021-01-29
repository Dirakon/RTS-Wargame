using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mover : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (agent.enabled == false)
            agent = null;
        noPathYet = true;
      //  agent.speed = 0;
      // agent.angularSpeed = 0;
    }
    [HideInInspector]
    public bool noPathYet=true;
    public float velocity = 0;
    // Update is called once per frame
    void Update()
    {
        if (agent != null || speed == 0)
            return;
        var p = (transform.position - tar);
        p.y = 0;
        if (p.magnitude < stoppingDistance)
        {
            noPathYet = true;
        }
        Vector3 target;
        target = tar;
        float desiredVelocity;
        if (isStopped || noPathYet)
        {
            desiredVelocity = 0.0f;
        }else
        {
            desiredVelocity = speed;
        }
        //Make velocity

        var velMovHow = acceleration * Mathf.Sign(desiredVelocity - velocity) * Time.deltaTime;
        if (Mathf.Abs(velocity + velMovHow) > speed)
        {
            velocity = desiredVelocity * Mathf.Sign(velocity + acceleration);
        }else
        {
            if (desiredVelocity == 0.0f &&(velocity == 0.0f|| Mathf.Sign(velocity) != Mathf.Sign(velocity + velMovHow)))
            {
                    velocity = 0.0f;
            }
            else
            {
                velocity += velMovHow;
            }
        }
        rb.velocity = transform.forward * velocity;
        if (isStopped || noPathYet)
        {
            return;
        }

        target.y = transform.position.y;
        Quaternion idealRotation = Quaternion.LookRotation((target - transform.position).normalized);
        var movHow = (idealRotation.eulerAngles - transform.rotation.eulerAngles);
        for (int i = 0; i < 3; ++i)
        {
            movHow[i] = movHow[i] % 360;
            if (movHow[i] < 0)
                movHow[i] = 360 + movHow[i];

            if (movHow[i] > 180)
            {
                movHow[i] -= 360;
            }
        }
      //  movHow.x = 0.0f;
        var move = movHow.normalized * Time.deltaTime * angularSpeed;

        //if (movHow.magnitude <= move.magnitude)
        if (movHow.magnitude < angularSpeed / 100f)
        {
            transform.rotation = idealRotation;
        }
        else
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + move);
        }
        //rb.velocity = transform.forward * speed;
    }
    Rigidbody rb;
    public NavMeshAgent agent;
    Vector3 tar = Vector3.negativeInfinity;
    float speed;
    float angularSpeed;
    float acceleration;
    bool isStopped=false;
    float stoppingDistance;
    public void SetAngularSpeed(float angularSpeed)
    {
        if (agent != null)
            agent.angularSpeed = angularSpeed;
       this.angularSpeed = angularSpeed;
    }
    public void SetStopingDistance(float stDist)
    {
        if (agent != null)
            agent.stoppingDistance = stDist;
        stoppingDistance = stDist;
    }
    public float GetStoppingDistance()
    {
        return stoppingDistance;
    }
    public void SetSpeed(float speed)
    {
       if (agent != null)
            agent.speed = speed;
       this.speed = speed;
    }
    public void SetAcceleration(float acceleration)
    {
        if (agent != null)
            agent.acceleration = acceleration;
        this.acceleration = acceleration;
    }
    public void SetIsStopped(bool isStopped)
    {
        if (agent != null)
           agent.isStopped = isStopped;
        this.isStopped = isStopped;
    }
    public float GetSpeed()
    {
        return speed;
    }
    public float GetAcceleration()
    {
        return acceleration;
    }
    public float GetAngularSpeed()
    {
        return angularSpeed;
    }
    public bool GetIsStopped()
    {
        return isStopped;
    }
    public void SetDestination(Vector3 dest)
    {
        if (agent == null)
        {
            tar = dest;
            noPathYet = false;
        }
        else
        {
            agent.SetDestination(dest);
            noPathYet = false;
        }
    }
    public float getVelocity()
    {
        if (agent == null)
        {
            return velocity;
        }else
        {
            return agent.velocity.magnitude;
        }
    }
}
