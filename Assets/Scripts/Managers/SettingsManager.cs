using UnityEngine;

public class SettingsManager : MonoBehaviour {
    public LeanTweenType globalTweenConfig;

    [Header("Symbols")]
    public LeanTweenType symbolTweenConfig;
    public float symbolMoveTime = 1f;
    public float symbolRespawnTime = 1f;
    public float symbolHightlightTime = 0.5f;
    public float symbolHighlightMax = 255;

    [Header("Score")]
    public int positiveMatchScore = 10;
    public int negativeMatchScore = -10;
    public int sendScore = 5;
    public float minScorebarScale = 1f;
    public float maxScorebarScale = 27f;
    public float scoreGrowthTime = 0.2f;

    [Header("Player")]
    public int maxScore = 200;
    public float playerMoveTime = 0.05f;
    public float playerActivePassiveTime = 0.5f;
    public float playerPulsatingSwitchTime = 0.05f;
    public float playerPulseScaleUpTo = 1.5f;
    public Color activeEmissionColor = Color.grey;

    [Header("Conversion Table")]
    public float conversionTableShiftTime = 0.1f;    

    [Header("Camera")]
    public float cameraMovingTime = 3f;

    [Header("Effects")]    
    public float dustFlashTime = 0.04166602f;
    public float dustSmallFlashSize = 0.05f;
    public float dustBigFlashSize = 0.2f;
    public LeanTweenType dustFlashTweenConfig;
    public float tableHighlightTime = 0.04166602f;

    public float scaleDelta {
        get {
            return maxScorebarScale - minScorebarScale;
        }
    }

    public static SettingsManager instance;

    private void Awake() {
        instance = this;

        // symbolMoveTime = RhythmManager.instance.squareLength / 8;
        // symbolHightlightTime = RhythmManager.instance.squareLength / 16;
        // scoreGrowthTime = RhythmManager.instance.squareLength / 32;
        // playerMoveTime = RhythmManager.instance.squareLength / 128;
        // playerActivePassiveTime = RhythmManager.instance.squareLength / 8;
    }
}
