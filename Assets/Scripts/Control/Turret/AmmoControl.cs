using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

/// <summary>
/// 控制总弹药量.
/// </summary>
public class AmmoControl : MonoBehaviour
{
    [Tooltip("最大弹药数量.")]
    public float maxAmmo;
    
    [Tooltip("无限弹药开关. 弹药将不会减少(但是会增加).")]
    public bool infinite;
    
    [Header("状态参数")]
    
    [Tooltip("当前弹药数量.")]
    [SerializeField] float _ammo;
    
    public float ammo
    {
        get => _ammo;
        set => _ammo = value.Clamp(infinite ? _ammo : 0, maxAmmo);
    }
    
    void Start()
    {
        ammo = maxAmmo.Max(ammo);
    }
}
