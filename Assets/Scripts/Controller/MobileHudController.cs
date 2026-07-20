using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 管理战斗场景中的移动端专属 HUD，包括平台开关、Safe Area 适配和技能按钮布局。
/// </summary>
public sealed class MobileHudController : MonoBehaviour
{
    [Header("移动端根节点")]
    [SerializeField] private GameObject mobileRoot;
    [SerializeField] private RectTransform safeAreaRoot;

    [Header("移动端技能栏")]
    [SerializeField] private RectTransform mobileSkillBar;
    [SerializeField] private RectTransform magnetButton;
    [SerializeField] private RectTransform rageButton;
    [SerializeField] private RectTransform dimensionSlashButton;

    [Header("移动端技能图标")]
    [SerializeField] private RectTransform magnetVisual;
    [SerializeField] private RectTransform rageVisual;
    [SerializeField] private RectTransform dimensionSlashVisual;
    [SerializeField] private float magnetVisualSize = 188f;
    [SerializeField] private float rageVisualSize = 124f;
    [SerializeField] private float dimensionSlashVisualSize = 118f;
    [SerializeField] private float cooldownOverlaySize = 112f;

    [Header("战斗 HUD Safe Area")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private RectTransform experienceBar;
    [SerializeField] private RectTransform timeDisplay;
    [SerializeField] private RectTransform killCountDisplay;

    [Header("PC 输入提示")]
    [SerializeField] private GameObject[] desktopInputPrompts;

    [Header("桌面平台专属设置")]
    [SerializeField] private GameObject[] desktopOnlyObjects;

    [Header("移动端触摸区域")]
    [SerializeField] private Graphic[] touchTargetGraphics;
    [SerializeField] private Vector2 minimumTouchTargetSize = new Vector2(96f, 80f);

    [Header("编辑器调试")]
    [SerializeField] private bool simulateMobileInEditor;

    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;
    private float lastCanvasScaleFactor = -1f;
    private Vector2 healthBarStartPosition;
    private Vector2 experienceBarStartPosition;
    private Vector2 timeDisplayStartPosition;
    private Vector2 killCountStartPosition;

    private void Awake()
    {
        bool mobileHudEnabled = ShouldEnableMobileHud();

        if (mobileRoot != null)
        {
            mobileRoot.SetActive(mobileHudEnabled);
        }

        if (!mobileHudEnabled)
        {
            enabled = false;
            return;
        }

        CacheGameplayHudPositions();
        SetDesktopPromptsVisible(false);
        SetDesktopOnlyObjectsVisible(false);
        ExpandTouchTargets();
        ArrangeMobileSkillButtons();
        ApplySafeArea(true);
    }

    private void Update()
    {
        ApplySafeArea(false);
    }

    /// <summary>
    /// 判断当前运行环境是否应启用移动端 HUD。
    /// </summary>
    /// <remarks>
    /// 使用注意：普通 Editor Play 和非 Android 构建始终返回 false，只有勾选模拟开关时 Editor 才会显示移动端布局。
    /// </remarks>
    private bool ShouldEnableMobileHud()
    {
#if UNITY_EDITOR
        return simulateMobileInEditor;
#elif UNITY_ANDROID
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    /// 把现有技能按钮重挂到移动端技能栏并按左下横排布局。
    /// </summary>
    /// <remarks>
    /// 使用注意：这里只移动原按钮，不复制技能对象，因此原有冷却、点击和 Addressables 图标逻辑仍由同一套组件负责。
    /// </remarks>
    private void ArrangeMobileSkillButtons()
    {
        if (mobileSkillBar == null)
        {
            Debug.LogWarning("[MobileHud] MobileSkillBar 未绑定，跳过移动端技能布局。", this);
            return;
        }

        ArrangeSkillButton(magnetButton, 0);
        ArrangeSkillButton(rageButton, 1);
        ArrangeSkillButton(dimensionSlashButton, 2);

        // 三张原图虽然都是 32x32，但透明边距不同，使用不同 Rect 尺寸后可见图案才会接近同样大小。
        ArrangeSkillVisual(magnetVisual, magnetVisualSize, cooldownOverlaySize);
        ArrangeSkillVisual(rageVisual, rageVisualSize, cooldownOverlaySize);
        ArrangeSkillVisual(dimensionSlashVisual, dimensionSlashVisualSize, cooldownOverlaySize);
    }

    /// <summary>
    /// 将一个技能按钮放到移动技能栏的指定横向槽位。
    /// </summary>
    /// <remarks>
    /// 使用注意：槽位按 150 像素按钮宽度和 20 像素间距计算，调用前应保证传入的是技能按钮最外层 RectTransform。
    /// </remarks>
    private void ArrangeSkillButton(RectTransform button, int slotIndex)
    {
        if (button == null)
        {
            return;
        }

        button.SetParent(mobileSkillBar, false);
        button.SetSiblingIndex(slotIndex);
        button.anchorMin = new Vector2(0f, 0.5f);
        button.anchorMax = new Vector2(0f, 0.5f);
        button.pivot = new Vector2(0f, 0.5f);
        button.sizeDelta = new Vector2(150f, 150f);
        button.anchoredPosition = new Vector2(slotIndex * 170f, 0f);
        button.localScale = Vector3.one;
    }

    /// <summary>
    /// 统一移动端技能图标和冷却遮罩的中心位置与显示尺寸。
    /// </summary>
    /// <remarks>
    /// 使用注意：size 指的是完整 Sprite Rect 的尺寸；不同图片的透明边距不同，因此不应强制使用同一个数值。
    /// </remarks>
    private static void ArrangeSkillVisual(RectTransform visual, float size, float overlaySize)
    {
        if (visual == null || size <= 0f)
        {
            return;
        }

        visual.anchorMin = new Vector2(0.5f, 0.5f);
        visual.anchorMax = new Vector2(0.5f, 0.5f);
        visual.pivot = new Vector2(0.5f, 0.5f);
        visual.anchoredPosition = Vector2.zero;
        visual.sizeDelta = new Vector2(size, size);
        visual.localScale = Vector3.one;

        // 当前三个技能都把径向冷却 Image 放在图标的第一个子节点中。
        // 遮罩没有技能原图的透明边距，因此统一使用独立的可见尺寸，避免进入 CD 后大小突然变化。
        if (visual.childCount > 0 && visual.GetChild(0) is RectTransform cooldownOverlay)
        {
            float safeOverlaySize = overlaySize > 0f ? overlaySize : size;
            cooldownOverlay.anchorMin = new Vector2(0.5f, 0.5f);
            cooldownOverlay.anchorMax = new Vector2(0.5f, 0.5f);
            cooldownOverlay.pivot = new Vector2(0.5f, 0.5f);
            cooldownOverlay.anchoredPosition = Vector2.zero;
            cooldownOverlay.sizeDelta = new Vector2(safeOverlaySize, safeOverlaySize);
            cooldownOverlay.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// 根据当前屏幕 Safe Area 更新移动端安全区锚点。
    /// </summary>
    /// <remarks>
    /// 使用注意：只有分辨率或 Safe Area 发生变化时才会写入 RectTransform，避免每帧产生无意义的布局更新。
    /// </remarks>
    private void ApplySafeArea(bool force)
    {
        if (safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        Rect safeArea = Screen.safeArea;
        Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
        float canvasScaleFactor = GetCanvasScaleFactor();
        if (!force
            && safeArea == lastSafeArea
            && screenSize == lastScreenSize
            && Mathf.Approximately(canvasScaleFactor, lastCanvasScaleFactor))
        {
            return;
        }

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        safeAreaRoot.anchorMin = anchorMin;
        safeAreaRoot.anchorMax = anchorMax;
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;

        ApplyGameplayHudInsets(safeArea, canvasScaleFactor);

        lastSafeArea = safeArea;
        lastScreenSize = screenSize;
        lastCanvasScaleFactor = canvasScaleFactor;
    }

    /// <summary>
    /// 记录战斗 HUD 在 1920x1080 设计稿中的初始位置。
    /// </summary>
    /// <remarks>
    /// 使用注意：记录必须发生在第一次应用 Safe Area 之前，旋转屏幕时始终从初始位置重新计算，避免偏移累加。
    /// </remarks>
    private void CacheGameplayHudPositions()
    {
        healthBarStartPosition = GetAnchoredPosition(healthBar);
        experienceBarStartPosition = GetAnchoredPosition(experienceBar);
        timeDisplayStartPosition = GetAnchoredPosition(timeDisplay);
        killCountStartPosition = GetAnchoredPosition(killCountDisplay);
    }

    /// <summary>
    /// 按设备四边安全距离偏移战斗 HUD 的边缘元素。
    /// </summary>
    /// <remarks>
    /// 使用注意：这里只移动血条、经验、时间和击杀数，不会改变暂停、升级或技能逻辑。
    /// </remarks>
    private void ApplyGameplayHudInsets(Rect safeArea, float scaleFactor)
    {
        float leftInset = safeArea.xMin / scaleFactor;
        float rightInset = (Screen.width - safeArea.xMax) / scaleFactor;
        float topInset = (Screen.height - safeArea.yMax) / scaleFactor;

        SetAnchoredPosition(healthBar, healthBarStartPosition + new Vector2(leftInset, -topInset));
        SetAnchoredPosition(experienceBar, experienceBarStartPosition + new Vector2(leftInset, -topInset));
        SetAnchoredPosition(timeDisplay, timeDisplayStartPosition + new Vector2(0f, -topInset));
        SetAnchoredPosition(killCountDisplay, killCountStartPosition + new Vector2(-rightInset, -topInset));
    }

    /// <summary>
    /// 读取当前 Canvas 的实际缩放系数，供屏幕像素和 UI 设计坐标之间换算。
    /// </summary>
    /// <remarks>
    /// 使用注意：CanvasScaler 可能在 Awake 之后才更新 scaleFactor，因此调用方必须持续检测该值是否变化。
    /// </remarks>
    private float GetCanvasScaleFactor()
    {
        return rootCanvas != null && rootCanvas.scaleFactor > 0f
            ? rootCanvas.scaleFactor
            : 1f;
    }

    /// <summary>
    /// 安全读取可选 RectTransform 的锚点坐标。
    /// </summary>
    private static Vector2 GetAnchoredPosition(RectTransform target)
    {
        return target != null ? target.anchoredPosition : Vector2.zero;
    }

    /// <summary>
    /// 安全写入可选 RectTransform 的锚点坐标。
    /// </summary>
    private static void SetAnchoredPosition(RectTransform target, Vector2 position)
    {
        if (target != null)
        {
            target.anchoredPosition = position;
        }
    }

    /// <summary>
    /// 统一设置键盘和手柄按键提示的显示状态。
    /// </summary>
    /// <remarks>
    /// 使用注意：移动端只隐藏按键提示，不隐藏技能按钮本身。
    /// </remarks>
    private void SetDesktopPromptsVisible(bool visible)
    {
        if (desktopInputPrompts == null)
        {
            return;
        }

        foreach (GameObject prompt in desktopInputPrompts)
        {
            if (prompt != null)
            {
                prompt.SetActive(visible);
            }
        }
    }

    /// <summary>
    /// 设置只在桌面平台显示的战斗内设置项。
    /// </summary>
    /// <remarks>
    /// 使用注意：Android 中隐藏分辨率和全屏选项，PC 分支不会调用该方法。
    /// </remarks>
    private void SetDesktopOnlyObjectsVisible(bool visible)
    {
        if (desktopOnlyObjects == null)
        {
            return;
        }

        foreach (GameObject desktopOnlyObject in desktopOnlyObjects)
        {
            if (desktopOnlyObject != null)
            {
                desktopOnlyObject.SetActive(visible);
            }
        }
    }

    /// <summary>
    /// 扩大战斗内设置中偏小控件的透明触摸区域。
    /// </summary>
    /// <remarks>
    /// 使用注意：只修改 Graphic 的射线 Padding，不改变语言下拉框和震动开关的视觉尺寸。
    /// </remarks>
    private void ExpandTouchTargets()
    {
        if (touchTargetGraphics == null)
        {
            return;
        }

        foreach (Graphic graphic in touchTargetGraphics)
        {
            if (graphic == null)
            {
                continue;
            }

            Rect rect = graphic.rectTransform.rect;
            float horizontalPadding = Mathf.Max(0f, (minimumTouchTargetSize.x - rect.width) * 0.5f);
            float verticalPadding = Mathf.Max(0f, (minimumTouchTargetSize.y - rect.height) * 0.5f);
            graphic.raycastPadding = new Vector4(
                -horizontalPadding,
                -verticalPadding,
                -horizontalPadding,
                -verticalPadding);
        }
    }
}
