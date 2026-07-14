using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 导弹武器控制器。
// 运行时攻击和升级优先交给 hotfix.weapon.missile.lua，Lua 失败时保留下面的 C# 回退逻辑。
public class MissileController : HotfixWeaponController
{
    
    public float missileAttackRangeOut;
    public float missileAttackRangeIn;

    protected override string GetDefaultLuaModuleName()
    {
        // 如果 Inspector 没填 luaModuleName，默认加载这个 Lua 文件。
        return "hotfix.weapon.missile";
    }

    protected override void Attack()
    {
        // 导弹发射优先走 Lua 的 OnAttack(host)。
        if (TryLuaAttack())
        {
            return;
        }

        base.Attack();
        // for (int i = 0; i < count; i++) 
        // {
        //     float radius = Random.Range(missileAttackRangeIn, missileAttackRangeOut);
        //     float angle = Random.Range(0f, 2f * Mathf.PI);
        //     float x = radius * Mathf.Cos(angle);
        //     float y = radius * Mathf.Sin(angle);
        //     Vector2 attackPosition = new Vector2(x, y);            
        //     GameObject missle = Instantiate(prefab, (Vector2)transform.position+ attackPosition, Quaternion.identity);            

        // }
        

    }
    

    public void levelUp()
    {
        // 升级优先走 Lua 的 OnLevelUp(host)，方便热更伤害、CD、发射数量和满级规则。
        if (TryLuaLevelUp())
        {
            return;
        }

        switch (level)
        {
            case 0:     
                level++;
                break;
            case 1:
                cooldownDuration -= 1;
                
                level++;
                break;
            case 2:
                damage += 10;
                level++;
                break;
            case 3:
                
                level++;
                break;
            case 4:
                timer /= 2;
                level++;
                break;
            case 5:
                damage *= 2;
                GetComponent<Weapon>().isLevelMax = true;
                WeaponSelectController.instance.LevelMaxRemove("Missle");
                weapon.weaponLevel++;
                break;
        }

    }
    private void MissileDestroy(GameObject obj)
    {
        Destroy(obj);
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(transform.position, missileAttackRangeOut);
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(transform.position, missileAttackRangeIn);
    //}
}
