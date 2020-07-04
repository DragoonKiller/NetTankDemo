using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

/// <summary>
/// 控制血条行为.
/// </summary>
[ExecuteAlways]
public class ClipAmmoCountDisplay : MonoBehaviour
{
    public BattleHUDDisplay hud;
    
    public PlayerControl player => hud.player;
    
    [Tooltip("需要控制的UI文本. 数量应该和 launch point 数量一致.")]
    public Text[] ammoTexts;
    
    [Tooltip("需要控制的UI文本. 数量应该和 launch point 数量一致.")]
    public Text[] maxAmmoTexts;
    
    [Tooltip("弹药条显示图片.")]
    public Image[] images;
    
    [Tooltip("显示装填中的文本.")]
    public Text reloadingText;
    
    [Tooltip("文本颜色.")]
    public Color ammoColor;
    
    [Tooltip("文本颜色.")]
    public Color maxAmmoColor;
    
    [Tooltip("装填时的弹药条颜色.")]
    public Color reloadingColor;
    
    [Tooltip("弹药条颜色.")]
    public Color clipColor;
    
    public float clipMult => 1f / player.launches.Length;
    
    void Update()
    {
        bool reloading = false;
        for(int i = 0; i < player.launches.Length; i++)
        {
            var launcher = player.launches[i];
            var image = images[i];
            var text = ammoTexts[i];
            var maxText = maxAmmoTexts[i];
            text.text = launcher.clipAmmo.ToString().WithColor(ammoColor);
            maxText.text = launcher.clipMaxAmmo.ToString().WithColor(maxAmmoColor);
            if(launcher.reloading)
            {
                image.color = reloadingColor;
                image.fillAmount = clipMult * (1 - launcher.reloadingProcess / launcher.reloadingTime);
                reloading = true;
            }
            else
            {
                image.color = clipColor;
                image.fillAmount = clipMult * launcher.clipAmmo / launcher.clipMaxAmmo;
            }
            
            ammoTexts[i].gameObject.SetActive(true);
            maxAmmoTexts[i].gameObject.SetActive(true);
            images[i].gameObject.SetActive(true);
        }
        
        // 屏蔽掉多余的文本.
        for(int i = player.launches.Length; i < ammoTexts.Length; i++)
        {
            ammoTexts[i].gameObject.SetActive(false);
            maxAmmoTexts[i].gameObject.SetActive(false);
            images[i].gameObject.SetActive(false);
        }
        
        reloadingText.enabled = reloading;
    }
    
}
