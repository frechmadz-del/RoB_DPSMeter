using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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

namespace BlackveilDpsMeter
{
    [BepInPlugin("com.gemini.dpsmeter", "RoB Persistent DPS", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public int AttackerID = -1;
        public float TotalDamage = 0f;
        public float TotalMinion = 0f;
        public float TotalBurn = 0f;
        public float TotalRoot = 0f;
        public float TotalPoison = 0f;
        public float TotalBleed = 0f;   
        public float TotalShock = 0f;
        public float TotalChill = 0f;
        public float TotalCurse = 0f;
        public float ActiveCombatTime = 0f; // New: Tracks accumulated combat seconds
        public float StartTime = -1f;
        public float LastHitTime = -1f; // out of fight timer
        public static bool IsSyncingDps = false;

        void Awake()
        {
            Instance = this;
            
            // 1. Force Harmony to patch the game's own Update loop as a heartbeat
            var harmony = new Harmony("com.gemini.dpsmeter");
            harmony.PatchAll();

            // 2. Create a hidden object that survives scene changes
            var tracker = new GameObject("DPS_Global_Bus");
            tracker.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(tracker);
            tracker.AddComponent<PersistentUI>();

            // Subscribe to Unity's sceneLoaded event
            SceneManager.sceneLoaded += OnSceneLoaded;

            Logger.LogInfo("Mod Injected: Hidden Bus & Harmony Patches Active.");
        }
        public void ResetMeter()
        {
            Plugin.Instance.TotalDamage = 0f;
            Plugin.Instance.TotalBurn = 0f;
            Plugin.Instance.TotalRoot = 0f;
            Plugin.Instance.TotalPoison = 0f;
            Plugin.Instance.TotalBleed = 0f;
            Plugin.Instance.TotalShock = 0f;
            Plugin.Instance.TotalChill = 0f;
            Plugin.Instance.TotalCurse = 0f;
            Plugin.Instance.TotalMinion = 0f;
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
        public class PlayerStats
        {
            public string Name = "Unknown"; // Added to store the Username
            public float TotalDamage;
            public float TotalBurn;
            public float TotalRoot;
            public float TotalPoison;
            public float TotalBleed;
            public float TotalShock;
            public float TotalCurse;
        }

// This is the variable the compiler was looking for
        public Dictionary<int, PlayerStats> RemotePlayers = new Dictionary<int, PlayerStats>();

        // A simple class to hold remote player stats (you can expand this as needed)
        public void UpdateRemoteStats(int id, string name, string[] values)
        {
            if (!RemotePlayers.ContainsKey(id))
            {
                RemotePlayers[id] = new PlayerStats();
            }

            var stats = RemotePlayers[id];
            stats.Name = name; // Link the name to the ID

            // Parse the values (Assignment, not addition)
            float.TryParse(values[0], out stats.TotalDamage);
            float.TryParse(values[1], out stats.TotalBurn);
            float.TryParse(values[2], out stats.TotalRoot);
            float.TryParse(values[3], out stats.TotalPoison);
            float.TryParse(values[4], out stats.TotalBleed);
            float.TryParse(values[5], out stats.TotalShock);
            float.TryParse(values[6], out stats.TotalCurse);
        }
    }

    // This class handles the actual rendering and stays alive forever
    public class PersistentUI : MonoBehaviour
    {
        private Text _uiText;
        private GameObject _canvasObj;
        private GameObject _panelObj;

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

            // 2. Create the Background PANEL (Opaque)
            _panelObj = new GameObject("DPS_Background_Panel");
            _panelObj.transform.SetParent(_canvasObj.transform, false);

            Image panelImage = _panelObj.AddComponent<Image>();
            
            // Set Color: Black (0,0,0) with 80% Opacity (0.8f Alpha)
            // If you want it 100% opaque, set alpha to 1.0f.
            panelImage.color = new Color(0f, 0f, 0f, 0.8f); 

            // Anchoring and Scaling the Panel (Right-Center)
            RectTransform panelRect = panelImage.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 0.5f); // Right side, Middle height
            panelRect.anchorMax = new Vector2(1, 0.5f);
            panelRect.pivot = new Vector2(1, 0.5f);
            panelRect.anchoredPosition = new Vector2(-10, 0); // 10 pixels in from the edge
            panelRect.sizeDelta = new Vector2(220, 170); // FIXED SIZE (Width, Height)

            // 3. Create the TEXT Display (As child of the panel)
            GameObject textObj = new GameObject("DPS_Text_Display");
            textObj.transform.SetParent(_panelObj.transform, false);

            _uiText = textObj.AddComponent<Text>();
            
            // Set Font: Using Builtin Arial for maximum compatibility
            _uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _uiText.fontSize = 15;
            _uiText.color = Color.yellow;
            
            // Alignment: Middle-Right looks best when pinned to the right side
            _uiText.alignment = TextAnchor.UpperLeft; 
            _uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _uiText.verticalOverflow = VerticalWrapMode.Overflow;

            // Positioning Text within the Panel
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero; // Stretch to parent panel
            textRect.anchorMax = Vector2.one;

            // ADDING PADDING:
            // Left offset = 15 (Push text 15 pixels away from left edge)
            // Right offset = 10 (Push numbers slightly in from right edge)
            textRect.offsetMin = new Vector2(15, 0); 
            textRect.offsetMax = new Vector2(-10, 0);  
        }

        void Update()
        {
            // Safety check: The UI will only update if the plugin successfully captured the start time
            if (Plugin.Instance.StartTime > 0)
            {
                    // Check if we are "In Combat" (Hit within the last 1.0 seconds)
                    // 1. Determine if we are currently hitting things
                    bool inCombat = (Time.time - Plugin.Instance.LastHitTime) <= 1.0f;

                    // 2. Only tick the clock forward if we are in combat
                    if (inCombat)
                    {
                        Plugin.Instance.ActiveCombatTime += Time.deltaTime;
                    }

                    float displayTime = Mathf.Max(0.1f, Plugin.Instance.ActiveCombatTime);

                    if (displayTime > 0.1f)
                    {           
                        
                        float dps_Total = Plugin.Instance.TotalDamage / displayTime;
                        // Use a small helper function to keep the code clean
                        string FormatLine(string label, float val) => 
                            $"{label} DPS: {val / displayTime:F1} ({(val / displayTime / dps_Total) * 100:F1}%)";
                        // Use a small helper function for colors
                        string ColorText(string text, string hex) => $"<color={hex}>{text}</color>";
                        // --- CLIPPED BAR LOGIC ---
                        int barWidth = 18; // Total character slots
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
                        AddClippedSegment(Plugin.Instance.TotalBurn, "#FFA500");
                        AddClippedSegment(Plugin.Instance.TotalPoison, "#800080");
                        AddClippedSegment(Plugin.Instance.TotalBleed, "#FF0000");
                        AddClippedSegment(Plugin.Instance.TotalShock, "#d9ff00");
                        AddClippedSegment(Plugin.Instance.TotalRoot, "#805700");
                        AddClippedSegment(Plugin.Instance.TotalChill, "#00FFFF");
                        AddClippedSegment(Plugin.Instance.TotalCurse, "#019262");
                        AddClippedSegment(Plugin.Instance.TotalMinion, "#ff00f2");

                        // FILLER: If damage types don't sum to 100% (Raw damage), or rounding left a gap
                        if (remainingSlots > 0) {
                            visualBar += $"<color=#555555>{new string('█', remainingSlots)}</color>";
                        }
                        // -------------------------


                        _uiText.text = string.Join("\n", 
                            visualBar,
                            ColorText($"TOTAL DPS: {dps_Total:F1}", "#FFFFFF"),
                            ColorText(FormatLine("BURN", Plugin.Instance.TotalBurn), "#FFA500"),
                            ColorText(FormatLine("POISON", Plugin.Instance.TotalPoison), "#800080"),
                            ColorText(FormatLine("BLEED", Plugin.Instance.TotalBleed), "#FF0000"),
                            ColorText(FormatLine("SHOCK", Plugin.Instance.TotalShock), "#d9ff00"),
                            ColorText(FormatLine("ROOT", Plugin.Instance.TotalRoot), "#805700"),
                            ColorText(FormatLine("FROST", Plugin.Instance.TotalChill), "#00FFFF"),
                            ColorText(FormatLine("CURSE", Plugin.Instance.TotalCurse), "#019262"),
                            ColorText(FormatLine("MINION", Plugin.Instance.TotalMinion), "#ff00f2")
                        );
                    }
                    else
                    {
                        // Optional: Dim the text or add "(PAUSED)" to show combat ended
                        _uiText.color = new Color(0.7f, 0.7f, 0f); // Dimmer Yellow
                        // We don't recalculate DPS here, so it stays frozen at the last value
                    }
            }

            if (UnityEngine.InputSystem.Keyboard.current.f10Key.wasPressedThisFrame)
            {
                Plugin.Instance.ResetMeter();
            }
            // Sync with lobby (for host and clients)
            if (PlayerManager.Instance != null && PlayerManager.Instance.LocalChampion != null)
            {
                var localChamp = PlayerManager.Instance.LocalChampion;
                var runner = localChamp.Runner; // Get the NetworkRunner from the champion
                float nextSyncTime = 0f; // Initialize next sync time     

                if (runner.IsServer && runner.IsRunning && Time.time > nextSyncTime)
                {
                    nextSyncTime = Time.time + 2.0f;

                    foreach (var player in PlayerManager.Instance.GetPlayers()) // Or your player list
                    {
                        Debug.Log($"[DPS] Host sync loop running for {player.UserName} player.");
                        // Trigger the RPC for each player
                        // Our Patch (below) will intercept this and attach that specific player's stats
                        PlayerManager.Instance.RPC_Handle_SetUserData_All(
                            player.Object.InputAuthority, 
                            player.UserName, 
                            player.ProfileUUID
                        );
                    }
                }
            }
            // Inside your Update loop

        }
    }

