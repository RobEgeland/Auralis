using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Core.Engine
{
    public interface IFadeEngine
    {
        public bool IsBusy { get;  }
        Task FadeToAsync(
            string sourceKey,
            float targetVolumeDb, 
            int durationMs, 
            CancellationToken ct);
    }
}
