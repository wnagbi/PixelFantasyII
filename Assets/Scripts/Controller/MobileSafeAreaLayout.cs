using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 将场景中已经存在的全屏 UI 根节点限制在设备 Safe Area 内。
/// </summary>
public sealed class MobileSafeAreaLayout : MonoBehaviour
{
    [SerializeField] private RectTransform[] safeAreaRoots;
    [SerializeField] private GameObject[] desktopOnlyObjects;
    [SerializeField] private Graphic[] touchTargetGraphics;
    [SerializeField] private Vector2 minimumTouchTargetSize = new Vector2(96f, 80f);
    [SerializeField] private bool simulateMobileInEditor;

    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;

    private void Awake()
    {
        if (!ShouldApplyMobileLayout())
        {
            enabled = false;
            return;
        }

        SetDesktopOnlyObjectsVisible(false);
        ExpandTouchTargets();
        ApplySafeArea(true);
    }

    private void Update()
    {
        ApplySafeArea(false);
    }

    /// <summary>
    /// 判断当前环境是否需要应用移动端安全区布局。
    /// </summary>
    /// <remarks>
    /// 使用注意：Android 构建自动启用；普通 Editor Play 需要勾选模拟开关，Windows 构建不会修改布局。
    /// </remarks>
    private bool ShouldApplyMobileLayout()
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
    /// 将所有已绑定根节点的锚点更新为当前设备 Safe Area。
    /// </summary>
    /// <remarks>
    /// 使用注意：只在安全区或分辨率变化时更新，根节点本身应保持四边拉伸且 Offset 为零。
    /// </remarks>
    private void ApplySafeArea(bool force)
    {
        if (safeAreaRoots == null || Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        Rect safeArea = Screen.safeArea;
        Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
        if (!force && safeArea == lastSafeArea && screenSize == lastScreenSize)
        {
            return;
        }

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        foreach (RectTransform root in safeAreaRoots)
        {
            if (root == null)
            {
                continue;
            }

            root.anchorMin = anchorMin;
            root.anchorMax = anchorMax;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
        }

        lastSafeArea = safeArea;
        lastScreenSize = screenSize;
    }

    /// <summary>
    /// 统一设置只对桌面平台有意义的设置控件。
    /// </summary>
    /// <remarks>
    /// 使用注意：Android 中用于隐藏分辨率和全屏选项；Windows 分支不会执行该方法。
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
    /// 扩大偏小控件的透明射线命中区域，不拉伸 UI 美术。
    /// </summary>
    /// <remarks>
    /// 使用注意：适合 Dropdown、Toggle 等视觉高度较小的控件；相邻控件之间应留有足够间距，避免命中区域重叠。
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
