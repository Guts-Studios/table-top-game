using UnityEngine;
using UnityEngine.Events;

namespace Warslammer.Core
{
    /// <summary>
    /// Manages the phases within a player's turn (Move, Action, End)
    /// </summary>
    public class PhaseManager : MonoBehaviour
    {
        #region Singleton
        private static PhaseManager _instance;
        
        /// <summary>
        /// Global access point for the PhaseManager
        /// </summary>
        public static PhaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PhaseManager>();
                }
                return _instance;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Fired when a phase changes
        /// </summary>
        public UnityEvent<GamePhase> OnPhaseChanged = new UnityEvent<GamePhase>();
        
        /// <summary>
        /// Fired when all phases are complete
        /// </summary>
        public UnityEvent OnAllPhasesComplete = new UnityEvent();
        #endregion

        #region Properties
        [Header("Phase State")]
        [SerializeField]
        [Tooltip("Current phase")]
        private GamePhase _currentPhase = GamePhase.TurnStart;
        
        /// <summary>
        /// Current phase of the turn
        /// </summary>
        public GamePhase CurrentPhase => _currentPhase;

        /// <summary>
        /// Is the phase system currently active?
        /// </summary>
        public bool IsActive { get; private set; }

        [Header("Phase Configuration")]
        [SerializeField]
        [Tooltip("Automatically advance through phases")]
        private bool _autoAdvancePhases = false;
        
        /// <summary>
        /// Should phases automatically advance?
        /// </summary>
        public bool AutoAdvancePhases
        {
            get => _autoAdvancePhases;
            set => _autoAdvancePhases = value;
        }

        [SerializeField]
        [Tooltip("Delay between auto-advancing phases")]
        private float _phaseAdvanceDelay = 1f;
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
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        #endregion

        #region Phase Management
        /// <summary>
        /// Start the phase sequence for a turn
        /// </summary>
        public void StartPhaseSequence()
        {
            Debug.Log("[PhaseManager] Starting phase sequence");
            
            IsActive = true;
            _currentPhase = GamePhase.TurnStart;
            
            EnterPhase(GamePhase.TurnStart);
        }

        /// <summary>
        /// Advance to the next phase
        /// </summary>
        public void NextPhase()
        {
            if (!IsActive)
            {
                Debug.LogWarning("[PhaseManager] Cannot advance phase - phase system is not active");
                return;
            }

            GamePhase nextPhase = GetNextPhase(_currentPhase);
            
            if (nextPhase == GamePhase.TurnStart)
            {
                // We've completed all phases
                CompletePhaseSequence();
            }
            else
            {
                EnterPhase(nextPhase);
            }
        }

        /// <summary>
        /// Enter a specific phase
        /// </summary>
        private void EnterPhase(GamePhase phase)
        {
            GamePhase previousPhase = _currentPhase;
            _currentPhase = phase;

            Debug.Log($"[PhaseManager] Phase changed: {previousPhase} -> {phase}");

            // Exit previous phase
            ExitPhase(previousPhase);

            // Enter new phase
            OnPhaseEnter(phase);

            // Notify listeners
            OnPhaseChanged?.Invoke(phase);

            // Auto-advance if enabled
            if (_autoAdvancePhases && phase != GamePhase.Movement && phase != GamePhase.Action)
            {
                Invoke(nameof(NextPhase), _phaseAdvanceDelay);
            }
        }

        /// <summary>
        /// Called when exiting a phase
        /// </summary>
        private void ExitPhase(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.TurnStart:
                    // TODO: Clean up turn start effects
                    break;
                case GamePhase.Movement:
                    // TODO: Finalize movement
                    break;
                case GamePhase.Action:
                    // TODO: Finalize actions
                    break;
                case GamePhase.TurnEnd:
                    // TODO: Clean up turn end effects
                    break;
            }
        }

        /// <summary>
        /// Called when entering a phase
        /// </summary>
        private void OnPhaseEnter(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.TurnStart:
                    HandleTurnStartPhase();
                    break;
                case GamePhase.Movement:
                    HandleMovementPhase();
                    break;
                case GamePhase.Action:
                    HandleActionPhase();
                    break;
                case GamePhase.TurnEnd:
                    HandleTurnEndPhase();
                    break;
            }
        }

        /// <summary>
        /// Complete the phase sequence
        /// </summary>
        private void CompletePhaseSequence()
        {
            Debug.Log("[PhaseManager] Phase sequence complete");
            
            IsActive = false;
            OnAllPhasesComplete?.Invoke();
        }
        #endregion

        #region Phase Handlers
        /// <summary>
        /// Handle Turn Start phase logic
        /// </summary>
        private void HandleTurnStartPhase()
        {
            Debug.Log("[PhaseManager] Turn Start Phase");
            
            // TODO: Reset unit states
            // TODO: Apply start-of-turn effects (abilities, terrain, etc.)
            // TODO: Check for victory conditions
            
            // Automatically advance to movement phase
            if (_autoAdvancePhases)
            {
                Invoke(nameof(NextPhase), _phaseAdvanceDelay);
            }
        }

        /// <summary>
        /// Handle Movement phase logic
        /// </summary>
        private void HandleMovementPhase()
        {
            Debug.Log("[PhaseManager] Movement Phase");
            
            // TODO: Enable unit movement
            // TODO: Show movement ranges
            // TODO: Handle movement input
            
            // Note: This phase typically waits for player confirmation before advancing
        }

        /// <summary>
        /// Handle Action phase logic
        /// </summary>
        private void HandleActionPhase()
        {
            Debug.Log("[PhaseManager] Action Phase");
            
            // TODO: Enable unit actions (attacks, abilities)
            // TODO: Handle action input
            // TODO: Resolve combat
            
            // Note: This phase typically waits for player confirmation before advancing
        }

        /// <summary>
        /// Handle Turn End phase logic
        /// </summary>
        private void HandleTurnEndPhase()
        {
            Debug.Log("[PhaseManager] Turn End Phase");
            
            // TODO: Apply end-of-turn effects
            // TODO: Process status effects (poison, regeneration, etc.)
            // TODO: Update cooldowns
            
            // Automatically complete the sequence
            if (_autoAdvancePhases)
            {
                Invoke(nameof(CompletePhaseSequence), _phaseAdvanceDelay);
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get the next phase in the sequence
        /// </summary>
        private GamePhase GetNextPhase(GamePhase currentPhase)
        {
            switch (currentPhase)
            {
                case GamePhase.TurnStart:
                    return GamePhase.Movement;
                case GamePhase.Movement:
                    return GamePhase.Action;
                case GamePhase.Action:
                    return GamePhase.TurnEnd;
                case GamePhase.TurnEnd:
                    return GamePhase.TurnStart; // Signals completion
                default:
                    return GamePhase.TurnStart;
            }
        }

        /// <summary>
        /// Check if we're in a specific phase
        /// </summary>
        public bool IsInPhase(GamePhase phase)
        {
            return _currentPhase == phase && IsActive;
        }

        /// <summary>
        /// Force skip to a specific phase
        /// </summary>
        public void SkipToPhase(GamePhase phase)
        {
            if (!IsActive)
            {
                Debug.LogWarning("[PhaseManager] Cannot skip to phase - phase system is not active");
                return;
            }

            Debug.Log($"[PhaseManager] Skipping to phase: {phase}");
            EnterPhase(phase);
        }

        /// <summary>
        /// Reset the phase manager
        /// </summary>
        public void Reset()
        {
            IsActive = false;
            _currentPhase = GamePhase.TurnStart;
            CancelInvoke();
        }
        #endregion
    }
}