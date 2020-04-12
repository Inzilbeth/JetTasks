using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Task2
{
    /// <summary>
    /// Implementation of AsserCache class.
    /// </summary>
    public class AssetCache : IAssetCache
    {
        private const int CHECKAMOUNT = 20;
        private Dictionary<string, Cache> index;
        private Cache result;
        private string currentString;
        private int currentStringNumber;
        private ulong currentAnchor;
        private StreamReader file;
        private bool insideGameObject;
        private int nodesChecked;

        private Dictionary<string, DateTime?> timestamp;
        
        private Regex idRegex; 
        private Regex guidRegex;
        private Regex headerBeginningRegex;
        private Regex headerRegex;

        private const string idPattern = @"{fileID: (\d+)}";
        private const string guidPattern = @"guid: (\w+)";
        private const string headerBeginningPattern = "---";
        private const string headerPattern = @"--- !u!(\d+) &(\d+)";
        private const string componentsPattern = @"m_Component";

        /// <summary>
        /// AssetCache constructor.
        /// </summary>
        public AssetCache()
        {
            index = new Dictionary<string, Cache>();
            timestamp = new Dictionary<string, DateTime?>();
            currentString = null;
            currentAnchor = 0;
            file = null;
            insideGameObject = false;
            nodesChecked = 0;
            idRegex = new Regex(idPattern);
            guidRegex = new Regex(guidPattern);
            headerBeginningRegex = new Regex(headerBeginningPattern);
            headerRegex = new Regex(headerPattern);
        }


        public object Build(string path, Action interruptChecker)
        {
            file = File.OpenText(path);
            DateTime fileTimestamp = File.GetLastWriteTime(path);

            try
            {
                timestamp.Add(path, fileTimestamp);
                try
                {
                    NewBuild(interruptChecker);
                }
                catch
                {
                    file.Close();
                    return null;
                }
            }
            catch (ArgumentException)
            {
                if (timestamp[path] == fileTimestamp)
                {
                    try
                    {
                        ContinueBuild(interruptChecker);
                    }
                    catch
                    {
                        file.Close();
                        return null;
                    }
                }
                else
                {
                    timestamp[path] = fileTimestamp;
                    try
                    {
                        NewBuild(interruptChecker);
                    }
                    catch
                    {
                        file.Close();
                        return null;
                    }
                }
            }
            

            /**/
            file.Close();
            Merge(path, result);
            return result;
        }

        /// <summary>
        /// Reads a line from file & increases <see cref="currentStringNumber"/> by one.
        /// </summary>
        /// <returns></returns>
        private string ReadLine()
        {
            currentStringNumber++;
            return file.ReadLine();
        }

        /// <summary>
        /// Start parsing from the beginning.
        /// </summary>
        /// <param name="interruptChecker">Checks if interrpution is called, throws
        /// 'OperationCanceledException' if so.</param>
        private void NewBuild(Action interruptChecker)
        {
            result = new Cache();

            currentStringNumber = 0;

            ReadLine();
            ReadLine();

            if ((currentString = ReadLine()) != null)
            {
                while (currentString != null)
                {
                    ParseAnchorHeader();
                    ParseAnchor(interruptChecker);
                }
            }
        }

        /// <summary>
        /// Continue parsing from the <see cref="currentStringNumber"/>'th line.
        /// </summary>
        /// <param name="interruptChecker">Checks if interrpution is called, throws
        /// 'OperationCanceledException' if so.</param>
        private void ContinueBuild(Action interruptChecker)
        {
            for (int i = 1; i < currentStringNumber; i++)
            {
                file.ReadLine();
            }
            if ((currentString = file.ReadLine()) != null)
            {
                while (currentString != null)
                {
                    ParseAnchorHeader();
                    ParseAnchor(interruptChecker);
                }
            }
            
        }

        /// <summary>
        /// Parses one anchor.
        /// </summary>
        /// <param name="InterruptChecker">Checks if interrpution is called, throws
        /// 'OperationCanceledException' if so.</param>
        private void ParseAnchor(Action InterruptChecker)
        {
            
            while ((currentString = ReadLine()) != null && !headerBeginningRegex.IsMatch(currentString))
            {
                ParseAnchorField();
            }

            nodesChecked++;
            
            if (nodesChecked == CHECKAMOUNT)
            {
                InterruptChecker();
            }
        }

        /// <summary>
        /// Parses a line started by "--- !u!" 
        /// </summary>
        private void ParseAnchorHeader()
        {
            insideGameObject = false;

            Match match = headerRegex.Match(currentString);
            if (match.Success)
            {
                currentAnchor = Convert.ToUInt64(match.Groups[2].Value);
                result.AddAnchorUsage(currentAnchor);
                if (Convert.ToUInt64(match.Groups[1].Value) == 1)
                {
                    insideGameObject = true;
                }
            }
        }

        /// <summary>
        /// Parses a line within an anchor.
        /// </summary>
        private void ParseAnchorField()
        {
            Match matchId = idRegex.Match(currentString);
            if (matchId.Success)
            {
                result.AddAnchorUsage(Convert.ToUInt64(matchId.Groups[1].Value));
            }
            Match matchGuid = guidRegex.Match(currentString);
            
            if (matchGuid.Success)
            {
                result.AddResourceUsage(Convert.ToString(matchGuid.Groups[1].Value));
            }
            
            if (insideGameObject)
            {
                
                if (Regex.IsMatch(currentString, componentsPattern))
                {
                    ParseComponents();
                }
            }
        }

        /// <summary>
        /// Parses components after "m_Components:" line within an anchor.
        /// </summary>
        private void ParseComponents()
        {
            while (Regex.IsMatch(currentString = ReadLine(), "-"))
            {
                Match match = idRegex.Match(currentString);
                if(match.Success)
                {
                    result.AddAnchorComponent(currentAnchor, Convert.ToUInt64(match.Groups[1].Value));
                }
            }
        }

        public void Merge(string path, object result)
        {
            try
            {
                index.Add(path, result as Cache);
            }
            catch (ArgumentException)
            {
                index[path] = result as Cache;
            }
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
                allComponents = allComponents.Union(pair.Value.GetAnchorComponents(gameObjectAnchor)).ToList();
            }
            return allComponents;
        }

        /// <summary>
        /// Testing method. Writes the result cache to a file.
        /// </summary>
        /// <param name="path">Path to the output file.</param>
        public void WriteToFile(string path)
        {
            result.PrintCache(path);
        }
    }
}