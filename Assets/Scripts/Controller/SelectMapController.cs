using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

// 地图选择界面的分数显示。
// 从 save_data.json 读取累计分数，并用本地化文本刷新到 UI。
public class SelectMapController : MonoBehaviour
{
    public Text scoreText;
    public LocalizedString nameString;
    private string scoreLabel = string.Empty;

    private void Awake()
    {
        // 指定本地化表中的分数字段。
        nameString.TableEntryReference = "ScoreText";
    }

    private void OnEnable()
    {
        // 分数变化由 PlayerSaveStore 广播，地图界面只负责刷新显示。
        GameEvents.ScoreChanged += RefreshScore;
        nameString.StringChanged += OnScoreLabelChanged;
        RefreshScore(PlayerSaveStore.Current.score);
    }

    private void OnDisable()
    {
        GameEvents.ScoreChanged -= RefreshScore;
        nameString.StringChanged -= OnScoreLabelChanged;
    }

    private void RefreshScore(int score)
    {
        // 这里不再每帧读取 JSON，只有进入界面或分数变化时刷新。
        if (scoreText != null)
        {
            scoreText.text = scoreLabel + score.ToString();
        }
    }

    private void OnScoreLabelChanged(string localizedText)
    {
        scoreLabel = localizedText;
        RefreshScore(PlayerSaveStore.Current.score);
    }
}
