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
using UnityEngine.UIElements.UIR;
using RR;
using Fusion;
using TMPro;
using System.Linq;

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
        private bool _hasInitialized = false;

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
            if (this == null || !this.gameObject.activeInHierarchy)
            {
                return;
            }
            if (!_hasInitialized)
            {
                StartCoroutine(WaitForLevelLoad());
                Logger.LogInfo($"First scene loaded: {scene.name}. DPS Meter is now active.");
            }

            _hasInitialized = true;
        }
        private IEnumerator WaitForLevelLoad()
        {
            // Wait until the game's player system is actually ready
            while (PlayerManager.Instance == null || PlayerManager.Instance.GetPlayers().Count == 0)
            {
                yield return new WaitForSeconds(1.0f);
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
        private TMP_SpriteAsset _gameSpriteAsset = null;

        private float nextSyncTime = 0f;
        private bool _spritesLinked = false;
        private GameObject _canvasObj;
        private GameObject _panelObj;
        // We change the dictionary to store TextMeshPro components
        private Dictionary<string, TextMeshProUGUI> _dpsTextRefs = new Dictionary<string, TextMeshProUGUI>();

        private static readonly Dictionary<string, string> StatIconMap = new Dictionary<string, string>
        {
            { "burn", "PerkEffect_Icon_Burn" },
            { "poison", "PerkEffect_Icon_Poison" },
            { "bleed", "PerkEffect_Icon_Bleed" },
            { "shock", "PerkEffect_Icon_Shock" },
            { "frozen", "PerkEffect_Icon_Chilled" }, // "frozen" in code -> "Chilled" in assets
            { "root", "PerkEffect_Icon_Root" },
            { "summon", "PerkEffect_Icon_Summon" },
            { "curse", "Stats_Icon_Curse" },
        };

        void Start()
        {
            // 1. Setup UI Root
            _canvasObj = new GameObject("DPS_Overlay_Canvas");
            Object.DontDestroyOnLoad(_canvasObj);
            _canvasObj.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            _canvasObj.AddComponent<GraphicRaycaster>();

            _panelObj = new GameObject("DPS_Background_Panel");
            _panelObj.transform.SetParent(_canvasObj.transform, false);
            _panelObj.AddComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.85f);

            RectTransform panelRect = _panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(1, 0.5f);
            panelRect.anchoredPosition = new Vector2(-10, 0);
            panelRect.sizeDelta = new Vector2(260, 240); 

            // 2. Layout Group (Keeps rows tidy)
            VerticalLayoutGroup vlg = _panelObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 4;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;

            

            // 3. Create the Rows using the game's Sprite Names
            // These icon names must match the game's Sprite Asset
            CreateProperRow("TOTAL", "#FFFFFF", ""); 
            CreateProperRow("BURN", "#FFA500", "burn");
            CreateProperRow("POISON", "#800080", "poison");
            CreateProperRow("BLEED", "#FF0000", "bleed");
            CreateProperRow("SHOCK", "#d9ff00", "shock");
            CreateProperRow("ROOT", "#805700", "root");
            CreateProperRow("FROST", "#00FFFF", "frozen");
            CreateProperRow("CURSE", "#019262", "curse");
            CreateProperRow("MINION", "#ff00f2", "summon");

        }
        // public Sprite GetStatSprite(string key)
        // {
        //     if (!StatIconMap.TryGetValue(key.ToLower(), out string texName))
        //     {
        //         Debug.LogWarning($"[DPS Meter] No icon mapping found for key: {key}");
        //         return null;
        //     }

        //     // Search for the specific texture by name
        //     Texture2D tex = null;
        //     var allTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
        //     foreach (var t in allTextures)
        //     {
        //         if (t.name == texName)
        //         {
        //             tex = t;
        //             break;
        //         }
        //     }

        //     if (tex != null)
        //     {
        //         // Create the sprite from the found texture
        //         return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        //     }

        //     return null;
        // }

        private Sprite GetStatSprite(string iconName)
        {
            // Exact names from your previous log dump
            string texName = iconName.ToLower() switch
            {
                "burn" => "PerkEffect_Icon_Burn",
                "poison" => "PerkEffect_Icon_Poison",
                "bleed" => "PerkEffect_Icon_Bleed",
                "shock" => "PerkEffect_Icon_Shock",
                "frozen" => "PerkEffect_Icon_Chilled",
                "root" => "PerkEffect_Icon_Root",
                "curse" => "Stats_Icon_Curse",
                _ => "StatType_Default" 
            };

            // Try to find the texture
            Texture2D tex = Resources.FindObjectsOfTypeAll<Texture2D>()
                            .FirstOrDefault(t => t.name.Equals(texName, System.StringComparison.OrdinalIgnoreCase));

            // If the specific icon failed, try to find ANY icon as a fallback
            if (tex == null) tex = Resources.FindObjectsOfTypeAll<Texture2D>().FirstOrDefault(t => t.name == "StatType_Default");

            if (tex != null)
            {
                // IMPORTANT: Create the sprite with the correct dimensions
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            return null;
}

        private void CreateProperRow(string key, string hex, string iconName)
        {
            // 1. Main Row Container
            GameObject rowObj = new GameObject(key + "_Row");
            rowObj.transform.SetParent(_panelObj.transform, false);

            // 2. Layout Group (The "Glue" that fixes the overlapping)
            var layout = rowObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;  // Let the text expand
            layout.childControlHeight = true; // Match row height
            layout.childForceExpandWidth = false;

            // 3. Create the Icon
            GameObject iconGo = new GameObject(key + "_Icon");
            iconGo.transform.SetParent(rowObj.transform, false);
            
            var img = iconGo.AddComponent<UnityEngine.UI.Image>();
            Sprite iconSprite = GetStatSprite(iconName);
            
            if (iconSprite != null) {
                img.sprite = iconSprite;
            } else {
                img.color = new Color(0, 0, 0, 0); // Hide the image if sprite is missing
            }

            var layoutElem = iconGo.AddComponent<UnityEngine.UI.LayoutElement>();
            layoutElem.minWidth = 20;
            layoutElem.minHeight = 20;
            layoutElem.preferredWidth = 20;
            layoutElem.preferredHeight = 20;

            // You can still keep this for the base transform size
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(20, 20);

            // 4. Create the Text
            GameObject textGo = new GameObject(key + "_Text");
            textGo.transform.SetParent(rowObj.transform, false);
            
            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 14; // Slightly smaller to fit your UI better
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.enableWordWrapping = false;
            
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
                tmp.color = c;

            _dpsTextRefs[key] = tmp;
        }

        void Update()
        {
            if (Plugin.Instance.StartTime > 0)
            { 
                
                bool inCombat = (Time.time - Plugin.Instance.LastHitTime) <= 1.0f;
                if (inCombat) Plugin.Instance.ActiveCombatTime += Time.deltaTime;

                float displayTime = Mathf.Max(0.1f, Plugin.Instance.ActiveCombatTime);
                float dps_Total = Plugin.Instance.TotalDamage / displayTime;

                // NEW: Lazy-link the sprites once combat starts
                if (!_spritesLinked) 
                {
                    _spritesLinked = TryLinkGameSprites();
                }

                void UpdateRow(string key, float totalDmg)
                {
                    if (!_dpsTextRefs.TryGetValue(key, out TextMeshProUGUI tmp)) return;

                    float currentDps = totalDmg / displayTime;
                    float pct = (dps_Total > 0.1f) ? (currentDps / dps_Total) * 100f : 0f;

                    // Prepend the icon tag stored in the GameObject name
                    tmp.text = $"{tmp.gameObject.name}{key}: {currentDps:F1} ({pct:F1}%)";
                }

                // Refresh all lines
                _dpsTextRefs["TOTAL"].text = $"TOTAL: {dps_Total:F1}";
                UpdateRow("BURN", Plugin.Instance.TotalBurn);
                UpdateRow("POISON", Plugin.Instance.TotalPoison);
                UpdateRow("BLEED", Plugin.Instance.TotalBleed);
                UpdateRow("SHOCK", Plugin.Instance.TotalShock);
                UpdateRow("ROOT", Plugin.Instance.TotalRoot);
                UpdateRow("FROST", Plugin.Instance.TotalChill);
                UpdateRow("CURSE", Plugin.Instance.TotalCurse);
                UpdateRow("MINION", Plugin.Instance.TotalMinion);
            }
        

            if (UnityEngine.InputSystem.Keyboard.current.f10Key.wasPressedThisFrame)
            {
                Plugin.Instance.ResetMeter();
            }
            if (UnityEngine.InputSystem.Keyboard.current.f9Key.wasPressedThisFrame)
                {
                    Debug.Log("[DPS Meter] Scanning for Icon Textures...");
                    Texture2D[] allTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
                    foreach (var tex in allTextures)
                    {
                        // Filter for common icon naming conventions
                        if (tex.name.ToLower().Contains("icon") || tex.name.ToLower().Contains("stat") || tex.name.ToLower().Contains("sprite"))
                        {
                            Debug.Log($"[DPS Meter] Found Texture: {tex.name} ({tex.width}x{tex.height})");
                        }
                    }
                }
            // Sync with lobby (for host and clients)
            if (PlayerManager.Instance != null && PlayerManager.Instance.LocalChampion != null)
            {
                var localChamp = PlayerManager.Instance.LocalChampion;
                var runner = localChamp.Runner; // Get the NetworkRunner from the champion
                if (runner != null && runner.IsServer && runner.IsRunning)
                {
                    // 2. Only proceed if current time has passed the target sync time
                    if (Time.time > nextSyncTime)
                    {
                        // 3. Immediately set the next target time (Current time + 2 seconds)
                        nextSyncTime = Time.time + 2.0f;

                        foreach (var player in PlayerManager.Instance.GetPlayers())
                        {
                            if (player == null || player.Object == null) continue;

                            Debug.Log($"[DPS] Host sync loop running for {player.UserName}");
                            
                            // Trigger the RPC
                            PlayerManager.Instance.RPC_Handle_SetUserData_All(
                                player.Object.InputAuthority, 
                                player.UserName, 
                                player.ProfileUUID
                            );
                        }
                    }
                }
            }
            // Inside your Update loop

        }
        private bool TryLinkGameSprites()
        {
            // 1. Try to find any SpriteAsset directly in memory (most reliable for Addressables)
            TMP_SpriteAsset[] allAssets = Resources.FindObjectsOfTypeAll<TMP_SpriteAsset>();
            
            foreach (var asset in allAssets)
            {
                // Ignore the empty default one; we want the one with the game's icons
                if (asset.name != "Default Sprite Asset" && asset.spriteCharacterTable.Count > 0)
                {
                    _gameSpriteAsset = asset;
                    ApplySpriteAssetToUI();
                    Debug.Log($"[DPS Meter] FOUND Sprite Asset in memory: {asset.name} (Icons: {asset.spriteCharacterTable.Count})");
                    return true;
                }
            }

            // 2. Fallback: Search through ALL TextMeshPro components if the direct search failed
            var allTMP = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var ui in allTMP)
            {
                if (ui.spriteAsset != null && ui.spriteAsset.name != "Default Sprite Asset")
                {
                    _gameSpriteAsset = ui.spriteAsset;
                    ApplySpriteAssetToUI();
                    Debug.Log($"[DPS Meter] FOUND Sprite Asset via UI Element: {ui.spriteAsset.name}");
                    return true;
                }
            }

            return false;
        }

        private void ApplySpriteAssetToUI()
        {
            foreach (var row in _dpsTextRefs.Values)
            {
                row.spriteAsset = _gameSpriteAsset;
                // Force a refresh of the text to re-parse the <sprite> tag
                row.SetAllDirty();
            }
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