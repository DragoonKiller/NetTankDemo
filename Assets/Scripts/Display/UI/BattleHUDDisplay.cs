using System;
using UnityEngine;

/// <summary>
/// 这个脚本为战斗界面的UI提供所需的信息, 例如 player control 等.
/// </summary>
public class BattleHUDDisplay : MonoBehaviour
{
    [Tooltip("战斗UI显示哪个单位的属性.")]
    public PlayerControl player;
    
    [Tooltip("控制哪个轨迹预测脚本.")]
    public PredictionControl prediction;
    
    public UnitControl unit => player.unit;
    
}
