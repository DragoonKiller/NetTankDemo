using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制退出按钮的程序.
/// </summary>
[RequireComponent(typeof(Button))]
public class ReturnButtonControl : MonoBehaviour
{
    [Tooltip("Escspe面板.")]
    public GameObject menu;
    
    public Button button => this.GetComponent<Button>();
    
    void Start()
    {
        button.onClick.AddListener(ClosePanel);
    }
    
    void OnDestroy()
    {
        button.onClick.RemoveListener(ClosePanel);
    }
    
    void ClosePanel()
    {
        menu.SetActive(false);
    }
}
