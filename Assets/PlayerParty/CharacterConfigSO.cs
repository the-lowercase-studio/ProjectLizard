using Assets.ElementalSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Config", menuName = "Scriptable Objects/Character/Character Config")]
public class CharacterConfigSO : ScriptableObject
{
    [Header("Basic Information")]
    [field: SerializeField] public string Name { get; private set; }

    [field: SerializeField][TextArea(2, 4)] public string Description { get; private set; }

    [Header("Combat Stats")]
    [field: SerializeField] public int HealthContribution { get; private set; } = 50;

    [field: SerializeField] public Elements Element { get; private set; } = Elements.Physical;

    [Header("Visual")]
    [field: SerializeField] public Sprite Sprite { get; private set; }

    [field: SerializeField] public RuntimeAnimatorController AnimatorController { get; private set; }
}
