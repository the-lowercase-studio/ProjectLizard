using Assets.Cards.CardsHand;
using Assets.Energy;
using Assets.Inputs;
using Assets.Inputs.Pointer;
using Assets.Targeting;
using Assets.Turns;
using Assets.UI;
using Reflex.Core;
using UnityEngine;

namespace Assets.Installers
{
    public class SceneInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private TurnManager _turnManager;
        [SerializeField] private EnergyManager _energyManager;
        [SerializeField] private CardsHandManager _cardsHandManager;
        [SerializeField] private CardsHandPresenter _cardsHandPresenter;
        [SerializeField] private TargetsManager _targetsManager;
        [SerializeField] private PlayerParty _playerParty;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private UITransformsProvider _uiTransformsProvider;
        [SerializeField] private PointerPositioner _pointerPositioner;

        public void InstallBindings(ContainerBuilder builder)
        {
            builder.RegisterValue(_turnManager, new[] { typeof(ITurnManager) });
            builder.RegisterValue(_energyManager, new[] { typeof(IEnergyManager) });
            builder.RegisterValue(_cardsHandManager, new[] { typeof(ICardsHandManager) });
            builder.RegisterValue(_cardsHandPresenter, new[] { typeof(ICardsHandPresenter) });
            builder.RegisterValue(_targetsManager, new[] { typeof(ITargetsManager) });
            builder.RegisterValue(_playerParty, new[] { typeof(IPlayerParty) });
            builder.RegisterValue(_inputHandler, new[] { typeof(IInputHandler) });
            builder.RegisterValue(_uiTransformsProvider, new[] { typeof(IUITransformsProvider) });
            builder.RegisterValue(_pointerPositioner, new[] { typeof(IPointerPositioner) });
        }
    }
}
