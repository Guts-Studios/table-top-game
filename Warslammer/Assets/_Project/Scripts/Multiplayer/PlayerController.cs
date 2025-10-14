using System.Collections.Generic;
using UnityEngine;
using Warslammer.Core;
using Warslammer.Units;

namespace Warslammer.Multiplayer
{
    /// <summary>
    /// Represents a player (human or AI)
    /// Owns units and tracks player state
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region Properties
        [Header("Player Configuration")]
        [SerializeField]
        [Tooltip("Player index (0 or 1)")]
        private int _playerIndex = 0;
        
        /// <summary>
        /// Player index
        /// </summary>
        public int PlayerIndex => _playerIndex;

        [SerializeField]
        [Tooltip("Player type (Human, AI, or Network)")]
        private PlayerType _playerType = PlayerType.Human;
        
        /// <summary>
        /// Player type
        /// </summary>
        public PlayerType PlayerType => _playerType;

        [SerializeField]
        [Tooltip("Player name")]
        private string _playerName = "Player";
        
        /// <summary>
        /// Player name
        /// </summary>
        public string PlayerName => _playerName;

        [SerializeField]
        [Tooltip("Player color for UI and highlights")]
        private Color _playerColor = Color.blue;
        
        /// <summary>
        /// Player color
        /// </summary>
        public Color PlayerColor => _playerColor;

        [Header("Army")]
        [SerializeField]
        [Tooltip("List of units owned by this player")]
        private List<Unit> _ownedUnits = new List<Unit>();
        
        /// <summary>
        /// List of units owned by this player
        /// </summary>
        public List<Unit> OwnedUnits => _ownedUnits;

        [SerializeField]
        [Tooltip("Total army points value")]
        private int _armyPoints = 0;
        
        /// <summary>
        /// Total army points value
        /// </summary>
        public int ArmyPoints => _armyPoints;

        /// <summary>
        /// Is this player the active player?
        /// </summary>
        public bool IsActivePlayer
        {
            get
            {
                TurnManager turnManager = TurnManager.Instance;
                return turnManager != null && turnManager.ActivePlayerIndex == _playerIndex;
            }
        }

        /// <summary>
        /// Number of units alive
        /// </summary>
        public int UnitsAlive
        {
            get
            {
                int count = 0;
                foreach (Unit unit in _ownedUnits)
                {
                    if (unit != null && unit.IsAlive)
                        count++;
                }
                return count;
            }
        }
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Subscribe to turn events
            TurnManager turnManager = TurnManager.Instance;
            if (turnManager != null)
            {
                turnManager.OnTurnStart.AddListener(OnTurnStart);
                turnManager.OnTurnEnd.AddListener(OnTurnEnd);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from turn events
            TurnManager turnManager = TurnManager.Instance;
            if (turnManager != null)
            {
                turnManager.OnTurnStart.RemoveListener(OnTurnStart);
                turnManager.OnTurnEnd.RemoveListener(OnTurnEnd);
            }
        }
        #endregion

        #region Unit Management
        /// <summary>
        /// Add a unit to this player's army
        /// </summary>
        /// <param name="unit">Unit to add</param>
        public void AddUnit(Unit unit)
        {
            if (unit == null)
                return;

            if (!_ownedUnits.Contains(unit))
            {
                _ownedUnits.Add(unit);
                unit.OwnerPlayerIndex = _playerIndex;

                // Update army points
                if (unit.UnitData != null)
                {
                    _armyPoints += unit.UnitData.GetTotalPointsCost();
                }

                Debug.Log($"[PlayerController] {_playerName} added unit {unit.name} (Total: {UnitsAlive} units, {_armyPoints} points)");
            }
        }

        /// <summary>
        /// Remove a unit from this player's army
        /// </summary>
        /// <param name="unit">Unit to remove</param>
        public void RemoveUnit(Unit unit)
        {
            if (unit == null)
                return;

            if (_ownedUnits.Contains(unit))
            {
                _ownedUnits.Remove(unit);

                // Update army points
                if (unit.UnitData != null)
                {
                    _armyPoints -= unit.UnitData.GetTotalPointsCost();
                }

                Debug.Log($"[PlayerController] {_playerName} removed unit {unit.name} (Total: {UnitsAlive} units, {_armyPoints} points)");
            }
        }

        /// <summary>
        /// Clear all units from this player's army
        /// </summary>
        public void ClearUnits()
        {
            _ownedUnits.Clear();
            _armyPoints = 0;
        }

        /// <summary>
        /// Get all alive units
        /// </summary>
        public List<Unit> GetAliveUnits()
        {
            List<Unit> aliveUnits = new List<Unit>();
            
            foreach (Unit unit in _ownedUnits)
            {
                if (unit != null && unit.IsAlive)
                {
                    aliveUnits.Add(unit);
                }
            }

            return aliveUnits;
        }
        #endregion

        #region Turn Management
        /// <summary>
        /// Called when any turn starts
        /// </summary>
        private void OnTurnStart(int turnNumber)
        {
            if (!IsActivePlayer)
                return;

            Debug.Log($"[PlayerController] {_playerName}'s turn started (Turn {turnNumber})");

            // Reset all units for the new turn
            foreach (Unit unit in _ownedUnits)
            {
                if (unit != null && unit.IsAlive)
                {
                    unit.OnTurnStart();
                }
            }

            // TODO: Phase 3 - AI logic for AI players
            if (_playerType == PlayerType.AI)
            {
                // AI would make decisions here
            }
        }

        /// <summary>
        /// Called when any turn ends
        /// </summary>
        private void OnTurnEnd(int turnNumber)
        {
            if (!IsActivePlayer)
                return;

            Debug.Log($"[PlayerController] {_playerName}'s turn ended");

            // Process end of turn for all units
            foreach (Unit unit in _ownedUnits)
            {
                if (unit != null && unit.IsAlive)
                {
                    unit.OnTurnEnd();
                }
            }
        }
        #endregion

        #region Victory Conditions
        /// <summary>
        /// Check if this player has been defeated
        /// </summary>
        public bool IsDefeated()
        {
            return UnitsAlive == 0;
        }

        /// <summary>
        /// Get victory points (for objective-based games)
        /// </summary>
        public int GetVictoryPoints()
        {
            // TODO: Phase 3 - Implement victory point system
            return 0;
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set player configuration
        /// </summary>
        public void Initialize(int playerIndex, PlayerType playerType, string playerName, Color playerColor)
        {
            _playerIndex = playerIndex;
            _playerType = playerType;
            _playerName = playerName;
            _playerColor = playerColor;

            Debug.Log($"[PlayerController] Initialized {_playerName} (Index: {_playerIndex}, Type: {_playerType})");
        }
        #endregion
    }
}