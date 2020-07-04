using UnityEngine;
using System;
using Utils;
using Systems;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AI;

/// <summary>
/// 控制坦克的AI.
/// 同时控制炮台和坦克 (因为任务要求需要判断"支持AI坦克在攻击完角色后并且被玩家反击时"的状态, 懒得分开写了)
/// </summary>
public class AITankControl : MonoBehaviour
{
    [Tooltip("需要控制的单位.")]
    public UnitControl unit;
    
    [Tooltip("索敌组件.")]
    public AISearchEnemyControl searcher;
    
    [Tooltip("逃跑路点.")]
    public AIEscapeControl escapeSearcher;
    
    [Tooltip("躲藏路点.")]
    public AIHideControl hideSearcher;
    
    [Tooltip("搜索路径的冷却时间范围.")]
    public float minRepathCooldown;
    
    [Tooltip("搜索路径的冷却时间范围.")]
    public float maxRepathCooldown;
    
    [Tooltip("炮塔空闲时瞄准这个 transform 的 forward 方向.")]
    public Transform idleTrans;
    
    [Header("炮塔")]
    
    [Tooltip("瞄准容差. 单位角度.")]
    public float aimingTolerance;
    
    [Header("坦克")]
    
    [Tooltip("空闲时的巡逻路径.")]
    public PatrolControl patrolPath;
    
    [Tooltip("与路点的距离小于这个值时算作到达.")]
    public float reachRange;
    
    [Tooltip("最优攻击距离, 攻击状态接近到该距离后就停止.")]
    public float attackRange;
    
    [Tooltip("切换到逃跑状态的 hp 比例.")]
    public float strengthLowRate;
    
    [Tooltip("逃到多远就不逃了.")]
    public float escapeRange;
    
    [Header("状态参数")]
    
    // < field is assigned but never used > 警告
    // 纠正: 反射调用. 给编辑器.
    #pragma warning disable CS0414
    
    [Tooltip("当前状态.")]
    [ReadOnly] [SerializeField] string currentState;
    
    [Tooltip("再往哪里移动.")]
    [ReadOnly] [SerializeField] string moveState;
    
    #pragma warning restore CS0414
    
    /// <summary>
    /// 指示生命值是否过低.
    /// </summary>
    bool strengthTooLow => unit.currentStrength < unit.maxStrength * strengthLowRate;
    
    public TankControl tank => unit.GetComponent<TankControl>();
    
    public TurretControl turret => unit.GetComponent<TurretControl>();
    
    public LaunchControl[] launches => unit.GetComponent<LaunchGroupControl>().launches;
    
    /// <summary>
    /// AI状态机.
    /// </summary>
    StateMachine stm;


    // ================================================================================================================
    // 状态机
    // ================================================================================================================
    
    /// <summary>
    /// 走到某个地方的状态机.
    /// </summary>
    class TankMoveState : StateMachine
    {
        public AITankControl control;
        public Vector3 targetPos;
        public Func<Vector3> getTargetPos;
        public Action additionalAction;
        public Func<bool> stop;
        public override IEnumerable<Transfer> Step()
        {
            IReadOnlyList<Vector3> path = null;
            int cur = 1;
            
            // 路径更新.
            yield return CallPass(new StateMachine.Timer(
                control.GenerateRepathCooldown(),
                () => {
                    if(getTargetPos != null) targetPos = getTargetPos();
                    var newPath = control.FindPath(targetPos);
                    if(newPath != null) (path, cur) = (newPath, 1);
                }
            ));
            
            // 移动控制.
            while(true)
            {
                yield return Pass();
                additionalAction?.Invoke();
                if(path == null || (stop != null && stop()))
                {
                    control.moveState = "Stop";
                    control.Stop();
                    continue;
                }
                control.moveState = $"Goto { path[cur] }";
                Debug.DrawLine(control.unit.aimCenter, path[cur], Color.magenta);
                path.Draw(Color.blue);
                if(control.DirectGoto(path[cur])) cur = (cur + 1).ModSys(path.Count);
            }
        }
    }



