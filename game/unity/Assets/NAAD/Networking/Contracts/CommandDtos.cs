using System;
using UnityEngine;

namespace NAAD.Networking.Contracts
{
    /// <summary>
    /// Phase D — JsonUtility-friendly envelopes for naad_execute_command.
    /// Prefer these over ad-hoc string search when decoding router responses.
    /// </summary>
    [Serializable]
    public sealed class ExecuteCommandRequestDto
    {
        public string p_request_id;
        public string p_type;
        public string p_payload_json;
    }

    [Serializable]
    public sealed class CommandResultDto
    {
        public string requestId;
        public bool success;
        public string errorCode;
        public string errorMessage;
    }

    public static class CommandDtoCodec
    {
        public static CommandResultDto ParseResult(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new CommandResultDto { success = false, errorCode = "EMPTY_BODY" };

            try
            {
                var dto = JsonUtility.FromJson<CommandResultDto>(json);
                if (dto != null) return dto;
            }
            catch
            {
                // fall through — some servers nest fields differently
            }

            return new CommandResultDto
            {
                success = false,
                errorCode = "PARSE_FAILED",
                errorMessage = "Could not decode CommandResultDto"
            };
        }
    }
}
