using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RaidersOfBlackveilMod;

namespace BlackveilDpsMeter
{
    internal static class GraphUiPalette
    {
        internal static readonly Color Window = new Color(0.055f, 0.052f, 0.047f, 0.98f);
        internal static readonly Color Header = new Color(0.12f, 0.105f, 0.074f, 1f);
        internal static readonly Color Panel = new Color(0.075f, 0.07f, 0.061f, 0.92f);
        internal static readonly Color Sidebar = new Color(0.047f, 0.045f, 0.041f, 1f);
        internal static readonly Color Plot = new Color(0.025f, 0.026f, 0.028f, 0.92f);
        internal static readonly Color Border = new Color(0.48f, 0.39f, 0.22f, 1f);
        internal static readonly Color Gold = new Color(0.82f, 0.69f, 0.39f, 1f);
        internal static readonly Color Text = new Color(0.86f, 0.84f, 0.78f, 1f);
        internal static readonly Color MutedText = new Color(0.62f, 0.6f, 0.55f, 1f);
        internal static readonly Color PerkText = new Color(0.92f, 0.91f, 0.87f, 1f);
        internal static readonly Color PerkUpgradeOne = new Color(0.62f, 0.82f, 0.65f, 1f);
        internal static readonly Color PerkUpgradeTwo = new Color(0.96f, 0.62f, 0.32f, 1f);
        internal static readonly Color PerkTalent = new Color(0.58f, 0.76f, 0.96f, 1f);
        internal static readonly Color Button = new Color(0.12f, 0.115f, 0.102f, 1f);
        internal static readonly Color SelectedButton = new Color(0.23f, 0.185f, 0.105f, 1f);
        internal static readonly Color Grid = new Color(0.39f, 0.35f, 0.27f, 0.48f);
        internal static readonly Color TotalArea = new Color(0.58f, 0.59f, 0.58f, 0.78f);
        internal static readonly Color TotalLine = new Color(0.94f, 0.91f, 0.81f, 1f);
    }

    public static class StageDamageTypes
    {
        public static readonly string[] Names =
        {
            "Burn", "Poison", "Bleed", "Shock", "Root", "Frost", "Curse", "Minion", "Bless", "Fury"
        };

        public static readonly Color[] Colors =
        {
            new Color(0.95f, 0.35f, 0.14f, 0.72f),
            new Color(0.61f, 0.2f, 0.8f, 0.72f),
            new Color(0.82f, 0.26f, 0.25f, 0.72f),
            new Color(0.88f, 0.76f, 0.18f, 0.72f),
            new Color(0.71f, 0.52f, 0.43f, 0.72f),
            new Color(0.25f, 0.77f, 0.85f, 0.72f),
            new Color(0.12f, 0.52f, 0.37f, 0.72f),
            new Color(0.9f, 0.37f, 0.78f, 0.72f),
            new Color(0.85f, 0.95f, 0.61f, 0.72f),
            new Color(0.52f, 0.2f, 0.12f, 0.72f)
        };

        public static float[] GetDamageValues(PlayerStats stats)
        {
            return new[]
            {
                stats.TotalBurn,
                stats.TotalPoison,
                stats.TotalBleed,
                stats.TotalShock,
                stats.TotalRoot,
                stats.TotalFrost,
                stats.TotalCurse,
                stats.TotalMinion,
                stats.TotalBless,
                stats.TotalFury
            };
        }
    }

    public sealed class StageDpsRecord
    {
        public int StageNumber { get; }
        public string SceneName { get; }
        public float TotalDamage { get; }
        public float CombatTime { get; }
        public float Dps { get; }
        public float[] DamageByType { get; }
        public string[] CollectedPerks { get; }
        public int[] CollectedPerkIds { get; }
        public int[] PerkUpgradeLevels { get; }
        public bool[] PerkTalents { get; }
        public bool[] MasterTalents { get; }

        public StageDpsRecord(
            int stageNumber,
            string sceneName,
            PlayerStats playerStats,
            float combatTime,
            string[] collectedPerks = null,
            int[] perkUpgradeLevels = null,
            bool[] perkTalents = null,
            bool[] masterTalents = null,
            int[] collectedPerkIds = null)
        {
            StageNumber = stageNumber;
            SceneName = sceneName;
            TotalDamage = playerStats.TotalDamage;
            CombatTime = combatTime;
            Dps = TotalDamage / Mathf.Max(0.1f, combatTime);
            DamageByType = StageDamageTypes.GetDamageValues(playerStats);
            CollectedPerks = collectedPerks != null ? (string[])collectedPerks.Clone() : Array.Empty<string>();
            CollectedPerkIds = collectedPerkIds != null && collectedPerkIds.Length == CollectedPerks.Length
                ? (int[])collectedPerkIds.Clone()
                : new int[CollectedPerks.Length];
            PerkUpgradeLevels = perkUpgradeLevels != null && perkUpgradeLevels.Length == CollectedPerks.Length
                ? (int[])perkUpgradeLevels.Clone()
                : new int[CollectedPerks.Length];
            PerkTalents = perkTalents != null && perkTalents.Length == CollectedPerks.Length
                ? (bool[])perkTalents.Clone()
                : new bool[CollectedPerks.Length];
            MasterTalents = masterTalents != null && masterTalents.Length == CollectedPerks.Length
                ? (bool[])masterTalents.Clone()
                : new bool[CollectedPerks.Length];
        }
    }

    public sealed class CombatStatsRecord
    {
        public int StageNumber { get; }
        public string SceneName { get; }
        public string SelectedElement { get; }
        public float[] Values { get; }
        public float[] MaximumValues { get; }

        public CombatStatsRecord(
            int stageNumber,
            string sceneName,
            string selectedElement,
            float[] values,
            float[] maximumValues)
        {
            StageNumber = stageNumber;
            SceneName = sceneName;
            SelectedElement = selectedElement;
            Values = values != null ? (float[])values.Clone() : new float[13];
            MaximumValues = maximumValues != null && maximumValues.Length == Values.Length
                ? (float[])maximumValues.Clone()
                : (float[])Values.Clone();
        }
    }

    public sealed class CombatStatsLineGraphic : MaskableGraphic
    {
        public IList<CombatStatsRecord> Records { get; set; }
        public int[] StatIndexes { get; set; }
        public Color[] SeriesColors { get; set; }
        public float AxisMinimum { get; set; }
        public float AxisMaximum { get; set; } = 1f;
        public float LineThickness { get; set; } = 2.5f;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            if (Records == null || Records.Count == 0 || StatIndexes == null ||
                SeriesColors == null || StatIndexes.Length != SeriesColors.Length ||
                AxisMaximum <= AxisMinimum)
            {
                return;
            }

            Rect rect = GetPixelAdjustedRect();
            for (int tick = 0; tick <= 4; tick++)
            {
                float y = rect.yMin + rect.height * tick / 4f;
                AddQuad(vertexHelper,
                    new Vector2(rect.xMin, y - 0.5f),
                    new Vector2(rect.xMax, y - 0.5f),
                    new Vector2(rect.xMax, y + 0.5f),
                    new Vector2(rect.xMin, y + 0.5f),
                    GraphUiPalette.Grid);
            }

            for (int i = 0; i < StatIndexes.Length; i++)
            {
                DrawSeries(vertexHelper, rect, StatIndexes[i], true, SeriesColors[i]);
                DrawSeries(vertexHelper, rect, StatIndexes[i], false, Color.Lerp(SeriesColors[i], Color.white, 0.45f));
            }
        }

        private void DrawSeries(VertexHelper vertexHelper, Rect rect, int statIndex, bool maximum, Color seriesColor)
        {
            if (Records.Count == 1)
            {
                float value = maximum ? Records[0].MaximumValues[statIndex] : Records[0].Values[statIndex];
                AddMarker(vertexHelper, rect.center.x, GetY(rect, value), seriesColor);
                return;
            }

            for (int i = 0; i < Records.Count - 1; i++)
            {
                float startX = rect.xMin + rect.width * i / (Records.Count - 1);
                float endX = rect.xMin + rect.width * (i + 1) / (Records.Count - 1);
                float startValue = maximum ? Records[i].MaximumValues[statIndex] : Records[i].Values[statIndex];
                float endValue = maximum ? Records[i + 1].MaximumValues[statIndex] : Records[i + 1].Values[statIndex];
                float startY = GetY(rect, startValue);
                float endY = GetY(rect, endValue);
                AddLine(vertexHelper, new Vector2(startX, startY), new Vector2(endX, endY), seriesColor);
                AddMarker(vertexHelper, startX, startY, seriesColor);
                if (i == Records.Count - 2)
                {
                    AddMarker(vertexHelper, endX, endY, seriesColor);
                }
            }
        }

        private float GetY(Rect rect, float value)
        {
            return rect.yMin + rect.height * Mathf.Clamp01((value - AxisMinimum) / (AxisMaximum - AxisMinimum));
        }

        private void AddLine(VertexHelper vertexHelper, Vector2 start, Vector2 end, Color lineColor)
        {
            Vector2 direction = (end - start).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * (LineThickness * 0.5f);
            AddQuad(vertexHelper, start - normal, end - normal, end + normal, start + normal, lineColor);
        }

        private void AddMarker(VertexHelper vertexHelper, float x, float y, Color markerColor)
        {
            const float markerSize = 3.5f;
            AddQuad(vertexHelper,
                new Vector2(x - markerSize, y - markerSize),
                new Vector2(x + markerSize, y - markerSize),
                new Vector2(x + markerSize, y + markerSize),
                new Vector2(x - markerSize, y + markerSize),
                markerColor);
        }

