using System.IO;
using System.Text;
using MirraCloud.Core.Debugging;
using UnityEditor;
using UnityEngine;

namespace MirraCloud.Editor
{
    public sealed class RestApiInspectorWindow : EditorWindow
    {
        private const string EnabledPrefKey = "MirraCloud.RestApiInspector.Enabled";

        private Vector2 _listScroll;
        private Vector2 _detailsScroll;
        private int _selectedIndex = -1;
        private string _filter;

        [MenuItem("MirraCloud/Request Inspector")]
        public static void Open()
        {
            GetWindow<RestApiInspectorWindow>("MirraCloud Requests");
        }

        private void OnEnable()
        {
            RestApiTraceBus.IsEnabled = EditorPrefs.GetBool(EnabledPrefKey, true);
            RestApiTraceBus.OnChanged += Repaint;
        }

        private void OnDisable()
        {
            RestApiTraceBus.OnChanged -= Repaint;
        }

        private void OnGUI()
        {
            DrawToolbar();

            var entries = RestApiTraceBus.Entries;
            if (_selectedIndex >= entries.Count)
            {
                _selectedIndex = entries.Count - 1;
            }

            EditorGUILayout.BeginHorizontal();
            DrawList(entries);
            DrawDetails(entries);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var enabled = GUILayout.Toggle(RestApiTraceBus.IsEnabled, "Capture", EditorStyles.toolbarButton, GUILayout.Width(70));
                if (enabled != RestApiTraceBus.IsEnabled)
                {
                    RestApiTraceBus.IsEnabled = enabled;
                    EditorPrefs.SetBool(EnabledPrefKey, enabled);
                }

                GUILayout.Space(8);
                _filter = GUILayout.TextField(_filter ?? string.Empty, EditorStyles.toolbarSearchField, GUILayout.MinWidth(200));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    RestApiTraceBus.Clear();
                    _selectedIndex = -1;
                }

                if (GUILayout.Button("Export", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    Export();
                }
            }
        }

        private void DrawList(System.Collections.Generic.IReadOnlyList<RestApiTraceEntry> entries)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(position.width * 0.45f)))
            {
                _listScroll = EditorGUILayout.BeginScrollView(_listScroll);

                for (var i = 0; i < entries.Count; i++)
                {
                    var e = entries[i];
                    if (IsVisibleByFilter(e) == false)
                    {
                        continue;
                    }

                    var label = $"#{e.Id} {(e.IsSuccess ? "OK" : "ERR")} {e.Method} {e.Route} ({e.HttpStatusCode?.ToString() ?? "-"}) {e.DurationMs}ms r={e.RetryCount}";
                    var isSelected = i == _selectedIndex;

                    var prevColor = GUI.color;
                    GUI.color = e.IsSuccess ? new Color(0.85f, 1f, 0.85f) : new Color(1f, 0.85f, 0.85f);
                    if (GUILayout.Toggle(isSelected, label, "Button"))
                    {
                        _selectedIndex = i;
                    }
                    GUI.color = prevColor;
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawDetails(System.Collections.Generic.IReadOnlyList<RestApiTraceEntry> entries)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                if (_selectedIndex < 0 || _selectedIndex >= entries.Count)
                {
                    EditorGUILayout.HelpBox("Select a request from the list to see details.", MessageType.Info);
                    return;
                }

                var e = entries[_selectedIndex];

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Time (UTC)", e.TimestampUtc.ToString("O"));
                    EditorGUILayout.LabelField("Method", e.Method);
                    EditorGUILayout.LabelField("Route", e.Route);
                    EditorGUILayout.LabelField("Url", e.Url);
                    EditorGUILayout.LabelField("Status", e.HttpStatusCode?.ToString() ?? "-");
                    EditorGUILayout.LabelField("Duration", $"{e.DurationMs} ms");
                    EditorGUILayout.LabelField("Retries", e.RetryCount.ToString());

                    if (e.Error != null)
                    {
                        EditorGUILayout.LabelField("Error", $"{e.Error.Type}: {e.Error.Message}");
                    }
                }

                _detailsScroll = EditorGUILayout.BeginScrollView(_detailsScroll);
                EditorGUILayout.LabelField("Request Body", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(e.RequestBody ?? string.Empty, GUILayout.MinHeight(80));
                GUILayout.Space(8);
                EditorGUILayout.LabelField("Response Body", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(e.ResponseBody ?? string.Empty, GUILayout.MinHeight(120));
                EditorGUILayout.EndScrollView();

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Copy Request"))
                    {
                        EditorGUIUtility.systemCopyBuffer = e.RequestBody ?? string.Empty;
                    }

                    if (GUILayout.Button("Copy Response"))
                    {
                        EditorGUIUtility.systemCopyBuffer = e.ResponseBody ?? string.Empty;
                    }
                }
            }
        }

        private bool IsVisibleByFilter(RestApiTraceEntry entry)
        {
            if (string.IsNullOrWhiteSpace(_filter))
            {
                return true;
            }

            var f = _filter.Trim();
            return (entry.Route != null && entry.Route.IndexOf(f, System.StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (entry.Url != null && entry.Url.IndexOf(f, System.StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (entry.Method != null && entry.Method.IndexOf(f, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void Export()
        {
            var path = EditorUtility.SaveFilePanel(
                "Export MirraCloud requests",
                "",
                "mirracloud_requests.txt",
                "txt");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (var e in RestApiTraceBus.Entries)
            {
                if (IsVisibleByFilter(e) == false)
                {
                    continue;
                }

                sb.AppendLine($"#{e.Id} [{e.TimestampUtc:O}] {(e.IsSuccess ? "OK" : "ERR")} {e.Method} {e.Url}");
                sb.AppendLine($"Status: {e.HttpStatusCode?.ToString() ?? "-"}, Duration: {e.DurationMs}ms, Retries: {e.RetryCount}");
                if (e.Error != null)
                {
                    sb.AppendLine($"Error: {e.Error.Type} {e.Error.Message}");
                }
                sb.AppendLine("RequestBody:");
                sb.AppendLine(e.RequestBody ?? string.Empty);
                sb.AppendLine("ResponseBody:");
                sb.AppendLine(e.ResponseBody ?? string.Empty);
                sb.AppendLine(new string('-', 80));
            }

            File.WriteAllText(path, sb.ToString());
            AssetDatabase.Refresh();
        }
    }
}
