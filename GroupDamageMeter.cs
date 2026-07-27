using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using BlackveilDpsMeter;
using RR;
using TMPro;

namespace RaidersOfBlackveilMod
{
    public class GroupDamageMeter : MonoBehaviour
    {
        public static GroupDamageMeter Instance { get; private set; }

        private GameObject _canvasObj;
        private GameObject _panelObj;
        private TextMeshProUGUI _uiText;
        private float _updateTimer = 0f;
        private bool _lastPersistentUIVisibility = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GroupDamageMeter] GroupDamageMeter initialized and set to persist across scenes.");
        }

        private void Start()
        {
            SetupHUD();
        }

        private void SetupHUD()
        {
            // 1. Create root canvas (above DPS meter which is 999, below CombatInfo which is 998)
            _canvasObj = new GameObject("Modded_Group_DPS_Root");
            DontDestroyOnLoad(_canvasObj);

            // Set the layer to "Ignore Raycast" (Layer 2)
            _canvasObj.layer = 2;

            Canvas canvas = _canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 997; // Between DPS meter (999) and CombatInfo (998)

            // 2. Create the Background Frame
            _panelObj = new GameObject("Modded_Group_Frame_Background");
            _panelObj.transform.SetParent(_canvasObj.transform, false);
            _panelObj.layer = 2;

            UnityEngine.UI.Image frameImage = _panelObj.AddComponent<UnityEngine.UI.Image>();
            frameImage.color = new Color(0.45f, 0.35f, 0.2f, 1f); 

            RectTransform panelRect = frameImage.rectTransform;
            panelRect.anchorMin = new Vector2(1, 0);
            panelRect.anchorMax = new Vector2(1, 0);
            panelRect.pivot = new Vector2(1, 0);
            panelRect.anchoredPosition = new Vector2(-10, 190); 
            panelRect.sizeDelta = new Vector2(190, 140); // Width x Height

            var outline = _panelObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.15f, 0.1f, 0.05f, 1f);
            outline.effectDistance = new Vector2(2, -2);

            // 2b. Inner Area
            GameObject innerArea = new GameObject("Modded_Group_Inner_Area");
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
            GameObject textObj = new GameObject("Modded_Group_Value_Text");
            textObj.transform.SetParent(innerArea.transform, false);
            textObj.layer = 2;

            _uiText = textObj.AddComponent<TextMeshProUGUI>();
            _uiText.fontSize = 12;
            _uiText.color = Color.white;
            _uiText.alignment = TextAlignmentOptions.TopLeft;

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
            // 1. Grab the game's current UI visibility state
            bool gameUIIsVisible = Plugin._isVisible;

            // 2. If the game's UI visibility state flipped, update immediately!
            if (gameUIIsVisible != _lastPersistentUIVisibility)
            {
                _lastPersistentUIVisibility = gameUIIsVisible;
                UpdateVisibilityState();
            }

            // Stop updates immediately if disabled or hidden
            if (_canvasObj == null || _panelObj == null || !Plugin.Instance.go_timer)
                return;

            _updateTimer += Time.deltaTime;
            if (_updateTimer >= 0.5f) // Update every 0.5 seconds
            {
                _updateTimer = 0f;

                UpdateGroupDamageDisplay();
            }
        }

        private void UpdateGroupDamageDisplay()
        {
            var pm = RR.PlayerManager.Instance;
            if (pm == null)
            {
                _uiText.text = "<color=#FF5555>PlayerManager Error</color>";
                return;
            }

            // Fetch data for all 3 player slots
            var groupData = new List<(string name, float dps)>();
            float groupTotalDPS = 0f;

            for (int slot = 0; slot < 3; slot++)
            {
                var player = pm.GetPlayerBySlot(slot);
                if (player != null)
                {
                    string playerName = string.IsNullOrEmpty(player.UserName) ? $"Player {slot}" : player.UserName;
                    int actorID = slot;

                    float playerDPS = 0f;
                    if (actorID >= 0 && Plugin.Instance.AllPlayerStats.ContainsKey(actorID))
                    {
                        var playerStats = Plugin.Instance.AllPlayerStats[actorID];
                        float displayTime = Mathf.Max(0.1f, Plugin.Instance.ActiveCombatTime);
                        playerDPS = playerStats.TotalDamage / displayTime;
                        groupTotalDPS += playerDPS;
                    }

                    groupData.Add((playerName, playerDPS)); // Percentage will be calculated below
                }
            }

            // Calculate percentages
            if (groupTotalDPS > 0)
            {
                for (int i = 0; i < groupData.Count; i++)
                {
                    var (name, dps) = groupData[i];
                    groupData[i] = (name, dps);
                }
            }

            // Build the display text
            // Find the highest DPS in the group to use as the scaling anchor
            float maxDPS = 0f;
            for (int i = 0; i < groupData.Count; i++)
            {
                if (groupData[i].Item2 > maxDPS)
                {
                    maxDPS = groupData[i].Item2;
                }
            }

            // Build the display text
            string displayText = $"<b><color=#A0FFA0>GROUP DPS: {groupTotalDPS:F1}</color></b>\n";

            // Add bar and stats for each player
            for (int i = 0; i < groupData.Count; i++)
            {
                var (name, dps) = groupData[i];

                // Color code by slot
                string barColor = i switch
                {
                    0 => "#5F8FFF", // Blue for slot 0
                    1 => "#5FFF5F", // Green for slot 1
                    2 => "#FF5F5F", // Red for slot 2
                    _ => "#CCCCCC"
                };

                // Scale bar width relative to the top DPS instead of group total
                int barWidth = 0;
                if (maxDPS > 0)
                {
                    float relativeScale = dps / maxDPS; // Will be exactly 1.0 for the top player
                    barWidth = (int)(relativeScale * 20f); // Max 15 chars
                }

                string bar = barWidth > 0 ? $"<color={barColor}>{new string('█', barWidth)}</color>" : "";

                displayText += $"{name} [{dps:F1}]\n{bar}\n";
            }

            _uiText.text = displayText;
        }

        public void UpdateVisibilityState()
        {
            if (_canvasObj != null)
            {
                _canvasObj.SetActive(Plugin.ShowGroupDPS && Plugin._isVisible);
            }
        }
    }
}
