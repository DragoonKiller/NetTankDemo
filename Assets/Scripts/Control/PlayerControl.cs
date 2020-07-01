using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using System;
using Systems;
using System.Linq;

public class PlayerControl : MonoBehaviour
{
    [Tooltip("需要控制的炮塔.")]
    public TurretControl turret;
    
    [Tooltip("需要控制的坦克.")]
    public TankControl tank;
    
    [Tooltip("需要控制的开火点.")]
    public LaunchControl[] launches;
    
    [Tooltip("需要控制的单位.")]
    public UnitControl unit;
    
    [Tooltip("准星在屏幕的哪个位置. 用 0 到 1 的数字指定坐标在屏幕中的相对位置.")]
    public Vector2 cursorPos;
    
    [Tooltip("最大瞄准距离; 如果这个距离内没有瞄准的物体, 默认瞄准半径为该数值的球面上的一个点.")]
    public float maxDistance;
    
    [Tooltip("瞄准线会被什么物体击中.")]
    public LayerMask aimingMask;
    
    /// 用于实现轮流开火的状态机.
    FireSTM fireSTM;
    
    /// <summary>
    /// 用于实现交替开火的状态机.
    /// </summary>
    class FireSTM : StateMachine
    {
        public PlayerControl player;
        public LaunchControl[] launches;
        
        public override IEnumerable<Transfer> Step()
        {
            int cur = 1;
            float t = 0;
            while(true)
            {
                yield return Pass();
                if(!player.enabled) continue;
                float cooldown = launches.Average(x => x.totalCooldown) / launches.Length;
                if(Input.GetKey(KeyCode.Mouse0) && t == 0)
                {
                    if(launches[cur].TryFire())
                    {
                        t += cooldown;
                        cur = (cur + 1).ModSys(launches.Length);
                    }       
                }
                
                t = (t - Time.deltaTime).Max(0);
            }
        }
    }
    
    void Start()
    {
        fireSTM = StateMachine.Register(new FireSTM(){ launches = launches, player = this });
    }
    
    void Update()
    {
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
            tank.forwarding =
                stay ? 0
                : forward ? 1
                : -1;
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
        
        // 见 https://forum.unity.com/threads/detect-resolution-selected-in-editor-game-view.299814/
        // 以及 https://answers.unity.com/questions/179775/game-window-size-from-editor-window-in-editor-mode.html
        #if UNITY_EDITOR
        var curResolution = Resolution.GetMainGameViewSize();
        #else
        var curResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        #endif
        
        var ray = Camera.main.ScreenPointToRay(
            new Vector2(
                curResolution.x * cursorPos.x, 
                curResolution.y * cursorPos.y
            )
        );
        
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
