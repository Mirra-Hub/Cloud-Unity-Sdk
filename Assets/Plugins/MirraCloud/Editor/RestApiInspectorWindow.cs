using System.IO;
using System.Text;
using MirraCloud.Core.Debugging;
using MirraCloud.Json;
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
        private static readonly JsonService JsonService = new JsonService();

        private static GUIStyle _listItemLeftStyle;
        private static GUIStyle _listItemRightStyle;

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

                    EnsureListStyles();

                    var leftText = $"#{e.Id} {(e.IsSuccess ? "OK" : "ERR")} {e.Method} {e.Route}";
                    var rightText = $"{(e.HttpStatusCode?.ToString() ?? "-")} {e.DurationMs}ms r={e.RetryCount}";

                    var rect = GUILayoutUtility.GetRect(0f, EditorGUIUtility.singleLineHeight + 8, GUILayout.ExpandWidth(true));
                    var isSelected = i == _selectedIndex;

                    var baseColor = e.IsSuccess ? new Color(0.82f, 0.95f, 0.82f) : new Color(0.97f, 0.82f, 0.82f);
                    var bg = isSelected ? new Color(0.35f, 0.55f, 0.9f, 0.35f) : baseColor;
                    EditorGUI.DrawRect(rect, bg);

                    var content = new GUIContent(leftText, e.Url ?? e.Route ?? string.Empty);

                    var leftRect = new Rect(rect.x, rect.y, rect.width - 120, rect.height);
                    var rightRect = new Rect(rect.x + rect.width - 120, rect.y, 120, rect.height);

                    GUI.Label(leftRect, content, _listItemLeftStyle);
                    GUI.Label(rightRect, rightText, _listItemRightStyle);
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

                    if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                    {
                        _selectedIndex = i;
                        Event.current.Use();
                        GUI.FocusControl(null);
                    }
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
                EditorGUILayout.TextArea(FormatJsonOrRaw(e.RequestBody) ?? string.Empty, GUILayout.MinHeight(80));
                GUILayout.Space(8);
                EditorGUILayout.LabelField("Response Body", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(FormatJsonOrRaw(e.ResponseBody) ?? string.Empty, GUILayout.MinHeight(120));
                EditorGUILayout.EndScrollView();

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Copy Request"))
                    {
                        EditorGUIUtility.systemCopyBuffer = FormatJsonOrRaw(e.RequestBody) ?? string.Empty;
                    }

                    if (GUILayout.Button("Copy Response"))
                    {
                        EditorGUIUtility.systemCopyBuffer = FormatJsonOrRaw(e.ResponseBody) ?? string.Empty;
                    }
                }
            }
        }

        private static void EnsureListStyles()
        {
            if (_listItemLeftStyle != null && _listItemRightStyle != null)
            {
                return;
            }

            _listItemLeftStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                padding = new RectOffset(8, 4, 2, 2),
                wordWrap = false
            };

            _listItemRightStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight,
                clipping = TextClipping.Clip,
                padding = new RectOffset(4, 8, 2, 2),
                wordWrap = false
            };
        }

        private static string FormatJsonOrRaw(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return raw;
            }

            var trimmed = raw.TrimStart();
            if (trimmed.Length == 0)
            {
                return raw;
            }

            var first = trimmed[0];
            if (first != '{' && first != '[')
            {
                return raw;
            }

            try
            {
                var jsonValue = JsonService.FromJson<JsonValue>(raw);
                return JsonService.ToJson(jsonValue, prettyPrint: true);
            }
            catch
            {
                return raw;
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
                sb.AppendLine(FormatJsonOrRaw(e.RequestBody) ?? string.Empty);
                sb.AppendLine("ResponseBody:");
                sb.AppendLine(FormatJsonOrRaw(e.ResponseBody) ?? string.Empty);
                sb.AppendLine(new string('-', 80));
            }

            File.WriteAllText(path, sb.ToString());
            AssetDatabase.Refresh();
        }
    }
}
