using Auralis.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Models
{
    public sealed class AudioSourceViewModel
    {
        public string SourceKey { get; }
        public string DisplayName { get; }

        public AudioSourceViewModel(AudioSource source)
        {
            SourceKey = source.SourceKey;
            DisplayName = source.DisplayName;
        }
    }
}
