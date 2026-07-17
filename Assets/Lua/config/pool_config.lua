return {
    -- 这里统一声明“哪些对象池的 prefab 支持 Addressables 热更新”。
    -- poolName 必须和场景中 ObjPoolManager Inspector 里的对象池名称一致。
    -- prefabKey 必须和 Addressables 里配置的 Address 一致。
    -- 新增可热更对象池时，只需要在这里追加一项，不需要再改 ObjPoolManager C#。
    addressable_pools = {
        {
            -- 敌人对象池：刷怪系统仍然通过 ObjPoolManager.GetObj("Silm") 取对象。
            -- 这里只负责在启动时把 Silm 池的 prefab 替换成 Addressables 加载到的新 prefab。
            poolName = "Silm",
            prefabKey = "enemy/slim",
        },
        {
            -- 经验掉落物对象池。
            poolName = "Exp",
            prefabKey = "pickup/exp",
        },
        {
            -- 血瓶掉落物对象池。
            poolName = "Potion",
            prefabKey = "pickup/blood",
        },
    }
}
