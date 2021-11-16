using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using ChaCustom;

using BepInEx.Logging;
using HarmonyLib;

namespace SMAP
{
	public partial class SMAP
	{
		private static List<string> AccessoryParentKey = new List<string>();

		private static string FindReverseBone(string _name)
		{
			if (_name == "a_n_hair_pin")
				return "a_n_hair_pin_R";
			if (_name == "a_n_hair_pin_R")
				return "a_n_hair_pin";

			string _lookup = "";

			if (_name.EndsWith("_R", StringComparison.Ordinal))
				_lookup = _name.Remove(_name.Length - 1) + "L";
			if (_name.EndsWith("_L", StringComparison.Ordinal))
				_lookup = _name.Remove(_name.Length - 1) + "R";

			if (_name.Remove(_name.Length - 3).EndsWith("_R", StringComparison.Ordinal))
				_lookup = _name.Remove(_name.Length - 4) + "L" + _name.Substring(_name.Length - 3);
			if (_name.Remove(_name.Length - 3).EndsWith("_L", StringComparison.Ordinal))
				_lookup = _name.Remove(_name.Length - 4) + "R" + _name.Substring(_name.Length - 3);

			if (_lookup.IsNullOrEmpty())
				return _name;

			return AccessoryParentKey.Where(x => x == _lookup).FirstOrDefault() ?? _name;
		}

		// this part is basically took from KK_MoreAccessoryParents
		internal static class Hooks
		{
			private static readonly Type EnumAccesoryParentKeyType = typeof(ChaAccessoryDefine.AccessoryParentKey);
			private static readonly Type EnumRefObjKeyType = typeof(ChaReference.RefObjKey);
			private static readonly string[] SelectParentHookCache;
			internal static readonly int AccessoryParentKeyOriginalCount;
			internal static readonly int RefObjKeyOriginalCount;

