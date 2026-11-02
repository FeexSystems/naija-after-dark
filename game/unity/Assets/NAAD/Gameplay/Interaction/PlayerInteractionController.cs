using System;
using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Core.Logging;
using NAAD.Networking;
using NAAD.World;
using UnityEngine;

namespace NAAD.Gameplay.Interaction
{
    /// <summary>
    /// Gate D — script-driven interaction over world <see cref="SceneAnchor"/>s.
    /// Movement remains on PlayerLocomotionController; this only probes and
    /// dispatches authoritative commands (never mutates wallet/location locally).
    ///
    /// Anchors are the <c>NAAD.World.SceneAnchor</c> markers that NaadScenePipeline
    /// authors into each scene. Their contentId is translated by
    /// <see cref="SceneAnchorCommandMap"/> — there is no second anchor type.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerInteractionController : MonoBehaviour
    {
        [SerializeField] private float pollRadius = 3.0f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private bool interactOnFire1 = true;

        public SceneAnchor CurrentAnchor { get; private set; }
        public string Status { get; private set; } = "";
        public bool IsBusy { get; private set; }

        public event Action Changed;

        private NAADApplicationRoot Root => NAADApplicationRoot.Instance;
        private INAADLogger Log => Root != null ? Root.Logger : null;

        private void Update()
        {
            if (IsBusy) return;
            RefreshNearestAnchor();
            var pressed = UnityEngine.Input.GetKeyDown(interactKey)
                          || (interactOnFire1 && UnityEngine.Input.GetButtonDown("Fire1"));
            if (pressed && CurrentAnchor != null)
                _ = InteractAsync(CurrentAnchor);
        }

        private void RefreshNearestAnchor()
        {
            SceneAnchor best = null;
            var bestDist = float.MaxValue;
            var anchors = FindObjectsByType<SceneAnchor>(FindObjectsSortMode.None);
            var pos = transform.position;
            foreach (var anchor in anchors)
            {
                if (anchor == null) continue;

                // Spawn points and camera rigs are positioning markers, not interactions.
                if (anchor.Kind == SceneAnchor.AnchorKind.PlayerSpawn ||
                    anchor.Kind == SceneAnchor.AnchorKind.CameraRig) continue;

                if (!SceneAnchorCommandMap.TryResolve(anchor, out _)) continue;

                var d = Vector3.Distance(pos, anchor.transform.position);
                if (d <= pollRadius && d < bestDist)
                {
                    best = anchor;
                    bestDist = d;
                }
            }

            if (best != CurrentAnchor)
            {
                CurrentAnchor = best;
                Status = best != null
                    ? (string.IsNullOrEmpty(best.DisplayName) ? best.ContentId : best.DisplayName)
                    : "";
                Changed?.Invoke();
            }
        }

        public async Task<bool> InteractAsync(SceneAnchor anchor)
        {
            if (anchor == null || Root == null || Root.Commands == null)
            {
                Status = "Interaction unavailable";
                Changed?.Invoke();
                return false;
            }

            if (!SceneAnchorCommandMap.TryResolve(anchor, out var command))
            {
                Status = "No command mapped for " + anchor.ContentId;
                Changed?.Invoke();
                return false;
            }

            IsBusy = true;
            Status = "Interacting: " + anchor.DisplayName;
            Changed?.Invoke();

            try
            {
                switch (command.Kind)
                {
                    case InteractionKind.Travel:
                        return await DispatchTravel(command.Payload);
                    case InteractionKind.Spend:
                        return await DispatchSpend(command.Payload);
                    case InteractionKind.AttendEvent:
                        return await DispatchAttend(command.Payload);
                    case InteractionKind.Talk:
                        if (Root.Dialogue == null)
                        {
                            Status = "Dialogue service missing";
                            return false;
                        }
                        var prompt = "Hey — " + (string.IsNullOrEmpty(anchor.DisplayName) ? "how far?" : anchor.DisplayName);
                        var talk = await Root.Dialogue.TalkToTundeAsync(prompt, Guid.NewGuid().ToString("N"));
                        Status = talk != null && talk.Success
                            ? (talk.Dialogue ?? "…")
                            : (talk?.ErrorCode ?? "Talk failed");
                        return talk != null && talk.Success;
                    case InteractionKind.Examine:
                        Status = "Examined " + anchor.DisplayName;
                        return true;
                    default:
                        Status = "No interaction";
                        return false;
                }
            }
            finally
            {
                IsBusy = false;
                Changed?.Invoke();
            }
        }

        private async Task<bool> DispatchTravel(string locationId)
        {
            if (string.IsNullOrEmpty(locationId))
            {
                Status = "Travel anchor missing destination";
                return false;
            }

            var cmd = new GameCommand
            {
                RequestId = Guid.NewGuid().ToString("N"),
                Type = GameCommandType.Travel,
                Payload = { ["toLocationId"] = locationId }
            };
            Log?.Info("Interaction", "requestId=" + cmd.RequestId + " type=TRAVEL");
            var result = await Root.Commands.ExecuteAsync(cmd);
            Status = result.Success ? "Traveled" : (result.ErrorCode ?? "Travel failed");
            return result.Success;
        }

        private async Task<bool> DispatchSpend(string sku)
        {
            if (string.IsNullOrEmpty(sku))
            {
                Status = "Spend anchor missing sku";
                return false;
            }

            var cmd = new GameCommand
            {
                RequestId = Guid.NewGuid().ToString("N"),
                Type = GameCommandType.Spend,
                Payload = { ["sku"] = sku }
            };
            Log?.Info("Interaction", "requestId=" + cmd.RequestId + " type=SPEND");
            var result = await Root.Commands.ExecuteAsync(cmd);
            Status = result.Success ? "Spent " + sku : (result.ErrorCode ?? "Spend failed");
            return result.Success;
        }

        private async Task<bool> DispatchAttend(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                Status = "Event anchor missing eventId";
                return false;
            }

            var cmd = new GameCommand
            {
                RequestId = Guid.NewGuid().ToString("N"),
                Type = GameCommandType.AttendEvent,
                Payload = { ["eventId"] = eventId }
            };
            Log?.Info("Interaction", "requestId=" + cmd.RequestId + " type=ATTEND_EVENT");
            var result = await Root.Commands.ExecuteAsync(cmd);
            Status = result.Success ? "Attended" : (result.ErrorCode ?? "Attend failed");
            return result.Success;
        }
    }
}
