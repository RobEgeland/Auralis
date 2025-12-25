using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Core.Playback
{
    public interface IMediaPlaybackMonitor
    {
        bool IsPrimaryPlaying(string sourceKey);
    }
}

