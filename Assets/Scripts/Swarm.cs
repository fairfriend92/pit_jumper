using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swarm : MonoBehaviour {
    private BoxCollider2D swarmBoxCollider;
    private AudioSource audioSource;
    private AudioClip swarmNoiseClip;
    private int extraSeconds, 
        frameCounter = 0, 
        currentSprite = 0;
    private readonly float MAX_AUDIO_DISTANCE = 8.0f;
    private static readonly int ANIM_FRAME = 2;
    private SpriteRenderer swarmSR;
    private Sprite[] swarmiAnimSprites = new Sprite[ANIM_FRAME];

	void Awake () {
        swarmNoiseClip = Resources.Load<AudioClip>("SFXs/swarm");

        audioSource = GetComponent<AudioSource>();

        swarmBoxCollider = GetComponent<BoxCollider2D>();

        // extraSeconds is accessed statically because it needs to be 
        // preserved when a new scene is loaded.
        extraSeconds = Player.extraSeconds;

        swarmSR = GetComponent<SpriteRenderer>();
        swarmiAnimSprites = Resources.LoadAll<Sprite>("Sprites/Grasshoppers");
	}
	
	void Update () {
        if (frameCounter % ANIM_FRAME == 0 & Player.updateGame)
        {
            swarmSR.sprite = swarmiAnimSprites[currentSprite];
            currentSprite = currentSprite == 0 ? 1 : 0;
        }
        frameCounter++;

        float volume = Player.swarmDistance > MAX_AUDIO_DISTANCE ? 0.0f :
               1.0f - Player.swarmDistance / MAX_AUDIO_DISTANCE;
        audioSource.volume = volume;

        if (!audioSource.isPlaying & Player.updateGame)
        {
            audioSource.PlayOneShot(swarmNoiseClip);
        } else if (!Player.updateGame)
        {
            audioSource.Stop();
        }
        
        if (Player.updateGame)
        {
            float timeToBegin = Time.timeSinceLevelLoad - extraSeconds;
            if (timeToBegin >= 0)
            {
                transform.Translate(new Vector3(0.0f, 0.05f));
            } else
            // The more extra seconds the player has accumulated, the more muted
            // the sound of the swarm is going to be, to suggest distance.
            {
                audioSource.volume *= 1.0f - Mathf.Abs(timeToBegin / extraSeconds);
            }
        }

        if (swarmBoxCollider.OverlapPoint(Player.playerCollider.transform.position))
        {
            Player.gameOver = true;
            Player.EndLevel();
        }
	}
}
