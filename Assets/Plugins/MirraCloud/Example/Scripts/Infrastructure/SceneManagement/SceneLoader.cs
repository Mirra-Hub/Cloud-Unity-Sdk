using UnityEngine.SceneManagement;

namespace Plugins.MirraCloud.Example.Scripts.Infrastructure.SceneManagement
{
    public class SceneLoader
    {
        public const string LOBBY_SCENE_NAME = "MC_Example_Lobby";
        
        public void LoadLobby()
        {
            SceneManager.LoadSceneAsync(LOBBY_SCENE_NAME, LoadSceneMode.Single);
        }
    }
}