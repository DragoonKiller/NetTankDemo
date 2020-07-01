using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class TrailControl : MonoBehaviour
{
    [Tooltip("删除时间.")]
    public float timeOut;
    
    TrailRenderer tr => this.GetComponent<TrailRenderer>();
    
    
    void Update()
    {
        timeOut -= Time.deltaTime;
        if(timeOut <= 0) Destroy(this.gameObject);
    }
    
}
