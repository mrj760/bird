using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using static BirdMain;

public class BirdPlayerController : MonoBehaviour
{
    private BirdMain main;
    private BirdAnimation anim;
    private Rigidbody rb;
    public CapsuleCollider [] col;

    [Header("Input")]
    private float hinp, vinp, bmpinp, ltinp, rtinp, totalFlyBlend;
    private float inputLeverage = 1f;
    
    [Header("Rotation")] 
    public float yawSpeed = 100f;
    public float pitchSpeed = 130f, rollSpeed = 200f, diveRotationStrength = .1f;
    private float fdt, downFactor, pitchRatio;
    private Vector3 down = Quaternion.LookRotation(Vector3.down).eulerAngles;
    private Quaternion inputRot, diveRot;

    [Header("Movement")] 
    [Range(0,10)] public float naturalGlideAcceleration = 2f;
    [Range(0,10)] public float naturalFlappingAcceleration = 2f;
    [Range(0,10)] public float naturalDiveAcceleration = 4f;
    [Range(0,10)] public float overspeedCorrection = 3f;
    [Range(0,1)] public float diveThreshold = .1f;
    public float flapPushSpeed = 2f, flapHeightGain = 2f;
    private float velMag;
    private Vector3 localVel;

    [Header("Stalling")] 
    public float stallBoundaryZSpeed = 3f, stallImmunityTime = 2f;
    private bool hasStallImmunity;
    
    [Header("Speed Limits")] 
    public float minGlideZSpeed = 6f;
    public float maxDiveZSpeed = 20f, maxGlideZSpeed = 8f, maxFlapZSpeed = 10f, maxFlapYSpeed = 4f, moveSpeedMult = 0;

    [Header("State Handling")] 
    public float stallTime = 1.5f;

    [Header("Crashing")]
    public float crashBackupZSpeed = 4f, liteCrashZSpeedMult = .85f;
    private float lastRecordedZSpeed;
    
    [Header("Other")]
    private const float RECIPROCAL_180 = 1f/180f;
    


    private void Start()
    {
        main = GetComponent<BirdMain>();
        anim = main.anim;
        rb = GetComponent<Rigidbody>();
        col = GetComponents<CapsuleCollider>();
    }



    /** UPDATE */
    private void Update()
    {
        #if UNITY_EDITOR
        if (Input.GetButtonDown("Start Button"))
        {
            EditorApplication.ExitPlaymode();
        }
        #endif
        
        switch (main.state)
        {
            case (S.Stalling):
                break;

            case (S.Hovering):
                GetHoveringInput();
                break;

            case (S.BigCrash): /* Do not allow input */
                break;

            case (S.LiteCrash):
                GetLiteCrashInput();
                break;

            default:
                GetFlyingInput();
                HandleFlyingInputAnimation();
                break;
        }
    }
    
    /* Allow pitch/roll/yaw input
        Allow diving/flapping */
    private void GetFlyingInput()
    {
        /* Left stick used for Roll and Pitch */
        hinp = inputLeverage * Input.GetAxis("Horizontal");
        vinp = inputLeverage * Input.GetAxis("Vertical");
        
        /* Bumpers used for Yaw */
        bmpinp = 0 
                 - ((Input.GetButton("LeftBumper"))  ? 1 : 0) 
                 + ((Input.GetButton("RightBumper")) ? 1 : 0);
        bmpinp *= inputLeverage;
        
        /* Dive/Flap Input */
        ltinp = Input.GetAxis("LeftTrigger");
        rtinp = Input.GetAxis("RightTrigger");
        totalFlyBlend = rtinp - ltinp;

        /* Combined Trigger Input:
                    (+: Flapping)  (-: Diving)  (0: Gliding) */
        main.SetState(totalFlyBlend > 0f ? S.Flapping : totalFlyBlend < 0f ? S.Diving : S.Gliding);
    }

    /* Anim speed handled in this file for simplicity */
    private void HandleFlyingInputAnimation()
    {
        anim.SetFlyingBlend(totalFlyBlend * .33f + (.33f)); 
        if (main.state == S.Flapping) /* Flap faster when more RT pressure */
        {
            anim.SetSpeed(anim.FlapAnimSpeed * rtinp/* Mathf.Sqrt(rtinp) */);
        }
    }

