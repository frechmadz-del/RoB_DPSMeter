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

namespace BlackveilDpsMeter
{
    [BepInPlugin("com.gemini.dpsmeter", "RoB Persistent DPS", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public PlayerStats LocalPlayerStats = new PlayerStats();
        public float ActiveCombatTime = 0f; // Tracks accumulated combat seconds
        public float StartTime = -1f;
        public float LastHitTime = -1f; // out of fight timer
        private static bool _instanceExists = false;
        
        void Awake()
        {
            Instance = this;

            if (_instanceExists)
            {
                Destroy(this.gameObject);
                return;
            }

            _instanceExists = true;
            
            // 1. Force Harmony to patch the game's own Update loop as a heartbeat
            var harmony = new Harmony("com.gemini.dpsmeter");
            harmony.PatchAll();

            // 2. Create a hidden object that survives scene changes
            var tracker = new GameObject("DPS_Global_Bus");
            tracker.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(tracker);
            tracker.AddComponent<PersistentUI>();
            tracker.AddComponent<SummonController>();

            // Subscribe to Unity's sceneLoaded event
            SceneManager.sceneLoaded += OnSceneLoaded;

            Logger.LogInfo("Mod Injected: Hidden Bus & Harmony Patches Active.");

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
            Plugin.Instance.ActiveCombatTime = 0.11f;
        }
        // This method is called every time a new scene is loaded and resets the DPSmeter
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // List of scenes you DON'T want to reset on (like the Main Menu)
            if (scene.name != "MainMenu" && scene.name != "Lobby")
            {
                ResetMeter();
                Logger.LogInfo($"Meter reset via Scene Load: {scene.name}");
            }
        }
    }
    public class PlayerStats
    {
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

    // This class handles the actual rendering and stays alive forever
    public class PersistentUI : MonoBehaviour
    {
        private TextMeshProUGUI _uiText;
        private GameObject _canvasObj;
        private GameObject _panelObj;
        private float dpsupdateTime = 0f;
        private Canvas _targetCanvas;
        private bool _isVisible = false; // Start hidden, toggle with F11
        private bool _isKeyHeld = false; // Our custom debounce

        void Start()
        {
            // 1. Create the Root Canvas
            _canvasObj = new GameObject("DPS_Overlay_Canvas");
            Object.DontDestroyOnLoad(_canvasObj);
            
            Canvas canvas = _canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Force to top

            // Essential for Unity 2022.3 UI Modules
            _canvasObj.AddComponent<GraphicRaycaster>();
            // 2. Create the Background PANEL (The Border/Frame)
            _panelObj = new GameObject("DPS_Background_Frame");
            _panelObj.transform.SetParent(_canvasObj.transform, false);

            Image frameImage = _panelObj.AddComponent<Image>();
            // A "Bronze/Gold" color to match the image metal
            frameImage.color = new Color(0.45f, 0.35f, 0.2f, 1f); 

            RectTransform panelRect = frameImage.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 0.5f);
            panelRect.anchorMax = new Vector2(1, 0.5f);
            panelRect.pivot = new Vector2(1, 0.5f);
            panelRect.anchoredPosition = new Vector2(-10, 0);
            panelRect.sizeDelta = new Vector2(220, 210);

            // Add an Outline component to give it the "raised" metal edge look
            var outline = _panelObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0.15f, 0.1f, 0.05f, 1f); // Darker shadow
            outline.effectDistance = new Vector2(2, -2);

            // 2b. Create the Inner Background (The Teal/Green area)
            GameObject innerArea = new GameObject("Inner_Area");
            innerArea.transform.SetParent(_panelObj.transform, false);

            Image innerImage = innerArea.AddComponent<Image>();
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
            _targetCanvas.enabled = false; // Start with the canvas disabled (hidden)
        }
        

