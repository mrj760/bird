using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BirdMain;

public class BirdAnimation : MonoBehaviour
{
    private BirdMain main;
    private Animator anim;

    private static readonly int FlyingBlend = Animator.StringToHash("Flying Blend");
    private static readonly int SpeedMult = Animator.StringToHash("SpeedMult");

    private Dictionary <S, string> stateString = new Dictionary<S, string>();


    private void Awake()
    {
        main = GetComponent<BirdMain>();
        anim = GetComponent<Animator>();
        
        stateString.Add(S.Gliding, "Flying Blend Tree");
        stateString.Add(S.Flapping, "Flying Blend Tree");
        stateString.Add(S.Diving, "Flying Blend Tree");
        stateString.Add(S.Braking, "Brake");
        stateString.Add(S.Landing, "Land");
        stateString.Add(S.TakingOff, "Take Off");
    }

    private void Update()
    {
    }

    public void SetBlend(float blend)
    {
        anim.SetFloat(FlyingBlend, blend);
    }

    public float GetBlend()
    {
        return anim.GetFloat(FlyingBlend);
    }

    public void SetSpeed(float s)
    {
        // anim.SetFloat(SpeedMult, mult);
        anim.speed = s;
    }

    public float GetSpeed()
    {
        return anim.speed;
    }

    private const int numBlends = 3;
    public readonly float 
        DIVE_BLEND = 0, 
        GLIDE_BLEND = 1f/numBlends, 
        FLAP_BLEND = 2f/numBlends; 
    
    public void ChangeAnimation(S state)
    {
        if (stateString.TryGetValue(state, out var str))
        {
            // switch (state)
            // {
            //     case (S.Diving):
            //         // blend = DIVE_BLEND;
            //         break;
            //     case (S.Gliding):
            //         // blend = GLIDE_BLEND;
            //         break;
            //     case (S.Flapping):
            //         // blend = FLAP_BLEND;
            //         break;
            //     case (S.TakingOff):
            //         // blend = GLIDE_BLEND;
            //         break;
            //     default:
            //         break;
            // }
            anim.Play(str);
        }
        else
        {
            print("Error: BirdAnimation.cs: Cannot Find Animation Value: " + state);
        }
    }
}
