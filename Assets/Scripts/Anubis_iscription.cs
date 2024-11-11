using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anubis_iscription : MonoBehaviour {
    
    private static readonly int SAVE_MENU_ANIM_SPRITES = 4;
    private readonly Color lightColor = new Color((float)202 / 255, (float)141 / 255, (float)160 / 255);

    private Sprite inscriptionActivatedSprite,
        inscriptionDeActivatedSprite;
    private static Sprite[] saveMenuAnimSprites = new Sprite[SAVE_MENU_ANIM_SPRITES];
    private SpriteRenderer inscriptionLightSR, 
        inscriptionLightBlobSR,
        anubisStatueSR,
        inscriptionSR;
    private BoxCollider2D inscriptionCollider;
    private AudioSource inscriptionAudioSource;
    private AudioClip pickupClip;
    private GameObject saveMenuGO = null;

    private bool saving = false,

        tilePressed = false;
    private int frameCounter = 0,
        initialAmulets = 0;

    private readonly int ONE_AMULET_FRAMES = 180, 
        TOTAL_FRAMES = 60,
        FPS_HALF = 30,
        SAVE_MENU_TIME = 121;

	// Use this for initialization
	void Start () {
        inscriptionActivatedSprite = Resources.Load<Sprite>("Sprites/anubis_inscription_pressed");
        inscriptionSR = GetComponent<SpriteRenderer>();
        inscriptionDeActivatedSprite = inscriptionSR.sprite;

        inscriptionCollider = GetComponent<BoxCollider2D>();
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr.sprite.name == "inscription_light")
            {
                inscriptionLightSR = sr;
            } else if (sr.sprite.name == "inscription_light_blob")
            {
                inscriptionLightBlobSR = sr;
            }

        }

        inscriptionAudioSource = GetComponent<AudioSource>();
        pickupClip = Resources.Load<AudioClip>("SFXs/pickup");

        GameObject anubisStatue = GameObject.Find("Anubis statue");
        anubisStatueSR = anubisStatue.GetComponent<SpriteRenderer>();
        inscriptionLightSR.enabled = inscriptionLightBlobSR.enabled = false;

        saveMenuAnimSprites = Resources.LoadAll<Sprite>("Sprites/Save Menu");
    }

    // Update is called once per frame
    void Update () {
        // If the save message is being displayed...
        if (saveMenuGO != null && frameCounter < SAVE_MENU_TIME)
        {
            int currentAnim = frameCounter / FPS_HALF;
            if (currentAnim >= 2)
            {
                string saveString = "SAVING";
                Utils.DisplayTextLine(saveString, new Vector3(-0.225f, -1.05f, 0), 
                    new Vector3(0.25f, 0.25f, 0), 
                    0.48f, true, Utils.STANDARD_FONT, Color.black);

            } 
            if (currentAnim <= 2)
            {
                SpriteRenderer saveMenuSR = saveMenuGO.GetComponent<SpriteRenderer>();
                saveMenuSR.sprite = saveMenuAnimSprites[currentAnim];
            }
            frameCounter++;
        }
        // ...else if the save message should be closed...
        else if (saveMenuGO != null && frameCounter == SAVE_MENU_TIME)
        {
            int result = Utils.Save();
            if (result == Utils.FAILURE)
            {
                print("Saving procedure failed");
            }
            Object.Destroy(saveMenuGO);
            Utils.DeleteText();
            frameCounter = 0;
            Player.saving = false;
        }
        // ...else if the saving process has not began yet...
        else if (saveMenuGO == null)
        {
            inscriptionLightSR.enabled = inscriptionLightBlobSR.enabled = saving; 

            // If the player steps on the tile...
            if (!tilePressed && inscriptionCollider.OverlapPoint(Player.playerCollider.transform.position))
            {
                tilePressed = true;
                if (Player.totalAmuletsCounter > 0)
                {
                    initialAmulets = Player.totalAmuletsCounter;
                    saving = true;
                }
                inscriptionSR.sprite = inscriptionActivatedSprite; // The sprite for when the tile is pressed.
                inscriptionAudioSource.Play();
            }
            // ...else if the player steps away from the tile...
            else if (tilePressed && !inscriptionCollider.OverlapPoint(Player.playerCollider.transform.position))
            {
                tilePressed = false;                
                ResetInscription();
            }
                        
            // If the player has been standing on the tile for less than ONE_AMULET_FRAMES number of frames...
            if (frameCounter < ONE_AMULET_FRAMES & saving)
            {                
                // If the current frame is a multiple of TOTAL_FRAMES...
                if (frameCounter % TOTAL_FRAMES == 0)
                {
                    // If the player has enough amulets...
                    if (Player.totalAmuletsCounter > 00)
                    {
                        inscriptionAudioSource.PlayOneShot(pickupClip);
                        Player.totalAmuletsCounter--;
                        Player.updateAmuletsCounter();

                        // The intensity of the light (i.e. the alpha component of the sprite) is proportional 
                        // to the number of seconds the player has been standing on the tile for. 
                        int a = frameCounter / TOTAL_FRAMES + 1;

                        inscriptionLightSR.color = inscriptionLightBlobSR.color = new Color(1, 1, 1, (float)a / 4); // Set alpha in 4 steps every 60 frames
                        Player.playerSR.color = anubisStatueSR.color = Color.white + (float)a / 4 * (lightColor - Color.white);
                    }
                    // ...else if the player has ran out of amulets...
                    else if (Player.totalAmuletsCounter == 0)
                    {
                        Player.updateGame = false;
                        ResetInscription();
                    }
                }
                frameCounter++;
            }
            // ...else if the player has been standing on the tile for ANIM_FRAMES number of frames,
            // while donating amulets...
            else if (frameCounter == ONE_AMULET_FRAMES)
            {
                initialAmulets = Player.totalAmuletsCounter;
                saveMenuGO = Utils.CreateSaveMenu();
                ResetInscription();
            }            
        }
    }

    void ResetInscription()
    {
        inscriptionSR.sprite = inscriptionDeActivatedSprite;
        Player.totalAmuletsCounter = initialAmulets;
        Player.updateAmuletsCounter();
        saving = false;
        inscriptionLightSR.color = inscriptionLightBlobSR.color = new Color(1, 1, 1, 0);
        Player.playerSR.color = anubisStatueSR.color = Color.white;
        frameCounter = 0;        
    }
}
