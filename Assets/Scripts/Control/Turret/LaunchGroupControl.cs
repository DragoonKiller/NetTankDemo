using System;
using UnityEngine;
using Utils;

/// <summary>
/// 仅用于存储当前坦克的开火点.
/// </summary>
[RequireComponent(typeof(UnitControl))]
public class LaunchGroupControl : MonoBehaviour
{
    public LaunchControl[] launches;
}
