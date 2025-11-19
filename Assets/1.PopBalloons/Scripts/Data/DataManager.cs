using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PopBalloons.Utilities;


#if !NETFX_CORE
using System.Threading;
#else
using Windows.System.Threading;
using Windows.Foundation;
#endif

#if WINDOWS_UWP
using Windows.Storage;
#endif

namespace PopBalloons.Data
{


    public class DataManager : MonoBehaviour
    {

        /// <summary>
        /// This script serialize several informations gathered from user's 
        /// behaviour and play results.
        /// </summary>
        public static DataManager instance;
        private string filePath;

#if WINDOWS_UWP
        private StorageFile dataFile;
        private StorageFolder targetFolder;
#else
        private string targetFolder;
#endif
        private List<Datas> datasList;
        private DatasCollection datasCollection;
        private Datas datas;
        private string currentSaveTime;


        private string currentDay;

        private bool isRecording = false;
        


#if !NETFX_CORE
        Thread DataManagement;
#endif
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != null)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            //SceneManager.sceneLoaded += handleLevelEnd;
            GameCreator.OnGameStarted += HandleLevelStart;
            GameCreator.OnGameEnded += HandleLevelEnd;
            GameCreator.OnGameInterrupted += HandleLevelEnd;
            //GameManager.OnGameStateChanged += 
        }

        private void HandleLevelEnd(GameManager.GameType type)
        {
            // Skip data saving for FREEPLAY mode (no level data)
            if (type == GameManager.GameType.FREEPLAY)
            {
                Debug.Log("DataManager: Skipping data save for FREEPLAY mode");
                return;
            }
            
            // Safety check: ensure we have data to save
            if (datasList == null || datasList.Count == 0)
            {
                Debug.LogWarning("DataManager: No data to save (datasList is empty)");
                return;
            }
            
            // Safety check: ensure ScoreManager exists
            if (ScoreManager.instance == null)
            {
                Debug.LogError("DataManager: ScoreManager.instance is null! Cannot save score data.");
                return;
            }
            
            int datasIndex = datasList.Count - 1;

            datasList[datasIndex].levelDatas.score = ScoreManager.instance.score;
            //    datasList[datasIndex].levelDatas.name = level;
            //    datasList[datasIndex].levelDatas.score = score;
            switch (type)
            {
                case GameManager.GameType.COGNITIVE:
                    CognitiveDatas cognitiveInfos = new CognitiveDatas();
                    cognitiveInfos.correctBalloons = ScoreManager.instance.CorrectBalloon;
                    cognitiveInfos.wrongBalloons = ScoreManager.instance.WrongBalloon;
                    
                    //TODO: Add more infos
                    datasList[datasIndex].levelDatas.cognitiveDatas = cognitiveInfos;
                    break;
                case GameManager.GameType.FREEPLAY:
                    break;
                default:
                    break;
            }
            isRecording = false;
            SaveDatas();
        }

        private void HandleLevelStart(GameManager.GameType type)
        {
            InitRecording();
            isRecording = true;
        }

#if WINDOWS_UWP
        public async void InitRecording()
        {
            StorageFolder baseFolder =  await KnownFolders.PicturesLibrary.CreateFolderAsync("PopBalloon", CreationCollisionOption.OpenIfExists);
            StorageFolder dataFolder;
            if (!Directory.Exists(baseFolder.Path + "/Datas"))
            {
                dataFolder = await baseFolder.CreateFolderAsync("Datas", CreationCollisionOption.OpenIfExists);
            }
            else
	        {
                dataFolder = await baseFolder.GetFolderAsync("Datas");
	        }

            if (ProfilesManager.Instance.GetCurrentProfile() != null && ProfilesManager.Instance.GetCurrentProfile().data != null && ProfilesManager.Instance.GetCurrentProfile().data.username != "")
            {
                //Define a subfolder for current day sessions
                currentDay = DateTime.Now.ToString("yyyy-MM-dd");
                string targetFolderName = string.Format("{0}-{1}", ProfilesManager.Instance.GetCurrentProfile().data.username, currentDay);
                if (!Directory.Exists(Path.Combine(dataFolder.Path, targetFolderName)))
                {
                    targetFolder = await dataFolder.CreateFolderAsync(targetFolderName, CreationCollisionOption.OpenIfExists);
                }
                else
	            {
                    targetFolder = await dataFolder.GetFolderAsync(targetFolderName);
	            }

                currentSaveTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                dataFile = await targetFolder.CreateFileAsync(currentSaveTime + ".json", CreationCollisionOption.ReplaceExisting);
                filePath = dataFile.Path;
                
                // LoadDatas();
                InitDatas();
            }
        }