        private static void AddQuad(VertexHelper vertexHelper, Vector2 bottomLeft, Vector2 bottomRight, Vector2 topRight, Vector2 topLeft, Color quadColor)
        {
            int firstVertex = vertexHelper.currentVertCount;
            AddVertex(vertexHelper, bottomLeft, quadColor);
            AddVertex(vertexHelper, bottomRight, quadColor);
            AddVertex(vertexHelper, topRight, quadColor);
            AddVertex(vertexHelper, topLeft, quadColor);
            vertexHelper.AddTriangle(firstVertex, firstVertex + 1, firstVertex + 2);
            vertexHelper.AddTriangle(firstVertex, firstVertex + 2, firstVertex + 3);
        }

        private static void AddVertex(VertexHelper vertexHelper, Vector2 position, Color vertexColor)
        {
            var vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = vertexColor;
            vertexHelper.AddVert(vertex);
        }
    }

    public class StageDpsAreaGraphic : MaskableGraphic
    {
        public IList<StageDpsRecord> Records { get; set; }
        public float AxisMaximum { get; set; } = 1f;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            if (Records == null || Records.Count == 0 || AxisMaximum <= 0f)
            {
                return;
            }

            Rect rect = GetPixelAdjustedRect();
            int typeCount = StageDamageTypes.Names.Length;
            int segmentCount = Mathf.Max(1, Records.Count - 1);
            for (int type = -1; type < typeCount; type++)
            {
                Color areaColor = type < 0
                    ? GraphUiPalette.TotalArea * color
                    : StageDamageTypes.Colors[type] * color;
                for (int segment = 0; segment < segmentCount; segment++)
                {
                    int leftIndex = Records.Count == 1 ? 0 : segment;
                    int rightIndex = Records.Count == 1 ? 0 : segment + 1;
                    if (type >= 0 &&
                        segment == segmentCount - 1 &&
                        rightIndex == Records.Count - 1 &&
                        Records[rightIndex].DamageByType[type] <= 0f)
                    {
                        continue;
                    }

                    float leftX = Records.Count == 1 ? 0.4f : leftIndex / (float)(Records.Count - 1);
                    float rightX = Records.Count == 1 ? 0.6f : rightIndex / (float)(Records.Count - 1);
                    float leftBottom = GetStackValue(leftIndex, type, false) / AxisMaximum;
                    float rightBottom = GetStackValue(rightIndex, type, false) / AxisMaximum;
                    float leftTop = GetStackValue(leftIndex, type, true) / AxisMaximum;
                    float rightTop = GetStackValue(rightIndex, type, true) / AxisMaximum;

                    AddQuad(
                        vertexHelper,
                        new Vector2(rect.xMin + rect.width * leftX, rect.yMin + rect.height * leftBottom),
                        new Vector2(rect.xMin + rect.width * rightX, rect.yMin + rect.height * rightBottom),
                        new Vector2(rect.xMin + rect.width * rightX, rect.yMin + rect.height * rightTop),
                        new Vector2(rect.xMin + rect.width * leftX, rect.yMin + rect.height * leftTop),
                        areaColor);
                }
            }
        }

        private float GetStackValue(int recordIndex, int type, bool includeCurrentType)
        {
            if (type < 0)
            {
                return includeCurrentType ? Records[recordIndex].Dps : 0f;
            }

            float value = 0f;
            int end = includeCurrentType ? type : type - 1;
            for (int i = 0; i <= end; i++)
            {
                value += Records[recordIndex].DamageByType[i] / Mathf.Max(0.1f, Records[recordIndex].CombatTime);
            }
            return value;
        }

        private static void AddQuad(VertexHelper vertexHelper, Vector2 bottomLeft, Vector2 bottomRight, Vector2 topRight, Vector2 topLeft, Color color)
        {
            int start = vertexHelper.currentVertCount;
            AddVertex(vertexHelper, bottomLeft, color);
            AddVertex(vertexHelper, bottomRight, color);
            AddVertex(vertexHelper, topRight, color);
            AddVertex(vertexHelper, topLeft, color);
            vertexHelper.AddTriangle(start, start + 1, start + 2);
            vertexHelper.AddTriangle(start, start + 2, start + 3);
        }

        private static void AddVertex(VertexHelper vertexHelper, Vector2 position, Color color)
        {
            var vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            vertexHelper.AddVert(vertex);
        }
    }

    public class StageDpsLineGraphic : MaskableGraphic
    {
        public IList<StageDpsRecord> Records { get; set; }
        public float AxisMaximum { get; set; } = 1f;
        public float LineThickness { get; set; } = 3f;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            if (Records == null || Records.Count == 0 || AxisMaximum <= 0f)
            {
                return;
            }

            Rect rect = GetPixelAdjustedRect();

            float graphHeight = rect.height;

            if (Records.Count == 1)
            {
                float y = rect.yMin + graphHeight * Mathf.Clamp01(Records[0].Dps / AxisMaximum);
                float halfSize = LineThickness * 1.5f;
                AddQuad(
                    vertexHelper,
                    new Vector2(rect.center.x - halfSize, y - halfSize),
                    new Vector2(rect.center.x + halfSize, y - halfSize),
                    new Vector2(rect.center.x + halfSize, y + halfSize),
                    new Vector2(rect.center.x - halfSize, y + halfSize),
                    color);
                return;
            }

            int segmentCount = Mathf.Max(1, Records.Count - 1);
            for (int segment = 0; segment < segmentCount; segment++)
            {
                int leftIndex = segment;
                int rightIndex = segment + 1;
                float leftX = leftIndex / (float)(Records.Count - 1);
                float rightX = rightIndex / (float)(Records.Count - 1);
                float leftDps = Records[leftIndex].Dps;
                float rightDps = Records[rightIndex].Dps;
                Vector2 start = new Vector2(rect.xMin + rect.width * leftX, rect.yMin + graphHeight * Mathf.Clamp01(leftDps / AxisMaximum));
                Vector2 end = new Vector2(rect.xMin + rect.width * rightX, rect.yMin + graphHeight * Mathf.Clamp01(rightDps / AxisMaximum));
                Vector2 direction = (end - start).normalized;
                Vector2 normal = new Vector2(-direction.y, direction.x) * (LineThickness * 0.5f);

                int firstVertex = vertexHelper.currentVertCount;
                AddVertex(vertexHelper, start - normal, color);
                AddVertex(vertexHelper, end - normal, color);
                AddVertex(vertexHelper, end + normal, color);
                AddVertex(vertexHelper, start + normal, color);
                vertexHelper.AddTriangle(firstVertex, firstVertex + 1, firstVertex + 2);
                vertexHelper.AddTriangle(firstVertex, firstVertex + 2, firstVertex + 3);
            }
        }

        private static void AddVertex(VertexHelper vertexHelper, Vector2 position, Color color)
        {
            var vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            vertexHelper.AddVert(vertex);
        }

        private static void AddQuad(VertexHelper vertexHelper, Vector2 bottomLeft, Vector2 bottomRight, Vector2 topRight, Vector2 topLeft, Color color)
        {
            int firstVertex = vertexHelper.currentVertCount;
            AddVertex(vertexHelper, bottomLeft, color);
            AddVertex(vertexHelper, bottomRight, color);
            AddVertex(vertexHelper, topRight, color);
            AddVertex(vertexHelper, topLeft, color);
            vertexHelper.AddTriangle(firstVertex, firstVertex + 1, firstVertex + 2);
            vertexHelper.AddTriangle(firstVertex, firstVertex + 2, firstVertex + 3);
        }
    }

    public class GraphDashboardWindow : MonoBehaviour
    {
        private static readonly Color AccentColor = GraphUiPalette.Gold;
        private static readonly Color MutedColor = GraphUiPalette.MutedText;
        private static readonly string[] CombatStatNames =
        {
            "Magic Power",
            "Physical Power",
            "Shock",
            "Burn",
            "Chill",
            "Poison",
            "Bleed",
            "Fury",
            "Curse",
            "Summon",
            "Bless",
            "Critical Rate",
            "Critical Damage"
        };
        private static readonly Color[] CombatStatColors =
        {
            new Color(0.57f, 0.72f, 0.88f, 1f),
            new Color(0.84f, 0.58f, 0.48f, 1f),
            new Color(0.88f, 0.76f, 0.18f, 1f),
            new Color(0.95f, 0.35f, 0.14f, 1f),
            new Color(0.25f, 0.77f, 0.85f, 1f),
            new Color(0.61f, 0.2f, 0.8f, 1f),
            new Color(0.82f, 0.26f, 0.25f, 1f),
            new Color(0.52f, 0.2f, 0.12f, 1f),
            new Color(0.12f, 0.52f, 0.37f, 1f),
            new Color(0.9f, 0.37f, 0.78f, 1f),
            new Color(0.85f, 0.95f, 0.61f, 1f),
            new Color(0.72f, 0.57f, 0.79f, 1f),
            new Color(0.54f, 0.73f, 0.61f, 1f)
        };

        private sealed class CombatGraphView
        {
            public GameObject Root;
            public CombatStatsLineGraphic Graph;
            public int[] StatIndexes;
            public TextMeshProUGUI MinimumLabel;
            public TextMeshProUGUI MaximumLabel;
            public RectTransform StageLabels;
            public readonly List<TextMeshProUGUI> LegendLabels = new List<TextMeshProUGUI>();
        }

        private sealed class StageWindowPerkLabel
        {
            public int RecordIndex;
            public RectTransform Root;
        }

        private Canvas _canvas;
        private bool _visible;
        private RectTransform _contentRoot;
        private RectTransform _plotRoot;
        private RectTransform _stageWindowRoot;
        private RectTransform _stageWindowViewport;
        private RectTransform _stageWindowPerkOverlay;
        private RectTransform _stageWindowContent;
        private ScrollRect _stageWindowScrollRect;
        private StageDpsAreaGraphic _stageWindowAreaGraph;
        private StageDpsLineGraphic _stageWindowLineGraph;
        private TextMeshProUGUI _stageWindowEmptyMessage;
        private GameObject _combatStatsRoot;
        private RectTransform _perkTableRoot;
        private RectTransform _perkTableContent;
        private TextMeshProUGUI _perkTableEmptyMessage;
        private TextMeshProUGUI _sectionTitle;
        private TextMeshProUGUI _emptyMessage;
        private TextMeshProUGUI _xAxisTitle;
        private TextMeshProUGUI _yAxisTitle;
        private readonly List<UnityEngine.UI.Button> _sectionButtons = new List<UnityEngine.UI.Button>();
        private readonly List<TextMeshProUGUI> _sectionLabels = new List<TextMeshProUGUI>();
        private readonly List<UnityEngine.UI.Button> _stageSubtabButtons = new List<UnityEngine.UI.Button>();
        private readonly List<TextMeshProUGUI> _stageSubtabLabels = new List<TextMeshProUGUI>();
        private readonly List<TextMeshProUGUI> _stageWindowAxisLabels = new List<TextMeshProUGUI>();
        private readonly List<GameObject> _stageWindowStageLabels = new List<GameObject>();
        private readonly List<StageWindowPerkLabel> _stageWindowPerkLabels = new List<StageWindowPerkLabel>();
        private readonly List<GameObject> _chartLabels = new List<GameObject>();
        private readonly List<GameObject> _legendObjects = new List<GameObject>();
        private readonly List<UnityEngine.UI.Button> _combatSubtabButtons = new List<UnityEngine.UI.Button>();
        private readonly List<TextMeshProUGUI> _combatSubtabLabels = new List<TextMeshProUGUI>();
        private readonly List<CombatGraphView> _combatGraphViews = new List<CombatGraphView>();
        private int _selectedCombatSubtab;
        private int _selectedStageSubtab;
        private int _selectedSection;

        private void Start()
        {
            var canvasObject = new GameObject("DPS_Graph_Dashboard_Canvas");
            canvasObject.transform.SetParent(transform, false);

            _canvas = canvasObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 1000;
            var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var windowObject = new GameObject("DPS_Graph_Dashboard_Window");
            windowObject.transform.SetParent(canvasObject.transform, false);
            var windowImage = windowObject.AddComponent<UnityEngine.UI.Image>();
            windowImage.color = GraphUiPalette.Window;
            windowImage.raycastTarget = false;

            var windowRect = windowImage.rectTransform;
            windowRect.anchorMin = new Vector2(0.025f, 0.025f);
            windowRect.anchorMax = new Vector2(0.975f, 0.975f);
            windowRect.offsetMin = Vector2.zero;
            windowRect.offsetMax = Vector2.zero;

            var outline = windowObject.AddComponent<Outline>();
            outline.effectColor = GraphUiPalette.Border;
            outline.effectDistance = new Vector2(2f, -2f);

            var headerObject = new GameObject("Dashboard_Header");
            headerObject.transform.SetParent(windowObject.transform, false);
            var headerImage = headerObject.AddComponent<UnityEngine.UI.Image>();
            headerImage.color = GraphUiPalette.Header;
            headerImage.raycastTarget = false;

            var headerRect = headerImage.rectTransform;
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0f, 64f);

            var title = CreateLabel(headerObject.transform, "Dashboard_Title", "Run Information Dashboard", 28f);
            title.alignment = TextAlignmentOptions.MidlineLeft;
            title.rectTransform.anchorMin = Vector2.zero;
            title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = new Vector2(24f, 0f);
            title.rectTransform.offsetMax = new Vector2(-24f, 0f);
            title.color = GraphUiPalette.Gold;

            var sidebar = CreatePanel(windowObject.transform, "Dashboard_Sections", GraphUiPalette.Sidebar);
            SetAnchors(sidebar, new Vector2(0.02f, 0.04f), new Vector2(0.2f, 0.88f));

            CreateSectionButton(sidebar.transform, 0, "Stage DPS");
            CreateSectionButton(sidebar.transform, 1, "Combat Stats");
            CreateSectionButton(sidebar.transform, 2, "Perks by stage");

            var content = CreatePanel(windowObject.transform, "Dashboard_Content", GraphUiPalette.Panel);
            _contentRoot = content;
            SetAnchors(_contentRoot, new Vector2(0.22f, 0.04f), new Vector2(0.98f, 0.88f));

            _sectionTitle = CreateLabel(content.transform, "Dashboard_Section_Title", string.Empty, 27f);
            _sectionTitle.alignment = TextAlignmentOptions.MidlineLeft;
            _sectionTitle.rectTransform.anchorMin = new Vector2(0f, 1f);
            _sectionTitle.rectTransform.anchorMax = new Vector2(1f, 1f);
            _sectionTitle.rectTransform.pivot = new Vector2(0.5f, 1f);
            _sectionTitle.rectTransform.anchoredPosition = new Vector2(0f, -16f);
            _sectionTitle.rectTransform.sizeDelta = new Vector2(-40f, 42f);
            SetTextAutoSize(_sectionTitle, 18f, 27f);

            _plotRoot = CreatePanel(content.transform, "Stage_DPS_Plot", GraphUiPalette.Plot);
            SetAnchors(_plotRoot, new Vector2(0.14f, 0.31f), new Vector2(0.97f, 0.83f));

            _yAxisTitle = CreateLabel(content.transform, "DPS_Axis_Title", "DPS in stage", 18f);
            _yAxisTitle.alignment = TextAlignmentOptions.Center;
            _yAxisTitle.rectTransform.anchorMin = new Vector2(0f, 0.4f);
            _yAxisTitle.rectTransform.anchorMax = new Vector2(0.08f, 0.6f);
            _yAxisTitle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            SetTextAutoSize(_yAxisTitle, 12f, 18f);

            _xAxisTitle = CreateLabel(content.transform, "Stage_Axis_Title", "Stage", 18f);
            _xAxisTitle.alignment = TextAlignmentOptions.Center;
            _xAxisTitle.rectTransform.anchorMin = new Vector2(0.14f, 0.225f);
            _xAxisTitle.rectTransform.anchorMax = new Vector2(0.97f, 0.265f);
            SetTextAutoSize(_xAxisTitle, 12f, 18f);

            _emptyMessage = CreateLabel(content.transform, "Dashboard_Empty_Message", string.Empty, 22f);
            _emptyMessage.alignment = TextAlignmentOptions.Center;
            _emptyMessage.rectTransform.anchorMin = new Vector2(0.2f, 0.4f);
            _emptyMessage.rectTransform.anchorMax = new Vector2(0.9f, 0.6f);

            CreateStageDpsWindowPanel(content.transform);
            CreateCombatStatsPanel(content.transform);
            CreatePerkTable(content.transform);
            CreateLegend();
            CreateStageDpsSubtabs(content.transform);
            Canvas.ForceUpdateCanvases();
            UpdateSelectedSection();
            RefreshStageGraph(Plugin.Instance.StageDpsRecords);
            RefreshCombatStatsGraph(Plugin.Instance.CombatStatsRecords);
            RefreshPerkTable(Plugin.Instance.StageDpsRecords);

            _canvas.enabled = _visible;
        }

        public void ToggleVisibility()
        {
            _visible = !_visible;
            if (_canvas != null)
            {
                _canvas.enabled = _visible;
            }
        }

        public void Hide()
        {
            _visible = false;
            if (_canvas != null)
            {
                _canvas.enabled = false;
            }
        }

        public void RefreshStageGraph(IList<StageDpsRecord> records)
        {
            if (_plotRoot == null || _selectedSection != 0)
            {
                return;
            }

            RefreshStageDpsWindow(records);
            if (_selectedStageSubtab != 0)
            {
                ClearChildren(_plotRoot);
                for (int i = 0; i < _chartLabels.Count; i++)
                {
                    UnityEngine.Object.Destroy(_chartLabels[i]);
                }
                _chartLabels.Clear();
                return;
            }

            ClearChildren(_plotRoot);
            for (int i = 0; i < _chartLabels.Count; i++)
            {
                UnityEngine.Object.Destroy(_chartLabels[i]);
            }
            _chartLabels.Clear();

            _emptyMessage.gameObject.SetActive(records.Count == 0);
            _emptyMessage.text = records.Count == 0 ? "No stage data yet. Complete a stage to see its DPS." : string.Empty;
            if (records.Count == 0)
            {
                return;
            }

            float maxDps = 0f;
            for (int i = 0; i < records.Count; i++)
            {
                // Use records[i].Dps directly as the total peak for that stage, 
                // avoiding the double-count caused by adding DamageByType a second time.
                maxDps = Mathf.Max(maxDps, records[i].Dps);
            }

            // Apply the 1.25x multiplier so the true peak sits at 80% (4/5) of the height
            float axisMaximum = Mathf.Max(1f, maxDps * 1.25f);
            for (int tick = 0; tick <= 4; tick++)
            {
                float normalizedY = tick / 4f;
                var gridLine = CreateImage(_plotRoot, "DPS_Grid_Line", GraphUiPalette.Grid);
                SetAnchors(gridLine.rectTransform, new Vector2(0f, normalizedY), new Vector2(1f, normalizedY));
                gridLine.rectTransform.sizeDelta = new Vector2(0f, 1f);

                var tickLabel = CreateLabel(_contentRoot, "DPS_Tick", (axisMaximum * normalizedY).ToString("F0"), 14f);
                tickLabel.alignment = TextAlignmentOptions.MidlineRight;
                tickLabel.color = MutedColor;
                tickLabel.rectTransform.anchorMin = new Vector2(0.055f, 0.31f + normalizedY * 0.52f);
                tickLabel.rectTransform.anchorMax = new Vector2(0.125f, 0.31f + normalizedY * 0.52f);
                tickLabel.rectTransform.sizeDelta = new Vector2(0f, 22f);
                tickLabel.rectTransform.anchoredPosition = new Vector2(0f, -11f);
                SetTextAutoSize(tickLabel, 10f, 14f);
                _chartLabels.Add(tickLabel.gameObject);
            }

            var areaObject = new GameObject("Stage_DPS_Areas");
            areaObject.transform.SetParent(_plotRoot, false);
            var areaRect = areaObject.AddComponent<RectTransform>();
            SetAnchors(areaRect, Vector2.zero, Vector2.one);
            var areaGraph = areaObject.AddComponent<StageDpsAreaGraphic>();
            areaGraph.Records = records;
            areaGraph.AxisMaximum = axisMaximum;
            areaGraph.color = Color.white;
            areaGraph.raycastTarget = false;
            areaGraph.SetVerticesDirty();

            var lineObject = new GameObject("Stage_DPS_Total_Line");
            lineObject.transform.SetParent(_plotRoot, false);
            var lineRect = lineObject.AddComponent<RectTransform>();
            SetAnchors(lineRect, Vector2.zero, Vector2.one);
            var lineGraph = lineObject.AddComponent<StageDpsLineGraphic>();
            lineGraph.Records = records;
            lineGraph.AxisMaximum = axisMaximum;
            lineGraph.color = GraphUiPalette.TotalLine;
            lineGraph.raycastTarget = false;
            lineGraph.SetVerticesDirty();

            for (int i = 0; i < records.Count; i++)
            {
                var stageLabel = CreateLabel(_contentRoot, "Stage_Number", records[i].StageNumber.ToString(), 14f);
                stageLabel.alignment = TextAlignmentOptions.Center;
                stageLabel.color = MutedColor;
                float normalizedX = records.Count == 1 ? 0.5f : i / (float)(records.Count - 1);
                float stageX = 0.14f + 0.83f * normalizedX;
                float labelWidth = Mathf.Min(0.08f, 0.83f / records.Count * 0.9f);
                stageLabel.rectTransform.anchorMin = new Vector2(stageX - labelWidth * 0.5f, 0.275f);
                stageLabel.rectTransform.anchorMax = new Vector2(stageX + labelWidth * 0.5f, 0.305f);
                SetTextAutoSize(stageLabel, 9f, 14f);
                _chartLabels.Add(stageLabel.gameObject);
            }
        }

        private void CreateStageDpsSubtabs(Transform parent)
        {
            CreateStageDpsSubtabButton(parent, 0, "All stages", 0.035f, 0.17f);
            CreateStageDpsSubtabButton(parent, 1, "20-stage window", 0.18f, 0.36f);
            UpdateStageDpsSubtabs();
        }

        private void CreateStageDpsSubtabButton(Transform parent, int subtabIndex, string text, float xMin, float xMax)
        {
            var buttonImage = CreateImage(parent, "Stage_DPS_Subtab_Button", GraphUiPalette.Button);
            buttonImage.raycastTarget = true;
            SetAnchors(buttonImage.rectTransform, new Vector2(xMin, 0.85f), new Vector2(xMax, 0.92f));
            var button = buttonImage.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(() =>
            {
                _selectedStageSubtab = subtabIndex;
                UpdateStageDpsSubtabs();
                if (_selectedSection == 0)
                {
                    RefreshStageGraph(Plugin.Instance.StageDpsRecords);
                }
            });

            var label = CreateLabel(buttonImage.transform, "Stage_DPS_Subtab_Label", text, 15f);
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(3f, 0f);
            label.rectTransform.offsetMax = new Vector2(-3f, 0f);
            SetTextAutoSize(label, 9f, 15f);
            _stageSubtabButtons.Add(button);
            _stageSubtabLabels.Add(label);
        }

        private void CreateStageDpsWindowPanel(Transform parent)
        {
            _stageWindowRoot = CreatePanel(parent, "Stage_DPS_Window", GraphUiPalette.Plot);
            SetAnchors(_stageWindowRoot, new Vector2(0.035f, 0.055f), new Vector2(0.98f, 0.82f));

            var viewportImage = CreateImage(_stageWindowRoot, "Stage_DPS_Window_Viewport", GraphUiPalette.Plot);
            viewportImage.raycastTarget = true;
            _stageWindowViewport = viewportImage.rectTransform;
            SetAnchors(_stageWindowViewport, new Vector2(0.12f, 0.17f), new Vector2(0.97f, 0.83f));
            _stageWindowViewport.gameObject.AddComponent<RectMask2D>();

            var perkOverlayObject = new GameObject("Stage_DPS_Window_Perk_Overlay");
            perkOverlayObject.transform.SetParent(_stageWindowRoot, false);
            _stageWindowPerkOverlay = perkOverlayObject.AddComponent<RectTransform>();
            const float perkOverlayLeftPadding = 0.015f;
            SetAnchors(
                _stageWindowPerkOverlay,
                new Vector2(_stageWindowViewport.anchorMin.x - perkOverlayLeftPadding * 2f, _stageWindowViewport.anchorMin.y),
                new Vector2(_stageWindowViewport.anchorMax.x, 1f));
            perkOverlayObject.AddComponent<RectMask2D>();

            var contentObject = new GameObject("Stage_DPS_Window_Content");
            contentObject.transform.SetParent(_stageWindowViewport, false);
            _stageWindowContent = contentObject.AddComponent<RectTransform>();
            _stageWindowContent.anchorMin = new Vector2(0f, 0f);
            _stageWindowContent.anchorMax = new Vector2(0f, 1f);
            _stageWindowContent.pivot = new Vector2(0f, 0.5f);

            _stageWindowScrollRect = _stageWindowRoot.gameObject.AddComponent<ScrollRect>();
            _stageWindowScrollRect.viewport = _stageWindowViewport;
            _stageWindowScrollRect.content = _stageWindowContent;
            _stageWindowScrollRect.horizontal = true;
            _stageWindowScrollRect.vertical = false;
            _stageWindowScrollRect.movementType = ScrollRect.MovementType.Clamped;
            _stageWindowScrollRect.onValueChanged.AddListener(OnStageDpsWindowScrolled);

            var scrollbarImage = CreateImage(_stageWindowRoot, "Stage_DPS_Window_Scrollbar", GraphUiPalette.Header);
            scrollbarImage.raycastTarget = true;
            SetAnchors(scrollbarImage.rectTransform, new Vector2(0.12f, 0.08f), new Vector2(0.97f, 0.125f));
            var scrollbar = scrollbarImage.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.LeftToRight;

            var handleImage = CreateImage(scrollbarImage.transform, "Stage_DPS_Window_Scrollbar_Handle", GraphUiPalette.Gold);
            SetAnchors(handleImage.rectTransform, Vector2.zero, Vector2.one);
            scrollbar.handleRect = handleImage.rectTransform;
            scrollbar.targetGraphic = handleImage;
            _stageWindowScrollRect.horizontalScrollbar = scrollbar;
            _stageWindowScrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            _stageWindowScrollRect.horizontalScrollbarSpacing = -2f;

            var xAxisTitle = CreateLabel(_stageWindowRoot, "Stage_DPS_Window_X_Axis_Title", "Stage", 14f);
            xAxisTitle.alignment = TextAlignmentOptions.Center;
            SetAnchors(xAxisTitle.rectTransform, new Vector2(0.12f, 0.015f), new Vector2(0.97f, 0.07f));
            SetTextAutoSize(xAxisTitle, 9f, 14f);

            var yAxisTitle = CreateLabel(_stageWindowRoot, "Stage_DPS_Window_Y_Axis_Title", "DPS", 14f);
            yAxisTitle.alignment = TextAlignmentOptions.Center;
            yAxisTitle.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            SetAnchors(yAxisTitle.rectTransform, new Vector2(0f, 0.46f), new Vector2(0.1f, 0.54f));
            SetTextAutoSize(yAxisTitle, 9f, 14f);

            for (int tick = 0; tick <= 4; tick++)
            {
                float normalizedY = tick / 4f;
                float plotY = 0.17f + 0.66f * (0.16f + normalizedY * 0.75f);
                var tickLabel = CreateLabel(_stageWindowRoot, "Stage_DPS_Window_Axis_Tick", "0", 11f);
                tickLabel.alignment = TextAlignmentOptions.MidlineRight;
                tickLabel.color = MutedColor;
                tickLabel.rectTransform.anchorMin = new Vector2(0.015f, plotY);
                tickLabel.rectTransform.anchorMax = new Vector2(0.105f, plotY);
                tickLabel.rectTransform.sizeDelta = new Vector2(0f, 20f);
                tickLabel.rectTransform.anchoredPosition = new Vector2(0f, -10f);
                SetTextAutoSize(tickLabel, 8f, 11f);
                _stageWindowAxisLabels.Add(tickLabel);
            }

            _stageWindowEmptyMessage = CreateLabel(
                _stageWindowRoot,
                "Stage_DPS_Window_Empty_Message",
                "No stage data yet. Complete a stage to see its DPS.",
                18f);
            _stageWindowEmptyMessage.alignment = TextAlignmentOptions.Center;
            SetAnchors(_stageWindowEmptyMessage.rectTransform, new Vector2(0.15f, 0.4f), new Vector2(0.94f, 0.6f));
            SetTextAutoSize(_stageWindowEmptyMessage, 11f, 18f);
            _stageWindowPerkOverlay.SetAsLastSibling();
        }

        private void RefreshStageDpsWindow(IList<StageDpsRecord> records)
        {
            if (_stageWindowContent == null)
            {
                return;
            }

            ClearChildren(_stageWindowContent);
            ClearChildren(_stageWindowPerkOverlay);
            _stageWindowStageLabels.Clear();
            _stageWindowPerkLabels.Clear();
            _stageWindowAreaGraph = null;
            _stageWindowLineGraph = null;

            bool hasRecords = records != null && records.Count > 0;
            _stageWindowEmptyMessage.gameObject.SetActive(!hasRecords);
            _stageWindowScrollRect.enabled = hasRecords;
            float viewportWidth = Mathf.Max(1f, _stageWindowViewport.rect.width);
            float widthMultiplier = hasRecords
                ? Mathf.Max(1f, (records.Count - 1f) / 19f)
                : 1f;
            float contentWidth = viewportWidth * widthMultiplier;
            _stageWindowContent.sizeDelta = new Vector2(contentWidth, 0f);

            if (hasRecords)
            {
                var gridRoot = new GameObject("Stage_DPS_Window_Grid");
                gridRoot.transform.SetParent(_stageWindowContent, false);
                var gridRect = gridRoot.AddComponent<RectTransform>();
                SetAnchors(gridRect, new Vector2(0f, 0.16f), new Vector2(1f, 0.91f));
                for (int tick = 0; tick <= 4; tick++)
                {
                    var gridLine = CreateImage(gridRoot.transform, "Stage_DPS_Window_Grid_Line", GraphUiPalette.Grid);
                    float normalizedY = tick / 4f;
                    SetAnchors(gridLine.rectTransform, new Vector2(0f, normalizedY), new Vector2(1f, normalizedY));
                    gridLine.rectTransform.sizeDelta = new Vector2(0f, 1f);
                }

                var areaObject = new GameObject("Stage_DPS_Window_Areas");
                areaObject.transform.SetParent(_stageWindowContent, false);
                var areaRect = areaObject.AddComponent<RectTransform>();
                SetAnchors(areaRect, new Vector2(0f, 0.16f), new Vector2(1f, 0.91f));
                _stageWindowAreaGraph = areaObject.AddComponent<StageDpsAreaGraphic>();
                _stageWindowAreaGraph.Records = records;
                _stageWindowAreaGraph.color = Color.white;
                _stageWindowAreaGraph.raycastTarget = false;

                var lineObject = new GameObject("Stage_DPS_Window_Total_Line");
                lineObject.transform.SetParent(_stageWindowContent, false);
                var lineRect = lineObject.AddComponent<RectTransform>();
                SetAnchors(lineRect, new Vector2(0f, 0.16f), new Vector2(1f, 0.91f));
                _stageWindowLineGraph = lineObject.AddComponent<StageDpsLineGraphic>();
                _stageWindowLineGraph.Records = records;
                _stageWindowLineGraph.color = new Color(0.65f, 0.65f, 0.65f, 0.9f);
                _stageWindowLineGraph.LineThickness = 1.25f;
                _stageWindowLineGraph.raycastTarget = false;

                for (int i = 0; i < records.Count; i++)
                {
                    var stageLabel = CreateLabel(
                        _stageWindowContent,
                        "Stage_DPS_Window_Stage_Label",
                        records[i].StageNumber.ToString(),
                        11f);
                    stageLabel.alignment = TextAlignmentOptions.Center;
                    stageLabel.color = MutedColor;
                    float normalizedX = records.Count == 1 ? 0.5f : i / (float)(records.Count - 1);
                    stageLabel.rectTransform.anchorMin = new Vector2(normalizedX, 0.025f);
                    stageLabel.rectTransform.anchorMax = new Vector2(normalizedX, 0.13f);
                    stageLabel.rectTransform.sizeDelta = new Vector2(
                        Mathf.Min(72f, contentWidth / Mathf.Max(1, records.Count) * 0.9f),
                        20f);
                    SetTextAutoSize(stageLabel, 7f, 11f);
                    _stageWindowStageLabels.Add(stageLabel.gameObject);

                    if (records[i].CollectedPerks.Length > 0)
                    {
                        string[] perkLines = BuildPerkTableText(records[i], "\n").Split('\n');
                        float longestPerkLength = 0f;
                        for (int perkIndex = 0; perkIndex < records[i].CollectedPerks.Length; perkIndex++)
                        {
                            longestPerkLength = Mathf.Max(longestPerkLength, records[i].CollectedPerks[perkIndex].Length);
                        }
                        float labelWidth = Mathf.Clamp(longestPerkLength * 8.25f + 12f, 64f, 180f);
                        const float lineHeight = 22f;
                        float labelHeight = Mathf.Max(lineHeight, perkLines.Length * lineHeight);
                        var labelObject = new GameObject("Stage_DPS_Window_Perk_Label");
                        labelObject.transform.SetParent(_stageWindowPerkOverlay, false);
                        var labelRoot = labelObject.AddComponent<RectTransform>();
                        labelRoot.pivot = new Vector2(1f, 0f);
                        labelRoot.sizeDelta = new Vector2(labelWidth, labelHeight);

                        for (int perkIndex = 0; perkIndex < perkLines.Length; perkIndex++)
                        {
                            var perkLine = CreateLabel(labelRoot, "Stage_DPS_Window_Perk_Line", perkLines[perkIndex], 15f);
                            perkLine.alignment = TextAlignmentOptions.MidlineRight;
                            perkLine.textWrappingMode = TextWrappingModes.NoWrap;
                            perkLine.overflowMode = TextOverflowModes.Overflow;
                            perkLine.color = GraphUiPalette.PerkText;
                            perkLine.rectTransform.anchorMin = Vector2.one;
                            perkLine.rectTransform.anchorMax = Vector2.one;
                            perkLine.rectTransform.pivot = new Vector2(1f, 0.5f);
                            perkLine.rectTransform.sizeDelta = new Vector2(0f, lineHeight);
                            perkLine.rectTransform.anchoredPosition = new Vector2(0f, -perkIndex * lineHeight - lineHeight * 0.5f);
                            perkLine.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 315f);
                            SetTextAutoSize(perkLine, 10.5f, 15f);
                        }

                        _stageWindowPerkLabels.Add(new StageWindowPerkLabel
                        {
                            RecordIndex = i,
                            Root = labelRoot
                        });
                    }
                }
            }

            Canvas.ForceUpdateCanvases();
            _stageWindowScrollRect.horizontalNormalizedPosition = 1f;
            UpdateStageDpsWindowAxis(records);
            _stageWindowViewport.SetAsFirstSibling();
            _stageWindowPerkOverlay.SetAsLastSibling();
        }

        private void OnStageDpsWindowScrolled(Vector2 _)
        {
            UpdateStageDpsWindowAxis(Plugin.Instance.StageDpsRecords);
        }

        private void UpdateStageDpsWindowAxis(IList<StageDpsRecord> records)
        {
            if (_stageWindowAreaGraph == null || records == null || records.Count == 0)
            {
                return;
            }

            int visibleCount = Mathf.Min(20, records.Count);
            int firstVisible = records.Count <= visibleCount
                ? 0
                : Mathf.RoundToInt((records.Count - visibleCount) * _stageWindowScrollRect.horizontalNormalizedPosition);
            float maxDps = 0f;
            for (int i = firstVisible; i < firstVisible + visibleCount; i++)
            {
                maxDps = Mathf.Max(maxDps, records[i].Dps);
            }

            float axisMaximum = Mathf.Max(1f, maxDps * 1.25f);
            _stageWindowAreaGraph.AxisMaximum = axisMaximum;
            _stageWindowLineGraph.AxisMaximum = axisMaximum;
            _stageWindowAreaGraph.SetVerticesDirty();
            _stageWindowLineGraph.SetVerticesDirty();
            for (int tick = 0; tick < _stageWindowAxisLabels.Count; tick++)
            {
                _stageWindowAxisLabels[tick].text = (axisMaximum * tick / 4f).ToString("F0");
            }
            var placedPerkLabels = new List<RectTransform>();
            for (int i = 0; i < _stageWindowPerkLabels.Count; i++)
            {
                StageWindowPerkLabel perkLabel = _stageWindowPerkLabels[i];
                StageDpsRecord record = records[perkLabel.RecordIndex];
                float normalizedX = records.Count == 1
                    ? 0.5f
                    : perkLabel.RecordIndex / (float)(records.Count - 1);
                float plotY = 0.16f + 0.75f * Mathf.Clamp01(record.Dps / axisMaximum);
                float baseY = _stageWindowViewport.rect.height * plotY / _stageWindowPerkOverlay.rect.height;
                RectTransform labelRect = perkLabel.Root;
                labelRect.anchorMin = new Vector2(0f, baseY);
                labelRect.anchorMax = new Vector2(0f, baseY);

                bool hasClearPosition = false;
                float leftPadding = _stageWindowViewport.rect.width - _stageWindowPerkOverlay.rect.width;
                for (float verticalOffset = 5f; verticalOffset <= _stageWindowPerkOverlay.rect.height; verticalOffset += 12f)
                {
                    labelRect.anchoredPosition = new Vector2(
                        normalizedX * _stageWindowContent.rect.width + _stageWindowContent.anchoredPosition.x - leftPadding,
                        verticalOffset);
                    if (!IsStagePerkLabelOverlapping(labelRect, placedPerkLabels, 8f))
                    {
                        hasClearPosition = true;
                        break;
                    }
                }

                if (!hasClearPosition)
                {
                    labelRect.anchoredPosition = new Vector2(
                    normalizedX * _stageWindowContent.rect.width + _stageWindowContent.anchoredPosition.x - leftPadding,
                    _stageWindowPerkOverlay.rect.height);
                }
                placedPerkLabels.Add(labelRect);
            }
        }

        private static bool IsStagePerkLabelOverlapping(
            RectTransform candidate,
            IList<RectTransform> placedLabels,
            float spacing)
        {
            Vector2 candidateCenter = GetStagePerkLabelCenter(candidate);
            float candidateHalfWidth = candidate.rect.width * 0.5f + spacing;
            float candidateHalfHeight = candidate.rect.height * 0.5f + spacing;

            for (int i = 0; i < placedLabels.Count; i++)
            {
                RectTransform placed = placedLabels[i];
                Vector2 centerDelta = GetStagePerkLabelCenter(placed) - candidateCenter;
                Vector2 localDelta = Quaternion.Inverse(candidate.localRotation) * centerDelta;
                if (Mathf.Abs(localDelta.x) < candidateHalfWidth + placed.rect.width * 0.5f &&
                    Mathf.Abs(localDelta.y) < candidateHalfHeight + placed.rect.height * 0.5f)
                {
                    return true;
                }
            }

            return false;
        }

        private static Vector2 GetStagePerkLabelCenter(RectTransform label)
        {
            Vector2 pivotToCenter = new Vector2(
                (0.5f - label.pivot.x) * label.rect.width,
                (0.5f - label.pivot.y) * label.rect.height);
            return (Vector2)label.localPosition + (Vector2)(label.localRotation * pivotToCenter);
        }

        private void UpdateStageDpsSubtabs()
        {
            bool showAllStages = _selectedSection == 0 && _selectedStageSubtab == 0;
            bool showWindow = _selectedSection == 0 && _selectedStageSubtab == 1;
            _plotRoot.gameObject.SetActive(showAllStages);
            _xAxisTitle.gameObject.SetActive(showAllStages);
            _yAxisTitle.gameObject.SetActive(showAllStages);
            _stageWindowRoot.gameObject.SetActive(showWindow);
            _emptyMessage.gameObject.SetActive(showAllStages && Plugin.Instance.StageDpsRecords.Count == 0);

            for (int i = 0; i < _chartLabels.Count; i++)
            {
                _chartLabels[i].SetActive(showAllStages);
            }
            for (int i = 0; i < _legendObjects.Count; i++)
            {
                _legendObjects[i].SetActive(showAllStages);
            }
            for (int i = 0; i < _stageSubtabButtons.Count; i++)
            {
                _stageSubtabButtons[i].gameObject.SetActive(_selectedSection == 0);
                _stageSubtabButtons[i].image.color = i == _selectedStageSubtab
                    ? GraphUiPalette.SelectedButton
                    : GraphUiPalette.Button;
                _stageSubtabLabels[i].color = i == _selectedStageSubtab ? AccentColor : GraphUiPalette.Text;
            }
        }

        private void CreateCombatStatsPanel(Transform parent)
        {
            var panel = CreatePanel(parent, "Combat_Stats_Panel", GraphUiPalette.Plot);
            _combatStatsRoot = panel.gameObject;
            SetAnchors(panel, new Vector2(0.035f, 0.1f), new Vector2(0.98f, 0.84f));

            string[] subtabNames = { "Power", "Elements", "Criticals" };
            for (int i = 0; i < subtabNames.Length; i++)
            {
                int subtabIndex = i;
                var buttonImage = CreateImage(panel, "Combat_Stats_Subtab_Button", GraphUiPalette.Button);
                buttonImage.raycastTarget = true;
                buttonImage.rectTransform.anchorMin = new Vector2(0.025f + i * 0.19f, 0.895f);
                buttonImage.rectTransform.anchorMax = new Vector2(0.205f + i * 0.19f, 0.975f);
                buttonImage.rectTransform.offsetMin = Vector2.zero;
                buttonImage.rectTransform.offsetMax = Vector2.zero;
                var button = buttonImage.gameObject.AddComponent<UnityEngine.UI.Button>();
                button.targetGraphic = buttonImage;
                button.onClick.AddListener(() =>
                {
                    _selectedCombatSubtab = subtabIndex;
                    UpdateCombatSubtabs();
                });
                var label = CreateLabel(buttonImage.transform, "Combat_Stats_Subtab_Label", subtabNames[i], 15f);
                label.alignment = TextAlignmentOptions.Center;
                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = new Vector2(3f, 0f);
                label.rectTransform.offsetMax = new Vector2(-3f, 0f);
                SetTextAutoSize(label, 9f, 15f);
                _combatSubtabButtons.Add(button);
                _combatSubtabLabels.Add(label);
            }

            var powerView = CreateCombatGraphView(panel, "Power", new[] { 0, 1 }, new Vector2(0.025f, 0.04f), new Vector2(0.975f, 0.9f));
            _combatGraphViews.Add(powerView);

            var elementView = CreateCombatGraphView(panel, "Element Bonuses", new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 }, new Vector2(0.025f, 0.04f), new Vector2(0.975f, 0.9f));
            _combatGraphViews.Add(elementView);

            var criticalRoot = CreatePanel(panel, "Combat_Critical_Graphs", GraphUiPalette.Plot);
            SetAnchors(criticalRoot, new Vector2(0.025f, 0.04f), new Vector2(0.975f, 0.9f));
            var criticalRateView = CreateCombatGraphView(criticalRoot, "Critical Rate", new[] { 11 }, new Vector2(0f, 0.51f), new Vector2(1f, 1f));
            _combatGraphViews.Add(criticalRateView);
            var criticalDamageView = CreateCombatGraphView(criticalRoot, "Critical Damage", new[] { 12 }, new Vector2(0f, 0f), new Vector2(1f, 0.49f));
            _combatGraphViews.Add(criticalDamageView);
            _combatSubtabViews.Add(powerView.Root);
            _combatSubtabViews.Add(elementView.Root);
            _combatSubtabViews.Add(criticalRoot.gameObject);

            UpdateCombatSubtabs();
        }

        private readonly List<GameObject> _combatSubtabViews = new List<GameObject>();

        private CombatGraphView CreateCombatGraphView(
            Transform parent,
            string titleText,
            int[] statIndexes,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var rootRect = CreatePanel(parent, "Combat_Chart_View", GraphUiPalette.Button);
            rootRect.GetComponent<UnityEngine.UI.Image>().color = GraphUiPalette.Button;
            SetAnchors(rootRect, anchorMin, anchorMax);

            var view = new CombatGraphView
            {
                Root = rootRect.gameObject,
                StatIndexes = statIndexes
            };

            var title = CreateLabel(rootRect, "Combat_Chart_Title", titleText, 17f);
            title.alignment = TextAlignmentOptions.Center;
            title.rectTransform.anchorMin = new Vector2(0.045f, 0.55f);
            title.rectTransform.anchorMax = title.rectTransform.anchorMin;
            title.rectTransform.sizeDelta = new Vector2(170f, 28f);
            title.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            SetTextAutoSize(title, 11f, 17f);

            int legendColumns = statIndexes.Length > 4 ? 4 : statIndexes.Length;
            float legendStart = 0.23f;
            float legendWidth = 0.14f;
            float legendRowHeight = 0.045f;
            for (int i = 0; i < statIndexes.Length; i++)
            {
                var legend = CreateLabel(rootRect, "Combat_Chart_Legend", CombatStatNames[statIndexes[i]], 11f);
                legend.alignment = TextAlignmentOptions.MidlineLeft;
                legend.color = CombatStatColors[statIndexes[i]];
                int column = i % legendColumns;
                int row = i / legendColumns;
                float rowBottom = 0.015f + row * legendRowHeight;
                legend.rectTransform.anchorMin = new Vector2(legendStart + column * legendWidth, rowBottom);
                legend.rectTransform.anchorMax = new Vector2(legendStart + (column + 1) * legendWidth, rowBottom + legendRowHeight);
                SetTextAutoSize(legend, 7f, 11f);
                view.LegendLabels.Add(legend);
            }

            var maximumLegend = CreateLabel(rootRect, "Combat_Chart_Max_Legend", "MAX", 10f);
            maximumLegend.alignment = TextAlignmentOptions.Center;
            maximumLegend.color = GraphUiPalette.Text;
            maximumLegend.rectTransform.anchorMin = new Vector2(0.8f, 0.015f);
            maximumLegend.rectTransform.anchorMax = new Vector2(0.88f, 0.06f);
            SetTextAutoSize(maximumLegend, 7f, 10f);

            var endLegend = CreateLabel(rootRect, "Combat_Chart_End_Legend", "END", 10f);
            endLegend.alignment = TextAlignmentOptions.Center;
            endLegend.color = GraphUiPalette.MutedText;
            endLegend.rectTransform.anchorMin = new Vector2(0.89f, 0.015f);
            endLegend.rectTransform.anchorMax = new Vector2(0.97f, 0.06f);
            SetTextAutoSize(endLegend, 7f, 10f);

            var graphObject = new GameObject("Combat_Combined_Line_Graph");
            graphObject.transform.SetParent(rootRect, false);
            var graphRect = graphObject.AddComponent<RectTransform>();
            float graphTop = 0.84f;
            SetAnchors(graphRect, new Vector2(0.1f, 0.28f), new Vector2(0.87f, graphTop));
            var graph = graphObject.AddComponent<CombatStatsLineGraphic>();
            graph.StatIndexes = statIndexes;
            var colors = new Color[statIndexes.Length];
            for (int i = 0; i < statIndexes.Length; i++)
            {
                colors[i] = CombatStatColors[statIndexes[i]];
            }
            graph.SeriesColors = colors;
            graph.raycastTarget = false;
            view.Graph = graph;

            var maximumLabel = CreateLabel(rootRect, "Combat_Chart_Axis_Maximum", string.Empty, 10f);
            maximumLabel.alignment = TextAlignmentOptions.MidlineLeft;
            maximumLabel.color = GraphUiPalette.MutedText;
            maximumLabel.rectTransform.anchorMin = new Vector2(0.88f, graphTop - 0.06f);
            maximumLabel.rectTransform.anchorMax = new Vector2(0.99f, graphTop + 0.02f);
            SetTextAutoSize(maximumLabel, 7f, 10f);
            view.MaximumLabel = maximumLabel;

            var minimumLabel = CreateLabel(rootRect, "Combat_Chart_Axis_Minimum", string.Empty, 10f);
            minimumLabel.alignment = TextAlignmentOptions.MidlineLeft;
            minimumLabel.color = GraphUiPalette.MutedText;
            minimumLabel.rectTransform.anchorMin = new Vector2(0.88f, 0.25f);
            minimumLabel.rectTransform.anchorMax = new Vector2(0.99f, 0.33f);
            SetTextAutoSize(minimumLabel, 7f, 10f);
            view.MinimumLabel = minimumLabel;

            var stageRootObject = new GameObject("Combat_Chart_Stage_Labels");
            stageRootObject.transform.SetParent(rootRect, false);
            view.StageLabels = stageRootObject.AddComponent<RectTransform>();
            SetAnchors(view.StageLabels, new Vector2(0.1f, 0.17f), new Vector2(0.87f, 0.26f));

            return view;
        }

        public void RefreshCombatStatsGraph(IList<CombatStatsRecord> records)
        {
            if (_combatStatsRoot == null)
            {
                return;
            }

            _combatStatsRoot.SetActive(_selectedSection == 1);
            if (_selectedSection != 1)
            {
                return;
            }

            bool hasRecords = records.Count > 0;
            _emptyMessage.gameObject.SetActive(!hasRecords);
            _emptyMessage.text = hasRecords ? string.Empty : "Complete a stage to see combat stats by stage.";
            if (!hasRecords)
            {
                _emptyMessage.transform.SetAsLastSibling();
            }

            for (int viewIndex = 0; viewIndex < _combatGraphViews.Count; viewIndex++)
            {
                var view = _combatGraphViews[viewIndex];
                var graph = view.Graph;
                graph.Records = records;
                float maximum = 0f;
                for (int recordIndex = 0; recordIndex < records.Count; recordIndex++)
                {
                    for (int metric = 0; metric < view.StatIndexes.Length; metric++)
                    {
                        int statIndex = view.StatIndexes[metric];
                        maximum = Mathf.Max(maximum, Mathf.Max(records[recordIndex].Values[statIndex], records[recordIndex].MaximumValues[statIndex]));
                    }
                }
                float padding = Mathf.Max(0.5f, maximum * 0.1f);
                graph.AxisMinimum = 0f;
                graph.AxisMaximum = hasRecords ? maximum + padding : 1f;
                graph.SetVerticesDirty();

                string unit = view.StatIndexes[0] < 2 ? string.Empty : "%";
                view.MinimumLabel.text = hasRecords ? graph.AxisMinimum.ToString("F0") + unit : string.Empty;
                view.MaximumLabel.text = hasRecords ? graph.AxisMaximum.ToString("F0") + unit : string.Empty;
                CreateStageLabels(view.StageLabels, records);
            }
        }

        public void RefreshPerkTable(IList<StageDpsRecord> records)
        {
            if (_perkTableRoot == null || _selectedSection != 2)
            {
                return;
            }

            ClearChildren(_perkTableContent);
            StageDpsRecord preStageRecord = Plugin.Instance.PreStagePerksRecord;
            bool hasPreStagePerks = preStageRecord != null && preStageRecord.CollectedPerks.Length > 0;
            bool hasStagePerks = false;
            if (records != null)
            {
                for (int i = 0; i < records.Count; i++)
                {
                    if (records[i].CollectedPerks.Length > 0)
                    {
                        hasStagePerks = true;
                        break;
                    }
                }
            }

            bool hasPerks = hasPreStagePerks || hasStagePerks;
            _perkTableEmptyMessage.gameObject.SetActive(!hasPerks);
            if (!hasPerks)
            {
                _perkTableEmptyMessage.text = "Complete a stage to see perk pickups.";
                return;
            }

            int rowIndex = 0;
            if (hasPreStagePerks)
            {
                CreatePerkTableRow(preStageRecord, rowIndex++);
            }

            for (int i = 0; records != null && i < records.Count; i++)
            {
                StageDpsRecord record = records[i];
                if (record.CollectedPerks.Length == 0)
                {
                    continue;
                }
                CreatePerkTableRow(record, rowIndex++);
            }
        }

        private void CreatePerkTableRow(StageDpsRecord record, int rowIndex)
        {
            var rowImage = CreateImage(
                _perkTableContent,
                "Perk_Stage_Row",
                rowIndex % 2 == 0 ? GraphUiPalette.Button : GraphUiPalette.Panel);
            var rowRect = rowImage.rectTransform;
            string perksText = BuildPerkTableText(record);
            var layoutElement = rowImage.gameObject.AddComponent<LayoutElement>();

            var stageLabel = CreateLabel(rowImage.transform, "Perk_Stage_Number", "Stage " + record.StageNumber, 14f);
            SetPerkTableCell(stageLabel, 0f, 0.15f, TextAlignmentOptions.MidlineLeft);

            var perksLabel = CreateLabel(rowImage.transform, "Perk_Stage_Perks", perksText, 14f);
            perksLabel.textWrappingMode = TextWrappingModes.Normal;
            perksLabel.overflowMode = TextOverflowModes.Masking;
            SetPerkTableCell(perksLabel, 0.15f, 1f, TextAlignmentOptions.MidlineLeft);

            float rowWidth = _perkTableContent.rect.width;
            if (rowWidth <= 0f)
            {
                rowWidth = _perkTableRoot.rect.width * 0.96f;
            }
            float perksWidth = Mathf.Max(1f, rowWidth * 0.85f - 24f);
            float rowHeight = Mathf.Max(56f, perksLabel.GetPreferredValues(perksText, perksWidth, 0f).y + 16f);
            rowRect.sizeDelta = new Vector2(0f, rowHeight);
            layoutElement.minHeight = rowHeight;
            layoutElement.preferredHeight = rowHeight;
        }

        private static string BuildPerkTableText(StageDpsRecord record, string separator = ", ")
        {
            if (record.CollectedPerks.Length == 0)
            {
                return "No perks collected";
            }

            var formattedPerks = new string[record.CollectedPerks.Length];
            for (int i = 0; i < record.CollectedPerks.Length; i++)
            {
                int upgradeLevel = Mathf.Clamp(record.PerkUpgradeLevels[i], 0, 2);
                Color perkColor = record.PerkTalents[i]
                    ? GraphUiPalette.PerkTalent
                    : upgradeLevel == 1
                        ? GraphUiPalette.PerkUpgradeOne
                        : upgradeLevel >= 2
                            ? GraphUiPalette.PerkUpgradeTwo
                            : GraphUiPalette.PerkText;
                string upgradeMarker = upgradeLevel == 0
                    ? string.Empty
                    : upgradeLevel == 1 ? "+" : "++";
                string talentVersion = record.MasterTalents[i] ? " (Master)" : string.Empty;
                string colorHex = ColorUtility.ToHtmlStringRGB(perkColor);
                string perkName = (record.CollectedPerks[i] ?? string.Empty)
                    .Trim()
                    .Trim('[', ']')
                    .TrimEnd('.')
                    .Trim();
                if (perkName.StartsWith("Perkname.", StringComparison.OrdinalIgnoreCase))
                {
                    perkName = perkName.Substring("Perkname.".Length);
                }
                perkName = perkName.Replace("_", " ").Trim();
                formattedPerks[i] = "<color=#" + colorHex + ">" +
                    perkName + talentVersion + upgradeMarker + "</color>";
            }

            return string.Join(separator, formattedPerks);
        }

        private void CreatePerkTable(Transform parent)
        {
            _perkTableRoot = CreatePanel(parent, "Stage_Perk_Table", GraphUiPalette.Plot);
            SetAnchors(_perkTableRoot, new Vector2(0.035f, 0.055f), new Vector2(0.98f, 0.82f));
            _perkTableRoot.gameObject.SetActive(false);

            var header = CreateImage(_perkTableRoot, "Perk_Table_Header", GraphUiPalette.Header);
            header.rectTransform.anchorMin = new Vector2(0f, 1f);
            header.rectTransform.anchorMax = Vector2.one;
            header.rectTransform.pivot = new Vector2(0.5f, 1f);
            header.rectTransform.sizeDelta = new Vector2(0f, 44f);

            var stageHeader = CreateLabel(header.transform, "Perk_Table_Stage_Header", "Stage", 14f);
            SetPerkTableCell(stageHeader, 0f, 0.15f, TextAlignmentOptions.Center);
            stageHeader.color = GraphUiPalette.Gold;
            var perksHeader = CreateLabel(header.transform, "Perk_Table_Perks_Header", "Collected perks", 14f);
            SetPerkTableCell(perksHeader, 0.15f, 1f, TextAlignmentOptions.Center);
            perksHeader.color = GraphUiPalette.Gold;

            var viewportObject = new GameObject("Perk_Table_Viewport");
            viewportObject.transform.SetParent(_perkTableRoot, false);
            var viewportImage = viewportObject.AddComponent<UnityEngine.UI.Image>();
            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = true;
            var viewport = viewportImage.rectTransform;
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(12f, 8f);
            viewport.offsetMax = new Vector2(-18f, -50f);
            viewportObject.AddComponent<RectMask2D>();

            var contentObject = new GameObject("Perk_Table_Content");
            contentObject.transform.SetParent(viewport, false);
            _perkTableContent = contentObject.AddComponent<RectTransform>();
            _perkTableContent.anchorMin = new Vector2(0f, 1f);
            _perkTableContent.anchorMax = new Vector2(1f, 1f);
            _perkTableContent.pivot = new Vector2(0.5f, 1f);
            var layout = contentObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var contentSize = contentObject.AddComponent<ContentSizeFitter>();
            contentSize.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSize.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = _perkTableRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.viewport = viewport;
            scrollRect.content = _perkTableContent;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var scrollbarImage = CreateImage(_perkTableRoot, "Perk_Table_Scrollbar", GraphUiPalette.Header);
            scrollbarImage.raycastTarget = true;
            var scrollbarRect = scrollbarImage.rectTransform;
            scrollbarRect.anchorMin = new Vector2(1f, 0.04f);
            scrollbarRect.anchorMax = new Vector2(1f, 0.94f);
            scrollbarRect.sizeDelta = new Vector2(8f, 0f);
            scrollbarRect.anchoredPosition = new Vector2(-6f, 0f);
            var scrollbar = scrollbarImage.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            var handleImage = CreateImage(scrollbarImage.transform, "Perk_Table_Scrollbar_Handle", GraphUiPalette.Gold);
            handleImage.raycastTarget = true;
            SetAnchors(handleImage.rectTransform, Vector2.zero, Vector2.one);
            scrollbar.handleRect = handleImage.rectTransform;
            scrollbar.targetGraphic = handleImage;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollRect.verticalScrollbarSpacing = -2f;

            _perkTableEmptyMessage = CreateLabel(_perkTableRoot, "Perk_Table_Empty_Message", string.Empty, 20f);
            _perkTableEmptyMessage.alignment = TextAlignmentOptions.Center;
            _perkTableEmptyMessage.rectTransform.anchorMin = new Vector2(0.12f, 0.35f);
            _perkTableEmptyMessage.rectTransform.anchorMax = new Vector2(0.88f, 0.62f);
            SetTextAutoSize(_perkTableEmptyMessage, 12f, 20f);
        }

        private static void SetPerkTableCell(
            TextMeshProUGUI label,
            float anchorMinX,
            float anchorMaxX,
            TextAlignmentOptions alignment)
        {
            label.alignment = alignment;
            label.rectTransform.anchorMin = new Vector2(anchorMinX, 0f);
            label.rectTransform.anchorMax = new Vector2(anchorMaxX, 1f);
            label.rectTransform.offsetMin = new Vector2(12f, 4f);
            label.rectTransform.offsetMax = new Vector2(-12f, -4f);
            SetTextAutoSize(label, 10f, 14f);
        }

        private static void CreateStageLabels(RectTransform parent, IList<CombatStatsRecord> records)
        {
            ClearChildren(parent);
            for (int i = 0; i < records.Count; i++)
            {
                var label = CreateLabel(parent, "Combat_Stage_Label", records[i].StageNumber.ToString(), 10f);
                label.alignment = TextAlignmentOptions.Center;
                label.color = GraphUiPalette.MutedText;
                float x = records.Count == 1 ? 0.5f : i / (float)(records.Count - 1);
                float width = Mathf.Min(1f, 0.9f / records.Count);
                float left = i == 0 ? x : i == records.Count - 1 ? x - width : x - width * 0.5f;
                float right = i == 0 ? x + width : i == records.Count - 1 ? x : x + width * 0.5f;
                label.rectTransform.anchorMin = new Vector2(left, 0f);
                label.rectTransform.anchorMax = new Vector2(right, 1f);
                label.alignment = i == 0
                    ? TextAlignmentOptions.MidlineLeft
                    : i == records.Count - 1
                        ? TextAlignmentOptions.MidlineRight
                        : TextAlignmentOptions.Center;
                SetTextAutoSize(label, 7f, 10f);
            }
        }

        private void UpdateCombatSubtabs()
            {
                for (int i = 0; i < _combatSubtabButtons.Count; i++)
                {
                    _combatSubtabButtons[i].image.color = i == _selectedCombatSubtab
                        ? GraphUiPalette.SelectedButton
                        : GraphUiPalette.Button;
                    _combatSubtabLabels[i].color = i == _selectedCombatSubtab
                        ? AccentColor
                        : GraphUiPalette.Text;
                }
                for (int i = 0; i < _combatSubtabViews.Count; i++)
                {
                    _combatSubtabViews[i].SetActive(i == _selectedCombatSubtab);
                }
                if (_combatStatsRoot != null && _selectedSection == 1)
                {
                    _emptyMessage.gameObject.SetActive(Plugin.Instance.CombatStatsRecords.Count == 0);
                }
        }

        private void CreateLegend()
        {
            const int columnCount = 3;
            const float firstX = 0.15f;
            const float columnWidth = 0.27f;
            const float firstY = 0.17f;
            const float rowHeight = 0.035f;

            for (int i = 0; i <= StageDamageTypes.Names.Length; i++)
            {
                int row = i / columnCount;
                int column = i % columnCount;
                Color swatchColor = i == 0
                    ? GraphUiPalette.TotalArea
                    : StageDamageTypes.Colors[i - 1];
                string labelText = i == 0 ? "Total damage" : StageDamageTypes.Names[i - 1];
                var swatch = CreateImage(_contentRoot, "Damage_Type_Legend_Swatch", swatchColor);
                swatch.rectTransform.anchorMin = new Vector2(firstX + column * columnWidth, firstY - row * rowHeight);
                swatch.rectTransform.anchorMax = new Vector2(firstX + column * columnWidth, firstY - row * rowHeight);
                swatch.rectTransform.sizeDelta = new Vector2(14f, 14f);
                swatch.rectTransform.anchoredPosition = new Vector2(7f, -7f);
                _legendObjects.Add(swatch.gameObject);

                var label = CreateLabel(_contentRoot, "Damage_Type_Legend_Label", labelText, 15f);
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.color = MutedColor;
                SetTextAutoSize(label, 10f, 14f);
                label.rectTransform.anchorMin = new Vector2(firstX + column * columnWidth, firstY - row * rowHeight);
                label.rectTransform.anchorMax = new Vector2(firstX + column * columnWidth + columnWidth - 0.02f, firstY - row * rowHeight);
                label.rectTransform.sizeDelta = new Vector2(0f, 20f);
                label.rectTransform.anchoredPosition = new Vector2(26f, -7f);
                _legendObjects.Add(label.gameObject);
            }
        }

        private void CreateSectionButton(Transform parent, int sectionIndex, string text)
        {
            var buttonImage = CreateImage(parent, "Dashboard_Section_Button", GraphUiPalette.Button);
            buttonImage.raycastTarget = true;

            var buttonRect = buttonImage.rectTransform;
            buttonRect.anchorMin = new Vector2(0.06f, 1f);
            buttonRect.anchorMax = new Vector2(0.94f, 1f);
            buttonRect.pivot = new Vector2(0.5f, 1f);
            buttonRect.anchoredPosition = new Vector2(0f, -20f - sectionIndex * 64f);
            buttonRect.sizeDelta = new Vector2(0f, 52f);

            var button = buttonImage.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(() =>
            {
                _selectedSection = sectionIndex;
                UpdateSelectedSection();
                if (sectionIndex == 0)
                {
                    RefreshStageGraph(Plugin.Instance.StageDpsRecords);
                }
                else if (sectionIndex == 1)
                {
                    RefreshCombatStatsGraph(Plugin.Instance.CombatStatsRecords);
                }
                else if (sectionIndex == 2)
                {
                    RefreshPerkTable(Plugin.Instance.StageDpsRecords);
                }
            });

            var label = CreateLabel(buttonImage.transform, "Dashboard_Section_Label", text, 18f);
            label.alignment = TextAlignmentOptions.Center;
            SetTextAutoSize(label, 11f, 18f);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(4f, 0f);
            label.rectTransform.offsetMax = new Vector2(-4f, 0f);

            _sectionButtons.Add(button);
            _sectionLabels.Add(label);
        }

        private void UpdateSelectedSection()
        {
            string[] sectionNames = { "Stage DPS", "Combat Stats", "Perks by stage" };
            _sectionTitle.text = sectionNames[_selectedSection];
            for (int i = 0; i < _sectionButtons.Count; i++)
            {
                _sectionButtons[i].image.color = i == _selectedSection ? GraphUiPalette.SelectedButton : GraphUiPalette.Button;
                _sectionLabels[i].color = i == _selectedSection ? AccentColor : GraphUiPalette.Text;
            }

            bool showStageGraph = _selectedSection == 0;
            bool showCombatStats = _selectedSection == 1;
            bool showPerkTable = _selectedSection == 2;
            _plotRoot.gameObject.SetActive(showStageGraph);
            _xAxisTitle.gameObject.SetActive(showStageGraph);
            _yAxisTitle.gameObject.SetActive(showStageGraph);
            _combatStatsRoot.SetActive(showCombatStats);
            _perkTableRoot.gameObject.SetActive(showPerkTable);
            _emptyMessage.gameObject.SetActive(showStageGraph || showCombatStats);
            for (int i = 0; i < _chartLabels.Count; i++)
            {
                _chartLabels[i].SetActive(showStageGraph);
            }
            for (int i = 0; i < _legendObjects.Count; i++)
            {
                _legendObjects[i].SetActive(showStageGraph);
            }
            if (showPerkTable)
            {
                RefreshPerkTable(Plugin.Instance.StageDpsRecords);
            }
            else if (!showStageGraph && !showCombatStats)
            {
                _emptyMessage.text = "This graph section is ready for a future graph.";
            }
            else if (showCombatStats)
            {
                RefreshCombatStatsGraph(Plugin.Instance.CombatStatsRecords);
            }
            UpdateStageDpsSubtabs();
        }

        private static RectTransform CreatePanel(Transform parent, string objectName, Color color)
        {
            return CreateImage(parent, objectName, color).rectTransform;
        }

        private static UnityEngine.UI.Image CreateImage(Transform parent, string objectName, Color color)
        {
            var imageObject = new GameObject(objectName);
            imageObject.transform.SetParent(parent, false);
            var image = imageObject.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void SetAnchors(RectTransform rectTransform, Vector2 min, Vector2 max)
        {
            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(parent.GetChild(i).gameObject);
            }
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string objectName, string text, float fontSize)
        {
            var labelObject = new GameObject(objectName);
            labelObject.transform.SetParent(parent, false);

            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = GraphUiPalette.Text;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            return label;
        }

        private static void SetTextAutoSize(TextMeshProUGUI label, float minFontSize, float maxFontSize)
        {
            label.enableAutoSizing = true;
            label.fontSizeMin = minFontSize;
            label.fontSizeMax = maxFontSize;
        }
    }

}
