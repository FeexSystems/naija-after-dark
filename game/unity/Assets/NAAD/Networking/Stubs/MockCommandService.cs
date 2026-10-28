using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Gameplay;
using NAAD.Networking;
using NAAD.Networking.Supabase;
using UnityEngine;

namespace NAAD.Mock
{
    /// <summary>
    /// Mock implementation of <see cref="ICommandService"/> for automated testing.
    /// <para>
    /// Simulates authoritative server behavior by validating commands against
    /// current game state and returning deterministic results. Does NOT make
    /// network calls or write to the database.
    /// </para>
    /// </summary>
    public sealed class MockCommandService : ICommandService
    {
        private readonly MockPlayerService _players;
        private readonly MockWorldStateService _world;
        private readonly MockPhoneService _phone;
        private readonly MockAuthenticationService _auth;

        private readonly List<CommandRequest> _history = new();

        public event Action<CommandRequest, CommandResult>? OnCommandExecuted;

        public MockCommandService(
            MockAuthenticationService auth,
            MockPlayerService players,
            MockWorldStateService world,
            MockPhoneService phone)
        {
            _auth = auth ?? throw new System.ArgumentNullException(nameof(auth));
            _players = players ?? throw new System.ArgumentNullException(nameof(players));
            _world = world ?? throw new System.ArgumentNullException(nameof(world));
            _phone = phone ?? throw new System.ArgumentNullException(nameof(phone));
        }

