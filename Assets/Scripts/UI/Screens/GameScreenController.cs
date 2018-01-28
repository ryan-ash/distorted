using UnityEngine;

public class GameScreenController : ScreenBaseController {

    private static GameScreenController instance;

    void Start() {
        instance = this;
    }

    void OnEnable() {
            
    }
}