			private static readonly List<string> cf_j_bones = new List<string>() { "cf_j_ana", "cf_j_arm00_L", "cf_j_arm00_R", "cf_j_backsk_C_01", "cf_j_backsk_C_02", "cf_j_backsk_L_01", "cf_j_backsk_L_02", "cf_j_backsk_R_01", "cf_j_backsk_R_02", "cf_j_bnip02_L", "cf_j_bnip02_R", "cf_j_bnip02root_L", "cf_j_bnip02root_R", "cf_j_bust01_L", "cf_j_bust01_L_01", "cf_j_bust01_R", "cf_j_bust01_R_01", "cf_j_bust02_L", "cf_j_bust02_L_01", "cf_j_bust02_R", "cf_j_bust02_R_01", "cf_j_bust03_L", "cf_j_bust03_L_01", "cf_j_bust03_R", "cf_j_bust03_R_01", "cf_j_foot_L", "cf_j_foot_R", "cf_j_forearm01_L", "cf_j_forearm01_R", "cf_j_hand_L", "cf_j_hand_R", "cf_j_head", "cf_j_hips", "cf_j_index01_L", "cf_j_index01_R", "cf_j_index02_L", "cf_j_index02_R", "cf_j_index03_L", "cf_j_index03_R", "cf_j_index04_L", "cf_j_index04_R", "cf_j_kokan", "cf_j_leg01_L", "cf_j_leg01_R", "cf_j_leg03_L", "cf_j_leg03_R", "cf_j_little01_L", "cf_j_little01_R", "cf_j_little02_L", "cf_j_little02_R", "cf_j_little03_L", "cf_j_little03_R", "cf_j_little04_L", "cf_j_little04_R", "cf_j_middle01_L", "cf_j_middle01_R", "cf_j_middle02_L", "cf_j_middle02_R", "cf_j_middle03_L", "cf_j_middle03_R", "cf_j_middle04_L", "cf_j_middle04_R", "cf_j_neck", "cf_j_ring01_L", "cf_j_ring01_R", "cf_j_ring02_L", "cf_j_ring02_R", "cf_j_ring03_L", "cf_j_ring03_R", "cf_j_ring04_L", "cf_j_ring04_R", "cf_j_root", "cf_j_root_hit", "cf_j_shoulder_L", "cf_j_shoulder_R", "cf_j_siri_L", "cf_j_siri_L_01", "cf_j_siri_R", "cf_j_siri_R_01", "cf_j_sk_00_00", "cf_j_sk_00_01", "cf_j_sk_00_02", "cf_j_sk_00_03", "cf_j_sk_00_04", "cf_j_sk_00_05", "cf_j_sk_01_00", "cf_j_sk_01_01", "cf_j_sk_01_02", "cf_j_sk_01_03", "cf_j_sk_01_04", "cf_j_sk_01_05", "cf_j_sk_02_00", "cf_j_sk_02_01", "cf_j_sk_02_02", "cf_j_sk_02_03", "cf_j_sk_02_04", "cf_j_sk_02_05", "cf_j_sk_03_00", "cf_j_sk_03_01", "cf_j_sk_03_02", "cf_j_sk_03_03", "cf_j_sk_03_04", "cf_j_sk_03_05", "cf_j_sk_04_00", "cf_j_sk_04_01", "cf_j_sk_04_02", "cf_j_sk_04_03", "cf_j_sk_04_04", "cf_j_sk_04_05", "cf_j_sk_05_00", "cf_j_sk_05_01", "cf_j_sk_05_02", "cf_j_sk_05_03", "cf_j_sk_05_04", "cf_j_sk_05_05", "cf_j_sk_06_00", "cf_j_sk_06_01", "cf_j_sk_06_02", "cf_j_sk_06_03", "cf_j_sk_06_04", "cf_j_sk_06_05", "cf_j_sk_07_00", "cf_j_sk_07_01", "cf_j_sk_07_02", "cf_j_sk_07_03", "cf_j_sk_07_04", "cf_j_sk_07_05", "cf_j_spine01", "cf_j_spine02", "cf_j_spine03", "cf_j_spinesk_00", "cf_j_spinesk_01", "cf_j_spinesk_02", "cf_j_spinesk_03", "cf_j_spinesk_04", "cf_j_spinesk_05", "cf_j_tang_01", "cf_j_tang_02", "cf_j_tang_03", "cf_j_tang_04", "cf_j_tang_05", "cf_j_tang_L_03", "cf_j_tang_L_04", "cf_j_tang_L_05", "cf_j_tang_R_03", "cf_j_tang_R_04", "cf_j_tang_R_05", "cf_j_tang2_L_05", "cf_j_tang2_R_05", "cf_j_thigh00_L", "cf_j_thigh00_R", "cf_j_thumb01_L", "cf_j_thumb01_R", "cf_j_thumb02_L", "cf_j_thumb02_R", "cf_j_thumb03_L", "cf_j_thumb03_R", "cf_j_thumb04_L", "cf_j_thumb04_R", "cf_j_toes_L", "cf_j_toes_R", "cf_j_waist01", "cf_j_waist02" };
			private static readonly List<string> cf_s_bones = new List<string>() { "cf_s_ana", "cf_s_arm01_L", "cf_s_arm01_R", "cf_s_arm02_L", "cf_s_arm02_L_hit", "cf_s_arm02_R", "cf_s_arm02_R_hit", "cf_s_arm03_L", "cf_s_arm03_R", "cf_s_bnip01_L", "cf_s_bnip01_R", "cf_s_bnip015_L", "cf_s_bnip015_R", "cf_s_bnip02_L", "cf_s_bnip02_R", "cf_s_bnip025_L", "cf_s_bnip025_R", "cf_s_bnipacc_L", "cf_s_bnipacc_R", "cf_s_bust00_L", "cf_s_bust00_R", "cf_s_bust01_L", "cf_s_bust01_R", "cf_s_bust02_L", "cf_s_bust02_R", "cf_s_bust03_L", "cf_s_bust03_R", "cf_s_elbo_L", "cf_s_elbo_R", "cf_s_elboback_L", "cf_s_elboback_R", "cf_s_forearm01_L", "cf_s_forearm01_R", "cf_s_forearm02_L", "cf_s_forearm02_L_hit", "cf_s_forearm02_R", "cf_s_forearm02_R_hit", "cf_s_hand_L", "cf_s_hand_L_hit", "cf_s_hand_R", "cf_s_hand_R_hit", "cf_s_head", "cf_s_kneeB_L", "cf_s_kneeB_R", "cf_s_leg_L", "cf_s_leg_R", "cf_s_leg01_L", "cf_s_leg01_R", "cf_s_leg02_L", "cf_s_leg02_R", "cf_s_leg03_L", "cf_s_leg03_R", "cf_s_neck", "cf_s_shoulder02_L", "cf_s_shoulder02_R", "cf_s_siri_L", "cf_s_siri_R", "cf_s_spine01", "cf_s_spine02", "cf_s_spine03", "cf_s_thigh01_L", "cf_s_thigh01_R", "cf_s_thigh02_L", "cf_s_thigh02_R", "cf_s_thigh03_L", "cf_s_thigh03_R", "cf_s_waist01", "cf_s_waist02", "cf_s_wrist_L", "cf_s_wrist_R" };

