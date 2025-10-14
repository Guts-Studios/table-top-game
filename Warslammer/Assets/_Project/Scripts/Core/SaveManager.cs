using System;
using System.IO;
using UnityEngine;

namespace Warslammer.Core
{
    /// <summary>
    /// Manages game save/load operations using JSON serialization
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        #region Singleton
        private static SaveManager _instance;
        
        /// <summary>
        /// Global access point for the SaveManager
        /// </summary>
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SaveManager>();
                }
                return _instance;
            }
        }
        #endregion

        #region Properties
        [Header("Save Configuration")]
        [SerializeField]
        [Tooltip("Directory name for save files")]
        private string _saveDirectory = "Saves";
        
        [SerializeField]
        [Tooltip("File extension for save files")]
        private string _saveExtension = ".json";

        /// <summary>
        /// Full path to the save directory
        /// </summary>
        private string SavePath => Path.Combine(Application.persistentDataPath, _saveDirectory);

        /// <summary>
        /// Current save slot
        /// </summary>
        public int CurrentSaveSlot { get; private set; } = 0;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            
            _instance = this;
            
            // Ensure save directory exists
            EnsureSaveDirectoryExists();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Ensure the save directory exists
        /// </summary>
        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
                Debug.Log($"[SaveManager] Created save directory: {SavePath}");
            }
        }
        #endregion

        #region Save Operations
        /// <summary>
        /// Save game data to a specific slot
        /// </summary>
        /// <param name="saveData">Data to save</param>
        /// <param name="slotIndex">Save slot index</param>
        /// <returns>True if save was successful</returns>
        public bool SaveGame(GameSaveData saveData, int slotIndex)
        {
            try
            {
                string filePath = GetSaveFilePath(slotIndex);
                string jsonData = JsonUtility.ToJson(saveData, true);
                
                File.WriteAllText(filePath, jsonData);
                
                CurrentSaveSlot = slotIndex;
                Debug.Log($"[SaveManager] Game saved to slot {slotIndex}: {filePath}");
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Save game data to the current slot
        /// </summary>
        /// <param name="saveData">Data to save</param>
        /// <returns>True if save was successful</returns>
        public bool SaveGame(GameSaveData saveData)
        {
            return SaveGame(saveData, CurrentSaveSlot);
        }

        /// <summary>
        /// Quick save to a dedicated quick save slot
        /// </summary>
        /// <param name="saveData">Data to save</param>
        /// <returns>True if save was successful</returns>
        public bool QuickSave(GameSaveData saveData)
        {
            return SaveGame(saveData, -1); // Use -1 for quick save slot
        }
        #endregion

        #region Load Operations
        /// <summary>
        /// Load game data from a specific slot
        /// </summary>
        /// <param name="slotIndex">Save slot index</param>
        /// <returns>Loaded save data, or null if load failed</returns>
        public GameSaveData LoadGame(int slotIndex)
        {
            try
            {
                string filePath = GetSaveFilePath(slotIndex);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveManager] Save file not found: {filePath}");
                    return null;
                }
                
                string jsonData = File.ReadAllText(filePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                
                CurrentSaveSlot = slotIndex;
                Debug.Log($"[SaveManager] Game loaded from slot {slotIndex}");
                
                return saveData;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Quick load from the dedicated quick save slot
        /// </summary>
        /// <returns>Loaded save data, or null if load failed</returns>
        public GameSaveData QuickLoad()
        {
            return LoadGame(-1); // Use -1 for quick save slot
        }
        #endregion

        #region Save Management
        /// <summary>
        /// Delete a save file
        /// </summary>
        /// <param name="slotIndex">Save slot index to delete</param>
        /// <returns>True if deletion was successful</returns>
        public bool DeleteSave(int slotIndex)
        {
            try
            {
                string filePath = GetSaveFilePath(slotIndex);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveManager] Save file not found: {filePath}");
                    return false;
                }
                
                File.Delete(filePath);
                Debug.Log($"[SaveManager] Deleted save slot {slotIndex}");
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to delete save: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a save exists in a specific slot
        /// </summary>
        /// <param name="slotIndex">Save slot index to check</param>
        /// <returns>True if save exists</returns>
        public bool SaveExists(int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Get save file metadata
        /// </summary>
        /// <param name="slotIndex">Save slot index</param>
        /// <returns>Save metadata, or null if not found</returns>
        public SaveMetadata GetSaveMetadata(int slotIndex)
        {
            try
            {
                string filePath = GetSaveFilePath(slotIndex);
                
                if (!File.Exists(filePath))
                    return null;
                
                FileInfo fileInfo = new FileInfo(filePath);
                
                return new SaveMetadata
                {
                    slotIndex = slotIndex,
                    saveDate = fileInfo.LastWriteTime,
                    fileSize = fileInfo.Length
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to get save metadata: {e.Message}");
                return null;
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get the full file path for a save slot
        /// </summary>
        private string GetSaveFilePath(int slotIndex)
        {
            string fileName = slotIndex == -1 ? "quicksave" : $"save_{slotIndex}";
            return Path.Combine(SavePath, fileName + _saveExtension);
        }

        /// <summary>
        /// Get the number of available save slots
        /// </summary>
        public int GetSaveSlotCount()
        {
            // Count existing save files
            if (!Directory.Exists(SavePath))
                return 0;
            
            string[] files = Directory.GetFiles(SavePath, "*" + _saveExtension);
            return files.Length;
        }
        #endregion
    }

    #region Save Data Structures
    /// <summary>
    /// Main save data structure
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public string saveName;
        public string saveDate;
        public int currentTurn;
        public GameState gameState;
        
        // TODO: Add more save data fields
        // - Battle state
        // - Unit positions and states
        // - Campaign progress
        // - Player armies
        // - Mission state
    }

    /// <summary>
    /// Save file metadata
    /// </summary>
    [Serializable]
    public class SaveMetadata
    {
        public int slotIndex;
        public DateTime saveDate;
        public long fileSize;
    }
    #endregion
}