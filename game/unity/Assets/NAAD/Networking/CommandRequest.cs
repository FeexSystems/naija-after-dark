using System.Collections.Generic;
using System.Threading.Tasks;

namespace NAAD.Networking
{
    /// <summary>
    /// Wraps a <see cref="GameCommand"/> with request metadata for command execution.
    /// Used by the mock command service and automated tests.
    /// </summary>
    public sealed class CommandRequest
    {
        public string RequestId { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public GameCommand Command { get; set; } = new();
    }
}