using UnityEngine;

public class ScorebarController : MonoBehaviour {

    [HideInInspector]
    public int currentScore = 0;

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
        ).setEase(SettingsManager.instance.globalTweenConfig);;
    }
}
