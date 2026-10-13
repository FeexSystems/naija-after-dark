using System.Threading.Tasks;
using NAAD.Core.Bootstrap;
using NAAD.Networking;

namespace NAAD.UI
{
    /// <summary>
    /// Controller for Ping messaging screen.
    /// Displays contacts and messages with real-time updates.
    /// </summary>
    public class PingMessagingController : BaseScreenController
    {
        private IPhoneService? _phoneService;
        private PingMessagingData _data = new();

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

                // Load messages
                var messagesJson = await _phoneService.ListMessagesJsonAsync();
                
                if (string.IsNullOrEmpty(messagesJson))
                {
                    SetState(ScreenState.Empty);
                    return;
                }

                // Parse messages (TODO: Implement JSON parsing)
                ParseMessages(messagesJson);

                // Calculate total unread
                _data.TotalUnread = 0;
                foreach (var contact in _data.Contacts)
                {
                    _data.TotalUnread += contact.UnreadCount;
                }

                if (_data.Messages.Count == 0)
                {
                    SetState(ScreenState.Empty);
                }
                else
                {
                    SetState(ScreenState.Success);
                }
            }
            catch (System.Exception ex)
            {
                SetState(ScreenState.Error, $"Failed to load messages: {ex.Message}");
            }
        }

        private void ParseMessages(string messagesJson)
        {
            // TODO: Implement JSON parsing
            // For now, add sample data
            _data.Contacts = new System.Collections.Generic.List<ContactItem>
            {
                new ContactItem
                {
                    ContactId = "tunde",
                    DisplayName = "Tunde",
                    AvatarUrl = "",
                    UnreadCount = 2,
                    IsOnline = true,
                    LastSeen = "Online"
                }
            };

            _data.Messages = new System.Collections.Generic.List<MessageItem>
            {
                new MessageItem
                {
                    MessageId = "msg1",
                    SenderId = "tunde",
                    SenderName = "Tunde",
                    Content = "Hey! Are you coming to the club tonight?",
                    Timestamp = "8:30 PM",
                    IsRead = false,
                    IsFromPlayer = false
                },
                new MessageItem
                {
                    MessageId = "msg2",
                    SenderId = "tunde",
                    SenderName = "Tunde",
                    Content = "It's going to be lit!",
                    Timestamp = "8:31 PM",
                    IsRead = false,
                    IsFromPlayer = false
                }
            };
        }

        protected override void OnStateChanged(ScreenState newState, ScreenState oldState)
        {
            switch (newState)
            {
                case ScreenState.Loading:
                    ShowLoadingView();
                    break;
                case ScreenState.Empty:
                    ShowEmptyView();
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
        }

        private void ShowEmptyView()
        {
            // Show empty state message
            // Display "No messages yet"
        }

        private void ShowSuccessView()
        {
            // Show contact list
            // Show message list for selected contact
            // Update unread badges
        }

        private void ShowErrorView()
        {
            // Show error message
            // Display retry button
        }

        /// <summary>
        /// Select a contact to view messages.
        /// </summary>
        public void SelectContact(string contactId)
        {
            _data.SelectedContactId = contactId;
            UpdateMessageList();
        }

        private void UpdateMessageList()
        {
            // Filter messages for selected contact
            // Update UI
        }

        /// <summary>
        /// Send a message.
        /// </summary>
        public async void SendMessage(string content)
        {
            if (string.IsNullOrEmpty(content) || _phoneService == null)
                return;

            try
            {
                var requestId = System.Guid.NewGuid().ToString();
                var messageId = System.Guid.NewGuid().ToString();
                
                // Add message to local list immediately
                var newMessage = new MessageItem
                {
                    MessageId = messageId,
                    SenderId = "player",
                    SenderName = "You",
                    Content = content,
                    Timestamp = System.DateTime.Now.ToString("h:mm tt"),
                    IsRead = true,
                    IsFromPlayer = true
                };
                
                _data.Messages.Add(newMessage);
                UpdateMessageList();

                // Send to service
                var success = await _phoneService.RespondAsync(requestId, messageId, "send");
                
                if (!success)
                {
                    // Remove message if send failed
                    _data.Messages.Remove(newMessage);
                    SetState(ScreenState.Error, "Failed to send message");
                }
            }
            catch (System.Exception ex)
            {
                SetState(ScreenState.Error, $"Failed to send message: {ex.Message}");
            }
        }

        /// <summary>
        /// Respond to a message.
        /// </summary>
        public async void RespondToMessage(string requestId, string messageId, string action)
        {
            if (_phoneService == null)
                return;

            try
            {
                var success = await _phoneService.RespondAsync(requestId, messageId, action);
                
                if (success)
                {
                    // Mark message as read
                    var message = _data.Messages.Find(m => m.MessageId == messageId);
                    if (message != null)
                    {
                        message.IsRead = true;
                        UpdateMessageList();
                    }
                }
                else
                {
                    SetState(ScreenState.Error, "Failed to respond to message");
                }
            }
            catch (System.Exception ex)
            {
                SetState(ScreenState.Error, $"Failed to respond: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current messaging data for UI binding.
        /// </summary>
        public PingMessagingData GetData()
        {
            return _data;
        }

        /// <summary>
        /// Refresh messages.
        /// </summary>
        public void Refresh()
        {
            SetState(ScreenState.Loading);
            LoadData();
        }
    }
}
