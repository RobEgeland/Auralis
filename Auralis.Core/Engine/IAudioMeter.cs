using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Core.Engine
{
    public interface IAudioMeter
    {
        float GetCurrentLevelDb(string sourceKey);
    }
}
