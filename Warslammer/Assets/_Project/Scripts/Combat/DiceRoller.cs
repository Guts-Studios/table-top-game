using UnityEngine;

namespace Warslammer.Combat
{
    /// <summary>
    /// Performs dice rolls with visualization options
    /// Handles rerolls and modifier application
    /// </summary>
    public class DiceRoller : MonoBehaviour
    {
        #region Singleton
        private static DiceRoller _instance;
        
        /// <summary>
        /// Global access point for the DiceRoller
        /// </summary>
        public static DiceRoller Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DiceRoller>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DiceRoller");
                        _instance = go.AddComponent<DiceRoller>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Properties
        [Header("Dice Settings")]
        [SerializeField]
        [Tooltip("Use animated dice rolling (slower but more visual)")]
        private bool _useAnimatedRolls = false;
        
        /// <summary>
        /// Use animated dice rolling?
        /// </summary>
        public bool UseAnimatedRolls
        {
            get => _useAnimatedRolls;
            set => _useAnimatedRolls = value;
        }

        [SerializeField]
        [Tooltip("Seed for random number generator (0 for random seed)")]
        private int _randomSeed = 0;

        private System.Random _random;
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
            
            // Initialize random number generator
            if (_randomSeed != 0)
            {
                _random = new System.Random(_randomSeed);
            }
            else
            {
                _random = new System.Random();
            }
        }
        #endregion

        #region Dice Rolling
        /// <summary>
        /// Roll a dice pool and return the result
        /// </summary>
        /// <param name="pool">Dice pool to roll</param>
        /// <returns>Dice roll result</returns>
        public DiceRollResult Roll(DicePool pool)
        {
            if (pool == null || pool.diceCount <= 0)
            {
                return new DiceRollResult(4, 0);
            }

            DiceRollResult result = new DiceRollResult(pool.targetNumber, pool.diceModifier);

            // Roll each die
            for (int i = 0; i < pool.diceCount; i++)
            {
                int roll = RollSingleDie(pool.diceSides);
                
                // Check for reroll conditions
                bool shouldReroll = false;
                if (pool.rerollOnes && roll == 1)
                {
                    shouldReroll = true;
                }
                else if (pool.rerollFailed && (roll + pool.diceModifier) < pool.targetNumber)
                {
                    shouldReroll = true;
                }

                // Perform reroll if needed
                if (shouldReroll)
                {
                    result.MarkRerolled(i);
                    roll = RollSingleDie(pool.diceSides);
                }

                // Add roll to result (handles success and critical checking)
                result.AddRoll(roll, pool.diceSides);
            }

            Debug.Log($"[DiceRoller] Rolled {pool}: {result}");
            
            return result;
        }

        /// <summary>
        /// Roll a single die
        /// </summary>
        /// <param name="sides">Number of sides on the die</param>
        /// <returns>Roll result (1 to sides)</returns>
        private int RollSingleDie(int sides)
        {
            return _random.Next(1, sides + 1);
        }

        /// <summary>
        /// Roll multiple dice and return the sum
        /// </summary>
        /// <param name="count">Number of dice</param>
        /// <param name="sides">Sides per die</param>
        /// <returns>Sum of all rolls</returns>
        public int RollSum(int count, int sides)
        {
            int sum = 0;
            for (int i = 0; i < count; i++)
            {
                sum += RollSingleDie(sides);
            }
            return sum;
        }

        /// <summary>
        /// Roll a single d6
        /// </summary>
        /// <returns>Result (1-6)</returns>
        public int RollD6()
        {
            return RollSingleDie(6);
        }

        /// <summary>
        /// Roll 2d6 and return the sum
        /// </summary>
        /// <returns>Sum of 2d6</returns>
        public int Roll2D6()
        {
            return RollSingleDie(6) + RollSingleDie(6);
        }

        /// <summary>
        /// Roll a d100 (percentile)
        /// </summary>
        /// <returns>Result (1-100)</returns>
        public int RollD100()
        {
            return RollSingleDie(100);
        }
        #endregion

        #region Probability
        /// <summary>
        /// Calculate the probability of getting at least N successes
        /// </summary>
        /// <param name="pool">Dice pool</param>
        /// <param name="requiredSuccesses">Required number of successes</param>
        /// <returns>Probability (0.0 to 1.0)</returns>
        public float CalculateSuccessProbability(DicePool pool, int requiredSuccesses)
        {
            if (pool.diceCount <= 0 || requiredSuccesses <= 0)
                return 0f;

            // Simple approximation - exact calculation would require binomial distribution
            float successChance = (pool.diceSides - pool.targetNumber + 1 + pool.diceModifier) / (float)pool.diceSides;
            successChance = Mathf.Clamp01(successChance);
            
            float expectedSuccesses = pool.diceCount * successChance;
            
            // Very rough approximation
            if (requiredSuccesses <= expectedSuccesses)
                return 0.5f + (expectedSuccesses - requiredSuccesses) * 0.1f;
            else
                return 0.5f - (requiredSuccesses - expectedSuccesses) * 0.1f;
        }

        /// <summary>
        /// Calculate expected number of successes for a dice pool
        /// </summary>
        /// <param name="pool">Dice pool</param>
        /// <returns>Expected successes</returns>
        public float CalculateExpectedSuccesses(DicePool pool)
        {
            if (pool.diceCount <= 0)
                return 0f;

            float successChance = 0f;
            
            // Calculate base success chance
            for (int i = pool.targetNumber; i <= pool.diceSides; i++)
            {
                int modifiedRoll = i + pool.diceModifier;
                if (modifiedRoll >= pool.targetNumber)
                {
                    successChance += 1f / pool.diceSides;
                    
                    // Critical hits add extra success
                    if (pool.allowCriticals && i == pool.diceSides)
                    {
                        successChance += 1f / pool.diceSides;
                    }
                }
            }

            return pool.diceCount * successChance;
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set random seed (for testing/reproducibility)
        /// </summary>
        /// <param name="seed">Seed value (0 for random)</param>
        public void SetRandomSeed(int seed)
        {
            _randomSeed = seed;
            if (seed != 0)
            {
                _random = new System.Random(seed);
            }
            else
            {
                _random = new System.Random();
            }
        }

        /// <summary>
        /// Reset random seed to random
        /// </summary>
        public void ResetRandomSeed()
        {
            SetRandomSeed(0);
        }
        #endregion
    }
}