using UnityEngine.SceneManagement;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.SceneManagement
{
    public class SceneLoader
    {
        public const string LOBBY_SCENE_NAME = "MC_Example_Lobby";
        public const string LOGIN_SCENE_NAME = "MC_Example_Login";
        
        public void LoadLobbyScene()
        {
            SceneManager.LoadSceneAsync(LOBBY_SCENE_NAME, LoadSceneMode.Single);
        }   
        
        public void LoadLoginScene()
        {
            SceneManager.LoadSceneAsync(LOGIN_SCENE_NAME, LoadSceneMode.Single);
        }
    }
}