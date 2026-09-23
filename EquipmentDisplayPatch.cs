using HarmonyLib;
using RR.Game.Items;
using RR.Game.Pickups;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RR.UI.Controls;
using RR.UI.Components.Pickup;
using System.Linq;

namespace BlackveilDpsMeter
{
    public static class PermanentItemLabelPatch
    {
        private static readonly Dictionary<EntityId, WorldItemLabel> ActiveLabels = new Dictionary<EntityId, WorldItemLabel>();
        private static readonly Dictionary<int, int> RarityRankMap = new Dictionary<int, int>
        {
            { 0, 1 }, // Common
            { 1, 2 }, // Rare intentionally kept below the default threshold to prevent over-filtering
            { 2, 3 }, // Epic
            { 3, 4 }, // Legendary
            { 4, 5 }, // Mythic
            { 5, 1 }  // Uncommon
        };

        internal static void Apply(Harmony harmony)
        {
            Debug.Log("[ItemLabels] --- Initializing PermanentItemLabelPatch ---");

            try
            {
                PatchPickupType(harmony, typeof(EquipmentPickup), nameof(Postfix_OnEquipmentProcessed));
                PatchPickupType(harmony, typeof(ItemPickup), nameof(Postfix_OnItemPickupProcessed));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemLabels] Error during patch application: {ex.Message}");
            }
        }

        private static void PatchPickupType(Harmony harmony, Type pickupType, string postfixName)
        {
            int patchedCount = 0;
            MethodInfo[] methods = pickupType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                string name = method.Name;
                if (name.Contains("Spawn") || name.Contains("Init") || name.Contains("Setup") || name.Contains("SetEquipment") || name.Equals("OnEnable") || name.Equals("OnSpawned"))
                {
                    harmony.Patch(method, postfix: new HarmonyMethod(typeof(PermanentItemLabelPatch), postfixName));
                    Debug.Log($"[ItemLabels] Successfully hooked {pickupType.Name}.{name}()");
                    patchedCount++;
                }
            }

            if (patchedCount == 0)
            {
                Debug.LogWarning($"[ItemLabels] No matching setup methods found on {pickupType.Name}.");
            }

