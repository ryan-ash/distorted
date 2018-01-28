using UnityEngine;

public class SymbolController : MonoBehaviour {
    
    public FontAwesome3D iconHolder;
    public GameObject highlightAnchor;
    public float flashLength = 0.1f;

    [HideInInspector]
    public bool almostDead = false;

    private bool aimedAtPlayerOne = false;

    private Transform targetPrePlayer, targetPlayer;
    private Vector3 movementFrom;
    private Quaternion rotationFrom;
    private PlayerNumber targetPlayerNumber;
    private float currentHighlightTimer;

    private bool markedToMove = false;

    public void SetRandom() {
        iconHolder.SetRandomIcon();
        Fade(0f, 1f);
    }

    public void SetExact(string name) {
        iconHolder.UpdateIcon(name);
    }

    public void AimAtPlayer(bool aimAtPlayerOne) {
        aimedAtPlayerOne = aimAtPlayerOne;
        targetPlayerNumber = (aimAtPlayerOne) ? PlayerNumber.One : PlayerNumber.Two;
    }

    public void MarkToMove() {
        Highlight();
        markedToMove = true;
    }

    private void Update() {
        if (currentHighlightTimer > 0f) {
            currentHighlightTimer -= Time.deltaTime;
            iconHolder.color.g = SettingsManager.instance.symbolHighlightMax * (currentHighlightTimer / SettingsManager.instance.symbolHightlightTime);
        }
    }

    public void StartMoving() {
        if (!markedToMove)
            return;
        
        markedToMove = false;
        targetPrePlayer = (aimedAtPlayerOne) ? RhythmManager.instance.PreP1Conversion : RhythmManager.instance.PreP2Conversion;
        targetPlayer = (aimedAtPlayerOne) ? RhythmManager.instance.P1Target : RhythmManager.instance.P2Target;
        movementFrom = transform.position;
        rotationFrom = transform.rotation;
        LeanTween.value(gameObject, 0f, 1f, SettingsManager.instance.symbolMoveTime).setOnUpdate( 
            (float value) => {
                transform.position = Vector3.Lerp(movementFrom, targetPrePlayer.position, value);
            }
        ).setOnComplete(MoveToTargetPlayer).setEase(SettingsManager.instance.symbolTweenConfig);
    }

    public void Reveal() {
        Fade(0f, 1f);
    }

    public void Fade(float from, float to, bool dieAtTheEnd = false) {
        if (iconHolder.color.a == to)
            return;

        almostDead = dieAtTheEnd;
        
        LeanTween.value(gameObject, from, to, flashLength).setOnUpdate(iconHolder.ChangeAlpha).setOnComplete(
            () => {
                if (dieAtTheEnd) {
                    Destroy(gameObject);
                }
            }
        ).setEase(SettingsManager.instance.globalTweenConfig);
    }

    public void Highlight() {
        currentHighlightTimer = SettingsManager.instance.symbolHightlightTime;
    }

    private void MoveToTargetPlayer() {
        movementFrom = transform.position;
        rotationFrom = transform.rotation;
        LeanTween.value(gameObject, 0f, 1f, SettingsManager.instance.symbolMoveTime).setOnUpdate( 
            (float value) => {
                transform.position = Vector3.Lerp(movementFrom, targetPlayer.position, value);
            }
        ).setOnComplete(
            () => {
                Die();
            }
        ).setEase(SettingsManager.instance.symbolTweenConfig);
    }

    private void Die() {
        bool positiveDeath = PlayerController.CheckMatch(iconHolder.name, targetPlayerNumber);
        int delta = (positiveDeath) ? SettingsManager.instance.positiveMatchScore : SettingsManager.instance.negativeMatchScore;
        PlayerController.players[targetPlayerNumber].scorebar.UpdateScore(delta);
        PlayerController.players[targetPlayerNumber].PlaySound(positiveDeath ? "Receive" : "Miss");
        Fade(1, 0, true);
        ConversionTableController.RemoveSymbol(iconHolder.name, !aimedAtPlayerOne);
    }
}
