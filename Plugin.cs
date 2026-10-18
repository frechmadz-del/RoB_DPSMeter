using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono; // <--- ADD THIS FOR BEPINEX 6
using System.IO;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using RR.Game;
using RR.Game.Stats;
using RR.Game.Damage;
using RR.Game.Perk;
using RR.Utility;
using RR.Level;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Newtonsoft.Json;
using RR.Game.Character;
using RR.Game.Input;
using System.Reflection;
using RR.UI.Controls.Inventory;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;
using RR;
using Fusion;
using System.Linq;
using TMPro;
using JetBrains.Annotations;
using RR.Game.Items;
using BepInEx.Configuration;
using RaidersOfBlackveilMod;

namespace BlackveilDpsMeter
{
    [Serializable]
    public sealed class GraphHistoryData
    {
        public int Version = 1;
        public StageDpsHistoryData[] StageRecords = Array.Empty<StageDpsHistoryData>();
        public CombatStatsHistoryData[] CombatStatsRecords = Array.Empty<CombatStatsHistoryData>();
    }

    [Serializable]
    public sealed class StageDpsHistoryData
    {
        public int StageNumber;
        public string SceneName;
        public float TotalDamage;
        public float CombatTime;
        public float[] DamageByType;
        public string[] CollectedPerks;
        public int[] CollectedPerkIds;
        public int[] PerkUpgradeLevels;
        public bool[] PerkTalents;
        public bool[] MasterTalents;
    }

    [Serializable]
    public sealed class CombatStatsHistoryData
    {
        public int StageNumber;
        public string SceneName;
        public string SelectedElement;
        public float[] Values;
        public float[] MaximumValues;
    }

    [BepInPlugin("vibecoded_mod_1", "RoB_QoL_Mod", "1.0.0")]
    public class Plugin : BepInEx.Unity.Mono.BaseUnityPlugin
    {
        public static Plugin Instance;
        public PlayerStats LocalPlayerStats = new PlayerStats();
        public Dictionary<int, PlayerStats> AllPlayerStats = new Dictionary<int, PlayerStats>(); // Track ALL players' damage
        public float ActiveCombatTime = 0f; // Tracks accumulated combat seconds
        public float StartTime = -1f;
        public float LastHitTime = -1f; // out of fight timer
        public readonly List<StageDpsRecord> StageDpsRecords = new List<StageDpsRecord>();
        public readonly List<CombatStatsRecord> CombatStatsRecords = new List<CombatStatsRecord>();
        private string _currentStageSceneName;
        private ulong _currentStageSceneHandle;
        private float _currentStageCombatTime;
        private readonly float[] _currentStageMaximumCombatStats = new float[13];
        private bool _hasCurrentStageCombatStats;
        private float _nextCombatStatsSampleTime;
        private readonly List<string> _currentStagePerks = new List<string>();
        private readonly List<int> _currentStagePerkIds = new List<int>();
        private readonly List<int> _currentStagePerkUpgradeLevels = new List<int>();
        private readonly List<bool> _currentStagePerkTalents = new List<bool>();
        private readonly List<bool> _currentStageMasterTalents = new List<bool>();
        private readonly HashSet<int> _stageKnownPerkIds = new HashSet<int>();
        private readonly HashSet<int> _stageBaselinePerkIds = new HashSet<int>();
        private readonly HashSet<string> _stageRecordedTalentKeys = new HashSet<string>();
        private readonly Dictionary<int, int> _stageBaselinePerkUpgradeLevels = new Dictionary<int, int>();
        private readonly HashSet<int> _runRecordedPerkIds = new HashSet<int>();
        private readonly HashSet<string> _legacyRunRecordedPerkNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private PerkHandler _localPerkHandler;
        private string _pendingStageSceneName;
        private ulong _pendingStageSceneHandle;
        private bool _lobbySceneLoaded;
        public int LocalPlayerActorID = -1; // Captured when joining lobby, used to filter damage logs in multiplayer
        private static bool _instanceExists = false;
        public bool fetchingActorID = false; // Debug toggle to enable ActorID fetching logs in the Update loop
        public bool go_timer = false; // Debug toggle to enable combat timer logs in the Update loop
        public static bool _isVisible = true;
        private GameObject _uiBusHost;
        private bool _uiBusReady;
        
        public static ConfigEntry<bool> ShowDPSMeterConfig;
        public static ConfigEntry<bool> ShowGroupDPSConfig;
        public static ConfigEntry<bool> ShowCombatInfoConfig;
        public static ConfigEntry<string> SelectedElementConfig;
        public static ConfigEntry<int> MinRarityThresholdConfig; // Legacy shared threshold for older configs
        public static ConfigEntry<int> MinEquipmentRarityThresholdConfig;
        public static ConfigEntry<int> MinItemRarityThresholdConfig;
        public static ConfigEntry<bool> HighlightCurrencyConfig;
        public static ConfigEntry<string> HighlightedItemTypeConfig;
        public static ConfigEntry<string> SelectedRarityModeConfig; // Rarity mode for displayed equipment
        public static ConfigEntry<string> SelectedPickupModeConfig; // Pickup type mode: equipment, item, or both

        public static readonly List<string> HighlightItemTypeOptions = new List<string>
        {
            "currency",
            "transmutationstone",
            "raritystone"
        };

        public static bool ShowDPSMeter
        {
            get => ShowDPSMeterConfig?.Value ?? false;
            set
            {
                if (ShowDPSMeterConfig != null)
                {
                    ShowDPSMeterConfig.Value = value;
                    // Force BepInEx to save the config file to disk immediately
                    ShowDPSMeterConfig.ConfigFile.Save(); 
                }

            }
        }

        public static bool ShowGroupDPS
        {
            get => ShowGroupDPSConfig?.Value ?? false;
            set
            {
                if (ShowGroupDPSConfig != null)
                {
                    ShowGroupDPSConfig.Value = value;
                    ShowGroupDPSConfig.ConfigFile.Save();
                }
            }
        }

        public static bool ShowCombatInfo
        {
            get => ShowCombatInfoConfig?.Value ?? false;
            set
            {
                if (ShowCombatInfoConfig != null)
                {
                    ShowCombatInfoConfig.Value = value;
                    ShowCombatInfoConfig.ConfigFile.Save();
                }
            }
        }

    // 2. Create a clean public property for your UI to read and write to
        public static string SelectedElement
        {
            get => SelectedElementConfig?.Value ?? ""; // Default to empty string if config is null
            set
            {
                if (SelectedElementConfig != null)
                {
                    SelectedElementConfig.Value = value;
                    SelectedElementConfig.ConfigFile.Save();
                }
            }
        }

        // Legacy compatibility alias for the equipment threshold.
        public static int MinRarityThreshold
        {
            get => MinEquipmentRarityThresholdConfig?.Value ?? MinRarityThresholdConfig?.Value ?? 2;
            set
            {
                if (MinEquipmentRarityThresholdConfig != null)
                {
                    MinEquipmentRarityThresholdConfig.Value = value;
                    MinEquipmentRarityThresholdConfig.ConfigFile.Save();
                }

                if (MinRarityThresholdConfig != null)
                {
                    MinRarityThresholdConfig.Value = value;
                    MinRarityThresholdConfig.ConfigFile.Save();
                }
            }
        }

        public static int MinEquipmentRarityThreshold
        {
            get => MinEquipmentRarityThresholdConfig?.Value ?? MinRarityThresholdConfig?.Value ?? 2;
            set
            {
                if (MinEquipmentRarityThresholdConfig != null)
                {
                    MinEquipmentRarityThresholdConfig.Value = value;
                    MinEquipmentRarityThresholdConfig.ConfigFile.Save();
                }

                if (MinRarityThresholdConfig != null)
                {
                    MinRarityThresholdConfig.Value = value;
                    MinRarityThresholdConfig.ConfigFile.Save();
                }
            }
        }

        public static int MinItemRarityThreshold
        {
            get => MinItemRarityThresholdConfig?.Value ?? MinEquipmentRarityThresholdConfig?.Value ?? MinRarityThresholdConfig?.Value ?? 2;
            set
            {
                if (MinItemRarityThresholdConfig != null)
                {
                    MinItemRarityThresholdConfig.Value = value;
                    MinItemRarityThresholdConfig.ConfigFile.Save();
                }
            }
        }