    /// <summary>
    /// 巡逻状态.
    /// </summary>
    class TankPatrolState : StateMachine
    {
        public AITankControl control;
        public UnitControl hitBy;
        bool targetFound => control.searcher.target != null || hitBy != null;
        public override IEnumerable<Transfer> Step()
        {
            control.currentState = "Patrol";
            
            // 受攻击时, 标记一个 hitby.
            Signal<Signals.Hit>.Listen(HitBy);
            cancelCallback += () => Signal<Signals.Hit>.Remove(HitBy);
            
            foreach(var patrolPoint in control.patrolPath)
            {
                yield return Call(new TankMoveState() {
                    control = control,
                    targetPos = patrolPoint,
                    additionalAction = () => control.AimIdle(),
                    interrupt = () =>
                        targetFound                         // 发现了一个目标.
                        || ReachPatrolPoint(patrolPoint)    // 进入了巡逻路点范围, 直接走下一个巡逻路点.
                });
                
                if(targetFound)
                {
                    yield return Trans(new TankAttackState() {
                        control = control,
                        target = hitBy ? hitBy : control.searcher.target,
                        hitByTarget = hitBy != null
                    });
                }
                
                yield return Pass();
            }
        }
        
        bool ReachPatrolPoint(Vector3 pos) => control.patrolPath.Reached(control.unit.transform.position, pos);
        void HitBy(Signals.Hit hit)
        {
            if(hit.hit != control.unit) return;
            if(hit.source.factionId == hit.hit.factionId) return;
            hitBy = hit.source;
        }
    }
    
    
    
    /// <summary>
    /// 进攻状态.
    /// </summary>
    class TankAttackState : StateMachine
    {
        public AITankControl control;
        public UnitControl target;
        public bool hitByTarget;
        public bool hiding;
        Vector3 curPos => control.unit.transform.position;
        Vector3 targetPos;
        bool shouldPersue => targetPos.To(curPos).magnitude > control.attackRange;
        Vector3 hidePos;
        
        public override IEnumerable<Transfer> Step()
        {
            Debug.Assert(target != null);
            
            // 受攻击时, 目标可以被改变.
            Signal<Signals.Hit>.Listen(HitBy);
            cancelCallback += () => Signal<Signals.Hit>.Remove(HitBy);
            
            // 不停移动. 移动目标由 targetPos 控制.
            // 如果追击的是敌人不是路点, 那么足够靠近的时候就不追了.
            yield return CallPass(new TankMoveState() {
                control = control,
                getTargetPos = () => targetPos,
                stop = () => hiding ? control.Reached(targetPos) : !control.ShouldPersue(target)
            });
            
            // 总是在寻找可行躲藏路径...
            yield return CallPass(new StateMachine.Timer(
                control.GenerateRepathCooldown(),
                () => {
                    var moveTarget = control.hideSearcher.ClosetWaypoint(control.searcher, target, control.unit);
                    Debug.Assert(moveTarget != null);
                    hidePos = moveTarget.Value;
                    if(hiding) Debug.DrawLine(control.unit.aimCenter, hidePos, Color.yellow, 4);
                }
            ));
            
            while(true)
            {
                // 如果被打了, 就找个地方躲起来.
                if(!hiding && hitByTarget)
                {
                    control.currentState = "Attack Hide";
                    hitByTarget = false;
                    hiding = true;
                }
                
                // 已经躲好了, 并且敌人没有威胁值了, 继续打.
                if(hiding && control.Reached(targetPos) && target.threat == 0f)
                {
                    control.currentState = "Attack";
                    hiding = false;
                }
                
                // 正在躲藏的时候, 目标位置是躲藏点. 不在躲藏的话就直接跑到敌人跟前.
                if(hiding) targetPos = hidePos;
                else targetPos = target.transform.position;
                
                // 血量太低, 走人.
                if(control.strengthTooLow) yield return Trans(new TankEscapeState() { 
                    control = control,
                    target = target
                });
                
                TryAttack();
                
                // 敌人已经死了, 不用再打了.
                if(target.dead) yield return Trans(new TankPatrolState() { control = control });
                
                yield return Pass();
            }
        }
        
