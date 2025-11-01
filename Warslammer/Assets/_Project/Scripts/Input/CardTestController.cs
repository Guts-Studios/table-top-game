using UnityEngine;
using Warslammer.Units;
using Warslammer.Combat;

namespace Warslammer.Input
{
    /// <summary>
    /// Simple test controller for testing combat cards
    /// Press Q/W/E for attack cards, A/S/D for defense cards
    /// </summary>
    public class CardTestController : MonoBehaviour
    {
        [Header("Test Setup")]
        [Tooltip("Attacker unit (player 0)")]
        public Unit attacker;

        [Tooltip("Defender unit (player 1)")]
        public Unit defender;

        [Tooltip("Card manager for attacker")]
        public CardManager attackerCardManager;

        [Tooltip("Card manager for defender")]
        public CardManager defenderCardManager;

        [Header("Card Integration")]
        public CombatCardIntegration cardIntegration;

        private void Start()
        {
            if (attacker == null || defender == null)
            {
                Debug.LogError("[CardTestController] Assign attacker and defender units!");
                enabled = false;
                return;
            }

            if (attackerCardManager == null || defenderCardManager == null)
            {
                Debug.LogError("[CardTestController] Assign card managers!");
                enabled = false;
                return;
            }

            if (cardIntegration == null)
            {
                cardIntegration = CombatCardIntegration.Instance;
            }

            Debug.Log("[CardTestController] Ready!");
            Debug.Log("ATTACKER (Player 0) - Q/W/E: Attack cards, A/S/D: Defense cards");
            Debug.Log("DEFENDER (Player 1) - I/O/P: Attack cards, J/K/L: Defense cards");
            Debug.Log("SPACEBAR: Trigger combat");
            Debug.Log("H: Show all cards");
            Debug.Log("R: Reset cards");
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;

            // Attacker attack cards (Q/W/E = +1/+2/+3)
            if (keyboard.qKey.wasPressedThisFrame)
                PlayAttackCard(attacker, 0);
            if (keyboard.wKey.wasPressedThisFrame)
                PlayAttackCard(attacker, 1);
            if (keyboard.eKey.wasPressedThisFrame)
                PlayAttackCard(attacker, 2);

            // Attacker defense cards (A/S/D = +1/+2/+3)
            if (keyboard.aKey.wasPressedThisFrame)
                PlayDefenseCard(attacker, 0);
            if (keyboard.sKey.wasPressedThisFrame)
                PlayDefenseCard(attacker, 1);
            if (keyboard.dKey.wasPressedThisFrame)
                PlayDefenseCard(attacker, 2);

            // Defender attack cards (I/O/P = +1/+2/+3)
            if (keyboard.iKey.wasPressedThisFrame)
                PlayAttackCard(defender, 0);
            if (keyboard.oKey.wasPressedThisFrame)
                PlayAttackCard(defender, 1);
            if (keyboard.pKey.wasPressedThisFrame)
                PlayAttackCard(defender, 2);

            // Defender defense cards (J/K/L = +1/+2/+3)
            if (keyboard.jKey.wasPressedThisFrame)
                PlayDefenseCard(defender, 0);
            if (keyboard.kKey.wasPressedThisFrame)
                PlayDefenseCard(defender, 1);
            if (keyboard.lKey.wasPressedThisFrame)
                PlayDefenseCard(defender, 2);

            // Trigger combat
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                TriggerCombat();
            }

            // Show cards
            if (keyboard.hKey.wasPressedThisFrame)
            {
                ShowAllCards();
            }

            // Reset cards
            if (keyboard.rKey.wasPressedThisFrame)
            {
                ResetCards();
            }
        }

        private void PlayAttackCard(Unit unit, int cardIndex)
        {
            var cardManager = unit == attacker ? attackerCardManager : defenderCardManager;
            var attackCards = cardManager.GetAttackCards();

            if (cardIndex >= attackCards.Count)
            {
                Debug.LogWarning($"[CardTestController] Attack card {cardIndex} doesn't exist!");
                return;
            }

            CardData card = attackCards[cardIndex];
            bool success = cardIntegration.PlayAttackCard(unit, card);

            if (success)
            {
                Debug.Log($"<color=green>[CardTestController] {unit.name} played: {card.cardName} (+{card.attackBonus} ATK)</color>");
            }
            else
            {
                Debug.LogWarning($"<color=red>[CardTestController] Failed to play {card.cardName}</color>");
            }
        }

        private void PlayDefenseCard(Unit unit, int cardIndex)
        {
            var cardManager = unit == attacker ? attackerCardManager : defenderCardManager;
            var defenseCards = cardManager.GetDefenseCards();

            if (cardIndex >= defenseCards.Count)
            {
                Debug.LogWarning($"[CardTestController] Defense card {cardIndex} doesn't exist!");
                return;
            }

            CardData card = defenseCards[cardIndex];
            bool success = cardIntegration.PlayDefenseCard(unit, card);

            if (success)
            {
                Debug.Log($"<color=cyan>[CardTestController] {unit.name} played: {card.cardName} (+{card.defenseBonus} DEF)</color>");
            }
            else
            {
                Debug.LogWarning($"<color=red>[CardTestController] Failed to play {card.cardName}</color>");
            }
        }

        private void TriggerCombat()
        {
            Debug.Log("=== COMBAT START ===");

            var attackerCombat = attacker.GetComponent<UnitCombat>();
            if (attackerCombat == null)
            {
                Debug.LogError("[CardTestController] Attacker has no UnitCombat component!");
                return;
            }

            // Attack using cards
            CombatResult result = attackerCombat.Attack(defender);

            if (result != null)
            {
                Debug.Log($"<color=yellow>{result.GetCombatLogString()}</color>");
            }

            // Clear played cards after combat
            cardIntegration.ClearPlayedCards();

            Debug.Log("=== COMBAT END ===");
        }

        private void ShowAllCards()
        {
            Debug.Log("=== SHOWING ALL CARDS ===");
            attackerCardManager.DisplayCards();
            defenderCardManager.DisplayCards();
        }

        private void ResetCards()
        {
            attackerCardManager.OnTurnStart();
            defenderCardManager.OnTurnStart();
            cardIntegration.ClearPlayedCards();
            Debug.Log("[CardTestController] All cards reset!");
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.yellow;

            string text = "CARD TEST CONTROLLER\n\n" +
                         "ATTACKER (Red):\n" +
                         "Q/W/E: Attack +1/+2/+3\n" +
                         "A/S/D: Defense +1/+2/+3\n\n" +
                         "DEFENDER (Blue):\n" +
                         "I/O/P: Attack +1/+2/+3\n" +
                         "J/K/L: Defense +1/+2/+3\n\n" +
                         "SPACEBAR: Fight!\n" +
                         "H: Show Cards\n" +
                         "R: Reset Cards";

            GUI.Label(new Rect(10, 300, 300, 400), text, style);
        }
    }
}
