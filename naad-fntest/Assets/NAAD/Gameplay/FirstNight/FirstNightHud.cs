using UnityEngine;

namespace NAAD.Gameplay.FirstNight
{
    /// <summary>
    /// Minimal IMGUI driver for Phase B First Night loop.
    /// </summary>
    public sealed class FirstNightHud : MonoBehaviour
    {
        [SerializeField] private FirstNightController controller;

        private void Awake()
        {
            if (controller == null)
                controller = GetComponent<FirstNightController>();
        }

        private void OnGUI()
        {
            if (controller == null) return;

            const int w = 420;
            var x = 12;
            var y = 12;

            GUI.Box(new Rect(x, y, w, 360), "NAAD — First Night (Phase B)");
            y += 28;
            GUI.Label(new Rect(x + 10, y, w - 20, 40), "Step: " + controller.Step);
            y += 24;
            GUI.Label(new Rect(x + 10, y, w - 20, 60), controller.StatusMessage ?? "");
            y += 56;

            if (controller.LastBalance.HasValue)
            {
                GUI.Label(new Rect(x + 10, y, w - 20, 22), "Wallet: NGN " + controller.LastBalance.Value);
                y += 24;
            }

            GUI.enabled = !controller.IsBusy;

            if (GUI.Button(new Rect(x + 10, y, 180, 28), "1. Start Night"))
                _ = controller.StartNightAsync();
            y += 32;

            if (GUI.Button(new Rect(x + 10, y, 90, 28), "GO"))
                _ = controller.RespondToInviteAsync("GO");
            if (GUI.Button(new Rect(x + 110, y, 110, 28), "ASK DETAILS"))
                _ = controller.RespondToInviteAsync("ASK_DETAILS");
            if (GUI.Button(new Rect(x + 230, y, 100, 28), "DECLINE"))
                _ = controller.RespondToInviteAsync("DECLINE");
            y += 32;

            if (GUI.Button(new Rect(x + 10, y, 180, 28), "3. Suya Spot + Buy"))
                _ = controller.GoToSuyaAndBuyAsync();
            y += 32;

            if (GUI.Button(new Rect(x + 10, y, 180, 28), "4. Club + Ticket"))
                _ = controller.GoToClubAndBuyTicketAsync();
            y += 32;

            if (GUI.Button(new Rect(x + 10, y, 180, 28), "5. Complete Night"))
                _ = controller.CompleteNightAsync();
            y += 36;

            if (GUI.Button(new Rect(x + 10, y, 220, 32), "Run Full Night (GO)"))
                _ = controller.RunFullNightAsync("GO");

            GUI.enabled = true;

            if (!string.IsNullOrEmpty(controller.LastError))
            {
                y += 40;
                var c = GUI.color;
                GUI.color = Color.red;
                GUI.Label(new Rect(x + 10, y, w - 20, 40), controller.LastError);
                GUI.color = c;
            }
        }
    }
}
