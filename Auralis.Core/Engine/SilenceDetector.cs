using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Core.Engine
{
    public sealed class SilenceDetector
    {
        private readonly float _thresholdDb;
        private readonly int _holdMs;
        private int _silentMs;
        public SilenceDetector(float thresholdDb, int holdMs)
        {
            _thresholdDb = thresholdDb;
            _holdMs = holdMs;
        }
        public bool Update(float currentDb, int deltaMs)
        {
            if (currentDb < _thresholdDb)
                _silentMs += deltaMs;
            else
                _silentMs = 0;

            return _silentMs >= _holdMs;
        }

        public void Reset() => _silentMs = 0;
    }
}