        void HitBy(Signals.Hit hit)
        {
            if(hit.hit != control.unit) return;
            if(hit.source.factionId == hit.hit.factionId) return;
            (target, hitByTarget) = (hit.source, true);
        }
        void TryAttack() { if(control.Aim(target)) control.Launch(); }
    }
    
    
    /// <summary>
    /// 逃跑状态.
    /// </summary>
    class TankEscapeState : StateMachine
    {
        public AITankControl control;
        public UnitControl target;
        bool farEnough => control.escapeRange < target.aimCenter.To(control.unit.aimCenter).magnitude;
        public override IEnumerable<Transfer> Step()
        {
            Debug.Assert(target != null);
            control.currentState = "Escape";
            float timer = 0f;
            var targetPos = Vector3.zero;
            
            // 即使到了路点也会继续移动.
            yield return CallPass(new TankMoveState() {
                control = control,
                getTargetPos = () => targetPos,
                additionalAction = () => control.AimIdle(),
                interrupt = () => farEnough || target.dead 
            });
            
            // 更新逃跑路线.
            while(true)
            {
                // 够远了, 或者目标挂掉了, 就不逃了.
                if(farEnough || target.dead) yield return Trans(new TankPatrolState() { control = control });
                
                yield return Pass();
                
                // 定时更新逃跑路线.
                if(timer.CountDownTimeLoop(control.GenerateRepathCooldown()))
                {
                    var res = control.escapeSearcher.EscapeWaypoint(target.transform.position);
                    if(res.HasValue) targetPos = res.Value;
                }
            }
        }
    }
    
    // ================================================================================================================
    // Unity 内置回调函数
    // ================================================================================================================
    
    
    void Start()
    {
        // 初始化状态机.
        stm = StateMachine.Register(new TankPatrolState() { control = this });
    }
    
    void OnDestroy()
    {
        // 删除状态机.
        StateMachine.Remove(stm.tag);
    }
    
    void OnDrawGizmos()
    {
        if(unit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(unit.aimCenter, attackRange);
        }
    }
    
    
    
    // ================================================================================================================
    // 通用函数
    // ================================================================================================================
    
    /// <summary>
    /// 使用 NavMesh 寻找一条到达目标点的路径.
    /// 输出到 path.
    /// 使用一个临时的 NavMeshPath, 和一个临时的 path.
    /// (副作用: 再一次在同一个线程调用 NavMeshPath 将会使得之前获取的路径失效. )
    /// </summary>
    IReadOnlyList<Vector3> FindPath(Vector3 targetPos)
    {
        var tempNavPath = new NavMeshPath();
        if(NavMesh.CalculatePath(tank.transform.position, targetPos, NavMesh.AllAreas, tempNavPath))
        {
            var tempPath = new Vector3[128];
            int cnt = tempNavPath.GetCornersNonAlloc(tempPath);
            return tempPath.Prefix(cnt);
        }
        
        return null;
    }
    
    /// <summary>
    /// 开火.
    /// </summary>
    void Launch()
    {
        foreach(var launcher in launches) launcher.TryFire();
    }
    
    /// <summary>
    /// 控制坦克移动.
    /// </summary>
    bool DirectGoto(Vector3 waypoint)
    {
        // 调整坦克方向.
        var target = tank.transform.InverseTransformPoint(waypoint);
        var targetDir = new Vector2(target.z, target.x);
        var targetAngle = targetDir.Angle().ToDeg();    // 范围 (-180, 180]
        
        // 如果目标路点并不在正前方, 先转向.
        if(targetAngle.Abs() > 2f)
        {
            tank.forwarding = 0f;
            tank.turning = targetAngle.Sgn();
        }
        else // 然后前进.
        {
            tank.forwarding = 1.0f;
            tank.turning = 0f;
        }
        
        return Reached(waypoint);
    }
    
    /// <summary>
    /// 让坦克停止移动.
    /// </summary>
    void Stop()
    {
        tank.turning = tank.forwarding = 0f;
    }
    
    /// <summary>
    /// 判断是否到达了路点(waypoint).
    /// </summary>
    bool Reached(Vector3 pos) => tank.transform.position.To(pos).magnitude < reachRange;
    