            var disableMethod = AccessTools.Method(pickupType, "OnDisable");
            if (disableMethod != null)
            {
                harmony.Patch(disableMethod, postfix: new HarmonyMethod(typeof(PermanentItemLabelPatch), nameof(Postfix_OnItemDespawned)));
            }
        }

        [HarmonyPostfix]
        private static void Postfix_OnEquipmentProcessed(EquipmentPickup __instance)
        {
            if (__instance == null || !Plugin.ShouldShowEquipmentLabels) return;

            try
            {
                if (string.IsNullOrEmpty(Plugin.SelectedRarityMode) || Plugin.SelectedRarityMode.Equals("off", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                int rawRarity = Convert.ToInt32(__instance.Equipment.RarityLevel);
                int normalizedRarity = NormalizeRarity(rawRarity);
                int minThreshold = Plugin.MinEquipmentRarityThreshold;

                if (normalizedRarity >= minThreshold && Plugin.SelectedRarityMode == "equal and above")
                {
                    CreateWorldLabel(__instance);
                }
                else if (normalizedRarity == minThreshold && Plugin.SelectedRarityMode == "exclusive")
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
        private static void Postfix_OnItemPickupProcessed(ItemPickup __instance)
        {
            if (__instance == null || !Plugin.ShouldShowItemLabels) return;

            try
            {
                if (string.IsNullOrEmpty(Plugin.SelectedRarityMode) || Plugin.SelectedRarityMode.Equals("off", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                GenericItemDescriptor descriptor = new GenericItemDescriptor(__instance.Item);
                if (!Plugin.HighlightCurrency && IsCurrencyItem(descriptor.ItemType))
                {
                    return;
                }

                int rawRarity = (int)descriptor.Rarity;
                int normalizedRarity = NormalizeRarity(rawRarity);
                int minThreshold = Plugin.MinItemRarityThreshold;

                if (normalizedRarity >= minThreshold && Plugin.SelectedRarityMode == "equal and above")
                {
                    CreateWorldLabel(__instance);
                }
                else if (normalizedRarity == minThreshold && Plugin.SelectedRarityMode == "exclusive")
                {
                    CreateWorldLabel(__instance);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ItemLabels] Exception in Postfix_OnItemPickupProcessed: {ex.Message}");
            }
        }

        private static bool IsCurrencyItem(ItemType itemType)
        {
            return itemType == ItemType.BlackCoin
                || itemType == ItemType.BlackBlood
                || itemType == ItemType.Glitter
                || itemType == ItemType.Scrap;
        }

        private static int NormalizeRarity(int rawRarity)
        {
            if (RarityRankMap.TryGetValue(rawRarity, out int mapped))
            {
                return mapped;
            }

            return 1;
        }

        [HarmonyPostfix]
        private static void Postfix_OnItemDespawned(PickupItemWithUI __instance)
        {
            if (__instance == null) return;

            EntityId id = __instance.GetEntityId();
            if (ActiveLabels.TryGetValue(id, out WorldItemLabel label))
            {
                if (label != null) UnityEngine.Object.Destroy(label);
                ActiveLabels.Remove(id);
            }
        }

        private static void CreateWorldLabel(EquipmentPickup pickup)
        {
            EntityId id = pickup.GetEntityId();
            if (ActiveLabels.ContainsKey(id)) return;

            WorldItemLabel label = pickup.gameObject.AddComponent<WorldItemLabel>();
            label.Initialize(pickup);
            ActiveLabels[id] = label;
        }

        private static void CreateWorldLabel(ItemPickup pickup)
        {
            EntityId id = pickup.GetEntityId();
            if (ActiveLabels.ContainsKey(id)) return;

            WorldItemLabel label = pickup.gameObject.AddComponent<WorldItemLabel>();
            label.Initialize_Item(pickup);
            ActiveLabels[id] = label;
        }
    }

    public class WorldItemLabel : MonoBehaviour
    {
        private EquipmentPickup _pickup;
        private ItemPickup _itemPickup;

        private Camera _mainCamera;
        private string _labelText;
        private Color _textColor;
        private Color _backgroundColor;
        private GUIStyle _style;
        private static Texture2D _bgTexture;
        private static Texture2D _uberTexture;
        private static Texture2D _luckyTexture;
        private static Texture2D _chaosTexture;
        private static bool _texturesLoaded = false;
        private bool _isUber;
        private bool _isLucky;
        private bool _isChaos;

        private static void LoadTexturesOnce()
        {
            if (_texturesLoaded) return;
            _texturesLoaded = true;

            Texture2D[] allTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
            foreach (var t in allTextures)
            {
                if (t == null) continue;
                string tName = t.name;

                if (_uberTexture == null && tName.Equals("StatType_Uber", StringComparison.OrdinalIgnoreCase))
                {
                    _uberTexture = t;
                }
                else if (_luckyTexture == null && tName.Equals("Bonus_Lucky", StringComparison.OrdinalIgnoreCase))
                {
                    _luckyTexture = t;
                }
                else if (_chaosTexture == null && tName.Equals("StatType_Chaos", StringComparison.OrdinalIgnoreCase))
                {
                    _chaosTexture = t;
                }

                if (_uberTexture != null && _luckyTexture != null && _chaosTexture != null)
                {
                    break;
                }
            }
        }

        public void Initialize(EquipmentPickup pickup)
        {
            _pickup = pickup;
            _itemPickup = null;

            if (!IsLocalPlayerAllowedToPickup(pickup))
            {
                this.enabled = false;
                return;
            }

            _mainCamera = Camera.main;

            EquipmentDescriptor descriptor = new EquipmentDescriptor(pickup.Equipment);
            LocLabel locLabel = new LocLabel();
            descriptor.SetNameToLabel(locLabel);

            string formattedName = !string.IsNullOrEmpty(locLabel.text) ? locLabel.text : descriptor.Name;
            _labelText = formattedName;

            _isUber = false;
            if (descriptor.Properties != null)
            {
                for (int i = 0; i < descriptor.Properties.Length; i++)
                {
                    if (descriptor.Properties[i].IsUber)
                    {
                        _isUber = true;
                        break;
                    }
                }
            }

            _isLucky = descriptor.IsLucky;
            _isChaos = descriptor.IsChaos;

            int rarityValue = (int)pickup.Equipment.RarityLevel;
            _textColor = GetRarityTextColor(rarityValue);
            _backgroundColor = GetRarityBackgroundColor(rarityValue);

            if (_bgTexture == null)
            {
                _bgTexture = MakeSolidTexture(1, 1, Color.white);
            }

            LoadTexturesOnce();
            BuildStyle();
        }

        public void Initialize_Item(ItemPickup itemPickup)
        {
            _itemPickup = itemPickup;
            _pickup = null;

            if (!IsLocalPlayerAllowedToPickup(itemPickup))
            {
                this.enabled = false;
                return;
            }

            _mainCamera = Camera.main;

            GenericItemDescriptor descriptor = new GenericItemDescriptor(itemPickup.Item);
            LocLabel locLabel = new LocLabel();
            descriptor.SetNameToLabel(locLabel);

            string formattedName = !string.IsNullOrEmpty(locLabel.text) ? locLabel.text : descriptor.Name;
            _labelText = formattedName;

            _isUber = false;
            _isLucky = false;
            _isChaos = false;

            int rarityValue = (int)descriptor.Rarity;
            _textColor = GetRarityTextColor(rarityValue);
            _backgroundColor = GetRarityBackgroundColor(rarityValue);

            if (_bgTexture == null)
            {
                _bgTexture = MakeSolidTexture(1, 1, Color.white);
            }

            LoadTexturesOnce();
            BuildStyle();
        }

        private void BuildStyle()
        {
            _style = new GUIStyle
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
            _style.normal.textColor = _textColor;
        }

        private Color GetRarityTextColor(int rarityValue)
        {
            switch (rarityValue)
            {
                case 1: // Soft Sky Blue (Lightened)
                    return new Color(0.78f, 0.88f, 1.0f);
                case 2: // Pastel Lavender (Lightened)
                    return new Color(0.89f, 0.79f, 0.98f);
                case 3: // Bright Warm Yellow (Lightened)
                    return new Color(1.0f, 0.88f, 0.60f);
                case 4: // Soft Coral Pink (Lightened)
                    return new Color(0.98f, 0.72f, 0.76f);
                default:
                    return Color.white;
            }
        }

        private Color GetRarityBackgroundColor(int rarityValue)
        {
            switch (rarityValue)
            {
                case 1:
                    return new Color(0.12f, 0.18f, 0.28f, 0.9f);
                case 2:
                    return new Color(0.18f, 0.16f, 0.24f, 0.9f);
                case 3:
                    return new Color(0.22f, 0.18f, 0.12f, 0.9f);
                case 4:
                    return new Color(0.24f, 0.14f, 0.16f, 0.9f);
                default:
                    return new Color(0.12f, 0.12f, 0.12f, 0.85f);
            }
        }

        public bool IsLocalPlayerAllowedToPickup(PickupItemWithUI pickup)
        {
            if (pickup == null) return false;

            var localPlayer = RR.PlayerManager.Instance?.LocalPlayer;
            if (localPlayer == null) return false;

            if (pickup.PlayerFilter != RR.Game.Perk.PlayerFilter.AnyPlayer)
            {
                int localSlot = localPlayer.SlotIndex;
                int allowedSlot = (int)pickup.PlayerFilter;

                if (localSlot != allowedSlot)
                {
                    return false;
                }
            }

            return true;
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
            GameObject pickupObject = null;
            if (_pickup != null)
            {
                pickupObject = _pickup.gameObject;
            }
            else if (_itemPickup != null)
            {
                pickupObject = _itemPickup.gameObject;
            }

            if (pickupObject == null || !pickupObject.activeInHierarchy) return;

            if (RR.UI.UISystem.UIManager.Instance != null && RR.UI.UISystem.UIManager.Instance.IsMainLayerPageOpen)
            {
                return;
            }

            if (_mainCamera == null) _mainCamera = Camera.main;
            if (!this.enabled || _mainCamera == null) return;

            Vector3 worldPos = transform.position + new Vector3(0, 1.8f, 0);
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);

            if (screenPos.z <= 0) return;

            _style.alignment = TextAnchor.MiddleLeft;
            Vector2 textSize = _style.CalcSize(new GUIContent(_labelText));

            float paddingX = 16f;
            float paddingY = 8f;
            float iconSize = 16f;
            float iconSpacing = 2f;

            float totalIconWidth = 0f;
            if (_isUber) totalIconWidth += iconSpacing + iconSize;
            if (_isLucky) totalIconWidth += iconSize + iconSpacing;
            if (_isChaos) totalIconWidth += iconSpacing + iconSize;

            float boxWidth = textSize.x + totalIconWidth + paddingX;
            float boxHeight = textSize.y + paddingY;

            float boxX = screenPos.x - (boxWidth / 2f);
            float boxY = Screen.height - screenPos.y - (boxHeight / 2f);

            Rect bgRect = new Rect(boxX, boxY, boxWidth, boxHeight);

            Color savedGUIColor = GUI.color;
            GUI.color = _backgroundColor;
            GUI.DrawTexture(bgRect, _bgTexture);
            GUI.color = savedGUIColor;

            float contentStartX = boxX + (paddingX / 2f);
            Rect textRect = new Rect(contentStartX, boxY, textSize.x, boxHeight);

            GUIStyle outlineStyle = new GUIStyle(_style)
            {
                alignment = TextAnchor.MiddleLeft
            };
            outlineStyle.normal.textColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            GUI.Label(new Rect(textRect.x - 2, textRect.y, textRect.width, textRect.height), _labelText, outlineStyle);
            GUI.Label(new Rect(textRect.x + 2, textRect.y, textRect.width, textRect.height), _labelText, outlineStyle);
            GUI.Label(new Rect(textRect.x, textRect.y - 2, textRect.width, textRect.height), _labelText, outlineStyle);
            GUI.Label(new Rect(textRect.x, textRect.y + 2, textRect.width, textRect.height), _labelText, outlineStyle);
            GUI.Label(textRect, _labelText, _style);

            float currentIconX = textRect.xMax + iconSpacing;
            float iconY = boxY + (boxHeight - iconSize) / 2f;

            if (_isUber && _uberTexture != null)
            {
                Rect uberRect = new Rect(currentIconX, iconY, iconSize, iconSize);
                GUI.DrawTexture(uberRect, _uberTexture, ScaleMode.ScaleToFit, alphaBlend: true);
                currentIconX += iconSize + iconSpacing;
            }

            if (_isLucky && _luckyTexture != null)
            {
                Rect luckyRect = new Rect(currentIconX, iconY - 3f, iconSize + 6f, iconSize + 6f);
                GUI.DrawTexture(luckyRect, _luckyTexture, ScaleMode.ScaleToFit, alphaBlend: true);
                currentIconX += iconSize + 6f + iconSpacing;
            }

            if (_isChaos && _chaosTexture != null)
            {
                Rect chaosRect = new Rect(currentIconX, iconY, iconSize, iconSize);
                GUI.DrawTexture(chaosRect, _chaosTexture, ScaleMode.ScaleToFit, alphaBlend: true);
            }
        }
    }
}