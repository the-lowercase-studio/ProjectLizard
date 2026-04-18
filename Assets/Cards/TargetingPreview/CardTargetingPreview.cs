using Assets.Cards.Base.Damage;
using Assets.Cards.Base.Targeting;
using Assets.Effects.Base;
using Assets.Effects.UI;
using Assets.Targeting;
using Assets.UI;
using Reflex.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Cards.Base
{
    [RequireComponent(typeof(Card))]
    public class CardTargetingPreview : MonoBehaviour
    {
        [Inject] private ITargetsProvider _targetsProvider;
        [Inject] private ICardTargetResolver _cardTargetResolver;
        [Inject] private IUITransformsProvider _uiTransformsProvider;
        [Inject] private Camera _mainCamera;

        [SerializeField] private CardTargetCrosshairPresenter _crosshairPresenterPrefab;
        private RectTransform _crosshairContainer;

        private readonly List<PreviewEntry> _activeEntries = new List<PreviewEntry>();

        private Card _card;
        private bool _isPreviewVisible;

        private struct PreviewEntry
        {
            public Component TargetComponent;
            public CardTargetCrosshairPresenter Presenter;
        }

        private readonly struct TargetPreviewData
        {
            public ITarget Target { get; }
            public IReadOnlyList<EffectSO> Effects { get; }
            public CardDamagePreviewInfo DamageInfo { get; }

            public TargetPreviewData(ITarget target, IReadOnlyList<EffectSO> effects, CardDamagePreviewInfo damageInfo)
            {
                Target = target;
                Effects = effects;
                DamageInfo = damageInfo;
            }
        }

        private void Awake()
        {
            _card = GetComponent<Card>();
            _crosshairContainer = GameObject.FindGameObjectWithTag("DamageCrosshairsPreviewContainer")
                                            ?.GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            if (_card.Interactions is CardInteractions interactions)
            {
                interactions.OnHoverStart += OnHoverStart;
                interactions.OnHoverEnd += OnHoverEnd;
            }
        }

        private void OnDisable()
        {
            if (_card != null && _card.Interactions is CardInteractions interactions)
            {
                interactions.OnHoverStart -= OnHoverStart;
                interactions.OnHoverEnd -= OnHoverEnd;
            }

            HidePreview();
        }

        private void LateUpdate()
        {
            if (!_isPreviewVisible)
            {
                return;
            }

            UpdatePresenterPositions();
        }

        private void OnHoverStart(object sender, PointerEventData eventData)
        {
            ShowPreview();
        }

        private void OnHoverEnd(object sender, PointerEventData eventData)
        {
            HidePreview();
        }

        private void ShowPreview()
        {
            HidePreview();

            if (_crosshairPresenterPrefab == null)
            {
                Debug.LogWarning($"CardTargetingPreview on '{name}' has no crosshair presenter prefab assigned.");
                return;
            }

            RectTransform container = ResolveCrosshairContainer();
            if (container == null)
            {
                Debug.LogWarning($"CardTargetingPreview on '{name}' could not resolve a container RectTransform.");
                return;
            }

            IReadOnlyList<TargetPreviewData> previewTargets = ResolvePreviewTargets();

            foreach (TargetPreviewData data in previewTargets)
            {
                if (data.Target is not Component targetComponent)
                {
                    continue;
                }

                CardTargetCrosshairPresenter presenter = Instantiate(_crosshairPresenterPrefab, container);
                presenter.Initialize(new CardTargetCrosshairPresenterConfig(data.Effects, data.DamageInfo));

                _activeEntries.Add(new PreviewEntry
                {
                    TargetComponent = targetComponent,
                    Presenter = presenter
                });
            }

            _isPreviewVisible = _activeEntries.Count > 0;
            UpdatePresenterPositions();
        }

        private void HidePreview()
        {
            for (int i = 0; i < _activeEntries.Count; i++)
            {
                if (_activeEntries[i].Presenter != null)
                {
                    Destroy(_activeEntries[i].Presenter.gameObject);
                }
            }

            _activeEntries.Clear();
            _isPreviewVisible = false;
        }

        private IReadOnlyList<TargetPreviewData> ResolvePreviewTargets()
        {
            var orderedTargets = new List<ITarget>();
            var targetEffects = new Dictionary<ITarget, List<EffectSO>>();
            var targetDamageInfos = new Dictionary<ITarget, CardDamagePreviewInfo>();

            AddDamageTargets(orderedTargets, targetEffects, targetDamageInfos);
            AddEffectTargets(orderedTargets, targetEffects);

            var result = new List<TargetPreviewData>(orderedTargets.Count);
            for (int i = 0; i < orderedTargets.Count; i++)
            {
                ITarget target = orderedTargets[i];
                targetEffects.TryGetValue(target, out List<EffectSO> effects);
                targetDamageInfos.TryGetValue(target, out CardDamagePreviewInfo damageInfo);
                result.Add(new TargetPreviewData(target, effects ?? new List<EffectSO>(), damageInfo));
            }

            return result;
        }

        private void AddDamageTargets(List<ITarget> orderedTargets, Dictionary<ITarget, List<EffectSO>> targetEffects, Dictionary<ITarget, CardDamagePreviewInfo> targetDamageInfos)
        {
            CardDamageSO damageConfig = _card.Config?.Damage;
            if (damageConfig == null)
            {
                return;
            }

            IReadOnlyList<CardDamageTargetSelection> targetSelections = _cardTargetResolver.ResolveDamageTargets(_targetsProvider, _card.Config);

            foreach (CardDamageTargetSelection targetSelection in targetSelections)
            {
                if (targetSelection.Target == null)
                {
                    continue;
                }

                AddTargetIfNeeded(targetSelection.Target, orderedTargets, targetEffects);
                int modifiedDamage = CardDamage.GetModifiedDamageByStatusEffects(targetSelection.Target, damageConfig.DamageValue, _card.Config.Element);
                targetDamageInfos[targetSelection.Target] = new CardDamagePreviewInfo(modifiedDamage, targetSelection.HitCount);
            }
        }

        private void AddEffectTargets(List<ITarget> orderedTargets, Dictionary<ITarget, List<EffectSO>> targetEffects)
        {
            if (_card.Config?.Effects == null || _card.Config.Effects.Count == 0)
            {
                return;
            }

            ITarget effectTarget = _cardTargetResolver.ResolveEffectTarget(_targetsProvider, _card.Config);
            if (effectTarget == null)
            {
                return;
            }

            AddTargetIfNeeded(effectTarget, orderedTargets, targetEffects);

            if (!targetEffects.TryGetValue(effectTarget, out List<EffectSO> effectsForTarget))
            {
                effectsForTarget = new List<EffectSO>();
                targetEffects[effectTarget] = effectsForTarget;
            }

            effectsForTarget.AddRange(_card.Config.Effects.Where(effect => effect != null));
        }

        private static void AddTargetIfNeeded(ITarget target, List<ITarget> orderedTargets, Dictionary<ITarget, List<EffectSO>> targetEffects)
        {
            if (target == null)
            {
                return;
            }

            if (!orderedTargets.Contains(target))
            {
                orderedTargets.Add(target);
            }

            if (!targetEffects.ContainsKey(target))
            {
                targetEffects[target] = new List<EffectSO>();
            }
        }

        private void UpdatePresenterPositions()
        {
            RectTransform container = ResolveCrosshairContainer();
            if (container == null)
            {
                return;
            }

            for (int i = 0; i < _activeEntries.Count; i++)
            {
                PreviewEntry entry = _activeEntries[i];

                if (entry.Presenter == null || entry.TargetComponent == null)
                {
                    continue;
                }

                Transform anchorTransform = ResolveTargetAnchor(entry.TargetComponent.transform);
                Vector3 worldCenter = GetTargetVisualCenter(anchorTransform);
                Camera projectionCamera = ResolveProjectionCamera(anchorTransform);

                if (TryToLocalCanvasPosition(container, worldCenter, projectionCamera, out Vector2 localPoint))
                {
                    entry.Presenter.SetAnchoredPosition(localPoint);
                }
            }
        }

        private RectTransform ResolveCrosshairContainer()
        {
            if (_crosshairContainer != null)
            {
                return _crosshairContainer;
            }

            return _uiTransformsProvider?.FrontPanel;
        }

        private Transform ResolveTargetAnchor(Transform targetTransform)
        {
            if (targetTransform.TryGetComponent(out Enemies.Base.EnemyBase enemy) && enemy.Visual != null)
            {
                return enemy.Visual.transform;
            }

            return targetTransform;
        }

        private bool TryToLocalCanvasPosition(RectTransform container, Vector3 worldPosition, Camera projectionCamera, out Vector2 localPoint)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(projectionCamera, worldPosition);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(container, screenPoint, GetUICamera(container), out localPoint);
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

        private static Vector3 GetTargetVisualCenter(Transform targetTransform)
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

        private Camera GetUICamera(RectTransform container)
        {
            Canvas canvas = container.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return canvas.worldCamera != null ? canvas.worldCamera : _mainCamera;
        }
    }
}
