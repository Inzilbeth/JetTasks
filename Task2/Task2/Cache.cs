﻿using System.Collections.Generic;
using System;
using System.IO;

namespace Task2
{
    public class Cache
    {
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

        public Cache()
        {
            anchors = new Dictionary<ulong, Anchor>();
            resourses = new Dictionary<string, int>();
        }

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

        public void AddAnchorComponent(ulong id, ulong component)
        {
            anchors[id].components.Add(component);
        }

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

        public int GetAnchorUsages(ulong id)
        {
            if (anchors.ContainsKey(id))
            {
                return anchors[id].uses;
            }
            return 0;
        }

        public IEnumerable<ulong> GetAnchorComponents(ulong id)
        {
            if (anchors.ContainsKey(id))
            {
                return anchors[id].components;
            }
            return null;
        }

        public int GetResourceUsgaes(string guid)
        {
            if (resourses.ContainsKey(guid))
            {
                return resourses[guid];
            }
            return 0;
        }

        public void PrintCache()
        {
            StreamWriter sw = new StreamWriter(@"C:\Users\Талгат\Desktop\output.txt");

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
