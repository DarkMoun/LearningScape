﻿using UnityEngine;
using TMPro;

public class ValueSlider : MonoBehaviour {

	TMP_Text percentageTextSlider;

	// Use this for initialization
	void Start () {
		percentageTextSlider = GetComponent<TMP_Text> ();
	}

	//if value is on 0 and 1
	public void textUpdatePercent (float valueSlider){
		percentageTextSlider.text = Mathf.RoundToInt (valueSlider * 100) + "%";
	}

	//if value is on 0 and 100
	public void textUpdate (float valueSlider){
		percentageTextSlider.text = Mathf.RoundToInt (valueSlider) + "%";
	}

	//if value is on 0 and 10
	public void textUpdateTenPercent (float valueSlider){
		percentageTextSlider.text = Mathf.RoundToInt (valueSlider * 10) + "%";
	}
}
