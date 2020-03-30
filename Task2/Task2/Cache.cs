using System.Collections.Generic;
using System.IO;

/// <summary>
/// Default namespace.
/// </summary>
namespace Task2
{
    /// <summary>
    /// Implementation of cache class.
    /// </summary>
    public class Cache
    {
        /// <summary>
        /// Scene's anchor.
        /// </summary>
        private class Anchor
        {
            public int uses;
            public List<ulong> components;

            public Anchor()
            {
                uses = 1;
                components = new List<ulong>();
            }
        }

        private Dictionary<ulong, Anchor> anchors;
        private Dictionary<string, int> resourses;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Cache()
        {
            anchors = new Dictionary<ulong, Anchor>();
            resourses = new Dictionary<string, int>();
        }

        /// <summary>
        /// Adds one usage to the anchor.
        /// </summary>
        /// <param name="id">Anchor's id</param>
        public void AddAnchorUsage(ulong id)
        {
            if (anchors.ContainsKey(id))
            {
                anchors[id].uses++;
            }
            else
            {
                anchors.Add(id, new Anchor());
            }
        }

        /// <summary>
        /// Adds a component to the anchor's components list.
        /// </summary>
        /// <param name="id">Anchor's id.</param>
        /// <param name="component">Component's id.</param>
        public void AddAnchorComponent(ulong id, ulong component)
        {
            if (anchors.ContainsKey(id))
            {
                anchors[id].components.Add(component);
            }
            else
            {
                AddAnchorUsage(id);
                anchors[id].components.Add(component);
            }
        }

        /// <summary>
        /// Adds a resource usage.
        /// </summary>
        /// <param name="guid">Resource's guid.</param>
        public void AddResourceUsage(string guid)
        {
            if (resourses.ContainsKey(guid))
            {
                resourses[guid]++;
            }
            else
            {
                resourses.Add(guid, 1);
            }
        }

        /// <summary>
        /// Gets the amount of anchor usages.
        /// </summary>
        /// <param name="id">Anchor's id.</param>
        /// <returns>Amount of anchor's usages.</returns>
        public int GetAnchorUsages(ulong id)
        {
            if (anchors.ContainsKey(id))
            {
                return anchors[id].uses;
            }
            return 0;
        }

        /// <summary>
        /// Gets the list of anchor components.
        /// </summary>
        /// <param name="id">Anchor's id.</param>
        /// <returns>List of anchor components.</returns>
        public List<ulong> GetAnchorComponents(ulong id)
        {
            if (anchors.ContainsKey(id))
            {
                return anchors[id].components;
            }
            return null;
        }

        /// <summary>
        /// Gets the amount of resource usages.
        /// </summary>
        /// <param name="guid">Resource's guid.</param>
        /// <returns>Amount of usages.</returns>
        public int GetResourceUsgaes(string guid)
        {
            if (resourses.ContainsKey(guid))
            {
                return resourses[guid];
            }
            return 0;
        }

        /// <summary>
        /// Prints cache to a file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        public void PrintCache(string path)
        {
            StreamWriter sw = new StreamWriter(path);

            sw.WriteLine("Anchors: ");
            foreach (KeyValuePair<ulong, Anchor> pair in anchors)
            {
                sw.WriteLine($"ID: {pair.Key}");
                sw.WriteLine($"Uses: {pair.Value.uses}");
                sw.WriteLine("Components: ");
                foreach (ulong id in pair.Value.components)
                    sw.WriteLine(id);
            }
            sw.WriteLine();
            sw.WriteLine("Resources: ");
            
            foreach (KeyValuePair<string, int> pair in resourses)
            {
                sw.WriteLine($"Guid: {pair.Key}");
                sw.WriteLine($"Uses: {pair.Value}");
            }

            sw.Close();
        }
    }
}
