using System.Collections.Generic;
using System.Linq;

namespace NDiff.ExtensionMethods
{
    public static class DictionaryExtensionMethods
    {
        /// <summary>
        /// Method used to update the key of a dictionary.
        /// </summary>
        /// <param name="dictionary">Dictionary.</param>
        /// <param name="oldKey">Old key.</param>
        /// <param name="newKey">New key.</param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public static void UpdateKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            TKey oldKey, TKey newKey)
        {
            var value = dictionary[oldKey];
            dictionary.Remove(oldKey);
            dictionary[newKey] = value;
        }

        /// <summary>
        /// Merges the source and other dictionary. In case the same keys appear an ArgumentException is thrown.
        /// </summary>
        /// <param name="source">Source dictionary.</param>
        /// <param name="other">Other dictionary.</param> 
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> source,
            IDictionary<TKey, TValue> other)
        {
            return source.Union(other).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}