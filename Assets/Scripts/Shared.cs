using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shared : MonoBehaviour {
    public static readonly int NUMBER_OF_LEVELS = 10;
    public static readonly int[] SCENE_AMULETS = new int[NUMBER_OF_LEVELS],
        TIME_GOAL = new int[NUMBER_OF_LEVELS];
    public static readonly float[] LEVEL_LENGTH = new float[NUMBER_OF_LEVELS];
    public static readonly bool[] SCENE_IS_TEMPLE = new bool[NUMBER_OF_LEVELS];

    // Manual initialization.
    public static void Init()
    {
        SCENE_AMULETS[0] = 18; TIME_GOAL[0] = 9; LEVEL_LENGTH[0] = 24; SCENE_IS_TEMPLE[0] = false;
        SCENE_AMULETS[1] = 10; TIME_GOAL[1] = 10; LEVEL_LENGTH[1] = 24; SCENE_IS_TEMPLE[1] = false;
        SCENE_AMULETS[2] = 10; TIME_GOAL[2] = 10; LEVEL_LENGTH[2] = 24; SCENE_IS_TEMPLE[2] = false;
        SCENE_AMULETS[3] = 0; TIME_GOAL[3] = 0; LEVEL_LENGTH[3] = 4.5f; SCENE_IS_TEMPLE[3] = true;
        SCENE_AMULETS[4] = 12; TIME_GOAL[4] = 11; LEVEL_LENGTH[4] = 24f; SCENE_IS_TEMPLE[4] = false;
        SCENE_AMULETS[5] = 14; TIME_GOAL[5] = 10; LEVEL_LENGTH[5] = 24f; SCENE_IS_TEMPLE[5] = false;
    }

    // Use this for initialization
    void Start () {
       
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
