﻿using UnityEngine;

public class ActionPerformedForLRS : MonoBehaviour {
    // Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).

    public string verb;
    public string objectType;
    public string objectName;

    public bool result = false;
    /// <summary>
    /// Negative: false, 0: null, Positive: true
    /// </summary>
    public int completed = 0;
    /// <summary>
    /// Negative: false, 0: null, Positive: true
    /// </summary>
    public int success = 0;
    public string response = null;
    public int? score = null;
    public float duration = 0;
}