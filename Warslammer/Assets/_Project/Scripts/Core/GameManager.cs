using UnityEngine;
using UnityEngine.Events;

namespace Warslammer.Core
{
    /// <summary>
    /// Main game manager - handles overall game state and coordinates all systems
    /// Singleton pattern for global access
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        private static GameManager _instance;
        
        /// <summary>
        /// Global access point for the GameManager
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Fired when the game state changes
        /// </summary>
        public UnityEvent<GameState> OnGameStateChanged = new UnityEvent<GameState>();
        
        /// <summary>
        /// Fired when the game is paused or unpaused
        /// </summary>
        public UnityEvent<bool> OnPauseStateChanged = new UnityEvent<bool>();
        #endregion

        #region Properties
        [Header("Game State")]
        [SerializeField]
        private GameState _currentState = GameState.MainMenu;
        
        /// <summary>
        /// Current state of the game
        /// </summary>
        public GameState CurrentState => _currentState;

        /// <summary>
        /// Is the game currently paused?
        /// </summary>
        public bool IsPaused { get; private set; }

        [Header("System References")]
        [Tooltip("Reference to the Turn Manager")]
        public TurnManager turnManager;
        
        [Tooltip("Reference to the Phase Manager")]
        public PhaseManager phaseManager;
        
        [Tooltip("Reference to the Save Manager")]
        public SaveManager saveManager;

        // TODO: Add references to other managers as they're implemented
        // - BattlefieldManager
        // - UIManager
        // - InputManager
        // - AudioManager
        // - CombatManager
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Enforce singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSystems();
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
        /// Initialize all game systems
        /// </summary>
        private void InitializeSystems()
        {
            Debug.Log("[GameManager] Initializing game systems...");

            // Find or create manager components
            if (turnManager == null)
                turnManager = GetComponent<TurnManager>() ?? gameObject.AddComponent<TurnManager>();
            
            if (phaseManager == null)
                phaseManager = GetComponent<PhaseManager>() ?? gameObject.AddComponent<PhaseManager>();
            
            if (saveManager == null)
                saveManager = GetComponent<SaveManager>() ?? gameObject.AddComponent<SaveManager>();

            // TODO: Initialize other systems
            
            Debug.Log("[GameManager] Game systems initialized successfully");
        }
        #endregion

        #region Game State Management
        /// <summary>
        /// Change the current game state
        /// </summary>
        /// <param name="newState">New state to transition to</param>
        public void ChangeGameState(GameState newState)
        {
            if (_currentState == newState)
                return;

            GameState previousState = _currentState;
            _currentState = newState;

            Debug.Log($"[GameManager] State changed: {previousState} -> {newState}");

            // Handle state exit logic
            OnStateExit(previousState);

            // Handle state entry logic
            OnStateEnter(newState);

            // Notify listeners
            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// Called when exiting a game state
        /// </summary>
        private void OnStateExit(GameState state)
        {
            switch (state)
            {
                case GameState.Battle:
                    // Clean up battle state
                    // TODO: Implement battle cleanup
                    break;
                case GameState.Campaign:
                    // Save campaign progress
                    // TODO: Implement campaign save
                    break;
            }
        }

        /// <summary>
        /// Called when entering a new game state
        /// </summary>
        private void OnStateEnter(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    // TODO: Load main menu UI
                    break;
                case GameState.Loading:
                    // TODO: Show loading screen
                    break;
                case GameState.Battle:
                    // Initialize battle systems
                    if (turnManager != null)
                        turnManager.InitializeBattle();
                    break;
                case GameState.ArmyBuilder:
                    // TODO: Load army builder UI
                    break;
                case GameState.Campaign:
                    // TODO: Load campaign UI
                    break;
                case GameState.Paused:
                    SetPaused(true);
                    break;
            }
        }
        #endregion

        #region Pause Management
        /// <summary>
        /// Pause or unpause the game
        /// </summary>
        /// <param name="paused">True to pause, false to unpause</param>
        public void SetPaused(bool paused)
        {
            if (IsPaused == paused)
                return;

            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;

            Debug.Log($"[GameManager] Game {(paused ? "paused" : "unpaused")}");

            OnPauseStateChanged?.Invoke(paused);
        }

        /// <summary>
        /// Toggle pause state
        /// </summary>
        public void TogglePause()
        {
            SetPaused(!IsPaused);
        }
        #endregion

        #region Battle Management
        /// <summary>
        /// Start a new battle
        /// </summary>
        public void StartBattle()
        {
            Debug.Log("[GameManager] Starting battle...");
            ChangeGameState(GameState.Battle);
        }

        /// <summary>
        /// End the current battle
        /// </summary>
        /// <param name="playerWon">Did the player win?</param>
        public void EndBattle(bool playerWon)
        {
            Debug.Log($"[GameManager] Battle ended. Player {(playerWon ? "won" : "lost")}");
            
            // TODO: Show battle results UI
            // TODO: Award rewards if player won
            // TODO: Update campaign progress
            
            ChangeGameState(GameState.MainMenu);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Quit the application
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[GameManager] Quitting game...");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        #endregion
    }
}