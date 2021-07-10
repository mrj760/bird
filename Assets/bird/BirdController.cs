using System;
using System.Collections;
using System.Linq.Expressions;
using UnityEngine;
using static Bird;

public class BirdController : MonoBehaviour
{
    private Bird main;
    private Rigidbody rb;

    public float desiredHeight;

    private void Awake()
    {
        main = GetComponent<Bird>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        StartCoroutine(nameof(CheckHeight));
    }

    /**
     * Should only Call from FixedUpdate()
     */
    private void Move(Vector3 dir, float moveSpeed)
    {
        rb.MovePosition(dir * (moveSpeed * Time.fixedDeltaTime));
    }

    public float
        dragMult = 20f,
        flapForce = 5f,
        glideFallSpeed = -.1f,
        diveFallSpeed = -1.5f;
    private void FixedUpdate()
    {
        var vel = rb.velocity;
        var blend = Mathf.Clamp(main.anim.blend, .01f, .66f);

        switch (main.state)
        {
            case (S.Gliding):
                vel.y -= .001f/(blend*dragMult);
                vel.y = Mathf.Clamp(vel.y, glideFallSpeed, 0f);
                break;
            case (S.Flapping):
                var forceMult = heightCheckFlapMult;
                rb.AddForce(transform.up * (push * flapForce * forceMult));
                break;
            case (S.Diving):
                vel.y -= .01f / (blend * dragMult);
                vel.y = Mathf.Clamp(vel.y, diveFallSpeed, 0f);
                break;
        }

        rb.angularVelocity = Vector3.zero;

        try
        {
            rb.velocity = vel;
        }
        catch (Exception)
        {
            print("Cannot set velocity to: " + vel);
        }
    }

    public float heightCheckDist = 15f, heightCorrectionOffset = 2f;
    private float heightCheckFlapMult = 0;
    private IEnumerator CheckHeight()
    {
        while (true)
        {
            if (Physics.Raycast(new Ray(
                    rb.position, 
                    -rb.transform.up), 
                out var hit, heightCheckDist)) //
            {
                var dist = hit.distance;
                if (desiredHeight - dist > heightCorrectionOffset)
                {
                    var curToDes = dist / desiredHeight;
                    heightCheckFlapMult = 
                        Mathf.Lerp(
                            heightCheckFlapMult, 
                            Mathf.Sqrt(1 - (curToDes * curToDes)),
                            .2f); //
                    main.SetState(S.Flapping);
                    main.anim.blend = 
                        (heightCheckFlapMult * main.anim.GLIDE_BLEND) + main.anim.GLIDE_BLEND;
                }
                else if (dist - desiredHeight > heightCorrectionOffset)
                {
                    main.SetState(S.Gliding);
                }
            }
            else
            {
                main.SetState(S.Gliding);
            }

            yield return new WaitForSeconds(.2f);
        }
    }

    private float push = 0;
    public void Flap(float _push)
    {
        push = _push;
        
        var vel = rb.velocity;
        vel.y = 0;
        rb.velocity = vel;
    }
}