    // patch to fetch dmgh remote
    [HarmonyPatch(typeof(RR.Game.Stats.Health), "AddDamageData")]
    public static class DamageDataPatch
    {
        static void Prefix(float damageValue, object damageType, int attackerID)
        {

            string typeStr = damageType?.ToString() ?? "";

            // 2. LOGIC FOR LOCAL PLAYER (YOU)
            if (attackerID == 0)
            {
                Debug.Log($"[DPS Meter] Local Hit Detected: {damageValue} damage of type {typeStr} time {Plugin.Instance.StartTime}");
                UpdateLocalStats(damageValue, typeStr);
            }
            // 3. LOGIC FOR REMOTE PLAYERS (UP TO 2 OTHERS)
            else if(attackerID == 1 || attackerID == 2)
            {
                // We check if we are already tracking this player, 
                // or if we have room to start tracking a new remote player (max 2)
                if (Plugin.Instance.RemotePlayers.ContainsKey(attackerID) || Plugin.Instance.RemotePlayers.Count < 2)
                {
                    UpdateRemoteStatsInternal(attackerID, damageValue, typeStr);
                }
            }
        }

        private static void UpdateLocalStats(float val, string type)
        {
            if (Plugin.Instance.StartTime < 0) Plugin.Instance.StartTime = Time.time;
            Plugin.Instance.LastHitTime = Time.time;
            
            Plugin.Instance.TotalDamage += val;

            if (type.Contains("Burn")) Plugin.Instance.TotalBurn += val;
            else if (type.Contains("Poison")) Plugin.Instance.TotalPoison += val;
            else if (type.Contains("Bleed")) Plugin.Instance.TotalBleed += val;
            else if (type.Contains("Shock")) Plugin.Instance.TotalShock += val;
            else if (type.Contains("Curse")) Plugin.Instance.TotalCurse += val;
            else if (type.Contains("Root")) Plugin.Instance.TotalRoot += val;
            else if (type.Contains("Freeze") || type.Contains("Chill")) Plugin.Instance.TotalChill += val;
        }

