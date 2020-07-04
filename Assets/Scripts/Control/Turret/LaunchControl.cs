using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using System;
using Systems;

public partial class Signals
{
    public struct Launch
    {
        public LaunchControl launcher;
        public Vector2 biasVec;
        public float biasDir;
    }
}

/// <summary>
/// 炮弹发射控制.
/// </summary>
public class LaunchControl : MonoBehaviour
{
    [Tooltip("发射的炮弹的从属者.")]
    public UnitControl unit;
    
    [Tooltip("炮弹预设.")]
    public CannonBallControl cannonBallTemplate;
    
    [Tooltip("基本开火冷却.")]
    public float cooldown;
    
    [Tooltip("开火特效.")]
    public GameObject launchFx;
    
    [Tooltip("开火音效.")]
    public AudioSource[] audioSources;
    
    [Tooltip("装填音效.")]
    public AudioSource[] reloadAudioSources;
    
    [Header("装填设置")]
    
    [Tooltip("备弹.")]
    public AmmoControl ammo;
    
    [Tooltip("载弹量.")]
    public float clipMaxAmmo;
    
    
    [Tooltip("装弹耗时.")]
    public float reloadingTime;
    
    [Tooltip("自动装填.")]
    public bool autoReload;
    
    [Tooltip("每次开火消耗的弹药数量.")]
    public float ammoCost;
    
    [Header("预热设置")]
    
    [Tooltip("预热不足导致的额外开火冷却.")]
    public float preheatCooldown;
    
    [Tooltip("完全预热需要打多少子弹.")]
    public int preheatTime;
    
    [Tooltip("预热热量的散失时间.")]
    public float preheatCoolingTime;
    
    [Tooltip("预热导致的炮弹偏移增量.")]
    public float preheatBias;
    
    [Header("状态参数")]
    
    [Tooltip("热量.")]
    public float heat;
    
    [Tooltip("当前弹匣内的弹药量.")]
    public float clipAmmo;
    
    [Tooltip("装弹剩余时间. 为 0 表示装载完毕. 不为 0 表示正在装弹.")]
    public float reloadingProcess;
    
    [Tooltip("当前冷却时间.")]
    public float cooldownProcess;
    
    [Tooltip("上一次开火音效的ID.")]
    public int fireAudioPlayed;
    
    [Tooltip("上一次装填音效的ID.")]
    public int reloadAudioPlayed;
    
    [Tooltip("记录上一帧的位置. 用来估算速度. 移动速度会被附加到初始速度上.")]
    public Vector3 lastPos;
    
    [Tooltip("估算出来的移动速度.")]
    public Vector3 additionalVelocity;
    
    /// <summary>
    /// 开火时的回调函数.
    /// </summary>
    public Action<CannonBallControl> launchCallback;
    
    /// <summary>
    /// 因为热量不足导致的额外冷却.
    /// </summary>
    public float additionalCooldown => preheatCooldown * (1 - heat);
    
    /// <summary>
    /// 总冷却时长.
    /// </summary>
    public float totalCooldown => cooldown + additionalCooldown;
    
    /// <summary>
    /// 是否有足够的弹药来开火.
    /// </summary>
    public bool hasEnoughAmmo => clipAmmo > 0;
    
    /// <summary>
    /// 是否正在装填弹药.
    /// </summary>
    public bool reloading => reloadingProcess != 0;
    
    /// <summary>
    /// 是否正在冷却.
    /// </summary>
    public bool coolingDown => cooldownProcess != 0;
    
    /// <summary>
    /// 弹匣是否满了.
    /// </summary>
    public bool clipFull => clipAmmo == clipMaxAmmo;
    
    /// <summary>
    /// 弹匣是否空了.
    /// </summary>
    public bool clipEmpty => clipAmmo == 0;
    
    /// <summary>
    /// 是否可以开火.
    /// </summary>
    public bool canFire => !coolingDown && !reloading;
    
    void Start()
    {
        fireAudioPlayed = UnityEngine.Random.Range(0, audioSources.Length);
        lastPos = this.transform.position;
    }
    
