using UnityEngine;
using System.Collections.Generic;

public enum PlayerNumber { One = 1, Two = 2 };
public enum MoveDirection { Top, Right, Bottom, Left }

public class PlayerController : MonoBehaviour {

    public PlayerNumber playerNumber;
    public Color symbolColor;
    public Transform topAnchor, rightAnchor, bottomAnchor, leftAnchor;
    public GameObject symbolTemplate;
    public Vector3 symbolSpawnDelta;
    public ScorebarController scorebar;
    public Transform fastCircle1, fastCircle2;
    public Renderer innerCircle1, innerCircle2;
    public Transform symbolHolder;

    private GameObject topSymbol, rightSymbol, bottomSymbol, leftSymbol;
    private Vector3 initialPosition;
    private Vector3 movementFrom;

    private Color innerCircle1Color, innerCircle2Color;

    private bool inputCooldown = false;
    private bool topCooldown, rightCooldown, bottomCooldown, leftCooldown;
    private bool canSend = false;
    private bool alreadySent = false;

    private float currentPulsatingTimer = 0f;
    private Vector3 C1ScaleUpFrom, C2ScaleUpFrom;

    private string lastMarkedSymbol = "";
    private GameObject lastMarkedSymbolObject;

    public static Dictionary<PlayerNumber, PlayerController> players = new Dictionary<PlayerNumber, PlayerController>();

    void Start() {
        topSymbol = SpawnSymbol(topAnchor);
        rightSymbol = SpawnSymbol(rightAnchor);
        bottomSymbol = SpawnSymbol(bottomAnchor);
        leftSymbol = SpawnSymbol(leftAnchor);
        initialPosition = transform.position;
        players[playerNumber] = this;
        ConversionTableController.Init(playerNumber == PlayerNumber.One ? true : false);
        C1ScaleUpFrom = fastCircle1.localScale;
        C2ScaleUpFrom = fastCircle2.localScale;
        innerCircle1Color = innerCircle1.material.color;
        innerCircle2Color = innerCircle2.material.color;
    }

    void Update() {
        HandleMovement();
        HandlePulsating();
    }

    public static bool CheckMatch(string name, PlayerNumber playerNumber) {
        bool result = false;
        if (playerNumber == PlayerNumber.One)
            result = ConversionTableController.IsMatch(players[playerNumber].lastMarkedSymbol, name);
        else
            result = ConversionTableController.IsMatch(name, players[playerNumber].lastMarkedSymbol);

        if (result == true) {
            Destroy(players[playerNumber].lastMarkedSymbolObject);
        }
        return result;
    }

    public void PlaySound(string name) {
        AudioManager.instance.PlaySound("P" + ((int)playerNumber).ToString() + "/" + name);
    }

    private void LightUp() {
        innerCircle1.material.color = Color.white;
        innerCircle2.material.color = Color.white;
    }

    private void LightDown() {
        innerCircle1.material.color = innerCircle1Color;
        innerCircle2.material.color = innerCircle2Color;
    }

    private GameObject SpawnSymbol(Transform anchor, bool updateConversionTable = false) {
        GameObject spawnedSymbol = Instantiate(symbolTemplate) as GameObject;
        spawnedSymbol.transform.SetParent(symbolHolder, false);
        spawnedSymbol.transform.position = anchor.position + symbolSpawnDelta;
        spawnedSymbol.SendMessage("SetRandom");
        spawnedSymbol.SendMessage("ChangeColor", symbolColor);
        spawnedSymbol.SendMessage("AimAtPlayer", playerNumber == PlayerNumber.One ? false : true);

        ConversionTableController.AddSymbol(spawnedSymbol.GetComponent<FontAwesome3D>().name, playerNumber == PlayerNumber.One ? true : false, updateConversionTable);            

        return spawnedSymbol;
    }

    protected virtual void HandleMovement() {
        if (inputCooldown)
            return;

        float xInput = Input.GetAxis("P" + ((int)playerNumber).ToString() + "X");
        float yInput = Input.GetAxis("P" + ((int)playerNumber).ToString() + "Y");

        if (xInput > 0f && !rightCooldown) {
            Move(rightAnchor, rightSymbol, MoveDirection.Right);
            rightCooldown = true;
        } else if (xInput < 0f && !leftCooldown) {
            Move(leftAnchor, leftSymbol, MoveDirection.Left);
            leftCooldown = true;
        } else if (yInput > 0f && !topCooldown) {
            Move(topAnchor, topSymbol, MoveDirection.Top);
            topCooldown = true;
        } else if (yInput < 0f && !bottomCooldown) {
            Move(bottomAnchor, bottomSymbol, MoveDirection.Bottom);
            bottomCooldown = true;
        }

        if (xInput == 0f) {
            rightCooldown = false;
            leftCooldown = false;
        }

        if (yInput == 0f) {
            topCooldown = false;
            bottomCooldown = false;
        }
    }

