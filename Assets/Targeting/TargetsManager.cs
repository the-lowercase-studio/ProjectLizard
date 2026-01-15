using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Targeting
{
    public class TargetsManager : MonoBehaviour
    {
        public static TargetsManager Instance;

        //TODO: Change to some kind of retrieve from Enemies Spawning Manager
        [SerializeField] private GameObject _enemiesContainer;

        private TargetsManager()
        { }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public IEnumerable<ITarget> GetTargets(TargetsMode mode)
        {
            return new List<ITarget>() { _enemiesContainer.transform.GetComponentsInChildren<ITarget>().First() };
        }
    }
}