    void Update()
    {
        if(autoReload && clipEmpty && ammo.ammo != 0 && !reloading) BeginReload();
        
        var prevReloadingProcess = reloadingProcess;
        reloadingProcess.CountDownTime();
        if(reloadingProcess == 0 && prevReloadingProcess != 0) EndReload();
        
        cooldownProcess.CountDownTime();
        
        var curPos = this.transform.position;
        additionalVelocity = lastPos.To(curPos) / Time.deltaTime;
        lastPos = curPos;
        
        heat = (heat - Time.deltaTime / preheatCoolingTime).Max(0);
    }
    
    /// <summary>
    /// 尝试装弹.
    /// </summary>
    public bool BeginReload()
    {
        if(!enabled) return false;
        if(clipFull) return false;
        if(reloading) return false;
        reloadingProcess = reloadingTime;
        PlayReloadAudio();
        return true;
    }
    
    /// <summary>
    /// 结束装填过程, 添加弹药.
    /// </summary>
    void EndReload()
    {
        var loadedAmmo = (clipMaxAmmo - clipAmmo).Min(ammo.ammo);
        ammo.ammo -= loadedAmmo;
        clipAmmo += loadedAmmo;
    }
    
    /// <summary>
    /// 尝试开火.
    /// </summary>
    public bool TryFire(int factionId = -1)
    {
        if(!enabled) return false;
        if(canFire)
        {
            ForceFire(factionId);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 不考虑任何条件, 直接开火.
    /// 会消减弹药量并重置冷却.
    /// </summary>
    public void ForceFire(int factionId = -1)
    {
        if(!enabled) return;
        
        Signal.Emit(new Signals.Launch() {
            launcher = this,
            biasVec = Vector2.right.Rot((0f, 360f).Random().ToRad()),
            biasDir = (-Mathf.PI, Mathf.PI).Random() + heat * preheatBias, // 积聚热量会导致偏差增加.
        
        });
    }
    
    /// <summary>
    /// 播放开火音效.
    /// </summary>
    void PlayFireAudio()
    {
        if(audioSources.Length == 0) return;
        
        // 随机选一个音效播放.
        int id = 0;
        if(audioSources.Length > 1) id = (0, audioSources.Length - 1).RandomWithout(fireAudioPlayed);
        audioSources[id].Play();
        fireAudioPlayed = id;
    }
    
    /// <summary>
    /// 播放装填音效.
    /// </summary>
    void PlayReloadAudio()
    {
        if(reloadAudioSources.Length == 0) return;
        int id = 0;
        if(reloadAudioSources.Length > 1) id = (0, reloadAudioSources.Length - 1).RandomWithout(reloadAudioPlayed);
        reloadAudioSources[id].Play();
        reloadAudioPlayed = id;
    }
    
    public static void LaunchCallback(Signals.Launch e)
    {
        var cannonBallTemplate = e.launcher.cannonBallTemplate;
        var unit = e.launcher.unit;
        var launcher = e.launcher;
        
        // 开火会把威胁值设置成 1.
        unit.threat = unit.threat.Max(1);
        
        // 创建炮弹.
        var g = GameObject.Instantiate(cannonBallTemplate.gameObject, launcher.transform.position, launcher.transform.rotation);
        
        var x = g.GetComponent<CannonBallControl>();
        x.owner = unit;
        
        // 炮口自身的移动速度会叠加到炮弹上.
        x.appendVelocity = launcher.additionalVelocity;
        
        if(launcher.launchFx != null) GameObject.Instantiate(launcher.launchFx, launcher.transform.position, launcher.transform.rotation);
        
        launcher.launchCallback?.Invoke(x);
        launcher.clipAmmo -= launcher.ammoCost;
        launcher.PlayFireAudio();
        
        // 每次开火会增加热量.
        launcher.heat = (launcher.heat + 1.0f / launcher.preheatTime).Min(1);
        
        // 热量不足会导致冷却变长.
        launcher.cooldownProcess = launcher.totalCooldown;
    }
    
}
