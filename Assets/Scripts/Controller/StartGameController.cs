using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 标题界面开始游戏控制器。
// Map 脚本会把选中的地图名写到 startGameTarget，开始按钮调用 Tp() 进入该场景。
public class StartGameController : MonoBehaviour
{
    public static StartGameController instance;
    public string startGameTarget;

    private void Awake()
    {
        // 地图选择按钮通过 instance 写入目标场景名。
        instance = this;
    }
    /// <summary>
    /// 没选地图时不切场景。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 StartGameController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void Tp() 
    {
        // 没选地图时不切场景。
        if (startGameTarget == null) return;
        SceneManager.LoadScene(startGameTarget);
    }
}