    private void HandlePulsating() {
        if (currentPulsatingTimer <= 0f) {
            canSend = !canSend;
            currentPulsatingTimer += SettingsManager.instance.playerActivePassiveTime;
            if (canSend) {
                alreadySent = false;
                LightUp();
                Vector3 C1ScaleUpTo = C1ScaleUpFrom * SettingsManager.instance.playerPulseScaleUpTo;
                Vector3 C2ScaleUpTo = C2ScaleUpFrom * SettingsManager.instance.playerPulseScaleUpTo;
                LeanTween.value(fastCircle1.gameObject, 0f, 1f, SettingsManager.instance.playerPulsatingSwitchTime).setOnUpdate(
                    (float value) => {
                        fastCircle1.localScale = Vector3.Lerp(C1ScaleUpFrom, C1ScaleUpTo, value);
                        fastCircle2.localScale = Vector3.Lerp(C2ScaleUpFrom, C2ScaleUpTo, value);
                    }
                ).setEase(SettingsManager.instance.globalTweenConfig);
            } else {
                LightDown();
                Vector3 C1ScaleDownFrom = fastCircle1.localScale;
                Vector3 C2ScaleDownFrom = fastCircle2.localScale;
                LeanTween.value(fastCircle1.gameObject, 0f, 1f, SettingsManager.instance.playerPulsatingSwitchTime).setOnUpdate(
                    (float value) => {
                        fastCircle1.localScale = Vector3.Lerp(C1ScaleDownFrom, C1ScaleUpFrom, value);
                        fastCircle2.localScale = Vector3.Lerp(C2ScaleDownFrom, C2ScaleUpFrom, value);
                    }
                ).setEase(SettingsManager.instance.globalTweenConfig);                       
            }
        }
        currentPulsatingTimer -= Time.deltaTime;
    }

    private void Move(Transform anchor, GameObject symbolSlot, MoveDirection direction) {
        inputCooldown = true;
        movementFrom = transform.position;
        PlaySound("Input");

        LeanTween.value(gameObject, 0f, 1f, SettingsManager.instance.playerMoveTime).setOnUpdate(
            (float value) => {
                transform.position = Vector3.Lerp(movementFrom, anchor.position, value);
            }
        ).setOnComplete(
            () => {
                TryActivateSymbolAndReturn(anchor, symbolSlot, direction);
            }
        ).setEase(SettingsManager.instance.globalTweenConfig);
    }

    private void TryActivateSymbolAndReturn(Transform anchor, GameObject symbolSlot, MoveDirection direction) {
        if (symbolSlot != null && canSend && !alreadySent) {
            symbolSlot.SendMessage("MarkToMove");
            lastMarkedSymbol = symbolSlot.GetComponent<FontAwesome3D>().name;
            lastMarkedSymbolObject = symbolSlot;
            scorebar.UpdateScore(SettingsManager.instance.sendScore);
            alreadySent = true;
            LightDown();
            // PlaySound("Send");

            switch (direction) {
                case MoveDirection.Top:
                    topSymbol = null;
                    break;
                case MoveDirection.Right:
                    rightSymbol = null;
                    break;
                case MoveDirection.Bottom:
                    bottomSymbol = null;
                    break;
                case MoveDirection.Left:
                    leftSymbol = null;
                    break;
            }

            Wait.Run(SettingsManager.instance.symbolRespawnTime, () => {
                switch (direction) {
                    case MoveDirection.Top:
                        topSymbol = SpawnSymbol(anchor, true);
                        break;
                    case MoveDirection.Right:
                        rightSymbol = SpawnSymbol(anchor, true);
                        break;
                    case MoveDirection.Bottom:
                        bottomSymbol = SpawnSymbol(anchor, true);
                        break;
                    case MoveDirection.Left:
                        leftSymbol = SpawnSymbol(anchor, true);
                        break;
                }
            });
        }
        
        ReturnToInitialPosition();
    }

    private void ReturnToInitialPosition() {
        movementFrom = transform.position;

        LeanTween.value(gameObject, 0f, 1f, SettingsManager.instance.playerMoveTime).setOnUpdate(
            (float value) => {
                transform.position = Vector3.Lerp(movementFrom, initialPosition, value);
            }
        ).setOnComplete(
            () => {
                inputCooldown = false;
            }
        ).setEase(SettingsManager.instance.globalTweenConfig);
    }
}
