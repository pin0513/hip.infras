using System;

namespace hip.Infrastructure.Interfaces
{
    public interface ICachingProvider
    {
        object Get(string cacheName);
        object Get(string cacheName, Func<object> func, double minutes = 1);
        bool Update(string cacheName, object content, double minutes = 1);
        bool Remove(string cacheName);
    }
}
