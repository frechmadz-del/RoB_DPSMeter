using System;
using BepInEx;
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
using RR.Game.Character;
using RR.Game.Input;
using System.Reflection;
using RR.UI.Controls.Inventory;
using UnityEngine.UIElements;
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
    [BepInPlugin("com.gemini.dpsmeter", "RoB Persistent DPS", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public PlayerStats LocalPlayerStats = new PlayerStats();
        public Dictionary<int, PlayerStats> AllPlayerStats = new Dictionary<int, PlayerStats>(); // Track ALL players' damage
        public float ActiveCombatTime = 0f; // Tracks accumulated combat seconds
        public float StartTime = -1f;
        public float LastHitTime = -1f; // out of fight timer
        public int LocalPlayerActorID = -1; // Captured when joining lobby, used to filter damage logs in multiplayer
        private static bool _instanceExists = false;
        public bool fetchingActorID = false; // Debug toggle to enable ActorID fetching logs in the Update loop
        private bool mainSceneLoaded = false; // Flag to ensure we only reset the meter once per scene load
        public bool go_timer = false; // Debug toggle to enable combat timer logs in the Update loop
        public static bool _isVisible = false;
        
        public static ConfigEntry<bool> ShowDPSMeterConfig;
        public static ConfigEntry<bool> ShowGroupDPSConfig;
        public static ConfigEntry<bool> ShowCombatInfoConfig;
        public static ConfigEntry<string> SelectedElementConfig;
        public static ConfigEntry<int> MinRarityThresholdConfig; // Min rarity threshold for displayed equipment
        public static ConfigEntry<string> SelectedRarityModeConfig; // Rarity mode for displayed equipment

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

        // Minimum rarity threshold for displayed equipment (0=Common, 1=Rare, 2=Epic, 3=Legendary, 4=Mythic)
        public static int MinRarityThreshold
        {
            get => MinRarityThresholdConfig?.Value ?? 2; // Default to Epic
            set
            {
                if (MinRarityThresholdConfig != null)
                {
                    MinRarityThresholdConfig.Value = value;
                    MinRarityThresholdConfig.ConfigFile.Save();
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
            MinRarityThresholdConfig = Config.Bind("Equipment", "MinRarityThreshold", 2, "Minimum rarity level for displayed equipment (2=Epic, 3=Legendary, 4=Mythic).");
            SelectedRarityModeConfig = Config.Bind("Equipment", "RarityMode", "off", "Rarity mode for displayed equipment");
            _isVisible = ShowDPSMeter;

            // 4. Run initialization ONCE and ONLY once
            var harmony = new Harmony("com.gemini.dpsmeter");
            harmony.PatchAll();
            HealthDamageLogPatch.Apply(harmony);
            SummonValidationPatches.Apply(harmony);
            PermanentItemLabelPatch.Apply(harmony);

            // 5. Create your UI Bus
            var tracker = new GameObject("DPS_Global_Bus");
            tracker.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(tracker);
            tracker.AddComponent<PersistentUI>();
            tracker.AddComponent<CombatInfoModule>();
            tracker.AddComponent<GroupDamageMeter>();
            tracker.AddComponent<HotkeyRunner>();

            // 5. Safe event subscription
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            Logger.LogInfo("Mod Injected: Hidden Bus & Harmony Patches Active.");
        }
        private class HotkeyRunner : MonoBehaviour
        {   
            private bool _isKeyHeld = false; // Our custom debounce
            private void Update()
            {   
                var keyboard = UnityEngine.InputSystem.Keyboard.current;
                bool ui_pressed = keyboard.pKey.isPressed;

                if (keyboard.oKey.isPressed)
                {
                    Plugin.Instance.ResetMeter();
                }
                
                if (ui_pressed && !_isKeyHeld )
                {   
                    Debug.Log("[DPS] P Pressed: Toggling UI Visibility");
                    _isKeyHeld = true;
                    Debug.Log($"[DPS] Current Visibility: {Plugin._isVisible} | Toggling to: {!Plugin._isVisible} | settings ShowDPSMeter: {Plugin.ShowDPSMeter}");
                    
                    Plugin.Instance.ToggleUIVisibility();
                    
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
            
            Plugin.Instance.ActiveCombatTime = 0.11f;
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
            ResetMeter();
            Logger.LogInfo($"Meter reset via Scene Load: {scene.name}");
            // Capture ActorID when joining Lobby
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
            Logger.LogInfo($"[DPS] Scene Loaded: {scene.name} | Fetching ActorID: {fetchingActorID}");
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
            _uiText.enableWordWrapping = false;

            _targetCanvas = _canvasObj.GetComponent<Canvas>();
            _targetCanvas.enabled = Plugin._isVisible; // Restore overlay visibility from config

        }
        

        private float _nextDebugTime = 0f;

        void Update()
        {
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

    // Allocate minons correctly to the summoner
    public class SummonController : MonoBehaviour 
    {
        private void Awake() 
        {
            Debug.Log("Persistent Summon Hook Controller Active.");
        }

    }
    public static class SummonValidationPatches
    {
        private const int INVALID_ID = -1;

        internal static void Apply(Harmony harmony)
        {
            try
            {
                // Patch StatsManager.SpawnerActorID property setter
                var spawnerActorIdSetter = AccessTools.PropertySetter(typeof(StatsManager), "SpawnerActorID");
                if (spawnerActorIdSetter != null)
                {
                    Debug.Log("[Summon] Successfully patched StatsManager.SpawnerActorID setter.");
                }
                else
                {
                    Debug.LogWarning("[Summon] Failed to find StatsManager.SpawnerActorID property setter.");
                }

                // Patch Summon.InitMinionSpawned method
                var initMinionMethod = AccessTools.Method(typeof(Summon), "InitMinionSpawned");
                if (initMinionMethod != null)
                {
                    harmony.Patch(initMinionMethod,
                        prefix: new HarmonyMethod(typeof(SummonValidationPatches), nameof(Prefix_InitMinionSpawned)));
                    Debug.Log("[Summon] Successfully patched Summon.InitMinionSpawned method.");
                }
                else
                {
                    Debug.LogWarning("[Summon] Failed to find Summon.InitMinionSpawned method.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Summon] Error applying Summon patches: {ex.Message}");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Summon), "InitMinionSpawned")]
        public static void Prefix_InitMinionSpawned(Summon __instance, NetworkObject obj)
        {
            // 1. Safe extraction of the summoner's stats
            var summonerStats = Traverse.Create(__instance).Field<StatsManager>("_stats").Value;
            
            Debug.Log($"[SummonController] Minion Spawned Triggered: {obj?.name ?? "Unknown Object"} | Summoner: {summonerStats?.name ?? "NULL"}");

            if (obj == null) return;

            // 2. Check if both the summoner and the spawned minion have StatsManager components
            if (summonerStats != null && obj.TryGetComponent<StatsManager>(out var minionStats))
            {
                int ownerID = summonerStats.ActorID;
                Debug.Log($"[SummonController] Summoner ActorID: {ownerID} | Minion Current SpawnerID: {minionStats.SpawnerActorID}");
                
                if (ownerID != INVALID_ID)
                {
                    // Use Traverse to set the backing field directly if the property setter is stubborn,
                    // OR just assign it directly now that the blocking prefix is removed.
                    minionStats.SpawnerActorID = ownerID;
                    
                    Debug.Log($"[SummonController] Successfully assigned SpawnerActorID {ownerID} to {obj.name}");
                }
                else
                {
                    Debug.LogWarning($"[SummonController] Did not assign ID because Summoner ActorID is INVALID (-1)");
                }
            }
            else
            {
                Debug.LogWarning($"[SummonController] Failed to assign: SummonerStats or MinionStats is missing.");
            }
        }
    }
}
