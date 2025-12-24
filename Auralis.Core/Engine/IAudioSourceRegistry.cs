using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Core.Engine
{
    public interface IAudioSourceRegistry
    {
        IReadOnlyCollection<int> GetPids(string sourceKey);
        bool TryGetPid(string sourceKey, out int pid);
    }

}
