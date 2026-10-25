using System;
using System.Collections.Generic;
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
        public Task<CommandResult> ExecuteAsync(CommandRequest request)
        {
            var cmd = request.Command ?? throw new System.ArgumentException("Command required.");
            var result = new CommandResult
            {
                RequestId = request.RequestId,
                Success = true,
            };

            try
            {
                switch (cmd.Type)
                {
                    case GameCommandType.TRAVEL:
                        result = ValidateAndExecuteTravel(request, cmd as TravelCommand);
                        break;

                    case GameCommandType.SPEND:
                        result = ValidateAndExecuteSpend(request, cmd as SpendCommand);
                        break;

                    case GameCommandType.BUY:
                        result = ValidateAndExecuteBuy(request, cmd as BuyCommand);
                        break;

                    case GameCommandType.START_NIGHT:
                        result = ValidateAndExecuteStartNight(request, cmd as StartNightCommand);
                        break;

                    case GameCommandType.COMPLETE_NIGHT:
                        result = ValidateAndExecuteCompleteNight(request, cmd as CompleteNightCommand);
                        break;

                    case GameCommandType.PHONE_REPLY:
                        result = ValidateAndExecutePhoneReply(request, cmd as PhoneReplyCommand);
                        break;

                    case GameCommandType.JOIN_ROOM:
                        result = ValidateAndExecuteJoinRoom(request, cmd as JoinRoomCommand);
                        break;

                    case GameCommandType.LEAVE_ROOM:
                        result = ValidateAndExecuteLeaveRoom(request, cmd as LeaveRoomCommand);
                        break;

                    case GameCommandType.ATTEND_EVENT:
                        result = ValidateAndExecuteAttendEvent(request, cmd as AttendEventCommand);
                        break;

                    case GameCommandType.RELATIONSHIP_DELTA:
                        result = ValidateAndExecuteRelationshipDelta(request, cmd as RelationshipDeltaCommand);
                        break;

                    case GameCommandType.SET_CAREER:
                        result = ValidateAndExecuteSetCareer(request, cmd as SetCareerCommand);
                        break;

                    case GameCommandType.ACCEPT_OPPORTUNITY:
                        result = ValidateAndExecuteAcceptOpportunity(request, cmd as AcceptOpportunityCommand);
                        break;

                    case GameCommandType.COMMIT_MEMORY:
                        result = ValidateAndExecuteCommitMemory(request, cmd as CommitMemoryCommand);
                        break;

                    case GameCommandType.SET_WORLD_PERIOD:
                        result = ValidateAndExecuteSetWorldPeriod(request, cmd as SetWorldPeriodCommand);
                        break;

                    default:
                        result.Success = false;
                        result.ErrorCode = "UNKNOWN_COMMAND";
                        result.ErrorMessage = $"Unknown command type: {cmd.Type}";
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
            OnCommandExecuted?.Invoke(request, result);
            return Task.FromResult(result);
        }

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

            _world.ApplyRelationshipDelta(command.TargetId.Value, (int)(command.Trust ?? 0));
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

            _players.SetCareer(command.CareerId.Value);
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

            result.Payload = new { accepted = true, opportunityId = command.OpportunityId.Value };
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

            _world.CommitMemory(command.NpcId.Value, command.MemoryType, command.Summary);
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

            _world.SetWorldPeriod(command.Period.Value);
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
                PlayerId = "mock_player",
                Spent = GetNightTotalSpent(),
                PeopleMet = 3,
                BestMoment = "Dancing at the club with friends",
                CompletedAt = System.DateTime.UtcNow
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

        private CommandResult ValidateAndExecuteTravel(CommandRequest request, TravelCommand command)
        {
            var result = new CommandResult { RequestId = request.RequestId };
            var currentLocation = _players.CurrentLocation.Value;

            if (!command.ToLocationId.HasValue)
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

            if (!NightDistrictIds.IsValidLocation(command.ToLocationId.Value))
            {
                result.Success = false;
                result.ErrorCode = "INVALID_LOCATION";
                result.ErrorMessage = "Location does not exist.";
                return result;
            }

            var pendingTravel = new PendingTravelCommand
            {
                RequestId = request.RequestId,
                ToLocationId = command.ToLocationId.Value,
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

            var currentBalance = _players.CurrentBalance.Value;
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
            var locationId = command.LocationId?.Value;

            if (locationId == null)
            {
                result.Success = false;
                result.ErrorCode = "INVALID_PAYLOAD";
                result.ErrorMessage = "locationId is required for BUY.";
                return result;
            }

            if (!NightDistrictIds.IsValidDistrict(locationId.Value))
            {
                result.Success = false;
                result.ErrorCode = "INVALID_LOCATION";
                result.ErrorMessage = "Location does not exist.";
                return result;
            }

            var currentLocation = _players.CurrentLocation.Value;
            if (currentLocation != locationId.Value)
            {
                result.Success = false;
                result.ErrorCode = "NOT_AT_LOCATION";
                result.ErrorMessage = "Cannot buy. Move to the correct location.";
                return result;
            }

            _players.SetBalance(_players.CurrentBalance.Value - GetSkuPrice(command.Sku));
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

            var currentLocation = _players.CurrentLocation.Value;
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

            var messages = _phone.Messages.Value;
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

            result.Payload = new { roomJoined = true, locationId = command.LocationId.Value };
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