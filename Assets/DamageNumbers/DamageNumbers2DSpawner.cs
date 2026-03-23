using Assets.CustomTypes;
using Assets.CustomTypes.ValueRanges;
using DG.Tweening;
using Reflex.Attributes;
using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.DamageNumbers
{
    public enum DamageNumberType
    {
        Health,
        Shield
    }

    public enum DamageNumberSpawnPattern
    {
        FullCircle,
        UpperHalf
    }

    public readonly struct DamageNumbers2DSpawnerConfig
    {
        public int Damage { get; }
        public DamageNumberType DamageType { get; }
        public DamageNumberSpawnPattern SpawnPattern { get; }
        public float? MovementAngleDegrees { get; }

        public DamageNumbers2DSpawnerConfig(
            int damage,
            DamageNumberSpawnPattern spawnPattern,
            DamageNumberType damageType = DamageNumberType.Health,
            float? movementAngleDegrees = null)
        {
            Damage = damage;
            DamageType = damageType;
            SpawnPattern = spawnPattern;
            MovementAngleDegrees = movementAngleDegrees;
        }
    }

    public interface IDamageNumbers2DSpawner
    {
        uint CurrentlySpawnedObjectsCount { get; }

        event EventHandler OnSpawnedEntityReleased;

        void DisableFunctionality();
        void EnableFunctionality();
        void Spawn(Vector3 worldPosition, DamageNumbers2DSpawnerConfig config, int count = 1);
        void SpawnAtTarget(Transform targetTransform, DamageNumbers2DSpawnerConfig config, int count = 1);
    }

    public class DamageNumbers2DSpawner : MonoBehaviour, IDamageNumbers2DSpawner
    {
        [Serializable]
        private struct VisualAppearanceByDamageThreshold
        {
            [SerializeField] public int Threshold;
            [SerializeField] public DamageNumberAppearance Appearance;

            public VisualAppearanceByDamageThreshold(int threshold, DamageNumberAppearance appearance)
            {
                Threshold = threshold;
                Appearance = appearance;
            }
        }

        [Inject] private Camera _mainCamera;

        [SerializeField] private RectTransform _spawnContainer;
        [SerializeField] private DamageNumber2D _damageNumberPrefab;

        [Header("Motion")]
        [SerializeField] private float _popupVisibilityDuration = 0.6f;
        [SerializeField] private FloatValueRange _popupMovementRange = new FloatValueRange(50f, 80f);

        [Header("Appearance by Damage")]
        [SerializeField] private VisualAppearanceByDamageThreshold[] _visualAppearanceByDamageThresholds;

        [Header("Appearance by Shield Damage")]
        [SerializeField] private VisualAppearanceByDamageThreshold[] _shieldVisualAppearanceByDamageThresholds;

        public event EventHandler OnSpawnedEntityReleased;

        public uint CurrentlySpawnedObjectsCount { get; private set; }

        private bool _isPopupsEnabled = true;

        public void EnableFunctionality()
        {
            _isPopupsEnabled = true;
        }

        public void DisableFunctionality()
        {
            _isPopupsEnabled = false;
        }

        public void Spawn(Vector3 worldPosition, DamageNumbers2DSpawnerConfig config, int count = 1)
        {
            SpawnInternal(worldPosition, _mainCamera, config, count);
        }

        public void SpawnAtTarget(Transform targetTransform, DamageNumbers2DSpawnerConfig config, int count = 1)
        {
            if (targetTransform == null)
            {
                return;
            }

            Vector3 spawnWorldPosition = GetTargetVisualCenter(targetTransform);
            Camera projectionCamera = ResolveProjectionCamera(targetTransform);

            SpawnInternal(spawnWorldPosition, projectionCamera, config, count);
        }

        private void SpawnInternal(Vector3 worldPosition, Camera projectionCamera, DamageNumbers2DSpawnerConfig config, int count)
        {
            if (!_isPopupsEnabled || _damageNumberPrefab == null || _spawnContainer == null)
            {
                return;
            }

            if (!TryGetThresholdsByDamageType(config.DamageType, out VisualAppearanceByDamageThreshold[] thresholds) || thresholds == null || thresholds.Length == 0)
            {
                Debug.LogError($"No {config.DamageType} damage-number appearance thresholds configured on {name}.");
                return;
            }


            if (!TryToLocalCanvasPosition(worldPosition, projectionCamera, out Vector2 anchoredStartPosition))
            {
                return;
            }

            DamageNumberAppearance appearance = FindCorrectAppearanceByThreshold(config.Damage, thresholds);

            for (int i = 0; i < count; i++)
            {
                DamageNumber2D damageNumber = Instantiate(_damageNumberPrefab, _spawnContainer);
                RectTransform popupTransform = damageNumber.transform as RectTransform;
                popupTransform.anchoredPosition = anchoredStartPosition;
                popupTransform.localScale = Vector3.one;
                popupTransform.localRotation = Quaternion.identity;

                damageNumber.Initialize(new DamageNumber2DConfig(config.Damage, appearance));

                damageNumber.OnLifeEnd += DamageNumber_OnLifeEnd;

                Vector2 destination = anchoredStartPosition + GetMovementOffset(config);
                popupTransform.DOAnchorPos(destination, _popupVisibilityDuration).SetEase(Ease.OutSine);

                CurrentlySpawnedObjectsCount++;
            }
        }

        private void DamageNumber_OnLifeEnd(object sender, EventArgs args)
        {
            if (sender is not DamageNumber2D damageNumber)
            {
                return;
            }

            CurrentlySpawnedObjectsCount--;

            damageNumber.OnLifeEnd -= DamageNumber_OnLifeEnd;

            Destroy(damageNumber.gameObject);

            OnSpawnedEntityReleased?.Invoke(damageNumber, EventArgs.Empty);
        }

        private bool TryToLocalCanvasPosition(Vector3 worldPosition, Camera projectionCamera, out Vector2 localPoint)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(projectionCamera, worldPosition);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(_spawnContainer, screenPoint, GetUICamera(), out localPoint);
        }

        private Camera ResolveProjectionCamera(Transform targetTransform)
        {
            Canvas targetCanvas = targetTransform.GetComponentInParent<Canvas>();
            if (targetCanvas != null)
            {
                if (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    return null;
                }

                if (targetCanvas.worldCamera != null)
                {
                    return targetCanvas.worldCamera;
                }
            }

            return _mainCamera;
        }

        private Vector3 GetTargetVisualCenter(Transform targetTransform)
        {
            if (targetTransform is RectTransform rectTransform)
            {
                return rectTransform.TransformPoint(rectTransform.rect.center);
            }

            if (targetTransform.TryGetComponent(out Renderer renderer))
            {
                return renderer.bounds.center;
            }

            if (targetTransform.TryGetComponent(out Collider2D collider2D))
            {
                return collider2D.bounds.center;
            }

            if (targetTransform.TryGetComponent(out Collider collider))
            {
                return collider.bounds.center;
            }

            return targetTransform.position;
        }

        private Camera GetUICamera()
        {
            Canvas canvas = _spawnContainer.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return canvas.worldCamera != null ? canvas.worldCamera : _mainCamera;
        }

        private Vector2 GetMovementOffset(DamageNumbers2DSpawnerConfig config)
        {
            float angle = config.MovementAngleDegrees ?? config.SpawnPattern switch
            {
                DamageNumberSpawnPattern.FullCircle => UnityEngine.Random.Range(0f, 360f),
                DamageNumberSpawnPattern.UpperHalf => UnityEngine.Random.Range(35f, 145f),
                _ => 90f
            };

            float angleInRadians = angle * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(_popupMovementRange.Min, _popupMovementRange.Max);

            return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * distance;
        }

        private DamageNumberAppearance FindCorrectAppearanceByThreshold(int damage, VisualAppearanceByDamageThreshold[] thresholds)
        {
            int selectedIndex = -1;
            int selectedThreshold = int.MinValue;
            int smallestThresholdIndex = 0;

            for (int i = 0; i < thresholds.Length; i++)
            {
                if (thresholds[i].Threshold < thresholds[smallestThresholdIndex].Threshold)
                {
                    smallestThresholdIndex = i;
                }

                if (thresholds[i].Threshold <= damage && thresholds[i].Threshold > selectedThreshold)
                {
                    selectedThreshold = thresholds[i].Threshold;
                    selectedIndex = i;
                }
            }

            return selectedIndex >= 0
                ? thresholds[selectedIndex].Appearance
                : thresholds[smallestThresholdIndex].Appearance;
        }

        private bool TryGetThresholdsByDamageType(DamageNumberType damageType, out VisualAppearanceByDamageThreshold[] thresholds)
        {
            bool hasHealthThresholds = _visualAppearanceByDamageThresholds != null && _visualAppearanceByDamageThresholds.Length > 0;
            bool hasShieldThresholds = _shieldVisualAppearanceByDamageThresholds != null && _shieldVisualAppearanceByDamageThresholds.Length > 0;

            if (damageType == DamageNumberType.Shield)
            {
                if (hasShieldThresholds)
                {
                    thresholds = _shieldVisualAppearanceByDamageThresholds;
                    return true;
                }

                if (hasHealthThresholds)
                {
                    thresholds = _visualAppearanceByDamageThresholds;
                    return true;
                }

                thresholds = null;
                return false;
            }

            if (hasHealthThresholds)
            {
                thresholds = _visualAppearanceByDamageThresholds;
                return true;
            }

            thresholds = null;
            return false;
        }
    }
}
