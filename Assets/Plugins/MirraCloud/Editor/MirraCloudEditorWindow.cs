using System.Collections.Generic;
using MirraCloud.Editor.Dto;
using UnityEditor;
using UnityEngine;

namespace MirraCloud.Editor
{
    public class MirraCloudEditorWindow : EditorWindow
    {
        private Texture2D logo;

        private const int PADDING_X = 5;
        private const int PADDING_Y = 10;

        private Configuration _configuration;
        private GUIStyle _titleStyle;
        private GUIStyle _sectionStyle;

        private EditorApiService _apiService;

        private string _saKeyInput = "";
        private bool _isConnecting;
        private string _connectError;

        private List<EditorProjectDto> _projects;
        private List<EditorBranchDto> _branches;
        private List<EditorApiTokenDto> _tokens;

        private int _selectedProjectIndex = -1;
        private int _selectedBranchIndex = -1;
        private int _selectedTokenIndex = -1;

        private bool _isLoadingProjects;
        private bool _isLoadingBranches;
        private bool _isLoadingTokens;

        [MenuItem("Tools/Mirra Cloud/Manager")]
        public static void Open()
        {
            MirraCloudEditorWindow window = GetWindow<MirraCloudEditorWindow>();
            window.titleContent = new GUIContent("Mirra Cloud");
            window.minSize = new Vector2(350, 300);
        }

        private void OnEnable()
        {
            logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Mirra Cloud/Editor/mirra_logo.png");

            _titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_apiService != null) return;

            _configuration = Configuration.Load();
            _apiService = new EditorApiService(_configuration);
            _saKeyInput = EditorApiService.GetSavedKey();

            if (_apiService.IsAuthenticated)
            {
                LoadProjects();
            }
            else if (!string.IsNullOrEmpty(_saKeyInput))
            {
                Connect();
            }
        }

