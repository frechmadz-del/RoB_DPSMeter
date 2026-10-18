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

    public enum RarityThreshold
            {
                Uncommon = 1,
                Rare = 2,
                Epic = 3,
                Legendary = 4,
                Mythic = 5
            }

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
    private const float SettingsControlWidth = 500f;
    private const float SettingsLabelWidth = 350f;

    private static void ApplyCommonControlStyle(VisualElement element)
    {
        element.style.marginLeft = 6;
        element.style.marginTop = 6;
        element.style.width = SettingsControlWidth;
    }

    private static void ApplyLabelStyle(VisualElement control, bool isToggle = false)
    {
        var label = control.Q<Label>();
        if (label == null)
            return;

        label.style.fontSize = 18;
        label.style.color = Color.white;
        label.style.unityFontStyleAndWeight = FontStyle.Normal;
        label.style.marginLeft = 0;
        label.style.paddingLeft = 0;

        if (isToggle)
        {
            label.style.minWidth = SettingsLabelWidth;
            label.style.width = SettingsLabelWidth;
        }
        else
        {
            label.style.minWidth = SettingsLabelWidth;
            label.style.width = SettingsLabelWidth;
        }
    }

    private static void ApplyDropdownStyle(DropdownField dropdown)
    {
        ApplyCommonControlStyle(dropdown);
        dropdown.style.flexDirection = FlexDirection.Row;
        dropdown.style.justifyContent = Justify.SpaceBetween;
        dropdown.style.alignItems = Align.Center;
        dropdown.style.height = 32;
        ApplyLabelStyle(dropdown);

        var textField = dropdown.Q<TextField>();
        if (textField != null)
        {
            textField.style.minWidth = 120;
            textField.style.flexGrow = 1;
            textField.style.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.85f);
            textField.style.color = new Color(0.12f, 0.12f, 0.12f, 1f);
        }
    }

    private static void ApplyToggleStyle(Toggle toggle)
    {
        ApplyCommonControlStyle(toggle);
        toggle.style.flexDirection = FlexDirection.Row;
        toggle.style.justifyContent = Justify.SpaceBetween;
        toggle.style.alignItems = Align.Center;
        toggle.style.height = 32;
        ApplyLabelStyle(toggle, true);

        VisualElement checkmark = toggle.Q<VisualElement>("unity-checkmark");
        if (checkmark != null)
        {
            checkmark.style.marginLeft = 8;
            checkmark.style.marginRight = 0;
        }
    }

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
            modsSection.style.marginTop = 25;

            Label modsHeader = new Label("DPS Meter Mod Settings");
            modsHeader.style.fontSize = 24;
            modsHeader.style.color = new Color(0.702f, 0.639f, 0.471f, 1.0f);
            modsHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            modsHeader.style.marginLeft = 6;
            modsHeader.style.marginBottom = 8;
            modsSection.Add(modsHeader);

            ScrollView modsContent = new ScrollView();
            modsContent.name = "ModsContent";
            modsContent.style.flexGrow = 1;
            modsContent.style.marginTop = 6;

            // --- Toggle: Enable DPS Meter ---
            Toggle dpsToggle = new Toggle("DPS Meter");
            dpsToggle.value = Plugin.ShowDPSMeter;
            ApplyToggleStyle(dpsToggle);
            dpsToggle.RegisterValueChangedCallback(evt =>
            {
                Plugin.ShowDPSMeter = evt.newValue; 
                if (PersistentUI._targetCanvas != null)
                    PersistentUI._targetCanvas.enabled = evt.newValue;
            });
            modsContent.Add(dpsToggle);

            // --- Toggle: Combat Info ---
            Toggle combatInfoToggle = new Toggle("Combat Info");
            combatInfoToggle.value = Plugin.ShowCombatInfo;
            ApplyToggleStyle(combatInfoToggle);
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

            // --- Dropdown: Single Choice Menu ---
            List<string> modeChoices_element = new List<string> { "", "Shock", "Burn", "Chill", "Poison", "Bleed", "Fury", "Curse", "Summon" , "Bless" }; // Add more elements as needed
            
            // NOTE: Replace "Plugin.SelectedElement" with your actual backing config field/property!
            DropdownField modeDropdown = new DropdownField("Select element for Combat info tracking", modeChoices_element, Plugin.SelectedElement ?? "");
            ApplyDropdownStyle(modeDropdown);
            
            modeDropdown.RegisterValueChangedCallback(evt =>
            {
                Plugin.SelectedElement = evt.newValue;
                Debug.Log($"[ModSettings] Selected Element changed to: {evt.newValue}");
                
                // If you need to trigger any logic immediately when changed, do it here.
            });
            modsContent.Add(modeDropdown);

            // --- Toggle: Group DPS ---
            Toggle groupDpsToggle = new Toggle("Group DPS");
            groupDpsToggle.value = Plugin.ShowGroupDPS;
            ApplyToggleStyle(groupDpsToggle);
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

            VisualElement modsSection_Loot = new VisualElement();
            modsSection_Loot.name = "ModsSection_Loot";
            modsSection_Loot.style.marginTop = 25;

            Label modsHeader_loot = new Label("Lootfilter Mod Settings");
            modsHeader_loot.style.fontSize = 24;
            modsHeader_loot.style.color = new Color(0.702f, 0.639f, 0.471f, 1.0f);
            modsHeader_loot.style.unityFontStyleAndWeight = FontStyle.Bold;
            modsHeader_loot.style.marginLeft = 6;
            modsHeader_loot.style.marginBottom = 8;
            modsSection_Loot.Add(modsHeader_loot);

            ScrollView modsContent_Loot = new ScrollView();
            modsContent_Loot.name = "ModsContent";
            modsContent_Loot.style.flexGrow = 1;
            modsContent_Loot.style.marginTop = 6;

            List<string> Choices_rarity_mode = new List<string> { "off", "equal and above"}; 
            
            // NOTE: Replace "Plugin.SelectedElement" with your actual backing config field/property!
            DropdownField Dropdown_rarity_mode = new DropdownField("Lootfilter", Choices_rarity_mode, Plugin.SelectedRarityMode ?? "off");
            ApplyDropdownStyle(Dropdown_rarity_mode);
            
            Dropdown_rarity_mode.RegisterValueChangedCallback(evt =>
            {
                Plugin.SelectedRarityMode = evt.newValue;
                Debug.Log($"[ModSettings] Selected Element changed to: {evt.newValue}");
                
                // If you need to trigger any logic immediately when changed, do it here.
            });
            modsContent_Loot.Add(Dropdown_rarity_mode);

            List<string> pickupModeChoices = new List<string> { "equipment", "item", "both" };
            DropdownField pickupModeDropdown = new DropdownField("Pickup labels", pickupModeChoices, Plugin.SelectedPickupMode);
            ApplyDropdownStyle(pickupModeDropdown);

            pickupModeDropdown.RegisterValueChangedCallback(evt =>
            {
                Plugin.SelectedPickupMode = evt.newValue;
                Debug.Log($"[ModSettings] Pickup mode changed to: {evt.newValue}");
            });
            modsContent_Loot.Add(pickupModeDropdown);

            foreach (string itemTypeKey in Plugin.GetHighlightItemTypeOptions())
            {
                Toggle itemHighlightToggle = new Toggle($"Highlight {Plugin.GetHighlightItemTypeDisplayName(itemTypeKey)}");
                itemHighlightToggle.value = Plugin.IsHighlightTypeEnabled(itemTypeKey);
                ApplyToggleStyle(itemHighlightToggle);

                itemHighlightToggle.RegisterValueChangedCallback(evt =>
                {
                    Plugin.SetHighlightTypeEnabled(itemTypeKey, evt.newValue);
                    Debug.Log($"[ModSettings] Highlight type '{itemTypeKey}' changed to: {evt.newValue}");
                });

                modsContent_Loot.Add(itemHighlightToggle);
            }

            List<string> modeChoices_rarity = new List<string>(Enum.GetNames(typeof(ModSettingsPage.RarityThreshold)));

            string equipmentInitialValue = Enum.GetName(typeof(ModSettingsPage.RarityThreshold), (ModSettingsPage.RarityThreshold)Plugin.MinEquipmentRarityThreshold)
                                            ?? ModSettingsPage.RarityThreshold.Epic.ToString();

            DropdownField equipmentRarityDropdown = new DropdownField("Equipment min rarity", modeChoices_rarity, equipmentInitialValue);
            ApplyDropdownStyle(equipmentRarityDropdown);

            equipmentRarityDropdown.RegisterValueChangedCallback(evt =>
            {
                if (Enum.TryParse(evt.newValue, out ModSettingsPage.RarityThreshold selectedRarity))
                {
                    int rarityIntValue = (int)selectedRarity;
                    Plugin.MinEquipmentRarityThreshold = rarityIntValue;
                    Debug.Log($"[ModSettings] Equipment rarity threshold changed to: {selectedRarity} (int: {Plugin.MinEquipmentRarityThreshold})");
                }
            });
            modsContent_Loot.Add(equipmentRarityDropdown);

            string itemInitialValue = Enum.GetName(typeof(ModSettingsPage.RarityThreshold), (ModSettingsPage.RarityThreshold)Plugin.MinItemRarityThreshold)
                                    ?? ModSettingsPage.RarityThreshold.Epic.ToString();

            DropdownField itemRarityDropdown = new DropdownField("Item min rarity", modeChoices_rarity, itemInitialValue);
            ApplyDropdownStyle(itemRarityDropdown);

            itemRarityDropdown.RegisterValueChangedCallback(evt =>
            {
                if (Enum.TryParse(evt.newValue, out ModSettingsPage.RarityThreshold selectedRarity))
                {
                    int rarityIntValue = (int)selectedRarity;
                    Plugin.MinItemRarityThreshold = rarityIntValue;
                    Debug.Log($"[ModSettings] Item rarity threshold changed to: {selectedRarity} (int: {Plugin.MinItemRarityThreshold})");
                }
            });
            modsContent_Loot.Add(itemRarityDropdown);

            // DPSMeter Mod Settings section
            modsSection.Add(modsContent);
            generalContent.Add(modsSection);

            // Lootfilter Mod Settings section
            modsSection_Loot.Add(modsContent_Loot);
            generalContent.Add(modsSection_Loot);

            Debug.Log("[ModSettings] Successfully patched Mod Settings page into settings menu!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ModSettings] Failed to patch SettingsPage: {ex}");
        }
    }
}