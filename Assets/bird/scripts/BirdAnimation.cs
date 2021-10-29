using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BirdMain;
using static BirdCollision;

public class BirdAnimation : MonoBehaviour
{
    private BirdMain main;
    private Animator anim;

    private Dictionary <S, string> stateString = new Dictionary<S, string>();
    Dictionary<BodyPart, float> LiteCrashBlends = new Dictionary<BodyPart, float>();

    public readonly float FlapAnimSpeed = 3f; /* Used in combination with flap input */
    
    /* Hashed Animator Parameters */
    private static readonly int 
        FlyingBlend = Animator.StringToHash("Flying Blend"), 
        LiteCrashBlend = Animator.StringToHash("Lite Crash Blend"),
        SpeedMult = Animator.StringToHash("SpeedMult");


    /* Get Components and add state-animation relationships to dictionary */
    private void Awake()
    {
        main = GetComponent<BirdMain>();
        anim = GetComponent<Animator>();
        
        stateString.Add(S.Gliding,   "Flying Blend Tree");
        stateString.Add(S.Flapping,  "Flying Blend Tree");
        stateString.Add(S.Diving,    "Flying Blend Tree");
        stateString.Add(S.Stalling,  "Stalling");
        stateString.Add(S.Hovering,  "Stalling");
        stateString.Add(S.BigCrash,  "Big Crash");
        stateString.Add(S.LiteCrash, "Lite Crash Blend Tree"); /* Holds a crash for every body type */
        stateString.Add(S.Braking,   "Brake");
        stateString.Add(S.Landing,   "Land");
        stateString.Add(S.TakingOff, "Take Off");

        LiteCrashBlends.Add(BodyPart.LeftLowerWing, 0f); /* Left wing takes full hit back */
        LiteCrashBlends.Add(BodyPart.LeftUpperWing, .25f); /* Left wing takes slight hit back */
        LiteCrashBlends.Add(BodyPart.Tail, .5f); /* Mid-point, Wings stay forward but shake a bit, more tail oomph */
        LiteCrashBlends.Add(BodyPart.RightUpperWing, .75f); /* Right wing takes slight hit back */
        LiteCrashBlends.Add(BodyPart.RightLowerWing, 1f); /* Right wing takes full hit back */
    }


    public void SetFlyingBlend(float blend) { anim.SetFloat(FlyingBlend, blend); }
    public float GetFlyingBlend() { return anim.GetFloat(FlyingBlend); }


    public void SetLiteCrashBlend(float blend) { anim.SetFloat(LiteCrashBlend, blend); }
    public float GetLiteCrashBlend() { return anim.GetFloat(LiteCrashBlend); }
    

    public void SetSpeed(float s) { anim.speed = s; }
    public float GetSpeed() { return anim.speed; }


    private const int numFlyBlends = 3, numLiteCrashBlends = 5;
    public readonly float 
        /* Flying Blends */
        DiveBlend = 0, /* Wings fully tucked */
        GlideBlend = 1f/numFlyBlends, /* Mid-point, Stable outspread wings */
        FlapBlend = 2f/numFlyBlends; /* Wings and tail fully flapping */


    /* Takes a state parameter and plays corresponding animation */
    private string currentAnim = "";
    public void ChangeAnimation(S state)
    {
        /* Get the corresponding string from the dictionary */
        if (stateString.TryGetValue(state, out var str))
        {
            if (str.Equals(currentAnim)) return;

            switch (state)
            {
                case (S.Hovering):
                    SetSpeed(2f);
                    break;

                case (S.Stalling):
                    SetSpeed(2f);
                    break;

                case (S.Flapping):
                    /* Flapping anim speed is taken care of in bird movement manager for simplicity */
                    // nothing here
                    break;

                case (S.BigCrash):
                    SetSpeed(2.5f);
                    break;

                case (S.LiteCrash):
                    SetSpeed(1);
                    if (LiteCrashBlends.TryGetValue(main.bodyPartCrashed, out float blend))
                        SetLiteCrashBlend(blend);
                    else
                        print("Error: BirdAnimation.cs: Cannot find blend value for crashed" +
                                "body part: " + main.bodyPartCrashed.ToString());
                    break;

                default:
                    SetSpeed(1);
                    break;
            }
            
            currentAnim = str;
            anim.Play(str);
        }
        else { print("Error: BirdAnimation.cs: Cannot Find Animation Value for state: " + state); }
    }

}
