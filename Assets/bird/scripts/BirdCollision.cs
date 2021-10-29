using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BirdMain;

public class BirdCollision : MonoBehaviour
{
    
    BirdMain main;

    public enum BodyPart {
        Torso,
        LeftUpperWing,
        LeftLowerWing,
        RightUpperWing,
        RightLowerWing,
        Tail,
        NULL
    }

    public static Dictionary<BodyPart, S> birdCollisionStateDict = new Dictionary<BodyPart, S>();

    [Header("Type")]
    public BodyPart bodyPart = BodyPart.NULL;

    private void Awake()
    {
        if (birdCollisionStateDict.Count == 0)
        {
            birdCollisionStateDict.Add(BodyPart.Torso, S.BigCrash);
            birdCollisionStateDict.Add(BodyPart.Tail, S.LiteCrash);
            birdCollisionStateDict.Add(BodyPart.RightLowerWing, S.LiteCrash);
            birdCollisionStateDict.Add(BodyPart.RightUpperWing, S.LiteCrash);
            birdCollisionStateDict.Add(BodyPart.LeftLowerWing, S.LiteCrash);
            birdCollisionStateDict.Add(BodyPart.LeftUpperWing, S.LiteCrash);
        }
    }

    private void Start()
    {
        main = GetComponentInParent<BirdMain>();
        if (main == null)
            print("Cannot find main module for: " + this.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (main.state == S.BigCrash || /* don't crash if already big crashing */
            main.state == S.Hovering || /* Don't crash if still */
            main.state == S.Stalling)   /* Don't crash if still */
            return;
        Crash(bodyPart);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (main.state == S.BigCrash || /* don't crash if already big crashing */
            main.state == S.Hovering || /* Don't crash if still */
            main.state == S.Stalling)   /* Don't crash if still */
            return;
        Crash(bodyPart);
    }

    private void OnCollisionStay(Collision other)
    {
        if (main.state == S.Stalling || main.state == S.Hovering)
        {
            main.rb.MovePosition(Vector3.up * Time.deltaTime);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        main.rb.isKinematic = true; /* briefly kinematic to reset movement */
        main.rb.isKinematic = false;
    }

    int foo = 0;
    public void Crash(BirdCollision.BodyPart bodyPart)
    {


        switch(main.state)
        {
            case(S.BigCrash): /* Already Crashing */
                return;
        }

        main.bodyPartCrashed = bodyPart;

        print("Crashed " + main.bodyPartCrashed.ToString() + " " + foo++);
        switch (main.bodyPartCrashed)
        {
            /* Hitting torso causes full stun and reversal */
            case(BodyPart.Torso):
                main.SetState(S.BigCrash);
                break;

            /* Hitting Lower wing causes light stun */
            case(BodyPart.LeftLowerWing):
                main.SetState(S.LiteCrash);
                break;
            case(BodyPart.RightLowerWing):
                main.SetState(S.LiteCrash);
                break;

            /* Hitting upper wing causes light stun */
            case(BodyPart.LeftUpperWing):
                main.SetState(S.LiteCrash);
                break;
            case(BodyPart.RightUpperWing):
                main.SetState(S.LiteCrash);
                break;
            case(BodyPart.Tail):
                main.SetState(S.LiteCrash);
                break;

            default:
                print("Invalid body part crashed.");
                break;
        }
    }

    public void UnCrash()
    {
        print("Uncrashing");
        switch (main.state)
        {
            case(S.BigCrash):
                main.SetState(S.Stalling);
                break;
            case(S.LiteCrash):
                main.SetState(S.Gliding);
                break;
            default:
                break;
        }
        main.bodyPartCrashed = BodyPart.NULL;
    }

}
