using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人死亡状态。
// 进入时执行 EnemyDestroy，默认会计数、掉落并回收到对象池。
public class EnemyDieState : IState
{
    private Enemy enemy;
    public EnemyDieState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void OnEnter()
    {
        // 执行死亡收尾。当前这里也会给 Lua 一个接管死亡规则的机会。
        enemy.EnemyDestroy();
    }


    public void OnUpData()
    {
        if (!enemy.isDie) 
        {
            // 对象池重新启用或死亡标记清除后，回到移动状态。
            enemy.TransitionState(EnemyStateType.Move);
        }
    }
    public void OnFixUpData()
    {
       
    }
    public void OnExit()
    {
        // 离开死亡状态时清除死亡标记。
        enemy.isDie = false;
    }

}
