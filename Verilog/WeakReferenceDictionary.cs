using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SystemVerilogCore.Verilog
{
    /// <summary>
    /// A dictionary whose values are held weakly.
    ///
    /// <para>
    /// The parser's register name spaces map a declaration name to the file that
    /// declared it. Holding those files strongly would keep every parsed file
    /// alive for as long as the project is open, so the entries are weak: a file
    /// that has been closed and collected simply disappears from the table.
    /// Lookups therefore never fail loudly - a collected entry reads as "not
    /// registered".
    /// </para>
    ///
    /// <para>
    /// This type used to live in the editor plugin assembly. It is plain BCL
    /// code (no UI types), and the parser is the only consumer, so it belongs in
    /// the UI-free assembly alongside the rest of the parser.
    /// </para>
    /// </summary>
    public class WeakReferenceDictionary<K, T> where K : notnull where T : class
    {
        private ConcurrentDictionary<K, System.WeakReference<T>> itemRefs = new ConcurrentDictionary<K, WeakReference<T>>();

        public int Count { get { return itemRefs.Count; } }
        public void Register(K key, T item)
        {
            WeakReference < T > weakRef = new WeakReference<T>(item);
            itemRefs.AddOrUpdate(key, weakRef, (k, old) => { return weakRef; });
        }

        public bool Remove(K key)
        {
            return itemRefs.TryRemove(key, out _);
        }

        public bool HasItem(K key)
        {
            return itemRefs.ContainsKey(key);
        }

        public T? GetItem(K key)
        {
            if(!itemRefs.TryGetValue(key, out var weakRef))
            {
                return null;
            }
            T? item;
            if (weakRef.TryGetTarget(out item))
            {
                return item;
            }
            else
            {
                itemRefs.TryRemove(key, out _);
                return null;
            }
        }

        public List<K> GetMatchedKeyList(Func<T, bool> isMatched)
        {
            List<K> resultKeys = new List<K>();

            foreach (var weakRefKvp in itemRefs)
            {
                T? item;
                if (!weakRefKvp.Value.TryGetTarget(out item))
                {
                    itemRefs.TryRemove(weakRefKvp.Key, out _);
                }
                else if (isMatched(item))
                {
                    resultKeys.Add(weakRefKvp.Key);
                }

            }
            return resultKeys;
        }

        public List<K> KeyList()
        {
            return itemRefs.Keys.ToList();
        }

        public void CleanDictionary()
        {
            foreach (var weakRefKvp in itemRefs)
            {
                T? item;
                if (!weakRefKvp.Value.TryGetTarget(out item))
                {
                    itemRefs.TryRemove(weakRefKvp.Key, out _);
                }
            }
        }
    }
}
