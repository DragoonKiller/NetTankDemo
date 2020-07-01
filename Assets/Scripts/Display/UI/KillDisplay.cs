using System;
using UnityEngine;
using UnityEngine.UI;
using Systems;
using Utils;

/// <summary>
/// 控制击杀和被击杀提示.
/// </summary>
public class KillDisplay : MonoBehaviour
{
    public BattleHUDDisplay hud;
    
    public PlayerControl player => hud.player;
    
    [Tooltip("击杀提示文本框.")]
    public Text text;
    
    [Tooltip("文本颜色.")]
    public Color color;
    
    [Tooltip("多久后开始隐去.")]
    public float fadeBeginTime;
    
    [Tooltip("隐去时间.")]
    public float fadeTime;
    
    float t;
    
    UnitControl me => player.unit;
    
    void Start()
    {
        Signal<Signals.Hit>.Listen(KillCallback);
    }
    
    void Update()
    {
        if(t.CountDownTime() < fadeTime) text.color = color * t / fadeTime;
        else text.color = color;
    }
    
    void OnDestroy()
    {
        Signal<Signals.Hit>.Remove(KillCallback);
    }
    
    void KillCallback(Signals.Hit e)
    {
        if(!e.hit.dead) return;
        text.text = $"{e.source.gameObject.name} 击杀 {e.hit.gameObject.name}";
        t = fadeBeginTime + fadeTime;
    }
}
