using System;
using UnityEngine;

/// <summary>
/// 这个脚本为战斗界面的UI提供所需的信息, 例如 player control 等.
/// </summary>
public class BattleHUDDisplay : MonoBehaviour
{
    public static BattleHUDDisplay inst;
    
    BattleHUDDisplay() => inst = this;
    
    public PredictionControl prediction => unit.GetComponent<PredictionControl>();
    
    public PlayerControl player => PlayerControl.inst;
    
    public UnitControl unit => player.unit;
    
}