#else
        public void InitRecording()
        {
            String persistentDataPath = Application.persistentDataPath;
            //Create Data folder if needed
            if (!Directory.Exists(persistentDataPath + "/Datas"))
                Directory.CreateDirectory(persistentDataPath + "/Datas");

            if (ProfilesManager.Instance.GetCurrentProfile() != null && ProfilesManager.Instance.GetCurrentProfile().data != null)
            {
                // Use username if available, otherwise use profile ID
                string folderName = !string.IsNullOrEmpty(ProfilesManager.Instance.GetCurrentProfile().data.username) 
                    ? ProfilesManager.Instance.GetCurrentProfile().data.username 
                    : ProfilesManager.Instance.GetCurrentProfile().id;
                
                //Create user folder if required
                if (!Directory.Exists(persistentDataPath + string.Format("/Datas/{0}", folderName)))
                    Directory.CreateDirectory(persistentDataPath + string.Format("/Datas/{0}", folderName));

                //Define a subfolder for current day sessions
                currentDay = DateTime.Now.ToString("yyyy-MM-dd");
                targetFolder = persistentDataPath + string.Format("/Datas/{0}/{1}", folderName, currentDay);

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }
                currentSaveTime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                filePath = targetFolder + string.Format("/{0}.json", currentSaveTime);
                //currentSaveTime = DateTime.Now.ToString("yyyy/MM/dd/ HH:mm");
                //currentSaveTime = DateTime.Now.ToString("yyyy/MM/dd/ HH:mm");
                
                // Intended to load previous instance of data if any (not fully implemented yet)
                // LoadDatas();
                InitDatas();
                
                Debug.Log($"DataManager: Recording initialized for profile {folderName}");
            }
            else
            {
                Debug.LogWarning("DataManager: Cannot init recording - no current profile!");
            }
        }
#endif
        private void OnDestroy()
        {
            //SceneManager.sceneLoaded -= HandleLevelEnd;
            GameCreator.OnGameStarted -= HandleLevelStart;
            GameCreator.OnGameEnded -= HandleLevelEnd;
            GameCreator.OnGameInterrupted -= HandleLevelEnd;
        }

        // Gather all the datas about User's Gaze
        public void AddGazeItemData(GazeItemDatas datas)
        {
            if (datasList != null && isRecording)
            {
                int datasIndex = datasList.Count - 1;
                datasList[datasIndex].levelDatas.listGazeItemDatas.Add(datas);
            }
        }
        
        public void AddGazeData(GazeDatas data)
        {
            if (datasList != null && isRecording)
            {
                int datasIndex = datasList.Count - 1;
                datasList[datasIndex].levelDatas.listGazeDatas.Add(data);
            }
        }

        // Gather all the datas about balloons events
        public void AddBalloonsDatas(BalloonDatas datas)
        {
            if (datasList != null && isRecording)
            {
                int datasIndex = datasList.Count - 1;
                datasList[datasIndex].levelDatas.listBalloonDatas.Add(datas);
            }
        }

        // Gather all the datas about user while playing
        public void AddUsersDatas(UserDatas datas)
        {
            if (datasList != null && isRecording)
            {
                int datasIndex = datasList.Count - 1;
                datasList[datasIndex].levelDatas.userDatas.Add(datas);
            }
        }

        public void AddWavesData(Waves wavesData)
        {
            if (datasList != null && isRecording)
            {
                int dataIndex = datasList.Count - 1;
                datasList[dataIndex].levelDatas.Waves.Add(wavesData);
            }
        }
        
#if WINDOWS_UWP
        public async void SaveDatas()
        {
            try
            {
                string json = JsonUtility.ToJson(datasCollection, true);
                if (dataFile != null)
                    await FileIO.WriteTextAsync(dataFile, json);
            }
            catch(FileNotFoundException)
            {
                // For example, handle file not found
            }

        }
#else
        // Save all datas gathered during current level before loading the next one.
        public void SaveDatas()
        {
            //    int datasIndex = datasList.Count - 1;
            //    datasList[datasIndex].levelDatas.name = level;
            //    datasList[datasIndex].levelDatas.score = score;

            string json = JsonUtility.ToJson(datasCollection, true);
#if !NETFX_CORE
            DataManagement = new Thread( () => File.WriteAllText(filePath, json) );
            DataManagement.Start();
#else
            IAsyncAction asyncAction = ThreadPool.RunAsync((workItem)=>File.WriteAllText(filePath, json));
#endif
        }
