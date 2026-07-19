using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 龙卷风武器控制器。
// 龙卷风移动、数量、范围和升级规则优先由 hotfix.weapon.tornado.lua 接管。
public class TornadoController : HotfixWeaponController
{
    public float moveRange;
    public List<GameObject> tornado;
    private Vector3 newPos;
    private float startTime;
    private Vector3 originPos;
    private float distance;
    private bool isAttack;

    protected override void Start()
    {
        base.Start();
    }
    /// <summary>
    /// 根据最新数据刷新 TornadoController 的状态或显示。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 TornadoController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void Refresh()
    {
        tornado.Clear();
        for (int i = 0; i < count; i++) 
        {
            GameObject tornadoObj = InstantiateRuntimePrefab(transform.position + new Vector3(Random.Range(-moveRange, moveRange), Random.Range(-moveRange, moveRange), 0), Quaternion.identity);
            if (tornadoObj != null)
            {
                tornado.Add(tornadoObj);
            }
        }
        
    }

    /// <summary>
    /// 武器 Prefab 实例化后向实体注入所属 Controller。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 TornadoController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void OnRuntimePrefabInstantiated(GameObject instance, bool isSecondaryPrefab)
    {
        if (instance != null && instance.TryGetComponent(out Tornado tornadoComponent))
        {
            tornadoComponent.Init(this);
        }
    }

    /// <summary>
    /// 暴露给 Lua：升级或配置变化后销毁旧龙卷风并重新生成。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 TornadoController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void RebuildTornadoes()
    {
        // 暴露给 Lua：升级或配置变化后销毁旧龙卷风并重新生成。
        for (int i = 0; i < tornado.Count; i++)
        {
            if (tornado[i] != null)
            {
                Destroy(tornado[i]);
            }
        }

        Refresh();
    }

    /// <summary>
    /// 在 Lua 模块和 Addressables Prefab 准备完成后初始化具体武器表现。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 TornadoController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void OnHotfixStartReady()
    {
        RebuildTornadoes();
    }

    protected override void Update()
    {
        if (!IsHotfixStartReady)
        {
            return;
        }

        weapon.weaponLevel = level;

        // 这个武器的每帧移动逻辑主要交给 Lua 的 OnUpdate(host, deltaTime)。
        TryLuaUpdate(Time.deltaTime);
        //if (!isAttack)
        //    Attack();
        //else 
        //{            
        //    float dist = (Time.time - startTime) * speed;
        //    float factor = dist / distance;
        //    Debug.Log(factor);
        //    tornado.transform.position = Vector3.Lerp(originPos, newPos, factor);
        //    if(tornado.transform.position == newPos)
        //        isAttack = false;
        //}
            
    }
    /// <summary>
    /// 龙卷风攻击行为优先由 Lua 控制。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 TornadoController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override void Attack()
    {
        // 龙卷风攻击行为优先由 Lua 控制。
        if (TryLuaAttack())
        {
            return;
        }

        //startTime = Time.time;
        //base.Attack();
        //float newX = transform.position.x+Random.Range(-moveRange, moveRange);
        //float newY = transform.position.y+Random.Range(-moveRange, moveRange);
        //newPos = new Vector3 (newX, newY, 0);
        //originPos = tornado.transform.position;
        //distance = Vector3.Distance(originPos,new Vector3(newX,newY,0));
        //isAttack = true;
            
    }
    /// <summary>
    /// 升级优先交给 Lua，Lua 失败时使用 C# 默认升级表。
    /// </summary>
    /// <remarks>
    /// 使用注意：调用前应确保 TornadoController 的 Inspector 引用和运行时依赖已经初始化。
    /// </remarks>
    public void levelUp()
    {
        // 升级优先交给 Lua，Lua 失败时使用 C# 默认升级表。
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
                damage++;
                level++;
                break;
            case 2:
                count++;
                level++;
                break;
            case 3:
                moveRange--;
                level++;
                break;
            case 4:
                damage+=2;
                level++;
                break;
            case 5:
                moveRange -= 2;
                GetComponent<Weapon>().isLevelMax = true;
                WeaponSelectController.instance.LevelMaxRemove("Funnel");
                weapon.weaponLevel++;
                break;
        }
        for (int i = 0; i < tornado.Count; i++)
        {
            Destroy(tornado[i]);
        }
        Refresh();
    }

    /// <summary>
    /// 默认 Lua 模块路径。
    /// </summary>
    /// <remarks>
    /// 使用注意：仅供 TornadoController 及其派生类调用，重写时应保持基类约定和调用顺序。
    /// </remarks>
    protected override string GetDefaultLuaModuleName()
    {
        // 默认 Lua 模块路径。
        return "hotfix.weapon.tornado";
    }
}
