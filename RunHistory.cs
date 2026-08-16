using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RR.Game;
using RR.Game.Perk;
using RR.Game.Stats;
using UnityEngine;


namespace BlackveilDpsMeter
{
    public class RunHistoryManager : MonoBehaviour
    {
        public static RunHistoryManager Instance { get; private set; }

        public static bool _showUI = false;
        private Rect _windowRect = new Rect(20, 20, 350, 400);

        // Store history: List of Runs
        public static List<RunData> RunHistory = new List<RunData>();
        private bool[] _foldoutStates = new bool[3];

        /// <summary>
        /// Call this during your plugin's initial setup.
        /// </summary>
        public static void Init(Harmony harmony)
        {
            // Apply Harmony patches for Run History
            harmony.PatchAll(typeof(StatsManager_Patch));

            // Create a persistent GameObject to handle Update and OnGUI rendering
            var go = new GameObject("RunHistoryManager");
            UnityEngine.Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<RunHistoryManager>();
            Debug.Log("RunHistoryManager initialized and Harmony patches applied.");
        }

        private Vector2 _scrollPosition = Vector2.zero;

        private Texture2D _blackTexture;

        private Texture2D GetBlackTexture()
        {
            if (_blackTexture == null)
            {
                _blackTexture = new Texture2D(1, 1);
                _blackTexture.SetPixel(0, 0, Color.black);
                _blackTexture.Apply();
            }
            return _blackTexture;
        }

        private void OnGUI()
        {
            if (!_showUI) return;

            // Use getter to guarantee texture is initialized
            try
            {
                GUI.depth = -10000;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Draw background
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GetBlackTexture());

                // Main Area
                GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, Screen.height - 40));
                
                GUILayout.Label("Run History (Last 3 Runs)");
                Debug.Log("[RunHistoryManager] Displaying Run History UI.");
                
                if (RunHistory.Count == 0)
                {
                    GUILayout.Label("No runs recorded yet in this session.");
                    Debug.Log("[RunHistoryManager] No runs recorded yet in this session.");
                }
                else
                {
                    _scrollPosition = GUILayout.BeginScrollView(
                        _scrollPosition,
                        GUILayout.Width(Screen.width - 40),
                        GUILayout.Height(Screen.height - 100)
                    );

                    var recentRuns = RunHistory.TakeLast(3).Reverse().ToList();

                    for (int i = 0; i < recentRuns.Count; i++)
                    {
                        var run = recentRuns[i];
                        string dateStr = run.Timestamp.ToString("HH:mm:ss");
                        int runNumber = RunHistory.Count - i;

                        string arrow = _foldoutStates[i] ? "▼" : "►";

                        if (GUILayout.Button($"{arrow} Run #{runNumber} ({dateStr}) - {run.Perks.Count} Perks", GUILayout.Height(35)))
                        {
                            _foldoutStates[i] = !_foldoutStates[i];
                        }

                        if (_foldoutStates[i])
                        {
                            GUILayout.BeginVertical(GUI.skin.box);
                            if (run.Perks.Count == 0)
                            {
                                GUILayout.Label("  (No perks acquired)");
                            }
                            else
                            {
                                foreach (var perk in run.Perks)
                                {
                                    GUILayout.Label($"  • {perk}");
                                }
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.Space(10);
                    }

                    GUILayout.EndScrollView();
                }

                GUILayout.EndArea();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RunHistoryManager] Error inside OnGUI: {ex}");
            }
}

        public class RunData
        {
            public DateTime Timestamp { get; set; }
            public List<string> Perks { get; set; } = new List<string>();
        }

        // --- HARMONY PATCH ---
        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.TriggerGlobalEvent))]
        public static class StatsManager_Patch
        {
            [HarmonyPrefix]
            public static void Prefix(CharacterEvent gameEvent, TriggerParams triggerParam)
            {
                if (gameEvent == CharacterEvent.OnAllPlayerDied)
                {
                    SaveRunHistory();
                }
            }

            private static void SaveRunHistory()
            {
                var currentRun = new RunData { Timestamp = DateTime.Now };

                PerkHandler perkHandler = UnityEngine.Object.FindObjectOfType<PerkHandler>();

                if (perkHandler != null)
                {
                    // Access private _collectedPerks list via Traverse
                    var perksList = Traverse.Create(perkHandler)
                        .Field("_collectedPerks")
                        .GetValue<IList>();

                    if (perksList != null)
                    {
                        foreach (var perkDescriptor in perksList)
                        {
                            string perkName = Traverse.Create(perkDescriptor).Field("name").GetValue<string>()
                                           ?? perkDescriptor?.ToString()
                                           ?? "Unknown Perk";

                            currentRun.Perks.Add(perkName);
                        }
                    }
                }

                RunHistory.Add(currentRun);
            }
        }
    }
}