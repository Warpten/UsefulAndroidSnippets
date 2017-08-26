using System;
using System.Collections.Generic;
using System.Linq;

namespace Deezy
{
    public static class LinqExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> arr, Func<T, bool> fn)
        {
            var idx = 0;
            foreach (var elem in arr)
            {
                if (fn(elem))
                    return idx;

                ++idx;
            }
            return idx;
        }

        // Or ...

        public static int IndexOf<T>(this IList<T> arr, Func<T, bool> fn)
        {
            var idx = 0;
            for (var i = 0; i < arr.Count; ++i)
            {
                if (fn(arr[i]))
                    return idx;

                ++idx;
            }
            return idx;
        }
    }
}
