
namespace BTM
{
    interface IBTMBase
    {
        string ToString();
        string ToShortString();
    }

    interface IBTMBuilder<BTMBase> where BTMBase : IBTMBase
    {
        void Reset();
        BTMBase Result();
    }

    interface IRestoreable<BTMBase> where BTMBase : IBTMBase
    {
        BTMBase Clone();
        void CopyFrom(BTMBase src);
    }
}
