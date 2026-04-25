using Assets.Cards.Base.Damage;
using Assets.Cards.Base.Targeting;
using Assets.Targeting;
using Assets.UI;
using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Assets.Cards.Base.Interaction;
using Assets.Cards.Base;
using Assets.Cards.TargetingPreview.UI;

namespace Assets.Cards.TargetingPreview
{
    [RequireComponent(typeof(Card))]
    public class CardTargetingPreview : MonoBehaviour
    {
        [Inject] private readonly ITargetsProvider _targetsProvider;
        [Inject] private readonly ICardTargetResolver _cardTargetResolver;
        [Inject] private readonly IUITransformsProvider _uiTransformsProvider;
        [Inject] private readonly Camera _mainCamera;
        [Inject] private readonly ICardDragLock _cardDragLock;

        [SerializeField] private CardTargetCrosshairPresenter _crosshairPresenterPrefab;
        private RectTransform _crosshairContainer;

        private readonly List<PreviewEntry> _activeEntries = new();

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
            public IReadOnlyList<CardTargetEffectPreview> Effects { get; }
            public CardDamagePreviewInfo DamageInfo { get; }

            public TargetPreviewData(ITarget target, IReadOnlyList<CardTargetEffectPreview> effects, CardDamagePreviewInfo damageInfo)
            {
                Target = target;
                Effects = effects;
                DamageInfo = damageInfo;
            }
        }

        private bool _isHovered;
        private bool _isDragged;
        private bool _isShowingPreview;

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
                interactions.OnDragStart += OnDragStart;
                interactions.OnDragEnd += OnDragEnd;
            }
        }

        private void OnDisable()
        {
            if (_card != null && _card.Interactions is CardInteractions interactions)
            {
                interactions.OnHoverStart -= OnHoverStart;
                interactions.OnHoverEnd -= OnHoverEnd;
                interactions.OnDragStart -= OnDragStart;
                interactions.OnDragEnd -= OnDragEnd;
            }

            _isHovered = false;
            _isDragged = false;
            _isShowingPreview = false;

            HidePreview();

            _card.ClearCachedAttackPlan();
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
            _isHovered = true;
            EvaluateVisibility();
        }

        private void OnHoverEnd(object sender, PointerEventData eventData)
        {
            _isHovered = false;
            EvaluateVisibility();
        }

        private void OnDragStart(object sender, PointerEventData eventData)
        {
            _isDragged = true;
            EvaluateVisibility();
        }

        private void OnDragEnd(object sender, PointerEventData eventData)
        {
            _isDragged = false;
            EvaluateVisibility();
        }

        private void EvaluateVisibility()
        {
            bool shouldShow = _isDragged || _isHovered && (_cardDragLock == null || !_cardDragLock.IsAnyCardBeingDragged);

            if (shouldShow && !_isShowingPreview)
            {
                _isShowingPreview = true;
                ShowPreview();
            }
            else if (!shouldShow && _isShowingPreview)
            {
                _isShowingPreview = false;
                HidePreview();
            }
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

            IReadOnlyList<CardResolvedHit> attackPlan = _card.CachedAttackPlan;
            if (attackPlan == null || attackPlan.Count == 0)
            {
                attackPlan = _cardTargetResolver.ResolveAttackHits(_targetsProvider, _card.Config);
                _card.SetCachedAttackPlan(attackPlan);
            }

            IReadOnlyList<TargetPreviewData> previewTargets = ResolvePreviewTargets(attackPlan);

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

        private IReadOnlyList<TargetPreviewData> ResolvePreviewTargets(IReadOnlyList<CardResolvedHit> attackPlan)
        {
            var orderedTargets = new List<ITarget>();
            var targetEffects = new Dictionary<ITarget, List<CardTargetEffectPreview>>();
            var targetDamageTotals = new Dictionary<ITarget, int>();
            var targetHitCounts = new Dictionary<ITarget, int>();

            AddPreviewHits(attackPlan, orderedTargets, targetEffects, targetDamageTotals, targetHitCounts);

            var result = new List<TargetPreviewData>(orderedTargets.Count);
            for (int i = 0; i < orderedTargets.Count; i++)
            {
                ITarget target = orderedTargets[i];
                targetEffects.TryGetValue(target, out List<CardTargetEffectPreview> effects);
                targetDamageTotals.TryGetValue(target, out int totalDamage);
                targetHitCounts.TryGetValue(target, out int hitCount);
                CardDamagePreviewInfo damageInfo = new(totalDamage, hitCount);
                result.Add(new TargetPreviewData(target, effects ?? new List<CardTargetEffectPreview>(), damageInfo));
            }

            return result;
        }

        private void AddPreviewHits(IReadOnlyList<CardResolvedHit> attackPlan, List<ITarget> orderedTargets, Dictionary<ITarget, List<CardTargetEffectPreview>> targetEffects, Dictionary<ITarget, int> targetDamageTotals, Dictionary<ITarget, int> targetHitCounts)
        {
            if (attackPlan == null)
            {
                return;
            }

            for (int hitIndex = 0; hitIndex < attackPlan.Count; hitIndex++)
            {
                CardResolvedHit hit = attackPlan[hitIndex];
                if (hit.Target == null || hit.Step?.Damage == null || !CardDamage.IsTargetAlive(hit.Target))
                {
                    continue;
                }

                AddTargetIfNeeded(hit.Target, orderedTargets, targetEffects);

                int modifiedDamage = CardDamage.GetModifiedDamageByStatusEffects(hit.Target, hit.Step.Damage.DamageValue, _card.Config.Element);
                targetDamageTotals.TryGetValue(hit.Target, out int totalDamage);
                targetDamageTotals[hit.Target] = totalDamage + modifiedDamage;

                targetHitCounts.TryGetValue(hit.Target, out int hitCount);
                targetHitCounts[hit.Target] = hitCount + 1;

                if (hit.Step.Effect != null && hit.Step.Effect.HasVisuals)
                {
                    AddEffectIfNeeded(targetEffects[hit.Target], new CardTargetEffectPreview(hit.Step.Effect, hit.Step.GetClampedEffectChance()));
                }
            }
        }

        private static void AddTargetIfNeeded(ITarget target, List<ITarget> orderedTargets, Dictionary<ITarget, List<CardTargetEffectPreview>> targetEffects)
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
                targetEffects[target] = new List<CardTargetEffectPreview>();
            }
        }

        private static void AddEffectIfNeeded(List<CardTargetEffectPreview> effects, CardTargetEffectPreview preview)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].Effect == preview.Effect && Mathf.Approximately(effects[i].Chance, preview.Chance))
                {
                    return;
                }
            }

            effects.Add(preview);
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

            return canvas.worldCamera ?? _mainCamera;
        }
    }
}
