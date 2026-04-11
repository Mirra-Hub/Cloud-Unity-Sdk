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

        private EditorApiService _apiService;
        private LoginView _loginView;
        private ProjectSettingsView _settingsView;

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

            _loginView = new LoginView(_apiService, Repaint);
            _loginView.OnConnected += OnConnected;

            _settingsView = new ProjectSettingsView(_apiService, _configuration, Repaint);
            _settingsView.OnDisconnectRequested += Disconnect;

            if (_apiService.IsAuthenticated)
            {
                _settingsView.LoadProjects();
            }
            else if (_loginView.HasSavedKey)
            {
                _loginView.AutoConnect();
            }
        }

        private void OnGUI()
        {
            EnsureInitialized();

            GUILayout.BeginHorizontal("box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            GUILayout.Space(PADDING_X);

            GUILayout.BeginVertical();
            GUILayout.Space(PADDING_Y);

            DrawHeader();
            GUILayout.Space(10);

            if (_apiService == null || !_apiService.IsAuthenticated)
            {
                _loginView.Draw();
            }
            else
            {
                _settingsView.Draw();
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

        private void OnConnected()
        {
            _settingsView.LoadProjects();
        }

        private void Disconnect()
        {
            _apiService.Disconnect();
            _loginView.Reset();
            _settingsView.Reset();
            Repaint();
        }
    }
}
