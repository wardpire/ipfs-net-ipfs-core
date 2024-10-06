using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs.CoreApi
{
    public interface IDistibutedValueStore
    {
        Task<IEnumerable<byte[]>> FindSimilarValuesAsync(string @namespace, MultiHash key, CancellationToken cancellationToken = default);
        Task<byte[]?> TryGetValueAsync(string @namespace, MultiHash key, CancellationToken cancellationToken = default);
        Task PutValueAsync(string @namespace, MultiHash key, byte[] value, CancellationToken cancellationToken = default);
    }
}
