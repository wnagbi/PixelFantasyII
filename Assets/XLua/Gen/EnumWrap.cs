#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    
    public class TutorialTestEnumWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(Tutorial.TestEnum), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(Tutorial.TestEnum), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(Tutorial.TestEnum), L, null, 3, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "E1", Tutorial.TestEnum.E1);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "E2", Tutorial.TestEnum.E2);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(Tutorial.TestEnum), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushTutorialTestEnum(L, (Tutorial.TestEnum)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "E1"))
                {
                    translator.PushTutorialTestEnum(L, Tutorial.TestEnum.E1);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "E2"))
                {
                    translator.PushTutorialTestEnum(L, Tutorial.TestEnum.E2);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for Tutorial.TestEnum!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for Tutorial.TestEnum! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class TutorialDerivedClassTestEnumInnerWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(Tutorial.DerivedClass.TestEnumInner), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(Tutorial.DerivedClass.TestEnumInner), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(Tutorial.DerivedClass.TestEnumInner), L, null, 3, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "E3", Tutorial.DerivedClass.TestEnumInner.E3);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "E4", Tutorial.DerivedClass.TestEnumInner.E4);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(Tutorial.DerivedClass.TestEnumInner), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushTutorialDerivedClassTestEnumInner(L, (Tutorial.DerivedClass.TestEnumInner)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "E3"))
                {
                    translator.PushTutorialDerivedClassTestEnumInner(L, Tutorial.DerivedClass.TestEnumInner.E3);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "E4"))
                {
                    translator.PushTutorialDerivedClassTestEnumInner(L, Tutorial.DerivedClass.TestEnumInner.E4);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for Tutorial.DerivedClass.TestEnumInner!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for Tutorial.DerivedClass.TestEnumInner! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class PlayerStateTypeWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(PlayerStateType), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(PlayerStateType), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(PlayerStateType), L, null, 5, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Idle", PlayerStateType.Idle);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Move", PlayerStateType.Move);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Hurt", PlayerStateType.Hurt);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Die", PlayerStateType.Die);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(PlayerStateType), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushPlayerStateType(L, (PlayerStateType)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "Idle"))
                {
                    translator.PushPlayerStateType(L, PlayerStateType.Idle);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Move"))
                {
                    translator.PushPlayerStateType(L, PlayerStateType.Move);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Hurt"))
                {
                    translator.PushPlayerStateType(L, PlayerStateType.Hurt);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Die"))
                {
                    translator.PushPlayerStateType(L, PlayerStateType.Die);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for PlayerStateType!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for PlayerStateType! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class EnemyStateTypeWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(EnemyStateType), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(EnemyStateType), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(EnemyStateType), L, null, 6, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Idle", EnemyStateType.Idle);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Move", EnemyStateType.Move);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Attack", EnemyStateType.Attack);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Hurt", EnemyStateType.Hurt);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Die", EnemyStateType.Die);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(EnemyStateType), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushEnemyStateType(L, (EnemyStateType)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "Idle"))
                {
                    translator.PushEnemyStateType(L, EnemyStateType.Idle);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Move"))
                {
                    translator.PushEnemyStateType(L, EnemyStateType.Move);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Attack"))
                {
                    translator.PushEnemyStateType(L, EnemyStateType.Attack);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Hurt"))
                {
                    translator.PushEnemyStateType(L, EnemyStateType.Hurt);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Die"))
                {
                    translator.PushEnemyStateType(L, EnemyStateType.Die);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for EnemyStateType!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for EnemyStateType! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class PickUpTpyeWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(PickUpTpye), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(PickUpTpye), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(PickUpTpye), L, null, 3, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Exp", PickUpTpye.Exp);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Blood", PickUpTpye.Blood);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(PickUpTpye), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushPickUpTpye(L, (PickUpTpye)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "Exp"))
                {
                    translator.PushPickUpTpye(L, PickUpTpye.Exp);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Blood"))
                {
                    translator.PushPickUpTpye(L, PickUpTpye.Blood);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for PickUpTpye!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for PickUpTpye! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
}