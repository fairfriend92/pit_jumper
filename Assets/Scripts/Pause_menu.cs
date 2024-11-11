using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {

    public static bool openMenu = false, 
        closeMenu = false,
        animatingMenu = false,
        loadSuccess = false,
        noSaveData = false;
    private static readonly int MENU_BACK_ANIM_LENGTH = 120,
        PAUSE_MENU_INTERVAL = 60,
        SELECTED_WORD_INTERVAL = 40,
        FPS = 30;
    private static int pauseSymbolAnimCounter = 0,
        selWrdAnimCounter = 0;
    private static GameObject menuBackgroundGO = null,
        pauseSymbolGO = null;
    private static SpriteRenderer menuBackgroundSR = null,
        pauseSymbolSR = null;
    private static Sprite menuBackgroundS = null,
        pauseSymbolS = null,
        playSymbolS = null,
        loadSymbolS = null;    
    private static GameObject[] pauseMenuText = null;
    private static List<Utils.Word> optionsWords = new List<Utils.Word>(3);
    private static Utils.Word resumeWord = null,
        loadWord = null,
        exitWord = null;
    private static Color tvGreen = new Color(0.3f, 1.0f, 0.0f, 1.0f);

    // Use this for initialization
    void Start () {        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static void Init()
    {
        menuBackgroundS = Resources.Load<Sprite>("Sprites/Pause Menu/menu_background");
        pauseSymbolS = Resources.Load<Sprite>("Sprites/Pause Menu/pause_symbol");
        playSymbolS = Resources.Load<Sprite>("Sprites/Pause Menu/play_symbol");
        loadSymbolS = Resources.Load<Sprite>("Sprites/Pause Menu/backwards_symbol");
    }

    public static void UpdateMenu()
    {
        animatingMenu = openMenu | closeMenu;

        // If the menu is being opened...
        if (openMenu)
        {
            // If this is the 1st frame of the opening animation...
            if (pauseSymbolAnimCounter == 0)
            {
                if (Utils.Load() == Utils.FAILURE)
                {
                    noSaveData = true;
                } else
                {
                    noSaveData = false;
                }
                menuBackgroundGO = new GameObject("Pause menu background");
                menuBackgroundGO.AddComponent<SpriteRenderer>();
                menuBackgroundGO.transform.parent = Player.gui.transform;
                menuBackgroundGO.transform.localScale = new Vector3(0.4f, 0.4f, 0.0f);
                menuBackgroundGO.transform.localPosition = new Vector3(0.0f, -1.25f, 0.0f);

                pauseSymbolGO = new GameObject("Pause symbol");
                pauseSymbolGO.AddComponent<SpriteRenderer>();
                pauseSymbolGO.transform.parent = Player.gui.transform;
                pauseSymbolGO.transform.localScale = new Vector3(5f, 5f, 0.0f);
                pauseSymbolGO.transform.localPosition = new Vector3(-1.5f, -0.2f, 0.0f);

                menuBackgroundSR = menuBackgroundGO.GetComponent<SpriteRenderer>();
                menuBackgroundSR.sortingLayerName = "GUI";
                menuBackgroundSR.sprite = menuBackgroundS;
                menuBackgroundSR.color = new Color(1, 1, 1, (float)FPS / MENU_BACK_ANIM_LENGTH);

                pauseSymbolSR = pauseSymbolGO.GetComponent<SpriteRenderer>();
                pauseSymbolSR.sortingLayerName = "GUI";

                // GUI layer sorting order
                //  1 - GUI
                //  2 - GUI text
                //  3 - pause menu scanlines
                //  4 - pause menu text and symbols
                menuBackgroundSR.sortingOrder = Player.gui.GetComponent<SpriteRenderer>().sortingOrder + 2;
                pauseSymbolSR.sortingOrder = menuBackgroundSR.sortingOrder + 1;

                pauseSymbolSR.sprite = pauseSymbolS;
                pauseSymbolSR.enabled = true; // Choose false if symbol should be pulsing
            }
            // If the animation has not ended, update background every second
            else if (pauseSymbolAnimCounter < MENU_BACK_ANIM_LENGTH-FPS & pauseSymbolAnimCounter % FPS == 0)
            {
                menuBackgroundSR.color += new Color(0, 0, 0, (float)FPS / MENU_BACK_ANIM_LENGTH);
            }
            // If the animation has ended...
            else if (pauseSymbolAnimCounter == MENU_BACK_ANIM_LENGTH-FPS)
            {
                openMenu = false;
                pauseSymbolAnimCounter = PAUSE_MENU_INTERVAL-1;
            }
        }

        // If the menu is neither being closed nor opened...
        if (!animatingMenu)
        {
            // Change selected option when moving in menu
            if (Input.GetKeyDown(Bindings.pause))
            {
                closeMenu = true;
                pauseSymbolAnimCounter = 0;
                selWrdAnimCounter = 0;
            } else if (Input.GetKeyDown(Bindings.moveDown))
            {
                if (resumeWord != null && resumeWord.selected)
                {
                    resumeWord.DeSelect(tvGreen, tvGreen);
                    if (noSaveData)
                    {
                        exitWord.DeSelect(tvGreen, tvGreen);
                    } else
                    {
                        loadWord.DeSelect(tvGreen, tvGreen);
                    }
                    selWrdAnimCounter = 0;
                } else if (loadWord != null && loadWord.selected)
                {
                    loadWord.DeSelect(tvGreen, tvGreen);
                    exitWord.DeSelect(tvGreen, tvGreen);
                    selWrdAnimCounter = 0;
                }
            } else if (Input.GetKeyDown(Bindings.moveUp))
            {
                if (loadWord != null && loadWord.selected)
                {                    
                    loadWord.DeSelect(tvGreen, tvGreen);
                    resumeWord.DeSelect(tvGreen, tvGreen);
                    selWrdAnimCounter = 0;
                }
                else if (exitWord != null && exitWord.selected)
                {                    
                    exitWord.DeSelect(tvGreen, tvGreen);
                    if (noSaveData)
                    {
                        resumeWord.DeSelect(tvGreen, tvGreen);
                    } else
                    {
                        loadWord.DeSelect(tvGreen, tvGreen);

                    }
                    selWrdAnimCounter = 0;
                }
            } else if (Input.GetKeyDown(Bindings.jump))
            {
                if (resumeWord != null && resumeWord.selected)
                {
                    closeMenu = true;
                    pauseSymbolAnimCounter = 0;
                    selWrdAnimCounter = 0;
                }

                if (loadWord != null && loadWord.selected)
                {
                    int result = Utils.Load();
                    if (result == Utils.FAILURE)
                    {
                        print("Failed to load saved data");
                    } else
                    {
                        closeMenu = true;
                        loadSuccess = true;                  
                    }
                }

                if (exitWord != null && exitWord.selected)
                {
                    Application.Quit();
                }
            }

            // Make the pause symbol flash
            if (pauseSymbolAnimCounter % PAUSE_MENU_INTERVAL  == 0)
            {
                //pauseSymbolSR.enabled = !pauseSymbolSR.enabled; // Un-comment if symbol should be pulsing
                pauseSymbolAnimCounter = 0;
            }

            // Make the selected word flash
            if (selWrdAnimCounter % SELECTED_WORD_INTERVAL == 0)
            {
                if (resumeWord != null && resumeWord.selected)
                {
                    resumeWord.Blink();
                }
                else if (loadWord != null && loadWord.selected)
                {
                    loadWord.Blink();
                }
                else if (exitWord != null && exitWord.selected)
                {
                    exitWord.Blink();
                }

                selWrdAnimCounter = 0;
            }

            string[] textLines = new string[]
            {
                "RESUME", "LOAD", "EXIT"
            };

            // If the options of the pause menu have not been created yet...
            if (pauseMenuText == null)
            {
                pauseMenuText = Utils.DisplayText(textLines,
                    new Vector3(-0.4f, -1.05f, 0),
                    new Vector3(0.5f, 0.5f, 0), 
                    1.0f, true, Utils.PIXEL_FONT, tvGreen);

                optionsWords.Clear();
                resumeWord = Utils.FindWord(pauseMenuText, "RESUME");
                optionsWords.Add(resumeWord);
                loadWord = Utils.FindWord(pauseMenuText, "LOAD");
                if (noSaveData)
                {
                    loadWord.Paint(tvGreen - new Color(0.0f, 0.0f, 0.0f, 0.75f));
                }
                optionsWords.Add(loadWord);
                exitWord = Utils.FindWord(pauseMenuText, "EXIT");
                optionsWords.Add(exitWord);

                resumeWord.DeSelect(tvGreen, tvGreen);
            }   
        }

        // If the menu is being closed...
        if (closeMenu)
        {
            pauseSymbolSR.enabled = true;
            pauseSymbolSR.sprite = loadSuccess ? loadSymbolS : playSymbolS;
            // If the options have not been destroyed yet...
            if (pauseMenuText != null)
            {
                foreach (GameObject word in pauseMenuText)
                {
                    word.GetComponent<SpriteRenderer>().enabled = false;
                    Object.Destroy(word);
                }
                pauseMenuText = null;
            }    
            
            // If the background has not disappeared yet...
            if (pauseSymbolAnimCounter % FPS == 0 & menuBackgroundSR.color.a > 0.0f)
            {
                menuBackgroundSR.color -= new Color(0, 0, 0, (float)FPS / MENU_BACK_ANIM_LENGTH);
            }
            // If the background has disappeared 
            else if (menuBackgroundSR.color.a == 0.0f)
            {  
                pauseSymbolAnimCounter = 0;
                selWrdAnimCounter = 0;
                closeMenu = false;
                Player.gamePaused = false;

                Object.Destroy(menuBackgroundGO);
                Object.Destroy(pauseSymbolGO);

                if (loadSuccess)
                {
                    loadSuccess = false;
                    Player.livesCounter = Utils.saveData.lives;
                    Player.totalAmuletsCounter = Utils.saveData.amulets;
                    Player.extraSeconds = Utils.saveData.extraSeconds;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(Utils.saveData.level);
                }
            }
        }

        if (Player.gamePaused)
        {
            pauseSymbolAnimCounter++;
            selWrdAnimCounter++;
        }
    } 

}
