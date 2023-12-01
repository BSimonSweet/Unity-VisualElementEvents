using System;
using UnityEngine.Pool;

namespace BsiGame.UI.UIElements
{
	public static class PooledObjectHelpers
	{
		public static void Release<T>(this ref PooledObject<T>? pooledObject)
			where T : class
		{
			if(pooledObject == null)
				return;

			_Dispose(pooledObject.Value);
			pooledObject = default;

			// -- //

			void _Dispose<TDispose>(TDispose obj) where TDispose : IDisposable
				=> obj.Dispose();
		}
	}
}