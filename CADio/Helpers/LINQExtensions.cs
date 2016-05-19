using System;
using System.Collections.Generic;

namespace CADio.Helpers
{
    public static class LINQExtensions
    {
        /// <summary>
        /// Returns the index of the first element in the sequence 
        /// that satisfies a condition.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements of <paramref name="source"/>.
        /// </typeparam>
        /// <param name="source">
        /// An <see cref="IEnumerable{T}"/> that contains
        /// the elements to apply the predicate to.
        /// </param>
        /// <param name="predicate">
        /// A function to test each element for a condition.
        /// </param>
        /// <returns>
        /// The zero-based index position of the first element of <paramref name="source"/>
        /// for which <paramref name="predicate"/> returns <see langword="true"/>;
        /// or -1 if <paramref name="source"/> is empty
        /// or no element satisfies the condition.
        /// </returns>
        public static int IndexOf<TSource>(this IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            // source: http://stackoverflow.com/questions/12695501/get-index-of-first-non-whitespace-character-in-c-sharp-string
            int i = 0;

            foreach (TSource element in source)
            {
                if (predicate(element))
                    return i;

                i++;
            }

            return -1;
        }
    }
}