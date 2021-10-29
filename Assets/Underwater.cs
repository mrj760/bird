using UnityEngine;
using System.Collections;
 
public class Underwater: MonoBehaviour {
    public float waterHeight;
 
    private bool isUnderwater;
    // private Color normalColor;
    private Color underwaterColor;
 
    // Use this for initialization
    void Start () {
        // normalColor = new Color (0.5f, 0.5f, 0.5f, 0.5f);
        underwaterColor = new Color(0.03f, 0.09f, 0.25f, 0.89f);
        RenderSettings.fogDensity = 0;
        RenderSettings.fogColor = underwaterColor;
        RenderSettings.fog = true;
    }
 
    // Update is called once per frame
    void Update () {
        if (transform.position.y < waterHeight != isUnderwater) {
            isUnderwater = transform.position.y < waterHeight;
            if (isUnderwater) SetUnderwater ();
            if (!isUnderwater) SetNormal ();
        }
    }
 
    void SetNormal () {
        // RenderSettings.fogColor = normalColor;
        RenderSettings.fogDensity = 0;
    }
 
    void SetUnderwater ()
    {
        RenderSettings.fogDensity = 0.1f;
    }
}