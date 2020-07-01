using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制退出按钮的程序.
/// </summary>
[RequireComponent(typeof(Button))]
public class ExitButtonControl : MonoBehaviour
{
    [Tooltip("返回哪个场景.")]
    public string exitTo;
    
    public Button button => this.GetComponent<Button>();
    
    void Start()
    {
        button.onClick.AddListener(ChangeScene);
    }
    
    void OnDestroy()
    {
        button.onClick.RemoveListener(ChangeScene);
    }
    
    void ChangeScene()
    {
        Component.Destroy(Component.FindObjectOfType<Env>());
        SceneManager.UnloadSceneAsync(gameObject.scene);
        SceneManager.LoadSceneAsync(exitTo, LoadSceneMode.Additive);
    }
}
