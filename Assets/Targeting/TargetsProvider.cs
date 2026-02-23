using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Targeting
{
    public interface ITargetsProvider
    {
        IEnumerable<ITarget> GetAll(TargetsMode mode);

        ITarget GetFirst();

        ITarget GetLast();

        IEnumerable<ITarget> GetFromStart(int count);

        IEnumerable<ITarget> GetFromEnd(int count);

        IEnumerable<ITarget> GetRandom(int count);

        ITarget GetClosest(ITarget target);
    }

    public class TargetsProvider : MonoBehaviour, ITargetsProvider
    {
        //TODO: Add actual caching of targets based on enemies container / spawner events
        [SerializeField] private GameObject _enemiesContainer;

        public IEnumerable<ITarget> GetAll(TargetsMode mode)
        {
            return _enemiesContainer.transform.GetComponentsInChildren<ITarget>();
        }

        public ITarget GetFirst()
        {
            var targets = _enemiesContainer.transform.GetComponentsInChildren<ITarget>();
            return targets.FirstOrDefault();
        }

        public ITarget GetLast()
        {
            var targets = _enemiesContainer.transform.GetComponentsInChildren<ITarget>();
            return targets.LastOrDefault();
        }

        public IEnumerable<ITarget> GetFromStart(int count)
        {
            var targets = _enemiesContainer.transform.GetComponentsInChildren<ITarget>();
            return targets.Take(count);
        }

        public IEnumerable<ITarget> GetFromEnd(int count)
        {
            var targets = _enemiesContainer.transform.GetComponentsInChildren<ITarget>();
            return targets.Skip(Mathf.Max(0, targets.Length - count));
        }

        public IEnumerable<ITarget> GetRandom(int count)
        {
            var targets = _enemiesContainer.transform.GetComponentsInChildren<ITarget>();
            return targets.OrderBy(x => Random.value).Take(count);
        }

        public ITarget GetClosest(ITarget target)
        {
            var targets = _enemiesContainer.transform.GetComponentsInChildren<ITarget>();

            if (targets.Length <= 1)
                return null;

            var index = System.Array.IndexOf(targets, target);

            if (index == -1)
                return null;

            if (index == 0)
                return targets[1];

            if (index == targets.Length - 1)
                return targets[index - 1];

            return Random.value < 0.5f ? targets[index - 1] : targets[index + 1];
        }
    }
}
