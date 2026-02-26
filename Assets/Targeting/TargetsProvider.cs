using Assets.CustomTypes;
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

        ITarget GetClosestByRandomDirection(ITarget target);

        ITarget GetClosestByDirection(ITarget target, HorizontalDirection horizontalDirection);
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

        public ITarget GetClosestByRandomDirection(ITarget target)
        {
            var targets = _enemiesContainer.transform.GetComponentsInChildren<ITarget>();

            if (targets.Length <= 1)
                return null;

            var index = System.Array.IndexOf(targets, target);

            if (index == -1)
                return null;

            HorizontalDirection direction;

            if (index == 0)
                direction = HorizontalDirection.Right;
            else if (index == targets.Length - 1)
                direction = HorizontalDirection.Left;
            else
                direction = Random.value < 0.5f ? HorizontalDirection.Left : HorizontalDirection.Right;

            return GetClosestByDirection(target, direction);
        }

        public ITarget GetClosestByDirection(ITarget target, HorizontalDirection horizontalDirection)
        {
            var targets = _enemiesContainer.transform.GetComponentsInChildren<ITarget>();

            if (targets.Length <= 1)
            {
                return null;
            }

            var targetIndex = System.Array.IndexOf(targets, target);

            if (targetIndex == -1)
            {
                return null;
            }

            return horizontalDirection switch
            {
                HorizontalDirection.Left => GetClosestFromLeft(targets, targetIndex),
                HorizontalDirection.Right => GetClosestFromRight(targets, targetIndex),
                _ => null,
            };
        }

        private ITarget GetClosestFromRight(ITarget[] targets, int targetIndex)
        {
            return (targetIndex >= 0 && targetIndex < targets.Length - 1) ? targets[targetIndex + 1] : null;
        }

        private ITarget GetClosestFromLeft(ITarget[] targets, int targetIndex)
        {
            return (targetIndex > 0 && targetIndex < targets.Length) ? targets[targetIndex - 1] : null;
        }
    }
}
