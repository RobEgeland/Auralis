using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Core.Models
{
    public sealed class AudioSource
    {
        // Stable identifier (e.g., "spotify.exe", "chrome.exe")
        public string SourceKey { get; init; } = string.Empty;

        // Friendly display name for UI
        public string DisplayName { get; init; } = string.Empty;
    }
}
