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
using System.Linq;
using TMPro;

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
        public float TotalFrost = 0f;
        public float TotalCurse = 0f;
        public float TotalBless = 0f;
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
            tracker.AddComponent<LeaderboardUI>();


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
            Plugin.Instance.TotalFrost = 0f;
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
            public float TotalPoison;
            public float TotalBleed;
            public float TotalShock;
            public float TotalRoot;
            public float TotalFrost;
            public float TotalCurse;
            public float TotalMinion;
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
            float.TryParse(values[7], out stats.TotalFrost);
            float.TryParse(values[8], out stats.TotalMinion);

        }
    }

    // This class handles the actual rendering and stays alive forever
    public class PersistentUI : MonoBehaviour
    {
        private TextMeshProUGUI _uiText;
        private GameObject _canvasObj;
        private GameObject _panelObj;
        private float nextSyncTime = 0f; // Initialize next sync time
        private float dpsupdateTime = 0f;

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

            // Use TextMeshProUGUI instead of legacy Text
            _uiText = textObj.AddComponent<TextMeshProUGUI>();

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

            // Positioning Text within the Panel
            RectTransform textRect = _uiText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 0); 
            textRect.offsetMax = new Vector2(-10, 0);
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
                // 1. Sync the data from the dictionary back to the Plugin totals
                RepopulateFromRemote();

                float displayTime = Mathf.Max(0.1f, Plugin.Instance.ActiveCombatTime);

                if (displayTime > 0.1f)
                {           
                    
                    float dps_Total = Plugin.Instance.TotalDamage / displayTime;
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
                    AddClippedSegment(Plugin.Instance.TotalBurn, "#FFA500");
                    AddClippedSegment(Plugin.Instance.TotalPoison, "#800080");
                    AddClippedSegment(Plugin.Instance.TotalBleed, "#FF0000");
                    AddClippedSegment(Plugin.Instance.TotalShock, "#d9ff00");
                    AddClippedSegment(Plugin.Instance.TotalRoot, "#805700");
                    AddClippedSegment(Plugin.Instance.TotalFrost, "#00FFFF");
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
                        ColorText(FormatLine("FROST", Plugin.Instance.TotalFrost), "#00FFFF"),
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

                if (runner != null && runner.IsServer)
                {

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
        public void RepopulateFromRemote()
            {
                // 1. Find your entry in the dictionary by name
                var manager = GameObject.FindObjectOfType<PlayerManager>();
                if (manager == null || manager.LocalPlayer == null) return;
                string myName = manager.LocalPlayer.UserName;
                var myData = Plugin.Instance.RemotePlayers.Values.FirstOrDefault(p => p.Name == myName);

                if (myData != null)
                {
                    // 2. Directly copy the totals back into the main Plugin instance
                    Plugin.Instance.TotalBurn = myData.TotalBurn;
                    Plugin.Instance.TotalPoison = myData.TotalPoison;
                    Plugin.Instance.TotalBleed = myData.TotalBleed;
                    Plugin.Instance.TotalShock = myData.TotalShock;
                    Plugin.Instance.TotalRoot = myData.TotalRoot;
                    Plugin.Instance.TotalFrost = myData.TotalFrost;
                    Plugin.Instance.TotalCurse = myData.TotalCurse;
                    Plugin.Instance.TotalMinion = myData.TotalMinion;

                    // Ensure the main total stays in sync too
                    Plugin.Instance.TotalDamage = myData.TotalDamage;
                }
                else
                {
                    Debug.LogWarning($"[DPS] No matching entry found in RemotePlayers for {myName}");
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

    public class LeaderboardUI : MonoBehaviour
    {
    private class PlayerBarRefs
        {
        public GameObject Root;
        public Image BarFill;
        public TextMeshProUGUI InfoText;
        }
    private GameObject _leaderboardPanel;
    private Dictionary<int, PlayerBarRefs> _playerBars = new Dictionary<int, PlayerBarRefs>();
    private bool _isInitialized = false;

    private void Update()
    {
        if (!_isInitialized)
        {
            var pm = RR.PlayerManager.Instance;
            if (pm == null || pm.GetPlayers().Count <= 1) 
            {
                return; // Stay uninitialized while solo
            }

            GameObject canvasObj = GameObject.Find("DPS_Overlay_Canvas");
            if (canvasObj != null)
            {
                InitUI(canvasObj.transform);
                _isInitialized = true;
            }
            return;
        }

        // Now 'PlayerBarRefs' will be recognized here
        UpdateLeaderboard();
    }

    private void InitUI(Transform parentCanvas)
    {
        _leaderboardPanel = new GameObject("Leaderboard_Panel");
        _leaderboardPanel.transform.SetParent(parentCanvas, false);

        // MATCH PERSISTENT UI BACKGROUND
        Image panelBg = _leaderboardPanel.AddComponent<Image>();
        Texture2D whiteTex = new Texture2D(1, 1);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();
        panelBg.sprite = Sprite.Create(whiteTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        // Exact match to your PersistentUI: Black (0,0,0) at 80% Opacity (0.8f)
        panelBg.color = new Color(0f, 0f, 0f, 0.8f); 

        VerticalLayoutGroup vlg = _leaderboardPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(5, 5, 5, 5);
        vlg.spacing = 4;
        vlg.childControlHeight = false;
        vlg.childForceExpandHeight = false;

        RectTransform rect = _leaderboardPanel.GetComponent<RectTransform>();
        
        // Anchor to Right-Center (Same as Persistent UI)
        rect.anchorMin = new Vector2(1, 0.5f);
        rect.anchorMax = new Vector2(1, 0.5f);
        rect.pivot = new Vector2(1, 1); // Top-Right pivot

        // Position: -90Y clears the 170-height PersistentUI panel perfectly
        rect.anchoredPosition = new Vector2(-10, -90); 
        rect.sizeDelta = new Vector2(220, 110); 
        _leaderboardPanel.transform.localScale = Vector3.one;
    }

    private void UpdateLeaderboard()
    {
        var remotePlayers = Plugin.Instance.RemotePlayers;
        
        // Hide if no other players are present
        if (remotePlayers == null || remotePlayers.Count == 0) 
        {
            if (_leaderboardPanel.activeSelf) _leaderboardPanel.SetActive(false);
            return;
        }

        if (!_leaderboardPanel.activeSelf) _leaderboardPanel.SetActive(true);

        var sorted = remotePlayers.Values
            .OrderByDescending(p => p.TotalDamage)
            .Take(3)
            .ToList();
        
        float combatTime = Mathf.Max(0.1f, Plugin.Instance.ActiveCombatTime);
        float topDmg = sorted[0].TotalDamage;

        for (int i = 0; i < sorted.Count; i++)
        {
            if (!_playerBars.TryGetValue(i, out PlayerBarRefs refs))
            {
                refs = CreatePlayerBar(i);
                _playerBars[i] = refs;
            }

            refs.Root.SetActive(true);
            float dps = sorted[i].TotalDamage / combatTime;
            
            // Bar Fill Logic
            refs.BarFill.fillAmount = (topDmg > 0) ? (sorted[i].TotalDamage / topDmg) : 0;
            
            // Set Text: "Name: 1,234 DPS" in Black
            refs.InfoText.color = Color.black; 
            refs.InfoText.text = $"{sorted[i].Name}: {dps:N0} DPS";
        }

        // Cleanup extra bars
        for (int i = sorted.Count; i < _playerBars.Count; i++)
        {
            if (_playerBars.ContainsKey(i)) _playerBars[i].Root.SetActive(false);
        }
    }

    private PlayerBarRefs CreatePlayerBar(int index)
        {
            GameObject row = new GameObject("PlayerBar_" + index);
            row.transform.SetParent(_leaderboardPanel.transform, false);
            
            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, 25);

            // 1. Create the Background
            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.3f);

            // 2. Create the Bar Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(row.transform, false);
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            
            fillImg.color = (index == 0) ? new Color(1f, 0.8f, 0f, 1f) : new Color(0f, 0.75f, 1f, 1f);

            RectTransform fillRect = fillImg.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            // 3. Create the Text (TextMeshPro Edition)
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(row.transform, false);
            
            TextMeshProUGUI t = textObj.AddComponent<TextMeshProUGUI>();

            // --- FONT ASSIGNMENT ---
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
            
            if (gameFont != null) t.font = gameFont;
            // -----------------------

            t.fontSize = 14;
            t.fontStyle = FontStyles.Bold; // Note the 's' in FontStyles
            t.alignment = TextAlignmentOptions.Left; // TMP specific alignment
            t.color = Color.black; 
            
            // TMP handles overflow by default, but we can be explicit:
            t.overflowMode = TextOverflowModes.Overflow;
            t.enableWordWrapping = false;

            RectTransform tRect = t.rectTransform;
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;
            tRect.offsetMin = new Vector2(10, 0); 

            textObj.transform.SetAsLastSibling();

            return new PlayerBarRefs { Root = row, BarFill = fillImg, InfoText = t };
        }
    }


    // patch to fetch dmgh remote
[HarmonyPatch(typeof(RR.Game.Stats.Health), "AddDamageData")]
public static class DamageDataPatch
{
    static void Prefix(float damageValue, object damageType, int attackerID)
    {
        // 1. Get a safe string for the damage type
        string typeStr = damageType?.ToString() ?? "";

        // 2. Uniform logic: If there is an attacker, update their stats in the dictionary
        // This handles YOU and Remote players identically based on their unique ID
        Debug.Log($"[DPS Meter] Damage Detected: {damageValue} of type {typeStr} from AttackerID {attackerID}");
        if (attackerID >= 0 && attackerID < Plugin.Instance.RemotePlayers.Count)
        {
            UpdatePlayerStats(attackerID, damageValue, typeStr);
        }
    }

private static void UpdatePlayerStats(int id, float val, string type)
{
    // The incoming 'id' is 0, but the list uses 1. 
    // We define 'lookupId' to bridge that gap.
    int lookupId = id + 1;

    if (!Plugin.Instance.RemotePlayers.TryGetValue(lookupId, out var stats))
    {
        stats = new Plugin.PlayerStats();
        
        var pm = RR.PlayerManager.Instance;
        if (pm != null)
        {
            // Match the offset ID against the PlayerManager indexing
            var playerEntity = pm.GetPlayers().Find(p => p != null && p.FusionPlayerRef.PlayerId == lookupId);
            
            if (playerEntity != null && !string.IsNullOrEmpty(playerEntity.UserName))
            {
                stats.Name = playerEntity.UserName;
                Debug.Log($"[DPS] Linked ID {id} to Player {lookupId}: {stats.Name}");
            }
            else
            {
                stats.Name = $"Player {lookupId}";
            }
        }

        // Save using the lookupId so your UI can find it easily
        Plugin.Instance.RemotePlayers[lookupId] = stats;
    }

        // --- GLOBAL COMBAT TIMING ---
        if (Plugin.Instance.StartTime < 0) Plugin.Instance.StartTime = Time.time;
        Plugin.Instance.LastHitTime = Time.time;

        // --- UNIFORM STAT UPDATING ---
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

    [HarmonyPatch(typeof(Attack), "ModifyDamageWithModifierAndCritical")]
    public class MinionDamageHook
    {
        static void Postfix(object __instance, StatsManager victim, ref DamageDescriptor damageDesc, bool onlyForUI)
        {
            //MZa turned off to check if this enables dps tracker on remote player
            //if (onlyForUI) return;

            // Since the method is in the 'Attack' class, '__instance' refers to the Attack object.
            // We need to find the stats associated with this attack.
            // Based on your original code, it looks like 'Attack' has a private field called '_stats'.

            if (damageDesc.damageValue != 0 && onlyForUI)
            {   
                var stats = Traverse.Create(__instance).Field("_stats").GetValue<StatsManager>();
                float finalDmg = damageDesc.damageValue;
                Debug.Log($"[DPS Meter] Attacker {stats.name} dealt {finalDmg} damage to {victim?.name}");
                if (Plugin.Instance.StartTime < 0) Plugin.Instance.StartTime = Time.time;
                Plugin.Instance.LastHitTime = Time.time;
                // Traverse can pull all field names and values into a dictionary
                var fields = Traverse.Create(stats).Fields();
                
                Debug.Log($"--- Inspecting StatsManager ({fields.Count} fields found) ---");
                foreach (var fieldName in fields)
                {
                    var val = Traverse.Create(stats).Field(fieldName).GetValue();
                    Debug.Log($"Field: {fieldName} | Value: {val}");
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

            // 1. Get the ActorID from the Attacker (__instance)
            // Based on your log: Field: <ActorID>k__BackingField
            int attackerActorID = Traverse.Create(__instance).Field("<ActorID>k__BackingField").GetValue<int>();

            // 2. Apply your +1 offset to match the PlayerManager/Dictionary indexing
            int lookupId = attackerActorID + 1;

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
                        // 3. Find the correct player stats in your dictionary
                        if (Plugin.Instance.RemotePlayers.TryGetValue(lookupId, out var stats))
                        {
                            stats.TotalFrost += frozenDelta;
                            
                            // Also track global combat timing for this player
                            if (Plugin.Instance.StartTime < 0) Plugin.Instance.StartTime = Time.time;
                            Plugin.Instance.LastHitTime = Time.time;

                            Debug.Log($"[DPS Meter] Actor {attackerActorID} (Key {lookupId}) dealt {frozenDelta} Frozen Bonus Damage.");
                        }
                        else
                        {
                            // Fallback: If stats don't exist yet, create them or log the miss
                            Debug.LogWarning($"[DPS Meter] Received Frozen Damage for Actor {attackerActorID}, but no RemotePlayer found at Key {lookupId}");
                        }
                    }
                }
            }
        }
    }
public static class DamageIdentityBridge
{
    // This "remembers" the minion ID for the duration of one hit
    public static int ActiveMinionActorID = -1;

    [HarmonyPatch(typeof(RR.Game.Stats.Health), "TakeBasicDamage")]
    public static class TakeBasicDamagePatch
    {
        static void Prefix(StatsManager attacker)
        {
            // If the attacker is a minion (ID > 10 usually), remember its ID
            if (attacker != null && attacker.ActorID > 10)
            {
                ActiveMinionActorID = attacker.ActorID;
            }
            else
            {
                ActiveMinionActorID = -1;
            }
        }

        static void Postfix()
        {
            // Clear it after the hit is fully processed to avoid misattribution
            ActiveMinionActorID = -1;
        }
    }

    [HarmonyPatch(typeof(RR.Game.Stats.Health), "AddDamageData")]
    public static class AddDamageDataPatch
    {
        static void Prefix(float damageValue, int attackerID)
        {
            // If the game says the attacker is ID 0 (Owner/You),
            // but our Bridge remembers a Minion ID (like 14)...
            bool isMinionHit = (ActiveMinionActorID != -1);

            // Apply your standard FusionID offset
            int lookupId = (attackerID == 0) ? 1 : attackerID + 1;

            if (Plugin.Instance.RemotePlayers.TryGetValue(lookupId, out var stats))
            {
                if (isMinionHit)
                {
                    // Success! We've distilled the damage
                    stats.TotalMinion += damageValue;
                    Debug.Log($"[DPS] Distilled {damageValue} Minion Damage from Actor {ActiveMinionActorID} (credited to Owner {attackerID})");
                }
            }
        }
    }
}
    // sending to DPS to lobby
    /// <summary>
    /// 
    /// </summary>

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
                        Name = userName,
                        TotalDamage = Plugin.Instance.TotalDamage,
                        TotalBurn = Plugin.Instance.TotalBurn,
                        TotalRoot = Plugin.Instance.TotalRoot,
                        TotalPoison = Plugin.Instance.TotalPoison,
                        TotalBleed = Plugin.Instance.TotalBleed,
                        TotalShock = Plugin.Instance.TotalShock,
                        TotalFrost = Plugin.Instance.TotalFrost,
                        TotalCurse = Plugin.Instance.TotalCurse,
                        TotalMinion = Plugin.Instance.TotalMinion
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
                        (int)statsToSync.TotalFrost, (int)statsToSync.TotalCurse, 
                        (int)statsToSync.TotalMinion);

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
                        if (values.Length >= 9)
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