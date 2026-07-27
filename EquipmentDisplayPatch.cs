using HarmonyLib;
using RR.Game.Items;
using RR.Game.Pickups;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RR.UI.Controls;

namespace BlackveilDpsMeter
{
    public static class PermanentItemLabelPatch
    {
        private static readonly Dictionary<int, WorldItemLabel> ActiveLabels = new Dictionary<int, WorldItemLabel>();

        internal static void Apply(Harmony harmony)
        {
            Debug.Log("[ItemLabels] --- Initializing PermanentItemLabelPatch ---");

            try
            {
                int patchedCount = 0;

                // Scan EquipmentPickup for network spawn or setup methods
                MethodInfo[] methods = typeof(EquipmentPickup).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                foreach (var method in methods)
                {
                    string name = method.Name;
                    if (name.Contains("Spawn") || name.Contains("Init") || name.Contains("Setup") || name.Contains("SetEquipment") || name.Equals("OnEnable"))
                    {
                        harmony.Patch(method, postfix: new HarmonyMethod(typeof(PermanentItemLabelPatch), nameof(Postfix_OnEquipmentProcessed)));
                        Debug.Log($"[ItemLabels] Successfully hooked EquipmentPickup.{name}()");
                        patchedCount++;
                    }
                }

                if (patchedCount == 0)
                {
                    Debug.LogError("[ItemLabels] Could NOT find any matching setup methods on EquipmentPickup! Dumping method names:");
                    foreach (var m in methods)
                    {
                        Debug.Log($"[ItemLabels] Found Method: {m.Name}");
                    }
                }

                // Clean up when item despawns
                var disableMethod = AccessTools.Method(typeof(EquipmentPickup), "OnDisable");
                if (disableMethod != null)
                {
                    harmony.Patch(disableMethod, postfix: new HarmonyMethod(typeof(PermanentItemLabelPatch), nameof(Postfix_OnItemDespawned)));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemLabels] Error during patch application: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        private static void Postfix_OnEquipmentProcessed(EquipmentPickup __instance)
        {
            if (__instance == null ) return;

            try
            {
                int rawRarity = Convert.ToInt32(__instance.Equipment.RarityLevel);
                
                // Normalize Uncommon (5) so it doesn't bypass higher thresholds
                int normalizedRarity = NormalizeRarity(rawRarity);
                int minThreshold = Plugin.MinRarityThreshold;

                if (normalizedRarity >= minThreshold && Plugin.SelectedRarityMode == "equal and above")
                {
                    CreateWorldLabel(__instance);
                }
                else if (normalizedRarity == minThreshold && Plugin.SelectedRarityMode == "exclusive")
                {
                    CreateWorldLabel(__instance);
                }
                // Plugin.SelectedRarityMode == "off" is off

            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ItemLabels] Exception in Postfix_OnEquipmentProcessed: {ex.Message}");
            }
        }

        private static int NormalizeRarity(int rawRarity)
        {
            // Common: 0, Rare: 1, Epic: 2, Legendary: 3, Mythic: 4, Uncommon: 5 -> mapped to 1
            if (rawRarity == 5) return 1; 
            return rawRarity;
        }

        [HarmonyPostfix]
        private static void Postfix_OnItemDespawned(EquipmentPickup __instance)
        {
            if (__instance == null) return;

            int id = __instance.GetInstanceID();
            if (ActiveLabels.TryGetValue(id, out WorldItemLabel label))
            {
                if (label != null) UnityEngine.Object.Destroy(label);
                ActiveLabels.Remove(id);
            }
        }

        private static void CreateWorldLabel(EquipmentPickup pickup)
        {
            int id = pickup.GetInstanceID();
            if (ActiveLabels.ContainsKey(id)) return;

            WorldItemLabel label = pickup.gameObject.AddComponent<WorldItemLabel>();
            label.Initialize(pickup);
            ActiveLabels[id] = label;
        }
    }

    public class WorldItemLabel : MonoBehaviour
    {
        private EquipmentPickup _pickup;
        private Camera _mainCamera;
        private string _labelText;
        private Color _textColor;
        private Color _backgroundColor;
        private GUIStyle _style;
        private static Texture2D _bgTexture;

