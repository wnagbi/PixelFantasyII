#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;


namespace XLua.CSObjectWrap
{
    public class XLua_Gen_Initer_Register__
	{
        
        
        static void wrapInit0(LuaEnv luaenv, ObjectTranslator translator)
        {
        
            translator.DelayWrapLoader(typeof(Tutorial.BaseClass), TutorialBaseClassWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(Tutorial.TestEnum), TutorialTestEnumWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(Tutorial.DerivedClass), TutorialDerivedClassWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(Tutorial.ICalc), TutorialICalcWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(Tutorial.DerivedClassExtensions), TutorialDerivedClassExtensionsWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(Tutorial.DerivedClass.TestEnumInner), TutorialDerivedClassTestEnumInnerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.GameObject), UnityEngineGameObjectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Component), UnityEngineComponentWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Transform), UnityEngineTransformWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Vector2), UnityEngineVector2Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Vector3), UnityEngineVector3Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Quaternion), UnityEngineQuaternionWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Mathf), UnityEngineMathfWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Random), UnityEngineRandomWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Time), UnityEngineTimeWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Debug), UnityEngineDebugWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Rigidbody2D), UnityEngineRigidbody2DWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Collider2D), UnityEngineCollider2DWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Physics2D), UnityEnginePhysics2DWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.LayerMask), UnityEngineLayerMaskWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Animator), UnityEngineAnimatorWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.SpriteRenderer), UnityEngineSpriteRendererWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.Image), UnityEngineUIImageWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Events.UnityEvent), UnityEngineEventsUnityEventWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(PlayerData), PlayerDataWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(Player), PlayerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(PlayerStateType), PlayerStateTypeWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(ExpController), ExpControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(Weapon), WeaponWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(WeaponController), WeaponControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(HotfixWeaponController), HotfixWeaponControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(MissileController), MissileControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(KnifeController), KnifeControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(ScytheController), ScytheControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(SwordController), SwordControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(FunnelController), FunnelControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(TornadoController), TornadoControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(NewController), NewControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(SkillController), SkillControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(Enemy), EnemyWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(EnemyStateType), EnemyStateTypeWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(EnemySpawner), EnemySpawnerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(PickUp), PickUpWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(PickUpTpye), PickUpTpyeWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(PickUpGenerator), PickUpGeneratorWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(TaskController), TaskControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(ObjPoolManager), ObjPoolManagerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(AudioController), AudioControllerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(WeaponSelectController), WeaponSelectControllerWrap.__Register);
        
        
        
        }
        
        static void Init(LuaEnv luaenv, ObjectTranslator translator)
        {
            
            wrapInit0(luaenv, translator);
            
            
            translator.AddInterfaceBridgeCreator(typeof(Tutorial.CSCallLua.ItfD), TutorialCSCallLuaItfDBridge.__Create);
            
        }
        
	    static XLua_Gen_Initer_Register__()
        {
		    XLua.LuaEnv.AddIniter(Init);
		}
		
		
	}
	
}
namespace XLua
{
	public partial class ObjectTranslator
	{
		static XLua.CSObjectWrap.XLua_Gen_Initer_Register__ s_gen_reg_dumb_obj = new XLua.CSObjectWrap.XLua_Gen_Initer_Register__();
		static XLua.CSObjectWrap.XLua_Gen_Initer_Register__ gen_reg_dumb_obj {get{return s_gen_reg_dumb_obj;}}
	}
	
	internal partial class InternalGlobals
    {
	    
	    static InternalGlobals()
		{
		    extensionMethodMap = new Dictionary<Type, IEnumerable<MethodInfo>>()
			{
			    
			};
			
			genTryArrayGetPtr = StaticLuaCallbacks.__tryArrayGet;
            genTryArraySetPtr = StaticLuaCallbacks.__tryArraySet;
		}
	}
}
