using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustController : MonoBehaviour {

	private Renderer renderer;

	public static DustController instance;

	// Use this for initialization
	void Awake () {
		renderer = GetComponent<Renderer>();
		instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static void Flash(bool counterFlash) {
		float flashSize = (counterFlash) ? SettingsManager.instance.dustBigFlashSize : SettingsManager.instance.dustSmallFlashSize;
		float flashTime = (counterFlash) ? SettingsManager.instance.dustFlashTime : SettingsManager.instance.dustFlashTime / 2;
		LeanTween.value(instance.gameObject, 0f, flashSize, flashTime).setOnUpdate(
            (float value) => {
				Color tmpColor = instance.renderer.material.GetColor("_TintColor");
                tmpColor.a = value;
				instance.renderer.material.SetColor("_TintColor", tmpColor);
            }
        ).setEase(SettingsManager.instance.dustFlashTweenConfig);

	}
}
