using UnityEngine;
using UnityEngine.UI;
using System;
using Utils;

public class TargetLockDisplay : MonoBehaviour
{
    public BattleHUDDisplay hud;
    
    public PlayerControl player => hud.player;
    
    public PredictionControl prediction => hud.prediction;
    
    public Image strengthBar;
    
    public RectTransform trans => this.GetComponent<RectTransform>();
    
    public Camera cam => Camera.main;
    
    void Update()
    {
        var target = prediction.hitUnit;
        foreach(var i in this.GetComponentsInChildren<Image>()) i.enabled = target != null;
        if(target == null) return;
        var screenPos = cam.WorldToScreenPoint(target.aimCenter);
        trans.position = screenPos;
        strengthBar.fillAmount = target.currentStrength / target.maxStrength;
    }
}
