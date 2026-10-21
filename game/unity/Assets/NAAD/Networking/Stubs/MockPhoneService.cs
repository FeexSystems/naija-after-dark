using System;
using System.Collections.Generic;
using System.Linq;
using NAAD.Networking.Supabase;
using UnityEngine;

namespace NAAD.Mock
{
    /// <summary>
    /// Mock implementation of <see cref="IPhoneService"/> for automated testing.
    /// <para>
    /// Maintains an in-memory message list with Tunde's beach invite, supports
    /// listing, responding, and refreshing. State reset via <see cref="Reset"/>.
    /// </para>
    /// </summary>
    public sealed class MockPhoneService : IPhoneService
    {
        public List<PhoneMessage> Messages => _messages;

        private readonly List<PhoneMessage> _messages = new();
        private readonly System.Random _random = new(42);

        public MockPhoneService()
        {
            // Seed with Tunde's beach invite
            _messages.Add(new PhoneMessage
            {
                Id = "msg_tunde_beach_1",
                SenderId = NightDistrictIds.Tunde,
                SenderName = "Tunde",
                ThreadKey = "tunde_beach_invite",
                Body = "you pulling up to the beach tonight? we party then we beat jack dance floor",
                SenderType = "BOT",
                ChosenAction = null,
                CreatedAt = System.DateTime.UtcNow.AddHours(-1)
            });

            _messages.Add(new PhoneMessage
            {
                Id = "msg_tunde_morning",
                SenderId = NightDistrictIds.Tunde,
                SenderName = "Tunde",
                ThreadKey = "tunde_morning",
                Body = "morning. sleep ok?",
                SenderType = "BOT",
                ChosenAction = null,
                CreatedAt = System.DateTime.UtcNow.AddHours(-5)
            });
        }

        public Task<List<PhoneMessage>> ListMessagesAsync(string? filter, string? limit)
        {
            var result = filter == null
                ? _messages
                : _messages.FindAll(m => m.Body.Contains(filter, System.StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(result);
        }

        public Task<bool> SendMessageAsync(string target, string body, string requestId)
        {
            _messages.Add(new PhoneMessage
            {
                Id = $"msg_{System.Guid.NewGuid().ToString("N")[..8]}",
                SenderId = NightDistrictIds.Tunde,
                SenderName = "Tunde",
                ThreadKey = "new_thread",
                Body = body,
                SenderType = "USER",
                ChosenAction = "GO",
                CreatedAt = System.DateTime.UtcNow
            });
            return Task.FromResult(true);
        }

        public Task<bool> RespondToMessageAsync(string messageId, string action, string requestId)
        {
            var msg = _messages.Find(m => m.Id == messageId);
            if (msg == null) return Task.FromResult(false);

            msg.ChosenAction = action;
            msg.ResponseTime = System.DateTime.UtcNow;
            return Task.FromResult(true);
        }

        public Task<bool> MarkAsReadAsync(string messageId, string requestId)
        {
            var msg = _messages.Find(m => m.Id == messageId);
            if (msg != null) msg.IsRead = true;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteMessageAsync(string messageId, string requestId)
        {
            var msg = _messages.Find(m => m.Id == messageId);
            if (msg != null) _messages.Remove(msg);
            return Task.FromResult(true);
        }

        public Task<bool> CompleteNightAsync()
        {
            _messages.Clear();
            _messages.Add(new PhoneMessage
            {
                Id = "msg_night_summary",
                SenderId = NightDistrictIds.Tunde,
                SenderName = "Tunde",
                ThreadKey = "night_summary",
                Body = "night end. you op. best moment: club dance. come back later?",
                SenderType = "BOT",
                ChosenAction = "GO",
                CreatedAt = System.DateTime.UtcNow
            });
            return Task.FromResult(true);
        }

        public Task<bool> NotifyNotificationAsync(string message)
        {
            Debug.Log($"[MockPhone] Notification: {message}");
            return Task.FromResult(true);
        }

        public void Reset()
        {
            _messages.Clear();
            _messages.Add(new PhoneMessage
            {
                Id = "msg_tunde_beach_1",
                SenderId = NightDistrictIds.Tunde,
                SenderName = "Tunde",
                ThreadKey = "tunde_beach_invite",
                Body = "you pulling up to the beach tonight? we party then we beat jack dance floor",
                SenderType = "BOT",
                ChosenAction = null,
                CreatedAt = System.DateTime.UtcNow.AddHours(-1)
            });
        }
    }
}