			static Hooks()
			{
				// Do this before hooking!
				SelectParentHookCache = (from key in Enum.GetNames(EnumAccesoryParentKeyType)
										 where key != "none"
										 select key).ToArray();

				AccessoryParentKeyOriginalCount = Enum.GetValues(EnumAccesoryParentKeyType).Length;
				RefObjKeyOriginalCount = Enum.GetValues(EnumRefObjKeyType).Length;

				AccessoryParentKey.AddRange(cf_j_bones);
				AccessoryParentKey.AddRange(cf_s_bones);
			}

			internal static void Initialize()
			{
				Harmony.CreateAndPatchAll(typeof(Hooks));

				UpdateChaAccessoryDefine();
			}

			private static void UpdateChaAccessoryDefine()
			{
				// todo ChaAccessoryDefine.GetReverseParent
				int length = Enum.GetValues(EnumAccesoryParentKeyType).Length;
				string[] accNames = ChaAccessoryDefine.AccessoryParentName.Concat(AccessoryParentKey).ToArray();
				int num = accNames.Length;
				if (length == num)
				{
					for (int j = 0; j < length; j++)
						ChaAccessoryDefine.dictAccessoryParent[j] = accNames[j];
				}
				else
				{
					_logger.Log(LogLevel.Error, "Invalid ChaAccessoryDefine.AccessoryParentName or Enum.GetValues(typeof(ChaAccessoryDefine.AccessoryParentKey))");
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(ChaAccessoryDefine), nameof(ChaAccessoryDefine.GetReverseParent), new[] { typeof(string) })]
			private static void ChaAccessoryDefine_GetReverseParent_Postfix(string key, ref string __result)
			{
				if (__result == string.Empty)
					__result = FindReverseBone(key);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(ChaAccessoryDefine), nameof(ChaAccessoryDefine.GetReverseParent), new[] { typeof(ChaAccessoryDefine.AccessoryParentKey) })]
			private static void ChaAccessoryDefine_GetReverseParent_Postfix(ChaAccessoryDefine.AccessoryParentKey key, ref ChaAccessoryDefine.AccessoryParentKey __result)
			{
				if (__result == ChaAccessoryDefine.AccessoryParentKey.none)
				{
					try
					{
						__result = (ChaAccessoryDefine.AccessoryParentKey) Enum.Parse(EnumAccesoryParentKeyType, FindReverseBone(key.ToString()));
					}
					catch (Exception e)
					{
						_logger.Log(LogLevel.Error, e);
					}
				}
			}

