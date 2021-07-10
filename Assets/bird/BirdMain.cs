using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BirdMain : MonoBehaviour
{
    // Things for Awake
    [NonSerialized] public BirdAnimation anim;
    [NonSerialized] public BirdController cont;
    
    private void Awake()
    {
        anim = GetComponent<BirdAnimation>();
        cont = GetComponent<BirdController>();
    }
    
    // Members
    public enum S // State
    {
        Gliding, Flapping, Diving, Braking, Landing, TakingOff
    }
    public S state;
    
    
    // Functions
    private void Start()
    {
        // StartCoroutine(nameof(ChangeState));
        SetState(S.Flapping);
    }

    public void SetState (S newState)
    {
        if (newState == state) return;

        // Do things depending on old value of state
        switch (state)
        {
            case (S.Flapping):
                cont.Flap(0);
                break;
            default:
                break;
        }
        // Do things depending on new value of state
        switch (newState)
        {
            case S.Diving:
                break;
            case S.Gliding:
                break;
            case S.Flapping:
                break;
        }
        
        state = newState;
        print("State Set:" + newState);
        anim.ChangeAnimation(state);
    }
}
