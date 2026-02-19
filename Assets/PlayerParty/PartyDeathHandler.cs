using Assets.Audio;
using Assets.Scripts.DeathHandlers;
using Assets.Scripts.HealthSystem;
using UnityEngine;

[RequireComponent(typeof(PlayerParty))]
public class PartyDeathHandler : DeathHandlerBase
{
    private PlayerParty _party;

    protected override IHealth Health => _party.Health;
    protected override IAudioClipPlayer AudioClipPlayer => _party.AudioClipPlayer;

    private void Awake()
    {
        _party = GetComponent<PlayerParty>();
    }

    protected override void HandleDeath()
    {
        foreach (var character in _party.GetAllCharacters())
        {
            character.gameObject.SetActive(false);
        }
    }
}
