using UnityEngine;
using Warslammer.Units;

namespace Warslammer.Combat
{
    /// <summary>
    /// Handles morale checks and Shaken status
    /// Triggered when units lose significant HP in one round
    /// </summary>
    public class MoraleSystem : MonoBehaviour
    {
        #region Singleton
        private static MoraleSystem _instance;
        
        /// <summary>
        /// Global access point for the MoraleSystem
        /// </summary>
        public static MoraleSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<MoraleSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("MoraleSystem");
                        _instance = go.AddComponent<MoraleSystem>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Properties
        [Header("Morale Settings")]
        [SerializeField]
        [Tooltip("HP loss threshold to trigger morale check (as percentage)")]
        [Range(0f, 1f)]
        private float _moraleCheckThreshold = 0.25f;

        [SerializeField]
        [Tooltip("Base morale value for all units")]
        private int _baseMoraleValue = 7;

        [SerializeField]
        [Tooltip("Use 2d6 for morale checks (true) or 1d6 (false)")]
        private bool _use2d6 = true;

        private DiceRoller _diceRoller;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            _diceRoller = DiceRoller.Instance;
        }
        #endregion

        #region Morale Checks
        /// <summary>
        /// Check if unit should make a morale check
        /// </summary>
        /// <param name="unit">Unit to check</param>
        /// <param name="damageTaken">Damage taken this round</param>
        /// <returns>True if morale check is needed</returns>
        public bool ShouldCheckMorale(Unit unit, int damageTaken)
        {
            if (unit == null || !unit.IsAlive)
                return false;

            float damagePercentage = (float)damageTaken / unit.MaxHealth;
            return damagePercentage >= _moraleCheckThreshold;
        }

        /// <summary>
        /// Perform a morale check for a unit
        /// </summary>
        /// <param name="unit">Unit to check</param>
        /// <returns>True if morale check passed</returns>
        public bool CheckMorale(Unit unit)
        {
            if (unit == null)
                return false;

            // Get unit's leadership value (TODO: Phase 4 - add Leadership stat to UnitData)
            int leadership = GetUnitLeadership(unit);

            // Roll morale
            int roll = _use2d6 ? _diceRoller.Roll2D6() : _diceRoller.RollD6();

            bool passed = roll <= leadership;

            Debug.Log($"[MoraleSystem] {unit.GetUnitName()} morale check: rolled {roll} vs Leadership {leadership} - {(passed ? "PASSED" : "FAILED")}");

            if (!passed)
            {
                // Apply Shaken status
                ApplyShaken(unit);
            }

            return passed;
        }

        /// <summary>
        /// Get unit's leadership value
        /// </summary>
        /// <param name="unit">Unit to check</param>
        /// <returns>Leadership value</returns>
        private int GetUnitLeadership(Unit unit)
        {
            // TODO: Phase 4 - get from unit data
            // For now, use base value
            int leadership = _baseMoraleValue;

            // Apply modifiers
            StatusEffectManager effectManager = unit.GetComponent<StatusEffectManager>();
            if (effectManager != null && effectManager.IsShaken())
            {
                leadership -= 2; // Penalty if already shaken
            }

            return leadership;
        }

        /// <summary>
        /// Apply Shaken status to a unit
        /// </summary>
        /// <param name="unit">Unit to affect</param>
        private void ApplyShaken(Unit unit)
        {
            StatusEffectManager effectManager = unit.GetComponent<StatusEffectManager>();
            if (effectManager != null)
            {
                effectManager.ApplyShaken();
                Debug.Log($"[MoraleSystem] {unit.GetUnitName()} is now SHAKEN!");
            }
        }

        /// <summary>
        /// Check morale after taking damage
        /// </summary>
        /// <param name="unit">Unit that took damage</param>
        /// <param name="damageTaken">Amount of damage taken</param>
        public void CheckMoraleAfterDamage(Unit unit, int damageTaken)
        {
            if (ShouldCheckMorale(unit, damageTaken))
            {
                CheckMorale(unit);
            }
        }
        #endregion

        #region Rally
        /// <summary>
        /// Attempt to rally a shaken unit
        /// </summary>
        /// <param name="unit">Unit to rally</param>
        /// <returns>True if rally succeeded</returns>
        public bool AttemptRally(Unit unit)
        {
            if (unit == null)
                return false;

            StatusEffectManager effectManager = unit.GetComponent<StatusEffectManager>();
            if (effectManager == null || !effectManager.IsShaken())
            {
                Debug.LogWarning($"[MoraleSystem] {unit.GetUnitName()} is not shaken, cannot rally");
                return false;
            }

            // Get leadership
            int leadership = GetUnitLeadership(unit);

            // Roll rally check (same as morale check)
            int roll = _use2d6 ? _diceRoller.Roll2D6() : _diceRoller.RollD6();

            bool rallied = roll <= leadership;

            Debug.Log($"[MoraleSystem] {unit.GetUnitName()} rally attempt: rolled {roll} vs Leadership {leadership} - {(rallied ? "RALLIED!" : "FAILED")}");

            if (rallied)
            {
                effectManager.RemoveShaken();
            }

            return rallied;
        }

        /// <summary>
        /// Automatically rally a unit (e.g., from abilities)
        /// </summary>
        /// <param name="unit">Unit to rally</param>
        public void AutoRally(Unit unit)
        {
            if (unit == null)
                return;

            StatusEffectManager effectManager = unit.GetComponent<StatusEffectManager>();
            if (effectManager != null && effectManager.IsShaken())
            {
                effectManager.RemoveShaken();
                Debug.Log($"[MoraleSystem] {unit.GetUnitName()} automatically rallied!");
            }
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set morale check threshold
        /// </summary>
        /// <param name="threshold">HP loss percentage (0-1)</param>
        public void SetMoraleCheckThreshold(float threshold)
        {
            _moraleCheckThreshold = Mathf.Clamp01(threshold);
        }

        /// <summary>
        /// Set base morale value
        /// </summary>
        /// <param name="morale">Base morale value</param>
        public void SetBaseMoraleValue(int morale)
        {
            _baseMoraleValue = Mathf.Max(1, morale);
        }

        /// <summary>
        /// Set whether to use 2d6 or 1d6 for checks
        /// </summary>
        /// <param name="use2d6">Use 2d6?</param>
        public void SetUse2D6(bool use2d6)
        {
            _use2d6 = use2d6;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Calculate morale check probability
        /// </summary>
        /// <param name="leadership">Leadership value</param>
        /// <returns>Probability of passing (0-1)</returns>
        public float CalculateMoralePassProbability(int leadership)
        {
            if (_use2d6)
            {
                // 2d6: probability calculation is more complex
                // Approximate: (leadership - 1) / 11
                return Mathf.Clamp01((float)(leadership - 1) / 11f);
            }
            else
            {
                // 1d6: simple probability
                return Mathf.Clamp01((float)leadership / 6f);
            }
        }
        #endregion
    }
}