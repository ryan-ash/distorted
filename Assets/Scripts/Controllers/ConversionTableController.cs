using UnityEngine;
using System.Collections.Generic;

public static class ListExtension {
    private static System.Random rng = new System.Random();  

    public static void Shuffle<T>(this IList<T> list) {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }
    }
}

public class ConversionTableController : MonoBehaviour {
    
    public Transform P1Holder, P2Holder;
    public GameObject symbolTemplate;
    public float distanceBetweenSymbols = 0.25f;

    private bool P1Ready = false, P2Ready = false;

    public static ConversionTableController instance;
    private static List<string> P1Symbols = new List<string>();
    private static List<string> P2Symbols = new List<string>();

    void Awake() {
        instance = this;
    }

    public static void AddSymbol(string name, bool isPlayerOneSymbol, bool spawn = false) {
        if (isPlayerOneSymbol) {
            P1Symbols.Add(name);
        } else {
            P2Symbols.Add(name);
        }

        if (spawn)
            Spawn(isPlayerOneSymbol, name);
    }

    public static void RemoveSymbol(string name, bool isPlayerOneSymbol) {
        int index = 0;
        if (isPlayerOneSymbol) {
            index = P1Symbols.IndexOf(name);
            P1Symbols.RemoveAt(index);
            foreach (Transform symbolTransform in instance.P1Holder) {
                SymbolController symbol = symbolTransform.gameObject.GetComponent<SymbolController>();
                if (symbol.iconHolder.name == name) {
                    symbol.Fade(1, 0, true);
                }
            }
        } else {
            index = P2Symbols.IndexOf(name);
            P2Symbols.RemoveAt(index);
            foreach (Transform symbolTransform in instance.P2Holder) {
                SymbolController symbol = symbolTransform.gameObject.GetComponent<SymbolController>();
                if (symbol.iconHolder.name == name) {
                    symbol.Fade(1, 0, true);
                }
            }
        }

        Shift(isPlayerOneSymbol);
    }

    public static bool IsMatch(string P1Name, string P2Name) {
        int P1Index = P1Symbols.IndexOf(P1Name);
        int P2Index = P2Symbols.IndexOf(P2Name);
        return P1Index == P2Index && P1Index != -1;
    }

    public static void Init(bool isPlayerOneInit) {
        if (isPlayerOneInit)
            instance.P1Ready = true;
        else
            instance.P2Ready = true;

        if (!instance.P1Ready || !instance.P2Ready)
            return;

        P1Symbols.Shuffle();
        P2Symbols.Shuffle();

        foreach (string name in P1Symbols) {
            Spawn(true, name);
        }

        foreach (string name in P2Symbols) {
            Spawn(false, name);
        }
    }

    public static void Spawn(bool isPlayerOneSymbol, string name) {
        GameObject spawnedSymbol = Instantiate(instance.symbolTemplate) as GameObject;
        spawnedSymbol.SendMessage("SetExact", name);
        Transform holder = (isPlayerOneSymbol) ? instance.P1Holder : instance.P2Holder;
        spawnedSymbol.transform.SetParent(holder, false);

        int index = 0;

        if (isPlayerOneSymbol) {
            index = P1Symbols.IndexOf(name);
            spawnedSymbol.GetComponent<FontAwesome3D>().ChangeColor(PlayerController.players[PlayerNumber.One].symbolColor);
        } else {
            index = P2Symbols.IndexOf(name);
            spawnedSymbol.GetComponent<FontAwesome3D>().ChangeColor(PlayerController.players[PlayerNumber.Two].symbolColor);
        }

        spawnedSymbol.transform.position = holder.position + new Vector3(0, index * instance.distanceBetweenSymbols, 0);

        if (index < 4)
            spawnedSymbol.SendMessage("Reveal");
    }

    public static void Shift(bool isPlayerOne) {
        Transform shiftingHolder = (isPlayerOne) ? instance.P1Holder : instance.P2Holder;
        int index = 0;
        for (int i = 0; i < shiftingHolder.childCount; i++) {
            Transform shiftingChild = shiftingHolder.GetChild(i);
            Vector3 movementFrom = shiftingChild.position;
            Vector3 movementTo = shiftingHolder.position + new Vector3(0, index * instance.distanceBetweenSymbols, 0);
            LeanTween.value(shiftingChild.gameObject, 0f, 1f, SettingsManager.instance.conversionTableShiftTime).setOnUpdate(
                (float value) => {
                    shiftingChild.position = Vector3.Lerp(movementFrom, movementTo, value);
                }
            ).setEase(SettingsManager.instance.globalTweenConfig);
            if (index < 4)
                shiftingChild.gameObject.SendMessage("Reveal");

            if (!shiftingChild.gameObject.GetComponent<SymbolController>().almostDead)
                index += 1;
        }        
    }

    public static bool SymbolPresent(string name) {
        return P1Symbols.Contains(name) || P2Symbols.Contains(name);
    }
}
