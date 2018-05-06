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
    private float fastCircle1InitialSpeed;
    private float fastCircle2InitialSpeed;
    private Vector3 C1ScaleUpFrom, C2ScaleUpFrom, C1ScaleUpTo, C2ScaleUpTo;
    private GameObject lastMarkedSymbolObject;

    [HideInInspector]
    public string lastMarkedSymbol = "";

    public List<GameObject> sentSymbolsInProgress = new List<GameObject>();

    public static Dictionary<PlayerNumber, PlayerController> players = new Dictionary<PlayerNumber, PlayerController>();

    private Dictionary<MoveDirection, Transform> directionToAnchor;

    void Start() {
        directionToAnchor = new Dictionary<MoveDirection, Transform>() {
            {MoveDirection.Top, topAnchor},
            {MoveDirection.Right, rightAnchor},
            {MoveDirection.Bottom, bottomAnchor},
            {MoveDirection.Left, leftAnchor}
        };

        topSymbol = SpawnSymbol(MoveDirection.Top);
        rightSymbol = SpawnSymbol(MoveDirection.Right);
        bottomSymbol = SpawnSymbol(MoveDirection.Bottom);
        leftSymbol = SpawnSymbol(MoveDirection.Left);

        initialPosition = transform.position;
        players[playerNumber] = this;
        ConversionTableController.Init(playerNumber == PlayerNumber.One ? true : false);
        C1ScaleUpFrom = fastCircle1.localScale;
        C2ScaleUpFrom = fastCircle2.localScale;
        C1ScaleUpTo = C1ScaleUpFrom * SettingsManager.instance.playerPulseScaleUpTo;
        C2ScaleUpTo = C2ScaleUpFrom * SettingsManager.instance.playerPulseScaleUpTo;
        innerCircle1Color = innerCircle1.material.color;
        innerCircle2Color = innerCircle2.material.color;
        fastCircle1InitialSpeed = fastCircle1.GetComponent<Rotator>().rotationSpeed;
        fastCircle2InitialSpeed = fastCircle1.GetComponent<Rotator>().rotationSpeed;
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

        return result;
    }

    public void PlaySound(string name) {
        AudioManager.instance.PlaySound("P" + ((int)playerNumber).ToString() + "/" + name);
    }

    private GameObject SpawnSymbol(MoveDirection direction, bool updateConversionTable = false) {
        Transform anchor = directionToAnchor[direction];
        GameObject spawnedSymbol = Instantiate(symbolTemplate) as GameObject;
        spawnedSymbol.transform.SetParent(symbolHolder, false);
        spawnedSymbol.transform.position = anchor.position + symbolSpawnDelta;
        spawnedSymbol.SendMessage("SetRandom");
        spawnedSymbol.SendMessage("ChangeColor", symbolColor);
        spawnedSymbol.SendMessage("AimAtPlayer", playerNumber == PlayerNumber.One ? false : true);

        ConversionTableController.AddSymbol(direction, spawnedSymbol.GetComponent<FontAwesome3D>().name, playerNumber == PlayerNumber.One ? true : false, !updateConversionTable);            

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

    private void LightUp(bool forward) {
        innerCircle1.material.color = forward ? Color.white : innerCircle1Color;
        innerCircle2.material.color = forward ? Color.white : innerCircle2Color;
        innerCircle1.material.SetColor("_EmissionColor", forward ? SettingsManager.instance.activeEmissionColor : Color.black);
        innerCircle2.material.SetColor("_EmissionColor", forward ? SettingsManager.instance.activeEmissionColor : Color.black);
        innerCircle2.gameObject.SetActive(forward);
        
        fastCircle1.GetComponent<Rotator>().rotationSpeed = forward ? fastCircle1InitialSpeed : fastCircle1InitialSpeed / SettingsManager.instance.rotationSpeedDivider;
        fastCircle2.GetComponent<Rotator>().rotationSpeed = forward ? fastCircle2InitialSpeed : fastCircle2InitialSpeed / SettingsManager.instance.rotationSpeedDivider;
    }

    private void Scale(bool forward) {
        float startValue = forward ? 0f : 1f;
        float endValue = !forward ? 0f : 1f;

        LeanTween.value(fastCircle1.gameObject, startValue, endValue, SettingsManager.instance.playerPulsatingSwitchTime).setOnUpdate(
            (float value) => {
                fastCircle1.localScale = Vector3.Lerp(C1ScaleUpFrom, C1ScaleUpTo, value);
                fastCircle2.localScale = Vector3.Lerp(C2ScaleUpFrom, C2ScaleUpTo, value);
            }
        ).setEase(SettingsManager.instance.globalTweenConfig);

    }

    private void HandlePulsating() {
        if (currentPulsatingTimer <= 0f) {
            canSend = !canSend;
            currentPulsatingTimer += SettingsManager.instance.playerActivePassiveTime;
            LightUp(canSend);
            Scale(canSend);
            alreadySent = canSend ? false : alreadySent;
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
            sentSymbolsInProgress.Add(symbolSlot);
            symbolSlot.SendMessage("MarkToMove");
            lastMarkedSymbol = symbolSlot.GetComponent<FontAwesome3D>().name;
            lastMarkedSymbolObject = symbolSlot;
            alreadySent = true;
            LightUp(false);
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
                        topSymbol = SpawnSymbol(direction, true);
                        break;
                    case MoveDirection.Right:
                        rightSymbol = SpawnSymbol(direction, true);
                        break;
                    case MoveDirection.Bottom:
                        bottomSymbol = SpawnSymbol(direction, true);
                        break;
                    case MoveDirection.Left:
                        leftSymbol = SpawnSymbol(direction, true);
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
