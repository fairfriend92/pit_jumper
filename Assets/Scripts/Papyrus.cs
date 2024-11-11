using UnityEngine;
using UnityEngine.SceneManagement;

public class Papyrus : MonoBehaviour {
    // Flag that is set when the Papyrus is playing the unrolling animation.
    public static bool playAnim = false;

    // Fields that are meant to be filled by the classes that invoke 
    // the methods of Papyrus. 
    public static int amuletsLeft, timeLeft;

    private KeyCode moveUp = Bindings.moveUp,
       moveDown = Bindings.moveDown,
       jump = Bindings.jump;

    public static SpriteRenderer spriteRenderer;

    private int frameCounter = 0, // Counts the frame during which a given sprite is displayed.
        spriteCounter = 0; // Counts the sprite currently displayed. 

    private static readonly int ANIM_FRAMES = 10, // How many frames a sprite of the animation should be displayed for.
        ANIM_SPRITES = 6; // How many sprites the animation is made of.

    private Sprite[] animSprites = new Sprite[ANIM_SPRITES];

    // References to the selectable words that appear in the Papyrus. 
    private Utils.Word continueWord, restartWord;

    void Start () {
		animSprites = Resources.LoadAll<Sprite>("Sprites/Papyrus Scroll");
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
	
	void Update () {
		if (playAnim)
        {
            spriteRenderer.enabled = true;

            // The animation is composed of ANIM_SPRITES Sprites. 
            if (spriteCounter < ANIM_SPRITES)
            {
                // Each Sprite should be displayed for ANIM_FRAMES before being swapped for a new one.
                if (frameCounter == ANIM_FRAMES)
                {
                    frameCounter = 0;
                    spriteRenderer.sprite = animSprites[spriteCounter];
                    spriteCounter++;
                }
                else
                {
                    frameCounter++;
                }
            }
            // Animation ended, display text.
            else
            {
                playAnim = false;
                GameObject[] textObjs = ShowEndLevelText();
                continueWord = Utils.FindWord(textObjs, "CONTINUE");
                restartWord = Utils.FindWord(textObjs, "RESTART");
                continueWord.DeSelect(Color.black, Color.red);
            }
        } 
        else if (spriteRenderer.enabled & !Player.gamePaused)
        {
            if (Input.GetKeyDown(moveUp) | Input.GetKeyDown(moveDown))
            {
                continueWord.DeSelect(Color.black, Color.red);
                restartWord.DeSelect(Color.black, Color.red);
            }

            if (Input.GetKeyDown(jump))
            {
                if (restartWord.selected)
                {
                    Player.extraSeconds = 0;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                } 
                else if (continueWord.selected)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
            }
        }
	}

    GameObject[] ShowEndLevelText()
    {
        timeLeft = timeLeft > 0 ? timeLeft : 0;

        string medal = "NONE";

        if (amuletsLeft == 0 & timeLeft >= 0)
        {
            // The text parser use the a and t to identify the icons 
            // for the amulets and time medals.
            medal = "/a /t";
        }
        else if (amuletsLeft == 0)
        {
            medal = "/a";
        }
        else if (timeLeft > 0)
        {
            medal = "/t";
        }

        string[] textLines = new string[] { "AMULETS LEFT",
            amuletsLeft.ToString(),
            "SECONDS LEFT",
            timeLeft.ToString(),
            "",
            "MEDALS",
            medal,
            "",
            "RESTART",
            "CONTINUE" };

        GameObject[] textObjs = Utils.DisplayText(textLines,
                new Vector3(-0.325f, -1.05f, 0),
                new Vector3(0.25f, 0.25f, 0), 
                0.7f, false, Utils.STANDARD_FONT, Color.black);

        return textObjs;
    }
}
