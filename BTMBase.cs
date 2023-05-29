using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTM
{
    interface IBTMBase : ICloneable
    {
        string ToString();
        string ToShortString();
    }

    interface IBTMBuilder<BTMBase> where BTMBase : IBTMBase
    {
        void Reset();
        BTMBase Result();
    }
}
