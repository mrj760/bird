using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BirdMain;

public class BirdPlayerController : MonoBehaviour
{
    private BirdMain main;
    private BirdAnimation anim;
    private Rigidbody rb;

    private void Start()
    {
        main = GetComponent<BirdMain>();
        anim = main.anim;
        rb = GetComponent<Rigidbody>();
    }

    private float hinp, vinp, rhinp, ltinp, rtinp;
    private void Update()
    {
        // Left stick used for Yaw and Pitch
        hinp = Input.GetAxis("Horizontal");
        vinp = Input.GetAxis("Vertical");
        // Bumpers used for Roll
        rhinp = 0 
                - ((Input.GetButton("LeftBumper"))  ? 1 : 0) 
                + ((Input.GetButton("RightBumper")) ? 1 : 0);
        
        ltinp = Input.GetAxis("LeftTrigger");
        rtinp = Input.GetAxis("RightTrigger");
    }

    public float yawSpeed = 80f, pitchSpeed = 80f, rollSpeed = 80f;
    public float moveSpeed = 3f, diveSpeed = 10, flapSpeed = 3;
    private Quaternion deltaRot, diveRot;

    private void FixedUpdate()
    {

        Vector3 totalV3Rot =
            // (Yaw+Pitch+Roll) * FDT
            Time.fixedDeltaTime *
            (  Vector3.up      * (hinp  * yawSpeed)
             + Vector3.right   * (vinp  * pitchSpeed)
             + Vector3.forward * (rhinp * rollSpeed));
        // turn total into Quat and rotate the RB
        deltaRot = Quaternion.Euler(totalV3Rot);
        rb.MoveRotation(rb.rotation * deltaRot);
        
        // Total Dive/Flap Input
        var totalFlyBlend = rtinp - ltinp;
        
        // Cater to main state handler and animator
        // +: Flapping // -: Diving // 0: Gliding
        main.SetState(totalFlyBlend > 0 ? S.Flapping: totalFlyBlend < 0 ? S.Diving : S.Gliding);
        anim.SetBlend(totalFlyBlend/3 + (.33f));
        
        // Add rotation toward ground when Diving
        if (main.state == S.Diving)
        {
            Vector3 down = 
                Quaternion.LookRotation(Vector3.up).eulerAngles;    // it says 'up' but it gives down
            down.y = 
                rb.rotation.eulerAngles.y;
            var t = (ltinp/10)/2;
            print(t);
            diveRot = 
                Quaternion.Slerp(rb.rotation, Quaternion.Euler(down), t);
            rb.MoveRotation(diveRot);
        }

        // Flap faster when more RT pressure
        if (main.state == S.Flapping)
        {
            anim.SetSpeed(flapSpeed * rtinp);
        }
        else
        {
            anim.SetSpeed(1);
        }
    }

    private float moveSpeedMult = 0;
    public void SetMoveSpeed(float _f)
    {
        moveSpeedMult = _f;
    }
}
