using System.Collections.Generic;

namespace ChengYuan.Core.SimpleStateChecking;

public interface IHasSimpleStateCheckers
{
    IList<ISimpleStateChecker<IHasSimpleStateCheckers>> StateCheckers { get; }
}
