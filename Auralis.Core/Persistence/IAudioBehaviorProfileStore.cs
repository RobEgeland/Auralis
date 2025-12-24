using Auralis.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Core.Persistence
{
    public interface IAudioBehaviorProfileStore
    {
        AudioBehaviorProfile? LoadProfile();
        void SaveProfile(AudioBehaviorProfile profile);
    }
}