        /// <summary>
        /// Execute a game command and return the result.
        /// <para>
        /// All mutations go through this method in production. In mock mode,
        /// returns deterministic results based on the current state.
        /// </para>
        /// </summary>
        public Task<CommandResult> ExecuteAsync(GameCommand command)
        {
            var request = new CommandRequest
            {
                RequestId = command.RequestId,
                PlayerId = command.PlayerId
            };
            var result = new CommandResult
            {
                RequestId = command.RequestId,
                Success = true,
            };

            try
            {
                switch (command.Type)
                {
                    case GameCommandType.Travel:
                        result = ValidateAndExecuteTravel(request, ReadTravel(command));
                        break;

                    case GameCommandType.Spend:
                        result = ValidateAndExecuteSpend(request, ReadSpend(command));
                        break;

                    case GameCommandType.Buy:
                        result = ValidateAndExecuteBuy(request, ReadBuy(command));
                        break;

                    case GameCommandType.StartNight:
                        result = ValidateAndExecuteStartNight(request, new StartNightCommand());
                        break;

                    case GameCommandType.CompleteNight:
                        result = ValidateAndExecuteCompleteNight(request, new CompleteNightCommand());
                        break;

                    case GameCommandType.PhoneReply:
                        result = ValidateAndExecutePhoneReply(request, ReadPhoneReply(command));
                        break;

                    case GameCommandType.JoinRoom:
                        result = ValidateAndExecuteJoinRoom(request, ReadJoinRoom(command));
                        break;

                    case GameCommandType.LeaveRoom:
                        result = ValidateAndExecuteLeaveRoom(request, new LeaveRoomCommand());
                        break;

                    case GameCommandType.AttendEvent:
                        result = ValidateAndExecuteAttendEvent(request, ReadAttendEvent(command));
                        break;

                    case GameCommandType.RelationshipDelta:
                        result = ValidateAndExecuteRelationshipDelta(request, ReadRelationshipDelta(command));
                        break;

                    case GameCommandType.SetCareer:
                        result = ValidateAndExecuteSetCareer(request, ReadSetCareer(command));
                        break;

                    case GameCommandType.AcceptOpportunity:
                        result = ValidateAndExecuteAcceptOpportunity(request, ReadAcceptOpportunity(command));
                        break;

                    case GameCommandType.CommitMemory:
                        result = ValidateAndExecuteCommitMemory(request, ReadCommitMemory(command));
                        break;

                    case GameCommandType.SetWorldPeriod:
                        result = ValidateAndExecuteSetWorldPeriod(request, ReadSetWorldPeriod(command));
                        break;

                    default:
                        result.Success = false;
                        result.ErrorCode = "UNKNOWN_COMMAND";
                        result.ErrorMessage = $"Unknown command type: {command.Type}";
                        break;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorCode = "EXECUTION_ERROR";
                result.ErrorMessage = ex.Message;
            }

            _history.Add(request);
            return Task.FromResult(result);
        }

        public Task<CommandResult> ExecuteAsync(CommandRequest request)
        {
            var command = new GameCommand
            {
                RequestId = request.RequestId,
                PlayerId = request.PlayerId,
                Type = request.Command.Type,
                Payload = request.Command.Payload,
                ClientTimestamp = request.Command.ClientTimestamp
            };
            return ExecuteAsync(command);
        }

        private static string? PayloadString(GameCommand command, string key)
        {
            if (command.Payload == null) return null;
            if (command.Payload is IDictionary<string, object> dictObj &&
                dictObj.TryGetValue(key, out var vObj))
                return vObj?.ToString();
            if (command.Payload is IDictionary<string, string> dictStr &&
                dictStr.TryGetValue(key, out var vStr))
                return vStr;
            return null;
        }

        private static int? PayloadInt(GameCommand command, string key)
        {
            var s = PayloadString(command, key);
            if (string.IsNullOrEmpty(s)) return null;
            return int.TryParse(s, out var n) ? n : null;
        }

        private static NAAD.Networking.TravelCommand ReadTravel(GameCommand command) => new()
        {
            ToLocationId = PayloadString(command, "toLocationId")
        };

        private static SpendCommand ReadSpend(GameCommand command) => new()
        {
            Sku = PayloadString(command, "sku")
        };

        private static BuyCommand ReadBuy(GameCommand command) => new()
        {
            Sku = PayloadString(command, "sku"),
            LocationId = PayloadString(command, "locationId")
        };

        private static PhoneReplyCommand ReadPhoneReply(GameCommand command) => new()
        {
            MessageId = PayloadString(command, "messageId"),
            Action = PayloadString(command, "action")
        };

        private static JoinRoomCommand ReadJoinRoom(GameCommand command) => new()
        {
            LocationId = PayloadString(command, "locationId")
        };

        private static AttendEventCommand ReadAttendEvent(GameCommand command) => new()
        {
            EventId = PayloadString(command, "eventId")
        };

        private static RelationshipDeltaCommand ReadRelationshipDelta(GameCommand command) => new()
        {
            TargetId = PayloadString(command, "targetId"),
            Trust = PayloadInt(command, "trust")
        };

        private static SetCareerCommand ReadSetCareer(GameCommand command) => new()
        {
            CareerId = PayloadString(command, "careerId")
        };

        private static AcceptOpportunityCommand ReadAcceptOpportunity(GameCommand command) => new()
        {
            OpportunityId = PayloadString(command, "opportunityId")
        };

        private static CommitMemoryCommand ReadCommitMemory(GameCommand command) => new()
        {
            NpcId = PayloadString(command, "npcId"),
            MemoryType = PayloadString(command, "memoryType"),
            Summary = PayloadString(command, "summary")
        };

        private static SetWorldPeriodCommand ReadSetWorldPeriod(GameCommand command) => new()
        {
            Period = PayloadString(command, "period")
        };

        private CommandResult ValidateAndExecuteRelationshipDelta(CommandRequest request, RelationshipDeltaCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };

            if (command.TargetId == null)
            {
                result.Success = false;
                result.ErrorCode = "INVALID_PAYLOAD";
                result.ErrorMessage = "targetId is required for RELATIONSHIP_DELTA.";
                return result;
            }

            _world.ApplyRelationshipDelta(command.TargetId ?? string.Empty, (int)(command.Trust ?? 0));
            result.Payload = new { success = true };
            return result;
        }

        private CommandResult ValidateAndExecuteSetCareer(CommandRequest request, SetCareerCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };

            if (command.CareerId == null)
            {
                result.Success = false;
                result.ErrorCode = "INVALID_PAYLOAD";
                result.ErrorMessage = "careerId is required for SET_CAREER.";
                return result;
            }

