using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Core.Models
{
    public sealed class AudioAppTarget
    {
        public int ProcessId { get; init; }
        public string ProcessName { get; init; } = string.Empty;
    }
}
