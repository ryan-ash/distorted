using UnityEngine;

public class MainMenuScreenController : ScreenBaseController {

    private static MainMenuScreenController instance;

    void Start() {
        instance = this;
    }

    void OnEnable() {
            
    }

    public void ClickSinglePlayer() {
        GameManager.ChangeGameStateTo(GameState.SinglePlayer);
    }

    public void ClickMultiPlayer() {
        GameManager.ChangeGameStateTo(GameState.MultiPlayer);
    }

    public void ClickQuit() {
        Application.Quit();
    }
}
