using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Warslammer.Core;

namespace Warslammer.Combat
{
    /// <summary>
    /// Manages a player's combat cards
    /// Players have a fixed set of 10 cards (not drawn randomly):
    /// - 3 Attack cards (+1, +2, +3)
    /// - 3 Defense cards (+1, +2, +3)
    /// - 4 Faction-specific cards
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        #region Events
        /// <summary>
        /// Fired when a card is played
        /// </summary>
        public UnityEvent<CardData> OnCardPlayed = new UnityEvent<CardData>();

        /// <summary>
        /// Fired when card play is requested (for UI)
        /// </summary>
        public UnityEvent<List<CardData>> OnRequestCardSelection = new UnityEvent<List<CardData>>();

        /// <summary>
        /// Fired when cards are refreshed
        /// </summary>
        public UnityEvent OnCardsRefreshed = new UnityEvent();
        #endregion

        #region Serialized Fields
        [Header("Player Configuration")]
        [Tooltip("Player index this card manager belongs to")]
        [SerializeField]
        private int _playerIndex = 0;

        [Tooltip("Faction for this player (determines faction cards)")]
        [SerializeField]
        private FactionType _faction = FactionType.None;

        [Header("Standard Cards")]
        [Tooltip("Standard attack cards (+1, +2, +3) - Everyone has these")]
        [SerializeField]
        private List<CardData> _standardAttackCards = new List<CardData>();

        [Tooltip("Standard defense cards (+1, +2, +3) - Everyone has these")]
        [SerializeField]
        private List<CardData> _standardDefenseCards = new List<CardData>();

        [Header("Faction Cards")]
        [Tooltip("Faction-specific cards (4 cards)")]
        [SerializeField]
        private List<CardData> _factionCards = new List<CardData>();

        [Header("Card State")]
        [Tooltip("Cards that have been used this turn")]
        [SerializeField]
        private List<CardData> _usedCards = new List<CardData>();

        [Tooltip("Cards that have been used this battle (one-use cards)")]
        [SerializeField]
        private List<CardData> _usedThisBattle = new List<CardData>();
        #endregion

        #region Properties
        /// <summary>
        /// Player index
        /// </summary>
        public int PlayerIndex => _playerIndex;

        /// <summary>
        /// Player faction
        /// </summary>
        public FactionType Faction => _faction;

        /// <summary>
        /// All cards available to this player (10 total)
        /// </summary>
        public List<CardData> AllCards
        {
            get
            {
                List<CardData> all = new List<CardData>();
                all.AddRange(_standardAttackCards);
                all.AddRange(_standardDefenseCards);
                all.AddRange(_factionCards);
                return all;
            }
        }

        /// <summary>
        /// Cards available to play right now
        /// </summary>
        public List<CardData> AvailableCards
        {
            get
            {
                List<CardData> available = new List<CardData>();

                foreach (var card in AllCards)
                {
                    if (CanPlayCard(card))
                    {
                        available.Add(card);
                    }
                }

                return available;
            }
        }

        /// <summary>
        /// Cards used this turn
        /// </summary>
        public List<CardData> UsedCards => new List<CardData>(_usedCards);

        /// <summary>
        /// Cards used this battle (one-use only)
        /// </summary>
        public List<CardData> UsedThisBattle => new List<CardData>(_usedThisBattle);
        #endregion

        #region Initialization
        private void Awake()
        {
            ValidateCardSetup();
        }

        /// <summary>
        /// Validate that card setup is correct
        /// </summary>
        private void ValidateCardSetup()
        {
            if (_standardAttackCards.Count != 3)
            {
                Debug.LogWarning($"[CardManager] Player {_playerIndex} should have 3 attack cards, has {_standardAttackCards.Count}");
            }

            if (_standardDefenseCards.Count != 3)
            {
                Debug.LogWarning($"[CardManager] Player {_playerIndex} should have 3 defense cards, has {_standardDefenseCards.Count}");
            }

            if (_factionCards.Count != 4)
            {
                Debug.LogWarning($"[CardManager] Player {_playerIndex} should have 4 faction cards, has {_factionCards.Count}");
            }

            int totalCards = AllCards.Count;
            if (totalCards != 10)
            {
                Debug.LogWarning($"[CardManager] Player {_playerIndex} should have 10 cards total, has {totalCards}");
            }
        }

        /// <summary>
        /// Set up cards for this player
        /// </summary>
        public void InitializeCards(int playerIndex, FactionType faction,
                                    List<CardData> attackCards,
                                    List<CardData> defenseCards,
                                    List<CardData> factionCards)
        {
            _playerIndex = playerIndex;
            _faction = faction;
            _standardAttackCards = new List<CardData>(attackCards);
            _standardDefenseCards = new List<CardData>(defenseCards);
            _factionCards = new List<CardData>(factionCards);

            _usedCards.Clear();
            _usedThisBattle.Clear();

            ValidateCardSetup();
            Debug.Log($"[CardManager] Player {_playerIndex} ({_faction}) initialized with 10 cards");
        }
        #endregion

