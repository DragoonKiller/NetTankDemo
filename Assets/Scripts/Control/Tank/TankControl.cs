using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

[RequireComponent(typeof(Rigidbody))]
public class TankControl : MonoBehaviour
{
    [Tooltip("质心相对位置.")]
    public Vector3 centerOfMass;
    
    [Tooltip("左边履带.")]
    public TrackControl leftTrack;
    
    [Tooltip("右边履带.")]
    public TrackControl rightTrack;
    
    [Tooltip("空闲状态的音效.")]
    public AudioSource idleAudio;
    
    [Tooltip("行进状态的音效.")]
    public AudioSource runAudio;
    
    [Tooltip("行进状态的音调调整.")]
    public float audioPitchChange;
    
    [Tooltip("行进状态的音调调整时间长度.")]
    public float audioChangeTime;
    
    [Header("状态参数")]
    
    [Tooltip("启动程度. 控制坦克的音效.")]
    [ReadOnly] [SerializeField] [Range(0, 1)] float workingRate;
    
    [Tooltip("右转程度. 负数表示左转.")]
    [ReadOnly] [SerializeField] [Range(-1, 1)] float _turning;
    
    [Tooltip("前进程度. 负数表示后退.")]
    [ReadOnly] [SerializeField] [Range(-1, 1)] float _forwarding;
    
    
    
    float idleAudioBaseVolume;
    float runAudioBaseVolume;
    float runAudioBasePitch;
    
    Rigidbody body => this.GetComponent<Rigidbody>();
    
    
    public float turning
    {
        get => _turning;
        set
        {
            _turning = value;
            SetupPower(value, forwarding);
        }
    }
    
    public float forwarding
    {
        get => _forwarding;
        set
        {
            _forwarding = value;
            SetupPower(turning, value);
        }
    }
    
    void Start()
    {
        idleAudioBaseVolume = idleAudio.volume;
        runAudioBaseVolume = runAudio.volume;
        runAudioBasePitch = runAudio.pitch;
    }
    
    void Update()
    {
        SyncCenterOfMass();
        SyncAudio();
    }
    
    
    /// <summary>
    /// 控制音效.
    /// </summary>
    void SyncAudio()
    {
        bool active = forwarding != 0 || turning != 0;
        if(active) workingRate += Time.deltaTime / audioChangeTime;
        else workingRate -= Time.deltaTime / audioChangeTime;
        workingRate = workingRate.Clamp(0, 1);
        idleAudio.volume = idleAudioBaseVolume * (1 - workingRate);
        runAudio.volume = runAudioBaseVolume * workingRate;
        runAudio.pitch = runAudioBasePitch + workingRate.Xmap(0, 1, 0, audioPitchChange);
    }
    
    /// <summary>
    /// 更新质心位置.
    /// </summary>
    void SyncCenterOfMass()
    {
        body.centerOfMass = centerOfMass;
    }
    
    /// <summary>
    /// 根据给定参数算出要给两侧履带多大的功率.
    /// turning 表示功率差, forwarding 表示功率平均值.
    /// </summary>
    void SetupPower(float turning, float forwarding)
    {
        leftTrack.output = (forwarding + turning).Clamp(-1, 1);
        rightTrack.output = (forwarding - turning).Clamp(-1, 1);
    }
    
}
