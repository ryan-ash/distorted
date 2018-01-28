using UnityEngine;

public class RhythmManager : MonoBehaviour {
    public GameObject symbols;
    public float sendTimer = 0.5f;
    public float squareLength = 5333.25f / 1000f;

    [Header("Movement Anchors")]
    public Transform P1Target;
    public Transform P2Target;
    public Transform PreP1Conversion;
    public Transform PreP2Conversion;

    [Header("Scorebars")]
    public ScorebarController P1Scorebar;
    public ScorebarController P2Scorebar;

    public static RhythmManager instance;

    private int currentRound = 0;
    private int nextRound = 1;
    private int prevRound = 1;
    private int scoreToNextRound = 0;
    private int scoreToPrevRound = 0;
    private float currentSendTimer = 0f;
    private float currentLoopTimer = 0f;
    private bool ignoreScore = false;
    private bool roundChanged = false;
    private bool isGameOver = false;

    private float currentRoundLength;

    void Start() {
        instance = this;
        sendTimer = squareLength / 4;
    }

    private void Update() {
        if (isGameOver || !GameManager.gameStarted)
            return;

        if (currentSendTimer <= 0f) {
            currentSendTimer += sendTimer;
            symbols.BroadcastMessage("StartMoving", SendMessageOptions.DontRequireReceiver);
        }
        currentSendTimer -= Time.deltaTime;

        if (currentLoopTimer <= 0f) {
            Round();
        }
        currentLoopTimer -= Time.deltaTime;

        if (Input.GetKey("]"))
            nextRound = 8;
    }

    public static void CheckUpdateRound() {
        if (instance.ignoreScore || instance.roundChanged || instance.isGameOver)
            return;
        
        int maxScore = Mathf.Max(instance.P1Scorebar.currentScore, instance.P2Scorebar.currentScore);
        if (maxScore >= instance.scoreToNextRound) {
            instance.roundChanged = true;            
            instance.nextRound++;
        }
        if (maxScore <= instance.scoreToPrevRound) {
            instance.roundChanged = true;            
            instance.nextRound = instance.prevRound;
        }
    }

    public void Round() {        
        currentRound = nextRound;
        roundChanged = false;
        switch(currentRound) {
            case 1:
                SettingsManager.instance.playerActivePassiveTime = squareLength / 4;
                SettingsManager.instance.symbolMoveTime = squareLength / 4;
                nextRound = 2;
                ignoreScore = true;
                currentRoundLength = squareLength * 4;
                break;
            case 2:
                SettingsManager.instance.playerActivePassiveTime = squareLength / 8;
                SettingsManager.instance.symbolMoveTime = squareLength / 4;
                nextRound = 2;
                prevRound = 2;
                scoreToNextRound = 40;
                scoreToPrevRound = 0;
                ignoreScore = false;
                currentRoundLength = squareLength * 2;
                break;
            case 3:
                SettingsManager.instance.symbolMoveTime = squareLength / 8;
                nextRound = 3;
                prevRound = 2;
                scoreToNextRound = 80;
                scoreToPrevRound = 30;
                ignoreScore = false;
                currentRoundLength = squareLength * 2;
                break;
            case 4:
                SettingsManager.instance.playerActivePassiveTime = squareLength / 8;
                SettingsManager.instance.symbolMoveTime = squareLength / 8;
                nextRound = 4;
                prevRound = 3;
                scoreToNextRound = 120;
                scoreToPrevRound = 60;
                ignoreScore = false;
                currentRoundLength = squareLength * 2;
                break;
            case 5:
                SettingsManager.instance.playerActivePassiveTime = squareLength / 16;
                nextRound = 6;
                ignoreScore = true;
                currentRoundLength = squareLength * 5;
                break;
            case 6:
                SettingsManager.instance.symbolMoveTime = squareLength / 16;
                nextRound = 6;
                prevRound = 4;
                scoreToNextRound = 160;
                scoreToPrevRound = 120;
                ignoreScore = false;
                currentRoundLength = squareLength * 2;
                break;
            case 7:
                nextRound = 7;
                prevRound = 6;
                scoreToNextRound = 200;
                scoreToPrevRound = 160;
                ignoreScore = false;
                currentRoundLength = squareLength * 2;
                break;
            case 8:
                isGameOver = true;
                CameraSetController.MoveToPlayer(P1Scorebar.currentScore > P2Scorebar.currentScore);
                GameManager.ChangeGameStateTo(GameState.GameOver);
                break;
        }
        currentLoopTimer += currentRoundLength;
        currentSendTimer = 0f;
        AudioManager.instance.PlaySound("Distorted" + currentRound.ToString());
    }
}
