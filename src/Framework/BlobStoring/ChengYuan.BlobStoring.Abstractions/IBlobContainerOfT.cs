using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ChengYuan.BlobStoring;

public interface IBlobContainer<TContainer> : IBlobContainer
    where TContainer : class;
