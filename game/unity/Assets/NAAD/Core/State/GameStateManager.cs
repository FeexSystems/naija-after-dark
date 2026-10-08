using NAAD.Core.Logging;
using NAAD.Networking;

namespace NAAD.Core.State
{
    /// <summary>
    /// Holds client-side cache of server-authoritative data.
    /// Never treats local values as source of truth for money or progression.
    /// </summary>
    public sealed class GameStateManager
    {
        private readonly INAADLogger _log;

        public GamePhase Phase { get; private set; } = GamePhase.Boot;
        public PlayerDto? Player { get; private set; }
        public WorldStateDto? WorldState { get; private set; }
        public string? LastError { get; private set; }

        public GameStateManager(INAADLogger log)
        {
            _log = log;
        }

        public void SetPhase(GamePhase phase)
        {
            Phase = phase;
            _log.Info("State", $"Phase → {phase}");
        }

        public void SetPlayer(PlayerDto? player)
        {
            Player = player;
            if (player != null)
                _log.Info("State", $"Player cached: {player.DisplayName} ({player.Id})");
        }

        public void SetWorldState(WorldStateDto? world)
        {
            WorldState = world;
            if (world != null)
                _log.Info("State", $"World cached: weather={world.Weather} time={world.GameTime}");
        }

        public void SetError(string message)
        {
            LastError = message;
            Phase = GamePhase.Failed;
            _log.Error("State", message);
        }
    }
}