        public static string SelectedRarityMode
        {
            get => SelectedRarityModeConfig?.Value ?? "off"; // Default to "off"
            set
            {
                if (SelectedRarityModeConfig != null)
                {
                    SelectedRarityModeConfig.Value = value;
                    SelectedRarityModeConfig.ConfigFile.Save();
                }
            }
        }

        public static bool HighlightCurrency
        {
            get => IsHighlightTypeEnabled("currency");
            set
            {
                SetHighlightTypeEnabled("currency", value);
                if (HighlightCurrencyConfig != null)
                {
                    HighlightCurrencyConfig.Value = value;
                    HighlightCurrencyConfig.ConfigFile.Save();
                }
            }
        }

        public static bool HighlightTransmutationStone
        {
            get => IsHighlightTypeEnabled("transmutationstone");
            set => SetHighlightTypeEnabled("transmutationstone", value);
        }

        public static bool HighlightRarityStone
        {
            get => IsHighlightTypeEnabled("raritystone");
            set => SetHighlightTypeEnabled("raritystone", value);
        }

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

            switch (itemTypeKey.Trim().ToLowerInvariant())
            {
                case "currency":
                    return "Currency";
                case "transmutationstone":
                    return "Control Cube and Chaos Prism";
                case "raritystone":
                    return "Artificer Orb";
                default:
                    return itemTypeKey;
            }
        }

        public static bool IsHighlightTypeEnabled(string itemTypeKey)
        {
            return GetEnabledHighlightTypes().Contains(NormalizeHighlightType(itemTypeKey));
        }

        public static void SetHighlightTypeEnabled(string itemTypeKey, bool enabled)
        {
            string normalized = NormalizeHighlightType(itemTypeKey);
            if (string.IsNullOrEmpty(normalized))
            {
                return;
            }

            HashSet<string> enabledTypes = GetEnabledHighlightTypes();
            if (enabled)
            {
                enabledTypes.Add(normalized);
            }
            else
            {
                enabledTypes.Remove(normalized);
            }

            if (HighlightedItemTypeConfig != null)
            {
                HighlightedItemTypeConfig.Value = string.Join(",", HighlightItemTypeOptions.Where(type => enabledTypes.Contains(type)).ToList());
                HighlightedItemTypeConfig.ConfigFile.Save();
            }
        }

