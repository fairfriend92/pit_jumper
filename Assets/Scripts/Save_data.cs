using UnityEngine.SceneManagement;

[System.Serializable]

public class SaveData {
    public int lives,
        amulets,
        extraSeconds,
        level;

    public SaveData()
    {
        lives = Player.livesCounter;
        amulets = Player.totalAmuletsCounter;
        extraSeconds = Player.extraSeconds;
        level = SceneManager.GetActiveScene().buildIndex;        
    }
}
