using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Networking;

namespace NAAD.UI
{
    /// <summary>
    /// Controller for night summary screen.
    /// Displays end-of-night statistics and achievements.
    /// </summary>
    public class NightSummaryController : BaseScreenController
    {
        private IPhoneService? _phoneService;
        private NightSummaryData _data = new();
        private string _currentRequestId = string.Empty;

        protected override void Initialize()
        {
            var root = NAADApplicationRoot.Instance;
            if (root == null) return;

            _phoneService = root.Phone;
        }

        protected override async void LoadData()
        {
            SetState(ScreenState.Loading);

            try
            {
                if (_phoneService == null)
                {
                    SetState(ScreenState.Error, "Phone service not available");
                    return;
                }

                // Complete night and get summary
                _currentRequestId = System.Guid.NewGuid().ToString();
                var summary = await _phoneService.CompleteNightAsync(_currentRequestId);

                if (summary == null)
                {
                    SetState(ScreenState.Error, "Failed to retrieve night summary");
                    return;
                }

                // Map service DTO to UI data contract
                _data.Success = summary.Success;
                _data.SpentNgn = summary.SpentNgn;
                _data.Transactions = summary.Transactions;
                _data.PeopleMet = summary.PeopleMet;
                _data.NewConnection = summary.NewConnection ?? string.Empty;
                _data.TrustWithTunde = summary.TrustWithTunde;
                _data.BestMoment = summary.BestMoment;
                _data.Duration = "4 hours"; // TODO: Calculate from timestamps
                _data.EndTime = System.DateTime.Now.ToString("h:mm tt");

                if (summary.Success)
                {
                    SetState(ScreenState.Success);
                }
                else
                {
                    SetState(ScreenState.Error, "Night completion failed");
                }
            }
            catch (System.Exception ex)
            {
                SetState(ScreenState.Error, $"Failed to load night summary: {ex.Message}");
            }
        }

        protected override void OnStateChanged(ScreenState newState, ScreenState oldState)
        {
            switch (newState)
            {
                case ScreenState.Loading:
                    ShowLoadingView();
                    break;
                case ScreenState.Success:
                    ShowSuccessView();
                    break;
                case ScreenState.Error:
                    ShowErrorView();
                    break;
            }
        }

        private void ShowLoadingView()
        {
            // Show loading spinner
            // Display "Calculating your night..."
        }

        private void ShowSuccessView()
        {
            // Show night summary cards
            // Display statistics
            // Show achievements
            // Display "Continue" button
        }

        private void ShowErrorView()
        {
            // Show error message
            // Display retry button
            // Display "Skip" option
        }

        /// <summary>
        /// Get current night summary data for UI binding.
        /// </summary>
        public NightSummaryData GetData()
        {
            return _data;
        }

        /// <summary>
        /// Retry night completion.
        /// </summary>
        public void Retry()
        {
            SetState(ScreenState.Retry);
            LoadData();
        }

        /// <summary>
        /// Skip night summary and continue.
        /// </summary>
        public void Skip()
        {
            // Navigate to next screen
            // TODO: Implement navigation
        }

        /// <summary>
        /// Continue to next screen after viewing summary.
        /// </summary>
        public void Continue()
        {
            // Navigate to next screen
            // TODO: Implement navigation
        }

        /// <summary>
        /// Share night summary (optional feature).
        /// </summary>
        public void ShareSummary()
        {
            // Open share dialog
            // TODO: Implement sharing
        }
    }
}
