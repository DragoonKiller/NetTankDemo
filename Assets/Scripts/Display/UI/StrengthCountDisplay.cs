using UnityEngine;
using UnityEngine.UI;
using System;
using Utils;

/// <summary>
/// 控制血条行为.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Text))]
public class StrengthCountDisplay : MonoBehaviour
{
    [Tooltip("当前血量的文字颜色.")]
    public Color currentHpColor;
    
    [Tooltip("普通文字颜色.")]
    public Color normalColor;
    
    [Tooltip("最大血量的文字颜色.")]
    public Color maxHpColor;
    
    public BattleHUDDisplay hud;
    
    public PlayerControl player => hud.player;
    
    UnitControl unit => player.unit;
    
    
    Text text => this.GetComponent<Text>();
    
    void Update()
    {
        var hp = unit.currentStrength;
        var maxHp = unit.maxStrength;
        text.text = $"{hp.ToString().WithColor(currentHpColor)}{"/".WithColor(normalColor)}{maxHp.ToString().WithColor(maxHpColor)}";
    }
    
}
