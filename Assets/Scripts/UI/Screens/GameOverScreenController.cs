using UnityEngine;

public class GameOverScreenController : ScreenBaseController {

    private static GameOverScreenController instance;

    void Start()
    {
        instance = this;
    }

    public void Restart() {
        GameManager.doRestart = true;
        Application.LoadLevel("Main");
    }

    public void Menu() {
        Application.LoadLevel("Main");
    }
}
