using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using BlackveilDpsMeter;
using Fusion;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using RR;
using RR.Game;
using RR.Game.Input;
using RR.Game.Items;
using RR.Game.Pickups;
using RR.Game.Stats;
using RR.UI.Components.Pickup;
using RR.UI.Controls;
using RR.UI.Pages;
using RR.UI.UISystem;
using RaidersOfBlackveilMod;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("QoL Mod")]
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyDescription("Quality of Life Mod for Raiders of Blackveil")]
[assembly: AssemblyFileVersion("1.0.1.0")]
[assembly: AssemblyInformationalVersion("1.0.1+824080e5dfc812eaa92b40cd4be24eb619dba5f9")]
[assembly: AssemblyProduct("QoL Mod")]
[assembly: AssemblyTitle("QoL Mod")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.1.0")]
[module: UnverifiableCode]
[module: RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
public class ModSettingsPage : SettingsBasePage
{
	public enum RarityThreshold
	{
		Uncommon = 1,
		Rare,
		Epic,
		Legendary,
		Mythic
	}

	public override bool AcceptDefaultCall => true;

	public bool ChangesPending => false;

	public ModSettingsPage(PageArgs args)
		: base("SettingsPage", args)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}

	public void Initialize(VisualElement root)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected Obj, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected Obj, but got Unknown
		Label val = new Label("Mod Configurations");
		((VisualElement)val).style.fontSize = StyleLength.op_Implicit(24f);
		((VisualElement)val).style.color = StyleColor.op_Implicit(Color.white);
		root.Add((VisualElement)(object)val);
		ScrollView val2 = new ScrollView();
		root.Add((VisualElement)(object)val2);
	}

	public override void DefaultCall()
	{
		Debug.Log((object)"Resetting all Mod Settings to Default!");
	}

	public override void ApplyChanges(Action onFinished)
	{
		onFinished?.Invoke();
	}
}
[HarmonyPatch(/*Could not decode attribute arguments.*/)]
public static class SettingsPage_Constructor_Patch
{
	private const float SettingsControlWidth = 500f;

	private const float SettingsLabelWidth = 350f;

	public static UIDynamicPageLayer<ModSettingsPage> ModSettingsPageLayer;

	private static void ApplyCommonControlStyle(VisualElement element)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		element.style.marginLeft = StyleLength.op_Implicit(6f);
		element.style.marginTop = StyleLength.op_Implicit(6f);
		element.style.width = StyleLength.op_Implicit(500f);
	}

	private static void ApplyLabelStyle(VisualElement control, bool isToggle = false)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		Label val = UQueryExtensions.Q<Label>(control, (string)null, (string)null);
		if (val != null)
		{
			((VisualElement)val).style.fontSize = StyleLength.op_Implicit(18f);
			((VisualElement)val).style.color = StyleColor.op_Implicit(Color.white);
			((VisualElement)val).style.unityFontStyleAndWeight = StyleEnum<FontStyle>.op_Implicit((FontStyle)0);
			((VisualElement)val).style.marginLeft = StyleLength.op_Implicit(0f);
			((VisualElement)val).style.paddingLeft = StyleLength.op_Implicit(0f);
			if (isToggle)
			{
				((VisualElement)val).style.minWidth = StyleLength.op_Implicit(350f);
				((VisualElement)val).style.width = StyleLength.op_Implicit(350f);
			}
			else
			{
				((VisualElement)val).style.minWidth = StyleLength.op_Implicit(350f);
				((VisualElement)val).style.width = StyleLength.op_Implicit(350f);
			}
		}
	}

	private static void ApplyDropdownStyle(DropdownField dropdown)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		ApplyCommonControlStyle((VisualElement)(object)dropdown);
		((VisualElement)dropdown).style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)2);
		((VisualElement)dropdown).style.justifyContent = StyleEnum<Justify>.op_Implicit((Justify)3);
		((VisualElement)dropdown).style.alignItems = StyleEnum<Align>.op_Implicit((Align)2);
		((VisualElement)dropdown).style.height = StyleLength.op_Implicit(32f);
		ApplyLabelStyle((VisualElement)(object)dropdown);
		TextField val = UQueryExtensions.Q<TextField>((VisualElement)(object)dropdown, (string)null, (string)null);
		if (val != null)
		{
			((VisualElement)val).style.minWidth = StyleLength.op_Implicit(120f);
			((VisualElement)val).style.flexGrow = StyleFloat.op_Implicit(1f);
			((VisualElement)val).style.backgroundColor = StyleColor.op_Implicit(new Color(0.8f, 0.8f, 0.8f, 0.85f));
			((VisualElement)val).style.color = StyleColor.op_Implicit(new Color(0.12f, 0.12f, 0.12f, 1f));
		}
	}

	private static void ApplyToggleStyle(Toggle toggle)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		ApplyCommonControlStyle((VisualElement)(object)toggle);
		((VisualElement)toggle).style.flexDirection = StyleEnum<FlexDirection>.op_Implicit((FlexDirection)2);
		((VisualElement)toggle).style.justifyContent = StyleEnum<Justify>.op_Implicit((Justify)3);
		((VisualElement)toggle).style.alignItems = StyleEnum<Align>.op_Implicit((Align)2);
		((VisualElement)toggle).style.height = StyleLength.op_Implicit(32f);
		ApplyLabelStyle((VisualElement)(object)toggle, isToggle: true);
		VisualElement val = UQueryExtensions.Q<VisualElement>((VisualElement)(object)toggle, "unity-checkmark", (string)null);
		if (val != null)
		{
			val.style.marginLeft = StyleLength.op_Implicit(8f);
			val.style.marginRight = StyleLength.op_Implicit(0f);
		}
	}

	[HarmonyPostfix]
	public static void Postfix(SettingsPage __instance)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected Obj, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected Obj, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected Obj, but got Unknown
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected Obj, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Expected Obj, but got Unknown
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Expected Obj, but got Unknown
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Expected Obj, but got Unknown
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Expected Obj, but got Unknown
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Expected Obj, but got Unknown
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Expected Obj, but got Unknown
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Expected Obj, but got Unknown
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Expected Obj, but got Unknown
		//IL_0580: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Expected Obj, but got Unknown
		//IL_062d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Expected Obj, but got Unknown
		//IL_06a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Expected Obj, but got Unknown
		try
		{
			VisualElement val = UQueryExtensions.Q<VisualElement>((VisualElement)(object)((PageBase)__instance).RootElement, "CategoryButtons", (string)null);
			if (val == null)
			{
				return;
			}
			VisualElement val2 = UQueryExtensions.Q<VisualElement>(val, "CatGeneral", (string)null);
			if (val2 == null)
			{
				return;
			}
			VisualElement val3 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)((PageBase)__instance).RootElement, "ScrollContent", (string)null);
			if (val3 == null)
			{
				ScrollView val4 = UQueryExtensions.Query<ScrollView>((VisualElement)(object)((PageBase)__instance).RootElement, (string)null, (string)null).First();
				val3 = (VisualElement)(((object)val4) ?? ((object)((PageBase)__instance).RootElement));
			}
			VisualElement val5 = new VisualElement();
			val5.name = "ModsSection";
			val5.style.marginTop = StyleLength.op_Implicit(25f);
			Label val6 = new Label("DPS Meter Mod Settings");
			((VisualElement)val6).style.fontSize = StyleLength.op_Implicit(24f);
			((VisualElement)val6).style.color = StyleColor.op_Implicit(new Color(0.702f, 0.639f, 0.471f, 1f));
			((VisualElement)val6).style.unityFontStyleAndWeight = StyleEnum<FontStyle>.op_Implicit((FontStyle)1);
			((VisualElement)val6).style.marginLeft = StyleLength.op_Implicit(6f);
			((VisualElement)val6).style.marginBottom = StyleLength.op_Implicit(8f);
			val5.Add((VisualElement)(object)val6);
			ScrollView val7 = new ScrollView();
			((VisualElement)val7).name = "ModsContent";
			((VisualElement)val7).style.flexGrow = StyleFloat.op_Implicit(1f);
			((VisualElement)val7).style.marginTop = StyleLength.op_Implicit(6f);
			Toggle val8 = new Toggle("DPS Meter");
			((BaseField<bool>)(object)val8).value = Plugin.ShowDPSMeter;
			ApplyToggleStyle(val8);
			INotifyValueChangedExtensions.RegisterValueChangedCallback<bool>((INotifyValueChanged<bool>)(object)val8, (EventCallback<ChangeEvent<bool>>)((ChangeEvent<bool> evt) =>
			{
				Plugin.ShowDPSMeter = evt.newValue;
				if ((Object)(object)PersistentUI._targetCanvas != (Object)null)
				{
					((Behaviour)PersistentUI._targetCanvas).enabled = evt.newValue;
				}
			}));
			((VisualElement)val7).Add((VisualElement)(object)val8);
			Toggle val9 = new Toggle("Combat Info");
			((BaseField<bool>)(object)val9).value = Plugin.ShowCombatInfo;
			ApplyToggleStyle(val9);
			INotifyValueChangedExtensions.RegisterValueChangedCallback<bool>((INotifyValueChanged<bool>)(object)val9, (EventCallback<ChangeEvent<bool>>)((ChangeEvent<bool> evt) =>
			{
				Plugin.ShowCombatInfo = evt.newValue;
				if ((Object)(object)CombatInfoModule.Instance != (Object)null)
				{
					CombatInfoModule.Instance.UpdateVisibilityState();
				}
				Debug.Log((object)$"[ModSettings] Combat Info Toggle changed to: {evt.newValue}");
			}));
			((VisualElement)val7).Add((VisualElement)(object)val9);
			List<string> list = new List<string> { "", "Shock", "Burn", "Chill", "Poison", "Bleed", "Fury", "Curse", "Summon", "Bless" };
			DropdownField val10 = new DropdownField("Select element for Combat info tracking", list, Plugin.SelectedElement ?? "", (Func<string, string>)null, (Func<string, string>)null);
			ApplyDropdownStyle(val10);
			INotifyValueChangedExtensions.RegisterValueChangedCallback<string>((INotifyValueChanged<string>)(object)val10, (EventCallback<ChangeEvent<string>>)((ChangeEvent<string> evt) =>
			{
				Plugin.SelectedElement = evt.newValue;
				Debug.Log((object)("[ModSettings] Selected Element changed to: " + evt.newValue));
			}));
			((VisualElement)val7).Add((VisualElement)(object)val10);
			Toggle val11 = new Toggle("Group DPS");
			((BaseField<bool>)(object)val11).value = Plugin.ShowGroupDPS;
			ApplyToggleStyle(val11);
			INotifyValueChangedExtensions.RegisterValueChangedCallback<bool>((INotifyValueChanged<bool>)(object)val11, (EventCallback<ChangeEvent<bool>>)((ChangeEvent<bool> evt) =>
			{
				Plugin.ShowGroupDPS = evt.newValue;
				if ((Object)(object)GroupDamageMeter.Instance != (Object)null)
				{
					GroupDamageMeter.Instance.UpdateVisibilityState();
				}
				Debug.Log((object)$"[ModSettings] Group DPS Toggle changed to: {evt.newValue}");
			}));
			((VisualElement)val7).Add((VisualElement)(object)val11);
			VisualElement val12 = new VisualElement();
			val12.name = "ModsSection_Loot";
			val12.style.marginTop = StyleLength.op_Implicit(25f);
			Label val13 = new Label("Lootfilter Mod Settings");
			((VisualElement)val13).style.fontSize = StyleLength.op_Implicit(24f);
			((VisualElement)val13).style.color = StyleColor.op_Implicit(new Color(0.702f, 0.639f, 0.471f, 1f));
			((VisualElement)val13).style.unityFontStyleAndWeight = StyleEnum<FontStyle>.op_Implicit((FontStyle)1);
			((VisualElement)val13).style.marginLeft = StyleLength.op_Implicit(6f);
			((VisualElement)val13).style.marginBottom = StyleLength.op_Implicit(8f);
			val12.Add((VisualElement)(object)val13);
			ScrollView val14 = new ScrollView();
			((VisualElement)val14).name = "ModsContent";
			((VisualElement)val14).style.flexGrow = StyleFloat.op_Implicit(1f);
			((VisualElement)val14).style.marginTop = StyleLength.op_Implicit(6f);
			List<string> list2 = new List<string> { "off", "equal and above" };
			DropdownField val15 = new DropdownField("Lootfilter", list2, Plugin.SelectedRarityMode ?? "off", (Func<string, string>)null, (Func<string, string>)null);
			ApplyDropdownStyle(val15);
			INotifyValueChangedExtensions.RegisterValueChangedCallback<string>((INotifyValueChanged<string>)(object)val15, (EventCallback<ChangeEvent<string>>)((ChangeEvent<string> evt) =>
			{
				Plugin.SelectedRarityMode = evt.newValue;
				Debug.Log((object)("[ModSettings] Selected Element changed to: " + evt.newValue));
			}));
			((VisualElement)val14).Add((VisualElement)(object)val15);
			List<string> list3 = new List<string> { "equipment", "item", "both" };
			DropdownField val16 = new DropdownField("Pickup labels", list3, Plugin.SelectedPickupMode, (Func<string, string>)null, (Func<string, string>)null);
			ApplyDropdownStyle(val16);
			INotifyValueChangedExtensions.RegisterValueChangedCallback<string>((INotifyValueChanged<string>)(object)val16, (EventCallback<ChangeEvent<string>>)((ChangeEvent<string> evt) =>
			{
				Plugin.SelectedPickupMode = evt.newValue;
				Debug.Log((object)("[ModSettings] Pickup mode changed to: " + evt.newValue));
			}));
			((VisualElement)val14).Add((VisualElement)(object)val16);
			foreach (string itemTypeKey in Plugin.GetHighlightItemTypeOptions())
			{
				Toggle val17 = new Toggle("Highlight " + Plugin.GetHighlightItemTypeDisplayName(itemTypeKey));
				((BaseField<bool>)(object)val17).value = Plugin.IsHighlightTypeEnabled(itemTypeKey);
				ApplyToggleStyle(val17);
				INotifyValueChangedExtensions.RegisterValueChangedCallback<bool>((INotifyValueChanged<bool>)(object)val17, (EventCallback<ChangeEvent<bool>>)((ChangeEvent<bool> evt) =>
				{
					Plugin.SetHighlightTypeEnabled(itemTypeKey, evt.newValue);
					Debug.Log((object)$"[ModSettings] Highlight type '{itemTypeKey}' changed to: {evt.newValue}");
				}));
				((VisualElement)val14).Add((VisualElement)(object)val17);
			}
			List<string> list4 = new List<string>(Enum.GetNames(typeof(ModSettingsPage.RarityThreshold)));
			string text = Enum.GetName(typeof(ModSettingsPage.RarityThreshold), (ModSettingsPage.RarityThreshold)Plugin.MinEquipmentRarityThreshold) ?? ModSettingsPage.RarityThreshold.Epic.ToString();
			DropdownField val18 = new DropdownField("Equipment min rarity", list4, text, (Func<string, string>)null, (Func<string, string>)null);
			ApplyDropdownStyle(val18);
			INotifyValueChangedExtensions.RegisterValueChangedCallback<string>((INotifyValueChanged<string>)(object)val18, (EventCallback<ChangeEvent<string>>)((ChangeEvent<string> evt) =>
			{
				if (Enum.TryParse<ModSettingsPage.RarityThreshold>(evt.newValue, out var result))
				{
					int minEquipmentRarityThreshold = (int)result;
					Plugin.MinEquipmentRarityThreshold = minEquipmentRarityThreshold;
					Debug.Log((object)$"[ModSettings] Equipment rarity threshold changed to: {result} (int: {Plugin.MinEquipmentRarityThreshold})");
				}
			}));
			((VisualElement)val14).Add((VisualElement)(object)val18);
			string text2 = Enum.GetName(typeof(ModSettingsPage.RarityThreshold), (ModSettingsPage.RarityThreshold)Plugin.MinItemRarityThreshold) ?? ModSettingsPage.RarityThreshold.Epic.ToString();
			DropdownField val19 = new DropdownField("Item min rarity", list4, text2, (Func<string, string>)null, (Func<string, string>)null);
			ApplyDropdownStyle(val19);
			INotifyValueChangedExtensions.RegisterValueChangedCallback<string>((INotifyValueChanged<string>)(object)val19, (EventCallback<ChangeEvent<string>>)((ChangeEvent<string> evt) =>
			{
				if (Enum.TryParse<ModSettingsPage.RarityThreshold>(evt.newValue, out var result))
				{
					int minItemRarityThreshold = (int)result;
					Plugin.MinItemRarityThreshold = minItemRarityThreshold;
					Debug.Log((object)$"[ModSettings] Item rarity threshold changed to: {result} (int: {Plugin.MinItemRarityThreshold})");
				}
			}));
			((VisualElement)val14).Add((VisualElement)(object)val19);
			val5.Add((VisualElement)(object)val7);
			val3.Add(val5);
			val12.Add((VisualElement)(object)val14);
			val3.Add(val12);
			Debug.Log((object)"[ModSettings] Successfully patched Mod Settings page into settings menu!");
		}
		catch (Exception arg)
		{
			Debug.LogError((object)$"[ModSettings] Failed to patch SettingsPage: {arg}");
		}
	}
}
namespace BlackveilDpsMeter
{
	public static class PermanentItemLabelPatch
	{
		private static readonly Dictionary<EntityId, WorldItemLabel> ActiveLabels = new Dictionary<EntityId, WorldItemLabel>();

