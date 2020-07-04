using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using System;
using Systems;
using System.Linq;

public class PlayerControl : MonoBehaviour
{
    public static PlayerControl inst;
    
    PlayerControl() => inst = this;
    
    [Tooltip("组件是否正在工作.")]
    public bool working;
    
    [Tooltip("需要控制的单位.")]
    public UnitControl unit;
    
    [Tooltip("准星在屏幕的哪个位置. 用 0 到 1 的数字指定坐标在屏幕中的相对位置.")]
    public Vector2 cursorPos;
    
    [Tooltip("最大瞄准距离; 如果这个距离内没有瞄准的物体, 默认瞄准半径为该数值的球面上的一个点.")]
    public float maxDistance;
    
    [Tooltip("瞄准线会被什么物体击中.")]
    public LayerMask aimingMask;
    
    public TurretControl turret => unit.GetComponent<TurretControl>();
    
    public TankControl tank => unit.GetComponent<TankControl>();
    
    public LaunchGroupControl launchGroup => unit.GetComponent<LaunchGroupControl>();

    public LaunchControl[] launches => launchGroup.launches;
    
    /// 用于实现轮流开火的状态机.
    FireSTM fireSTM;
    
    /// <summary>
    /// 用于实现交替开火的状态机.
    /// </summary>
    class FireSTM : StateMachine
    {
        public PlayerControl player;
        
        public override IEnumerable<Transfer> Step()
        {
            int cur = 0;
            float t = 0;
            while(true)
            {
                yield return Pass();
                if(!player) continue;
                if(!player.enabled) continue;
                if(!player.working) continue;
                
                float cooldown = player.launches.Average(x => x.totalCooldown) / player.launches.Length;
                if(Input.GetKey(KeyCode.Mouse0) && t == 0)
                {
                    if(player.launches[cur].TryFire())
                    {
                        t += cooldown;
                        cur = (cur + 1).ModSys(player.launches.Length);
                    }
                }
                
                t = (t - Time.deltaTime).Max(0);
            }
        }
    }
    
    void Start()
    {
        fireSTM = StateMachine.Register(new FireSTM(){ player = this });
    }
    
    void Update()
    {
        if(!working) return;
        ControlTank();
        ControlTurret();
        ControlReload();
    }
    
    void Destroy()
    {
        StateMachine.Remove(fireSTM.tag);
    }
    
    void ControlReload()
    {
        if(Input.GetKey(KeyCode.R)) foreach(var i in launches) i.BeginReload();
    }
    
    
    void ControlTank()
    {
        bool left = Input.GetKey(KeyCode.A);
        bool right = Input.GetKey(KeyCode.D);
        bool forward = Input.GetKey(KeyCode.W);
        bool backward = Input.GetKey(KeyCode.S);
        
        bool stay = forward == backward;
        bool stayDir = left == right;
        
        if(stayDir)
        {
            tank.turning = 0;
            tank.forwarding = stay ? 0 : forward ? 1 : -1;
        }
        else if(stay) // && !stayDir
        {
            tank.forwarding = 0;
            tank.turning = left ? -1 : 1;
        }
        else if(forward)
        {
            if(left)
            {
                tank.forwarding = 0.5f;
                tank.turning = -0.5f;
            }
            else if(right)
            {
                tank.forwarding = 0.5f;
                tank.turning = 0.5f;
            }
            else
            {
                tank.forwarding = 1f;
                tank.turning = 0f;
            }
        }
        else if(backward)
        {
            if(left)
            {
                tank.forwarding = -0.5f;
                tank.turning = 0.5f;
            }
            else if(right)
            {
                tank.forwarding = -0.5f;
                tank.turning = -0.5f;
            }
            else
            {
                tank.forwarding = -1f;
                tank.turning = 0f;
            }
        }
        else if(left)
        {
            tank.forwarding = 0f;
            tank.turning = -1f;
        }
        else if(right)
        {
            tank.forwarding = 0f;
            tank.turning = 1f;
        }
        else
        {
            tank.turning = tank.forwarding = 0;
        }
    }
    
    void ControlTurret()
    {
        if(turret == null) return;
        
        var ray = Camera.main.ViewportPointToRay(cursorPos);
        
        if(Physics.Raycast(ray, out var hit, maxDistance, aimingMask))
        {
            turret.targetPos = hit.point;
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction.Len(maxDistance));
            turret.targetPos = unit.transform.position + ray.direction.Len(maxDistance);
            Debug.DrawLine(ray.origin, turret.targetPos, Color.black);
        }
        
    }
    
    
}
