using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

/// <summary>
/// 战斗 HUD 控制器。
/// 只负责显示血量、经验、击杀数和暂停面板，不再每帧主动轮询数据。
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("Exp")]
    public Slider expSlider;
    public Text expText;

    [Header("Hp")]
    public Image hpMaskimage;

    [Header("Esc Panel")]
    public GameObject escPanel;

    [Header("Kill Number")]
    public Text killNumber;

    public LocalizedString nameString;

    private float originalSize;
    private Tween hpTween;
    private string killLabel = string.Empty;

    private void Awake()
    {
        // 记录血条遮罩的初始宽度，后续按血量百分比缩放宽度。
        originalSize = hpMaskimage.rectTransform.rect.width;
        nameString.TableEntryReference = "KillNumText";
    }

    private void OnEnable()
    {
        // 事件驱动刷新：数据变化时才更新 UI。
        GameEvents.HealthChanged += UpdateHealthUI;
        GameEvents.ExpChanged += UpdateExp;
        GameEvents.KillCountChanged += UpdateKillNumber;
        nameString.StringChanged += OnKillLabelChanged;

        // UI 刚启用时主动刷新一次，避免等下一次事件前显示旧内容。
        PlayerData data = PlayerData.getInstance();
        UpdateHealthUI(data.CurrentHealth, data.CurrentMaxHealth);
        UpdateKillNumber(RunData.KillCount);
    }

    private void OnDisable()
    {
        // 取消订阅，避免场景切换或对象禁用后事件继续访问旧 UI。
        GameEvents.HealthChanged -= UpdateHealthUI;
        GameEvents.ExpChanged -= UpdateExp;
        GameEvents.KillCountChanged -= UpdateKillNumber;
        nameString.StringChanged -= OnKillLabelChanged;
        hpTween?.Kill();
    }

    public void OnEsc()
    {
        escPanel.SetActive(!escPanel.activeSelf);
        Time.timeScale = escPanel.activeSelf ? 0f : 1f;
    }

    public void UpdateExp(int currentExp, int levelExp, int currentLevel)
    {
        // ExpController 只负责计算经验变化，这里只负责显示。
        expSlider.maxValue = Mathf.Max(1, levelExp);
        expSlider.value = Mathf.Clamp(currentExp, 0, levelExp);
        expText.text = "Level: " + currentLevel.ToString();
    }

    public void SetHPValue(float fillPercent)
    {
        fillPercent = Mathf.Clamp01(fillPercent);

        RectTransform rect = hpMaskimage.rectTransform;
        float targetWidth = fillPercent * originalSize;

        // 新血量动画开始前停止旧动画，避免连续受伤/回血时多个 Tween 抢同一个宽度。
        hpTween?.Kill();

        float currentWidth = rect.rect.width;
        hpTween = DOTween.To(
            () => currentWidth,
            value =>
            {
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
            },
            targetWidth,
            0.15f
        ).SetEase(Ease.OutQuad);
    }

    private void UpdateHealthUI(float current, float max)
    {
        SetHPValue(max <= 0f ? 0f : current / max);
    }

    private void UpdateKillNumber(int killCount)
    {
        if (killNumber != null)
        {
            killNumber.text = killLabel + killCount.ToString();
        }
    }

    private void OnKillLabelChanged(string localizedText)
    {
        killLabel = localizedText;
        UpdateKillNumber(RunData.KillCount);
    }

    public void setKillNumber()
    {
        // 保留旧公开方法，兼容可能存在的 Inspector 按钮或旧脚本调用。
        UpdateKillNumber(RunData.KillCount);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
