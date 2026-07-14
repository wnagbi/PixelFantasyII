using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;
using DG.Tweening;

// 游戏内 HUD 控制器。
// 负责经验条、血量条、暂停面板、击杀数文本等战斗 UI。
public class UIController : MonoBehaviour
{
    [Header("Exp")]
    public Slider expSlider;
    public Text expText;
    [Header("Hp")]
    public Image hpMaskimage;
    private float originalSize;
    [Header("Esc Panel")]
    public GameObject escPanel;
    [Header("Kill Number")]
    public Text killNumber;

    public LocalizedString nameString;


    private Tween hpTween;
    private bool isEsc;
    private void Awake()
    {       
        // 记录血条遮罩原始宽度，之后按血量比例缩放宽度。
        originalSize = hpMaskimage.rectTransform.rect.width;
        nameString.TableEntryReference = "KillNumText";
    }
    private void OnEnable()
    {
        // 订阅 PlayerData 血量变化事件。只有通过 AddHealth/TakeDamage/SetHealth 改血才会自动刷新 UI。
        PlayerData.getInstance().OnHealthChanged += UpdateHealthUI;
    }
    private void OnDisable()
    {
        // 取消订阅，避免对象销毁后事件还调用旧 UI。
        PlayerData.getInstance().OnHealthChanged -= UpdateHealthUI;
    }
    public void OnEsc()
    {
        // 切换暂停面板显示，同时暂停/恢复游戏时间。
        escPanel.SetActive(!escPanel.activeSelf);
        if (escPanel.activeSelf)
        {
            Time.timeScale = 0f;
        }
        else 
        {
            Time.timeScale = 1f;
        }
    }
    private void Update()
    {
        setKillNumber();
    }

    public void UpdateExp(int currtExp,int levelExp,int currtLevel)
    {
        // 刷新经验条和等级文本。
        expSlider.maxValue = levelExp;
        expSlider.value = currtExp;      
        expText.text = "Level: "+ currtLevel.ToString();
    }
    public void SetHPValue(float fillPercent)
    {
        fillPercent = Mathf.Clamp01(fillPercent);

        RectTransform rect = hpMaskimage.rectTransform;
        float targetWidth = fillPercent * originalSize;

        hpTween?.Kill();

        float currentWidth = rect.rect.width;

        hpTween = DOTween.To(
            () => currentWidth,
            value =>
            {
                rect.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    value
                );
            },
            targetWidth,
            0.15f
        ).SetEase(Ease.OutQuad);
        // 通过改变遮罩宽度实现血条填充效果。
        // hpMaskimage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillPercent * originalSize);
    }
    private void UpdateHealthUI(float current, float max)
    {
        // PlayerData 血量变化事件回调。
        SetHPValue(current / max);
    }

    public void setKillNumber() 
    {
        // 从 PlayerPrefs 读取击杀数并刷新本地化文本。
        killNumber.text = $"{nameString.GetLocalizedString()}" + PlayerPrefs.GetInt("KillNum").ToString();
    }
    public void QuitGame() 
    {
        // 打包后退出游戏。
        Application.Quit();
    }
}
