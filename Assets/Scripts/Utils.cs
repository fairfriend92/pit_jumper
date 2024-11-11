using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Utils : MonoBehaviour {
    // Hashtable containing sprites that represent all the letters of the alphabet,
    // the 10 digits and various symbols.
    private static Hashtable lettersHashTable = new Hashtable(38),
        lettersPixelStyleHT = new Hashtable(25);
    public static string lan = "EN";
    private static List<string> languages = new List<string>() { "EN", "IT" };
    private static Queue<GameObject> dirtBuffer = new Queue<GameObject>(7), 
        footprintsBuffer = new Queue<GameObject>(13);
    // The keys of the dictionary are concatenated strings: english_string + lan
    // The value is the English string translated. 
    private static Dictionary<string, string> stringsDictionary = new Dictionary<string, string>();

    public static readonly float CHR_SPACE = 0.06f,
        LINE_SPACE = 0.075f;
    public const int STANDARD_FONT = 0,
        PIXEL_FONT = 1, 
        SUCCESS = 1,
        FAILURE = 0;
    
    private static List<Sprite> lettersSprites,
        lettersPixelStyleS,
        digitsSprites,
        symbolsSprites;

    private static Sprite dirtSprite,
        footprintSprite;

    public static SaveData saveData;

    public static bool initialized = false;
    
    // Initialization method that must be called manually.
    public static void Init()
    {
        List<string> englishStrings = new List<string>();
        foreach (string lan in languages)
        {
            string filePath = "Assets/Resources/Text/" + lan + ".txt";
            StreamReader streamReader = new StreamReader(filePath);
            string line;
            int lineNumber = 0;
            while ((line = streamReader.ReadLine()) != null)
            {
                if (lan == "EN")
                {
                    englishStrings.Add(line);
                    stringsDictionary.Add(line + lan, line);
                } else
                {
                    string englishString = englishStrings[lineNumber];                   
                    stringsDictionary.Add(englishString + lan, line);
                    lineNumber++;
                }                
            }
            streamReader.Close();
        }

        lettersSprites = new List<Sprite>(Resources.LoadAll<Sprite>("Sprites/Letters"));
        lettersPixelStyleS = new List<Sprite>(Resources.LoadAll<Sprite>("Sprites/Letters pixel"));
        foreach (Sprite letter in lettersSprites)
        {            
            lettersHashTable.Add(letter.name, letter);
        }
        foreach (Sprite letterPixelStyle in lettersPixelStyleS)
        {
            lettersPixelStyleHT.Add(letterPixelStyle.name, letterPixelStyle);
        }

        digitsSprites = new List<Sprite>(Resources.LoadAll<Sprite>("Sprites/Digits"));
        foreach (Sprite digit in digitsSprites)
        {
            lettersHashTable.Add(digit.name, digit);
        }

        symbolsSprites = new List<Sprite>(Resources.LoadAll<Sprite>("Sprites/Symbols"));
        foreach (Sprite symbol in symbolsSprites)
        {
            lettersHashTable.Add(symbol.name, symbol);
        }

        dirtSprite = Resources.Load<Sprite>("Sprites/dirt");
        footprintSprite = Resources.Load<Sprite>("Sprites/footprint");        

        initialized = true;
    }
    
    public static void GenerateDirt (Vector3 position, Vector3 playerRotation, float localScaleXSign, bool generateFootprint)
    {
        GameObject dirtGO = new GameObject("Dirt");
        GameObject footprintGO = new GameObject("Footprint");

        // Contains the displacement of the dirt sprite, that depends on
        // which leg is moving forward
        Vector3 offset; 

        // The displacement is orthogonal to the axis alongside which the
        // player runs, hence when moving left and right the displacemnt is on the y axis
        if (playerRotation.z == 0.0f | playerRotation.z == 180.0f)
        {
            offset = new Vector3(localScaleXSign * 0.25f, 0, 0);
        } else
        {
            offset = new Vector3(0, localScaleXSign * 0.25f, 0);
        }

        dirtGO.transform.position = footprintGO.transform.position = position + offset;
        footprintGO.transform.Rotate(playerRotation);
        dirtGO.transform.localScale = new Vector3(8, 8, 0);       
        footprintGO.transform.localScale = dirtGO.transform.localScale * 0.75f;     

        SpriteRenderer dirtSR = dirtGO.AddComponent<SpriteRenderer>();
        SpriteRenderer footprintSR = footprintGO.AddComponent<SpriteRenderer>();
        dirtSR.sortingLayerName = footprintSR.sortingLayerName = "Background";
        dirtSR.sortingOrder = footprintSR.sortingOrder = 1;
        dirtSR.sprite = dirtSprite;
        if (generateFootprint)
        {
            footprintSR.sprite = footprintSprite; 
        }        

        dirtGO.AddComponent<Dirt>();
        if (dirtBuffer.Count == 6)
        {
            Object.Destroy(dirtBuffer.Peek());
            dirtBuffer.Dequeue();
        }
        dirtBuffer.Enqueue(dirtGO);

        if (footprintsBuffer.Count == 12)
        {
            Object.Destroy(footprintsBuffer.Peek());
            footprintsBuffer.Dequeue();
        }
        footprintsBuffer.Enqueue(footprintGO);
    }

    private static string TranslateText(string text)
    {
        string translatedText;
        try
        {
            translatedText = stringsDictionary[text + lan];
        }
        catch (KeyNotFoundException)
        {
            translatedText = text;
        }

        return translatedText;
    }

    public class Word
    {
        public bool selected = false;
        List<GameObject> letters; 

        public Word(string word, List<GameObject> wordObjs)
        {
            letters = wordObjs;
        }

        public void Paint(Color color)
        {
            foreach (GameObject letter in letters)
            {
                SpriteRenderer letterSR = letter.GetComponent<SpriteRenderer>();
                letterSR.enabled = true;
                letterSR.color = color;
            }
        }

        public void Blink()
        {
            foreach (GameObject letter in letters)
            {
                SpriteRenderer letterSR = letter.GetComponent<SpriteRenderer>();
                letterSR.enabled = !letterSR.enabled;
            }
        }

        // The method changes the state of the word from selected to
        // deselected and vice versa. 
        public void DeSelect(Color oldColor, Color newColor)
        {
            selected = !selected;
            if (selected)
            {
                Paint(newColor);
            }
            else
            {
                Paint(oldColor);
            }
        }
    } 
       
    public static Word FindWord(GameObject[] textObjs, string word)
    {
        word = TranslateText(word);
        List<GameObject> lettersLst = new List<GameObject>(word.Length);
        char[] wordChrs = word.ToCharArray();

        int i = 0;
        for (int j = 0; j < textObjs.Length & i < word.Length; j++)
        {
            SpriteRenderer objSR = textObjs[j].GetComponent<SpriteRenderer>();

            string letter = objSR.sprite != null ? objSR.sprite.name : "null";

            if (letter == wordChrs[i].ToString())
            {
                lettersLst.Add(textObjs[j]);
                i++;
            }
            else
            {
                lettersLst.Clear();
                i = 0;
            }
        }

        return new Word(word, lettersLst);
    }
          
	private static Sprite[] StringToSprites(string text, int font)
    {
        char[] textCharsArray = text.ToCharArray();
        Sprite[] textSprites = new Sprite[textCharsArray.Length];

        for (int i = 0; i < textCharsArray.Length; i++)
        {
            if (textCharsArray[i].ToString() == "/")
            {
                switch (textCharsArray[i + 1].ToString())
                {
                    case "/":
                        textSprites[i] = (Sprite)lettersHashTable["slash"];
                        break;
                    case "a":
                        textSprites[i] = (Sprite)lettersHashTable["amulet"];
                        break;
                    case "t":
                        textSprites[i] = (Sprite)lettersHashTable["time"];
                        break;
                }

                i++;
            } 
            else
            {
                switch (font)
                {
                    case STANDARD_FONT:
                        // The space character is represented by a NULL object
                        textSprites[i] = (Sprite)lettersHashTable[textCharsArray[i].ToString()];
                        break;
                    case PIXEL_FONT:
                        textSprites[i] = (Sprite)lettersPixelStyleHT[textCharsArray[i].ToString()];
                        break;
                }
                
            }
        }

        return textSprites;
    }

    public static GameObject[] DisplayTextLine(string text, Vector3 position, 
        Vector3 scale, float maxLength, bool centering, int font, Color color)
    {   
        Sprite[] textSprites = StringToSprites(TranslateText(text), font);

        float maxChrSpace = CHR_SPACE * scale.x / 0.25f; // 0.25 is the default size of the characters

        float textLength = maxChrSpace * textSprites.Length;
        float chrSpace = maxChrSpace;
        if (textLength > maxLength)
        {
            chrSpace = maxLength / textSprites.Length;
            scale.x = scale.x * chrSpace / maxChrSpace;
        }

        if (centering)
        {
            textLength = chrSpace * textSprites.Length;
            float padding = (maxLength - textLength) / 2.0f;         
            position.x += padding;
        }

        SpriteRenderer[] textSRs = new SpriteRenderer[textSprites.Length];
        SpriteRenderer guiSR = Player.gui.GetComponent<SpriteRenderer>();
        GameObject[] textGOs = new GameObject[textSprites.Length];

        for (int i = 0; i < textSprites.Length; i++)
        {
            textGOs[i] = new GameObject();
            textGOs[i].name = "text";
            textGOs[i].transform.parent = Player.gui.transform;
            textGOs[i].transform.localPosition = position;
            textGOs[i].transform.localScale = scale;

            textSRs[i] = textGOs[i].AddComponent<SpriteRenderer>();

            textSRs[i].sprite = textSprites[i];

            // Symbols Sprites shouldn't be coloured as they already have 
            // their own colours.
            if (!symbolsSprites.Contains(textSprites[i]))
            {
                textSRs[i].color = color;
            }

            textSRs[i].sortingLayerID = guiSR.sortingLayerID;
            textSRs[i].sortingOrder = guiSR.sortingOrder + 1;
            // Pixel font is used for the pause menu,
            // which sits on top of main screen             
            if (font == PIXEL_FONT)
            {
                textSRs[i].sortingOrder += 2;   // +1 for the GUI text and +1 for the scanlines 
                                                // of the pause menu
            }

            position.x += chrSpace;
        }

        return textGOs;
    }

    public static GameObject[] DisplayText(string[] textLines, Vector3 position,
        Vector3 scale, float maxLength, bool centering, int font, Color color)
    {
        float yOffset = 0;
        GameObject[] textObjs = new GameObject[0];
        foreach (string textLine in textLines)
        {
            GameObject[] tmpArray = Utils.DisplayTextLine(textLine,
                position - new Vector3(0, yOffset, 0), scale, maxLength, centering, font, color);

            // The GameObjects that represent the characters of the text to be displayed are stored
            // in an array. For each new line the array must be resized. 
            GameObject[] newArray = new GameObject[textObjs.Length + tmpArray.Length];
            System.Array.Copy(textObjs, 0, newArray, 0, textObjs.Length);
            System.Array.Copy(tmpArray, 0, newArray, textObjs.Length, tmpArray.Length);
            textObjs = newArray;

            yOffset += Utils.LINE_SPACE * scale.y / 0.25f; // 0.25 is the default size of the characters
        }

        return textObjs;
    } 

    public static void DeleteText()
    {
        foreach (Transform child in Player.gui.GetComponentsInChildren<Transform>())
        {
            GameObject childGO = child.gameObject;
            if (childGO.name.Contains("text"))
            {
                Object.Destroy(childGO);
            }            
        }
    }

    public static int Save()
    {
        saveData = new SaveData();
        string filePath = Application.persistentDataPath + "\\save_data.dat";
        
        using (Stream stream = File.Open(filePath, FileMode.Create))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            try
            {
                binaryFormatter.Serialize(stream, saveData);
            } catch (System.Runtime.Serialization.SerializationException e)
            {
                print("Utils.Save(): serialization exception");
                return FAILURE;
            } catch (System.Security.SecurityException e)
            {
                print("Utils.Save(): security exception");
                return FAILURE;
            }
        }

        return SUCCESS;
    }

    public static int Load()
    {
        string filePath = Application.persistentDataPath + "\\save_data.dat";

        if (!File.Exists(filePath))
        {
            return FAILURE;
        } else
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                try
                {
                    saveData = (SaveData) binaryFormatter.Deserialize(stream);
                }
                catch (System.ArgumentNullException e)
                {
                    print("Utils.Load(): argument null exception");
                    return FAILURE;
                }
                catch (System.Runtime.Serialization.SerializationException e)
                {
                    print("Utils.Load(): serialization exception");
                    return FAILURE;
                }
                catch (System.Security.SecurityException e)
                {
                    print("Utils.Load(): security exception");
                    return FAILURE;
                }
            }
        }

        return SUCCESS;
    }

    public static GameObject CreateSaveMenu()
    {
        Player.saving = true; // Signals the Player class that the update should stop.
        GameObject saveMenuGO = new GameObject();
        saveMenuGO.transform.parent = Player.gui.transform;
        saveMenuGO.transform.localPosition = new Vector3(0, -1.1f, 0);
        saveMenuGO.transform.localScale = new Vector3(2.0f, 2.0f, 0.0f);
        saveMenuGO.AddComponent<SpriteRenderer>();
        SpriteRenderer saveMenuSR = saveMenuGO.GetComponent<SpriteRenderer>();
        saveMenuSR.sortingLayerName = "GUI";
        return saveMenuGO;
    }
}