    /* Allow yaw input
        Allow exit by flapping/diving
        Do not allow pitch/roll input */
    private void GetHoveringInput()
    {
        /* Bumpers used for Roll */
        bmpinp = 0 
                 - ((Input.GetButton("LeftBumper"))  ? 1 : 0) 
                 + ((Input.GetButton("RightBumper")) ? 1 : 0);
        
        /* Total Dive/Flap Input */
        ltinp = Input.GetAxis("LeftTrigger");
        rtinp = Input.GetAxis("RightTrigger");
        totalFlyBlend = rtinp - ltinp;

        /* Combined Trigger Input:
                    (+: Flapping)  (-: Diving)  (0: Gliding) */
        if (Mathf.Abs(totalFlyBlend) > .5f) /* Exit hover if holding trigger at least halfway */
        {
            main.SetState(totalFlyBlend > 0f ? S.Flapping : S.Diving);
            StopCoroutine(StallExitCRT(0f));
            StartCoroutine(StallExitCRT(stallImmunityTime));
        }
    }

    /* Allow pitch/roll/yaw input
        Do not allow flapping/diving */
    private void GetLiteCrashInput()
    {
        /* Left stick used for Roll and Pitch */
        hinp = inputLeverage * Input.GetAxis("Horizontal");
        vinp = inputLeverage * Input.GetAxis("Vertical");
        
        /* Bumpers used for Yaw */
        bmpinp = 0 
                 - ((Input.GetButton("LeftBumper"))  ? 1 : 0) 
                 + ((Input.GetButton("RightBumper")) ? 1 : 0);
        bmpinp *= inputLeverage;
    }

    /** END UPDATE */



    /** FIXED UPDATE */
    private void FixedUpdate()
    {
        FixedSetup();
        FixedHandleRotation();
        FixedHandleMovement();
    }

    void FixedSetup()
    {
        fdt = Time.fixedDeltaTime;

        /* ROTATION */
        Vector3 totalV3Rot = fdt *
               ( Vector3.up      * (bmpinp * yawSpeed  )
               + Vector3.right   * (-vinp  * pitchSpeed)
               + Vector3.forward * (-hinp  * rollSpeed ) );
        inputRot = Quaternion.Euler(totalV3Rot);
        
        Vector3 locRot = transform.localRotation.eulerAngles;
        /* (0 : flat), (1 : down), (-1 : up) */
        pitchRatio = Mathf.Cos(Mathf.PI * (locRot.x - 90) * RECIPROCAL_180);

        /* MOVEMENT */
        if (main.state == S.BigCrash) 
            return;
        velMag = rb.velocity.magnitude;
        localVel = transform.InverseTransformDirection(velMag * transform.forward);
        if (localVel.z < stallBoundaryZSpeed)
        {
            main.SetState(S.Stalling);
        }
    }

    private void FixedHandleRotation()
    {
        rb.MoveRotation(rb.rotation * inputRot);

        switch (main.state)
        {
            case S.Diving: /* Add rotation toward ground when Diving */
                down.y = rb.rotation.eulerAngles.y;
                downFactor = ltinp * diveRotationStrength;
                diveRot = Quaternion.Slerp(rb.rotation, Quaternion.Euler(down), downFactor);
                rb.MoveRotation(diveRot);
                break;
                
            default:
                break;
        }
        
    }
    
    private void FixedHandleMovement()
    {
        switch (main.state)
        { 
            case (S.Gliding):
                Gliding:
                if (localVel.z > maxGlideZSpeed) /* Return to gliding speed if going too fast */
                {
                    localVel.z += fdt * naturalGlideAcceleration * (-overspeedCorrection + pitchGravityCalculation(pitchRatio));
                }
                else /* Obey speed laws normally */
                {
                    localVel.z += fdt * naturalGlideAcceleration * pitchGravityCalculation(pitchRatio);
                }
                break;
            
            case S.Flapping: /* thrust upward/forward when wings flap down. Very slight downward thrust when wings flap up. */

                // float flapAmount = (main.anim.GetBlend() - .3333f) * 3;
                float flapAmount = rtinp;
                float mult = flapAmount * moveSpeedMult * fdt;
                
                float y = localVel.y + flapHeightGain * mult;
                float heightGainSpeedCap = maxFlapYSpeed * flapAmount;
                y = Mathf.Clamp(y, -heightGainSpeedCap * .5f, heightGainSpeedCap);

                float z = localVel.z;
                float cappedFlapZSpeed = minGlideZSpeed + ((maxFlapZSpeed - minGlideZSpeed) * flapAmount);
                if (z > cappedFlapZSpeed) /* not flapping hard enough to go faster, act as if gliding */
                {
                    goto Gliding;
                }
                else /* flapping enough to accelerate */
                {
                    z += fdt * ((flapPushSpeed * mult) + (naturalFlappingAcceleration * pitchGravityCalculation(pitchRatio)));
                }
                
                if (z > maxFlapZSpeed) {z = maxFlapZSpeed; }
                localVel.y = y;
                localVel.z = z;
                break;
            
            case S.Diving: /* increase velocity to max depending on dive amount */

                if (localVel.z > maxDiveZSpeed)
                {
                    localVel.z -= fdt * naturalDiveAcceleration;
                }
                else
                {
                    if (ltinp > diveThreshold) /* Add faster dive acceleration if tucking in wings enough */
                            localVel.z += ltinp * fdt * naturalDiveAcceleration * pitchGravityCalculation(pitchRatio);
                    else goto Gliding;
                }
                break;

            case S.Stalling: /* Stop */
                localVel.z = 0f;
                break;

            case S.Hovering: /* Flap in place */
                localVel.z = 0f;
                break;

            case S.BigCrash: /* Move backwards */
                localVel.z = -crashBackupZSpeed;
                break;

            case S.LiteCrash: /* Slow down slightly */
                localVel.z = lastRecordedZSpeed * liteCrashZSpeedMult;
                break;

            default:
                break;
        }

        localVel.x = 0f;
        rb.velocity = transform.TransformDirection(localVel);
        // print("Loc Vel : " + transform.InverseTransformDirection(rb.velocity));
    }