			/// <summary>
			/// Used to add new items to game enums
			/// </summary>
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Enum), nameof(Enum.GetValues), new[] { typeof(Type) })]
			private static void Enum_GetValues_Postfix(Type enumType, ref Array __result)
			{
				if (enumType == EnumAccesoryParentKeyType)
				{
					ArrayList stock = new ArrayList(__result);
					stock.AddRange(
						Enumerable.Range(0, AccessoryParentKey.Count)
							.Select(x => Enum.ToObject(enumType, AccessoryParentKeyOriginalCount + x))
							.ToArray());
					__result = stock.ToArray();
				}
				else if (enumType == EnumRefObjKeyType)
				{
					ArrayList stock = new ArrayList(__result);
					stock.AddRange(
						Enumerable.Range(0, AccessoryParentKey.Count)
							.Select(x => Enum.ToObject(enumType, RefObjKeyOriginalCount + x))
							.ToArray());
					__result = stock.ToArray();
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Enum), nameof(Enum.GetNames), new[] { typeof(Type) })]
			private static void Enum_GetNames_Postfix(Type enumType, ref string[] __result)
			{
				if (enumType == EnumAccesoryParentKeyType || enumType == EnumRefObjKeyType)
				{
					List<string> stock = new List<string>(__result);
					stock.AddRange(AccessoryParentKey);
					__result = stock.ToArray();
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Enum), nameof(Enum.GetName), new[] { typeof(Type), typeof(object) })]
			private static void Enum_GetName_Postfix(Type enumType, object value, ref string __result)
			{
				if (__result != null) return;

				if (enumType == EnumAccesoryParentKeyType)
				{
					int index = Convert.ToInt32(value) - AccessoryParentKeyOriginalCount;
					if (AccessoryParentKey.Count > index && index >= 0)
						__result = AccessoryParentKey[index];
				}
				else if (enumType == EnumRefObjKeyType)
				{
					int index = Convert.ToInt32(value) - RefObjKeyOriginalCount;
					if (AccessoryParentKey.Count > index && index >= 0)
						__result = AccessoryParentKey[index];
				}
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(Enum), nameof(Enum.Parse), new[] { typeof(Type), typeof(string), typeof(bool) })]
			private static bool Enum_Parse_Prefix(Type enumType, string value, ref object __result)
			{
				if (enumType == EnumAccesoryParentKeyType)
				{
					int index = AccessoryParentKey.IndexOf(value);
					if (index >= 0)
					{
						__result = Enum.ToObject(enumType, AccessoryParentKeyOriginalCount + index);
						return false;
					}
				}
				else if (enumType == EnumRefObjKeyType)
				{
					int index = AccessoryParentKey.IndexOf(value);
					if (index >= 0)
					{
						__result = Enum.ToObject(enumType, RefObjKeyOriginalCount + index);
						return false;
					}
				}
				return true;
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(ChaReference), nameof(ChaReference.CreateReferenceInfo), new[] { typeof(ulong), typeof(GameObject) })]
			private static void ChaReference_CreateReferenceInfo_Postfix(ChaReference __instance, ulong flags, GameObject objRef)
			{
				if (null == objRef || (int)(flags - 1UL) != 0) return;

				CreateReferenceImpl(__instance, objRef);
			}

			private static void CreateReferenceImpl(ChaReference __instance, GameObject objRef)
			{
				var findAssist = new FindAssist();
				findAssist.Initialize(objRef.transform);

				var dict = (Dictionary<ChaReference.RefObjKey, GameObject>)
					AccessTools.Field(typeof(ChaReference), "dictRefObj").GetValue(__instance);

				for (var i = 0; i < AccessoryParentKey.Count; i++)
				{
					dict[(ChaReference.RefObjKey)(RefObjKeyOriginalCount + i)] =
						findAssist.GetObjectFromName(AccessoryParentKey[i]);
				}
			}
			/*
#if KKS
			[HarmonyPostfix]
			[HarmonyPatch(typeof(ChaReference), nameof(ChaReference.CreateReferenceInfo), typeof(ulong), typeof(ChaLoad.ChaPreparationBodyBone.BoneInfo[]))]
			public static void CreateReferenceInfoHook(ChaReference __instance, ulong flags, ChaLoad.ChaPreparationBodyBone.BoneInfo[] boneInfos)
			{
				if (CustomBase.Instance.chaCtrl == null) return;
				if (null == boneInfos || (int)(flags - 1UL) != 0) return;

				var dict = __instance.dictRefObj;

				for (var i = 0; i < AccessoryParentKey.Count; i++)
				{
					dict[(ChaReference.RefObjKey)(RefObjKeyOriginalCount + i)] = boneInfos.First(x => x.name == AccessoryParentKey[i]).gameObject;
				}
			}
#endif
			*/
			[HarmonyPostfix]
			[HarmonyPatch(typeof(ChaReference), nameof(ChaReference.ReleaseRefObject), new[] { typeof(ulong) })]
			private static void ChaReference_ReleaseRefObject_Postfix(ChaReference __instance, ulong flags)
			{
				if ((int) (flags - 1UL) != 0)
					return;

				for (int i = 0; i < AccessoryParentKey.Count; i++)
					__instance.dictRefObj.Remove((ChaReference.RefObjKey) (RefObjKeyOriginalCount + i));
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(CustomAcsParentWindow), nameof(CustomAcsParentWindow.SelectParent), new[] { typeof(string) })]
			private static bool CustomAcsParentWindow_SelectParent_Prefix(CustomAcsParentWindow __instance, string parentKey, ref int __result)
			{
				if (_tglSMAP != null && !parentKey.StartsWith("a_n_"))
					_tglSMAP.isOn = true;

				if (TrySetSelectedBone(parentKey, ref __result))
					return false;

				// Fall back to stock logic
				int num = Array.IndexOf(SelectParentHookCache, parentKey);
				if (num != -1)
					__instance.tglParent[num].isOn = true;

				__result = num;
				return false;
			}

			private static bool TrySetSelectedBone(string parentKey, ref int resultEnumId)
			{
				int myIndex = AccessoryParentKey.IndexOf(parentKey);
				if (myIndex >= 0)
				{
					resultEnumId = AccessoryParentKeyOriginalCount - 1 + myIndex; // -1 skip none element
					return true;
				}

				return false;
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAccessoryParent), new[] { typeof(int), typeof(string) })]
			private static bool ChaControl_ChangeAccessoryParent_Prefix(ChaControl __instance, int slotNo, string parentStr, ref bool __result)
			{
				if (slotNo >= __instance.objAccessory.Length)
				{
					__result = false;
					return false;
				}

				GameObject gameObject = __instance.objAccessory[slotNo];
				if (gameObject == null)
				{
					__result = false;
					return false;
				}
				if (parentStr == "none")
				{
					gameObject.transform.SetParent(null, worldPositionStays: false);
					__result = true;
					return false;
				}
				ListInfoBase listInfoBase = __instance.infoAccessory[slotNo];
				if (listInfoBase != null)
				{
					listInfoBase = gameObject.GetComponent<ListInfoComponent>().data;
				}
				if (listInfoBase.GetInfo(ChaListDefine.KeyType.Parent) == "0")
				{
					__result = false;
					return false;
				}
				try
				{
					/*
					ChaReference.RefObjKey key = (ChaReference.RefObjKey) Enum.Parse(typeof(ChaReference.RefObjKey), parentStr);
					GameObject referenceInfo = __instance.GetReferenceInfo(key);
					*/
					GameObject referenceInfo = __instance.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == parentStr)?.gameObject;
					if (referenceInfo == null)
					{
						__result = false;
						return false;
					}
					gameObject.transform.SetParent(referenceInfo.transform, worldPositionStays: false);
					__instance.nowCoordinate.accessory.parts[slotNo].parentKey = parentStr;
					__instance.nowCoordinate.accessory.parts[slotNo].partsOfHead = ChaAccessoryDefine.CheckPartsOfHead(parentStr);
				}
				catch (ArgumentException)
				{
					__result = false;
					return false;
				}
				__result = true;
				return false;
			}
		}
	}
}
