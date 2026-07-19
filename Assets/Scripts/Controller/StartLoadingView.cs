using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// start 场景里的加载界面视图。
// UI 对象必须在 Unity 场景中手动创建并拖到这些字段上，本脚本不负责创建 UI。
public sealed class StartLoadingView : MonoBehaviour
{
    public Slider progressSlider;
    public Text statusText;
    public Text percentText;

    [SerializeField] private float smoothDuration = 0.18f;

    private float currentValue;

    /// <summary>
    /// 所有入口先限制到 0~1，Slider 和百分比文字始终保持一致。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 StartLoadingView 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void SetProgress(float value, string status)
    {
        // 所有入口先限制到 0~1，Slider 和百分比文字始终保持一致。
        currentValue = Mathf.Clamp01(value);

        if (progressSlider != null)
        {
            progressSlider.value = currentValue;
        }

        if (statusText != null && !string.IsNullOrEmpty(status))
        {
            statusText.text = status;
        }

        if (percentText != null)
        {
            percentText.text = Mathf.RoundToInt(currentValue * 100f) + "%";
        }
    }

    /// <summary>
    /// 使用 unscaledDeltaTime，避免启动阶段或 timeScale 为 0 时进度条停止。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 StartLoadingView 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public IEnumerator SmoothTo(float target, string status)
    {
        // 使用 unscaledDeltaTime，避免启动阶段或 timeScale 为 0 时进度条停止。
        target = Mathf.Clamp01(target);
        float start = progressSlider != null ? progressSlider.value : currentValue;
        float elapsed = 0f;

        if (statusText != null && !string.IsNullOrEmpty(status))
        {
            statusText.text = status;
        }

        while (elapsed < smoothDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = smoothDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / smoothDuration);
            SetProgress(Mathf.Lerp(start, target, t), status);
            yield return null;
        }

        SetProgress(target, status);
    }
}