		private static readonly Dictionary<int, int> RarityRankMap = new Dictionary<int, int>
		{
			{ 0, 1 },
			{ 1, 2 },
			{ 2, 3 },
			{ 3, 4 },
			{ 4, 5 },
			{ 5, 1 }
		};

		internal static void Apply(Harmony harmony)
		{
			Debug.Log((object)"[ItemLabels] --- Initializing PermanentItemLabelPatch ---");
			try
			{
				PatchPickupType(harmony, typeof(EquipmentPickup), "Postfix_OnEquipmentProcessed");
				PatchPickupType(harmony, typeof(ItemPickup), "Postfix_OnItemPickupProcessed");
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("[ItemLabels] Error during patch application: " + ex.Message));
			}
		}

		private static void PatchPickupType(Harmony harmony, Type pickupType, string postfixName)
		{
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Expected Obj, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Expected Obj, but got Unknown
			int num = 0;
			MethodInfo[] methods = pickupType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo in array)
			{
				string name = methodInfo.Name;
				if (name.Contains("Spawn") || name.Contains("Init") || name.Contains("Setup") || name.Contains("SetEquipment") || name.Equals("OnEnable") || name.Equals("OnSpawned"))
				{
					harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(typeof(PermanentItemLabelPatch), postfixName, (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
					Debug.Log((object)("[ItemLabels] Successfully hooked " + pickupType.Name + "." + name + "()"));
					num++;
				}
			}
			if (num == 0)
			{
				Debug.LogWarning((object)("[ItemLabels] No matching setup methods found on " + pickupType.Name + "."));
			}
			MethodInfo methodInfo2 = AccessTools.Method(pickupType, "OnDisable", (Type[])null, (Type[])null);
			if (methodInfo2 != null)
			{
				harmony.Patch((MethodBase)methodInfo2, (HarmonyMethod)null, new HarmonyMethod(typeof(PermanentItemLabelPatch), "Postfix_OnItemDespawned", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}

		[HarmonyPostfix]
		private static void Postfix_OnEquipmentProcessed(EquipmentPickup __instance)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)__instance == (Object)null || !Plugin.ShouldShowEquipmentLabels)
			{
				return;
			}
			try
			{
				if (!string.IsNullOrEmpty(Plugin.SelectedRarityMode) && !Plugin.SelectedRarityMode.Equals("off", StringComparison.OrdinalIgnoreCase))
				{
					NetworkEquipmentDescriptor equipment = __instance.Equipment;
					int rawRarity = Convert.ToInt32(equipment.RarityLevel);
					int num = NormalizeRarity(rawRarity);
					int minEquipmentRarityThreshold = Plugin.MinEquipmentRarityThreshold;
					if (num >= minEquipmentRarityThreshold && Plugin.SelectedRarityMode == "equal and above")
					{
						CreateWorldLabel(__instance);
					}
					else if (num == minEquipmentRarityThreshold && Plugin.SelectedRarityMode == "exclusive")
					{
						CreateWorldLabel(__instance);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)("[ItemLabels] Exception in Postfix_OnEquipmentProcessed: " + ex.Message));
			}
		}

		[HarmonyPostfix]
		private static void Postfix_OnItemPickupProcessed(ItemPickup __instance)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Expected Obj, but got Unknown
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Expected I4, but got Unknown
			if ((Object)(object)__instance == (Object)null || !Plugin.ShouldShowItemLabels)
			{
				return;
			}
			try
			{
				if (string.IsNullOrEmpty(Plugin.SelectedRarityMode) || Plugin.SelectedRarityMode.Equals("off", StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
				GenericItemDescriptor val = new GenericItemDescriptor(__instance.Item);
				if (Plugin.ShouldHighlightItemType(((ItemDescriptor)val).ItemType))
				{
					int rawRarity = (int)val.Rarity;
					int num = NormalizeRarity(rawRarity);
					int minItemRarityThreshold = Plugin.MinItemRarityThreshold;
					if (num >= minItemRarityThreshold && Plugin.SelectedRarityMode == "equal and above")
					{
						CreateWorldLabel(__instance);
					}
					else if (num == minItemRarityThreshold && Plugin.SelectedRarityMode == "exclusive")
					{
						CreateWorldLabel(__instance);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)("[ItemLabels] Exception in Postfix_OnItemPickupProcessed: " + ex.Message));
			}
		}

		private static int NormalizeRarity(int rawRarity)
		{
			if (RarityRankMap.TryGetValue(rawRarity, out var value))
			{
				return value;
			}
			return 1;
		}

		[HarmonyPostfix]
		private static void Postfix_OnItemDespawned(PickupItemWithUI __instance)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)__instance == (Object)null)
			{
				return;
			}
			EntityId entityId = ((Object)__instance).GetEntityId();
			if (ActiveLabels.TryGetValue(entityId, out var value))
			{
				if ((Object)(object)value != (Object)null)
				{
					Object.Destroy((Object)(object)value);
				}
				ActiveLabels.Remove(entityId);
			}
		}

		private static void CreateWorldLabel(EquipmentPickup pickup)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			EntityId entityId = ((Object)pickup).GetEntityId();
			if (!ActiveLabels.ContainsKey(entityId))
			{
				WorldItemLabel worldItemLabel = ((Component)pickup).gameObject.AddComponent<WorldItemLabel>();
				worldItemLabel.Initialize(pickup);
				ActiveLabels[entityId] = worldItemLabel;
			}
		}

		private static void CreateWorldLabel(ItemPickup pickup)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			EntityId entityId = ((Object)pickup).GetEntityId();
			if (!ActiveLabels.ContainsKey(entityId))
			{
				WorldItemLabel worldItemLabel = ((Component)pickup).gameObject.AddComponent<WorldItemLabel>();
				worldItemLabel.Initialize_Item(pickup);
				ActiveLabels[entityId] = worldItemLabel;
			}
		}
	}
	public class WorldItemLabel : MonoBehaviour
	{
		private EquipmentPickup _pickup;

		private ItemPickup _itemPickup;

		private Camera _mainCamera;

		private string _labelText;

		private Color _textColor;

		private Color _backgroundColor;

		private GUIStyle _style;

		private static Texture2D _bgTexture;

		private static Texture2D _uberTexture;

		private static Texture2D _luckyTexture;

		private static Texture2D _chaosTexture;

		private static bool _texturesLoaded;

		private bool _isUber;

		private bool _isLucky;

		private bool _isChaos;

		private static void LoadTexturesOnce()
		{
			if (_texturesLoaded)
			{
				return;
			}
			_texturesLoaded = true;
			Texture2D[] array = Resources.FindObjectsOfTypeAll<Texture2D>();
			Texture2D[] array2 = array;
			foreach (Texture2D val in array2)
			{
				if (!((Object)(object)val == (Object)null))
				{
					string name = ((Object)val).name;
					if ((Object)(object)_uberTexture == (Object)null && name.Equals("StatType_Uber", StringComparison.OrdinalIgnoreCase))
					{
						_uberTexture = val;
					}
					else if ((Object)(object)_luckyTexture == (Object)null && name.Equals("Bonus_Lucky", StringComparison.OrdinalIgnoreCase))
					{
						_luckyTexture = val;
					}
					else if ((Object)(object)_chaosTexture == (Object)null && name.Equals("StatType_Chaos", StringComparison.OrdinalIgnoreCase))
					{
						_chaosTexture = val;
					}
					if ((Object)(object)_uberTexture != (Object)null && (Object)(object)_luckyTexture != (Object)null && (Object)(object)_chaosTexture != (Object)null)
					{
						break;
					}
				}
			}
		}

		public void Initialize(EquipmentPickup pickup)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected Obj, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected Obj, but got Unknown
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Expected I4, but got Unknown
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			_pickup = pickup;
			_itemPickup = null;
			if (!IsLocalPlayerAllowedToPickup((PickupItemWithUI)(object)pickup))
			{
				((Behaviour)this).enabled = false;
				return;
			}
			_mainCamera = Camera.main;
			EquipmentDescriptor val = new EquipmentDescriptor(pickup.Equipment);
			LocLabel val2 = new LocLabel();
			val.SetNameToLabel(val2);
			string labelText = ((!string.IsNullOrEmpty(((TextElement)val2).text)) ? ((TextElement)val2).text : ((ItemDescriptor)val).Name);
			_labelText = labelText;
			_isUber = false;
			if (val.Properties != null)
			{
				for (int i = 0; i < val.Properties.Length; i++)
				{
					if (val.Properties[i].IsUber)
					{
						_isUber = true;
						break;
					}
				}
			}
			_isLucky = val.IsLucky;
			_isChaos = val.IsChaos;
			NetworkEquipmentDescriptor equipment = pickup.Equipment;
			int rarityValue = (int)equipment.RarityLevel;
			_textColor = GetRarityTextColor(rarityValue);
			_backgroundColor = GetRarityBackgroundColor(rarityValue);
			if ((Object)(object)_bgTexture == (Object)null)
			{
				_bgTexture = MakeSolidTexture(1, 1, Color.white);
			}
			LoadTexturesOnce();
			BuildStyle();
		}

		public void Initialize_Item(ItemPickup itemPickup)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected Obj, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected Obj, but got Unknown
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected I4, but got Unknown
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			_itemPickup = itemPickup;
			_pickup = null;
			if (!IsLocalPlayerAllowedToPickup((PickupItemWithUI)(object)itemPickup))
			{
				((Behaviour)this).enabled = false;
				return;
			}
			_mainCamera = Camera.main;
			GenericItemDescriptor val = new GenericItemDescriptor(itemPickup.Item);
			LocLabel val2 = new LocLabel();
			val.SetNameToLabel(val2);
			string labelText = ((!string.IsNullOrEmpty(((TextElement)val2).text)) ? ((TextElement)val2).text : ((ItemDescriptor)val).Name);
			_labelText = labelText;
			_isUber = false;
			_isLucky = false;
			_isChaos = false;
			int rarityValue = (int)val.Rarity;
			_textColor = GetRarityTextColor(rarityValue);
			_backgroundColor = GetRarityBackgroundColor(rarityValue);
			if ((Object)(object)_bgTexture == (Object)null)
			{
				_bgTexture = MakeSolidTexture(1, 1, Color.white);
			}
			LoadTexturesOnce();
			BuildStyle();
		}

		private void BuildStyle()
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Expected Obj, but got Unknown
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			_style = new GUIStyle
			{
				fontSize = 16,
				fontStyle = (FontStyle)1,
				alignment = (TextAnchor)3,
				richText = true
			};
			_style.normal.textColor = _textColor;
		}

		private Color GetRarityTextColor(int rarityValue)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			return rarityValue switch
			{
				1 => new Color(0.78f, 0.88f, 1f), 
				2 => new Color(0.89f, 0.79f, 0.98f), 
				3 => new Color(1f, 0.88f, 0.6f), 
				4 => new Color(0.98f, 0.72f, 0.76f), 
				5 => new Color(0.72f, 0.98f, 0.76f), 
				_ => Color.white, 
			};
		}

		private Color GetRarityBackgroundColor(int rarityValue)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			return rarityValue switch
			{
				1 => new Color(0.12f, 0.18f, 0.28f, 0.9f), 
				2 => new Color(0.18f, 0.16f, 0.24f, 0.9f), 
				3 => new Color(0.22f, 0.18f, 0.12f, 0.9f), 
				4 => new Color(0.24f, 0.14f, 0.16f, 0.9f), 
				5 => new Color(0.14f, 0.24f, 0.16f, 0.9f), 
				_ => new Color(0.12f, 0.12f, 0.12f, 0.85f), 
			};
		}

		public bool IsLocalPlayerAllowedToPickup(PickupItemWithUI pickup)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Invalid comparison between Unknown and I4
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Expected I4, but got Unknown
			if ((Object)(object)pickup == (Object)null)
			{
				return false;
			}
			PlayerManager instance = PlayerManager.Instance;
			Player val = ((instance != null) ? instance.LocalPlayer : null);
			if ((Object)(object)val == (Object)null)
			{
				return false;
			}
			if ((int)pickup.PlayerFilter != 3)
			{
				int slotIndex = val.SlotIndex;
				int num = (int)pickup.PlayerFilter;
				if (slotIndex != num)
				{
					return false;
				}
			}
			return true;
		}

		private Texture2D MakeSolidTexture(int width, int height, Color color)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected Obj, but got Unknown
			Color[] array = new Color[width * height];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = color;
			}
			Texture2D val = new Texture2D(width, height);
			val.SetPixels(array);
			val.Apply();
			return val;
		}

		private void OnGUI()
		{
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Expected Obj, but got Unknown
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_024e: Expected Obj, but got Unknown
			//IL_0269: Unknown result type (might be due to invalid IL or missing references)
			//IL_0296: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0300: Unknown result type (might be due to invalid IL or missing references)
			//IL_0335: Unknown result type (might be due to invalid IL or missing references)
			//IL_0348: Unknown result type (might be due to invalid IL or missing references)
			//IL_039f: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0443: Unknown result type (might be due to invalid IL or missing references)
			//IL_0448: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = null;
			if ((Object)(object)_pickup != (Object)null)
			{
				val = ((Component)_pickup).gameObject;
			}
			else if ((Object)(object)_itemPickup != (Object)null)
			{
				val = ((Component)_itemPickup).gameObject;
			}
			if ((Object)(object)val == (Object)null || !val.activeInHierarchy || ((Object)(object)UIManager.Instance != (Object)null && UIManager.Instance.IsMainLayerPageOpen))
			{
				return;
			}
			if ((Object)(object)_mainCamera == (Object)null)
			{
				_mainCamera = Camera.main;
			}
			if (!((Behaviour)this).enabled || (Object)(object)_mainCamera == (Object)null)
			{
				return;
			}
			Vector3 val2 = ((Component)this).transform.position + new Vector3(0f, 1.8f, 0f);
			Vector3 val3 = _mainCamera.WorldToScreenPoint(val2);
			if (!(val3.z <= 0f))
			{
				_style.alignment = (TextAnchor)3;
				Vector2 val4 = _style.CalcSize(new GUIContent(_labelText));
				float num = 16f;
				float num2 = 8f;
				float num3 = 16f;
				float num4 = 2f;
				float num5 = 0f;
				if (_isUber)
				{
					num5 += num4 + num3;
				}
				if (_isLucky)
				{
					num5 += num3 + num4;
				}
				if (_isChaos)
				{
					num5 += num4 + num3;
				}
				float num6 = val4.x + num5 + num;
				float num7 = val4.y + num2;
				float num8 = val3.x - num6 / 2f;
				float num9 = (float)Screen.height - val3.y - num7 / 2f;
				Rect val5 = new Rect(num8, num9, num6, num7);
				Color color = GUI.color;
				GUI.color = _backgroundColor;
				GUI.DrawTexture(val5, (Texture)(object)_bgTexture);
				GUI.color = color;
				float num10 = num8 + num / 2f;
				Rect val6 = new Rect(num10, num9, val4.x, num7);
				GUIStyle val7 = new GUIStyle(_style)
				{
					alignment = (TextAnchor)3
				};
				val7.normal.textColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
				GUI.Label(new Rect(val6.x - 2f, val6.y, val6.width, val6.height), _labelText, val7);
				GUI.Label(new Rect(val6.x + 2f, val6.y, val6.width, val6.height), _labelText, val7);
				GUI.Label(new Rect(val6.x, val6.y - 2f, val6.width, val6.height), _labelText, val7);
				GUI.Label(new Rect(val6.x, val6.y + 2f, val6.width, val6.height), _labelText, val7);
				GUI.Label(val6, _labelText, _style);
				float num11 = val6.xMax + num4;
				float num12 = num9 + (num7 - num3) / 2f;
				if (_isUber && (Object)(object)_uberTexture != (Object)null)
				{
					Rect val8 = new Rect(num11, num12, num3, num3);
					GUI.DrawTexture(val8, (Texture)(object)_uberTexture, (ScaleMode)2, true);
					num11 += num3 + num4;
				}
				if (_isLucky && (Object)(object)_luckyTexture != (Object)null)
				{
					Rect val9 = new Rect(num11, num12 - 3f, num3 + 6f, num3 + 6f);
					GUI.DrawTexture(val9, (Texture)(object)_luckyTexture, (ScaleMode)2, true);
					num11 += num3 + 6f + num4;
				}
				if (_isChaos && (Object)(object)_chaosTexture != (Object)null)
				{
					Rect val10 = new Rect(num11, num12, num3, num3);
					GUI.DrawTexture(val10, (Texture)(object)_chaosTexture, (ScaleMode)2, true);
				}
			}
		}
	}
	[BepInPlugin("vibecoded_mod_1", "RoB_QoL_Mod", "1.0.0")]
	public class Plugin : BaseUnityPlugin
	{
		private class HotkeyRunner : MonoBehaviour
		{
			private bool _isKeyHeld = false;

			private void Update()
			{
				Keyboard current = Keyboard.current;
				if (current != null)
				{
					bool isPressed = ((ButtonControl)current.pKey).isPressed;
					if (((ButtonControl)current.oKey).isPressed)
					{
						Instance?.ResetMeter();
					}
					if (isPressed && !_isKeyHeld)
					{
						Debug.Log((object)"[DPS] P Pressed: Toggling UI Visibility");
						_isKeyHeld = true;
						Debug.Log((object)$"[DPS] Current Visibility: {_isVisible} | Toggling to: {!_isVisible} | settings ShowDPSMeter: {ShowDPSMeter}");
						Instance?.ToggleUIVisibility();
					}
					else if (!isPressed)
					{
						_isKeyHeld = false;
					}
				}
			}
		}

		public static Plugin Instance;

		public PlayerStats LocalPlayerStats = new PlayerStats();

		public Dictionary<int, PlayerStats> AllPlayerStats = new Dictionary<int, PlayerStats>();

		public float ActiveCombatTime = 0f;

		public float StartTime = -1f;

		public float LastHitTime = -1f;

		public int LocalPlayerActorID = -1;

		private static bool _instanceExists = false;

		public bool fetchingActorID = false;

		private bool mainSceneLoaded = false;

		public bool go_timer = false;

		public static bool _isVisible = true;

		private GameObject _uiBusHost;

		private bool _uiBusReady;

		public static ConfigEntry<bool> ShowDPSMeterConfig;

		public static ConfigEntry<bool> ShowGroupDPSConfig;

		public static ConfigEntry<bool> ShowCombatInfoConfig;

		public static ConfigEntry<string> SelectedElementConfig;

		public static ConfigEntry<int> MinRarityThresholdConfig;

		public static ConfigEntry<int> MinEquipmentRarityThresholdConfig;

		public static ConfigEntry<int> MinItemRarityThresholdConfig;

		public static ConfigEntry<bool> HighlightCurrencyConfig;

		public static ConfigEntry<string> HighlightedItemTypeConfig;

		public static ConfigEntry<string> SelectedRarityModeConfig;

		public static ConfigEntry<string> SelectedPickupModeConfig;

		public static readonly List<string> HighlightItemTypeOptions = new List<string> { "currency", "transmutationstone", "raritystone" };

		public static bool ShowDPSMeter
		{
			get
			{
				return ShowDPSMeterConfig?.Value ?? false;
			}
			set
			{
				if (ShowDPSMeterConfig != null)
				{
					ShowDPSMeterConfig.Value = value;
					((ConfigEntryBase)ShowDPSMeterConfig).ConfigFile.Save();
				}
			}
		}

		public static bool ShowGroupDPS
		{
			get
			{
				return ShowGroupDPSConfig?.Value ?? false;
			}
			set
			{
				if (ShowGroupDPSConfig != null)
				{
					ShowGroupDPSConfig.Value = value;
					((ConfigEntryBase)ShowGroupDPSConfig).ConfigFile.Save();
				}
			}
		}

		public static bool ShowCombatInfo
		{
			get
			{
				return ShowCombatInfoConfig?.Value ?? false;
			}
			set
			{
				if (ShowCombatInfoConfig != null)
				{
					ShowCombatInfoConfig.Value = value;
					((ConfigEntryBase)ShowCombatInfoConfig).ConfigFile.Save();
				}
			}
		}

		public static string SelectedElement
		{
			get
			{
				return SelectedElementConfig?.Value ?? "";
			}
			set
			{
				if (SelectedElementConfig != null)
				{
					SelectedElementConfig.Value = value;
					((ConfigEntryBase)SelectedElementConfig).ConfigFile.Save();
				}
			}
		}

		public static int MinRarityThreshold
		{
			get
			{
				return MinEquipmentRarityThresholdConfig?.Value ?? MinRarityThresholdConfig?.Value ?? 2;
			}
			set
			{
				if (MinEquipmentRarityThresholdConfig != null)
				{
					MinEquipmentRarityThresholdConfig.Value = value;
					((ConfigEntryBase)MinEquipmentRarityThresholdConfig).ConfigFile.Save();
				}
				if (MinRarityThresholdConfig != null)
				{
					MinRarityThresholdConfig.Value = value;
					((ConfigEntryBase)MinRarityThresholdConfig).ConfigFile.Save();
				}
			}
		}

		public static int MinEquipmentRarityThreshold
		{
			get
			{
				return MinEquipmentRarityThresholdConfig?.Value ?? MinRarityThresholdConfig?.Value ?? 2;
			}
			set
			{
				if (MinEquipmentRarityThresholdConfig != null)
				{
					MinEquipmentRarityThresholdConfig.Value = value;
					((ConfigEntryBase)MinEquipmentRarityThresholdConfig).ConfigFile.Save();
				}
				if (MinRarityThresholdConfig != null)
				{
					MinRarityThresholdConfig.Value = value;
					((ConfigEntryBase)MinRarityThresholdConfig).ConfigFile.Save();
				}
			}
		}

		public static int MinItemRarityThreshold
		{
			get
			{
				return MinItemRarityThresholdConfig?.Value ?? MinEquipmentRarityThresholdConfig?.Value ?? MinRarityThresholdConfig?.Value ?? 2;
			}
			set
			{
				if (MinItemRarityThresholdConfig != null)
				{
					MinItemRarityThresholdConfig.Value = value;
					((ConfigEntryBase)MinItemRarityThresholdConfig).ConfigFile.Save();
				}
			}
		}

		public static string SelectedRarityMode
		{
			get
			{
				return SelectedRarityModeConfig?.Value ?? "off";
			}
			set
			{
				if (SelectedRarityModeConfig != null)
				{
					SelectedRarityModeConfig.Value = value;
					((ConfigEntryBase)SelectedRarityModeConfig).ConfigFile.Save();
				}
			}
		}

		public static bool HighlightCurrency
		{
			get
			{
				return IsHighlightTypeEnabled("currency");
			}
			set
			{
				SetHighlightTypeEnabled("currency", value);
				if (HighlightCurrencyConfig != null)
				{
					HighlightCurrencyConfig.Value = value;
					((ConfigEntryBase)HighlightCurrencyConfig).ConfigFile.Save();
				}
			}
		}

		public static bool HighlightTransmutationStone
		{
			get
			{
				return IsHighlightTypeEnabled("transmutationstone");
			}
			set
			{
				SetHighlightTypeEnabled("transmutationstone", value);
			}
		}

		public static bool HighlightRarityStone
		{
			get
			{
				return IsHighlightTypeEnabled("raritystone");
			}
			set
			{
				SetHighlightTypeEnabled("raritystone", value);
			}
		}

		public static string SelectedPickupMode
		{
			get
			{
				return SelectedPickupModeConfig?.Value ?? "both";
			}
			set
			{
				if (SelectedPickupModeConfig != null)
				{
					string text = value?.Trim().ToLowerInvariant();
					if (text != "equipment" && text != "item" && text != "both")
					{
						text = "both";
					}
					SelectedPickupModeConfig.Value = text;
					((ConfigEntryBase)SelectedPickupModeConfig).ConfigFile.Save();
				}
			}
		}

		public static bool ShouldShowEquipmentLabels => SelectedPickupMode == "equipment" || SelectedPickupMode == "both";

		public static bool ShouldShowItemLabels => SelectedPickupMode == "item" || SelectedPickupMode == "both";

		public static List<string> GetHighlightItemTypeOptions()
		{
			return new List<string>(HighlightItemTypeOptions);
		}

		public static string GetHighlightItemTypeDisplayName(string itemTypeKey)
		{
			if (string.IsNullOrWhiteSpace(itemTypeKey))
			{
				return "Custom";
			}
			return itemTypeKey.Trim().ToLowerInvariant() switch
			{
				"currency" => "Currency", 
				"transmutationstone" => "Control Cube and Chaos Prism", 
				"raritystone" => "Artificer Orb", 
				_ => itemTypeKey, 
			};
		}

		public static bool IsHighlightTypeEnabled(string itemTypeKey)
		{
			return GetEnabledHighlightTypes().Contains(NormalizeHighlightType(itemTypeKey));
		}

		public static void SetHighlightTypeEnabled(string itemTypeKey, bool enabled)
		{
			string text = NormalizeHighlightType(itemTypeKey);
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			HashSet<string> enabledTypes = GetEnabledHighlightTypes();
			if (enabled)
			{
				enabledTypes.Add(text);
			}
			else
			{
				enabledTypes.Remove(text);
			}
			if (HighlightedItemTypeConfig != null)
			{
				HighlightedItemTypeConfig.Value = string.Join(",", HighlightItemTypeOptions.Where((string type) => enabledTypes.Contains(type)).ToList());
				((ConfigEntryBase)HighlightedItemTypeConfig).ConfigFile.Save();
			}
		}

		public static bool ShouldHighlightItemType(ItemType itemType)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Invalid comparison between Unknown and I4
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Invalid comparison between Unknown and I4
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Invalid comparison between Unknown and I4
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Invalid comparison between Unknown and I4
			if ((int)itemType <= 40)
			{
				if (itemType - 30 <= 2 || (int)itemType == 40)
				{
					return HighlightCurrency;
				}
			}
			else
			{
				if ((int)itemType == 60)
				{
					return HighlightTransmutationStone;
				}
				if ((int)itemType == 61)
				{
					return HighlightRarityStone;
				}
			}
			return false;
		}

		private static HashSet<string> GetEnabledHighlightTypes()
		{
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (HighlightedItemTypeConfig != null)
			{
				string[] array = HighlightedItemTypeConfig.Value.Split(',', ';');
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i]?.Trim();
					if (!string.IsNullOrEmpty(text))
					{
						string text2 = NormalizeHighlightType(text);
						if (!string.IsNullOrEmpty(text2))
						{
							hashSet.Add(text2);
						}
					}
				}
			}
			if (hashSet.Count == 0 && HighlightCurrencyConfig != null && HighlightCurrencyConfig.Value)
			{
				hashSet.Add("currency");
			}
			if (hashSet.Count == 0)
			{
				foreach (string highlightItemTypeOption in HighlightItemTypeOptions)
				{
					hashSet.Add(highlightItemTypeOption);
				}
			}
			return hashSet;
		}

		private static string NormalizeHighlightType(string itemTypeKey)
		{
			if (string.IsNullOrWhiteSpace(itemTypeKey))
			{
				return string.Empty;
			}
			string text = itemTypeKey.Trim();
			switch (text.ToLowerInvariant())
			{
			case "currency":
			case "blackcoin":
			case "blackblood":
			case "glitter":
			case "scrap":
				return "currency";
			case "transmutationstone":
			case "transmutation stone":
				return "transmutationstone";
			case "raritystone":
			case "rarity stone":
				return "raritystone";
			default:
				return text.ToLowerInvariant();
			}
		}

		private void Awake()
		{
			//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02de: Expected Obj, but got Unknown
			if (_instanceExists && (Object)(object)Instance != (Object)(object)this)
			{
				((BaseUnityPlugin)this).Logger.LogWarning((object)"Duplicate plugin instance detected. Destroying duplicate.");
				Object.Destroy((Object)(object)((Component)this).gameObject);
				return;
			}
			Instance = this;
			_instanceExists = true;
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
			ShowDPSMeterConfig = ((BaseUnityPlugin)this).Config.Bind<bool>("General", "ShowDPSMeter", false, "Show the DPS meter overlay.");
			ShowGroupDPSConfig = ((BaseUnityPlugin)this).Config.Bind<bool>("General", "ShowGroupDPS", false, "Show the group DPS display.");
			ShowCombatInfoConfig = ((BaseUnityPlugin)this).Config.Bind<bool>("General", "ShowCombatInfo", false, "Show additional combat information.");
			SelectedElementConfig = ((BaseUnityPlugin)this).Config.Bind<string>("General", "SelectedElement", "", "The currently selected element for combat information.");
			MinRarityThresholdConfig = ((BaseUnityPlugin)this).Config.Bind<int>("Equipment", "MinRarityThreshold", 2, "Legacy shared rarity threshold for displayed labels.");
			MinEquipmentRarityThresholdConfig = ((BaseUnityPlugin)this).Config.Bind<int>("Equipment", "MinEquipmentRarityThreshold", 2, "Minimum rarity level for displayed equipment.");
			MinItemRarityThresholdConfig = ((BaseUnityPlugin)this).Config.Bind<int>("Equipment", "MinItemRarityThreshold", 2, "Minimum rarity level for displayed items.");
			HighlightCurrencyConfig = ((BaseUnityPlugin)this).Config.Bind<bool>("Equipment", "HighlightCurrency", true, "Legacy compatibility toggle for highlighting currency items.");
			HighlightedItemTypeConfig = ((BaseUnityPlugin)this).Config.Bind<string>("Equipment", "HighlightedItemTypes", "currency,transmutationstone,raritystone", "Comma-separated item categories to highlight in the loot filter. Supported values: currency, transmutationstone, raritystone.");
			SelectedRarityModeConfig = ((BaseUnityPlugin)this).Config.Bind<string>("Equipment", "RarityMode", "off", "Rarity mode for displayed equipment");
			SelectedPickupModeConfig = ((BaseUnityPlugin)this).Config.Bind<string>("Equipment", "PickupTypeMode", "both", "Which world pickup labels to show: equipment, item, or both.");
			if (string.IsNullOrWhiteSpace(HighlightedItemTypeConfig.Value))
			{
				HighlightedItemTypeConfig.Value = string.Join(",", HighlightItemTypeOptions);
				((ConfigEntryBase)HighlightedItemTypeConfig).ConfigFile.Save();
			}
			else
			{
				HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				string[] array = HighlightedItemTypeConfig.Value.Split(',', ';');
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i]?.Trim();
					if (!string.IsNullOrEmpty(text))
					{
						string text2 = NormalizeHighlightType(text);
						if (!string.IsNullOrEmpty(text2))
						{
							hashSet.Add(text2);
						}
					}
				}
				if (hashSet.Count == 0)
				{
					HighlightedItemTypeConfig.Value = string.Join(",", HighlightItemTypeOptions);
					((ConfigEntryBase)HighlightedItemTypeConfig).ConfigFile.Save();
				}
			}
			Harmony val = new Harmony("com.gemini.dpsmeter");
			val.PatchAll();
			HealthDamageLogPatch.Apply(val);
			PermanentItemLabelPatch.Apply(val);
			SceneManager.sceneLoaded -= OnSceneLoaded;
			SceneManager.sceneLoaded += OnSceneLoaded;
			((BaseUnityPlugin)this).Logger.LogInfo((object)"Mod Injected: Hidden Bus & Harmony Patches Active.");
		}

		private IEnumerator InitializeUiBusAfterSceneLoad()
		{
			yield return (object)new WaitForSecondsRealtime(0.5f);
			if (!_uiBusReady)
			{
				if ((Object)(object)_uiBusHost == (Object)null)
				{
					_uiBusHost = new GameObject("DPS_Global_Bus");
					((Object)_uiBusHost).hideFlags = (HideFlags)61;
					Object.DontDestroyOnLoad((Object)(object)_uiBusHost);
				}
				if ((Object)(object)_uiBusHost.GetComponent<PersistentUI>() == (Object)null)
				{
					_uiBusHost.AddComponent<PersistentUI>();
				}
				if ((Object)(object)_uiBusHost.GetComponent<CombatInfoModule>() == (Object)null)
				{
					_uiBusHost.AddComponent<CombatInfoModule>();
				}
				if ((Object)(object)_uiBusHost.GetComponent<GroupDamageMeter>() == (Object)null)
				{
					_uiBusHost.AddComponent<GroupDamageMeter>();
				}
				if ((Object)(object)_uiBusHost.GetComponent<HotkeyRunner>() == (Object)null)
				{
					_uiBusHost.AddComponent<HotkeyRunner>();
				}
				_uiBusReady = true;
			}
		}

		public void ToggleUIVisibility()
		{
			_isVisible = !_isVisible;
			Debug.Log((object)$"[UI] Visibility set to: {_isVisible}");
		}

		public void ResetMeter()
		{
			LocalPlayerStats.Reset();
			LocalPlayerStats.TotalDamage = 0f;
			LocalPlayerStats.TotalBurn = 0f;
			LocalPlayerStats.TotalRoot = 0f;
			LocalPlayerStats.TotalPoison = 0f;
			LocalPlayerStats.TotalBleed = 0f;
			LocalPlayerStats.TotalShock = 0f;
			LocalPlayerStats.TotalFrost = 0f;
			LocalPlayerStats.TotalCurse = 0f;
			LocalPlayerStats.TotalMinion = 0f;
			LocalPlayerStats.TotalBless = 0f;
			LocalPlayerStats.TotalFury = 0f;
			foreach (KeyValuePair<int, PlayerStats> allPlayerStat in AllPlayerStats)
			{
				allPlayerStat.Value.Reset();
			}
			AllPlayerStats.Clear();
			Instance.ActiveCombatTime = 0.11f;
		}

		public PlayerStats GetPlayerStats(int actorID)
		{
			if (!AllPlayerStats.ContainsKey(actorID))
			{
				AllPlayerStats[actorID] = new PlayerStats
				{
					ActorID = actorID
				};
			}
			return AllPlayerStats[actorID];
		}

		public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected Obj, but got Unknown
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Expected Obj, but got Unknown
			ResetMeter();
			ManualLogSource logger = ((BaseUnityPlugin)this).Logger;
			bool flag = default;
			BepInExInfoLogInterpolatedStringHandler val = new BepInExInfoLogInterpolatedStringHandler(28, 1, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("Meter reset via Scene Load: ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(scene.name);
			}
			logger.LogInfo(val);
			if (!_uiBusReady)
			{
				((MonoBehaviour)this).StartCoroutine(InitializeUiBusAfterSceneLoad());
			}
			if (scene.name == "MainScene")
			{
				mainSceneLoaded = true;
				go_timer = false;
			}
			else if (scene.name != "MainScene" && mainSceneLoaded)
			{
				mainSceneLoaded = false;
				fetchingActorID = true;
				go_timer = true;
			}
			else
			{
				fetchingActorID = false;
			}
			ManualLogSource logger2 = ((BaseUnityPlugin)this).Logger;
			val = new BepInExInfoLogInterpolatedStringHandler(41, 2, ref flag);
			if (flag)
			{
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral("[DPS] Scene Loaded: ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<string>(scene.name);
				((BepInExLogInterpolatedStringHandler)val).AppendLiteral(" | Fetching ActorID: ");
				((BepInExLogInterpolatedStringHandler)val).AppendFormatted<bool>(fetchingActorID);
			}
			logger2.LogInfo(val);
		}
	}
	public class PlayerStats
	{
		public int ActorID = -1;

		public string Name = "LocalPlayer";

		public float TotalDamage;

		public float TotalBurn;

		public float TotalPoison;

		public float TotalBleed;

		public float TotalShock;

		public float TotalRoot;

		public float TotalFrost;

		public float TotalCurse;

		public float TotalMinion;

		public float TotalBless;

		public float TotalFury;

		public void Reset()
		{
			TotalDamage = 0f;
			TotalBurn = 0f;
			TotalPoison = 0f;
			TotalBleed = 0f;
			TotalShock = 0f;
			TotalRoot = 0f;
			TotalFrost = 0f;
			TotalCurse = 0f;
			TotalMinion = 0f;
			TotalBless = 0f;
			TotalFury = 0f;
		}
	}
	public class PersistentUI : MonoBehaviour
	{
		private TextMeshProUGUI _uiText;

		private GameObject _canvasObj;

		private GameObject _panelObj;

		private float dpsupdateTime = 0f;

		public static Canvas _targetCanvas;

		private float _nextDebugTime = 0f;

		private void Start()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected Obj, but got Unknown
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Expected Obj, but got Unknown
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Expected Obj, but got Unknown
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0205: Expected Obj, but got Unknown
			//IL_023e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0269: Unknown result type (might be due to invalid IL or missing references)
			//IL_0276: Unknown result type (might be due to invalid IL or missing references)
			//IL_0283: Unknown result type (might be due to invalid IL or missing references)
			//IL_029a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0349: Unknown result type (might be due to invalid IL or missing references)
			_canvasObj = new GameObject("DPS_Overlay_Canvas");
			Object.DontDestroyOnLoad((Object)(object)_canvasObj);
			Canvas val = _canvasObj.AddComponent<Canvas>();
			val.renderMode = (RenderMode)0;
			val.sortingOrder = 999;
			_canvasObj.AddComponent<GraphicRaycaster>();
			_panelObj = new GameObject("DPS_Background_Frame");
			_panelObj.transform.SetParent(_canvasObj.transform, false);
			Image val2 = _panelObj.AddComponent<Image>();
			((Graphic)val2).color = new Color(0.45f, 0.35f, 0.2f, 1f);
			RectTransform component = ((Component)val2).GetComponent<RectTransform>();
			component.anchorMin = new Vector2(1f, 0.5f);
			component.anchorMax = new Vector2(1f, 0.5f);
			component.pivot = new Vector2(1f, 0.5f);
			component.anchoredPosition = new Vector2(-10f, 0f);
			component.sizeDelta = new Vector2(220f, 220f);
			Outline val3 = _panelObj.AddComponent<Outline>();
			((Shadow)val3).effectColor = new Color(0.15f, 0.1f, 0.05f, 1f);
			((Shadow)val3).effectDistance = new Vector2(2f, -2f);
			GameObject val4 = new GameObject("Inner_Area");
			val4.transform.SetParent(_panelObj.transform, false);
			Image val5 = val4.AddComponent<Image>();
			((Graphic)val5).color = new Color(0f, 0f, 0f, 0.95f);
			RectTransform rectTransform = ((Graphic)val5).rectTransform;
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = new Vector2(5f, 5f);
			rectTransform.offsetMax = new Vector2(-5f, -5f);
			GameObject val6 = new GameObject("DPS_Text_Display");
			val6.transform.SetParent(val4.transform, false);
			_uiText = val6.AddComponent<TextMeshProUGUI>();
			((TMP_Text)_uiText).fontSize = 16f;
			((Graphic)_uiText).color = Color.white;
			((TMP_Text)_uiText).alignment = (TextAlignmentOptions)514;
			RectTransform rectTransform2 = ((TMP_Text)_uiText).rectTransform;
			rectTransform2.anchorMin = Vector2.zero;
			rectTransform2.anchorMax = Vector2.one;
			rectTransform2.sizeDelta = Vector2.zero;
			rectTransform2.offsetMin = new Vector2(5f, 5f);
			rectTransform2.offsetMax = new Vector2(-5f, -5f);
			TMP_FontAsset val7 = null;
			TMP_FontAsset[] array = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
			TMP_FontAsset[] array2 = array;
			foreach (TMP_FontAsset val8 in array2)
			{
				if (((Object)val8).name.Contains("LiberationSans"))
				{
					val7 = val8;
					break;
				}
			}
			if ((Object)(object)val7 != (Object)null)
			{
				((TMP_Text)_uiText).font = val7;
			}
			else
			{
				Debug.LogWarning((object)"[DPS] Could not find LiberationSans SDF, using TMP default.");
			}
			((TMP_Text)_uiText).fontSize = 14f;
			((Graphic)_uiText).color = Color.yellow;
			((TMP_Text)_uiText).alignment = (TextAlignmentOptions)257;
			((TMP_Text)_uiText).overflowMode = (TextOverflowModes)0;
			((TMP_Text)_uiText).textWrappingMode = (TextWrappingModes)0;
			_targetCanvas = _canvasObj.GetComponent<Canvas>();
			((Behaviour)_targetCanvas).enabled = Plugin._isVisible;
		}

		private void Update()
		{
			//IL_05d4: Unknown result type (might be due to invalid IL or missing references)
			if (Plugin.Instance.fetchingActorID)
			{
				PlayerManager instance = PlayerManager.Instance;
				if ((Object)(object)instance != (Object)null && (Object)(object)instance.LocalPlayer != (Object)null)
				{
					int num = -1;
					Debug.Log((object)"[DPS] Attempting to capture LocalPlayer ActorID...");
					Debug.Log((object)$"[DPS] PlayerManager.LocalPlayer: {((Object)instance.LocalPlayer).name} | ActorID: {instance.LocalPlayerSlot}");
					num = instance.LocalPlayerSlot;
					if (num != -1)
					{
						Debug.Log((object)$"[DPS] Captured LocalPlayer ActorID: {num}");
						Plugin.Instance.LocalPlayerActorID = num;
						Plugin.Instance.LocalPlayerStats.ActorID = num;
						Plugin.Instance.fetchingActorID = false;
						Debug.Log((object)$"[DPS] ✓ LocalPlayerActorID set to {Plugin.Instance.LocalPlayerActorID}");
					}
				}
			}
			((Behaviour)_targetCanvas).enabled = Plugin.ShowDPSMeter && Plugin._isVisible;
			if (Time.time >= _nextDebugTime)
			{
				_nextDebugTime = Time.time + 2f;
				Debug.Log((object)"[DPS Debug] Heartbeat");
				PrintActivePlayersDebug();
			}
			if (Plugin.Instance.StartTime > 0f && Time.time - Plugin.Instance.LastHitTime <= 1f)
			{
				Plugin.Instance.ActiveCombatTime += Time.deltaTime;
			}
			if (!(Plugin.Instance.StartTime > 0f) || !(Time.time > dpsupdateTime))
			{
				return;
			}
			dpsupdateTime = Time.time + 0.5f;
			float displayTime = Mathf.Max(0.1f, Plugin.Instance.ActiveCombatTime);
			float dps_Total;
			int barWidth;
			int remainingSlots;
			string visualBar;
			if (displayTime > 0.1f)
			{
				dps_Total = Plugin.Instance.LocalPlayerStats.TotalDamage / displayTime;
				barWidth = 20;
				remainingSlots = barWidth;
				visualBar = "";
				AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalBurn, "#f17728");
				AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalPoison, "#952db8");
				AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalBleed, "#c94a46");
				AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalShock, "#b7a93f");
				AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalRoot, "#be927e");
				AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalFrost, "#57cccc");
				AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalCurse, "#267b5b");
				AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalMinion, "#f06ee9");
				AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalBless, "#d8f19c");
				AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalFury, "#64210f");
				if (remainingSlots > 0)
				{
					visualBar = visualBar + "<color=#555555>" + new string('█', remainingSlots) + "</color>";
				}
				string text = string.Join("\n", visualBar, "<size=+4>" + ColorText($"Total DPS: {dps_Total:F1}", "#dbdbdb") + "</size>", ColorText(FormatLine("Burn", Plugin.Instance.LocalPlayerStats.TotalBurn), "#f17728"), ColorText(FormatLine("Poison", Plugin.Instance.LocalPlayerStats.TotalPoison), "#952db8"), ColorText(FormatLine("Bleed", Plugin.Instance.LocalPlayerStats.TotalBleed), "#c94a46"), ColorText(FormatLine("Shock", Plugin.Instance.LocalPlayerStats.TotalShock), "#b7a93f"), ColorText(FormatLine("Root", Plugin.Instance.LocalPlayerStats.TotalRoot), "#be927e"), ColorText(FormatLine("Frost", Plugin.Instance.LocalPlayerStats.TotalFrost), "#57cccc"), ColorText(FormatLine("Curse", Plugin.Instance.LocalPlayerStats.TotalCurse), "#267b5b"), ColorText(FormatLine("Minion", Plugin.Instance.LocalPlayerStats.TotalMinion), "#f06ee9"), ColorText(FormatLine("Bless", Plugin.Instance.LocalPlayerStats.TotalBless), "#d8f19c"), ColorText(FormatLine("Fury", Plugin.Instance.LocalPlayerStats.TotalFury), "#64210f"));
				((TMP_Text)_uiText).text = "<width=75%>" + text + "</width>";
			}
			else
			{
				((Graphic)_uiText).color = new Color(0.7f, 0.7f, 0f);
			}
			void AddClippedSegment(float damage, string hex)
			{
				if (!(damage <= 0f) && remainingSlots > 0)
				{
					int num2 = Mathf.RoundToInt(damage / displayTime / dps_Total * (float)barWidth);
					num2 = Mathf.Min(num2, remainingSlots);
					if (num2 > 0)
					{
						visualBar = visualBar + "<color=" + hex + ">" + new string('█', num2) + "</color>";
						remainingSlots -= num2;
					}
				}
			}
			static string ColorText(string text2, string hex)
			{
				return "<color=" + hex + ">" + text2 + "</color>";
			}
			string FormatLine(string label, float val)
			{
				return $"{label}: {val / displayTime:F1} ({val / displayTime / dps_Total * 100f:F1}%)";
			}
		}

		private void PrintActivePlayersDebug()
		{
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			PlayerManager instance = PlayerManager.Instance;
			if ((Object)(object)instance == (Object)null)
			{
				Debug.Log((object)"[DPS Debug] PlayerManager.Instance is NULL");
				return;
			}
			List<Player> players = instance.GetPlayers();
			Debug.Log((object)$"--- [DPS Debug] Active Players Count: {players.Count} ---");
			for (int i = 0; i < players.Count; i++)
			{
				Player val = players[i];
				if ((Object)(object)val == (Object)null)
				{
					Debug.Log((object)$"  [{i}] PLAYER OBJECT IS NULL");
					continue;
				}
				string text = (string.IsNullOrEmpty(val.UserName) ? "EMPTY_NAME" : val.UserName);
				PlayerRef fusionPlayerRef = val.FusionPlayerRef;
				int playerId = fusionPlayerRef.PlayerId;
				int playerId2 = val.PlayerId;
				bool flag = (Object)(object)instance.LocalPlayer == (Object)(object)val;
				Debug.Log((object)$"  [{i}] Name: {text} | FusionID: {playerId} | PlayerID: {playerId2} | Local: {flag}");
			}
			Debug.Log((object)"------------------------------------------");
		}
	}
	internal static class HealthDamageLogPatch
	{
		internal static void Apply(Harmony harmony)
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Expected Obj, but got Unknown
			//IL_0074: Expected Obj, but got Unknown
			if (HealthDamageLogController.Initialize())
			{
				MethodInfo methodInfo = AccessTools.Method(typeof(Health), "Render", (Type[])null, (Type[])null);
				if (methodInfo == null)
				{
					Debug.LogWarning((object)"DPS Meter: Health.Render not found — damage logging inactive.");
					return;
				}
				harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(HealthDamageLogPatch), "RenderPrefix", (Type[])null), new HarmonyMethod(typeof(HealthDamageLogPatch), "RenderPostfix", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				Debug.Log((object)"DPS Meter: Health.Render patched.");
			}
		}

		private static void RenderPrefix(Health __instance, out int __state)
		{
			HealthDamageLogController.CaptureState(__instance, out __state);
		}

		private static void RenderPostfix(Health __instance, int __state)
		{
			HealthDamageLogController.ProcessNewEntries(__instance, __state);
		}
	}
	internal static class HealthDamageLogController
	{
		private static FieldInfo _lastVisualizedField;

		private static PropertyInfo _arrayProp;

		private static MethodInfo _arrayGetMethod;

		private static FieldInfo _valueField;

		private static FieldInfo _attackerIdField;

		private static FieldInfo _damageTypeField;

		private static FieldInfo _criticalField;

		private static FieldInfo _criticalInnerField;

		internal static bool Initialize()
		{
			_lastVisualizedField = AccessTools.Field(typeof(Health), "_lastVisualizedDamageData");
			if (_lastVisualizedField == null)
			{
				Debug.LogWarning((object)"DPS Meter: Health._lastVisualizedDamageData not found — inactive.");
				return false;
			}
			Type nestedType = typeof(Health).GetNestedType("NetworkedDamageData", BindingFlags.NonPublic);
			if (nestedType == null)
			{
				Debug.LogWarning((object)"DPS Meter: Health.NetworkedDamageData not found — inactive.");
				return false;
			}
			_arrayProp = AccessTools.Property(typeof(Health), "ReceivedDamageDataArray");
			if (_arrayProp == null)
			{
				Debug.LogWarning((object)"DPS Meter: Health.ReceivedDamageDataArray not found — inactive.");
				return false;
			}
			Type type = typeof(NetworkArray<>).MakeGenericType(nestedType);
			_arrayGetMethod = type.GetMethod("Get") ?? type.GetProperty("Item")?.GetMethod;
			if (_arrayGetMethod == null)
			{
				Debug.LogWarning((object)"DPS Meter: NetworkArray<T>.Get/Item not found — inactive.");
				return false;
			}
			_valueField = nestedType.GetField("value");
			_attackerIdField = nestedType.GetField("attackerID");
			_damageTypeField = nestedType.GetField("damageStatusType");
			_criticalField = nestedType.GetField("critical");
			if (_criticalField != null)
			{
				Type fieldType = _criticalField.FieldType;
				_criticalInnerField = fieldType.GetField("_isSet", BindingFlags.Instance | BindingFlags.NonPublic) ?? fieldType.GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic) ?? fieldType.GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);
			}
			return true;
		}

		internal static void CaptureState(Health instance, out int state)
		{
			state = (int)_lastVisualizedField.GetValue(instance);
		}

		internal static void ProcessNewEntries(Health instance, int oldCounter)
		{
			try
			{
				int num = (int)_lastVisualizedField.GetValue(instance);
				if (num <= oldCounter)
				{
					return;
				}
				object value = _arrayProp.GetValue(instance);
				for (int i = oldCounter; i < num; i++)
				{
					object obj = _arrayGetMethod.Invoke(value, new object[1] { i % 64 });
					if (obj != null)
					{
						float num2 = (float)(_valueField?.GetValue(obj) ?? ((object)0f));
						if (num2 != 0f)
						{
							int num3 = (int)(_attackerIdField?.GetValue(obj) ?? ((object)(-1)));
							object obj2 = _damageTypeField?.GetValue(obj);
							bool flag = ReadCritical(obj);
							UpdatePlayerStats(num2, obj2?.ToString() ?? "", num3, instance);
							Debug.Log((object)$"DPS Meter [{((Object)instance).name}] val={num2:F1} type={obj2} crit={flag} attackerId={num3}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)("DPS Meter: error — " + ex.Message));
			}
		}

		private static bool ReadCritical(object entry)
		{
			if (_criticalField == null || _criticalInnerField == null)
			{
				return false;
			}
			try
			{
				object value = _criticalField.GetValue(entry);
				if (value == null)
				{
					return false;
				}
				object value2 = _criticalInnerField.GetValue(value);
				return (value2 is byte b) ? (b != 0) : (value2 is int num && num != 0);
			}
			catch
			{
				return false;
			}
		}

		private static void UpdatePlayerStats(float val, string type, int attackerId, Health victimHealth = null)
		{
			if (Plugin.Instance.StartTime < 0f)
			{
				Plugin.Instance.StartTime = Time.time;
			}
			Plugin.Instance.LastHitTime = Time.time;
			Debug.Log((object)$"[DPS Meter] Processing Damage: {val} Type: {type} AttackerID: {attackerId}");
			PlayerStats playerStats = Plugin.Instance.GetPlayerStats(attackerId);
			playerStats.TotalDamage += val;
			if (attackerId != Plugin.Instance.LocalPlayerActorID)
			{
				return;
			}
			PlayerStats localPlayerStats = Plugin.Instance.LocalPlayerStats;
			localPlayerStats.TotalDamage += val;
			if (type.Contains("Burn"))
			{
				localPlayerStats.TotalBurn += val;
			}
			else if (type.Contains("Poison"))
			{
				localPlayerStats.TotalPoison += val;
			}
			else if (type.Contains("Bleed"))
			{
				localPlayerStats.TotalBleed += val;
			}
			else if (type.Contains("Shock"))
			{
				localPlayerStats.TotalShock += val;
			}
			else if (type.Contains("Curse"))
			{
				localPlayerStats.TotalCurse += val;
			}
			else if (type.Contains("Root"))
			{
				localPlayerStats.TotalRoot += val;
			}
			else if (type.Contains("Freeze") || type.Contains("Chill") || type.Contains("Frost"))
			{
				localPlayerStats.TotalFrost += val;
			}
			if (!((Object)(object)victimHealth != (Object)null))
			{
				return;
			}
			try
			{
				StatsManager component = ((Component)victimHealth).GetComponent<StatsManager>();
				if (!((Object)(object)component != (Object)null) || !component.IsFrozen || component.ActorID == attackerId)
				{
					return;
				}
				Debug.Log((object)$"[DPS] Victim is frozen. Checking for frozen bonus from attacker {attackerId}...");
				PlayerManager instance = PlayerManager.Instance;
				if (!((Object)(object)instance != (Object)null))
				{
					return;
				}
				Player val2 = instance.GetPlayers().FirstOrDefault((Player p) =>
				{
					StatsManager component3 = ((Component)p).GetComponent<StatsManager>();
					return (Object)(object)component3 != (Object)null && component3.ActorID == attackerId;
				});
				if (!((Object)(object)val2 != (Object)null))
				{
					return;
				}
				Chill component2 = ((Component)val2).GetComponent<Chill>();
				if ((Object)(object)component2 != (Object)null)
				{
					float damageMultiplierAgainstFrozenTarget = component2.DamageMultiplierAgainstFrozenTarget;
					if (damageMultiplierAgainstFrozenTarget > 1f)
					{
						float num = val / damageMultiplierAgainstFrozenTarget;
						float num2 = val - num;
						localPlayerStats.TotalFrost += num2;
						playerStats.TotalDamage += num2;
						Debug.Log((object)$"[DPS] Frozen Bonus Detected: +{num2:F1}");
					}
				}
			}
			catch
			{
			}
		}
	}
	[HarmonyPatch(typeof(Attack), "ModifyDamageWithModifierAndCritical")]
	public class RemoteDamageHook
	{
		private static void Postfix(object __instance, StatsManager victim, ref DamageDescriptor damageDesc, UserAction userAction, bool onlyForUI)
		{
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			if (damageDesc.damageValue == 0f)
			{
				return;
			}
			StatsManager value = Traverse.Create(__instance).Field("_stats").GetValue<StatsManager>();
			float damageValue = damageDesc.damageValue;
			if (onlyForUI)
			{
				if (Plugin.Instance.StartTime < 0f)
				{
					Plugin.Instance.StartTime = Time.time;
				}
				Plugin.Instance.LastHitTime = Time.time;
			}
			else if (value.IsChampionMinion)
			{
				int spawnerActorID = value.SpawnerActorID;
				if (spawnerActorID != -1)
				{
					PlayerStats playerStats = Plugin.Instance.GetPlayerStats(spawnerActorID);
					playerStats.TotalDamage += damageValue;
					if (spawnerActorID == Plugin.Instance.LocalPlayerActorID)
					{
						Plugin.Instance.LocalPlayerStats.TotalMinion += damageValue;
					}
					Debug.Log((object)$"[DPS] Distilled {damageValue} Minion Damage from Actor {value.ActorID}");
				}
			}
			else
			{
				if (!value.IsChampion || value.ActorID != Plugin.Instance.LocalPlayerActorID)
				{
					return;
				}
				PlayerStats localPlayerStats = Plugin.Instance.LocalPlayerStats;
				if ((int)userAction == 0)
				{
					if (damageDesc.blessedAttack)
					{
						float num = 1f + (float)value.Bless.EmpoweredAttackDamageIncrementPCT / 100f;
						float num2 = damageValue / num;
						localPlayerStats.TotalBless += damageValue - num2;
						Debug.Log((object)$"[DPS] Credited {value.Bless.EmpoweredAttackDamageIncrementPCT}% Blessed Damage");
					}
					if (damageDesc.furyAttack)
					{
						ChangeableDamageIncrement boostedAttackDamageIncPCT = value.Fury.BoostedAttackDamageIncPCT;
						float num3 = 1f + boostedAttackDamageIncPCT.Value / 100f;
						float num4 = damageValue / num3;
						localPlayerStats.TotalFury += damageValue - num4;
					}
				}
			}
		}
	}
	[HarmonyPatch(typeof(StatsManager), "CalculateMyDamageAgainst")]
	public class FrozenDamageHook
	{
		[HarmonyPostfix]
		private static void Postfix(StatsManager __instance, StatsManager victim, ref DamageDescriptor dmgDesc, bool calcForUI)
		{
			if ((Object)(object)victim == (Object)null || (Object)(object)__instance == (Object)null || !victim.IsFrozen)
			{
				return;
			}
			Chill component = ((Component)__instance).GetComponent<Chill>();
			StatsManager component2 = ((Component)__instance).GetComponent<StatsManager>();
			if (!((Object)(object)component != (Object)null) || !((Object)(object)component2 != (Object)null) || component2.ActorID != Plugin.Instance.LocalPlayerActorID)
			{
				return;
			}
			float damageMultiplierAgainstFrozenTarget = component.DamageMultiplierAgainstFrozenTarget;
			if (damageMultiplierAgainstFrozenTarget <= 1f)
			{
				return;
			}
			float num = dmgDesc.damageValue / damageMultiplierAgainstFrozenTarget;
			float num2 = dmgDesc.damageValue - num;
			if (num2 > 0f)
			{
				Plugin.Instance.LocalPlayerStats.TotalFrost += num2;
				if (Plugin.Instance.StartTime < 0f)
				{
					Plugin.Instance.StartTime = Time.time;
				}
				Plugin.Instance.LastHitTime = Time.time;
				Debug.Log((object)$"[DPS Meter] Dealt {num2} Frozen Bonus Damage.");
			}
		}
	}
}
namespace RaidersOfBlackveilMod
{
	public class CombatInfoModule : MonoBehaviour
	{
		private GameObject _canvasObj;

		private GameObject _panelObj;

		private TextMeshProUGUI _uiText;

		private float _updateTimer = 0f;

		private bool _lastPersistentUIVisibility = true;

		public static CombatInfoModule Instance { get; private set; }

		private void Awake()
		{
			if ((Object)(object)Instance != (Object)null && (Object)(object)Instance != (Object)(object)this)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
				return;
			}
			Instance = this;
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
			Debug.Log((object)"[CombatInfo] CombatInfoModule initialized and set to persist across scenes.");
		}

		private void Start()
		{
			SetupHUD();
		}

		private void SetupHUD()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected Obj, but got Unknown
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected Obj, but got Unknown
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Expected Obj, but got Unknown
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Expected Obj, but got Unknown
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_0291: Unknown result type (might be due to invalid IL or missing references)
			//IL_029e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
			_canvasObj = new GameObject("Modded_M_Power_Display_Root");
			Object.DontDestroyOnLoad((Object)(object)_canvasObj);
			_canvasObj.layer = 2;
			Canvas val = _canvasObj.AddComponent<Canvas>();
			val.renderMode = (RenderMode)0;
			val.overrideSorting = true;
			val.sortingOrder = 998;
			_panelObj = new GameObject("Modded_Frame_Background");
			_panelObj.transform.SetParent(_canvasObj.transform, false);
			_panelObj.layer = 2;
			Image val2 = _panelObj.AddComponent<Image>();
			((Graphic)val2).color = new Color(0.45f, 0.35f, 0.2f, 1f);
			RectTransform rectTransform = ((Graphic)val2).rectTransform;
			rectTransform.anchorMin = new Vector2(1f, 0.5f);
			rectTransform.anchorMax = new Vector2(1f, 0.5f);
			rectTransform.pivot = new Vector2(1f, 0.5f);
			rectTransform.anchoredPosition = new Vector2(-10f, -160f);
			rectTransform.sizeDelta = new Vector2(180f, 85f);
			Outline val3 = _panelObj.AddComponent<Outline>();
			((Shadow)val3).effectColor = new Color(0.15f, 0.1f, 0.05f, 1f);
			((Shadow)val3).effectDistance = new Vector2(2f, -2f);
			GameObject val4 = new GameObject("Modded_Inner_Area");
			val4.transform.SetParent(_panelObj.transform, false);
			val4.layer = 2;
			Image val5 = val4.AddComponent<Image>();
			((Graphic)val5).color = new Color(0f, 0f, 0f, 0.95f);
			RectTransform rectTransform2 = ((Graphic)val5).rectTransform;
			rectTransform2.anchorMin = Vector2.zero;
			rectTransform2.anchorMax = Vector2.one;
			rectTransform2.offsetMin = new Vector2(3f, 3f);
			rectTransform2.offsetMax = new Vector2(-3f, -3f);
			GameObject val6 = new GameObject("Modded_Value_Text");
			val6.transform.SetParent(val4.transform, false);
			val6.layer = 2;
			_uiText = val6.AddComponent<TextMeshProUGUI>();
			((TMP_Text)_uiText).fontSize = 14f;
			((Graphic)_uiText).color = Color.white;
			((TMP_Text)_uiText).alignment = (TextAlignmentOptions)514;
			RectTransform rectTransform3 = ((TMP_Text)_uiText).rectTransform;
			rectTransform3.anchorMin = Vector2.zero;
			rectTransform3.anchorMax = Vector2.one;
			rectTransform3.sizeDelta = Vector2.zero;
			rectTransform3.offsetMin = new Vector2(5f, 5f);
			rectTransform3.offsetMax = new Vector2(-5f, -5f);
			TMP_FontAsset[] array = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
			TMP_FontAsset[] array2 = array;
			foreach (TMP_FontAsset val7 in array2)
			{
				if (((Object)val7).name.Contains("LiberationSans"))
				{
					((TMP_Text)_uiText).font = val7;
					break;
				}
			}
			UpdateVisibilityState();
		}

		private void Update()
		{
			bool isVisible = Plugin._isVisible;
			if (isVisible != _lastPersistentUIVisibility)
			{
				_lastPersistentUIVisibility = isVisible;
				UpdateVisibilityState();
			}
			if ((Object)(object)_canvasObj == (Object)null || (Object)(object)_panelObj == (Object)null || !Plugin.Instance.go_timer)
			{
				return;
			}
			_updateTimer += Time.deltaTime;
			if (_updateTimer >= 0.25f)
			{
				_updateTimer = 0f;
				(float magicPower, float physicalPower, float CriticalChance, float CriticalDamage, bool isOutOfDanger, float element) tuple = FetchCombatData();
				float item = tuple.magicPower;
				float item2 = tuple.physicalPower;
				float item3 = tuple.CriticalChance;
				float item4 = tuple.CriticalDamage;
				bool item5 = tuple.isOutOfDanger;
				float item6 = tuple.element;
				string arg = (item5 ? "#A0FFA0" : "#FF5555");
				string text = Plugin.SelectedElement.ToUpper();
				string text2 = text.ToLower();
				if (1 == 0)
				{
				}
				string text3 = text2 switch
				{
					"burn" => "#f17728", 
					"poison" => "#952db8", 
					"bleed" => "#c94a46", 
					"shock" => "#b7a93f", 
					"root" => "#be927e", 
					"frost" => "#57cccc", 
					"chill" => "#57cccc", 
					"curse" => "#267b5b", 
					"minion" => "#f06ee9", 
					"summon" => "#f06ee9", 
					"bless" => "#d8f19c", 
					"fury" => "#64210f", 
					_ => "#ffffff", 
				};
				if (1 == 0)
				{
				}
				string hex = text3;
				string text4 = ColorText(text, hex, item6);
				((TMP_Text)_uiText).text = $"<color=#C090FF><b>MAGIC:</b> {item:F1}</color>\n" + $"<color=#FF8080><b>PHYSICAL:</b> {item2:F1}</color>\n" + text4 + "\n" + $"<color=#FFFF00><b>Crit:</b> {item3:F1}%//{item4:F1}%</color>\n" + $"<color={arg}><b>Safe:</b> {item5}</color>";
			}
			static string ColorText(string arg3, string arg2, float value)
			{
				return $"<color={arg2}><b>{arg3}:</b>{value:F1}</color>";
			}
		}

		public void UpdateVisibilityState()
		{
			if ((Object)(object)_canvasObj != (Object)null)
			{
				_canvasObj.SetActive(Plugin.ShowCombatInfo && Plugin._isVisible);
			}
		}

		private (float magicPower, float physicalPower, float CriticalChance, float CriticalDamage, bool isOutOfDanger, float element) FetchCombatData()
		{
			//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				StatsManager statsByActorID = StatsManager.GetStatsByActorID(Plugin.Instance.LocalPlayerActorID);
				if ((Object)(object)statsByActorID != (Object)null && statsByActorID.IsStatsInitialized && (Object)(object)((SimulationBehaviour)statsByActorID).Object != (Object)null && ((SimulationBehaviour)statsByActorID).Object.IsValid)
				{
					float magicPower = statsByActorID.MagicPower;
					float physicalPower = statsByActorID.PhysicalPower;
					float item = 0f;
					float item2 = 0f;
					if ((Object)(object)statsByActorID.Attack != (Object)null)
					{
						item = statsByActorID.Attack.CriticalChanceWithCap;
						item2 = statsByActorID.Attack.CriticalDamagePercentage;
					}
					else
					{
						Debug.LogWarning((object)$"[CombatInfo] stats.Attack is null for player {Plugin.Instance.LocalPlayerActorID}!");
					}
					bool item3 = true;
					if ((Object)(object)statsByActorID.RuntimeStats != (Object)null)
					{
						item3 = statsByActorID.RuntimeStats.IsOutOfDanger;
					}
					else
					{
						Debug.LogWarning((object)$"[CombatInfo] stats.RuntimeStats is null for player {Plugin.Instance.LocalPlayerActorID}!");
					}
					string selectedElement = Plugin.SelectedElement;
					if (1 == 0)
					{
					}
					float num;
					switch (selectedElement)
					{
					case "Shock":
						num = statsByActorID.Shock.DamageMultiplierPCT - 100f;
						break;
					case "Burn":
						num = statsByActorID.Burn.DamageMultiplierPCT - 100f;
						break;
					case "Chill":
						num = statsByActorID.Chill.DamageMultiplierAgainstFrozenTarget * 100f - 100f;
						break;
					case "Poison":
						num = statsByActorID.Poison.DamageMultiplierPCT - 100f;
						break;
					case "Bleed":
						num = statsByActorID.Bleed.DamageMultiplierPCT - 100f;
						break;
					case "Fury":
					{
						ChangeableDamageIncrement boostedAttackDamageIncPCT = statsByActorID.Fury.BoostedAttackDamageIncPCT;
						num = boostedAttackDamageIncPCT.Value;
						break;
					}
					case "Curse":
						num = statsByActorID.Curse.CurseDamageMultiplier * 100f - 100f;
						break;
					case "Summon":
						num = statsByActorID.Summon.MinionsDamageMultiplier.Multiplier * 100f - 100f;
						break;
					case "Bless":
						num = statsByActorID.Bless.EmpoweredAttackDamageIncrementForUI;
						break;
					default:
						num = 0f;
						break;
					}
					if (1 == 0)
					{
					}
					float item4 = num;
					return (magicPower: magicPower, physicalPower: physicalPower, CriticalChance: item, CriticalDamage: item2, isOutOfDanger: item3, element: item4);
				}
			}
			catch (Exception)
			{
				Debug.Log((object)$"[CombatInfo] Error fetching Magic Power for ActorID: {Plugin.Instance.LocalPlayerActorID}. Returning 0.");
			}
			return (magicPower: 0f, physicalPower: 0f, CriticalChance: 0f, CriticalDamage: 0f, isOutOfDanger: true, element: 0f);
		}
	}
	public class GroupDamageMeter : MonoBehaviour
	{
		private GameObject _canvasObj;

		private GameObject _panelObj;

		private TextMeshProUGUI _uiText;

		private float _updateTimer = 0f;

		private bool _lastPersistentUIVisibility = true;

		public static GroupDamageMeter Instance { get; private set; }

		private void Awake()
		{
			if ((Object)(object)Instance != (Object)null && (Object)(object)Instance != (Object)(object)this)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
				return;
			}
			Instance = this;
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
			Debug.Log((object)"[GroupDamageMeter] GroupDamageMeter initialized and set to persist across scenes.");
		}

		private void Start()
		{
			SetupHUD();
		}

		private void SetupHUD()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected Obj, but got Unknown
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected Obj, but got Unknown
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Expected Obj, but got Unknown
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Expected Obj, but got Unknown
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_0291: Unknown result type (might be due to invalid IL or missing references)
			//IL_029e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
			_canvasObj = new GameObject("Modded_Group_DPS_Root");
			Object.DontDestroyOnLoad((Object)(object)_canvasObj);
			_canvasObj.layer = 2;
			Canvas val = _canvasObj.AddComponent<Canvas>();
			val.renderMode = (RenderMode)0;
			val.overrideSorting = true;
			val.sortingOrder = 997;
			_panelObj = new GameObject("Modded_Group_Frame_Background");
			_panelObj.transform.SetParent(_canvasObj.transform, false);
			_panelObj.layer = 2;
			Image val2 = _panelObj.AddComponent<Image>();
			((Graphic)val2).color = new Color(0.45f, 0.35f, 0.2f, 1f);
			RectTransform rectTransform = ((Graphic)val2).rectTransform;
			rectTransform.anchorMin = new Vector2(1f, 0f);
			rectTransform.anchorMax = new Vector2(1f, 0f);
			rectTransform.pivot = new Vector2(1f, 0f);
			rectTransform.anchoredPosition = new Vector2(-10f, 210f);
			rectTransform.sizeDelta = new Vector2(190f, 120f);
			Outline val3 = _panelObj.AddComponent<Outline>();
			((Shadow)val3).effectColor = new Color(0.15f, 0.1f, 0.05f, 1f);
			((Shadow)val3).effectDistance = new Vector2(2f, -2f);
			GameObject val4 = new GameObject("Modded_Group_Inner_Area");
			val4.transform.SetParent(_panelObj.transform, false);
			val4.layer = 2;
			Image val5 = val4.AddComponent<Image>();
			((Graphic)val5).color = new Color(0f, 0f, 0f, 0.95f);
			RectTransform rectTransform2 = ((Graphic)val5).rectTransform;
			rectTransform2.anchorMin = Vector2.zero;
			rectTransform2.anchorMax = Vector2.one;
			rectTransform2.offsetMin = new Vector2(3f, 3f);
			rectTransform2.offsetMax = new Vector2(-3f, -3f);
			GameObject val6 = new GameObject("Modded_Group_Value_Text");
			val6.transform.SetParent(val4.transform, false);
			val6.layer = 2;
			_uiText = val6.AddComponent<TextMeshProUGUI>();
			((TMP_Text)_uiText).fontSize = 12f;
			((Graphic)_uiText).color = Color.white;
			((TMP_Text)_uiText).alignment = (TextAlignmentOptions)257;
			RectTransform rectTransform3 = ((TMP_Text)_uiText).rectTransform;
			rectTransform3.anchorMin = Vector2.zero;
			rectTransform3.anchorMax = Vector2.one;
			rectTransform3.sizeDelta = Vector2.zero;
			rectTransform3.offsetMin = new Vector2(5f, 5f);
			rectTransform3.offsetMax = new Vector2(-5f, -5f);
			TMP_FontAsset[] array = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
			TMP_FontAsset[] array2 = array;
			foreach (TMP_FontAsset val7 in array2)
			{
				if (((Object)val7).name.Contains("LiberationSans"))
				{
					((TMP_Text)_uiText).font = val7;
					break;
				}
			}
			UpdateVisibilityState();
		}

		private void Update()
		{
			bool isVisible = Plugin._isVisible;
			if (isVisible != _lastPersistentUIVisibility)
			{
				_lastPersistentUIVisibility = isVisible;
				UpdateVisibilityState();
			}
			if (!((Object)(object)_canvasObj == (Object)null) && !((Object)(object)_panelObj == (Object)null) && Plugin.Instance.go_timer)
			{
				_updateTimer += Time.deltaTime;
				if (_updateTimer >= 0.5f)
				{
					_updateTimer = 0f;
					UpdateGroupDamageDisplay();
				}
			}
		}

		private void UpdateGroupDamageDisplay()
		{
			PlayerManager instance = PlayerManager.Instance;
			if ((Object)(object)instance == (Object)null)
			{
				((TMP_Text)_uiText).text = "<color=#FF5555>PlayerManager Error</color>";
				return;
			}
			List<(string, float)> list = new List<(string, float)>();
			float num = 0f;
			for (int i = 0; i < 3; i++)
			{
				Player playerBySlot = instance.GetPlayerBySlot(i);
				if ((Object)(object)playerBySlot != (Object)null)
				{
					string item = (string.IsNullOrEmpty(playerBySlot.UserName) ? $"Player {i}" : playerBySlot.UserName);
					int num2 = i;
					float num3 = 0f;
					if (num2 >= 0 && Plugin.Instance.AllPlayerStats.ContainsKey(num2))
					{
						PlayerStats playerStats = Plugin.Instance.AllPlayerStats[num2];
						float num4 = Mathf.Max(0.1f, Plugin.Instance.ActiveCombatTime);
						num3 = playerStats.TotalDamage / num4;
						num += num3;
					}
					list.Add((item, num3));
				}
			}
			if (num > 0f)
			{
				for (int j = 0; j < list.Count; j++)
				{
					(string, float) tuple = list[j];
					string item2 = tuple.Item1;
					float item3 = tuple.Item2;
					list[j] = (item2, item3);
				}
			}
			float num5 = 0f;
			for (int k = 0; k < list.Count; k++)
			{
				if (list[k].Item2 > num5)
				{
					num5 = list[k].Item2;
				}
			}
			string text = $"<b><color=#A0FFA0>GROUP DPS: {num:F1}</color></b>\n";
			for (int l = 0; l < list.Count; l++)
			{
				var (arg, num6) = list[l];
				if (1 == 0)
				{
				}
				string text2 = l switch
				{
					0 => "#5F8FFF", 
					1 => "#5FFF5F", 
					2 => "#FF5F5F", 
					_ => "#CCCCCC", 
				};
				if (1 == 0)
				{
				}
				string text3 = text2;
				int num7 = 0;
				if (num5 > 0f)
				{
					float num8 = num6 / num5;
					num7 = (int)(num8 * 20f);
				}
				string arg2 = ((num7 > 0) ? ("<color=" + text3 + ">" + new string('█', num7) + "</color>") : "");
				text += $"{arg} [{num6:F1}]\n{arg2}\n";
			}
			((TMP_Text)_uiText).text = text;
		}

		public void UpdateVisibilityState()
		{
			if ((Object)(object)_canvasObj != (Object)null)
			{
				_canvasObj.SetActive(Plugin.ShowGroupDPS && Plugin._isVisible);
			}
		}
	}
}
