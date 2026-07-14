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

    public void SetProgress(float value, string status)
    {
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

    public IEnumerator SmoothTo(float target, string status)
    {
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