        public static bool ShouldHighlightItemType(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.BlackCoin:
                    return HighlightCurrency;
                case ItemType.BlackBlood:
                    return HighlightCurrency;
                case ItemType.Glitter:
                    return HighlightCurrency;
                case ItemType.Scrap:
                    return HighlightCurrency;
                case ItemType.TransmutationStone:
                    return HighlightTransmutationStone;
                case ItemType.RarityStone:
                    return HighlightRarityStone;
                default:
                    return true; // Default to highlighting other item types
            }
        }

        private static HashSet<string> GetEnabledHighlightTypes()
        {
            var enabledTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (HighlightedItemTypeConfig != null)
            {
                foreach (string itemType in HighlightedItemTypeConfig.Value.Split(',', ';'))
                {
                    string trimmed = itemType?.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                    {
                        continue;
                    }

                    string normalized = NormalizeHighlightType(trimmed);
                    if (!string.IsNullOrEmpty(normalized))
                    {
                        enabledTypes.Add(normalized);
                    }
                }
            }

            if (enabledTypes.Count == 0 && HighlightCurrencyConfig != null)
            {
                if (HighlightCurrencyConfig.Value)
                {
                    enabledTypes.Add("currency");
                }
            }

            if (enabledTypes.Count == 0)
            {
                foreach (string defaultType in HighlightItemTypeOptions)
                {
                    enabledTypes.Add(defaultType);
                }
            }

            return enabledTypes;
        }

        private static string NormalizeHighlightType(string itemTypeKey)
        {
            if (string.IsNullOrWhiteSpace(itemTypeKey))
            {
                return string.Empty;
            }

            string normalized = itemTypeKey.Trim();
            switch (normalized.ToLowerInvariant())
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
                    return normalized.ToLowerInvariant();
            }
        }

        public static string SelectedPickupMode
        {
            get => SelectedPickupModeConfig?.Value ?? "both";
            set
            {
                if (SelectedPickupModeConfig != null)
                {
                    string normalized = value?.Trim().ToLowerInvariant();
                    if (normalized != "equipment" && normalized != "item" && normalized != "both")
                    {
                        normalized = "both";
                    }

                    SelectedPickupModeConfig.Value = normalized;
                    SelectedPickupModeConfig.ConfigFile.Save();
                }
            }
        }

        public static bool ShouldShowEquipmentLabels => SelectedPickupMode == "equipment" || SelectedPickupMode == "both";
        public static bool ShouldShowItemLabels => SelectedPickupMode == "item" || SelectedPickupMode == "both";

        void Awake()
        {
            // 1. Check for duplicates immediately
            if (_instanceExists && Instance != this)
            {
                Logger.LogWarning("Duplicate plugin instance detected. Destroying duplicate.");
                Destroy(this.gameObject);
                return; // STOP everything right here. Do not patch, do not subscribe.
            }

            // 2. Set up the legitimate singleton instance
            Instance = this;
            _instanceExists = true;

            // Ensure this plugin GameObject stays active across scene transitions
            DontDestroyOnLoad(gameObject);

            // 3. Bind persistent config entries
            ShowDPSMeterConfig = Config.Bind("General", "ShowDPSMeter", false, "Show the DPS meter overlay.");
            ShowGroupDPSConfig = Config.Bind("General", "ShowGroupDPS", false, "Show the group DPS display.");
            ShowCombatInfoConfig = Config.Bind("General", "ShowCombatInfo", false, "Show additional combat information.");
            SelectedElementConfig = Config.Bind("General", "SelectedElement", "", "The currently selected element for combat information.");
            MinRarityThresholdConfig = Config.Bind("Equipment", "MinRarityThreshold", 2, "Legacy shared rarity threshold for displayed labels.");
            MinEquipmentRarityThresholdConfig = Config.Bind("Equipment", "MinEquipmentRarityThreshold", 2, "Minimum rarity level for displayed equipment.");
            MinItemRarityThresholdConfig = Config.Bind("Equipment", "MinItemRarityThreshold", 2, "Minimum rarity level for displayed items.");
            HighlightCurrencyConfig = Config.Bind("Equipment", "HighlightCurrency", true, "Legacy compatibility toggle for highlighting currency items.");
            HighlightedItemTypeConfig = Config.Bind("Equipment", "HighlightedItemTypes", "currency,transmutationstone,raritystone", "Comma-separated item categories to highlight in the loot filter. Supported values: currency, transmutationstone, raritystone.");
            SelectedRarityModeConfig = Config.Bind("Equipment", "RarityMode", "off", "Rarity mode for displayed equipment");
            SelectedPickupModeConfig = Config.Bind("Equipment", "PickupTypeMode", "both", "Which world pickup labels to show: equipment, item, or both.");

            LoadGraphHistory();

            if (string.IsNullOrWhiteSpace(HighlightedItemTypeConfig.Value))
            {
                HighlightedItemTypeConfig.Value = string.Join(",", HighlightItemTypeOptions);
                HighlightedItemTypeConfig.ConfigFile.Save();
            }
            else
            {
                HashSet<string> normalizedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string itemType in HighlightedItemTypeConfig.Value.Split(',', ';'))
                {
                    string trimmed = itemType?.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                    {
                        continue;
                    }

                    string normalized = NormalizeHighlightType(trimmed);
                    if (!string.IsNullOrEmpty(normalized))
                    {
                        normalizedTypes.Add(normalized);
                    }
                }

                if (normalizedTypes.Count == 0)
                {
                    HighlightedItemTypeConfig.Value = string.Join(",", HighlightItemTypeOptions);
                    HighlightedItemTypeConfig.ConfigFile.Save();
                }
            }

            // 4. Run initialization ONCE and ONLY once
            var harmony = new Harmony("com.gemini.dpsmeter");
            harmony.PatchAll();
            HealthDamageLogPatch.Apply(harmony);
            PermanentItemLabelPatch.Apply(harmony);

            // Unity 6: do not attach multiple MonoBehaviours while the plugin object is
            // still booting. The engine can hard-crash on AddComponent during Awake.
            // Delay the bus creation until after an actual scene is loaded.
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            Logger.LogInfo("Mod Injected: Hidden Bus & Harmony Patches Active.");
        }

        private IEnumerator InitializeUiBusAfterSceneLoad()
        {
            yield return new WaitForSecondsRealtime(0.5f);

            if (_uiBusReady)
                yield break;

            if (_uiBusHost == null)
            {
                _uiBusHost = new GameObject("DPS_Global_Bus");
                _uiBusHost.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(_uiBusHost);
            }

            if (_uiBusHost.GetComponent<PersistentUI>() == null)
                _uiBusHost.AddComponent<PersistentUI>();

            if (_uiBusHost.GetComponent<CombatInfoModule>() == null)
                _uiBusHost.AddComponent<CombatInfoModule>();

            if (_uiBusHost.GetComponent<GroupDamageMeter>() == null)
                _uiBusHost.AddComponent<GroupDamageMeter>();

            if (_uiBusHost.GetComponent<HotkeyRunner>() == null)
                _uiBusHost.AddComponent<HotkeyRunner>();

            if (_uiBusHost.GetComponent<GraphDashboardWindow>() == null)
                _uiBusHost.AddComponent<GraphDashboardWindow>();

            _uiBusReady = true;
        }

        private class HotkeyRunner : MonoBehaviour
        {   
            private bool _isKeyHeld = false; // Our custom debounce
            private void Update()
            {   
                var keyboard = UnityEngine.InputSystem.Keyboard.current;
                if (keyboard == null)
                {
                    return;
                }

                bool ui_pressed = keyboard.pKey.isPressed;
                bool controlPressed = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;

                if (keyboard.oKey.isPressed)
                {
                    Plugin.Instance?.ResetMeter();
                }
                
                if (ui_pressed && !_isKeyHeld )
                {   
                    _isKeyHeld = true;

                    if (controlPressed)
                    {
                        Debug.Log("[DPS] Ctrl+P pressed: Toggling graph dashboard");
                        GetComponent<GraphDashboardWindow>()?.ToggleVisibility();
                    }
                    else
                    {
                        Debug.Log("[DPS] P Pressed: Toggling UI Visibility");
                        Debug.Log($"[DPS] Current Visibility: {Plugin._isVisible} | Toggling to: {!Plugin._isVisible} | settings ShowDPSMeter: {Plugin.ShowDPSMeter}");
                        Plugin.Instance?.ToggleUIVisibility();
                    }
                }
                else if (!ui_pressed)
                {
                    _isKeyHeld = false; // Unlock when user lets go of P
                }
            }
        }
        public void ToggleUIVisibility()
        {
            _isVisible = !_isVisible;
            
            
            // Physical feedback in the editor console
            Debug.Log($"[UI] Visibility set to: {_isVisible}");
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
            
            // Reset all group player stats
            foreach (var kvp in AllPlayerStats)
            {
                kvp.Value.Reset();
            }
            AllPlayerStats.Clear();
            
            ActiveCombatTime = 0.11f;
            StartTime = -1f;
            LastHitTime = -1f;
        }

        private void Update()
        {
            if (!string.IsNullOrEmpty(_currentStageSceneName) && LocalPlayerActorID != -1)
            {
                TryBindLocalPerkHandler();
            }
        }

        private void TryBindLocalPerkHandler()
        {
            if (_localPerkHandler != null &&
                _localPerkHandler.Stats != null &&
                _localPerkHandler.Stats.ActorID == LocalPlayerActorID)
            {
                return;
            }

            UnbindLocalPerkHandler();
            PerkHandler[] handlers = Resources.FindObjectsOfTypeAll<PerkHandler>();
            for (int i = 0; i < handlers.Length; i++)
            {
                PerkHandler handler = handlers[i];
                if (handler == null ||
                    !handler.gameObject.scene.IsValid() ||
                    handler.Stats == null ||
                    handler.Stats.ActorID != LocalPlayerActorID)
                {
                    continue;
                }

                _localPerkHandler = handler;
                _localPerkHandler.OnPerkCollected += HandleLocalPerkCollected;
                SeedLocalPerkBaseline(_localPerkHandler);
                Logger.LogInfo($"[Perks] Tracking perk pickups for local player {LocalPlayerActorID}.");
                return;
            }
        }

        private void SeedLocalPerkBaseline(PerkHandler handler)
        {
            foreach (PerkDescriptor perk in handler.CollectedPerks)
            {
                if (perk == null)
                {
                    continue;
                }

                bool isNewlyObserved = _stageKnownPerkIds.Add(perk.PerkID);
                _stageBaselinePerkIds.Add(perk.PerkID);
                int upgradeLevel = Mathf.Clamp(handler.GetPerkUpgradeLevel(perk), 0, 2);
                _stageBaselinePerkUpgradeLevels[perk.PerkID] = upgradeLevel;
                if (isNewlyObserved && StageDpsRecords.Count == 0 && perk.IsWithEquipment)
                {
                    AddStagePerk(perk, upgradeLevel);
                }
            }
        }

        private void UnbindLocalPerkHandler()
        {
            if (_localPerkHandler != null)
            {
                _localPerkHandler.OnPerkCollected -= HandleLocalPerkCollected;
                _localPerkHandler = null;
            }
        }

        private void HandleLocalPerkCollected(PerkDescriptor perk)
        {
            if (perk == null || string.IsNullOrWhiteSpace(_currentStageSceneName))
            {
                return;
            }

            string perkName = GetPerkDisplayName(perk);
            int upgradeLevel = _localPerkHandler != null
                ? _localPerkHandler.GetPerkUpgradeLevel(perk)
                : 0;
            _stageBaselinePerkUpgradeLevels[perk.PerkID] = Mathf.Clamp(upgradeLevel, 0, 2);

            bool isNewOnHandler = _stageKnownPerkIds.Add(perk.PerkID);
            if (!isNewOnHandler ||
                string.IsNullOrWhiteSpace(perkName) ||
                _runRecordedPerkIds.Contains(perk.PerkID) ||
                _legacyRunRecordedPerkNames.Contains(perkName))
            {
                return;
            }

            AddStagePerk(perk, perkName, upgradeLevel);
        }

        private void AddStagePerk(PerkDescriptor perk, int upgradeLevel)
        {
            AddStagePerk(perk, GetPerkDisplayName(perk), upgradeLevel);
        }

        private void AddStagePerk(PerkDescriptor perk, string perkName, int upgradeLevel)
        {
            if (string.IsNullOrWhiteSpace(perkName))
            {
                return;
            }

            _currentStagePerkIds.Add(perk.PerkID);
            _currentStagePerks.Add(perkName);
            _currentStagePerkUpgradeLevels.Add(Mathf.Clamp(upgradeLevel, 0, 2));
            _currentStagePerkTalents.Add(perk.IsTalentPerk);
            _currentStageMasterTalents.Add(false);
            _stageKnownPerkIds.Add(perk.PerkID);
        }

        internal void RecordLocalChampionTalent(ChampionTalent talent, bool masterVersion)
        {
            if (talent == null ||
                talent.Stats == null ||
                talent.Stats.ActorID != LocalPlayerActorID ||
                LocalPlayerActorID == -1 ||
                string.IsNullOrWhiteSpace(_currentStageSceneName))
            {
                return;
            }

            PerkDescriptor talentPerk = talent.TalentPerk;
            if (talentPerk != null && _stageBaselinePerkIds.Contains(talentPerk.PerkID))
            {
                return;
            }

            string talentKey = talent.AbilityEnum + ":" + talent.TalentIndex + ":" + masterVersion;
            if (!_stageRecordedTalentKeys.Add(talentKey))
            {
                return;
            }

            string talentName = talent.Name != null ? talent.Name.Value : null;
            if (string.IsNullOrWhiteSpace(talentName))
            {
                _stageRecordedTalentKeys.Remove(talentKey);
                Logger.LogWarning($"[Perks] Could not record an activated champion talent with no usable name (index {talent.TalentIndex}).");
                return;
            }

            _currentStagePerkIds.Add(int.MinValue + _currentStagePerkIds.Count);
            _currentStagePerks.Add(talentName);
            _currentStagePerkUpgradeLevels.Add(0);
            _currentStagePerkTalents.Add(true);
            _currentStageMasterTalents.Add(masterVersion);
        }

        internal void RecordLocalPerkUpgrade(PerkHandler handler, int perkId)
        {
            if (handler == null ||
                handler != _localPerkHandler ||
                handler.Stats == null ||
                handler.Stats.ActorID != LocalPlayerActorID ||
                string.IsNullOrWhiteSpace(_currentStageSceneName))
            {
                return;
            }

            PerkDatabase perkDatabase = PerkDatabase.Instance;
            if (perkDatabase == null)
            {
                Logger.LogWarning($"[Perks] Could not record upgrade for perk ID {perkId}: PerkDatabase is unavailable.");
                return;
            }

            PerkDescriptor perk = perkDatabase.PerkDescriptorByID(perkId);
            if (perk == null)
            {
                Logger.LogWarning($"[Perks] Could not resolve upgraded perk ID {perkId}.");
                return;
            }

            string perkName = GetPerkDisplayName(perk);
            if (string.IsNullOrWhiteSpace(perkName))
            {
                return;
            }

            int upgradeLevel = handler.GetPerkUpgradeLevel(perk);
            int baselineUpgradeLevel = _stageBaselinePerkUpgradeLevels.TryGetValue(perkId, out int knownUpgradeLevel)
                ? knownUpgradeLevel
                : 0;
            if (upgradeLevel <= baselineUpgradeLevel)
            {
                return;
            }

            int stagePerkIndex = _currentStagePerkIds.LastIndexOf(perkId);
            if (stagePerkIndex >= 0)
            {
                _currentStagePerks[stagePerkIndex] = perkName;
                _currentStagePerkUpgradeLevels[stagePerkIndex] = Mathf.Clamp(upgradeLevel, 0, 2);
                _currentStagePerkTalents[stagePerkIndex] = perk.IsTalentPerk;
                _currentStageMasterTalents[stagePerkIndex] = false;
            }
            else
            {
                _currentStagePerkIds.Add(perkId);
                _currentStagePerks.Add(perkName);
                _currentStagePerkUpgradeLevels.Add(Mathf.Clamp(upgradeLevel, 0, 2));
                _currentStagePerkTalents.Add(perk.IsTalentPerk);
                _currentStageMasterTalents.Add(false);
            }
        }

        private string GetPerkDisplayName(PerkDescriptor perk)
        {
            string perkName = perk.HasTranslatedName
                ? LocalizationSettings.StringDatabase.GetLocalizedString(
                    "PerkName",
                    perk.name,
                    null,
                    FallbackBehavior.UseProjectSettings)
                : perk.name;
            if (string.IsNullOrWhiteSpace(perkName))
            {
                Logger.LogWarning($"[Perks] Could not resolve a display name for perk '{perk.name}'.");
                perkName = perk.name;
            }

            if (string.IsNullOrWhiteSpace(perkName))
            {
                Logger.LogWarning("[Perks] Ignored a perk with no usable name.");
                return null;
            }

            return perkName;
        }

        public PlayerStats GetPlayerStats(int actorID)
        {
            if (!AllPlayerStats.ContainsKey(actorID))
            {
                AllPlayerStats[actorID] = new PlayerStats { ActorID = actorID };
            }
            return AllPlayerStats[actorID];
        }
        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool isMainScene = scene.name == "MainScene";
            bool isLobbyScene = scene.name == "GB_LobbyAirship";
            bool stageWasActive = !string.IsNullOrEmpty(_currentStageSceneName);
            ulong sceneHandle = scene.handle.GetRawData();
            bool changedStage = stageWasActive && !isMainScene && !isLobbyScene &&
                sceneHandle != _currentStageSceneHandle;
            Debug.Log($"[DPS] Scene Loaded: {scene.name} | isMainScene: {isMainScene} | isLobbyScene: {isLobbyScene} | rootcount: {scene.rootCount} | stageWasActive: {stageWasActive}");

            if (changedStage)
            {
                FinishCurrentStage();
                UnbindLocalPerkHandler();
                _currentStageSceneName = null;
                _currentStageSceneHandle = 0;
            }
            else if (isMainScene || isLobbyScene)
            {
                UnbindLocalPerkHandler();
            }

            if (!isMainScene)
            {
                ResetMeter();
                Logger.LogInfo($"Meter reset via Scene Load: {scene.name}");
            }
            else
            {
                Logger.LogInfo("[DPS] MainScene is excluded from stage tracking; active stage data was preserved.");
            }

            if (!_uiBusReady)
            {
                StartCoroutine(InitializeUiBusAfterSceneLoad());
            }

            if (isLobbyScene)
            {
                _lobbySceneLoaded = true;
                _pendingStageSceneName = null;
                _pendingStageSceneHandle = 0;
                _currentStageSceneName = null;
                _currentStageSceneHandle = 0;
                fetchingActorID = true;
                go_timer = true;
            }
            else if (_lobbySceneLoaded)
            {
                _lobbySceneLoaded = false;
                StageDpsRecords.Clear();
                CombatStatsRecords.Clear();
                _runRecordedPerkIds.Clear();
                _legacyRunRecordedPerkNames.Clear();
                _currentStagePerks.Clear();
                _currentStagePerkIds.Clear();
                _currentStagePerkUpgradeLevels.Clear();
                _currentStagePerkTalents.Clear();
                _currentStageMasterTalents.Clear();
                _stageKnownPerkIds.Clear();
                _stageBaselinePerkIds.Clear();
                _stageRecordedTalentKeys.Clear();
                _stageBaselinePerkUpgradeLevels.Clear();
                var dashboard = _uiBusHost != null ? _uiBusHost.GetComponent<GraphDashboardWindow>() : null;
                dashboard?.RefreshStageGraph(StageDpsRecords);
                dashboard?.RefreshCombatStatsGraph(CombatStatsRecords);
                dashboard?.RefreshPerkTable(StageDpsRecords);
                SaveGraphHistory();
                Logger.LogInfo("[DPS] Cleared graph history after GB_LobbyAirship for a new run.");
            }

            if (isMainScene)
            {
                _pendingStageSceneName = null;
                _pendingStageSceneHandle = 0;
                fetchingActorID = false;
                go_timer = false;
            }
            else if (!isLobbyScene &&
                (!stageWasActive || changedStage) &&
                !string.IsNullOrEmpty(scene.name))
            {
                _pendingStageSceneName = scene.name;
                _pendingStageSceneHandle = sceneHandle;
                LocalPlayerActorID = -1;
                LocalPlayerStats.ActorID = -1;
                fetchingActorID = true;
                go_timer = false;
            }
            Logger.LogInfo($"[DPS] Scene Loaded: {scene.name} | Fetching ActorID: {fetchingActorID}");
        }

        internal void OnLocalPlayerActorIdCaptured()
        {
            if (LocalPlayerActorID == -1 || string.IsNullOrEmpty(_pendingStageSceneName))
            {
                return;
            }

            _currentStageSceneName = _pendingStageSceneName;
            _currentStageSceneHandle = _pendingStageSceneHandle;
            _pendingStageSceneName = null;
            _pendingStageSceneHandle = 0;
            _currentStagePerks.Clear();
            _currentStagePerkIds.Clear();
            _currentStagePerkUpgradeLevels.Clear();
            _currentStagePerkTalents.Clear();
            _currentStageMasterTalents.Clear();
            _stageKnownPerkIds.Clear();
            _stageBaselinePerkIds.Clear();
            _stageRecordedTalentKeys.Clear();
            _stageBaselinePerkUpgradeLevels.Clear();
            _currentStageCombatTime = 0f;
            Array.Clear(_currentStageMaximumCombatStats, 0, _currentStageMaximumCombatStats.Length);
            _hasCurrentStageCombatStats = false;
            _nextCombatStatsSampleTime = 0f;
            go_timer = true;
            Logger.LogInfo($"[DPS] Started stage tracking for {_currentStageSceneName} with local player {LocalPlayerActorID}.");
        }

        private void FinishCurrentStage()
        {
            if (string.IsNullOrEmpty(_currentStageSceneName) || LocalPlayerActorID == -1)
            {
                return;
            }

            float combatTime = _currentStageCombatTime;
            int stageNumber = StageDpsRecords.Count + 1;
            if (LocalPlayerStats.TotalDamage <= 0f && _currentStagePerks.Count == 0)
            {
                Logger.LogInfo($"[DPS] Skipped empty stage {_currentStageSceneName}; no local damage or new perks were recorded.");
                return;
            }

            CaptureCombatStatsForActiveStage(true);
            if (_localPerkHandler == null)
            {
                Logger.LogWarning($"[Perks] No local PerkHandler was available during stage {stageNumber}; perk pickups may be incomplete.");
            }

            StageDpsRecords.Add(new StageDpsRecord(
                stageNumber,
                _currentStageSceneName,
                LocalPlayerStats,
                combatTime,
                _currentStagePerks.ToArray(),
                _currentStagePerkUpgradeLevels.ToArray(),
                _currentStagePerkTalents.ToArray(),
                _currentStageMasterTalents.ToArray(),
                _currentStagePerkIds.ToArray()));
            for (int i = 0; i < _currentStagePerkIds.Count; i++)
            {
                if (_currentStagePerkIds[i] >= 0)
                {
                    _runRecordedPerkIds.Add(_currentStagePerkIds[i]);
                }
            }

            var combatInfo = CombatInfoModule.Instance;
            if (combatInfo != null)
            {
                CombatStatsRecords.Add(new CombatStatsRecord(
                    stageNumber,
                    _currentStageSceneName,
                    SelectedElement,
                    combatInfo.GetCombatStatsValues(),
                    _hasCurrentStageCombatStats ? _currentStageMaximumCombatStats : null));
            }
            else
            {
                Logger.LogWarning($"[CombatInfo] Could not record combat stats for stage {stageNumber}: CombatInfoModule is not available.");
            }

            var dashboard = _uiBusHost != null ? _uiBusHost.GetComponent<GraphDashboardWindow>() : null;
            dashboard?.RefreshStageGraph(StageDpsRecords);
            dashboard?.RefreshCombatStatsGraph(CombatStatsRecords);
            dashboard?.RefreshPerkTable(StageDpsRecords);
            SaveGraphHistory();
            Logger.LogInfo($"[DPS] Recorded stage {StageDpsRecords.Count}: {LocalPlayerStats.TotalDamage:F1} damage over {combatTime:F1}s ({LocalPlayerStats.TotalDamage / Mathf.Max(0.1f, combatTime):F1} DPS).");
        }

        internal void FinishCurrentRun()
        {
            if (string.IsNullOrEmpty(_currentStageSceneName))
            {
                return;
            }

            FinishCurrentStage();
            _currentStageSceneName = null;
            _currentStageSceneHandle = 0;
            UnbindLocalPerkHandler();
            SaveGraphHistory();
        }

        private string GraphHistoryPath => Path.Combine(Paths.ConfigPath, "RoB_DPSMeter_Graphs.json");

        private void LoadGraphHistory()
        {
            string path = GraphHistoryPath;
            if (!File.Exists(path))
            {
                Logger.LogInfo($"[DPS] No saved graph history found at '{path}'.");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                var history = JsonConvert.DeserializeObject<GraphHistoryData>(json);
                if (history == null || history.Version != 1)
                {
                    Logger.LogWarning($"[DPS] Graph history at '{path}' has an unsupported or invalid format; starting with empty graphs.");
                    return;
                }

                StageDpsRecords.Clear();
                _runRecordedPerkIds.Clear();
                _legacyRunRecordedPerkNames.Clear();
                if (history.StageRecords != null)
                {
                    foreach (var record in history.StageRecords)
                    {
                        if (record == null ||
                            string.IsNullOrEmpty(record.SceneName) ||
                            record.DamageByType == null ||
                            record.DamageByType.Length != StageDamageTypes.Names.Length)
                        {
                            Logger.LogWarning("[DPS] Skipping invalid stage DPS record in saved graph history.");
                            continue;
                        }

                        var stats = new PlayerStats
                        {
                            TotalDamage = record.TotalDamage,
                            TotalBurn = record.DamageByType[0],
                            TotalPoison = record.DamageByType[1],
                            TotalBleed = record.DamageByType[2],
                            TotalShock = record.DamageByType[3],
                            TotalRoot = record.DamageByType[4],
                            TotalFrost = record.DamageByType[5],
                            TotalCurse = record.DamageByType[6],
                            TotalMinion = record.DamageByType[7],
                            TotalBless = record.DamageByType[8],
                            TotalFury = record.DamageByType[9]
                        };
                        StageDpsRecords.Add(new StageDpsRecord(
                            record.StageNumber,
                            record.SceneName,
                            stats,
                            record.CombatTime,
                            record.CollectedPerks,
                            record.PerkUpgradeLevels,
                            record.PerkTalents,
                            record.MasterTalents,
                            record.CollectedPerkIds));

                        string[] collectedPerks = record.CollectedPerks ?? Array.Empty<string>();
                        bool hasPerkIds = record.CollectedPerkIds != null &&
                            record.CollectedPerkIds.Length == collectedPerks.Length;
                        for (int perkIndex = 0; perkIndex < collectedPerks.Length; perkIndex++)
                        {
                            if (hasPerkIds)
                            {
                                if (record.CollectedPerkIds[perkIndex] >= 0)
                                {
                                    _runRecordedPerkIds.Add(record.CollectedPerkIds[perkIndex]);
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(collectedPerks[perkIndex]))
                            {
                                _legacyRunRecordedPerkNames.Add(collectedPerks[perkIndex]);
                            }
                        }
                    }
                }

                CombatStatsRecords.Clear();
                if (history.CombatStatsRecords != null)
                {
                    foreach (var record in history.CombatStatsRecords)
                    {
                        if (record == null ||
                            string.IsNullOrEmpty(record.SceneName) ||
                            record.Values == null ||
                            record.Values.Length != 13 ||
                            record.MaximumValues == null ||
                            record.MaximumValues.Length != record.Values.Length)
                        {
                            Logger.LogWarning("[DPS] Skipping invalid combat stats record in saved graph history.");
                            continue;
                        }

                        CombatStatsRecords.Add(new CombatStatsRecord(
                            record.StageNumber,
                            record.SceneName,
                            record.SelectedElement,
                            record.Values,
                            record.MaximumValues));
                    }
                }
                Logger.LogInfo($"[DPS] Restored {StageDpsRecords.Count} stage graph records and {CombatStatsRecords.Count} combat stats records.");
            }
            catch (IOException exception)
            {
                Logger.LogError($"[DPS] Failed to read graph history from '{path}': {exception}");
            }
            catch (UnauthorizedAccessException exception)
            {
                Logger.LogError($"[DPS] Access denied while reading graph history from '{path}': {exception}");
            }
            catch (Exception exception)
            {
                Logger.LogError($"[DPS] Failed to load graph history from '{path}': {exception}");
            }
        }

        private void SaveGraphHistory()
        {
            string path = GraphHistoryPath;
            try
            {
                var history = new GraphHistoryData();
                history.StageRecords = StageDpsRecords
                    .Select(record => new StageDpsHistoryData
                    {
                        StageNumber = record.StageNumber,
                        SceneName = record.SceneName,
                        TotalDamage = record.TotalDamage,
                        CombatTime = record.CombatTime,
                        DamageByType = (float[])record.DamageByType.Clone(),
                        CollectedPerks = (string[])record.CollectedPerks.Clone(),
                        CollectedPerkIds = (int[])record.CollectedPerkIds.Clone(),
                        PerkUpgradeLevels = (int[])record.PerkUpgradeLevels.Clone(),
                        PerkTalents = (bool[])record.PerkTalents.Clone(),
                        MasterTalents = (bool[])record.MasterTalents.Clone()
                    })
                    .ToArray();

                history.CombatStatsRecords = CombatStatsRecords
                    .Select(record => new CombatStatsHistoryData
                    {
                        StageNumber = record.StageNumber,
                        SceneName = record.SceneName,
                        SelectedElement = record.SelectedElement,
                        Values = (float[])record.Values.Clone(),
                        MaximumValues = (float[])record.MaximumValues.Clone()
                    })
                    .ToArray();

                Directory.CreateDirectory(Paths.ConfigPath);
                string json = JsonConvert.SerializeObject(history, Formatting.Indented);
                var verifiedHistory = JsonConvert.DeserializeObject<GraphHistoryData>(json);
                if (verifiedHistory == null ||
                    verifiedHistory.StageRecords == null ||
                    verifiedHistory.CombatStatsRecords == null ||
                    verifiedHistory.StageRecords.Length != history.StageRecords.Length ||
                    verifiedHistory.CombatStatsRecords.Length != history.CombatStatsRecords.Length)
                {
                    throw new InvalidDataException("Serialized graph history failed record-count validation.");
                }

                File.WriteAllText(path, json);
                Logger.LogInfo($"[DPS] Saved {history.StageRecords.Length} stage graph records and {history.CombatStatsRecords.Length} combat stats records to '{path}'.");
            }
            catch (IOException exception)
            {
                Logger.LogError($"[DPS] Failed to save graph history to '{path}': {exception}");
            }
            catch (UnauthorizedAccessException exception)
            {
                Logger.LogError($"[DPS] Access denied while saving graph history to '{path}': {exception}");
            }
            catch (Exception exception)
            {
                Logger.LogError($"[DPS] Failed to serialize graph history to '{path}': {exception}");
            }
        }

        private void OnApplicationQuit()
        {
            FinishCurrentRun();
            UnbindLocalPerkHandler();
            SaveGraphHistory();
        }

        private void OnDestroy()
        {
            UnbindLocalPerkHandler();
        }

        internal void CaptureCombatStatsForActiveStage(bool force = false)
        {
            if (string.IsNullOrEmpty(_currentStageSceneName) ||
                (!force && Time.unscaledTime < _nextCombatStatsSampleTime))
            {
                return;
            }

            _nextCombatStatsSampleTime = Time.unscaledTime + 0.25f;
            var combatInfo = CombatInfoModule.Instance;
            if (combatInfo == null)
            {
                return;
            }

            float[] values = combatInfo.GetCombatStatsValues();
            for (int i = 0; i < values.Length; i++)
            {
                if (!_hasCurrentStageCombatStats || values[i] > _currentStageMaximumCombatStats[i])
                {
                    _currentStageMaximumCombatStats[i] = values[i];
                }
            }
            _hasCurrentStageCombatStats = true;
        }

        internal void TickStageCombatTime(float deltaTime)
        {
            if (!string.IsNullOrEmpty(_currentStageSceneName))
            {
                _currentStageCombatTime += deltaTime;
            }
        }
    }

    public class PlayerStats
    {
        public int ActorID = -1;  // Captured when lobby is joined
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

        // Resets all individual stats back to zero
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
    // leaderboard purposes, separate from the local player stats to avoid any accidental resets or data conflicts
    
    // This class handles the actual rendering and stays alive forever
    public class PersistentUI : MonoBehaviour
    {
        private TextMeshProUGUI _uiText;
        private GameObject _canvasObj;
        private GameObject _panelObj;
        private float dpsupdateTime = 0f;
        public static Canvas _targetCanvas;
        
        

        void Start()
        {
            // 1. Create the Root Canvas
            _canvasObj = new GameObject("DPS_Overlay_Canvas");
            UnityEngine.Object.DontDestroyOnLoad(_canvasObj);
            
            Canvas canvas = _canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Force to top

            // Essential for Unity 2022.3 UI Modules
            _canvasObj.AddComponent<GraphicRaycaster>();
            // 2. Create the Background PANEL (The Border/Frame)
            _panelObj = new GameObject("DPS_Background_Frame");
            _panelObj.transform.SetParent(_canvasObj.transform, false);

            UnityEngine.UI.Image frameImage = _panelObj.AddComponent<UnityEngine.UI.Image>();
            // A "Bronze/Gold" color to match the image metal
            frameImage.color = new Color(0.45f, 0.35f, 0.2f, 1f); 

            RectTransform panelRect = frameImage.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 0.5f);
            panelRect.anchorMax = new Vector2(1, 0.5f);
            panelRect.pivot = new Vector2(1, 0.5f);
            panelRect.anchoredPosition = new Vector2(-10, 0);
            panelRect.sizeDelta = new Vector2(220, 220);

            // Add an Outline component to give it the "raised" metal edge look
            var outline = _panelObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0.15f, 0.1f, 0.05f, 1f); // Darker shadow
            outline.effectDistance = new Vector2(2, -2);

            // 2b. Create the Inner Background (The Teal/Green area)
            GameObject innerArea = new GameObject("Inner_Area");
            innerArea.transform.SetParent(_panelObj.transform, false);

            UnityEngine.UI.Image innerImage = innerArea.AddComponent<UnityEngine.UI.Image>();
            // Dark Teal/Green color from your image
            innerImage.color = new Color(0f, 0f, 0f, 0.95f); 

            RectTransform innerRect = innerImage.rectTransform;
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            // Padding of 5 pixels creates the "Border" thickness
            innerRect.offsetMin = new Vector2(5, 5);
            innerRect.offsetMax = new Vector2(-5, -5);

            // 3. Create the TEXT Display (As child of the inner area)
            GameObject textObj = new GameObject("DPS_Text_Display");
            textObj.transform.SetParent(innerArea.transform, false);

            _uiText = textObj.AddComponent<TextMeshProUGUI>();
            _uiText.fontSize = 16;
            _uiText.color = Color.white;
            _uiText.alignment = TextAlignmentOptions.Center;

            // Ensure text stays inside the border
            RectTransform textRect = _uiText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero; // Stretch to fit inside padding
            textRect.offsetMin = new Vector2(5, 5); 
            textRect.offsetMax = new Vector2(-5, -5);

            // Find the LiberationSans SDF Asset in the game's memory
            TMP_FontAsset gameFont = null;
            var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var f in fonts)
            {
                if (f.name.Contains("LiberationSans"))
                {
                    gameFont = f;
                    break;
                }
            }

            if (gameFont != null)
            {
                _uiText.font = gameFont;
            }
            else
            {
                Debug.LogWarning("[DPS] Could not find LiberationSans SDF, using TMP default.");
            }

            _uiText.fontSize = 14;
            _uiText.color = Color.yellow;
            _uiText.alignment = TextAlignmentOptions.TopLeft; // TMP uses different alignment names

            // TMP handles overflow automatically, but we can set specific modes:
            _uiText.overflowMode = TextOverflowModes.Overflow;
            _uiText.textWrappingMode = TMPro.TextWrappingModes.NoWrap;

            _targetCanvas = _canvasObj.GetComponent<Canvas>();
            _targetCanvas.enabled = Plugin._isVisible; // Restore overlay visibility from config

        }
        

        private float _nextDebugTime = 0f;

        void Update()
        {
            Plugin.Instance.CaptureCombatStatsForActiveStage();

            // Capture ActorID from LocalPlayer on first frame available
            if (Plugin.Instance.fetchingActorID)
            {
                var pm = RR.PlayerManager.Instance;
                if (pm != null && pm.LocalPlayer != null)
                {
                    int capturedActorID = -1;
                    Debug.Log("[DPS] Attempting to capture LocalPlayer ActorID...");
                    // Try 1: local playerslot
                    Debug.Log($"[DPS] PlayerManager.LocalPlayer: {pm.LocalPlayer.name} | ActorID: {pm.LocalPlayerSlot}");
                    capturedActorID = pm.LocalPlayerSlot;
                    
                    if (capturedActorID != -1)
                    {
                        Debug.Log($"[DPS] Captured LocalPlayer ActorID: {capturedActorID}");
                        Plugin.Instance.LocalPlayerActorID = capturedActorID;
                        Plugin.Instance.LocalPlayerStats.ActorID = capturedActorID;
                        Plugin.Instance.fetchingActorID = false;
                        Plugin.Instance.OnLocalPlayerActorIdCaptured();
                        Debug.Log($"[DPS] ✓ LocalPlayerActorID set to {Plugin.Instance.LocalPlayerActorID}");
                    }
                }
            }

            
            _targetCanvas.enabled = Plugin.ShowDPSMeter && Plugin._isVisible; // Ensure canvas visibility matches both config and toggle state

            if (Time.time >= _nextDebugTime)
            {
                _nextDebugTime = Time.time + 2.0f;
                Debug.Log("[DPS Debug] Heartbeat");
                PrintActivePlayersDebug();
            }
            if (Plugin.Instance.StartTime > 0)
            {
                // Check if we are "In Combat" (Hit within the last 1.0 seconds)
                bool inCombat = (Time.time - Plugin.Instance.LastHitTime) <= 1.0f;

                if (inCombat)
                {
                    // Add time EVERY frame so the clock is accurate
                    Plugin.Instance.ActiveCombatTime += Time.deltaTime;
                    Plugin.Instance.TickStageCombatTime(Time.deltaTime);
                }
            }
            // Safety check: The UI will only update if the plugin successfully captured the start time
            if (Plugin.Instance.StartTime > 0 && Time.time > dpsupdateTime)
            {
                
                dpsupdateTime = Time.time + 0.5f; // Update every 0.5 seconds (adjust as needed)

                float displayTime = Mathf.Max(0.1f, Plugin.Instance.ActiveCombatTime);

                if (displayTime > 0.1f)
                {           
                    
                    float dps_Total = Plugin.Instance.LocalPlayerStats.TotalDamage / displayTime;
                    // Use a small helper function to keep the code clean
                    string FormatLine(string label, float val) => 
                        $"{label}: {val / displayTime:F1} ({(val / displayTime / dps_Total) * 100:F1}%)";
                    // Use a small helper function for colors
                    string ColorText(string text, string hex) => $"<color={hex}>{text}</color>";
                    // --- CLIPPED BAR LOGIC ---
                    int barWidth = 20; // Total character slots
                    int remainingSlots = barWidth;
                    string visualBar = "";

                    // Local helper to handle the clipping math
                    void AddClippedSegment(float damage, string hex) {
                        if (damage <= 0 || remainingSlots <= 0) return;

                        // Calculate proportional slots
                        int slots = Mathf.RoundToInt((damage/ displayTime / dps_Total) * barWidth);
                        
                        // CLIP: Ensure we don't take more than what's left
                        slots = Mathf.Min(slots, remainingSlots);
                        
                        if (slots > 0) {
                            visualBar += $"<color={hex}>{new string('█', slots)}</color>";
                            remainingSlots -= slots;
                        }
                    }

                    // Add segments in order of priority
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

                    // FILLER: If damage types don't sum to 100% (Raw damage), or rounding left a gap
                    if (remainingSlots > 0) {
                        visualBar += $"<color=#555555>{new string('█', remainingSlots)}</color>";
                    }
                    // -------------------------


                    string fullContent = string.Join("\n", 
                        visualBar,
                        $"<size=+4>{ColorText($"Total DPS: {dps_Total:F1}", "#dbdbdb")}</size>",
                        ColorText(FormatLine("Burn", Plugin.Instance.LocalPlayerStats.TotalBurn), "#f17728"),
                        ColorText(FormatLine("Poison", Plugin.Instance.LocalPlayerStats.TotalPoison), "#952db8"),
                        ColorText(FormatLine("Bleed", Plugin.Instance.LocalPlayerStats.TotalBleed), "#c94a46"),
                        ColorText(FormatLine("Shock", Plugin.Instance.LocalPlayerStats.TotalShock), "#b7a93f"),
                        ColorText(FormatLine("Root", Plugin.Instance.LocalPlayerStats.TotalRoot), "#be927e"),
                        ColorText(FormatLine("Frost", Plugin.Instance.LocalPlayerStats.TotalFrost), "#57cccc"),
                        ColorText(FormatLine("Curse", Plugin.Instance.LocalPlayerStats.TotalCurse), "#267b5b"),
                        ColorText(FormatLine("Minion", Plugin.Instance.LocalPlayerStats.TotalMinion), "#f06ee9"),
                        ColorText(FormatLine("Bless", Plugin.Instance.LocalPlayerStats.TotalBless), "#d8f19c"),
                        ColorText(FormatLine("Fury", Plugin.Instance.LocalPlayerStats.TotalFury), "#64210f")
                    );
                    _uiText.text = $"<width=75%>{fullContent}</width>";
                }
                else
                {
                    // Optional: Dim the text or add "(PAUSED)" to show combat ended
                    _uiText.color = new Color(0.7f, 0.7f, 0f); // Dimmer Yellow
                    // We don't recalculate DPS here, so it stays frozen at the last value
                }
            }
            
        }
 
        
        private void PrintActivePlayersDebug()
        {
            var pm = RR.PlayerManager.Instance;
            if (pm == null)
            {
                Debug.Log("[DPS Debug] PlayerManager.Instance is NULL");
                return;
            }

            var players = pm.GetPlayers();
            Debug.Log($"--- [DPS Debug] Active Players Count: {players.Count} ---");

            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (p == null)
                {
                    Debug.Log($"  [{i}] PLAYER OBJECT IS NULL");
                    continue;
                }

                // We check several ID types to ensure we find the one 
                // that matches your 'attackerID' in the patch.
                string name = string.IsNullOrEmpty(p.UserName) ? "EMPTY_NAME" : p.UserName;
                int fusionID = p.FusionPlayerRef.PlayerId;
                int playerID = p.PlayerId; // Internal RR ID
                bool isLocal = (pm.LocalPlayer == p);

                Debug.Log($"  [{i}] Name: {name} | FusionID: {fusionID} | PlayerID: {playerID} | Local: {isLocal}");
            }
            Debug.Log("------------------------------------------");
        }
    }

    [HarmonyPatch(typeof(GameManager), "RPC_GameEnd")]
    internal static class GameEndStageTrackingPatch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            Plugin.Instance?.FinishCurrentRun();
        }
    }

    [HarmonyPatch(typeof(PerkHandler), "RPC_UpgradePerk")]
    internal static class PerkUpgradeTrackingPatch
    {
        [HarmonyPostfix]
        private static void Postfix(PerkHandler __instance, int perkID)
        {
            Plugin.Instance?.RecordLocalPerkUpgrade(__instance, perkID);
        }
    }

    [HarmonyPatch(typeof(ChampionTalent), "ActivateInternal")]
    internal static class ChampionTalentTrackingPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ChampionTalent __instance, bool masterVersion)
        {
            Plugin.Instance?.RecordLocalChampionTalent(__instance, masterVersion);
        }
    }

    internal static class HealthDamageLogPatch
{
    internal static void Apply(Harmony harmony)
    {
        if (!HealthDamageLogController.Initialize()) { return; }

        var render = AccessTools.Method(typeof(Health), "Render");
        if (render == null)
        {
            Debug.LogWarning("DPS Meter: Health.Render not found — damage logging inactive.");
            return;
        }
        harmony.Patch(render,
            prefix: new HarmonyMethod(typeof(HealthDamageLogPatch), nameof(RenderPrefix)),
            postfix: new HarmonyMethod(typeof(HealthDamageLogPatch), nameof(RenderPostfix)));
        Debug.Log("DPS Meter: Health.Render patched.");
    }

