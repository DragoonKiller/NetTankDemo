using UnityEngine;
using UnityEngine.UI;
using System;
using Utils;

/// <summary>
/// 控制血条行为.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class StrengthBarDisplay : MonoBehaviour
{
    public BattleHUDDisplay hud;
    
    public PlayerControl player => hud.player;
    
    UnitControl unit => player.unit;
    
    Image image => this.GetComponent<Image>();
    
    void Update()
    {
        var hp = unit.currentStrength;
        var maxHp = unit.maxStrength;
        var ratio = (hp / maxHp).Clamp(0, 1);
        image.fillAmount = ratio;
    }
    
}