    /// <summary>
    /// 判断是否应当追击.
    /// </summary>
    bool ShouldPersue(UnitControl target) 
        => unit.aimCenter.To(target.aimCenter).magnitude > attackRange || !unit.CanSee(target);
    
    /// <summary>
    /// 生成一个冷却时间.
    /// </summary>
    float GenerateRepathCooldown() => (minRepathCooldown, maxRepathCooldown).Random();
    
    
    
    /// <summary>
    /// 设置当前瞄准状态为空闲瞄准状态.
    /// </summary>
    void AimIdle()
    {
        turret.targetDir = idleTrans ?  idleTrans.forward : turret.targetDir;
    }
    
    /// <summary>
    /// 瞄准某个东西.
    /// 返回是否已经瞄准了.
    /// </summary>
    bool Aim(UnitControl target)
    {
        // 有敌人, 计算落点....
        var aimingDir = Vector3.zero;
        var validPoint = 0;
        foreach(var launcher in launches)
        {
            var initalSpeed = launcher.cannonBallTemplate.speed;
            var launcherAimingDir = GetAimingPoint(initalSpeed, target.aimCenter, -target.body.velocity + unit.body.velocity, launcher.transform.position);
            aimingDir += launcherAimingDir.Value;
            validPoint += 1;
        }
        if(validPoint == 0) return false;
        aimingDir /= validPoint;
        turret.targetDir = aimingDir;
        return Vector3.Angle(turret.curDir, aimingDir) < aimingTolerance;
    }
    
    /// <summary>
    /// 获取目标方向. (考虑抛物线).
    /// </summary>
    /// <param name="initalSpeed">炮弹初始速度.</param>
    /// <param name="targetPos">目标当前位置.</param>
    /// <param name="targetRelativeVelocity">目标相对于自己的行进速度. 假设是匀速行进.</param>
    /// <param name="launchPoint">炮弹生成位置.</param>
    Vector3? GetAimingPoint(float initalSpeed, Vector3 targetPos, Vector3 targetRelativeVelocity, Vector3 launchPoint)
    {
        // 根据运动学定律列一个向量方程.
        // 方程三个变量, 时间, 发射俯仰角, 发射偏转角.
        // 方程三维的, 所以有三个独立等式.
        // 设其为 f(t, v) = 0
        // 给定命中时刻 t, 能根据式子轻易出速度方向. 即有 v = F(t)
        // 初始速度大小 |v| 已知, 两边平方, 套用四次方程求根公式.
        
        var g = Physics.gravity;
        var vt = targetRelativeVelocity;
        var pl = launchPoint;
        var pt = targetPos;
        var dp = pl - pt;
        var sol = Equation.Solve4(
            - 0.25f * g.Dot(g),
            - g.Dot(vt),
            g.Dot(dp) + vt.Dot(vt) - initalSpeed.Sqr(),
            2 * vt.Dot(dp),
            dp.sqrMagnitude
        );
        if(sol.x1.HasValue && float.IsInfinity(sol.x1.Value)) sol.x1 = null;
        if(sol.x2.HasValue && float.IsInfinity(sol.x2.Value)) sol.x2 = null;
        if(sol.x3.HasValue && float.IsInfinity(sol.x3.Value)) sol.x3 = null;
        if(sol.x4.HasValue && float.IsInfinity(sol.x4.Value)) sol.x4 = null;
        float? res = null;
        if(sol.x1.HasValue && ( res == null || (sol.x1.Value > 0 && sol.x1.Value < res.Value))) res = sol.x1;
        if(sol.x2.HasValue && ( res == null || (sol.x1.Value > 0 && sol.x2.Value < res.Value))) res = sol.x2;
        if(sol.x3.HasValue && ( res == null || (sol.x1.Value > 0 && sol.x3.Value < res.Value))) res = sol.x3;
        if(sol.x4.HasValue && ( res == null || (sol.x1.Value > 0 && sol.x4.Value < res.Value))) res = sol.x4;
        
        if(!res.HasValue) return null;
        
        var t = res.Value;
        return -0.5f * g * t - (pl - pt) / t - vt;
    }
}
