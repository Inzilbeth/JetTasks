using System;
using System.Collections.Generic;
using System.Linq;

namespace Task2
{
    /// <summary>
    /// Implementation of AssertCache class.
    /// </summary>
    public class AssetCache : IAssetCache
    {

        private Dictionary<string, Cache> index;
        private AssetParser parser;

        /// <summary>
        /// AssetCache constructor.
        /// </summary>
        public AssetCache()
        {
            index = new Dictionary<string, Cache>();
            parser = new AssetParser();
        }

        public object Build(string path, Action interruptChecker)
        {
            Cache cache = parser.Build(path, interruptChecker) as Cache;

            if (cache != null)
            {
                Merge(path, cache);
            }

            return cache;
        }

        public void Merge(string path, object result)
        {
            if (!index.ContainsKey(path))
            {
                index.Add(path, result as Cache);
            }

            index[path] = result as Cache;
        }

        public int GetLocalAnchorUsages(ulong anchor)
        {
            int totalUsages = 0;
            foreach (KeyValuePair<string, Cache> pair in index)
            {
                totalUsages += pair.Value.GetAnchorUsages(anchor);
            }
            return totalUsages;
        }

        public int GetGuidUsages(string guid)
        {
            int totalUsages = 0;

            foreach (KeyValuePair<string, Cache> pair in index)
            {
                totalUsages += pair.Value.GetResourceUsgaes(guid);
            }

            return totalUsages;
        }

        public IEnumerable<ulong> GetComponentsFor(ulong gameObjectAnchor)
        {
            var allComponents = new List<ulong>();

            foreach (KeyValuePair<string, Cache> pair in index)
            {
                if (pair.Value.GetAnchorComponents(gameObjectAnchor) != null)
                {
                    allComponents = allComponents.Union(pair.Value.GetAnchorComponents(gameObjectAnchor)).ToList();
                }
            }

            return allComponents;
        }

        /// <summary>
        /// Testing method. Writes the result cache to a file.
        /// </summary>
        /// <param name="path">Path to the output file.</param>
        public void WriteToFile(string path)
        { 
            foreach (KeyValuePair<string, Cache> pair in index)
            {
                pair.Value.PrintCache(path);
            }
        }
    }
}