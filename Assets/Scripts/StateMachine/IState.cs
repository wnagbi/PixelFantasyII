using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 项目通用状态机接口。
// Player、Enemy 和 LuaState 都实现它，从而可以用同一套 TransitionState 流程切换状态。
public interface IState 
{
    // 进入状态时调用一次。
    void OnEnter();   

    // 每帧 Update 调用。接口名保留项目原拼写 OnUpData。
    void OnUpData();

    // FixedUpdate 调用，适合放物理相关逻辑。
    void OnFixUpData();

    // 离开状态时调用一次。
    void OnExit();
}