            _players.SetCareer(command.CareerId ?? string.Empty);
            result.Payload = new { success = true };
            return result;
        }

        private CommandResult ValidateAndExecuteAcceptOpportunity(CommandRequest request, AcceptOpportunityCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };

            if (command.OpportunityId == null)
            {
                result.Success = false;
                result.ErrorCode = "INVALID_PAYLOAD";
                result.ErrorMessage = "opportunityId is required for ACCEPT_OPPORTUNITY.";
                return result;
            }

            result.Payload = new { accepted = true, opportunityId = command.OpportunityId ?? string.Empty };
            return result;
        }

        private CommandResult ValidateAndExecuteCommitMemory(CommandRequest request, CommitMemoryCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };

            if (command.NpcId == null)
            {
                result.Success = false;
                result.ErrorCode = "INVALID_PAYLOAD";
                result.ErrorMessage = "npcId is required for COMMIT_MEMORY.";
                return result;
            }

            _world.CommitMemory(command.NpcId ?? string.Empty, command.MemoryType, command.Summary);
            result.Payload = new { success = true };
            return result;
        }

        private CommandResult ValidateAndExecuteSetWorldPeriod(CommandRequest request, SetWorldPeriodCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };

            if (command.Period == null)
            {
                result.Success = false;
                result.ErrorCode = "INVALID_PAYLOAD";
                result.ErrorMessage = "period is required for SET_WORLD_PERIOD.";
                return result;
            }

            _world.SetWorldPeriod(command.Period ?? string.Empty);
            result.Payload = new { success = true };
            return result;
        }

        private int GetSkuPrice(string sku)
        {
            return sku switch
            {
                "SUYA_PLATE" => 5000,
                "BEACH_DRINK" => 3000,
                "SOFT_DRINK" => 1500,
                "CLUB_TICKET" => 20000,
                "TRANSPORT_SHORT" => 4000,
                _ => 0
            };
        }

        private int GetNightTotalSpent()
        {
            return 5000 + 20000;
        }

        private NightSummaryDto GetNightSummary()
        {
            return new NightSummaryDto
            {
                Success = true,
                SpentNgn = GetNightTotalSpent(),
                PeopleMet = 3,
                BestMoment = "Dancing at the club with friends"
            };
        }

        public void Reset()
        {
            _history.Clear();
            _auth.Reset();
            _players.Reset();
            _world.Reset();
            _phone.Reset();
        }

        public int CommandCount => _history.Count;

        private CommandResult ValidateAndExecuteTravel(CommandRequest request, NAAD.Networking.TravelCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };
            var currentLocation = _players.CurrentLocation;

            if (string.IsNullOrEmpty(command.ToLocationId))
            {
                result.Success = false;
                result.ErrorCode = "INVALID_PAYLOAD";
                result.ErrorMessage = "toLocationId is required for TRAVEL.";
                return result;
            }

            if (currentLocation == command.ToLocationId)
            {
                result.Success = false;
                result.ErrorCode = "NOT_MOVING";
                result.ErrorMessage = "Already at destination.";
                return result;
            }

            if (!NightDistrictIds.IsValidLocation(command.ToLocationId))
            {
                result.Success = false;
                result.ErrorCode = "INVALID_LOCATION";
                result.ErrorMessage = "Location does not exist.";
                return result;
            }

            var pendingTravel = new PendingTravelCommand
            {
                RequestId = request.RequestId,
                ToLocationId = command.ToLocationId,
                Coordinates = new Vector3(0f, 0f, 0f),
            };

            _world.AddPendingTravel(pendingTravel);
            result.Payload = new { pendingTravel };
            return result;
        }

        private CommandResult ValidateAndExecuteSpend(CommandRequest request, SpendCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };

            if (command.Sku == null)
            {
                result.Success = false;
                result.ErrorCode = "INVALID_PAYLOAD";
                result.ErrorMessage = "sku is required for SPEND.";
                return result;
            }

            var currentBalance = _players.CurrentBalance;
            var price = GetSkuPrice(command.Sku);

            if (currentBalance < price)
            {
                result.Success = false;
                result.ErrorCode = "INSUFFICIENT_FUNDS";
                result.ErrorMessage = $"Insufficient funds. Need ₦{price}, have ₦{currentBalance}.";
                return result;
            }

            _players.SetBalance(currentBalance - price);
            result.Payload = new { sku = command.Sku, amount = price, balanceAfter = currentBalance - price };
            return result;
        }

        private CommandResult ValidateAndExecuteBuy(CommandRequest request, BuyCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };
            var locationId = command.LocationId;

            if (string.IsNullOrEmpty(locationId))
            {
                result.Success = false;
                result.ErrorCode = "INVALID_PAYLOAD";
                result.ErrorMessage = "locationId is required for BUY.";
                return result;
            }

            if (!NightDistrictIds.IsValidDistrict(locationId))
            {
                result.Success = false;
                result.ErrorCode = "INVALID_LOCATION";
                result.ErrorMessage = "Location does not exist.";
                return result;
            }

            var currentLocation = _players.CurrentLocation;
            if (currentLocation != locationId)
            {
                result.Success = false;
                result.ErrorCode = "NOT_AT_LOCATION";
                result.ErrorMessage = "Cannot buy. Move to the correct location.";
                return result;
            }

            _players.SetBalance(_players.CurrentBalance - GetSkuPrice(command.Sku));
            result.Payload = new { success = true };
            return result;
        }

        private CommandResult ValidateAndExecuteStartNight(CommandRequest request, StartNightCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };

            if (!_auth.IsAuthenticated)
            {
                result.Success = false;
                result.ErrorCode = "NOT_AUTHENTICATED";
                result.ErrorMessage = "Player must be authenticated to start a night.";
                return result;
            }

            var currentLocation = _players.CurrentLocation;
            if (currentLocation != NightDistrictIds.Apartment)
            {
                result.Success = false;
                result.ErrorCode = "WRONG_LOCATION";
                result.ErrorMessage = "Player must be at the apartment to start the night.";
                return result;
            }

            result.Payload = new { nightStarted = true };
            return result;
        }

        private CommandResult ValidateAndExecuteCompleteNight(CommandRequest request, CompleteNightCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };
            result.Payload = new { nightCompleted = true, summary = GetNightSummary() };
            return result;
        }

        private CommandResult ValidateAndExecutePhoneReply(CommandRequest request, PhoneReplyCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };

            if (command.MessageId == null)
            {
                result.Success = false;
                result.ErrorCode = "INVALID_PAYLOAD";
                result.ErrorMessage = "messageId is required for PHONE_REPLY.";
                return result;
            }

            if (command.Action == null)
            {
                result.Success = false;
                result.ErrorCode = "INVALID_PAYLOAD";
                result.ErrorMessage = "action is required for PHONE_REPLY.";
                return result;
            }

            var messages = _phone.Messages;
            var messageExists = messages.Exists(m => m.Id == command.MessageId && m.ChosenAction == null);

            if (!messageExists)
            {
                result.Success = false;
                result.ErrorCode = "MESSAGE_NOT_FOUND";
                result.ErrorMessage = "Message not found or already responded to.";
                return result;
            }

            result.Payload = new { success = true };
            return result;
        }

        private CommandResult ValidateAndExecuteJoinRoom(CommandRequest request, JoinRoomCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };

            if (command.LocationId == null)
            {
                result.Success = false;
                result.ErrorCode = "INVALID_PAYLOAD";
                result.ErrorMessage = "locationId is required for JOIN_ROOM.";
                return result;
            }

            result.Payload = new { roomJoined = true, locationId = command.LocationId ?? string.Empty };
            return result;
        }

        private CommandResult ValidateAndExecuteLeaveRoom(CommandRequest request, LeaveRoomCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };
            result.Payload = new { roomLeft = true };
            return result;
        }

        private CommandResult ValidateAndExecuteAttendEvent(CommandRequest request, AttendEventCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };
            result.Payload = new { attended = true };
            return result;
        }
    }
}