using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Ipfs.Core
{
	public interface IStoreFactory
	{
		public IStore<TName, TValue> CreateStore<TName, TValue>(
			string @namespace,
			Func<TName, string>? nameToKey = default,
			Func<string, TName>? keyToName = default,
			Func<Stream, TName, TValue, CancellationToken, Task>? serialize = default,
			Func<Stream, TName, CancellationToken, Task<TValue>>? deserialize = default
			) where TValue : class;
	}
}