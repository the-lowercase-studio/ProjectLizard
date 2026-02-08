using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Targeting
{
    public interface ITargetsManager
    {
        IEnumerable<ITarget> GetTargets(TargetsMode mode);
    }

    public class TargetsManager : MonoBehaviour, ITargetsManager
    {
        //TODO: Change to some kind of retrieve from Enemies Spawning Manager
        [SerializeField] private GameObject _enemiesContainer;

        public IEnumerable<ITarget> GetTargets(TargetsMode mode)
        {
            return new List<ITarget>() { _enemiesContainer.transform.GetComponentsInChildren<ITarget>().First() };
        }
    }
}
