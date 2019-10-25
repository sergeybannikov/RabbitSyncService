using System;
using System.Diagnostics;

namespace RabbitSyncService.Extensions
{
    public static class TypeExtensions
    {
        [DebuggerStepThrough]
        public static bool Is<T>(this Type target) => typeof(T).IsAssignableFrom(target);
    }
}
