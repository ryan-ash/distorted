using UnityEngine;

public class ScorebarController : MonoBehaviour {

    [HideInInspector]
    public int currentScore = 0;
    public Color ownColor, enemyColor, successColor;

    void Awake() {
        SetColor(ownColor);
    }

    public void UpdateScore(int delta) {
        RhythmManager.CheckUpdateRound();
        currentScore += delta;
        currentScore = Mathf.Max(0, currentScore);
        currentScore = Mathf.Min(SettingsManager.instance.maxScore, currentScore);
        float currentScorebarScale = transform.localScale.x;
        float newScorebarScale = currentScore * 1.0f / SettingsManager.instance.maxScore * SettingsManager.instance.scaleDelta + SettingsManager.instance.minScorebarScale;
        LeanTween.value(gameObject, 0f, 1f, SettingsManager.instance.scoreGrowthTime).setOnUpdate(
            (float value) => {
                transform.localScale = new Vector3(
                    Mathf.Lerp(currentScorebarScale, newScorebarScale, value),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        ).setEase(SettingsManager.instance.globalTweenConfig);

        Highlight(delta > 0);
    }

    private void Highlight(bool positive) {
        Color targetColor = positive ? successColor : enemyColor;
        SetColor(targetColor);

        LeanTween.value(gameObject, 0f, 1f, SettingsManager.instance.scoreGrowthTime).setOnUpdate(
            (float value) => {
                float redComponent = Mathf.Lerp(targetColor.r, ownColor.r, value);
                float greenComponent = Mathf.Lerp(targetColor.g, ownColor.g, value);
                float blueComponent = Mathf.Lerp(targetColor.b, ownColor.b, value);
                Color resultColor = new Color(redComponent, greenComponent, blueComponent);
                SetColor(resultColor);
            }
        );
    }

    private void SetColor(Color targetColor) {
        transform.GetChild(0).GetComponent<Renderer>().material.color = targetColor;
    }
}
