using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {
    public static float swarmDistance = 0.0f;
    public static int extraSeconds = 0, 
        totalAmuletsCounter, 
        levelAmuletsCounter,
        livesCounter;
    public static BoxCollider2D playerCollider;

    // Game states.
    public static bool levelEnded = false,
        gameOver = false,
        updateGame = true,
        saving = false,
        gamePaused = false;

    private static float elapsedTime = 0.0f;

    // Action buttons.
    private KeyCode moveRight = Bindings.moveRight,
        moveLeft = Bindings.moveLeft,
        moveUp = Bindings.moveUp,
        moveDown = Bindings.moveDown,
        jump = Bindings.jump,
        cancelJump = Bindings.cancelJump;

    // Counters for the frames of the player animations.
    private int runAnimFrameCounter = 0, 
        playerCrouchFrameCounter = 0, 
        crouchAnimFrames = 0;

    // Player states.
    private bool playerCrounching = false, 
        playerJumping = false,
        playerFalling = false,
        pausedAudioSource = false;
    
    // Const.
    private readonly float MAX_LATERAL_DISTANCE = 3.0f, // Half the longitudinal length of the stage.
        MAX_DISTANCE_FROM_GUI = 11.5f, // Max distance between the player and the gui, before the pos gui is forcibly updated.
        RAYCAST_X_OFFSET = 0.6f, // 0.7f Half the length of the side of the player collision box parallel to the x axis.

        /*
         * <--> RAYCAST_X_OFFSET
         * -------
         * |     |  <- Player collision box
         * |     |
         * -------
         */

        RAYCAST_Y_OFFSET = 0.5f; // Half the lenght of side parallel to the y axis.

        /* 
         * ^  ^  ^
         * |  |  |   <- Each face has 3 raycasts 
         * ------- -
         * |     | | <- RAYCAST_Y_OFFSET
         * |     | -
         * -------
         */
             
    private readonly int RUN_ANIM_FRAMES = 10,
        CROUCH_ANIM_FRAMES = 80,
        MIN_CROUCH_ANIM_FRAMES = 40;
    
    private Vector3 originalPlayerScale;
    private Quaternion originalPlayerRotation;

    private GameObject jumpTarget, 
        mainCamera;
    public static GameObject gui;

    public static SpriteRenderer playerSR, 
        amuletsDigitOneSR, amuletsDigitTwoSR;
    private static SpriteRenderer jumpTargetSR,
        minutesDigitSR, secondsDigitOneSR, secondsDigitTwoSR;

    private Sprite playerStandS,
        playerJumpS,
        playerCrouchS,
        jumpTargetS,
        jumpTargetRedS,
        playerMoveCrateA, 
        playerMoveCrateB,
        currentSprite;
    private static Sprite[] digitsSprites = new Sprite[10];

    public static LayerMask obstaclesLayerMask, 
        pitsLayerMask, 
        collectablesLayerMask, 
        swarmLayerMask;

    private AudioClip stepClip, 
        jumpClip,
        pickupClip,
        screamClip,
        crateMovedClip;
    private AudioSource playerAudioSource;

    private static Scene scene;

    private RaycastHit2D pitsRayCast;

    void Awake () {
        playerCollider = GetComponent<BoxCollider2D>();
        
        // Static variables that must be reset.
        levelEnded = false;
        gameOver = false;
        levelAmuletsCounter = 0;

        // Load sprites.
        digitsSprites = Resources.LoadAll<Sprite>("Sprites/Digits");
        playerStandS = Resources.Load<Sprite>("Sprites/player_stand");
        playerJumpS = Resources.Load<Sprite>("Sprites/player_jump");
        playerMoveCrateA = Resources.Load<Sprite>("Sprites/player_move_crate_1");
        playerMoveCrateB = Resources.Load<Sprite>("Sprites/player_move_crate_2");
        playerCrouchS = Resources.Load<Sprite>("Sprites/player_crouch");
        jumpTargetRedS = Resources.Load<Sprite>("Sprites/jump_target_red");
        jumpTargetS = Resources.Load<Sprite>("Sprites/jump_target");
        currentSprite = playerJumpS;

        // Load audio clips.
        stepClip = Resources.Load<AudioClip>("SFXs/step");
        jumpClip = Resources.Load<AudioClip>("SFXs/jump");
        pickupClip = Resources.Load<AudioClip>("SFXs/pickup");
        screamClip = Resources.Load<AudioClip>("SFXs/scream");
        crateMovedClip = Resources.Load<AudioClip>("SFXs/crate_moved");

        // Game objects are retrieved.
        mainCamera = GameObject.FindWithTag("MainCamera");
        jumpTarget = GameObject.FindWithTag("Jump Target");
        gui = GameObject.FindWithTag("GUI");

        // Sprite renderes component are retrieved.
        playerSR = GetComponent<SpriteRenderer>();
        jumpTargetSR = jumpTarget.GetComponent<SpriteRenderer>();
        jumpTargetSR.enabled = false;

        amuletsDigitOneSR = GameObject.Find("Amulets first digit").GetComponent<SpriteRenderer>();
        amuletsDigitOneSR.color = Color.black;
        amuletsDigitTwoSR = GameObject.Find("Amulets second digit").GetComponent<SpriteRenderer>();
        amuletsDigitTwoSR.color = Color.black;
        minutesDigitSR = GameObject.Find("Minutes digit").GetComponent<SpriteRenderer>();
        minutesDigitSR.color = Color.black;
        secondsDigitOneSR = GameObject.Find("Seconds first digit").GetComponent<SpriteRenderer>();
        secondsDigitOneSR.color = Color.black;
        secondsDigitTwoSR = GameObject.Find("Seconds second digit").GetComponent<SpriteRenderer>();
        secondsDigitTwoSR.color = Color.black;

        originalPlayerScale = transform.localScale;
        originalPlayerRotation = transform.rotation;

        // LayerMasks are generated from the various layers.
        obstaclesLayerMask = LayerMask.GetMask("Obstacle");
        pitsLayerMask = LayerMask.GetMask("Pit");
        collectablesLayerMask = LayerMask.GetMask("Collectable");
        swarmLayerMask = LayerMask.GetMask("Swarm");

        playerAudioSource = GetComponent<AudioSource>();

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        if (!Utils.initialized)
        {
            Utils.Init();
            Shared.Init();            
        }

        // The menu must be re-initialized every time, since its objects
        // get destroyed with the Player.
        PauseMenu.Init();

        scene = SceneManager.GetActiveScene();

        // At the beginning of the level the gui must be updated with
        // the amulets collected in the previous levels.
        updateAmuletsCounter();
    }

    void Update () {
        int localScaleXSign = transform.localScale.x < 0 ? -1 : 1; // When moving up or down the sprite is mirrored using localScale. This keeps track of the sign.
        pitsRayCast = Physics2D.Raycast(transform.position, Vector3.back, 0.0f, pitsLayerMask); // Originates from the player, points downwards and detects pits. 
        RaycastHit2D jumpTargetRCBack = Physics2D.Raycast(jumpTarget.transform.position, Vector3.back, 0.0f);
        
        if (Input.GetKey(Bindings.pause) & !gamePaused)
        {
            gamePaused = true;
            PauseMenu.openMenu = true;
        } else if (gamePaused)
        {
            PauseMenu.UpdateMenu();
        }

        updateGame = !levelEnded & !gameOver & !saving & !gamePaused;

        // Update timer.
        if (updateGame & !Shared.SCENE_IS_TEMPLE[scene.buildIndex]) // Timer is deactivated in-between normal levels.
        {
            elapsedTime += Time.deltaTime;
            int minuteDigit = (int)elapsedTime / 60;
            int seconds = (int)elapsedTime - minuteDigit * 60 - extraSeconds;
                        
            secondsDigitOneSR.color = secondsDigitTwoSR.color = seconds < 0 ? Color.green : Color.black;
            seconds = Mathf.Abs(seconds);

            int secondsFirstDigit = seconds / 10;
            int secondsSecondDigit = seconds - secondsFirstDigit * 10;
            minutesDigitSR.sprite = digitsSprites[minuteDigit];
            secondsDigitOneSR.sprite = digitsSprites[secondsFirstDigit];
            secondsDigitTwoSR.sprite = digitsSprites[secondsSecondDigit];
        }

        // Update swarm.
        RaycastHit2D swarmHitInfo = Physics2D.Raycast(transform.position - new Vector3(0.0f, RAYCAST_Y_OFFSET, 0.0f), Vector2.down, Shared.LEVEL_LENGTH[scene.buildIndex], swarmLayerMask);
        if (swarmHitInfo.collider != null && swarmHitInfo.collider.tag == "Swarm")
        {
            swarmDistance = swarmHitInfo.distance;
        }

        // Update collectables.
        Collider2D collectableCollider = Physics2D.OverlapCircle(transform.position, 0.05f, collectablesLayerMask, 0.0f, 0.0f);
        if (collectableCollider != null)
        {
            GameObject collectable = collectableCollider.gameObject;
            if (collectable.tag == "Amulet" & !playerJumping)
            {
                playerAudioSource.PlayOneShot(pickupClip);
                collectable.GetComponent<SpriteRenderer>().enabled = false;
                collectable.GetComponent<Collider2D>().enabled = false;
                totalAmuletsCounter++;
                levelAmuletsCounter++;
                updateAmuletsCounter();
            }
            else if (collectable.tag == "End level")
            {
                levelEnded = true;
                EndLevel();
            }
        }

        // Handle pits and traps.
        if (pitsRayCast.collider != null & !playerJumping & !playerFalling)
        {
            if (pitsRayCast.collider.tag.Equals("Pit"))
            {
                playerFalling = true;
            } else if (pitsRayCast.collider.tag.Equals("Trap"))
            {
                Trap trap = (Trap)pitsRayCast.collider.GetComponent(typeof(Trap));
                trap.activated = true;
            }
        }

        // Scale spite when player is falling.
        if (playerFalling & updateGame)
        {   
            // Down-scale the sprite until it is practically invisible.
            if (localScaleXSign * transform.localScale.x > 0.1f)
            {
                transform.localScale -= new Vector3(localScaleXSign * 0.15f, 0.15f, 0.0f);
            } else if (!playerAudioSource.isPlaying)
            {
                gameOver = true;
                EndLevel();
            }
                        
            if (!pausedAudioSource & !playerAudioSource.isPlaying & !gameOver)
            {
                playerAudioSource.PlayOneShot(screamClip);
            } else if (pausedAudioSource & updateGame)
            {
                pausedAudioSource = false;
                playerAudioSource.UnPause();
            }
        } else if (playerFalling & !updateGame) {
            pausedAudioSource = true;
            playerAudioSource.Pause();
        }
        // Ground movement
        else if (updateGame)
        {
            // Set the default sprite.
            playerSR.sprite = playerStandS;
            //jumpTargetSR.sprite = jumpTargetS;

            transform.rotation = originalPlayerRotation;

            // Update jump target.
            if (Input.GetKey(jump) & !playerJumping)
            {
                playerSR.sprite = playerCrouchS;
                playerCrounching = true;
                jumpTargetSR.enabled = true;

                // The landing position moves forward only if there's no obstacle and the length of the jump is below the maximum.
                RaycastHit2D[] jumpTargetHitInfoUp = new RaycastHit2D[3];
                jumpTargetHitInfoUp[0] = Physics2D.Raycast(jumpTarget.transform.position + new Vector3(-RAYCAST_X_OFFSET, RAYCAST_Y_OFFSET), Vector3.up, 0.3f, obstaclesLayerMask);
                jumpTargetHitInfoUp[1] = Physics2D.Raycast(jumpTarget.transform.position + new Vector3(RAYCAST_X_OFFSET, RAYCAST_Y_OFFSET), Vector3.up, 0.3f, obstaclesLayerMask);
                jumpTargetHitInfoUp[2] = Physics2D.Raycast(jumpTarget.transform.position + new Vector3(0, RAYCAST_Y_OFFSET), Vector3.up, 0.3f, obstaclesLayerMask);

                if ((jumpTargetHitInfoUp[0].collider == null || !jumpTargetHitInfoUp[0].collider.tag.Contains("Obstacle")) & 
                    (jumpTargetHitInfoUp[1].collider == null || !jumpTargetHitInfoUp[1].collider.tag.Contains("Obstacle")) &
                    (jumpTargetHitInfoUp[2].collider == null || !jumpTargetHitInfoUp[2].collider.tag.Contains("Obstacle")) &
                    playerCrouchFrameCounter < CROUCH_ANIM_FRAMES )
                {
                    jumpTarget.transform.Translate(new Vector2(0.0f, 0.1f));
                    playerCrouchFrameCounter += 2;
                }

                // The jump target should be red if the player cannot jump.
                if (playerCrouchFrameCounter < MIN_CROUCH_ANIM_FRAMES)
                {
                    jumpTargetSR.sprite = jumpTargetRedS;
                }
                else
                {
                    jumpTargetSR.sprite = jumpTargetS;
                }
            }

            if (jumpTargetRCBack.collider != null && jumpTargetRCBack.collider.tag.Equals("Pit"))
            {
                jumpTargetSR.sprite = jumpTargetRedS;
            }

            bool finishedCrouching = Input.GetKeyUp(jump) | playerJumping | Input.GetKey(cancelJump);

            // Jump animation.
            if (finishedCrouching)
            {
                if ((playerCrouchFrameCounter >= MIN_CROUCH_ANIM_FRAMES | playerJumping))
                {
                    // Store the number of frames during which the player crouched.
                    crouchAnimFrames = playerCrouchFrameCounter > crouchAnimFrames ? playerCrouchFrameCounter : crouchAnimFrames;
                    playerCrounching = false;
                    if (playerCrouchFrameCounter > 0 & !Input.GetKey(cancelJump))
                    {
                        playerJumping = true;
                        playerSR.sprite = playerJumpS;

                        if (playerCrouchFrameCounter == crouchAnimFrames)
                        {
                            playerAudioSource.PlayOneShot(jumpClip);
                        }

                        transform.Translate(new Vector2(0.0f, 0.1f)); // The player is leaping forward while jumping.
                        if (gui.transform.position.y < Shared.LEVEL_LENGTH[scene.buildIndex] | 
                            Mathf.Abs(gui.transform.position.y - transform.position.y) > MAX_DISTANCE_FROM_GUI) // Do not move the camera and the GUI if upper border of the level has been reached.
                        {
                            mainCamera.transform.Translate(new Vector2(0.0f, 0.1f));
                            gui.transform.Translate(new Vector2(0.0f, 0.1f));
                        }

                        // The player trajectory should be parabolic. To give the impressiong of a parabola, 
                        // first upscale the player when going upward and then downscale it when going downward. 
                        if (playerCrouchFrameCounter > crouchAnimFrames / 2)
                        {
                            transform.localScale += new Vector3(localScaleXSign * 0.1f, 0.1f, 0.0f); // Sign of the scaling should alwasy be positive, hence the multiplication.  
                        }
                        else
                        {
                            transform.localScale -= new Vector3(localScaleXSign * 0.1f, 0.1f, 0.0f);
                        }

                        playerCrouchFrameCounter -= 2;
                    } else if (Input.GetKey(cancelJump))
                    {
                        playerCrouchFrameCounter = 0;
                    }
                    else
                    {
                        playerJumping = false;
                        // To eliminate rounding errors restore the original scale at the end of the jump.
                        transform.localScale = new Vector3(localScaleXSign * originalPlayerScale.x, originalPlayerScale.y, originalPlayerScale.z);
                        crouchAnimFrames = 0;
                        jumpTargetSR.enabled = false;
                    }
                }
                else
                {
                    playerCrouchFrameCounter = 0;
                    playerCrounching = false;
                    playerSR.sprite = playerStandS;
                    transform.localScale = new Vector3(localScaleXSign * originalPlayerScale.x, originalPlayerScale.y, originalPlayerScale.z);
                    jumpTargetSR.enabled = false;
                }
            }
           
            bool playerCanMove = !playerCrounching & !playerJumping;
            bool noObstacle = true; 

            // Movement:
            // For each face of the collision box there are three raycasts: two at the corners and one in the middle.
            if (Input.GetKey(moveRight) & transform.position.x < MAX_LATERAL_DISTANCE & playerCanMove)
            {
                RaycastHit2D[] playerHitInfo = new RaycastHit2D[3];
                playerHitInfo[0] = Physics2D.Raycast(transform.position + new Vector3(RAYCAST_X_OFFSET, RAYCAST_Y_OFFSET), Vector2.right, 0.1f, obstaclesLayerMask);
                playerHitInfo[1] = Physics2D.Raycast(transform.position + new Vector3(RAYCAST_X_OFFSET, -RAYCAST_Y_OFFSET), Vector2.right, 0.1f, obstaclesLayerMask);
                playerHitInfo[2] = Physics2D.Raycast(transform.position + new Vector3(RAYCAST_X_OFFSET, 0), Vector2.right, 0.1f, obstaclesLayerMask);

                int hits = 0;
                Collider2D crateCollider = null;
                for (int i = 0; i < 3; i++)
                {
                    if (playerHitInfo[i].collider != null)
                    {
                        noObstacle &= false;
                        hits = playerHitInfo[i].collider.tag.Contains("Crate") ? hits + 1 : hits;
                        crateCollider = playerHitInfo[i].collider;
                    }
                }

                crateCollider = hits < 2 ? null : crateCollider;

                if (noObstacle | hits >= 2)
                {
                    MovePlayer(Vector2.zero, new Vector3(0, 0, -90), crateCollider);
                } 
            } else 
            if (Input.GetKey(moveLeft) & transform.position.x > -MAX_LATERAL_DISTANCE & playerCanMove)
            {
                RaycastHit2D[] playerHitInfo = new RaycastHit2D[3];
                playerHitInfo[0] = Physics2D.Raycast(transform.position + new Vector3(-RAYCAST_X_OFFSET, RAYCAST_Y_OFFSET), Vector2.left, 0.1f, obstaclesLayerMask);
                playerHitInfo[1] = Physics2D.Raycast(transform.position + new Vector3(-RAYCAST_X_OFFSET, -RAYCAST_Y_OFFSET), Vector2.left, 0.1f, obstaclesLayerMask);
                playerHitInfo[2] = Physics2D.Raycast(transform.position + new Vector3(-RAYCAST_X_OFFSET, 0), Vector2.left, 0.1f, obstaclesLayerMask);

                int hits = 0;
                Collider2D crateCollider = null;
                for (int i = 0; i < 3; i++)
                {
                    if (playerHitInfo[i].collider != null)
                    {
                        noObstacle &= false;
                        hits = playerHitInfo[i].collider.tag.Contains("Crate") ? hits + 1 : hits;
                        crateCollider = playerHitInfo[i].collider;
                    }
                }

                crateCollider = hits < 2 ? null : crateCollider;

                if (noObstacle | hits >= 2)
                {
                    MovePlayer(Vector2.zero, new Vector3(0, 0, 90), crateCollider);
                }
            } else 
            if (Input.GetKey(moveUp) & playerCanMove)
            {
                RaycastHit2D[] playerHitInfo = new RaycastHit2D[3];
                playerHitInfo[0] = Physics2D.Raycast(transform.position + new Vector3(-RAYCAST_X_OFFSET, RAYCAST_Y_OFFSET), Vector2.up, 0.3f, obstaclesLayerMask);
                playerHitInfo[1] = Physics2D.Raycast(transform.position + new Vector3(RAYCAST_X_OFFSET, RAYCAST_Y_OFFSET), Vector2.up, 0.3f, obstaclesLayerMask);
                playerHitInfo[2] = Physics2D.Raycast(transform.position + new Vector3(0, RAYCAST_Y_OFFSET), Vector2.up, 0.3f, obstaclesLayerMask);

                int hits = 0;
                Collider2D crateCollider = null;
                for (int i = 0; i < 3; i++)
                {
                    if (playerHitInfo[i].collider != null)
                    {
                        noObstacle &= false;
                        hits = playerHitInfo[i].collider.tag.Contains("Crate") ? hits + 1 : hits;
                        crateCollider = playerHitInfo[i].collider;
                    }
                }

                crateCollider = hits < 2 ? null : crateCollider;

                if (noObstacle | hits >= 2)
                {
                    MovePlayer(new Vector2(0.0f, 0.1f), Vector2.zero, crateCollider);
                }
            } else 
            if (Input.GetKey(moveDown) & playerCanMove)
            {
                RaycastHit2D[] playerHitInfo = new RaycastHit2D[3];
                playerHitInfo[0] = Physics2D.Raycast(transform.position + new Vector3(-RAYCAST_X_OFFSET, -RAYCAST_Y_OFFSET), Vector2.down, 0.3f, obstaclesLayerMask);
                playerHitInfo[1] = Physics2D.Raycast(transform.position + new Vector3(RAYCAST_X_OFFSET, -RAYCAST_Y_OFFSET), Vector2.down, 0.3f, obstaclesLayerMask);
                playerHitInfo[2] = Physics2D.Raycast(transform.position + new Vector3(0, -RAYCAST_Y_OFFSET), Vector2.down, 0.3f, obstaclesLayerMask);

                int hits = 0;
                Collider2D crateCollider = null;
                for (int i = 0; i < 3; i++)
                {
                    if (playerHitInfo[i].collider != null)
                    {
                        noObstacle &= false;
                        hits = playerHitInfo[i].collider.tag.Contains("Crate") ? hits + 1 : hits;
                        crateCollider = playerHitInfo[i].collider;
                    }
                }

                crateCollider = hits < 2 ? null : crateCollider;

                if (noObstacle | hits >= 2)
                {
                    MovePlayer(new Vector2(0.0f, -0.1f), new Vector3(0, 0, 180), crateCollider);
                }
            }

            if (playerCanMove)
            {
                jumpTarget.transform.position = transform.position;
            }
        }
    }

    private void MovePlayer(Vector2 cameraTranslation, Vector3 playerRotation, Collider2D crateCollider)
    {
        float speed = 1.0f;
        bool canMove = true;
        if (crateCollider != null)
        {
            GameObject crateGO = crateCollider.gameObject;
            Crate crate = crateGO.GetComponent<Crate>();
            speed = 0.5f;
            currentSprite = currentSprite == playerMoveCrateA | currentSprite == playerMoveCrateB ?
                currentSprite : playerMoveCrateA;

            if (crate.CanBeMoved((int)playerRotation.z))
            {
                switch ((int)playerRotation.z)
                {
                    case 0:
                        crateCollider.transform.Translate(new Vector2(0.0f, 0.05f));
                        break;
                    case 90:
                        if (crateCollider.transform.position.x > -MAX_LATERAL_DISTANCE)
                        {
                            crateCollider.transform.Translate(new Vector2(-0.05f, 0.0f));
                        }
                        else
                        {
                            canMove = false;
                        }
                        break;
                    case 180:
                        crateCollider.transform.Translate(new Vector2(0.0f, -0.05f));
                        break;
                    case -90:
                        if (crateCollider.transform.position.x < MAX_LATERAL_DISTANCE)
                        {
                            crateCollider.transform.Translate(new Vector2(0.05f, 0.0f));
                        }
                        else
                        {
                            canMove = false;
                        }
                        break;
                }
            } else
            {
                canMove = false;
            }         
        }
        transform.Rotate(playerRotation); // Rotate the player in the right direciton.

        if (canMove)
        {
            transform.Translate(new Vector2(0.0f, 0.1f) * speed);
            if (gui.transform.position.y < Shared.LEVEL_LENGTH[scene.buildIndex] |
                Mathf.Abs(gui.transform.position.y - transform.position.y) > MAX_DISTANCE_FROM_GUI)
            {
                mainCamera.transform.Translate(cameraTranslation * speed);
                gui.transform.Translate(cameraTranslation * speed);
            }

            runAnimFrameCounter++;

            if (runAnimFrameCounter >= RUN_ANIM_FRAMES & crateCollider == null) 
            {
                currentSprite = playerJumpS;  // Jumpting and running use the same sprite.
                playerAudioSource.PlayOneShot(stepClip);
                // Mirror the sprite every RUN_ANIM_FRAMES.
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
               
                if (!Shared.SCENE_IS_TEMPLE[scene.buildIndex])
                {
                    Utils.GenerateDirt(transform.position, playerRotation, Mathf.Sign(transform.localScale.x), pitsRayCast.collider == null);
                }
                
                runAnimFrameCounter = 0;
            } else if (2 * RUN_ANIM_FRAMES % runAnimFrameCounter == 0 & crateCollider != null) // TRUE when player is pushing a crate
            {
                if (runAnimFrameCounter == 1 | runAnimFrameCounter == RUN_ANIM_FRAMES)
                {
                    playerAudioSource.PlayOneShot(crateMovedClip);                    
                }  else if (runAnimFrameCounter == 2 * RUN_ANIM_FRAMES) // When moving crates anim is slower, hence the factor 2
                {
                    if (currentSprite == playerMoveCrateA) // There are 2 alternating sprites when moving crates
                    {
                        currentSprite = playerMoveCrateB;
                        if (!Shared.SCENE_IS_TEMPLE[scene.buildIndex])
                        {
                            // Third argument determines dirt position offset, i.e. which foot is on the ground, therefore
                            // it alternates between two values whenever the sprite animation does
                            Utils.GenerateDirt(transform.position, playerRotation, 1, pitsRayCast.collider == null);
                        }
                    } else
                    {
                        currentSprite = playerMoveCrateA;
                        if (!Shared.SCENE_IS_TEMPLE[scene.buildIndex])
                        {
                            Utils.GenerateDirt(transform.position, playerRotation, -1, pitsRayCast.collider == null);
                        }
                    }
                    
                    runAnimFrameCounter = 0;
                }
            }
                        
            playerSR.sprite = currentSprite;
        }        
    }

    public static void updateAmuletsCounter ()
    {        
        int firstDigit = totalAmuletsCounter / 10;
        int secondDigit = totalAmuletsCounter - firstDigit * 10;
        amuletsDigitOneSR.sprite = digitsSprites[firstDigit];
        amuletsDigitTwoSR.sprite = digitsSprites[secondDigit];
    }

    public static void EndLevel()
    {
        elapsedTime = 0.0f;
        if (levelEnded)
        {
            if (!Papyrus.spriteRenderer.enabled & !Shared.SCENE_IS_TEMPLE[scene.buildIndex])
            {
                Papyrus.amuletsLeft = Shared.SCENE_AMULETS[scene.buildIndex] - levelAmuletsCounter; 
                Papyrus.timeLeft = Shared.TIME_GOAL[scene.buildIndex] - (int)Time.timeSinceLevelLoad + extraSeconds;
                extraSeconds = Papyrus.timeLeft;
                Papyrus.playAnim = true;
            } else if (Shared.SCENE_IS_TEMPLE[scene.buildIndex])
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
        else if (gameOver)
        {
            extraSeconds = 0;
            totalAmuletsCounter = 0;
            levelAmuletsCounter = 0;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
