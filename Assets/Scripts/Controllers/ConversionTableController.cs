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

public class ConversionTableLine {
    public MoveDirection direction;
    public List<string> symbols;
}

public class ConversionTableController : MonoBehaviour {
    
    public Transform P1Holder, P2Holder;
    public GameObject symbolTemplate;
    public GameObject lineTemplate;
    public float distanceBetweenSymbols = 0.25f;

    private bool P1Ready = false, P2Ready = false;

    public static ConversionTableController instance;
    private static List<ConversionTableLine> P1Symbols = new List<ConversionTableLine>();
    private static List<ConversionTableLine> P2Symbols = new List<ConversionTableLine>();

    void Awake() {
        instance = this;
    }

    public static void AddSymbol(MoveDirection direction, string name, bool isPlayerOneSymbol, bool initial) {
        ConversionTableLine currentLine = null;
        int index = -1;
        if (initial) {
            currentLine = new ConversionTableLine();
            currentLine.direction = direction;
            currentLine.symbols = new List<string>();
            if (isPlayerOneSymbol)
                P1Symbols.Add(currentLine);
            else
                P2Symbols.Add(currentLine);
        } else {
            List<ConversionTableLine> targetSymbols = isPlayerOneSymbol ? P1Symbols : P2Symbols;
            for (int i = 0; i < targetSymbols.Count; i++) {
                ConversionTableLine line = targetSymbols[i];
                if (line.direction == direction) {
                    currentLine = line;
                    index = i;
                    break;
                }
            }
        }

        currentLine.symbols.Add(name);

        if (!initial)
            Spawn(isPlayerOneSymbol, name, index);
    }

    public static void RemoveSymbol(string name, bool isPlayerOneSymbol) {
        int index = 0;
        if (isPlayerOneSymbol) {
            for(int i = 0; i < P1Symbols.Count; i++) {
                if (P1Symbols[i].symbols.Contains(name)) {
                    index = i;
                    P1Symbols[i].symbols.Remove(name);
                    break;
                }
            }

            foreach (Transform lineTransform in instance.P1Holder) {
                foreach (Transform symbolTransform in lineTransform) {
                    SymbolController symbol = symbolTransform.gameObject.GetComponent<SymbolController>();
                    if (symbol.iconHolder.name == name) {
                        symbol.Fade(1, 0, true);
                        break;
                    }
                }
            }
        } else {
            for(int i = 0; i < P2Symbols.Count; i++) {
                if (P2Symbols[i].symbols.Contains(name)) {
                    index = i;
                    P2Symbols[i].symbols.Remove(name);
                    break;
                }
            }

            foreach (Transform lineTransform in instance.P2Holder) {
                foreach (Transform symbolTransform in lineTransform) {
                    SymbolController symbol = symbolTransform.gameObject.GetComponent<SymbolController>();
                    if (symbol.iconHolder.name == name) {
                        symbol.Fade(1, 0, true);
                    }
                }
            }
        }

        Shift(isPlayerOneSymbol, index);
    }

    public static bool IsMatch(string P1Name, string P2Name) {
        int P1Index = -1;
        int P2Index = -1;

        for(int i = 0; i < P1Symbols.Count; i++) {
            if (P1Symbols[i].symbols.Contains(P1Name)) {
                P1Index = i;
                break;
            }
        }

        for(int i = 0; i < P2Symbols.Count; i++) {
            if (P2Symbols[i].symbols.Contains(P2Name)) {
                P2Index = i;
                break;
            }
        }

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

        for (int i = 0; i < P1Symbols.Count; i++) {
            GameObject spawnedLine = Instantiate(instance.lineTemplate) as GameObject;
            spawnedLine.transform.SetParent(instance.P1Holder, false);
            spawnedLine.transform.localPosition = new Vector3(0, i * instance.distanceBetweenSymbols, 0);
            Spawn(true, P1Symbols[i].symbols[0], i);
        }

        for (int i = 0; i < P2Symbols.Count; i++) {
            GameObject spawnedLine = Instantiate(instance.lineTemplate) as GameObject;
            spawnedLine.transform.SetParent(instance.P2Holder, false);
            spawnedLine.transform.localPosition = new Vector3(0, i * instance.distanceBetweenSymbols, 0);
            Spawn(false, P2Symbols[i].symbols[0], i);
        }
    }

    public static void Spawn(bool isPlayerOneSymbol, string name, int lineNumber) {
        GameObject spawnedSymbol = Instantiate(instance.symbolTemplate) as GameObject;
        spawnedSymbol.SendMessage("SetExact", name);
        Transform holder = (isPlayerOneSymbol) ? instance.P1Holder : instance.P2Holder;
        spawnedSymbol.transform.SetParent(holder.GetChild(lineNumber), false);

        int index = 0;

        if (isPlayerOneSymbol) {
            index = P1Symbols[lineNumber].symbols.IndexOf(name);
            spawnedSymbol.GetComponent<FontAwesome3D>().ChangeColor(PlayerController.players[PlayerNumber.One].symbolColor);
        } else {
            index = P2Symbols[lineNumber].symbols.IndexOf(name);
            spawnedSymbol.GetComponent<FontAwesome3D>().ChangeColor(PlayerController.players[PlayerNumber.Two].symbolColor);
        }

        index *= (isPlayerOneSymbol) ? -1 : 1;

        spawnedSymbol.transform.position = holder.position + new Vector3(index * instance.distanceBetweenSymbols, lineNumber * instance.distanceBetweenSymbols, 0);

        spawnedSymbol.SendMessage("Reveal");
    }

    public static void Shift(bool isPlayerOne, int lineNumber) {
        Transform holder = (isPlayerOne) ? instance.P1Holder : instance.P2Holder;
        int index = 0;
        Transform holderLine = holder.GetChild(lineNumber);
        for (int i = 0; i < holderLine.childCount; i++) {
            Transform shiftingChild = holderLine.GetChild(i);
            Vector3 movementFrom = shiftingChild.position;
            int direction = (isPlayerOne) ? -1 : 1;
            Vector3 movementTo = holderLine.position + new Vector3(index * instance.distanceBetweenSymbols * direction, 0);
            LeanTween.value(shiftingChild.gameObject, 0f, 1f, SettingsManager.instance.conversionTableShiftTime).setOnUpdate(
                (float value) => {
                    shiftingChild.position = Vector3.Lerp(movementFrom, movementTo, value);
                }
            ).setEase(SettingsManager.instance.globalTweenConfig);

            if (!shiftingChild.gameObject.GetComponent<SymbolController>().almostDead)
                index += 1;
        }
    }

    public static bool SymbolPresent(string name) {
        bool found = false;
        foreach (ConversionTableLine line in P1Symbols) {
            if (line.symbols.Contains(name)) {
                found = true;
                break;
            }
        }
        foreach (ConversionTableLine line in P2Symbols) {
            if (line.symbols.Contains(name)) {
                found = true;
                break;
            }
        }
        return found;
    }
}
