using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Utils;
using UnityEngine.UI;

/// <summary>
/// 控制弹道轨迹和落点预测.
/// </summary>
[RequireComponent(typeof(Image))]
public class PredictionCrosshairDisplay : MonoBehaviour
{
    public BattleHUDDisplay hud;
    
    public PlayerControl player => hud.player;
    
    public PredictionControl prediction => hud.prediction;
    
    Vector3? hitPoint => prediction.hitPoint;
    
    UnitControl hitUnit => prediction.hitUnit; 
    
    Image image => this.GetComponent<Image>();
    
    Color baseColor;
    Vector3 baseScale;
    
    LayerMask cannonballHit;
    
    void Start()
    {
        baseColor = image.color;
        baseScale = image.rectTransform.localScale;
        cannonballHit = LayerMask.GetMask("Unit", "Terrain");
    }
    
    void Update()
    {
        if(hitPoint == null)
        {
            image.color = new Color(0, 0, 0, 0);
        }
        else
        {
            image.color = baseColor;
            var targetPos = Camera.main.WorldToScreenPoint(hitPoint.Value);
            // 做一个 hardcode 平滑插值.
            var curPos = image.rectTransform.position;
            var powx = 0.5f; // 0.1f.Pow(Time.deltaTime);
            image.rectTransform.position = curPos * powx + targetPos * (1 - powx);
            // 根据目标点和当前点的差距放缩图标.
            var scale = (1 + ((curPos - targetPos).ToVec2() / image.rectTransform.sizeDelta).magnitude * 0.2f);
            scale = scale.Clamp(1.0f, 3.0f);
            image.rectTransform.localScale = baseScale * scale;
        }
        
    }
    
}