        private static void UpdateRemoteStatsInternal(int id, float val, string type)
        {
            // Ensure the PlayerStats object exists for this ID
            if (!Plugin.Instance.RemotePlayers.ContainsKey(id))
            {
                Plugin.Instance.RemotePlayers[id] = new Plugin.PlayerStats();
                Debug.Log($"[DPS Meter] Now tracking Remote Player ID: {id}");
            }

            var stats = Plugin.Instance.RemotePlayers[id];
            stats.TotalDamage += val;

            if (type.Contains("Burn")) stats.TotalBurn += val;
            else if (type.Contains("Poison")) stats.TotalPoison += val;
            else if (type.Contains("Bleed")) stats.TotalBleed += val;
            else if (type.Contains("Shock")) stats.TotalShock += val;
            else if (type.Contains("Curse")) stats.TotalCurse += val;
            else if (type.Contains("Root")) stats.TotalRoot += val;
        }
    }

    [HarmonyPatch(typeof(Attack), "ModifyDamageWithModifierAndCritical")]
    public class MinionDamageHook
    {
        static void Postfix(object __instance, StatsManager victim, ref DamageDescriptor damageDesc, bool onlyForUI)
        {
            if (onlyForUI) return;

            // Since the method is in the 'Attack' class, '__instance' refers to the Attack object.
            // We need to find the stats associated with this attack.
            // Based on your original code, it looks like 'Attack' has a private field called '_stats'.
            
            var stats = Traverse.Create(__instance).Field("_stats").GetValue<StatsManager>();

            if (stats != null && stats.IsChampionMinion)
            {
                float finalDmg = damageDesc.damageValue;
                if (Plugin.Instance.StartTime < 0) Plugin.Instance.StartTime = Time.time;
                    Plugin.Instance.TotalMinion += finalDmg;
                Debug.Log($"[DPS Meter] Minion {stats.name} dealt {finalDmg} damage to {victim?.name}");
            }
            else if (damageDesc.damageValue != 0)
            {
                if (Plugin.Instance.StartTime < 0) Plugin.Instance.StartTime = Time.time;
                Debug.Log($"[DPS Meter] hit detected. starting dps tracking.");
            }
        }
    }
    [HarmonyPatch(typeof(StatsManager), "CalculateMyDamageAgainst")]
    public class FrozenDamageHook
    {

