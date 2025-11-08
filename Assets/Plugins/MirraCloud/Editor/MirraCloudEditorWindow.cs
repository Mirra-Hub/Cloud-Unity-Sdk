using UnityEditor;
using UnityEngine;

namespace MirraCloud.Editor
{
    public class MirraCloudEditorWindow : EditorWindow
    {
        private Texture2D logo;

        private const int PADDING_x = 5;
        private const int PADDING_Y = 10;

        private Configuration _configuration;
        private GUIStyle _titleStyle;


        [MenuItem("Tools/Mirra Cloud/Manager")]
        public static void Open()
        {
            MirraCloudEditorWindow window = GetWindow<MirraCloudEditorWindow>();
            window.titleContent = new GUIContent("Mirra Cloud");
            window.minSize = new Vector2(300, 200);
        }

        private void OnEnable()
        {
            logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/Mirra Cloud/Editor/mirra_logo.png");
            _configuration = Configuration.Load();;
            
            _titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,  
                fontStyle = FontStyle.Bold 
            };
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            
            GUILayout.BeginHorizontal("box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            GUILayout.Space(PADDING_x);

            GUILayout.BeginVertical();
            GUILayout.Space(PADDING_Y);

            GUILayout.BeginHorizontal();
        
            if (logo != null)
            {
                GUILayout.Label(logo, GUILayout.Width(25), GUILayout.Height(25));
            }
        
            GUILayout.Label("Mirra Cloud", _titleStyle, GUILayout.Height(25));        
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Project ID", EditorStyles.label);
            _configuration.ProjectId = EditorGUILayout.TextField(_configuration.ProjectId);
            
            GUILayout.Space(4);
            
            EditorGUILayout.LabelField("Branch ID", EditorStyles.label);
            _configuration.BranchId = EditorGUILayout.TextField(_configuration.BranchId);
            
            GUILayout.Space(4);
            
            EditorGUILayout.LabelField("Token", EditorStyles.label);
            _configuration.Token = EditorGUILayout.TextField(_configuration.Token);

            GUILayout.Space(PADDING_Y);
            GUILayout.EndVertical();
            
            GUILayout.Space(PADDING_x);
            GUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_configuration);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
