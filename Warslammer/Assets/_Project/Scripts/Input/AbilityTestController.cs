using UnityEngine;
using Warslammer.Units;
using Warslammer.Data;

namespace Warslammer.Input
{
    /// <summary>
    /// Simple test controller for testing abilities
    /// Press number keys 1-6 to use abilities
    /// </summary>
    public class AbilityTestController : MonoBehaviour
    {
        [Header("Test Setup")]
        [Tooltip("Unit that will cast abilities")]
        public Unit caster;

        [Tooltip("Target unit for abilities")]
        public Unit target;

        private AbilityController _abilityController;
        private bool _targetSelf = false;

        private void Start()
        {
            if (caster == null)
            {
                Debug.LogError("[AbilityTestController] No caster assigned! Assign a unit in the Inspector.");
                enabled = false;
                return;
            }

            _abilityController = caster.GetComponent<AbilityController>();
            if (_abilityController == null)
            {
                Debug.LogError("[AbilityTestController] Caster has no AbilityController component!");
                enabled = false;
                return;
            }

            Debug.Log("[AbilityTestController] Ready! Press 1-6 to use abilities, T to toggle target, R to reset action points.");
            DisplayAbilities();
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;

            // Press 1-6 to use abilities
            if (keyboard.digit1Key.wasPressedThisFrame) UseAbility(0);
            if (keyboard.digit2Key.wasPressedThisFrame) UseAbility(1);
            if (keyboard.digit3Key.wasPressedThisFrame) UseAbility(2);
            if (keyboard.digit4Key.wasPressedThisFrame) UseAbility(3);
            if (keyboard.digit5Key.wasPressedThisFrame) UseAbility(4);
            if (keyboard.digit6Key.wasPressedThisFrame) UseAbility(5);

            // Toggle target
            if (keyboard.tKey.wasPressedThisFrame)
            {
                _targetSelf = !_targetSelf;
                Debug.Log($"[AbilityTestController] Target mode: {(_targetSelf ? "SELF" : "TARGET UNIT")}");
            }

            // Reset action points
            if (keyboard.rKey.wasPressedThisFrame)
            {
                _abilityController.ResetActionPoints();
                Debug.Log($"[AbilityTestController] Action points reset! ({_abilityController.ActionPoints}/{_abilityController.MaxActionPoints})");
            }

            // Display abilities list
            if (keyboard.hKey.wasPressedThisFrame)
            {
                DisplayAbilities();
            }
        }

        private void UseAbility(int index)
        {
            if (_abilityController.Abilities.Count <= index)
            {
                Debug.LogWarning($"[AbilityTestController] Ability slot {index + 1} is empty!");
                return;
            }

            AbilityData ability = _abilityController.Abilities[index];
            Unit targetUnit = _targetSelf ? caster : target;

            // Handle self/aura abilities
            if (ability.rangeType == Warslammer.Core.RangeType.Self ||
                ability.rangeType == Warslammer.Core.RangeType.Aura)
            {
                targetUnit = null; // Self abilities don't need a target
            }

            Debug.Log($"[AbilityTestController] Attempting to use: {ability.abilityName}");
            Debug.Log($"[AbilityTestController] Target: {(targetUnit != null ? targetUnit.name : "Self/AOE")}");
            Debug.Log($"[AbilityTestController] Action Points: {_abilityController.ActionPoints}/{_abilityController.MaxActionPoints}");

            bool success = _abilityController.UseAbility(ability, targetUnit);

            if (success)
            {
                Debug.Log($"<color=green>[AbilityTestController] ✓ {ability.abilityName} used successfully!</color>");
                Debug.Log($"[AbilityTestController] Remaining Action Points: {_abilityController.ActionPoints}");
            }
            else
            {
                Debug.LogWarning($"<color=red>[AbilityTestController] ✗ Failed to use {ability.abilityName}</color>");
            }
        }

        private void DisplayAbilities()
        {
            Debug.Log("=== AVAILABLE ABILITIES ===");

            if (_abilityController.Abilities.Count == 0)
            {
                Debug.LogWarning("No abilities assigned to caster! Assign abilities in the Inspector.");
                return;
            }

            for (int i = 0; i < _abilityController.Abilities.Count; i++)
            {
                AbilityData ability = _abilityController.Abilities[i];
                if (ability == null)
                {
                    Debug.Log($"  [{i + 1}] Empty slot");
                    continue;
                }

                int cooldown = _abilityController.GetAbilityCooldown(ability);
                string cooldownText = cooldown > 0 ? $"(Cooldown: {cooldown})" : "";

                Debug.Log($"  [{i + 1}] {ability.abilityName} - {ability.actionPointCost} AP {cooldownText}");
                Debug.Log($"      Range: {ability.range}\", AOE: {ability.areaOfEffect}\"");

                if (ability.damage > 0)
                    Debug.Log($"      Damage: {ability.damage} ({ability.damageType})");

                if (ability.healing > 0)
                    Debug.Log($"      Healing: {ability.healing}");

                if (ability.attackModifier != 0 || ability.defenseModifier != 0)
                    Debug.Log($"      Modifiers: ATK{ability.attackModifier:+0;-0;+0} DEF{ability.defenseModifier:+0;-0;+0}");
            }

            Debug.Log($"\nAction Points: {_abilityController.ActionPoints}/{_abilityController.MaxActionPoints}");
            Debug.Log($"Target Mode: {(_targetSelf ? "SELF" : "TARGET UNIT")}");
            Debug.Log("===========================");
        }

        private void OnGUI()
        {
            // Draw simple on-screen instructions
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.UpperLeft;

            string displayText = $"ABILITY TESTER\n" +
                                $"Action Points: {_abilityController.ActionPoints}/{_abilityController.MaxActionPoints}\n" +
                                $"Target: {(_targetSelf ? "SELF" : (target != null ? target.name : "NONE"))}\n\n" +
                                $"Controls:\n" +
                                $"1-6: Use Ability\n" +
                                $"T: Toggle Target\n" +
                                $"R: Reset AP\n" +
                                $"H: Show Abilities";

            GUI.Label(new Rect(10, 120, 300, 200), displayText, style);
        }
    }
}