        [HarmonyPostfix]
        static void Postfix(StatsManager __instance, StatsManager victim, ref DamageDescriptor dmgDesc, bool calcForUI)
        {
            if (calcForUI || victim == null) return;

            // To find the delta, we manually check the multiplier logic 
            // used in the original method.
            if (victim.IsFrozen)
            {
                Debug.Log($"[DPS Meter] Victim {victim.name} is frozen. Checking for Chill multiplier...");
                // Access the 'Chill' component from the character that dealt the damage
                var chill = __instance.GetComponent<RR.Game.Stats.Chill>();
                Debug.Log($"[DPS Meter] Hooking into: {__instance.name}");

                if (chill != null && victim.IsFrozen)
                {
                    float multiplier = chill.DamageMultiplierAgainstFrozenTarget;
                    Debug.Log($"[DPS Meter] Checking {multiplier} against frozen target.");
                    
                    // The damage currently in dmgDesc.damageValue ALREADY has the multiplier.
                    // Formula: OriginalDamage = FinalDamage / Multiplier
                    float damageBeforeFrozen = dmgDesc.damageValue / multiplier;
                    float frozenDelta = dmgDesc.damageValue - damageBeforeFrozen;

                    if (frozenDelta > 0)
                    {
                        Debug.Log($"[DPS Meter] Frozen Bonus Damage: {frozenDelta} (Total: {dmgDesc.damageValue})");
                        // Add frozenDelta to your "Frozen Damage" category in the meter
                        Plugin.Instance.TotalChill += frozenDelta;
                    }
                }
            }
        }
    }

