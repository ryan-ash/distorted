using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : PlayerController {
    private Vector2 input = Vector2.zero;

    void Start() {
        Init();
    }

    void Update() {
        if (!GameManager.gameStarted)
            return;

        HandleMovement(input.x, input.y);
        HandlePulsating();
    }

    Vector2 ProduceInput() {
        Vector2 result = Vector2.zero;
        PlayerController player = PlayerController.players[PlayerNumber.One];

        bool shouldReflect = player.sentSymbolsInProgress.Count > 0;
        shouldReflect = shouldReflect && (Random.Range(0f, 1f) < SettingsManager.instance.reflectChance);

        if (shouldReflect) {
            if (!SettingsManager.instance.shouldReflect) {
                return result;
            }
            List<string> names = ConversionTableController.GetMatchForAI(player.lastMarkedSymbol);
            if (topSymbol != null && names.Contains(topSymbol.GetComponent<FontAwesome3D>().name)) {
                result.y = 1f;
            } else if (bottomSymbol != null && names.Contains(bottomSymbol.GetComponent<FontAwesome3D>().name)) {
                result.y = -1f;
            } else if (leftSymbol != null && names.Contains(leftSymbol.GetComponent<FontAwesome3D>().name)) {
                result.x = -1f;
            } else if (rightSymbol != null && names.Contains(rightSymbol.GetComponent<FontAwesome3D>().name)) {
                result.x = 1f;
            }
        } else {
            if (!SettingsManager.instance.shouldProduceRandomInput) {
                return result;
            }
            bool xAxis = Random.Range(0f, 1f) < 0.5f;
            float value = Random.Range(0f, 1f) < 0.5f ? -1f : 1f;
            if (xAxis) {
                result.x = value;
            } else {
                result.y = value;
            }
        }

        return result;
    }

    private void PlanAIAction() {
        float attackThrow = Random.Range(0f, 1f);
        if (attackThrow > SettingsManager.instance.attackChance) {
            return;
        }

        float delay = currentPulsatingTimer + Random.Range(SettingsManager.instance.inputHumanization * -1, SettingsManager.instance.inputHumanization);
        StartCoroutine("AITick", delay);
    }

    protected override void ActivateDeactive() {
        canSend = !canSend;

        if (!canSend) {
            PlanAIAction();
        }
    }

    IEnumerator AITick(float delay) {
        yield return new WaitForSeconds(delay);
        input = ProduceInput();
        yield return new WaitForSeconds(Random.Range(0f, SettingsManager.instance.aiInputHang));
        input = Vector2.zero;
    }
}
