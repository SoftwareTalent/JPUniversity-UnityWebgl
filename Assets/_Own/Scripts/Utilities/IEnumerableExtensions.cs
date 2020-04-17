using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq {
    
    public static class IEnumerableExtensions {

        /// Applies an action to each element of a given IEnumerable
        public static void Each<T>(this IEnumerable<T> enumerable, Action<T> action) 
        {
            foreach (var element in enumerable)
            {
                action(element);
            }
        }
        
        public static Queue<T> ToQueue<T>(this IEnumerable<T> items)
        {
            return new Queue<T>(items);
        }
    }
}