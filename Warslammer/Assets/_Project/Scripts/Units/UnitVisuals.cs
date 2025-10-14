using UnityEngine;
using Warslammer.Data;

namespace Warslammer.Units
{
    /// <summary>
    /// Controls sprite rendering, selection highlights, damage indicators
    /// Handles visual feedback for unit state
    /// </summary>
    public class UnitVisuals : MonoBehaviour
    {
        #region Properties
        [Header("Sprite Rendering")]
        [SerializeField]
        [Tooltip("Main sprite renderer for the unit")]
        private SpriteRenderer _spriteRenderer;

        [SerializeField]
        [Tooltip("Base sprite for the unit")]
        private Sprite _baseSprite;

        [Header("Selection Visual")]
        [SerializeField]
        [Tooltip("Selection highlight renderer")]
        private SpriteRenderer _selectionHighlight;

        [SerializeField]
        [Tooltip("Color for selection highlight")]
        private Color _selectionColor = new Color(1f, 1f, 0f, 0.5f);

        [SerializeField]
        [Tooltip("Scale multiplier for selection highlight")]
        private float _selectionScale = 1.2f;

        [Header("Damage Indicators")]
        [SerializeField]
        [Tooltip("Damage flash color")]
        private Color _damageFlashColor = Color.red;

        [SerializeField]
        [Tooltip("Duration of damage flash")]
        private float _damageFlashDuration = 0.2f;

        [Header("Health Bar")]
        [SerializeField]
        [Tooltip("Show health bar above unit")]
        private bool _showHealthBar = true;

        [SerializeField]
        [Tooltip("Health bar container")]
        private GameObject _healthBarContainer;

        [SerializeField]
        [Tooltip("Health bar fill renderer")]
        private SpriteRenderer _healthBarFill;

        [SerializeField]
        [Tooltip("Offset for health bar")]
        private Vector3 _healthBarOffset = new Vector3(0, 1f, 0);

        private Unit _unit;
        private Color _originalColor;
        private bool _isFlashing;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _unit = GetComponent<Unit>();

            // Get or create sprite renderer
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
                if (_spriteRenderer == null)
                {
                    _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                }
            }

            _originalColor = _spriteRenderer.color;

            // Create selection highlight if needed
            if (_selectionHighlight == null)
            {
                CreateSelectionHighlight();
            }

            // Create health bar if needed
            if (_showHealthBar && _healthBarContainer == null)
            {
                CreateHealthBar();
            }

            // Hide selection by default
            if (_selectionHighlight != null)
            {
                _selectionHighlight.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            // Update health bar
            if (_showHealthBar && _unit != null)
            {
                UpdateHealthBar();
            }

            // Keep health bar facing camera
            if (_healthBarContainer != null && Camera.main != null)
            {
                _healthBarContainer.transform.LookAt(Camera.main.transform);
                _healthBarContainer.transform.Rotate(0, 180, 0);
            }
        }
        #endregion

        #region Sprite Management
        /// <summary>
        /// Set the unit's sprite
        /// </summary>
        /// <param name="sprite">Sprite to display</param>
        public void SetSprite(Sprite sprite)
        {
            if (_spriteRenderer != null && sprite != null)
            {
                _baseSprite = sprite;
                _spriteRenderer.sprite = sprite;
            }
        }

        /// <summary>
        /// Get the current sprite
        /// </summary>
        public Sprite GetCurrentSprite()
        {
            return _spriteRenderer != null ? _spriteRenderer.sprite : null;
        }

        /// <summary>
        /// Initialize visuals from unit data
        /// </summary>
        /// <param name="unitData">Unit data to use</param>
        public void Initialize(UnitData unitData)
        {
            if (unitData == null || _spriteRenderer == null)
                return;

            if (unitData.unitSprite != null)
            {
                SetSprite(unitData.unitSprite);
            }
        }
        #endregion

        #region Selection Highlight
        /// <summary>
        /// Create selection highlight sprite
        /// </summary>
        private void CreateSelectionHighlight()
        {
            GameObject highlightObj = new GameObject("SelectionHighlight");
            highlightObj.transform.SetParent(transform);
            highlightObj.transform.localPosition = Vector3.zero;
            highlightObj.transform.localScale = Vector3.one * _selectionScale;

            _selectionHighlight = highlightObj.AddComponent<SpriteRenderer>();
            
            // Create a simple circle sprite for highlight
            // In production, you'd use a proper highlight sprite
            if (_spriteRenderer != null && _spriteRenderer.sprite != null)
            {
                _selectionHighlight.sprite = _spriteRenderer.sprite;
            }

            _selectionHighlight.color = _selectionColor;
            _selectionHighlight.sortingOrder = _spriteRenderer != null ? _spriteRenderer.sortingOrder - 1 : -1;
            highlightObj.SetActive(false);
        }

