using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Warslammer.Core
{
    /// <summary>
    /// Manages the IGOUGO (I Go You Go) turn structure
    /// Handles turn order, turn progression, and player switching
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        #region Singleton
        private static TurnManager _instance;
        
        /// <summary>
        /// Global access point for the TurnManager
        /// </summary>
        public static TurnManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<TurnManager>();
                }
                return _instance;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Fired when a new turn begins
        /// </summary>
        public UnityEvent<int> OnTurnStart = new UnityEvent<int>();
        
        /// <summary>
        /// Fired when a turn ends
        /// </summary>
        public UnityEvent<int> OnTurnEnd = new UnityEvent<int>();
        
        /// <summary>
        /// Fired when the active player changes
        /// </summary>
        public UnityEvent<int> OnPlayerChanged = new UnityEvent<int>();
        #endregion

        #region Properties
        [Header("Turn State")]
        [SerializeField]
        [Tooltip("Current turn number")]
        private int _currentTurn = 1;
        
        /// <summary>
        /// Current turn number (starts at 1)
        /// </summary>
        public int CurrentTurn => _currentTurn;

        [SerializeField]
        [Tooltip("Index of the currently active player (0 or 1)")]
        private int _activePlayerIndex = 0;
        
        /// <summary>
        /// Index of the currently active player
        /// </summary>
        public int ActivePlayerIndex => _activePlayerIndex;

        [SerializeField]
        [Tooltip("Maximum number of turns (0 for unlimited)")]
        private int _maxTurns = 0;
        
        /// <summary>
        /// Maximum number of turns (0 for unlimited)
        /// </summary>
        public int MaxTurns
        {
            get => _maxTurns;
            set => _maxTurns = value;
        }

        [Header("Player Configuration")]
        [SerializeField]
        [Tooltip("Types of players in the battle")]
        private PlayerType[] _playerTypes = new PlayerType[] { PlayerType.Human, PlayerType.AI };
        
        /// <summary>
        /// Types of players in the battle
        /// </summary>
        public PlayerType[] PlayerTypes => _playerTypes;

        /// <summary>
        /// Is it currently the human player's turn?
        /// </summary>
        public bool IsHumanPlayerTurn => _playerTypes[_activePlayerIndex] == PlayerType.Human;

        /// <summary>
        /// Reference to the PhaseManager
        /// </summary>
        private PhaseManager _phaseManager;

        /// <summary>
        /// Is the battle currently active?
        /// </summary>
        public bool IsBattleActive { get; private set; }
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
            _phaseManager = GetComponent<PhaseManager>();
        }

        private void Start()
        {
            // Subscribe to phase manager events
            if (_phaseManager != null)
            {
                _phaseManager.OnAllPhasesComplete.AddListener(OnAllPhasesComplete);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }

            // Unsubscribe from events
            if (_phaseManager != null)
            {
                _phaseManager.OnAllPhasesComplete.RemoveListener(OnAllPhasesComplete);
            }
        }
        #endregion

        #region Battle Management
        /// <summary>
        /// Initialize and start a new battle
        /// </summary>
        public void InitializeBattle()
        {
            Debug.Log("[TurnManager] Initializing battle...");
            
            _currentTurn = 1;
            _activePlayerIndex = 0;
            IsBattleActive = true;

            // Determine turn order (could be based on initiative, etc.)
            // TODO: Implement initiative-based turn order
            
            StartTurn();
        }

        /// <summary>
        /// End the current battle
        /// </summary>
        public void EndBattle()
        {
            Debug.Log("[TurnManager] Ending battle...");
            
            IsBattleActive = false;
            
            // TODO: Calculate battle results
            // TODO: Award rewards
        }
        #endregion

        #region Turn Management
        /// <summary>
        /// Start a new turn
        /// </summary>
        private void StartTurn()
        {
            Debug.Log($"[TurnManager] Turn {_currentTurn} - Player {_activePlayerIndex + 1} ({_playerTypes[_activePlayerIndex]})");

            OnTurnStart?.Invoke(_currentTurn);
            OnPlayerChanged?.Invoke(_activePlayerIndex);

            // Start the phase sequence
            if (_phaseManager != null)
            {
                _phaseManager.StartPhaseSequence();
            }

            // TODO: Reset unit states for the new turn
            // TODO: Apply start-of-turn effects
            // TODO: Check for victory conditions
        }

        /// <summary>
        /// End the current turn and move to the next
        /// </summary>
        public void EndTurn()
        {
            Debug.Log($"[TurnManager] Ending turn {_currentTurn} for player {_activePlayerIndex + 1}");

            OnTurnEnd?.Invoke(_currentTurn);

            // TODO: Apply end-of-turn effects
            // TODO: Save turn state

            // Switch to next player
            SwitchPlayer();
        }

        /// <summary>
        /// Switch to the next player
        /// </summary>
        private void SwitchPlayer()
        {
            _activePlayerIndex++;

            // If we've cycled through all players, start a new turn
            if (_activePlayerIndex >= _playerTypes.Length)
            {
                _activePlayerIndex = 0;
                _currentTurn++;

                // Check if we've reached the turn limit
                if (_maxTurns > 0 && _currentTurn > _maxTurns)
                {
                    Debug.Log("[TurnManager] Turn limit reached!");
                    EndBattle();
                    return;
                }
            }

            StartTurn();
        }

        /// <summary>
        /// Called when all phases are complete
        /// </summary>
        private void OnAllPhasesComplete()
        {
            Debug.Log("[TurnManager] All phases complete for current turn");
            EndTurn();
        }
        #endregion

        #region Player Management
        /// <summary>
        /// Get the type of the currently active player
        /// </summary>
        public PlayerType GetActivePlayerType()
        {
            return _playerTypes[_activePlayerIndex];
        }

        /// <summary>
        /// Get the type of a specific player
        /// </summary>
        public PlayerType GetPlayerType(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= _playerTypes.Length)
                return PlayerType.Human;
            
            return _playerTypes[playerIndex];
        }

        /// <summary>
        /// Set the player types for the battle
        /// </summary>
        public void SetPlayerTypes(PlayerType[] playerTypes)
        {
            _playerTypes = playerTypes;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get the number of players in the battle
        /// </summary>
        public int GetPlayerCount()
        {
            return _playerTypes.Length;
        }

        /// <summary>
        /// Check if this is the final turn
        /// </summary>
        public bool IsFinalTurn()
        {
            return _maxTurns > 0 && _currentTurn >= _maxTurns;
        }

        /// <summary>
        /// Get remaining turns
        /// </summary>
        public int GetRemainingTurns()
        {
            if (_maxTurns <= 0)
                return -1; // Unlimited
            
            return Mathf.Max(0, _maxTurns - _currentTurn);
        }
        #endregion
    }
}