        private void OnGUI()
        {
            EnsureInitialized();

            if (_sectionStyle == null)
            {
                _sectionStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
            }

            GUILayout.BeginHorizontal("box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            GUILayout.Space(PADDING_X);

            GUILayout.BeginVertical();
            GUILayout.Space(PADDING_Y);

            DrawHeader();
            GUILayout.Space(10);

            if (_apiService == null || !_apiService.IsAuthenticated)
            {
                DrawLoginView();
            }
            else
            {
                DrawConnectedView();
            }

            GUILayout.Space(PADDING_Y);
            GUILayout.EndVertical();

            GUILayout.Space(PADDING_X);
            GUILayout.EndHorizontal();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();

            if (logo != null)
            {
                GUILayout.Label(logo, GUILayout.Width(25), GUILayout.Height(25));
            }

            GUILayout.Label("Mirra Cloud", _titleStyle, GUILayout.Height(25));
            GUILayout.EndHorizontal();
        }

        private void DrawLoginView()
        {
            GUILayout.Label("Connect with Service Account", _sectionStyle);
            GUILayout.Space(6);

            EditorGUILayout.LabelField("Service Account Key", EditorStyles.label);
            _saKeyInput = EditorGUILayout.PasswordField(_saKeyInput);

            GUILayout.Space(8);

            if (!string.IsNullOrEmpty(_connectError))
            {
                var prevColor = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.LabelField(_connectError, EditorStyles.wordWrappedLabel);
                GUI.color = prevColor;
                GUILayout.Space(4);
            }

            EditorGUI.BeginDisabledGroup(_isConnecting || string.IsNullOrEmpty(_saKeyInput));
            if (GUILayout.Button(_isConnecting ? "Connecting..." : "Connect", GUILayout.Height(28)))
            {
                Connect();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawConnectedView()
        {
            var prevColor = GUI.color;
            GUI.color = new Color(0.4f, 0.8f, 0.4f);
            EditorGUILayout.LabelField("Connected", _sectionStyle);
            GUI.color = prevColor;

            GUILayout.Space(6);

            DrawProjectDropdown();
            GUILayout.Space(4);

            DrawBranchDropdown();
            GUILayout.Space(4);

            DrawTokenDropdown();
            GUILayout.Space(12);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Height(24)))
            {
                LoadProjects();
            }
            if (GUILayout.Button("Disconnect", GUILayout.Height(24)))
            {
                Disconnect();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawProjectDropdown()
        {
            EditorGUILayout.LabelField("Project", EditorStyles.label);

            if (_isLoadingProjects)
            {
                EditorGUILayout.LabelField("Loading projects...", EditorStyles.miniLabel);
                return;
            }

            if (_projects == null || _projects.Count == 0)
            {
                EditorGUILayout.LabelField("No projects available", EditorStyles.miniLabel);
                return;
            }

            var names = new string[_projects.Count];
            for (int i = 0; i < _projects.Count; i++)
            {
                names[i] = _projects[i].name;
            }

            var currentIndex = FindCurrentIndex(_projects, _configuration.ProjectId, p => p.id);
            if (_selectedProjectIndex < 0) _selectedProjectIndex = currentIndex;

            EditorGUI.BeginChangeCheck();
            _selectedProjectIndex = EditorGUILayout.Popup(_selectedProjectIndex, names);
            if (EditorGUI.EndChangeCheck())
            {
                OnProjectSelected();
            }
        }

        private void DrawBranchDropdown()
        {
            EditorGUILayout.LabelField("Branch", EditorStyles.label);

            if (_isLoadingBranches)
            {
                EditorGUILayout.LabelField("Loading branches...", EditorStyles.miniLabel);
                return;
            }

            if (_branches == null || _branches.Count == 0)
            {
                EditorGUILayout.LabelField(_selectedProjectIndex >= 0 ? "No branches available" : "Select a project first", EditorStyles.miniLabel);
                return;
            }

            var names = new string[_branches.Count];
            for (int i = 0; i < _branches.Count; i++)
            {
                names[i] = _branches[i].name;
            }

            var currentIndex = FindCurrentIndex(_branches, _configuration.BranchId, b => b.id);
            if (_selectedBranchIndex < 0) _selectedBranchIndex = currentIndex;

            EditorGUI.BeginChangeCheck();
            _selectedBranchIndex = EditorGUILayout.Popup(_selectedBranchIndex, names);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyBranch();
            }
        }

        private void DrawTokenDropdown()
        {
            EditorGUILayout.LabelField("API Token", EditorStyles.label);

            if (_isLoadingTokens)
            {
                EditorGUILayout.LabelField("Loading tokens...", EditorStyles.miniLabel);
                return;
            }

            if (_tokens == null || _tokens.Count == 0)
            {
                EditorGUILayout.LabelField(_selectedProjectIndex >= 0 ? "No tokens available" : "Select a project first", EditorStyles.miniLabel);
                return;
            }

            var names = new string[_tokens.Count];
            for (int i = 0; i < _tokens.Count; i++)
            {
                var t = _tokens[i];
                names[i] = t.isEnabled ? t.name : $"{t.name} (disabled)";
            }

            var currentIndex = FindCurrentIndex(_tokens, _configuration.Token, t => t.token);
            if (_selectedTokenIndex < 0) _selectedTokenIndex = currentIndex;

            EditorGUI.BeginChangeCheck();
            _selectedTokenIndex = EditorGUILayout.Popup(_selectedTokenIndex, names);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyToken();
            }
        }

        private void Connect()
        {
            _isConnecting = true;
            _connectError = null;
            Repaint();

            var op = _apiService.ExchangeKeyAsync(_saKeyInput);
            op.OnCompleted += _ =>
            {
                _isConnecting = false;

                if (op.Result.IsSuccess)
                {
                    LoadProjects();
                }
                else
                {
                    _connectError = op.Result.Error?.Message ?? "Authentication failed";
                }

                Repaint();
            };
        }

        private void Disconnect()
        {
            _apiService.Disconnect();
            _saKeyInput = "";
            _projects = null;
            _branches = null;
            _tokens = null;
            _selectedProjectIndex = -1;
            _selectedBranchIndex = -1;
            _selectedTokenIndex = -1;
            Repaint();
        }

        private void LoadProjects()
        {
            var orgId = _apiService.OrgId;
            if (string.IsNullOrEmpty(orgId)) return;

            _isLoadingProjects = true;
            _projects = null;
            _branches = null;
            _tokens = null;
            _selectedProjectIndex = -1;
            _selectedBranchIndex = -1;
            _selectedTokenIndex = -1;
            Repaint();

            var op = _apiService.GetProjectsAsync(orgId);
            op.OnCompleted += _ =>
            {
                _isLoadingProjects = false;
                if (op.Result.IsSuccess)
                {
                    _projects = op.Result.Data ?? new List<EditorProjectDto>();
                    var idx = FindCurrentIndex(_projects, _configuration.ProjectId, p => p.id);
                    if (idx >= 0)
                    {
                        _selectedProjectIndex = idx;
                        OnProjectSelected();
                    }
                }
                Repaint();
            };
        }

        private void OnProjectSelected()
        {
            if (_projects == null || _selectedProjectIndex < 0 || _selectedProjectIndex >= _projects.Count) return;

            var project = _projects[_selectedProjectIndex];
            _configuration.ProjectId = project.id;
            SaveConfiguration();

            _branches = null;
            _tokens = null;
            _selectedBranchIndex = -1;
            _selectedTokenIndex = -1;

            LoadBranches(project.id);
            LoadTokens(project.id);
        }

        private void LoadBranches(string projectId)
        {
            _isLoadingBranches = true;
            Repaint();

            var op = _apiService.GetBranchesAsync(projectId);
            op.OnCompleted += _ =>
            {
                _isLoadingBranches = false;
                if (op.Result.IsSuccess)
                {
                    _branches = op.Result.Data ?? new List<EditorBranchDto>();
                    var idx = FindCurrentIndex(_branches, _configuration.BranchId, b => b.id);
                    if (idx >= 0)
                    {
                        _selectedBranchIndex = idx;
                        ApplyBranch();
                    }
                }
                Repaint();
            };
        }

        private void LoadTokens(string projectId)
        {
            var orgId = _apiService.OrgId;
            if (string.IsNullOrEmpty(orgId)) return;

            _isLoadingTokens = true;
            Repaint();

            var op = _apiService.GetTokensAsync(orgId, projectId);
            op.OnCompleted += _ =>
            {
                _isLoadingTokens = false;
                if (op.Result.IsSuccess)
                {
                    _tokens = op.Result.Data ?? new List<EditorApiTokenDto>();
                    var idx = FindCurrentIndex(_tokens, _configuration.Token, t => t.token);
                    if (idx >= 0)
                    {
                        _selectedTokenIndex = idx;
                        ApplyToken();
                    }
                }
                Repaint();
            };
        }

        private void ApplyBranch()
        {
            if (_branches == null || _selectedBranchIndex < 0 || _selectedBranchIndex >= _branches.Count) return;
            _configuration.BranchId = _branches[_selectedBranchIndex].id;
            SaveConfiguration();
        }

        private void ApplyToken()
        {
            if (_tokens == null || _selectedTokenIndex < 0 || _selectedTokenIndex >= _tokens.Count) return;
            _configuration.Token = _tokens[_selectedTokenIndex].token;
            SaveConfiguration();
        }

        private void SaveConfiguration()
        {
            EditorUtility.SetDirty(_configuration);
            AssetDatabase.SaveAssets();
        }

        private static int FindCurrentIndex<T>(List<T> items, string currentValue, System.Func<T, string> getId)
        {
            if (items == null || string.IsNullOrEmpty(currentValue)) return -1;
            for (int i = 0; i < items.Count; i++)
            {
                if (getId(items[i]) == currentValue) return i;
            }
            return -1;
        }
    }
}
