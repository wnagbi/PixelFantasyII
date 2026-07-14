using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

// 场景和时间暂停控制器。
// 主要由 UI 按钮事件调用：暂停、恢复、切换场景。
public class SceneController : MonoBehaviour
{

    public void TimePause() 
    {
        // 暂停游戏内时间，Update 仍执行但物理、动画等受 timeScale 影响的逻辑会停住。
        Time.timeScale = 0f;
     
    }
    public void TimeDisPause()
    {
        // 恢复正常游戏速度。
        Time.timeScale = 1f;
    }


    public void Teleport(string name) 
    {
        // load the scene
        // 切换场景时顺手恢复 timeScale，避免从暂停菜单进新场景后仍然暂停。
        SceneManager.LoadScene(name);
        Time.timeScale = 1.0f;

    }
    public void WaitForAniFinished(string ani)
    {
        // 根据名字找到 Animator，并等待当前动画播放完成后进入 level1。
        // 注意：这里的 while 是同步循环，后续更适合改成 Coroutine。
        Animator animator = GameObject.Find(ani).GetComponent<Animator>();
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // wait for the animation to finish
        while (stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f)
            {
                Teleport("level1");
                break;
            }
        }
    }
}