        /// <summary>
        /// Show selection highlight
        /// </summary>
        public void ShowSelection()
        {
            if (_selectionHighlight != null)
            {
                _selectionHighlight.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hide selection highlight
        /// </summary>
        public void HideSelection()
        {
            if (_selectionHighlight != null)
            {
                _selectionHighlight.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Set selection color
        /// </summary>
        /// <param name="color">Color for selection</param>
        public void SetSelectionColor(Color color)
        {
            _selectionColor = color;
            if (_selectionHighlight != null)
            {
                _selectionHighlight.color = color;
            }
        }
        #endregion

        #region Damage Visual Feedback
        /// <summary>
        /// Flash sprite to indicate damage
        /// </summary>
        public void FlashDamage()
        {
            if (_isFlashing || _spriteRenderer == null)
                return;

            StartCoroutine(DamageFlashCoroutine());
        }

        /// <summary>
        /// Damage flash coroutine
        /// </summary>
        private System.Collections.IEnumerator DamageFlashCoroutine()
        {
            _isFlashing = true;

            // Flash to damage color
            _spriteRenderer.color = _damageFlashColor;

            yield return new UnityEngine.WaitForSeconds(_damageFlashDuration);

            // Return to original color
            _spriteRenderer.color = _originalColor;

            _isFlashing = false;
        }

        /// <summary>
        /// Shake the unit sprite
        /// </summary>
        /// <param name="intensity">Shake intensity</param>
        /// <param name="duration">Shake duration</param>
        public void Shake(float intensity = 0.1f, float duration = 0.3f)
        {
            StartCoroutine(ShakeCoroutine(intensity, duration));
        }

        /// <summary>
        /// Shake animation coroutine
        /// </summary>
        private System.Collections.IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            Vector3 originalPosition = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-intensity, intensity);
                float z = Random.Range(-intensity, intensity);
                
                transform.localPosition = originalPosition + new Vector3(x, 0, z);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalPosition;
        }
        #endregion

        #region Health Bar
        /// <summary>
        /// Create health bar UI
        /// </summary>
        private void CreateHealthBar()
        {
            _healthBarContainer = new GameObject("HealthBar");
            _healthBarContainer.transform.SetParent(transform);
            _healthBarContainer.transform.localPosition = _healthBarOffset;

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(_healthBarContainer.transform);
            bgObj.transform.localPosition = Vector3.zero;
            SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
            bgRenderer.color = Color.gray;
            bgRenderer.sortingOrder = 100;

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(_healthBarContainer.transform);
            fillObj.transform.localPosition = Vector3.zero;
            _healthBarFill = fillObj.AddComponent<SpriteRenderer>();
            _healthBarFill.color = Color.green;
            _healthBarFill.sortingOrder = 101;

            // Create simple quad sprites for health bar
            // In production, use proper health bar sprites
            _healthBarContainer.transform.localScale = new Vector3(0.5f, 0.05f, 1f);
        }

        /// <summary>
        /// Update health bar display
        /// </summary>
        private void UpdateHealthBar()
        {
            if (_healthBarFill == null || _unit == null)
                return;

            float healthPercent = _unit.HealthPercentage;

            // Guard against NaN values (can happen if unit has no stats yet)
            if (float.IsNaN(healthPercent) || float.IsInfinity(healthPercent))
            {
                healthPercent = 1f; // Default to full health if stats not initialized
            }

            // Clamp to valid range
            healthPercent = Mathf.Clamp01(healthPercent);

            // Scale fill based on health percentage
            Vector3 fillScale = _healthBarFill.transform.localScale;
            fillScale.x = healthPercent;
            _healthBarFill.transform.localScale = fillScale;

            // Change color based on health
            if (healthPercent > 0.6f)
                _healthBarFill.color = Color.green;
            else if (healthPercent > 0.3f)
                _healthBarFill.color = Color.yellow;
            else
                _healthBarFill.color = Color.red;
        }

        /// <summary>
        /// Show/hide health bar
        /// </summary>
        /// <param name="show">Show health bar?</param>
        public void SetHealthBarVisible(bool show)
        {
            _showHealthBar = show;
            if (_healthBarContainer != null)
            {
                _healthBarContainer.SetActive(show);
            }
        }
        #endregion

        #region Hover Effects
        /// <summary>
        /// Show hover effect
        /// </summary>
        public void ShowHover()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.Lerp(_originalColor, Color.white, 0.3f);
            }
        }

        /// <summary>
        /// Hide hover effect
        /// </summary>
        public void HideHover()
        {
            if (_spriteRenderer != null && !_isFlashing)
            {
                _spriteRenderer.color = _originalColor;
            }
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set sprite renderer
        /// </summary>
        /// <param name="renderer">Sprite renderer to use</param>
        public void SetSpriteRenderer(SpriteRenderer renderer)
        {
            _spriteRenderer = renderer;
            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
        }

        /// <summary>
        /// Set damage flash settings
        /// </summary>
        /// <param name="color">Flash color</param>
        /// <param name="duration">Flash duration</param>
        public void SetDamageFlash(Color color, float duration)
        {
            _damageFlashColor = color;
            _damageFlashDuration = duration;
        }
        #endregion
    }
}