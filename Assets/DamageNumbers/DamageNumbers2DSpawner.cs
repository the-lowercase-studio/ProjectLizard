using Assets.CustomTypes;
using Assets.CustomTypes.ValueRanges;
using DG.Tweening;
using Reflex.Attributes;
using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.DamageNumbers
{
    public enum DamageNumberSpawnPattern
    {
        FullCircle,
        UpperHalf
    }

    public readonly struct DamageNumbers2DSpawnerConfig
    {
        public int Damage { get; }
        public DamageNumberSpawnPattern SpawnPattern { get; }

        public DamageNumbers2DSpawnerConfig(int damage, DamageNumberSpawnPattern spawnPattern)
        {
            Damage = damage;
            SpawnPattern = spawnPattern;
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

            if (_visualAppearanceByDamageThresholds == null || _visualAppearanceByDamageThresholds.Length == 0)
            {
                Debug.LogError($"No damage-number appearance thresholds configured on {name}.");
                return;
            }


            if (!TryToLocalCanvasPosition(worldPosition, projectionCamera, out Vector2 anchoredStartPosition))
            {
                return;
            }

            DamageNumberAppearance? appearance = FindCorrectAppearanceByThreshold(config.Damage);
            if (appearance is null)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                DamageNumber2D damageNumber = Instantiate(_damageNumberPrefab, _spawnContainer);
                RectTransform popupTransform = damageNumber.transform as RectTransform;
                popupTransform.anchoredPosition = anchoredStartPosition;
                popupTransform.localScale = Vector3.one;
                popupTransform.localRotation = Quaternion.identity;

                damageNumber.Initialize(new DamageNumber2DConfig(config.Damage, appearance.Value));

                damageNumber.OnLifeEnd += DamageNumber_OnLifeEnd;

                Vector2 destination = anchoredStartPosition + GetMovementOffset(config.SpawnPattern);
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

        private Vector2 GetMovementOffset(DamageNumberSpawnPattern spawnPattern)
        {
            float angle = spawnPattern switch
            {
                DamageNumberSpawnPattern.FullCircle => UnityEngine.Random.Range(0f, 360f),
                DamageNumberSpawnPattern.UpperHalf => UnityEngine.Random.Range(35f, 145f),
                _ => 90f
            };

            float angleInRadians = angle * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(_popupMovementRange.Min, _popupMovementRange.Max);

            return new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * distance;
        }

        private DamageNumberAppearance? FindCorrectAppearanceByThreshold(int damage)
        {
            for (int i = _visualAppearanceByDamageThresholds.Length - 1; i >= 0; i--)
            {
                if (_visualAppearanceByDamageThresholds[i].Threshold <= damage)
                {
                    return _visualAppearanceByDamageThresholds[i].Appearance;
                }
            }

            return null;
        }
    }
}
