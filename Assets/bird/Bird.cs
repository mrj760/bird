using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bird : MonoBehaviour
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
    
    private void Update()
    {
        
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
    
    // Coroutines
    // private IEnumerator ChangeState()
    // {
    //     while (true)
    //     {
    //         var vals = Enum.GetValues(typeof(S));
    //         var s = vals.GetValue(Random.Range(0, vals.Length));
    //         if (s is S _s)
    //         {
    //             SetState(_s);
    //         }
    //         else
    //         {
    //             print("Error: Unable to Randomly Set State");
    //         }
    //
    //         yield return new WaitForSeconds(3);
    //     }
    // }
}
