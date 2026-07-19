using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;
using BlackveilDpsMeter;
using RR.UI.Pages;
using RR.UI.UISystem; // Make sure to include this for PageArgs
using RaidersOfBlackveilMod;

public class ModSettingsPage : SettingsBasePage
{
    // Use existing settings page template to avoid missing VisualTreeAsset
    public ModSettingsPage(PageArgs args) : base("SettingsPage", args)
    {
        // The base class will handle setting the page Name using the string we passed above
    }

    // Note: SettingsBasePage does not expose an Initialize override publicly.
    // Provide a plain Initialize method in case external code calls it.
    public void Initialize(VisualElement root)
    {
        // Build your layout here!
        Label Title = new Label("Mod Configurations");
        Title.style.fontSize = 24;
        Title.style.color = Color.white;
        root.Add(Title);

        ScrollView settingsContainer = new ScrollView();
        root.Add(settingsContainer);
    }

    public override bool AcceptDefaultCall => true;

    public override void DefaultCall()
    {
        Debug.Log("Resetting all Mod Settings to Default!");
    }

    public new bool ChangesPending => false;

    public override void ApplyChanges(System.Action onFinished)
    {
        onFinished?.Invoke();
    }
}


[HarmonyPatch(typeof(SettingsPage), MethodType.Constructor, new Type[] { typeof(PageArgs) })]
public static class SettingsPage_Constructor_Patch
{
    // Store a static reference to our created dynamic page layer
    public static UIDynamicPageLayer<ModSettingsPage> ModSettingsPageLayer;

    [HarmonyPostfix]
    public static void Postfix(SettingsPage __instance)
    {
        try
        {
            VisualElement categoryButtonsContainer = __instance.RootElement.Q<VisualElement>("CategoryButtons");
            if (categoryButtonsContainer == null) return;

            VisualElement originalButton = categoryButtonsContainer.Q<VisualElement>("CatGeneral");
            if (originalButton == null) return;

            VisualElement generalContent = __instance.RootElement.Q<VisualElement>("ScrollContent");
            if (generalContent == null)
            {
                var firstScroll = __instance.RootElement.Query<ScrollView>().First();
                generalContent = firstScroll as VisualElement ?? __instance.RootElement;
            }

            VisualElement modsSection = new VisualElement();
            modsSection.name = "ModsSection";
            modsSection.style.marginTop = 8;

            Label modsHeader = new Label("Mods");
            modsHeader.style.fontSize = 24;
            modsHeader.style.color = Color.yellow;
            modsHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            modsHeader.style.marginLeft = 6;
            modsHeader.style.marginBottom = 8;
            modsSection.Add(modsHeader);

            ScrollView modsContent = new ScrollView();
            modsContent.name = "ModsContent";
            modsContent.style.flexGrow = 1;
            modsContent.style.marginTop = 6;

            // --- Toggle: Enable DPS Meter ---
            Toggle dpsToggle = new Toggle("Enable DPS Meter");
            dpsToggle.value = Plugin.ShowDPSMeter;
            dpsToggle.style.marginLeft = 6;
            dpsToggle.style.marginTop = 4;
            var dpsLabel = dpsToggle.Q<Label>();
            if (dpsLabel != null)
            {
                dpsLabel.style.fontSize = 18;
                dpsLabel.style.color = Color.white;
            }
            dpsToggle.RegisterValueChangedCallback(evt =>
            {
                Plugin.ShowDPSMeter = evt.newValue; 
                PersistentUI._isVisible = evt.newValue;
                if (PersistentUI._targetCanvas != null)
                    PersistentUI._targetCanvas.enabled = evt.newValue;
            });
            modsContent.Add(dpsToggle);

            // --- Toggle: Combat Info ---
            Toggle combatInfoToggle = new Toggle("Combat Info");
            combatInfoToggle.value = Plugin.ShowCombatInfo;
            combatInfoToggle.style.marginLeft = 6;
            combatInfoToggle.style.marginTop = 4;
            var combatInfoLabel = combatInfoToggle.Q<Label>();
            if (combatInfoLabel != null)
            {
                combatInfoLabel.style.fontSize = 18;
                combatInfoLabel.style.color = Color.white;
            }
            combatInfoToggle.RegisterValueChangedCallback(evt =>
            {
                Plugin.ShowCombatInfo = evt.newValue;
                if (CombatInfoModule.Instance != null)
                {
                    CombatInfoModule.Instance.UpdateVisibilityState();
                }
                Debug.Log($"[ModSettings] Combat Info Toggle changed to: {evt.newValue}");
            });
            modsContent.Add(combatInfoToggle);

            // --- Toggle: Group DPS ---
            Toggle groupDpsToggle = new Toggle("Group DPS");
            groupDpsToggle.value = Plugin.ShowGroupDPS;
            groupDpsToggle.style.marginLeft = 6;
            groupDpsToggle.style.marginTop = 4;
            var groupDpsLabel = groupDpsToggle.Q<Label>();
            if (groupDpsLabel != null)
            {
                groupDpsLabel.style.fontSize = 18;
                groupDpsLabel.style.color = Color.white;
            }
            groupDpsToggle.RegisterValueChangedCallback(evt =>
            {
                Plugin.ShowGroupDPS = evt.newValue;
                if (GroupDamageMeter.Instance != null)
                {
                    GroupDamageMeter.Instance.UpdateVisibilityState();
                }
                Debug.Log($"[ModSettings] Group DPS Toggle changed to: {evt.newValue}");
            });
            modsContent.Add(groupDpsToggle);

            // --- Dropdown: Single Choice Menu ---
            List<string> modeChoices = new List<string> { "", "Shock", "Burn", "Chill", "Poison", "Bleed", "Fury", "Curse", "Summon" , "Bless" }; // Add more elements as needed
            
            // NOTE: Replace "Plugin.SelectedElement" with your actual backing config field/property!
            DropdownField modeDropdown = new DropdownField("Select Mode", modeChoices, Plugin.SelectedElement ?? "");
            modeDropdown.style.marginLeft = 6;
            modeDropdown.style.marginTop = 8;
            modeDropdown.style.width = 300; // Gives it clean structure inside UI content

            var dropdownLabel = modeDropdown.Q<Label>();
            if (dropdownLabel != null)
            {
                dropdownLabel.style.fontSize = 18;
                dropdownLabel.style.color = Color.white;
            }
            
            modeDropdown.RegisterValueChangedCallback(evt =>
            {
                Plugin.SelectedElement = evt.newValue;
                Debug.Log($"[ModSettings] Selected Element changed to: {evt.newValue}");
                
                // If you need to trigger any logic immediately when changed, do it here.
            });
            modsContent.Add(modeDropdown);

            modsSection.Add(modsContent);
            generalContent.Add(modsSection);

            Debug.Log("[ModSettings] Successfully patched Mod Settings page into settings menu!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ModSettings] Failed to patch SettingsPage: {ex}");
        }
    }
}