#endif
        //Load all players profiles from json file
        public void LoadDatas()
        {
            string json = null;
            datasList = new List<Datas>();


            //Currently doesn't load any previous data for performance issue
            //if (!Directory.Exists(Application.persistentDataPath + string.Format("/Datas/{0}", ProfilesManager.Instance.GetCurrentProfile().data.username)))
            //    return;
        
            //if (File.Exists(filePath))
            //{
            //    json = File.ReadAllText(filePath);
            //    datasCollection = JsonUtility.FromJson<DatasCollection>(json);
            //    datasList = datasCollection.datasList;
            //}
        }

        // Initialization of serialization container and objects
        private void InitDatas()
        {
            datasList = new List<Datas>();
            datas = new Datas();

            datas.dateTime =  DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            datas.username = ProfilesManager.Instance.CurrentProfile.id;
            //NEw level is started
            
            
            LevelDatas levelData = new LevelDatas();
            datas.levelDatas.mode = GameManager.Instance.CurrentGameType.ToString();
            datas.levelDatas.name = GameManager.Instance.CurrentGameType + "_" + (GameManager.Instance.CurrentLevelIndex + 1);
            datas.levelDatas.score = 0;
            datas.levelDatas.userDatas = new List<UserDatas>();
            datas.levelDatas.listBalloonDatas = new List<BalloonDatas>();
            datas.levelDatas.listGazeItemDatas = new List<GazeItemDatas>();
            datas.levelDatas.listGazeDatas  = new List<GazeDatas>();
            datas.levelDatas.Waves = new List<Waves>();

            //// Créer une instance de la classe Options et la remplir avec les données du ballon
            //Options balloonOptions = new Options();
            //Waves wave = new Waves();
//
            //
            //for (int i = 0; i < totalBalloon; i++)
            //{
            //    balloonOptions.id = balloon.GetInstanceID();
            //    balloonOptions.color = balloon.GetColor().ToString();
            //    balloonOptions.balloonPosition = pos;
            //}
            //wave.intendedColor = intendedColor.ToString();
            //wave.balloonsOptions.Add(balloonOptions);
            //// Ajouter l'instance à la liste options
            ////options.Add(balloonOptions);
            //Debug.Log("Wavezz: " + JsonUtility.ToJson(wave, true));            //Debug.Log("Contents of options list: " + string.Join(", ", options.Select(opt => $"Color: {balloonOptions.color}, Position: {balloonOptions.balloonPosition}")));

            
            //Usefull for multiple level inside one json file
            //for (int i = 1; i <= 4; i++)
            //{
            //    datas.listLevelDatas.Add(new LevelDatas());
            //    datas.listLevelDatas[i - 1].name = "Level" + i.ToString();
            //    datas.listLevelDatas[i - 1].score = 0;
            //    datas.listLevelDatas[i - 1].userDatas = new List<UserDatas>();
            //    datas.listLevelDatas[i - 1].listBalloonDatas = new List<BalloonDatas>();
            //    //datas.listLevelDatas[i - 1].userDatas.handPos = new List<Vector3>();
            //}
            //
            
            datasList.Add(datas);
            datasCollection = new DatasCollection(datasList);
        }
    }

    // Datas serialization objects
    [Serializable]
    public class UserDatas
    {
        public Vector3 headPos;
        public float headRotationY;
        public float BPM;
        public string timeStamp;
    }

    [Serializable]
    public class BalloonDatas
    {
        // Temps du balloon
        public string timeOfSpawn;
        public string timeOfDestroy;
        public float lifeTime;

        //Gain de point ou condition de reussite / echec
        public float balloonPointGain;
        public bool balloonWasDestroyByUser;
        public bool balloonTimout;
        public string poppedColor;
        public string intendedColor;

        // distance parcourue depuis l'apparition du ballon.
        public float distance;

        //position du balloon
        public Vector3 balloonInitialPosition;
    }

    [Serializable]
    public class GazeItemDatas
    {
        public string objectType;
        public string timeOfLook;
        public float duration;
        public string targetName;
    }
    
    [Serializable]
    public class GazeDatas
    {
        public string targetName;
        public string timeStamp;
        public bool targetIsValid;
        public bool isCalibrationValid;
        public Vector3 origin ;
        public Vector3 direction;
        public Vector3 eyeGazeTarget;
    }
    [Serializable]
    public class Options
    {
        public string color;
        public int id;
        public Vector3 balloonPosition;
    }
    
    [Serializable]
    public class Waves
    {
        public string intendedColor;
        public int nbOption;
        public List<Options> balloonsOptions = new List<Options>();
    }
    
    [Serializable]
    public class CognitiveDatas
    {
        //TODO: Add color option
        //TODO: Add reversal color 
        public int correctBalloons;
        public int wrongBalloons;
    }
    
    [Serializable]
    public class LevelDatas
    {
        public string mode;
        public string name;
        public int score;
        public CognitiveDatas cognitiveDatas = null;
        public List<Waves> Waves = new List<Waves>();
        public List<UserDatas> userDatas = new List<UserDatas>();
        public List<BalloonDatas> listBalloonDatas = new List<BalloonDatas>();
        public List<GazeItemDatas> listGazeItemDatas = new List<GazeItemDatas>();
        public List<GazeDatas> listGazeDatas = new List<GazeDatas>();
    }

    [Serializable]
    public class Datas
    {
        public string username = "";
        public string dateTime ;
        public LevelDatas levelDatas = new LevelDatas();
        //public List<LevelDatas> listLevelDatas = new List<LevelDatas>();
    }

    [Serializable]
    public class DatasCollection
    {
        [SerializeField]
        public List<Datas> datasList;

        public DatasCollection(List<Datas> _datasList)
        {
            datasList = _datasList;
        }
    }
}