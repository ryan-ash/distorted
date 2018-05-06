using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustController : MonoBehaviour {

	public int playerNumber;
	private Renderer renderer;

	public static Dictionary<int, DustController> controllers = new Dictionary<int, DustController>();

	// Use this for initialization
	void Awake () {
		renderer = GetComponent<Renderer>();
		controllers[playerNumber] = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Flash(bool counterFlash) {
		float flashSize = (counterFlash) ? SettingsManager.instance.dustBigFlashSize : SettingsManager.instance.dustSmallFlashSize;
		float flashTime = (counterFlash) ? SettingsManager.instance.dustFlashTime : SettingsManager.instance.dustFlashTime / 2;
		LeanTween.value(gameObject, 0f, flashSize, flashTime).setOnUpdate(
            (float value) => {
				Color tmpColor = renderer.material.GetColor("_TintColor");
                tmpColor.a = value;
				renderer.material.SetColor("_TintColor", tmpColor);
            }
        ).setEase(SettingsManager.instance.dustFlashTweenConfig);
	}
}
