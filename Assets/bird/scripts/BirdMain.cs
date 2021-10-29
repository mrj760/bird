using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BirdCollision;
using Random = UnityEngine.Random;

public class BirdMain : MonoBehaviour
{
    // Things for Awake
    [NonSerialized] public BirdAnimation anim;
    [NonSerialized] public BirdPlayerController controller;
    [NonSerialized] public CapsuleCollider col;
    [NonSerialized] public Rigidbody rb;
    [NonSerialized] public BodyPart bodyPartCrashed;
    
    public enum S // State
    {
        Gliding, Flapping, Diving, Stalling, Hovering, BigCrash, LiteCrash, Braking, Landing, TakingOff
    }
    public S state { get; private set; }

    
    private void Awake()
    {
        anim = GetComponent<BirdAnimation>();
        controller = GetComponent<BirdPlayerController>();
        col = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
    }
    
    private void Start()
    {
        SetState(S.Hovering);
    }

    public void SetState (S newState)
    {

        if (newState == state) return;
        
        /* Check if the controller says we're good to change states.
            Let it do its prep work too */
        if (!controller.SetState(newState)) return;

        /* Do things depending on old value of state */
        switch (state) 
        {
            case (S.Hovering): /* Re-enable collision upon exiting hover */
                col.enabled = true;
                break;
            default:
                break;
        }
        
        state = newState;
        print("Now: " + state);

        /* Do things depending on new value of state */
        switch (state) 
        { 
            case (S.Stalling): /* Disable collision upon entering stalling/hovering */
                col.enabled = false;
                break;
            default:
                break;
        }
        
        anim.ChangeAnimation(state);
    }
    
}
