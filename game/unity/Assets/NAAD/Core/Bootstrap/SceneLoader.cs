using System.Threading.Tasks;
using NAAD.Core.Logging;
using UnityEngine.SceneManagement;

namespace NAAD.Core.Bootstrap
{
    public interface ISceneLoader
    {
        Task LoadSceneAsync(string sceneName);
    }

    public sealed class SceneLoader : ISceneLoader
    {
        private readonly INAADLogger _log;

        public SceneLoader(INAADLogger log)
        {
            _log = log;
        }

        public async Task LoadSceneAsync(string sceneName)
        {
            _log.Info("Scene", $"Loading scene '{sceneName}'");
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (op == null)
            {
                _log.Warn("Scene", $"LoadSceneAsync returned null for '{sceneName}' (scene may be missing from Build Settings). Skipping.");
                return;
            }

            while (!op.isDone)
                await Task.Yield();

            _log.Info("Scene", $"Scene '{sceneName}' loaded");
        }
    }
}