        #region Card Playing
        /// <summary>
        /// Check if a card can be played
        /// </summary>
        public bool CanPlayCard(CardData card)
        {
            if (card == null)
                return false;

            // Check if already used this turn
            if (card.oneUsePerTurn && _usedCards.Contains(card))
                return false;

            // Check if already used this battle
            if (card.oneUsePerBattle && _usedThisBattle.Contains(card))
                return false;

            // Check faction
            if (card.faction != FactionType.None && card.faction != _faction)
                return false;

            return true;
        }

        /// <summary>
        /// Play a card
        /// </summary>
        public bool PlayCard(CardData card)
        {
            if (!CanPlayCard(card))
            {
                Debug.LogWarning($"[CardManager] Cannot play {card.cardName}!");
                return false;
            }

            // Mark as used
            _usedCards.Add(card);

            if (card.oneUsePerBattle)
            {
                _usedThisBattle.Add(card);
            }

            // Fire event
            OnCardPlayed?.Invoke(card);

            Debug.Log($"[CardManager] Player {_playerIndex} played: {card.cardName} ({card.GetEffectSummary()})");

            // Play VFX/sound
            if (card.playVFXPrefab != null)
            {
                GameObject.Instantiate(card.playVFXPrefab, transform.position, Quaternion.identity);
            }

            if (card.playSound != null)
            {
                // TODO: Integrate with audio system when implemented
                Debug.Log($"[CardManager] Would play sound: {card.playSound.name}");
            }

            return true;
        }

        /// <summary>
        /// Request player to select a card (triggers UI)
        /// </summary>
        public void RequestCardSelection(CardTiming timing)
        {
            List<CardData> playableCards = new List<CardData>();

            foreach (var card in AllCards)
            {
                if (CanPlayCard(card) && card.CanBePlayed(timing, _faction))
                {
                    playableCards.Add(card);
                }
            }

            Debug.Log($"[CardManager] Player {_playerIndex} can play {playableCards.Count} cards at timing: {timing}");

            OnRequestCardSelection?.Invoke(playableCards);
        }
        #endregion

        #region Card Queries
        /// <summary>
        /// Get all attack cards
        /// </summary>
        public List<CardData> GetAttackCards()
        {
            return new List<CardData>(_standardAttackCards);
        }

        /// <summary>
        /// Get all defense cards
        /// </summary>
        public List<CardData> GetDefenseCards()
        {
            return new List<CardData>(_standardDefenseCards);
        }

        /// <summary>
        /// Get all faction cards
        /// </summary>
        public List<CardData> GetFactionCards()
        {
            return new List<CardData>(_factionCards);
        }

        /// <summary>
        /// Get card by ID
        /// </summary>
        public CardData GetCardByID(string cardID)
        {
            return AllCards.Find(c => c != null && c.cardID == cardID);
        }

        /// <summary>
        /// Check if card has been used this turn
        /// </summary>
        public bool HasUsedCard(CardData card)
        {
            return _usedCards.Contains(card);
        }

        /// <summary>
        /// Check if card has been used this battle
        /// </summary>
        public bool HasUsedCardThisBattle(CardData card)
        {
            return _usedThisBattle.Contains(card);
        }
        #endregion

        #region Turn Management
        /// <summary>
        /// Reset cards for new turn
        /// </summary>
        public void OnTurnStart()
        {
            // Clear used cards (but not one-use-per-battle cards)
            _usedCards.Clear();

            OnCardsRefreshed?.Invoke();

            Debug.Log($"[CardManager] Player {_playerIndex} cards refreshed for new turn");
        }

        /// <summary>
        /// Called at end of turn
        /// </summary>
        public void OnTurnEnd()
        {
            // Nothing to do for now
        }

        /// <summary>
        /// Reset all cards for new battle
        /// </summary>
        public void OnBattleStart()
        {
            _usedCards.Clear();
            _usedThisBattle.Clear();

            OnCardsRefreshed?.Invoke();

            Debug.Log($"[CardManager] Player {_playerIndex} cards reset for new battle");
        }
        #endregion

        #region Debug
        /// <summary>
        /// Display all cards in console
        /// </summary>
        public void DisplayCards()
        {
            Debug.Log($"=== PLAYER {_playerIndex} CARDS ({_faction}) ===");

            Debug.Log("ATTACK CARDS:");
            foreach (var card in _standardAttackCards)
            {
                string used = HasUsedCard(card) ? " [USED]" : "";
                Debug.Log($"  - {card.cardName}: {card.GetEffectSummary()}{used}");
            }

            Debug.Log("DEFENSE CARDS:");
            foreach (var card in _standardDefenseCards)
            {
                string used = HasUsedCard(card) ? " [USED]" : "";
                Debug.Log($"  - {card.cardName}: {card.GetEffectSummary()}{used}");
            }

            Debug.Log("FACTION CARDS:");
            foreach (var card in _factionCards)
            {
                string used = HasUsedCard(card) ? " [USED]" : "";
                string battleUsed = HasUsedCardThisBattle(card) ? " [USED THIS BATTLE]" : "";
                Debug.Log($"  - {card.cardName}: {card.GetEffectSummary()}{used}{battleUsed}");
            }

            Debug.Log($"Available cards: {AvailableCards.Count}/10");
            Debug.Log("=============================");
        }
        #endregion
    }
}
