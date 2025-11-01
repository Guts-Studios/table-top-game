using UnityEngine;
using Warslammer.Units;

namespace Warslammer.Combat
{
    /// <summary>
    /// Integrates card system into combat resolution
    /// Allows players to play cards during attack/defense phases
    /// </summary>
    public class CombatCardIntegration : MonoBehaviour
    {
        #region Singleton
        private static CombatCardIntegration _instance;
        public static CombatCardIntegration Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CombatCardIntegration>();
                }
                return _instance;
            }
        }
        #endregion

        #region Fields
        [Header("Card Managers")]
        [Tooltip("Card manager for player 1")]
        [SerializeField]
        private CardManager _player1CardManager;

        [Tooltip("Card manager for player 2")]
        [SerializeField]
        private CardManager _player2CardManager;

        [Header("Card State")]
        private CardData _lastAttackCardPlayed;
        private CardData _lastDefenseCardPlayed;
        #endregion

        #region Properties
        public CardManager Player1Cards => _player1CardManager;
        public CardManager Player2Cards => _player2CardManager;
        #endregion

        #region Initialization
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Card Manager Access
        /// <summary>
        /// Get card manager for a player
        /// </summary>
        public CardManager GetCardManager(int playerIndex)
        {
            if (playerIndex == 0)
                return _player1CardManager;
            else if (playerIndex == 1)
                return _player2CardManager;

            return null;
        }

        /// <summary>
        /// Get card manager for a unit
        /// </summary>
        public CardManager GetCardManagerForUnit(Unit unit)
        {
            if (unit == null)
                return null;

            return GetCardManager(unit.OwnerPlayerIndex);
        }
        #endregion

        #region Combat Card Hooks
        /// <summary>
        /// Apply attack card modifiers to dice pool
        /// </summary>
        public void ApplyAttackCardModifiers(Unit attacker, DicePool attackPool)
        {
            if (_lastAttackCardPlayed == null)
                return;

            // Apply attack bonus
            if (_lastAttackCardPlayed.attackBonus > 0)
            {
                attackPool.AddModifier(_lastAttackCardPlayed.attackBonus);
                Debug.Log($"[CombatCardIntegration] Applied +{_lastAttackCardPlayed.attackBonus} attack from {_lastAttackCardPlayed.cardName}");
            }

            // Apply bonus dice
            if (_lastAttackCardPlayed.bonusDice > 0)
            {
                attackPool.AddDice(_lastAttackCardPlayed.bonusDice);
                Debug.Log($"[CombatCardIntegration] Applied +{_lastAttackCardPlayed.bonusDice} dice from {_lastAttackCardPlayed.cardName}");
            }
        }

        /// <summary>
        /// Apply defense card modifiers to dice pool
        /// </summary>
        public void ApplyDefenseCardModifiers(Unit defender, DicePool defensePool)
        {
            if (_lastDefenseCardPlayed == null)
                return;

            // Apply defense bonus
            if (_lastDefenseCardPlayed.defenseBonus > 0)
            {
                defensePool.AddModifier(_lastDefenseCardPlayed.defenseBonus);
                Debug.Log($"[CombatCardIntegration] Applied +{_lastDefenseCardPlayed.defenseBonus} defense from {_lastDefenseCardPlayed.cardName}");
            }

            // Apply bonus dice
            if (_lastDefenseCardPlayed.bonusDice > 0)
            {
                defensePool.AddDice(_lastDefenseCardPlayed.bonusDice);
                Debug.Log($"[CombatCardIntegration] Applied +{_lastDefenseCardPlayed.bonusDice} dice from {_lastDefenseCardPlayed.cardName}");
            }
        }

        /// <summary>
        /// Apply attack card modifiers after rolling
        /// </summary>
        public void ApplyAttackCardPostRoll(DiceRollResult attackRoll)
        {
            if (_lastAttackCardPlayed == null)
                return;

            // Reroll ones
            if (_lastAttackCardPlayed.rerollOnes)
            {
                Debug.Log($"[CombatCardIntegration] Rerolling 1s from {_lastAttackCardPlayed.cardName}");
                // TODO: Implement reroll logic in DiceRoller
            }

            // Reroll failed
            if (_lastAttackCardPlayed.rerollFailed)
            {
                Debug.Log($"[CombatCardIntegration] Rerolling failed dice from {_lastAttackCardPlayed.cardName}");
                // TODO: Implement reroll logic in DiceRoller
            }

            // Auto-successes
            if (_lastAttackCardPlayed.autoSuccessCount > 0)
            {
                Debug.Log($"[CombatCardIntegration] Adding {_lastAttackCardPlayed.autoSuccessCount} auto-successes from {_lastAttackCardPlayed.cardName}");
                // TODO: Add auto-successes to result
            }
        }

        /// <summary>
        /// Apply defense card modifiers after rolling
        /// </summary>
        public void ApplyDefenseCardPostRoll(DiceRollResult defenseRoll)
        {
            if (_lastDefenseCardPlayed == null)
                return;

            // Reroll ones
            if (_lastDefenseCardPlayed.rerollOnes)
            {
                Debug.Log($"[CombatCardIntegration] Rerolling 1s from {_lastDefenseCardPlayed.cardName}");
            }

            // Reroll failed
            if (_lastDefenseCardPlayed.rerollFailed)
            {
                Debug.Log($"[CombatCardIntegration] Rerolling failed dice from {_lastDefenseCardPlayed.cardName}");
            }
        }

        /// <summary>
        /// Apply damage modifiers from cards
        /// </summary>
        public int ApplyDamageModifiers(int baseDamage)
        {
            int finalDamage = baseDamage;

            // Attack card damage modifiers
            if (_lastAttackCardPlayed != null)
            {
                finalDamage += _lastAttackCardPlayed.bonusDamage;
                finalDamage = Mathf.RoundToInt(finalDamage * _lastAttackCardPlayed.damageMultiplier);

                if (finalDamage != baseDamage)
                {
                    Debug.Log($"[CombatCardIntegration] Modified damage: {baseDamage} -> {finalDamage} from {_lastAttackCardPlayed.cardName}");
                }
            }

            // Defense card damage prevention
            if (_lastDefenseCardPlayed != null && _lastDefenseCardPlayed.preventDamage)
            {
                Debug.Log($"[CombatCardIntegration] Prevented all damage from {_lastDefenseCardPlayed.cardName}!");
                return 0;
            }

            return finalDamage;
        }

        /// <summary>
        /// Check if armor should be ignored
        /// </summary>
        public bool ShouldIgnoreArmor()
        {
            return _lastAttackCardPlayed != null && _lastAttackCardPlayed.ignoreArmor;
        }
        #endregion

        #region Card Playing
        /// <summary>
        /// Play an attack card
        /// </summary>
        public bool PlayAttackCard(Unit attacker, CardData card)
        {
            var cardManager = GetCardManagerForUnit(attacker);
            if (cardManager == null)
            {
                Debug.LogWarning("[CombatCardIntegration] No card manager for attacker!");
                return false;
            }

            if (!cardManager.CanPlayCard(card))
            {
                Debug.LogWarning($"[CombatCardIntegration] Cannot play {card.cardName}!");
                return false;
            }

            if (cardManager.PlayCard(card))
            {
                _lastAttackCardPlayed = card;
                Debug.Log($"[CombatCardIntegration] {attacker.GetUnitName()} played attack card: {card.cardName}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Play a defense card
        /// </summary>
        public bool PlayDefenseCard(Unit defender, CardData card)
        {
            var cardManager = GetCardManagerForUnit(defender);
            if (cardManager == null)
            {
                Debug.LogWarning("[CombatCardIntegration] No card manager for defender!");
                return false;
            }

            if (!cardManager.CanPlayCard(card))
            {
                Debug.LogWarning($"[CombatCardIntegration] Cannot play {card.cardName}!");
                return false;
            }

            if (cardManager.PlayCard(card))
            {
                _lastDefenseCardPlayed = card;
                Debug.Log($"[CombatCardIntegration] {defender.GetUnitName()} played defense card: {card.cardName}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clear played cards (call after combat resolves)
        /// </summary>
        public void ClearPlayedCards()
        {
            _lastAttackCardPlayed = null;
            _lastDefenseCardPlayed = null;
        }
        #endregion

        #region Turn Management
        /// <summary>
        /// Reset cards for new turn
        /// </summary>
        public void OnTurnStart(int playerIndex)
        {
            var cardManager = GetCardManager(playerIndex);
            if (cardManager != null)
            {
                cardManager.OnTurnStart();
            }
        }

        /// <summary>
        /// Reset all cards for new battle
        /// </summary>
        public void OnBattleStart()
        {
            _player1CardManager?.OnBattleStart();
            _player2CardManager?.OnBattleStart();
            ClearPlayedCards();
            Debug.Log("[CombatCardIntegration] Cards reset for new battle");
        }
        #endregion
    }
}
