using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dirt : MonoBehaviour {

    private readonly int LIFE_TIME = 80,
        ANIM_LENGTH = 20;
    private int counter = 0;
    private SpriteRenderer spriteRenderer;

   	// Use this for initialization
	void Start () {
        spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (counter == 0 || (counter < LIFE_TIME & counter % ANIM_LENGTH == 0) & Player.updateGame)
        {  
            float a = (float)ANIM_LENGTH / LIFE_TIME;            
            transform.localScale += new Vector3(10, 10, 0)*a;
            spriteRenderer.color -= new Color(0, 0, 0, a);
            counter++;
        } else if (counter < LIFE_TIME & Player.updateGame)
        {
            counter++;
        }        
    }
}
