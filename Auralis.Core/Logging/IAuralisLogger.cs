using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Core.Logging
{
    public interface IAuralisLogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception? ex = null);
    }
}
