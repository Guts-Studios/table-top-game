using UnityEngine;
using UnityEngine.UI;
using Warslammer.Utilities;

namespace Warslammer.UI
{
    /// <summary>
    /// Shows damage numbers floating above units
    /// Uses object pooling for performance
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        #region Properties
        [Header("UI References")]
        [SerializeField]
        [Tooltip("Text component for damage number")]
        private Text _damageText;

        [Header("Animation Settings")]
        [SerializeField]
        [Tooltip("Movement speed upward")]
        private float _moveSpeed = 2f;

        [SerializeField]
        [Tooltip("Lifetime in seconds")]
        private float _lifetime = 1.5f;

        [SerializeField]
        [Tooltip("Fade out animation curve")]
        private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        [Header("Colors")]
        [SerializeField]
        [Tooltip("Color for normal damage")]
        private Color _normalDamageColor = Color.red;

        [SerializeField]
        [Tooltip("Color for critical damage")]
        private Color _criticalDamageColor = Color.yellow;

        [SerializeField]
        [Tooltip("Color for healing")]
        private Color _healingColor = Color.green;

        [SerializeField]
        [Tooltip("Color for miss")]
        private Color _missColor = Color.gray;

        [Header("Scale Settings")]
        [SerializeField]
        [Tooltip("Scale for normal damage")]
        private float _normalScale = 1f;

        [SerializeField]
        [Tooltip("Scale for critical damage")]
        private float _criticalScale = 1.5f;

        private float _elapsedTime;
        private Vector3 _startPosition;
        private CanvasGroup _canvasGroup;
        private Camera _mainCamera;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_damageText == null)
            {
                _damageText = GetComponentInChildren<Text>();
            }

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (_elapsedTime >= _lifetime)
            {
                ReturnToPool();
                return;
            }

            // Move upward
            transform.position += Vector3.up * _moveSpeed * Time.deltaTime;

            // Fade out
            float normalizedTime = _elapsedTime / _lifetime;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _fadeCurve.Evaluate(normalizedTime);
            }

            // Face camera
            if (_mainCamera != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - _mainCamera.transform.position);
            }

            _elapsedTime += Time.deltaTime;
        }
        #endregion

        #region Setup
        /// <summary>
        /// Initialize popup with damage amount
        /// </summary>
        /// <param name="damage">Damage amount</param>
        /// <param name="position">World position to spawn at</param>
        /// <param name="isCritical">Is this a critical hit?</param>
        public void Setup(int damage, Vector3 position, bool isCritical = false)
        {
            _startPosition = position;
            transform.position = position;
            _elapsedTime = 0f;

            if (_damageText != null)
            {
                _damageText.text = damage.ToString();
                _damageText.color = isCritical ? _criticalDamageColor : _normalDamageColor;
            }

            transform.localScale = Vector3.one * (isCritical ? _criticalScale : _normalScale);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Initialize popup with healing amount
        /// </summary>
        /// <param name="healing">Healing amount</param>
        /// <param name="position">World position to spawn at</param>
        public void SetupHealing(int healing, Vector3 position)
        {
            _startPosition = position;
            transform.position = position;
            _elapsedTime = 0f;

            if (_damageText != null)
            {
                _damageText.text = $"+{healing}";
                _damageText.color = _healingColor;
            }

            transform.localScale = Vector3.one * _normalScale;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Initialize popup with miss text
        /// </summary>
        /// <param name="position">World position to spawn at</param>
        public void SetupMiss(Vector3 position)
        {
            _startPosition = position;
            transform.position = position;
            _elapsedTime = 0f;

            if (_damageText != null)
            {
                _damageText.text = "Miss";
                _damageText.color = _missColor;
            }

            transform.localScale = Vector3.one * _normalScale;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Initialize popup with custom text
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="position">World position to spawn at</param>
        /// <param name="color">Text color</param>
        public void SetupCustom(string text, Vector3 position, Color color)
        {
            _startPosition = position;
            transform.position = position;
            _elapsedTime = 0f;

            if (_damageText != null)
            {
                _damageText.text = text;
                _damageText.color = color;
            }

            transform.localScale = Vector3.one * _normalScale;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }
        #endregion

        #region Pooling
        /// <summary>
        /// Return this popup to the pool
        /// </summary>
        private void ReturnToPool()
        {
            // If using object pooling, return to pool
            // Otherwise, destroy
            gameObject.SetActive(false);
        }
        #endregion

        #region Static Helpers
        /// <summary>
        /// Spawn a damage popup at a position
        /// </summary>
        /// <param name="prefab">Damage popup prefab</param>
        /// <param name="damage">Damage amount</param>
        /// <param name="position">World position</param>
        /// <param name="isCritical">Is critical?</param>
        /// <param name="parent">Parent transform</param>
        /// <returns>Spawned popup instance</returns>
        public static DamagePopup SpawnDamagePopup(DamagePopup prefab, int damage, Vector3 position, bool isCritical = false, Transform parent = null)
        {
            if (prefab == null)
                return null;

            DamagePopup popup = Instantiate(prefab, parent);
            popup.Setup(damage, position, isCritical);
            return popup;
        }

        /// <summary>
        /// Spawn a healing popup at a position
        /// </summary>
        /// <param name="prefab">Damage popup prefab</param>
        /// <param name="healing">Healing amount</param>
        /// <param name="position">World position</param>
        /// <param name="parent">Parent transform</param>
        /// <returns>Spawned popup instance</returns>
        public static DamagePopup SpawnHealingPopup(DamagePopup prefab, int healing, Vector3 position, Transform parent = null)
        {
            if (prefab == null)
                return null;

            DamagePopup popup = Instantiate(prefab, parent);
            popup.SetupHealing(healing, position);
            return popup;
        }

        /// <summary>
        /// Spawn a miss popup at a position
        /// </summary>
        /// <param name="prefab">Damage popup prefab</param>
        /// <param name="position">World position</param>
        /// <param name="parent">Parent transform</param>
        /// <returns>Spawned popup instance</returns>
        public static DamagePopup SpawnMissPopup(DamagePopup prefab, Vector3 position, Transform parent = null)
        {
            if (prefab == null)
                return null;

            DamagePopup popup = Instantiate(prefab, parent);
            popup.SetupMiss(position);
            return popup;
        }
        #endregion
    }
}