using System;
using UnityEngine;
using UnityEngine.UIElements;
using BlackveilDpsMeter;
using RR;
using RR.Game.Stats;
using TMPro;
using UnityEngine.UI;

namespace RaidersOfBlackveilMod
{
    public class CombatInfoModule : MonoBehaviour
    {
        public static CombatInfoModule Instance { get; private set; }

        private GameObject _canvasObj;
        private GameObject _panelObj;
        private TextMeshProUGUI _uiText;
        private float _updateTimer = 0f;
        private bool _lastPersistentUIVisibility = true; // Cache the previous state

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[CombatInfo] CombatInfoModule initialized and set to persist across scenes.");
        }

        private void Start()
        {
            SetupHUD();
        }

        private void SetupHUD()
        {
            // 1. Rename to something completely unique to avoid the game's UI scanner
            _canvasObj = new GameObject("Modded_M_Power_Display_Root");
            DontDestroyOnLoad(_canvasObj);

            // Set the layer to "Ignore Raycast" (Layer 2) or a high layer so game cameras ignore it
            _canvasObj.layer = 2; 

            Canvas canvas = _canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Crucial: This tells Unity this canvas is an independent overlay 
            // and shouldn't be grouped with the game's main HUD hierarchy
            canvas.overrideSorting = true; 
            canvas.sortingOrder = 998; 

            // Do NOT add GraphicRaycaster unless you absolute need clickability.
            // Removing it prevents the game's EventSystem / UIManager from tracking it!
            // _canvasObj.AddComponent<GraphicRaycaster>(); 

            // 2. Create the Background Frame
            _panelObj = new GameObject("Modded_Frame_Background");
            _panelObj.transform.SetParent(_canvasObj.transform, false);
            _panelObj.layer = 2;

            UnityEngine.UI.Image frameImage = _panelObj.AddComponent<UnityEngine.UI.Image>();
            frameImage.color = new Color(0.45f, 0.35f, 0.2f, 1f); 

            RectTransform panelRect = frameImage.rectTransform;
            panelRect.anchorMin = new Vector2(1, 0.5f);
            panelRect.anchorMax = new Vector2(1, 0.5f);
            panelRect.pivot = new Vector2(1, 0.5f);
            panelRect.anchoredPosition = new Vector2(-10, -160); // bottom right corner
            panelRect.sizeDelta = new Vector2(180, 85);       

            var outline = _panelObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.15f, 0.1f, 0.05f, 1f);
            outline.effectDistance = new Vector2(2, -2);

            // 2b. Inner Area
            GameObject innerArea = new GameObject("Modded_Inner_Area");
            innerArea.transform.SetParent(_panelObj.transform, false);
            innerArea.layer = 2;

            UnityEngine.UI.Image innerImage = innerArea.AddComponent<UnityEngine.UI.Image>();
            innerImage.color = new Color(0f, 0f, 0f, 0.95f);

            RectTransform innerRect = innerImage.rectTransform;
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.offsetMin = new Vector2(3, 3); 
            innerRect.offsetMax = new Vector2(-3, -3);

            // 3. Dynamic Text Display
            GameObject textObj = new GameObject("Modded_Value_Text");
            textObj.transform.SetParent(innerArea.transform, false);
            textObj.layer = 2;

            _uiText = textObj.AddComponent<TextMeshProUGUI>();
            _uiText.fontSize = 14;
            _uiText.color = Color.white;
            _uiText.alignment = TextAlignmentOptions.Center;

            RectTransform textRect = _uiText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(5, 5); 
            textRect.offsetMax = new Vector2(-5, -5);

