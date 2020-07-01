using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utils;

/// <summary>
/// 控制血条行为.
/// </summary>
[ExecuteAlways]
public class BackupAmmoDisplay : MonoBehaviour
{
    public BattleHUDDisplay hud;
    
    [Tooltip("需要控制的UI文本.")]
    public Text text;
    
    [Tooltip("文本颜色.")]
    public Color color;
    
    PlayerControl player => hud.player;
    
    readonly HashSet<AmmoControl> ammoControls = new HashSet<AmmoControl>();
    
    void Start()
    {
        foreach(var i in player.launches) ammoControls.Add(i.ammo);
    }
    
    void Update()
    {
        var maxAmmo = ammoControls.Select(x => x.maxAmmo).Sum();
        var ammo = ammoControls.Select(x => x.ammo).Sum();
        text.text = ammo.ToString().WithColor(color);
    }
    
}
