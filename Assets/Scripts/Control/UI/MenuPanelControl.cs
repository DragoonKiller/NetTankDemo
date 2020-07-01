using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Systems;
using System.Collections.Generic;

public class MenuPanelControl : MonoBehaviour
{
    [Tooltip("菜单面板.")]
    public GameObject menu;
    
    /// <summary>
    /// 按键监听状态机.
    /// </summary>
    public StateMachine stm;
    
    /// <summary>
    /// 玩家控制器.
    /// </summary>
    public PlayerControl playerControl;
    
    void Start()
    {
        // 注册了的函数总是在执行.
        StateMachine.Register(stm = new SyncMenuStateMachine() { control = this });
    }
    
    void OnDestroy()
    {
        StateMachine.Remove(stm.tag);
    }

    class SyncMenuStateMachine : StateMachine
    {
        public MenuPanelControl control;
        
        public override IEnumerable<Transfer> Step()
        {
            // 初始先把菜单面板关掉.
            control.menu.SetActive(false);
            
            while(true)
            {
                yield return Pass();
                if(Input.GetKeyDown(KeyCode.Escape))
                {
                    if(control.menu.activeSelf)
                    {
                        control.menu.SetActive(false);
                        ExCursor.cursorLocked = true;
                        control.playerControl.enabled = true;
                    }
                    else
                    {
                        control.menu.SetActive(true);
                        ExCursor.cursorLocked = false;
                        control.playerControl.enabled = false;
                    }
                }
            }
        }
    }
}
