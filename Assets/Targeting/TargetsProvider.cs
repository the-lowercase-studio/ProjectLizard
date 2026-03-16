using Assets.CustomTypes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Targeting
{
    public interface ITargetsProvider
    {
        IEnumerable<ITarget> GetAll();

        ITarget GetFirst();

        ITarget GetLast();

        ITarget GetClosestByRandomDirection(ITarget target);

        ITarget GetClosestByDirection(ITarget target, HorizontalDirection horizontalDirection);

        IEnumerable<ITarget> GetFromStartPosition(
            StartPosition startPosition = StartPosition.Start, int count = 1);
    }

    public class TargetsProvider : MonoBehaviour, ITargetsProvider
    {
        //TODO: Add actual caching of targets based on enemies container / spawner events
        [SerializeField] private GameObject _enemiesContainer;

        public IEnumerable<ITarget> GetAll()
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

        public IEnumerable<ITarget> GetFromStartPosition(
            StartPosition startPosition = StartPosition.Start, int count = 1)
        {
            if (count <= 0)
            {
                return Enumerable.Empty<ITarget>();
            }

            var targets = _enemiesContainer.transform.GetComponentsInChildren<ITarget>();

            return startPosition switch
            {
                StartPosition.Start => GetFromStart(targets, count),
                StartPosition.End => GetFromEnd(targets, count),
                StartPosition.Center => GetFromCenter(targets, count)
            };
        }

        private IEnumerable<ITarget> GetFromStart(ITarget[] targets, int count = 1)
        {
            return targets.Take(count);
        }

        private IEnumerable<ITarget> GetFromEnd(ITarget[] targets, int count = 1)
        {
            return targets.Skip(Mathf.Max(0, targets.Length - count));
        }

        private IEnumerable<ITarget> GetFromCenter(ITarget[] targets, int count = 1)
        {
            var centerIndex = targets.Length / 2;
            var startIndex = Mathf.Max(0, centerIndex - count / 2);
            return targets.Skip(startIndex).Take(count);
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
