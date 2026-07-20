using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 在不同平台和宽高比下保持设置、地图选择界面的原始 16:9 版式。
/// 桌面端按完整屏幕等比适配，移动端额外避让 Safe Area 并应用触摸专属设置。
/// </summary>
public sealed class MobileSafeAreaLayout : MonoBehaviour
{
    [FormerlySerializedAs("safeAreaRoots")]
    [SerializeField] private RectTransform[] fullScreenRoots;
    [SerializeField] private RectTransform[] fittedContentRoots;
    [SerializeField] private GameObject[] desktopOnlyObjects;
    [SerializeField] private Graphic[] touchTargetGraphics;
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private Vector2 contentPadding = new Vector2(48f, 32f);
    [SerializeField] private Vector2 minimumTouchTargetSize = new Vector2(96f, 80f);
    [SerializeField, Range(0.5f, 1f)] private float minimumContentScale = 0.5f;
    [SerializeField] private bool allowContentUpscale;
    [SerializeField] private bool simulateMobileInEditor;

    private Canvas rootCanvas;
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;
    private float lastCanvasScaleFactor = -1f;
    private bool useMobileFeatures;

    private void Awake()
    {
        useMobileFeatures = ShouldUseMobileFeatures();
        rootCanvas = GetComponentInParent<Canvas>();
        SetDesktopOnlyObjectsVisible(!useMobileFeatures);
        if (useMobileFeatures)
        {
            ExpandTouchTargets();
        }

        StretchBackgroundRoots();
        Canvas.ForceUpdateCanvases();
        FitPanelsToAvailableArea(true);
    }

    private void Update()
    {
        FitPanelsToAvailableArea(false);
    }

    /// <summary>
    /// 判断当前运行环境是否需要启用 Safe Area、触摸区和移动端专属显隐。
    /// 使用注意：桌面端仍会执行 16:9 画框适配，但不会执行这里的移动端附加规则。
    /// </summary>
    private bool ShouldUseMobileFeatures()
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
    /// 保证最外层 UI 根节点铺满 Canvas，为非 16:9 屏幕提供稳定的底色区域。
    /// </summary>
    private void StretchBackgroundRoots()
    {
        if (fullScreenRoots == null)
        {
            return;
        }

        foreach (RectTransform root in fullScreenRoots)
        {
            if (root == null)
            {
                continue;
            }

            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
        }
    }

    /// <summary>
    /// 将内容层的父级画框等比缩放到当前可用区域，保持背景槽位与按钮位置一致。
    /// 使用注意：fittedContentRoots 应绑定 SettingContent、MapSelectContent 等内容层，
    /// 它们的直接父节点必须是包含背景图片的固定 1920x1080 画框，而不是全屏遮罩层。
    /// </summary>
    private void FitPanelsToAvailableArea(bool force)
    {
        if (fittedContentRoots == null
            || Screen.width <= 0
            || Screen.height <= 0
            || referenceResolution.x <= 0f
            || referenceResolution.y <= 0f)
        {
            return;
        }

        Rect safeArea = useMobileFeatures
            ? Screen.safeArea
            : new Rect(0f, 0f, Screen.width, Screen.height);
        Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
        float canvasScaleFactor = GetCanvasScaleFactor();
        if (!force
            && safeArea == lastSafeArea
            && screenSize == lastScreenSize
            && Mathf.Approximately(canvasScaleFactor, lastCanvasScaleFactor))
        {
            return;
        }

        Vector2 activePadding = useMobileFeatures ? contentPadding : Vector2.zero;
        Vector2 availableSize = safeArea.size / canvasScaleFactor;
        Vector2 fitSize = new Vector2(
            Mathf.Max(1f, availableSize.x - activePadding.x * 2f),
            Mathf.Max(1f, availableSize.y - activePadding.y * 2f));
        float panelScale = Mathf.Min(
            fitSize.x / referenceResolution.x,
            fitSize.y / referenceResolution.y);
        float maximumScale = allowContentUpscale ? float.MaxValue : 1f;
        float minimumScale = useMobileFeatures ? minimumContentScale : 0.1f;
        panelScale = Mathf.Clamp(panelScale, minimumScale, maximumScale);

        Vector2 safeCenterOffset = (safeArea.center
            - new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)) / canvasScaleFactor;

        foreach (RectTransform contentRoot in fittedContentRoots)
        {
            if (contentRoot == null || contentRoot.parent is not RectTransform panelRoot)
            {
                continue;
            }

            PreparePanelRoot(panelRoot, panelScale, safeCenterOffset);
            PrepareContentRoot(contentRoot);
        }

        lastSafeArea = safeArea;
        lastScreenSize = screenSize;
        lastCanvasScaleFactor = canvasScaleFactor;
    }

    /// <summary>
    /// 将包含背景图片的面板恢复为 1920×1080，并整体缩放到安全区中央。
    /// </summary>
    private void PreparePanelRoot(RectTransform panelRoot, float panelScale, Vector2 safeCenterOffset)
    {
        panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
        panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
        panelRoot.pivot = new Vector2(0.5f, 0.5f);
        panelRoot.anchoredPosition = safeCenterOffset;
        panelRoot.sizeDelta = referenceResolution;
        panelRoot.localScale = Vector3.one * panelScale;
    }

    /// <summary>
    /// 让内容层与父面板使用相同设计坐标，避免按钮相对背景发生二次缩放或偏移。
    /// </summary>
    private void PrepareContentRoot(RectTransform contentRoot)
    {
        contentRoot.anchorMin = new Vector2(0.5f, 0.5f);
        contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
        contentRoot.pivot = new Vector2(0.5f, 0.5f);
        contentRoot.anchoredPosition = Vector2.zero;
        contentRoot.sizeDelta = referenceResolution;
        contentRoot.localScale = Vector3.one;
    }

    /// <summary>
    /// 获取 Canvas 当前缩放比例，供屏幕像素与 UI 设计坐标互相转换。
    /// 使用注意：CanvasScaler 可能在 Awake 后才更新该值，因此 Update 会持续检测变化。
    /// </summary>
    private float GetCanvasScaleFactor()
    {
        return rootCanvas != null && rootCanvas.scaleFactor > 0f
            ? rootCanvas.scaleFactor
            : 1f;
    }

    /// <summary>
    /// 隐藏只适用于桌面平台的设置项。
    /// 使用注意：当前用于 Android 隐藏分辨率和全屏选项。
    /// </summary>
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
    /// 扩大偏小控件的透明射线命中区域，但不拉伸 UI 图片。
    /// 使用注意：相邻控件需要保留足够间距，避免触摸区域重叠。
    /// </summary>
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
