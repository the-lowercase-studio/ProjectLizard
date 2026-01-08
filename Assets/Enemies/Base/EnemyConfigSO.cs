using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Config", menuName = "Scriptable Objects/Enemy/Enemy Config")]
public class EnemyConfigSO : ScriptableObject
{
    [Header("Basic Information")]
    [field: SerializeField] public string Name { get; private set; }

    [field: SerializeField][TextArea(2, 4)] public string Description { get; private set; }

    [Header("Combat Stats")]
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;

    [field: SerializeField] public int BaseDamage { get; private set; } = 10;

    [Header("Visual")]
    [field: SerializeField] public Sprite Sprite { get; private set; }

    //[field: SerializeField] public RuntimeAnimatorController AnimatorController { get; private set; }

    //[Header("Elemental Properties")]
    //[field: SerializeField] public Elements ElementalAffinity { get; private set; } = Elements.Physical;

    //[field: SerializeField][Range(0f, 1f)] public float ElementalResistance { get; private set; } = 0f;
    //[field: SerializeField][Range(0f, 2f)] public float ElementalWeakness { get; private set; } = 1f;
}
