using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Systems;

/// <summary>
/// 控制炮弹的飞行逻辑, 命中逻辑.
/// </summary>
public class CannonBallControl : MonoBehaviour
{
    [Tooltip("角度偏差. 单位:角度/秒.")]
    public float bias;
    
    [Tooltip("初始速度")]
    public float speed;
    
    [Tooltip("初始速度加成.")]
    public Vector3 appendVelocity;
    
    [Tooltip("炮弹是谁发射的.")]
    public UnitControl owner;
    
    [Tooltip("炮弹伤害.")]
    public float damage;
    
    [Tooltip("拖尾轨迹对象的延迟删除时间.")]
    public float trailDeleteTime;
    
    [Tooltip("拖尾特效对象.")]
    public TrailRenderer trail;
    
    [Tooltip("炮弹击中坦克特效.")]
    public GameObject[] hitTargetFX;
    
    [Tooltip("炮弹击中地面特效.")]
    public GameObject[] hitGroundFX;
    
    /// <summary>
    /// 基础偏差方向.
    /// </summary>
    Vector2 biasVec;
    
    /// <summary>
    /// 偏差方向随时间的转向角度.
    /// </summary>
    float biasDir;
    
    Rigidbody rd => this.GetComponent<Rigidbody>();
    
    void Start()
    {
        rd.velocity = this.transform.forward * speed + appendVelocity;
        biasVec = Vector2.right.Rot((0f, 360f).Random().ToRad());
        biasDir = (-Mathf.PI, Mathf.PI).Random();
    }
    
    void Update()
    {
        // 计算偏差.
        biasVec = biasVec.Rot(biasDir * Time.deltaTime);
        rd.velocity = Vector3.Slerp(
            rd.velocity,
            rd.velocity.magnitude * this.transform.TransformVector(biasVec),
            bias / 90 * Time.deltaTime);
        
        // 同步方向.
        this.transform.rotation = Quaternion.LookRotation(rd.velocity, this.transform.up);
    }
    
    void OnCollisionEnter(Collision x)
    {
        // 命中单位.
        if(x.collider.attachedRigidbody != null
            && x.collider.attachedRigidbody.TryGetComponent<UnitControl>(out var unit))
        {
            Signal.Emit(new Signals.Hit() { hit = unit, source = this.owner, amount = damage });
            foreach(var i in hitTargetFX) GameObject.Instantiate(i, this.transform.position, this.transform.rotation);
        }
        else // 命中地形.
        {
            foreach(var i in hitGroundFX) GameObject.Instantiate(i, this.transform.position, Quaternion.identity);
        }
        
        // 把拖尾特效丢到世界坐标去.
        trail.transform.parent = null;
        var trc = trail.gameObject.AddComponent<TrailControl>();
        trc.timeOut = trailDeleteTime;
        
        // 删除自己.
        GameObject.Destroy(this.gameObject);
    }
}
