using UnityEngine;

namespace qy
{
    public class GameMainManager
    {
        private static GameMainManager _instance;

        public static GameMainManager Instance
        {
            get
            {
                if (Application.isPlaying && _instance == null)
                {
                    _instance = new GameMainManager();
                }
                return _instance;
            }
        }

        public ui.IUIManager uiManager
        {
            get
            {
                return ui.UIManager.Instance;
            }
        }
        public net.NetManager netManager
        {
            get
            {
                return net.NetManager.Instance;
            }
        }
        public IAudioManager audioManager
        {
            get
            {
                return AudioManager.Instance; 
            }
        }

        public config.ConfigManager configManager
        {
            get
            {
                return config.ConfigManager.Instance; 
            }
        }

        public WeatherManager weatherManager
        {
            get
            {
                return WeatherManager.Instance;
            }
        }

        public PlayerData playerData;
        public IPlayerModel playerModel;

        public int Chapter
        {
            get
            {
                var quest = playerData.GetQuest();
                return quest != null ? quest.chapter : 1;
            }
        }

        /// <summary>
        /// //全局脚本，可以使用monobehaviour方法
        /// </summary>
        public MonoBehaviour mono;
        
        public GameMainManager()
        {
            playerData = LocalDatasManager.playerData;
            playerModel = new PlayerModel(playerData);
            GuideManager guide = GuideManager.Instance;
            audioManager.SetSoundPathProxy(FilePathTools.getAudioPath);
            audioManager.SetMusicPathProxy(FilePathTools.getAudioPath);
        }

        public void Init()
        {
        }
    }
}