            // Find and assign font
            var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var f in fonts)
            {
                if (f.name.Contains("LiberationSans"))
                {
                    _uiText.font = f;
                    break;
                }
            }

            UpdateVisibilityState();
        }

        private void Update()
        {
            // 1. Grab the game's current UI visibility state (Assumes this is a static property)
            bool gameUIIsVisible = PersistentUI._isVisible; 

            // 2. If the game's UI visibility state flipped, update immediately!
            if (gameUIIsVisible != _lastPersistentUIVisibility)
            {
                _lastPersistentUIVisibility = gameUIIsVisible;
                UpdateVisibilityState(); // Re-evaluate active state
            }
            
            // Stop updates immediately if disabled or hidden
            if (_canvasObj == null || _panelObj == null || !Plugin.Instance.go_timer)
                 return;

            _updateTimer += Time.deltaTime;
            if (_updateTimer >= 0.25f) // 0.25 seconds = 4 Hz (4 times per second)
            {
                _updateTimer = 0f; // Reset the timer

                var (magicPower, physicalPower, CriticalChance, CriticalDamage, isOutOfDanger, element) = FetchCombatData();

                // 1. Determine the color and text dynamically based on the state
                string safeColor = isOutOfDanger ? "#A0FFA0" : "#FF5555";

                string elementLabel = Plugin.SelectedElement.ToUpper();

                var hexColor = elementLabel.ToLower() switch
            {
                "burn"   => "#f17728",
                "poison" => "#952db8",
                "bleed"  => "#c94a46",
                "shock"  => "#b7a93f",
                "root"   => "#be927e",
                "frost"  => "#57cccc",
                "chill"  => "#57cccc", // Maps chill to frost color/stat
                "curse"  => "#267b5b",
                "minion" => "#f06ee9",
                "summon" => "#f06ee9", // Maps summon to minion color/stat
                "bless"  => "#d8f19c",
                "fury"   => "#64210f",
                _        => "#ffffff" // Default fallback if no match
            };

            

            string ColorText(string text, string hex, float value) => $"<color={hex}><b>{text}:</b>{value:F1}</color>";

            string finalLine = ColorText(elementLabel, hexColor, element);

                
                // Format to 1 decimal place (e.g. 120.5)
                _uiText.text =  $"<color=#C090FF><b>MAGIC:</b> {magicPower:F1}</color>\n" +
                                $"<color=#FF8080><b>PHYSICAL:</b> {physicalPower:F1}</color>\n" +
                                finalLine + "\n" +
                                $"<color=#FFFF00><b>Crit:</b> {CriticalChance:F1}%//{CriticalDamage:F1}%</color>\n" +
                                $"<color={safeColor}><b>Safe:</b> {isOutOfDanger}</color>";
            }
        }

        public void UpdateVisibilityState()
        {
            if (_canvasObj != null)
            {
                _canvasObj.SetActive(Plugin.ShowCombatInfo && PersistentUI._isVisible);
            }
        }

        private (float magicPower, float physicalPower,float CriticalChance,float CriticalDamage, bool isOutOfDanger, float element) FetchCombatData()
        {
            {
            try
            {
                var stats = StatsManager.GetStatsByActorID(Plugin.Instance.LocalPlayerActorID);
                if (stats != null && stats.IsStatsInitialized && stats.Object != null && stats.Object.IsValid)
                    { 
                        float magic = stats.MagicPower;
                        float physical = stats.PhysicalPower;
                        float CriticalChance = 0f;
                        float CriticalDamage = 0f;
                        if (stats.Attack != null)
                        {
                            CriticalChance = stats.Attack.CriticalChanceWithCap;
                            CriticalDamage = stats.Attack.CriticalDamagePercentage;
                        }
                        else
                        {
                            Debug.LogWarning($"[CombatInfo] stats.Attack is null for player {Plugin.Instance.LocalPlayerActorID}!");
                        }
                        
                        // Direct access via the StatsManager property!
                        bool outOfDanger = true;
                        if (stats.RuntimeStats != null)
                        {
                            outOfDanger = stats.RuntimeStats.IsOutOfDanger;
                        }
                        else
                        {
                            Debug.LogWarning($"[CombatInfo] stats.RuntimeStats is null for player {Plugin.Instance.LocalPlayerActorID}!");
                        }

                        // Get element
                        float element = Plugin.SelectedElement switch
                        {
                            "Shock" => stats.Shock.DamageMultiplierPCT-100f,
                            "Burn"  => stats.Burn.DamageMultiplierPCT-100f,
                            "Chill" => stats.Chill.DamageMultiplierAgainstFrozenTarget*100f-100f,
                            "Poison" => stats.Poison.DamageMultiplierPCT-100f,
                            "Bleed" => stats.Bleed.DamageMultiplierPCT-100f,
                            "Fury" => stats.Fury.BoostedAttackDamageIncPCT.Value,
                            "Curse" => stats.Curse.CurseDamageMultiplier*100f-100f,
                            "Summon" => stats.Summon.MinionsDamageMultiplier.Multiplier*100f-100f,
                            "Bless" => stats.Bless.EmpoweredAttackDamageIncrementForUI,
                            _       => 0f // The default fallback if SelectedElement is null or an unexpected string
};

                        return (magic, physical, CriticalChance, CriticalDamage, outOfDanger,element);
                    }
                }
            catch (Exception)
            {
                Debug.Log($"[CombatInfo] Error fetching Magic Power for ActorID: {Plugin.Instance.LocalPlayerActorID}. Returning 0.");
                // Catching errors to prevent debug console spam during transitions
            }
            return (0f, 0f, 0f, 0f, true, 0f);  
            }
        }
    }
}