        private float _nextDebugTime = 0f;
        void Update()
        {
            if (Time.time >= _nextDebugTime)
            {
                _nextDebugTime = Time.time + 2.0f;
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
                    AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalBurn, "#FFA500");
                    AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalPoison, "#800080");
                    AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalBleed, "#FF0000");
                    AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalShock, "#d9ff00");
                    AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalRoot, "#805700");
                    AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalFrost, "#00FFFF");
                    AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalCurse, "#019262");
                    AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalMinion, "#ff00f2");
                    AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalBless, "#78a70a");
                    AddClippedSegment(Plugin.Instance.LocalPlayerStats.TotalFury, "#c22f02");

                    // FILLER: If damage types don't sum to 100% (Raw damage), or rounding left a gap
                    if (remainingSlots > 0) {
                        visualBar += $"<color=#555555>{new string('█', remainingSlots)}</color>";
                    }
                    // -------------------------


                    _uiText.text = string.Join("\n", 
                        visualBar,
                        ColorText($"TOTAL DPS: {dps_Total:F1}", "#FFFFFF"),
                        ColorText(FormatLine("BURN", Plugin.Instance.LocalPlayerStats.TotalBurn), "#FFA500"),
                        ColorText(FormatLine("POISON", Plugin.Instance.LocalPlayerStats.TotalPoison), "#800080"),
                        ColorText(FormatLine("BLEED", Plugin.Instance.LocalPlayerStats.TotalBleed), "#FF0000"),
                        ColorText(FormatLine("SHOCK", Plugin.Instance.LocalPlayerStats.TotalShock), "#d9ff00"),
                        ColorText(FormatLine("ROOT", Plugin.Instance.LocalPlayerStats.TotalRoot), "#805700"),
                        ColorText(FormatLine("FROST", Plugin.Instance.LocalPlayerStats.TotalFrost), "#00FFFF"),
                        ColorText(FormatLine("CURSE", Plugin.Instance.LocalPlayerStats.TotalCurse), "#019262"),
                        ColorText(FormatLine("MINION", Plugin.Instance.LocalPlayerStats.TotalMinion), "#ff00f2"),
                        ColorText(FormatLine("BLESS", Plugin.Instance.LocalPlayerStats.TotalBless), "#78a70a"),
                        ColorText(FormatLine("FURY", Plugin.Instance.LocalPlayerStats.TotalFury), "#c22f02")
                    );
                }
                else
                {
                    // Optional: Dim the text or add "(PAUSED)" to show combat ended
                    _uiText.color = new Color(0.7f, 0.7f, 0f); // Dimmer Yellow
                    // We don't recalculate DPS here, so it stays frozen at the last value
                }
            }
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            bool ui_pressed = keyboard.pKey.isPressed;

            if (keyboard.oKey.isPressed)
            {
                Plugin.Instance.ResetMeter();
            }
            // Check for F11 key press
            if (ui_pressed && !_isKeyHeld)
            {   
                Debug.Log("[DPS] F11 Pressed: Toggling UI Visibility");
                _isKeyHeld = true;
                ToggleUIVisibility();
            }
            else if (!ui_pressed)
            {
                _isKeyHeld = false; // Unlock when user lets go of F11
            }
        }
        private void ToggleUIVisibility()
        {
            _isVisible = !_isVisible;
            _targetCanvas.enabled = _isVisible;
            
            // Physical feedback in the editor console
            Debug.Log($"[UI] Visibility set to: {_isVisible}");
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

[HarmonyPatch(typeof(RR.Game.Stats.Health), "AddDamageData")]
public static class DamageDataPatch
{
    static void Prefix(float damageValue, object damageType, int attackerID)
    {
        // 1. Get a safe string for the damage type
        string typeStr = damageType?.ToString() ?? "";

        // 2. Update the local player stats
        Debug.Log($"[DPS Meter] Damage Detected: {damageValue} of type {typeStr} from AttackerID {attackerID}");
        
        UpdatePlayerStats(damageValue, typeStr);
    }

    private static void UpdatePlayerStats(float val, string type)
    {
        // --- GLOBAL COMBAT TIMING ---
        if (Plugin.Instance.StartTime < 0) Plugin.Instance.StartTime = Time.time;
        Plugin.Instance.LastHitTime = Time.time;

        // --- STAT UPDATING ---
        var stats = Plugin.Instance.LocalPlayerStats;
        stats.TotalDamage += val;

        if (type.Contains("Burn")) stats.TotalBurn += val;
        else if (type.Contains("Poison")) stats.TotalPoison += val;
        else if (type.Contains("Bleed")) stats.TotalBleed += val;
        else if (type.Contains("Shock")) stats.TotalShock += val;
        else if (type.Contains("Curse")) stats.TotalCurse += val;
        else if (type.Contains("Root")) stats.TotalRoot += val;
        else if (type.Contains("Freeze") || type.Contains("Chill") || type.Contains("Frost")) stats.TotalFrost += val;
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
                    Debug.Log($"[DPS Meter] Attacker {stats.name} dealt {damageValue} damage to {victim?.name}");
                    if (Plugin.Instance.StartTime < 0) Plugin.Instance.StartTime = Time.time;
                    Plugin.Instance.LastHitTime = Time.time;
                    var fields = Traverse.Create(stats).Fields();
                    Debug.Log($"--- Inspecting StatsManager ({fields.Count} fields found) ---");
                    foreach (var fieldName in fields)
                    {
                        var val = Traverse.Create(stats).Field(fieldName).GetValue();
                        Debug.Log($"Field: {fieldName} | Value: {val}");
                    }
                }
                else if (stats.IsChampionMinion && !onlyForUI)
                {
                    // 2. Use our Hooked SpawnerActorID
                    int ownerActorID = stats.SpawnerActorID;

                    if (ownerActorID != -1) // -1 is InvalidActorID
                    {
                        // 3. Check if this is the local player's minion
                        if (stats.IsChampion)
                        {
                            Plugin.Instance.LocalPlayerStats.TotalMinion += damageValue;
                            Debug.Log($"[DPS] Distilled {damageValue} Minion Damage from Actor {stats.ActorID}");
                        }
                    }
                }
                else if (stats.IsChampion && !onlyForUI)
                {
                    // Track BLESSED and FURY DMG for local player
                    Debug.Log($"[DEBUG BLEED] Base: {PerkDatabase.Instance.BleedBaseDamage} | From Hit: {damageValue * PerkDatabase.Instance.BleedHitDamagePercentage / 100f} | Multiplier: {stats.Bleed.DamageMultiplierPCT}% | FINAL TOTAL: {(PerkDatabase.Instance.BleedBaseDamage+ (damageValue * PerkDatabase.Instance.BleedHitDamagePercentage / 100f))* ( stats.Bleed.DamageMultiplierPCT / 100f)}");
                    Debug.Log($"[DPS Meter] Attacker {stats.name} has {stats.Attack.PhysicalPower} Physical Power and dealt {damageValue} damage has bleed multiplier of {stats.Bleed.IsActiveByUpgraded}");

                    switch (userAction)
                    {
                        case UserAction.Attack:
                        {
                            if (damageDesc.blessedAttack)
                            {
                                float multiplier = 1f + (stats.Bless.EmpoweredAttackDamageIncrementPCT / 100f);
                                float originalDamage = damageValue / multiplier;
                                Plugin.Instance.LocalPlayerStats.TotalBless += damageValue - originalDamage;
                                Debug.Log($"[DPS] Credited {stats.Bless.EmpoweredAttackDamageIncrementPCT}% Blessed Damage");

                            }
                            if (damageDesc.furyAttack)
                            {   
                                float multiplier = 1f + (stats.Fury.BoostedAttackDamageIncPCT.Value / 100f);
                                float originalDamage = damageValue / multiplier;
                                Plugin.Instance.LocalPlayerStats.TotalFury += damageValue - originalDamage;
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
           
            if (calcForUI || victim == null || __instance == null) return;

            if (victim.IsFrozen)
            {
                var chill = __instance.GetComponent<RR.Game.Stats.Chill>();
                
                if (chill != null)
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

    // sending to DPS to lobby
    /// <summary>
    /// 
    /// </summary>

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StatsManager), "SpawnerActorID", MethodType.Setter)]
        public static bool Prefix_SetSpawnerID(StatsManager __instance, int value)
        {
            if (value == INVALID_ID)
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Summon), "InitMinionSpawned")]
        public static void Prefix_InitMinionSpawned(Summon __instance, NetworkObject obj)
        {
            // Access private _stats via Traverse
            var summonerStats = Traverse.Create(__instance).Field<StatsManager>("_stats").Value;

            if (summonerStats != null && obj.TryGetComponent<StatsManager>(out var minionStats))
            {
                int ownerID = summonerStats.ActorID;
                
                if (ownerID != INVALID_ID)
                {
                    minionStats.SpawnerActorID = ownerID;
                }
            }
        }
    }
}