    private static void RenderPrefix(Health __instance, out int __state) =>
        HealthDamageLogController.CaptureState(__instance, out __state);

    private static void RenderPostfix(Health __instance, int __state) =>
        HealthDamageLogController.ProcessNewEntries(__instance, __state);
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
            Debug.LogWarning("DPS Meter: Health._lastVisualizedDamageData not found — inactive.");
            return false;
        }

        var nddType = typeof(Health).GetNestedType("NetworkedDamageData", BindingFlags.NonPublic);
        if (nddType == null)
        {
            Debug.LogWarning("DPS Meter: Health.NetworkedDamageData not found — inactive.");
            return false;
        }

        _arrayProp = AccessTools.Property(typeof(Health), "ReceivedDamageDataArray");
        if (_arrayProp == null)
        {
            Debug.LogWarning("DPS Meter: Health.ReceivedDamageDataArray not found — inactive.");
            return false;
        }

        var arrayType = typeof(NetworkArray<>).MakeGenericType(nddType);
        _arrayGetMethod = arrayType.GetMethod("Get") ?? arrayType.GetProperty("Item")?.GetMethod;
        if (_arrayGetMethod == null)
        {
            Debug.LogWarning("DPS Meter: NetworkArray<T>.Get/Item not found — inactive.");
            return false;
        }

        _valueField = nddType.GetField("value");
        _attackerIdField = nddType.GetField("attackerID");
        _damageTypeField = nddType.GetField("damageStatusType");
        _criticalField = nddType.GetField("critical");

        if (_criticalField != null)
        {
            var nbType = _criticalField.FieldType;
            _criticalInnerField =
                nbType.GetField("_isSet", BindingFlags.NonPublic | BindingFlags.Instance) ??
                nbType.GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance) ??
                nbType.GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);
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
            int newCounter = (int)_lastVisualizedField.GetValue(instance);
            if (newCounter <= oldCounter) { return; }

            object array = _arrayProp.GetValue(instance);
            for (int i = oldCounter; i < newCounter; i++)
            {
                object entry = _arrayGetMethod.Invoke(array, new object[] { i % 64 });
                if (entry == null) { continue; }

                float val = (float)(_valueField?.GetValue(entry) ?? 0f);
                if (val == 0f) { continue; } // skip dodges

                int attackerId = (int)(_attackerIdField?.GetValue(entry) ?? -1);
                object statusType = _damageTypeField?.GetValue(entry);
                bool crit = ReadCritical(entry);

                UpdatePlayerStats(val, statusType?.ToString() ?? "", attackerId, instance);
                Debug.Log($"DPS Meter [{instance.name}] val={val:F1} type={statusType} crit={crit} attackerId={attackerId}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"DPS Meter: error — {ex.Message}");
        }
    }

    private static bool ReadCritical(object entry)
    {
        if (_criticalField == null || _criticalInnerField == null) { return false; }
        try
        {
            object nb = _criticalField.GetValue(entry);
            if (nb == null) { return false; }
            var raw = _criticalInnerField.GetValue(nb);
            return raw is byte b ? b != 0 : raw is int v ? v != 0 : false;
        }
        catch { return false; }
    }

    private static void UpdatePlayerStats(float val, string type, int attackerId, Health victimHealth = null)
    {
        // --- GLOBAL COMBAT TIMING (updated for any damage event) ---
        if (Plugin.Instance.StartTime < 0) Plugin.Instance.StartTime = Time.time;
        Plugin.Instance.LastHitTime = Time.time;

        Debug.Log($"[DPS Meter] Processing Damage: {val} Type: {type} AttackerID: {attackerId}");

        // Track TOTAL damage for ALL players in the group (no type breakdown for group)
        var playerStats = Plugin.Instance.GetPlayerStats(attackerId);
        playerStats.TotalDamage += val;

        // Also update local player stats with full detail (for backwards compatibility and primary display)
        if (attackerId == Plugin.Instance.LocalPlayerActorID)
        {
            var localStats = Plugin.Instance.LocalPlayerStats;
            localStats.TotalDamage += val;
            
            if (type.Contains("Burn")) localStats.TotalBurn += val;
            else if (type.Contains("Poison")) localStats.TotalPoison += val;
            else if (type.Contains("Bleed")) localStats.TotalBleed += val;
            else if (type.Contains("Shock")) localStats.TotalShock += val;
            else if (type.Contains("Curse")) localStats.TotalCurse += val;
            else if (type.Contains("Root")) localStats.TotalRoot += val;
            else if (type.Contains("Freeze") || type.Contains("Chill") || type.Contains("Frost")) 
            {
                localStats.TotalFrost += val;
            }

            // --- FROZEN BONUS DETECTION ---
            if (victimHealth != null)
            {
                try
                {
                    var victimStats = victimHealth.GetComponent<StatsManager>();
                    if (victimStats != null && victimStats.IsFrozen && victimStats.ActorID != attackerId)
                    {
                        // Victim is frozen, check for frozen multiplier bonus
                        Debug.Log($"[DPS] Victim is frozen. Checking for frozen bonus from attacker {attackerId}...");
                        var pm = RR.PlayerManager.Instance;
                        if (pm != null)
                        {
                            var attackerPlayer = pm.GetPlayers().FirstOrDefault(p => 
                            {
                                var pStats = p.GetComponent<StatsManager>();
                                return pStats != null && pStats.ActorID == attackerId;
                            });
                            
                            if (attackerPlayer != null)
                            {
                                var chill = attackerPlayer.GetComponent<RR.Game.Stats.Chill>();
                                if (chill != null)
                                {
                                    float multiplier = chill.DamageMultiplierAgainstFrozenTarget;
                                    if (multiplier > 1.0f)
                                    {
                                        float damageBeforeFrozen = val / multiplier;
                                        float frozenBonus = val - damageBeforeFrozen;
                                        localStats.TotalFrost += frozenBonus;
                                        playerStats.TotalDamage += frozenBonus;
                                        Debug.Log($"[DPS] Frozen Bonus Detected: +{frozenBonus:F1}");
                                    }
                                }
                            }
                        }
                    }
                }
                catch { /* Silent fail for frozen check */ }
            }
        }
    }
}
    // this is to catch starting time for remote player and minion damage since it doesn't go through the normal damage pipeline
