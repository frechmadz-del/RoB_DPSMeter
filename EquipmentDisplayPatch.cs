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
            if (__instance == null) return;

            try
            {
                int rarityLevel = (int)__instance.Equipment.RarityLevel;
                int minThreshold = Plugin.MinRarityThreshold;

                if (rarityLevel >= minThreshold)
                {
                    CreateWorldLabel(__instance);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ItemLabels] Exception in Postfix_OnEquipmentProcessed: {ex.Message}");
            }
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
        private Color _labelColor;
        private GUIStyle _style;

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
                formattedName += " ✦ ";
            }

            if (descriptor.IsLucky)
            {
                formattedName += " ♣ ";
            }

            _labelText = formattedName;

            // Safe rarity color conversion
            int rarityValue = (int)pickup.Equipment.RarityLevel;
            _labelColor = rarityValue switch
            {
                1 => new Color(0f, 0.44f, 0.87f),    // Rare (Blue)
                2 => new Color(0.64f, 0.21f, 0.93f), // Epic (Purple)
                3 => new Color(1f, 0.5f, 0f),       // Legendary (Orange)
                4 => new Color(1f, 0f, 0f),      // Mythic (Red)
                _ => Color.white
            };

            _style = new GUIStyle
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _style.normal.textColor = _labelColor;
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
                float rectWidth = 300f;
                float rectHeight = 35f;
                Rect rect = new Rect(screenPos.x - (rectWidth / 2f), Screen.height - screenPos.y - (rectHeight / 2f), rectWidth, rectHeight);

                // Text shadow outline for readability
                GUIStyle outlineStyle = new GUIStyle(_style);
                outlineStyle.normal.textColor = Color.black;

                GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), _labelText, outlineStyle);
                GUI.Label(new Rect(rect.x - 1, rect.y - 1, rect.width, rect.height), _labelText, outlineStyle);
                GUI.Label(rect, _labelText, _style);
            }
        }
    }
}