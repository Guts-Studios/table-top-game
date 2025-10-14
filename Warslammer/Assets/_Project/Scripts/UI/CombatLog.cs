using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Warslammer.Combat;

namespace Warslammer.UI
{
    /// <summary>
    /// Displays combat events in a scrolling text log
    /// Shows attack results, damage, kills, etc.
    /// </summary>
    public class CombatLog : MonoBehaviour
    {
        #region Properties
        [Header("UI References")]
        [SerializeField]
        [Tooltip("Text component for combat log")]
        private Text _logText;

        [SerializeField]
        [Tooltip("ScrollRect for scrolling log")]
        private ScrollRect _scrollRect;

        [Header("Log Settings")]
        [SerializeField]
        [Tooltip("Maximum number of log entries")]
        private int _maxLogEntries = 100;

        [SerializeField]
        [Tooltip("Auto-scroll to bottom when new entry added")]
        private bool _autoScrollToBottom = true;

        [SerializeField]
        [Tooltip("Add timestamps to log entries")]
        private bool _showTimestamps = false;

        [SerializeField]
        [Tooltip("Color for damage text")]
        private Color _damageColor = Color.red;

        [SerializeField]
        [Tooltip("Color for kill text")]
        private Color _killColor = new Color(1f, 0.5f, 0f); // Orange

        [SerializeField]
        [Tooltip("Color for critical text")]
        private Color _criticalColor = Color.yellow;

        [SerializeField]
        [Tooltip("Color for miss text")]
        private Color _missColor = Color.gray;

        private List<string> _logEntries = new List<string>();
        private CombatResolver _combatResolver;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_logText == null)
            {
                _logText = GetComponentInChildren<Text>();
            }

            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }
        }

        private void Start()
        {
            // Subscribe to combat events
            _combatResolver = CombatResolver.Instance;
            if (_combatResolver != null)
            {
                _combatResolver.OnCombatResolved.AddListener(OnCombatResolved);
            }

            // Initialize log
            ClearLog();
            AddEntry("=== Combat Log Initialized ===");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_combatResolver != null)
            {
                _combatResolver.OnCombatResolved.RemoveListener(OnCombatResolved);
            }
        }
        #endregion

        #region Combat Events
        /// <summary>
        /// Called when combat is resolved
        /// </summary>
        private void OnCombatResolved(CombatResult result)
        {
            if (result == null)
                return;

            AddCombatResultEntry(result);
        }

        /// <summary>
        /// Add a formatted combat result to the log
        /// </summary>
        private void AddCombatResultEntry(CombatResult result)
        {
            string entry = "";

            // Attack declaration
            entry += $"{result.Attacker?.GetUnitName() ?? "Unknown"} attacks {result.Defender?.GetUnitName() ?? "Unknown"}";
            
            if (result.Weapon != null)
            {
                entry += $" with {result.Weapon.weaponName}";
            }

            entry += $" ({result.AttackDistance:F1}\")";

            AddEntry(entry);

            // Attack roll
            if (result.AttackRoll != null)
            {
                AddEntry($"  Attack: {result.AttackRoll.GetRollsString()} = {result.AttackRoll.Successes} successes");
            }

            // Defense roll
            if (result.DefenseRoll != null && result.DefenseRoll.Successes > 0)
            {
                AddEntry($"  Defense: {result.DefenseRoll.GetRollsString()} = {result.DefenseRoll.Successes} blocks");
            }

            // Result
            if (result.WasHit)
            {
                string damageText = $"  → HIT for {result.DamageDealt} damage!";
                
                if (result.HadCritical)
                {
                    damageText = ColorText(damageText, _criticalColor);
                    damageText += ColorText($" ({result.CriticalCount} CRITICAL!)", _criticalColor);
                }
                else
                {
                    damageText = ColorText(damageText, _damageColor);
                }

                AddEntry(damageText);

                if (result.WasKilled)
                {
                    string killText = $"  → {result.Defender?.GetUnitName()} was KILLED!";
                    AddEntry(ColorText(killText, _killColor));
                }
            }
            else
            {
                AddEntry(ColorText("  → Miss!", _missColor));
            }

            // Separator
            AddEntry("");
        }
        #endregion

        #region Log Management
        /// <summary>
        /// Add an entry to the combat log
        /// </summary>
        public void AddEntry(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                _logEntries.Add("");
            }
            else
            {
                string entry = message;

                // Add timestamp if enabled
                if (_showTimestamps)
                {
                    entry = $"[{Time.time:F1}s] {entry}";
                }

                _logEntries.Add(entry);
            }

            // Trim old entries if over limit
            while (_logEntries.Count > _maxLogEntries)
            {
                _logEntries.RemoveAt(0);
            }

            // Update display
            UpdateLogDisplay();

            // Auto-scroll to bottom
            if (_autoScrollToBottom)
            {
                ScrollToBottom();
            }
        }

        /// <summary>
        /// Clear the combat log
        /// </summary>
        public void ClearLog()
        {
            _logEntries.Clear();
            UpdateLogDisplay();
        }

        /// <summary>
        /// Update the log text display
        /// </summary>
        private void UpdateLogDisplay()
        {
            if (_logText == null)
                return;

            _logText.text = string.Join("\n", _logEntries);
        }

        /// <summary>
        /// Scroll to bottom of log
        /// </summary>
        private void ScrollToBottom()
        {
            if (_scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _scrollRect.verticalNormalizedPosition = 0f;
            }
        }
        #endregion

        #region Text Formatting
        /// <summary>
        /// Color text using rich text tags
        /// </summary>
        private string ColorText(string text, Color color)
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hexColor}>{text}</color>";
        }

        /// <summary>
        /// Make text bold
        /// </summary>
        private string BoldText(string text)
        {
            return $"<b>{text}</b>";
        }

        /// <summary>
        /// Make text italic
        /// </summary>
        private string ItalicText(string text)
        {
            return $"<i>{text}</i>";
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set maximum number of log entries
        /// </summary>
        public void SetMaxLogEntries(int max)
        {
            _maxLogEntries = Mathf.Max(1, max);
        }

        /// <summary>
        /// Enable/disable auto-scroll
        /// </summary>
        public void SetAutoScroll(bool enabled)
        {
            _autoScrollToBottom = enabled;
        }

        /// <summary>
        /// Enable/disable timestamps
        /// </summary>
        public void SetShowTimestamps(bool enabled)
        {
            _showTimestamps = enabled;
        }
        #endregion
    }
}