[HarmonyPatch(typeof(Attack), "ModifyDamageWithModifierAndCritical")]
public class RemoteDamageHook
{
    static void Postfix(object __instance, StatsManager victim, ref DamageDescriptor damageDesc, UserAction userAction, bool onlyForUI)
        {
            if (damageDesc.damageValue != 0) 
            {
                var stats = Traverse.Create(__instance).Field("_stats").GetValue<StatsManager>();
                float damageValue = damageDesc.damageValue;

                if (onlyForUI)
                {   
                    if (Plugin.Instance.StartTime < 0) Plugin.Instance.StartTime = Time.time;
                    Plugin.Instance.LastHitTime = Time.time;
                }
                else if (stats.IsChampionMinion)
                {
                    // Track TOTAL minion damage for ALL players (no type breakdown)
                    int ownerActorID = stats.SpawnerActorID;

                    if (ownerActorID != -1) // -1 is InvalidActorID
                    {
                        var ownerStats = Plugin.Instance.GetPlayerStats(ownerActorID);
                        ownerStats.TotalDamage += damageValue;
                        
                        // Also update local player if this is their minion
                        if (ownerActorID == Plugin.Instance.LocalPlayerActorID)
                        {
                            Plugin.Instance.LocalPlayerStats.TotalMinion += damageValue;
                        }
                        
                        Debug.Log($"[DPS] Distilled {damageValue} Minion Damage from Actor {stats.ActorID}");
                    }
                }
                else if (stats.IsChampion)
                {
                    // Track BLESSED and FURY for local player only
                    if (stats.ActorID != Plugin.Instance.LocalPlayerActorID) return;
                    
                    var localStats = Plugin.Instance.LocalPlayerStats;
                    
                    switch (userAction)
                        {
                            case UserAction.Attack:
                            {
                                if (damageDesc.blessedAttack)
                                {
                                    float multiplier = 1f + (stats.Bless.EmpoweredAttackDamageIncrementPCT / 100f);
                                    float originalDamage = damageValue / multiplier;
                                    localStats.TotalBless += damageValue - originalDamage;
                                    
                                    Debug.Log($"[DPS] Credited {stats.Bless.EmpoweredAttackDamageIncrementPCT}% Blessed Damage");

                                }
                                if (damageDesc.furyAttack)
                                {   
                                    float multiplier = 1f + (stats.Fury.BoostedAttackDamageIncPCT.Value / 100f);
                                    float originalDamage = damageValue / multiplier;
                                    localStats.TotalFury += damageValue - originalDamage;
                                }
                            }
                        break;
                        }
                    
                }

            }
        }
    }
    [HarmonyPatch(typeof(StatsManager), "CalculateMyDamageAgainst")]
    public class FrozenDamageHook
    {

        [HarmonyPostfix]
        static void Postfix(StatsManager __instance, StatsManager victim, ref DamageDescriptor dmgDesc, bool calcForUI)
        {
           
            if ( victim == null || __instance == null) return;

            if (victim.IsFrozen)
                {
                    var chill = __instance.GetComponent<RR.Game.Stats.Chill>();
                    var stats = __instance.GetComponent<RR.Game.Stats.StatsManager>();
                    
                    if (chill != null && stats != null && stats.ActorID == Plugin.Instance.LocalPlayerActorID)
                    {
                        float multiplier = chill.DamageMultiplierAgainstFrozenTarget;
                        
                        // Prevent division by zero if multiplier isn't set
                        if (multiplier <= 1.0f) return;

                        float damageBeforeFrozen = dmgDesc.damageValue / multiplier;
                        float frozenDelta = dmgDesc.damageValue - damageBeforeFrozen;

                        if (frozenDelta > 0)
                        {
                            // Track frozen damage for local player
                            Plugin.Instance.LocalPlayerStats.TotalFrost += frozenDelta;
                            
                            // Also track global combat timing for this player
                            if (Plugin.Instance.StartTime < 0) Plugin.Instance.StartTime = Time.time;
                            Plugin.Instance.LastHitTime = Time.time;

                            Debug.Log($"[DPS Meter] Dealt {frozenDelta} Frozen Bonus Damage.");
                        }
                    }
                }
            
        }
    }
}
    