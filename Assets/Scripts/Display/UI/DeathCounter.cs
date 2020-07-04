using System;
using UnityEngine;
using Systems;
using Utils;
using UnityEngine.UI;

public partial class Signals
{
    public struct KillCount
    {
        public int faction;
    }
}

public class DeathCounter : MonoBehaviour
{
    
    
    [Tooltip("显示队伍击杀数的文本框.")]
    public Text[] counters;
    
    [SerializeField] [ReadOnly] int[] kills;
    
    void Start()
    {
        Signal<Signals.KillCount>.Listen(CountKill);
        kills = new int[counters.Length];
    }
    
    void Update()
    {
        for(int i = 0; i < counters.Length; i++)
        {
            counters[i].text = kills[i].ToString();
        }
    }
    
    void OnDestroy()
    {
        Signal<Signals.KillCount>.Remove(CountKill);
    }
    
    void CountKill(Signals.KillCount x)
    {
        kills[x.faction - 1] += 1;
    }
}