    // sending to DPS to lobby

    [HarmonyPatch(typeof(RR.PlayerManager), "RPC_Handle_SetUserData_All")]
   public static class DpsSyncPatch
    {
        private const string Separator = "«DPS»";

        static bool Prefix(ref string userName, PlayerRef playerRef, NetworkBehaviour __instance)
        {
            var runner = __instance.Runner;
            if (runner == null) return true;

            // --- HOST: Pack data for the specific playerRef ---
            if (runner.IsServer && !userName.Contains(Separator))
            {
                Plugin.PlayerStats statsToSync = null;

                // Are we packing the Host's own stats or a teammate's stats?
                if (playerRef == runner.LocalPlayer)
                {
                    statsToSync = new Plugin.PlayerStats {
                        TotalDamage = Plugin.Instance.TotalDamage,
                        TotalBurn = Plugin.Instance.TotalBurn,
                        TotalRoot = Plugin.Instance.TotalRoot,
                        TotalPoison = Plugin.Instance.TotalPoison,
                        TotalBleed = Plugin.Instance.TotalBleed,
                        TotalShock = Plugin.Instance.TotalShock,
                        TotalCurse = Plugin.Instance.TotalCurse
                    };
                }
                else if (Plugin.Instance.RemotePlayers.TryGetValue(playerRef.PlayerId, out var remoteStats))
                {
                    statsToSync = remoteStats;
                }

                if (statsToSync != null)
                {
                    string dataPacket = string.Join(",", 
                        (int)statsToSync.TotalDamage, (int)statsToSync.TotalBurn,
                        (int)statsToSync.TotalRoot, (int)statsToSync.TotalPoison,
                        (int)statsToSync.TotalBleed, (int)statsToSync.TotalShock,
                        (int)statsToSync.TotalCurse);

                    userName = $"{userName}{Separator}{dataPacket}";
                     Debug.Log($"[DPS] Host Sending {userName}");
                }
            }

            // --- CLIENT/RECEIVER: Unpack and Update Dictionary ---
            if (userName.Contains(Separator))
            {
                Plugin.IsSyncingDps = true;
                try 
                {
                    string[] mainParts = userName.Split(new[] { Separator }, System.StringSplitOptions.None);
                    if (mainParts.Length > 1)
                    {
                        string[] values = mainParts[1].Split(',');
                        if (values.Length >= 7)
                        {
                            string cleanName = mainParts[0]; // This is the player.UserName
                             Debug.Log($"[DPS] Client Receiving Data for Player {cleanName}: {string.Join(",", values)}");
                            // Use the method you wrote earlier to update your local UI storage
                            Plugin.Instance.UpdateRemoteStats(playerRef.PlayerId, cleanName, values);
                        }
                    }
                    userName = mainParts[0]; // Strip the junk data for the UI
                }
                finally { Plugin.IsSyncingDps = false; }
                
                return false; // Skip the original GameManager logic
            }

            return true;
        }
    }
    [HarmonyPatch(typeof(GameManager), "HandleEvent_UserDataReceived")]
    public static class SilenceGameManagerPatch
    {
        // We use a Prefix to "Gatekeep" the original method
        static bool Prefix()
        {
            // If our sync flag is active, we return 'false' to skip the original method
            // This stops the "Player Joined, Spawning..." log and logic.
            if (Plugin.IsSyncingDps) 
            {
                return false; 
            }
            return true;
        }
    }
}