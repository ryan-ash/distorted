using UnityEngine;

public class CameraSetController : MonoBehaviour {

    public Transform P1Anchor, P2Anchor;

    public static CameraSetController instance;

    private void Awake() {
        instance = this;
    }

    public static void MoveToPlayer(bool toPlayerOne) {
        Vector3 moveTo = toPlayerOne ? instance.P1Anchor.position : instance.P2Anchor.position;
        Vector3 moveFrom = instance.transform.position;

        LeanTween.value(instance.gameObject, 0f, 1f, SettingsManager.instance.cameraMovingTime).setOnUpdate(
            (float value) => {
                instance.transform.position = Vector3.Lerp(moveFrom, moveTo, value);
            }
        ).setEase(SettingsManager.instance.globalTweenConfig);

    }
}
