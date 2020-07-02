using System;
using Utils;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public static CameraZoom inst;
    
    CameraZoom() => inst = this;
    
    [Tooltip("放大后的视角乘数.")]
    public float fieldOfViewMult;
    
    [Tooltip("放大时的灵敏度乘数.")]
    public float sensitivityMult;
    
    [Tooltip("放大时的准星.")]
    public GameObject zoomCrosshair;
    
    [Tooltip("平时的准星.")]
    public GameObject ordinaryCrosshair;
    
    [Header("状态参数")]
    
    [Tooltip("初始视角状态.")]
    [SerializeField] float baseFieldOfView;
    
    [Tooltip("初始灵敏度.")]
    [SerializeField] float baseSensitivity;
    
    CameraFollow follow => CameraFollow.inst;
    
    void Start()
    {
        baseFieldOfView = Camera.main.fieldOfView;
        baseSensitivity = follow.sensitivity;
    }
    
    void Update()
    {
        if(!follow.working) return;
        
        if(Input.GetKey(KeyCode.Mouse1))
        {
            zoomCrosshair.SetActive(true);
            ordinaryCrosshair.SetActive(false);
            Camera.main.fieldOfView = fieldOfViewMult * baseFieldOfView;
            follow.sensitivity = baseSensitivity * sensitivityMult;
        }
        else
        {
            zoomCrosshair.SetActive(false);
            ordinaryCrosshair.SetActive(true);
            Camera.main.fieldOfView = baseFieldOfView;
            follow.sensitivity = baseSensitivity;
        }
    }
}
