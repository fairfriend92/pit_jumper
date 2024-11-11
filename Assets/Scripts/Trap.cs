using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour {

    public bool activated = false;
    private int framesCounter = 0;
    private readonly int FRAMES_TO_BREAKDOWN = 40;
    private SpriteRenderer spriteRenderer = null;
    private BoxCollider2D trapCollider = null, 
        pitCollider = null;
    private GameObject pit = null;
    private Sprite brokenTrapSprite = null;
    private AudioSource audioSource = null;

    void Awake() {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        trapCollider = this.GetComponent<BoxCollider2D>();

        pit = new GameObject();
        pit.name = this.name + " pit";
        pit.tag = "Pit";
        pit.layer = LayerMask.NameToLayer("Pit");
        pit.transform.position = this.transform.position;
        pit.transform.localScale = this.transform.localScale;

        pit.AddComponent<BoxCollider2D>();
        pitCollider = pit.GetComponent<BoxCollider2D>();
        pitCollider.transform.position = this.transform.position;
        // Note that the trap itself is a little bit taller than the pit. 
        pitCollider.size = trapCollider.size - new Vector2(0.0f, 0.1f);
        pitCollider.enabled = false;

        brokenTrapSprite = Resources.Load<Sprite>("Sprites/trap_broken");

        audioSource = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update () {
		if (activated & Player.updateGame)
        {
            bool spriteIsVisible = spriteRenderer.transform.localScale.x > 0.1f;
            if (framesCounter >= FRAMES_TO_BREAKDOWN & spriteIsVisible)
            {
                trapCollider.enabled = false;
                pitCollider.enabled = true;
                spriteRenderer.transform.localScale -= new Vector3(0.15f, 0.15f, 0.0f);
            } else if (framesCounter < FRAMES_TO_BREAKDOWN)
            {
                if (framesCounter == 0)
                {
                    audioSource.Play();
                    spriteRenderer.sprite = brokenTrapSprite;
                }
                framesCounter++;                
            } else if (!spriteIsVisible)
            {
                spriteRenderer.enabled = false;
                activated = false;
            }
        }
	}
}