        public void Initialize(EquipmentPickup pickup)
        {
            _pickup = pickup;
            _mainCamera = Camera.main;

            // Fetch the full descriptor to extract localized Name and special statuses
            EquipmentDescriptor descriptor = new EquipmentDescriptor(pickup.Equipment);

            LocLabel locLabel = new LocLabel();
            descriptor.SetNameToLabel(locLabel);

            // Get localized full name from the label (falls back to descriptor.Name if text is null)
            string formattedName = !string.IsNullOrEmpty(locLabel.text) ? locLabel.text : descriptor.Name;

            bool isUber = false;
            if (descriptor.Properties != null)
            {
                for (int i = 0; i < descriptor.Properties.Length; i++)
                {
                    if (descriptor.Properties[i].IsUber)
                    {
                        isUber = true;
                        break;
                    }
                }
            }

            if (isUber)
            {
                formattedName += " ✦";
            }

            if (descriptor.IsLucky)
            {
                formattedName += " ♣";
            }

            if (_bgTexture == null)
            {
                _bgTexture = MakeSolidTexture(1, 1, Color.white);
            }

            _labelText = formattedName;

            // Safe rarity color conversion
            int rarityValue = (int)pickup.Equipment.RarityLevel;
            switch (rarityValue)
            {
                case 2: // Epic (Purple)
                    _textColor = new Color(0.78f, 0.58f, 0.95f);       // Soft purple text
                    _backgroundColor = new Color(0.18f, 0.16f, 0.24f, 0.9f); // Dark purple tint
                    break;
                case 3: // Legendary (Gold/Orange)
                    _textColor = new Color(1.0f, 0.75f, 0.28f);       // Gold/Orange text
                    _backgroundColor = new Color(0.22f, 0.18f, 0.12f, 0.9f); // Dark gold tint
                    break;
                case 4: // Mythic (Red/Pink)
                    _textColor = new Color(0.95f, 0.45f, 0.52f);       // Soft red text
                    _backgroundColor = new Color(0.24f, 0.14f, 0.16f, 0.9f); // Dark red tint
                    break;
                default: // Default / White
                    _textColor = Color.white;
                    _backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.85f);
                    break;
            }

            _style = new GUIStyle
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };
            _style.normal.textColor = _textColor;
        }
        private Texture2D MakeSolidTexture(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = color;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void OnGUI()
        {
            if (_pickup == null || !_pickup.gameObject.activeInHierarchy) return;
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Vector3 worldPos = transform.position + new Vector3(0, 1.8f, 0);
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);

            if (screenPos.z > 0)
            {
                // Calculate dynamic width based on exact text length + padding
                Vector2 textSize = _style.CalcSize(new GUIContent(_labelText));
                
                float paddingX = 20f;
                float paddingY = 8f;

                float boxWidth = textSize.x + paddingX ;
                float boxHeight = textSize.y + paddingY;

                float boxX = screenPos.x - (boxWidth / 2f);
                float boxY = Screen.height - screenPos.y - (boxHeight / 2f);

                Rect bgRect = new Rect(boxX, boxY, boxWidth, boxHeight);

                // 1. Draw dynamically sized dark background box
                Color savedGUIColor = GUI.color;
                GUI.color = _backgroundColor;
                GUI.DrawTexture(bgRect, _bgTexture);
                GUI.color = savedGUIColor;

                // 2. Draw Text centered inside background box

                Rect textRect = new Rect(boxX, boxY, boxWidth, boxHeight);

                // 3. Draw Dark Grey Text Outline (2px in all 4 directions)
                GUIStyle outlineStyle = new GUIStyle(_style);
                outlineStyle.normal.textColor = new Color(0.15f, 0.15f, 0.15f, 0.9f); // Dark Grey Outline

                GUI.Label(new Rect(textRect.x - 2, textRect.y, textRect.width, textRect.height), _labelText, outlineStyle);
                GUI.Label(new Rect(textRect.x + 2, textRect.y, textRect.width, textRect.height), _labelText, outlineStyle);
                GUI.Label(new Rect(textRect.x, textRect.y - 2, textRect.width, textRect.height), _labelText, outlineStyle);
                GUI.Label(new Rect(textRect.x, textRect.y + 2, textRect.width, textRect.height), _labelText, outlineStyle);
                
                // Draw Main Text
                GUI.Label(textRect, _labelText, _style);
            }
        }
    }
}