using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.HealthSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.UI
{
    public interface IHitMaterialFlasher
    {
        void AddImageTarget(Image image);
    }

    public class HitMaterialFlasher : MonoBehaviour, IHitMaterialFlasher
    {
        private const float FLASH_DURATION = 0.24f;
        [SerializeField] private Health _health;
        [SerializeField] private Material _hitMaterial;
        [SerializeField] private List<Image> _imageTargets;

        private Material _standardMaterial;
        private Coroutine _flashCoroutine;

        private void Start()
        {
            _standardMaterial = _imageTargets[0].material;
        }

        private void OnEnable()
        {
            _health.OnHealthDecreased += HandleHealthDecreased;
            _health.OnNoHealth += HandleHealthDecreased;
        }

        private void OnDisable()
        {
            _health.OnHealthDecreased -= HandleHealthDecreased;
            _health.OnNoHealth -= HandleHealthDecreased;
        }

        public void AddImageTarget(Image image)
        {
            _imageTargets.Add(image);
        }

        private IEnumerator FlashRoutine()
        {
            for (int i = 0; i < _imageTargets.Count; i++)
            {
                _imageTargets[i].material = _hitMaterial;
            }

            yield return new WaitForSeconds(FLASH_DURATION);

            for (int i = 0; i < _imageTargets.Count; i++)
            {
                _imageTargets[i].material = _standardMaterial;
            }

            _flashCoroutine = null;
        }

        private void HandleHealthDecreased(object sender, EventArgs e)
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(FlashRoutine());
        }
    }
}
