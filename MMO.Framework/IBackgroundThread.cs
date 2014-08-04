using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMO.Framework
{
    public interface IBackgroundThread
    {
        void Setup();
        void Run(Object threadContext);
        void Stop();
    }
}
