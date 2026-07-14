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
    public class SkillControllerWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(SkillController);
			Utils.BeginObjectRegister(type, L, translator, 0, 13, 26, 26);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnSkill1", _m_OnSkill1);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnSkill2", _m_OnSkill2);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnSkill3", _m_OnSkill3);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CanUseSkill", _m_CanUseSkill);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "TriggerMagnet", _m_TriggerMagnet);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "TriggerRage", _m_TriggerRage);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "TriggerDimensionSlash", _m_TriggerDimensionSlash);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RestoreFilter", _m_RestoreFilter);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ImproveAttackDuration", _m_ImproveAttackDuration);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RageCD", _m_RageCD);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "DSCD", _m_DSCD);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "UIChange", _m_UIChange);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "MagnetCD", _m_MagnetCD);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "magenetCD", _g_get_magenetCD);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "magnet", _g_get_magnet);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "magnetImage", _g_get_magnetImage);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "isMagnetCD", _g_get_isMagnetCD);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "pickUpAll", _g_get_pickUpAll);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "magnetKeyUi", _g_get_magnetKeyUi);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "magnetControUi", _g_get_magnetControUi);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "magnetImageButton", _g_get_magnetImageButton);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "rageCD", _g_get_rageCD);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "rageDutation", _g_get_rageDutation);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "rage", _g_get_rage);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "rageImage", _g_get_rageImage);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "isRageCD", _g_get_isRageCD);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "rageKeyUi", _g_get_rageKeyUi);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "rageControUi", _g_get_rageControUi);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "rageImageButton", _g_get_rageImageButton);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "dsCD", _g_get_dsCD);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "position", _g_get_position);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ds", _g_get_ds);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "uiFilter", _g_get_uiFilter);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "particle", _g_get_particle);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "dsImage", _g_get_dsImage);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "isDSCD", _g_get_isDSCD);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "dsKeyUi", _g_get_dsKeyUi);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "dsControUi", _g_get_dsControUi);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "dsImageButton", _g_get_dsImageButton);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "magenetCD", _s_set_magenetCD);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "magnet", _s_set_magnet);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "magnetImage", _s_set_magnetImage);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "isMagnetCD", _s_set_isMagnetCD);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "pickUpAll", _s_set_pickUpAll);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "magnetKeyUi", _s_set_magnetKeyUi);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "magnetControUi", _s_set_magnetControUi);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "magnetImageButton", _s_set_magnetImageButton);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "rageCD", _s_set_rageCD);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "rageDutation", _s_set_rageDutation);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "rage", _s_set_rage);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "rageImage", _s_set_rageImage);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "isRageCD", _s_set_isRageCD);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "rageKeyUi", _s_set_rageKeyUi);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "rageControUi", _s_set_rageControUi);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "rageImageButton", _s_set_rageImageButton);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "dsCD", _s_set_dsCD);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "position", _s_set_position);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ds", _s_set_ds);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "uiFilter", _s_set_uiFilter);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "particle", _s_set_particle);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "dsImage", _s_set_dsImage);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "isDSCD", _s_set_isDSCD);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "dsKeyUi", _s_set_dsKeyUi);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "dsControUi", _s_set_dsControUi);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "dsImageButton", _s_set_dsImageButton);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 1, 1);
			
			
            
			Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "Instance", _g_get_Instance);
            
			Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "Instance", _s_set_Instance);
            
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					var gen_ret = new SkillController();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to SkillController constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnSkill1(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.OnSkill1(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnSkill2(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.OnSkill2(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnSkill3(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.OnSkill3(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CanUseSkill(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _skillId = LuaAPI.xlua_tointeger(L, 2);
                    
                        var gen_ret = gen_to_be_invoked.CanUseSkill( _skillId );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_TriggerMagnet(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _cooldown = (float)LuaAPI.lua_tonumber(L, 2);
                    
                    gen_to_be_invoked.TriggerMagnet( _cooldown );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_TriggerRage(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _extraDamage = (float)LuaAPI.lua_tonumber(L, 2);
                    float _duration = (float)LuaAPI.lua_tonumber(L, 3);
                    float _cooldown = (float)LuaAPI.lua_tonumber(L, 4);
                    
                    gen_to_be_invoked.TriggerRage( _extraDamage, _duration, _cooldown );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_TriggerDimensionSlash(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _cooldown = (float)LuaAPI.lua_tonumber(L, 2);
                    
                    gen_to_be_invoked.TriggerDimensionSlash( _cooldown );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RestoreFilter(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.RestoreFilter(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ImproveAttackDuration(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _duration = (float)LuaAPI.lua_tonumber(L, 2);
                    
                    gen_to_be_invoked.ImproveAttackDuration( _duration );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RageCD(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _duration = (float)LuaAPI.lua_tonumber(L, 2);
                    
                    gen_to_be_invoked.RageCD( _duration );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DSCD(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _duration = (float)LuaAPI.lua_tonumber(L, 2);
                    
                    gen_to_be_invoked.DSCD( _duration );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UIChange(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.UIChange(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_MagnetCD(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _duration = (float)LuaAPI.lua_tonumber(L, 2);
                    
                    gen_to_be_invoked.MagnetCD( _duration );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Instance(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, SkillController.Instance);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_magenetCD(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.magenetCD);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_magnet(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.magnet);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_magnetImage(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.magnetImage);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_isMagnetCD(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.isMagnetCD);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_pickUpAll(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.pickUpAll);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_magnetKeyUi(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.magnetKeyUi);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_magnetControUi(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.magnetControUi);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_magnetImageButton(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.magnetImageButton);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_rageCD(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.rageCD);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_rageDutation(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.rageDutation);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_rage(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.rage);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_rageImage(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.rageImage);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_isRageCD(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.isRageCD);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_rageKeyUi(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.rageKeyUi);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_rageControUi(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.rageControUi);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_rageImageButton(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.rageImageButton);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_dsCD(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.dsCD);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_position(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.position);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ds(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.ds);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_uiFilter(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.uiFilter);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_particle(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.particle);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_dsImage(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.dsImage);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_isDSCD(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.isDSCD);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_dsKeyUi(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.dsKeyUi);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_dsControUi(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.dsControUi);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_dsImageButton(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.dsImageButton);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Instance(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    SkillController.Instance = (SkillController)translator.GetObject(L, 1, typeof(SkillController));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_magenetCD(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.magenetCD = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_magnet(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.magnet = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_magnetImage(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.magnetImage = (UnityEngine.UI.Image)translator.GetObject(L, 2, typeof(UnityEngine.UI.Image));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_isMagnetCD(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.isMagnetCD = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_pickUpAll(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.pickUpAll = (UnityEngine.Events.UnityEvent)translator.GetObject(L, 2, typeof(UnityEngine.Events.UnityEvent));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_magnetKeyUi(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.magnetKeyUi = (UnityEngine.Sprite)translator.GetObject(L, 2, typeof(UnityEngine.Sprite));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_magnetControUi(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.magnetControUi = (UnityEngine.Sprite)translator.GetObject(L, 2, typeof(UnityEngine.Sprite));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_magnetImageButton(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.magnetImageButton = (UnityEngine.UI.Image)translator.GetObject(L, 2, typeof(UnityEngine.UI.Image));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_rageCD(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.rageCD = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_rageDutation(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.rageDutation = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_rage(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.rage = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_rageImage(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.rageImage = (UnityEngine.UI.Image)translator.GetObject(L, 2, typeof(UnityEngine.UI.Image));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_isRageCD(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.isRageCD = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_rageKeyUi(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.rageKeyUi = (UnityEngine.Sprite)translator.GetObject(L, 2, typeof(UnityEngine.Sprite));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_rageControUi(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.rageControUi = (UnityEngine.Sprite)translator.GetObject(L, 2, typeof(UnityEngine.Sprite));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_rageImageButton(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.rageImageButton = (UnityEngine.UI.Image)translator.GetObject(L, 2, typeof(UnityEngine.UI.Image));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_dsCD(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.dsCD = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_position(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.position = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ds(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ds = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_uiFilter(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.uiFilter = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_particle(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.particle = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_dsImage(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.dsImage = (UnityEngine.UI.Image)translator.GetObject(L, 2, typeof(UnityEngine.UI.Image));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_isDSCD(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.isDSCD = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_dsKeyUi(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.dsKeyUi = (UnityEngine.Sprite)translator.GetObject(L, 2, typeof(UnityEngine.Sprite));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_dsControUi(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.dsControUi = (UnityEngine.Sprite)translator.GetObject(L, 2, typeof(UnityEngine.Sprite));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_dsImageButton(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SkillController gen_to_be_invoked = (SkillController)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.dsImageButton = (UnityEngine.UI.Image)translator.GetObject(L, 2, typeof(UnityEngine.UI.Image));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