    [Header("Pitch")]
    [Range(1,4)] public int pitchMode = 1;
    private float pitchGravityCalculation(float x)
    {
        switch (pitchMode)
        {
            case (1):
                /* y = .4893x^5 - .0227x^4 - 1.0978x^3 - .0958x^2 + 1.4248x - .0851 */
                return ( .4893f * x*x*x*x*x
                        -  .0227f * x*x*x*x
                        - 1.0978f * x*x*x
                        -  .0958f * x*x
                        + 1.4248f * x
                        -  .0851f );
            case (2):
                /* y = .7001x^4 - .2504x^3 - 1.1753x^2 + 1.5375x - .1721 */
                return ( .7001f * x*x*x*x
                        -  .2504f * x*x*x
                        - 1.1753f * x*x
                        + 1.5375f * x
                        -  .1721f );
            case (3):
                /* y = -0.3265x4 + 0.9258x3 - 0.1413x2 + 0.5873x - 0.0508 */
                return ( -.3265f * x*x*x*x
                        +  .9258f * x*x*x
                        -  .1413f * x*x
                        +  .5873f * x
                        -  .0508f );
            case (4):
                /* y = 2x - .1 */
                return 2*x - .3333333f;
        }
        print("BirdPlayerController.cs: Pitch/Gravity calculation has an invalid pitch mode");
        return -1;
    }

    /** END FIXED UPDATE */



    /** DO NOT DELETE - Used by the flapping animation to change how fast the bird gets pushed forward */
    public void SetMoveSpeed(float _f) { moveSpeedMult = _f; }
    


    /* Take care of local issues when changing states, 
        return false if a state change shouldn't occur */
    public bool SetState(S newState)
    {

        /* Depending on old value of state */
        switch (main.state)
        {
            case (S.BigCrash):
                ClearShit();
                StopCoroutine(StallCRT(0));
                hasStallImmunity = false;
                break;
            case (S.LiteCrash): /* Reset input leverage if recovering from lite crash */
                inputLeverage = 1;
                break;
        }

        /* Depending on new value of state */
        switch (newState)
        {
            case(S.Stalling):
                if (main.state == S.Hovering) return false; /* return false if trying to stall from hover */
                if (hasStallImmunity && Mathf.Abs(totalFlyBlend) > .5f) return false; /* return false if trying to stall right after exiting hover */
                ClearShit();
                StopCoroutine(StallCRT(0f));
                StartCoroutine(StallCRT(stallTime));
                break;

            case(S.Hovering):
                hasStallImmunity = true;
                break;

            case S.Flapping:
                anim.SetSpeed(anim.FlapAnimSpeed * rtinp);
                break;

            case S.LiteCrash:
                inputLeverage = .66f;
                RecordLastZSpeed();
                break;

            default:
                break;
        }

        /* All good to change states */
        return true;
    }


    public void RecordLastZSpeed()
    {
        lastRecordedZSpeed = localVel.z;
    }


    /* Clear rotation, movement, and input values. (except for Y-rotation) */
    public void ClearShit()
    {
        inputRot = Quaternion.Euler(Vector3.zero);
        hinp = 0;
        vinp = 0;
        totalFlyBlend = 0f;
        var rot = rb.rotation.eulerAngles;
        rot.x = 0;
        rot.z = 0;
        rb.rotation = Quaternion.Euler(rot);
        
        main.rb.isKinematic = true; /* briefly kinematic to reset movement */
        main.rb.isKinematic = false;
    }



    /** COROUTINES */
    private IEnumerator StallCRT(float time)
    {
        yield return new WaitForSeconds(time);
        main.SetState(S.Hovering);
    }

    private IEnumerator StallExitCRT(float time)
    {
        hasStallImmunity = true;
        yield return new WaitForSeconds(time);
        hasStallImmunity = false;
    }

    /** END COROUTINES */
}
