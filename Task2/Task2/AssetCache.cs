using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Task2
{
    class AssetCache : IAssetCache
    {
        private const int CHECKAMOUNT = -1; // put to 20 on release!

        private Cache result;
        private string currentString;
        private int currentStringNumber;
        private ulong currentAnchor;
        private StreamReader file;
        private bool insideGameObject;
        private int nodesChecked;
        DateTime? timestamp;
        
        Regex idRegex; 
        Regex guidRegex;
        Regex headerBeginningRegex;
        Regex headerRegex;

        string idPattern = @"{fileID: (\d+)}";
        string guidPattern = @"guid: (\w+)";
        string headerBeginningPattern = "---";
        string headerPattern = @"--- !u!(\d+) &(\d+)";
        string componentsPattern = @"m_Component";

        public AssetCache()
        {
            currentString = null;
            currentAnchor = 0;
            file = null;
            insideGameObject = false;
            nodesChecked = 0;
            timestamp = null;
            idRegex = new Regex(idPattern);
            guidRegex = new Regex(guidPattern);
            headerBeginningRegex = new Regex(headerBeginningPattern);
            headerRegex = new Regex(headerPattern);
        }


        public object Build(string path, Action interruptChecker)
        {
            file = File.OpenText(path);
            DateTime fileTimestamp = File.GetLastWriteTime(path);
            
            if (timestamp == fileTimestamp)
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
            timestamp = fileTimestamp;
            try
            {
                NewBuild(interruptChecker);
            }
            catch 
            {
                file.Close();
                return null; 
            }

            Console.WriteLine(currentStringNumber);
            return result;
        }

        private string ReadLine()
        {
            currentStringNumber++;
            return file.ReadLine();
        }

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
            throw new NotImplementedException();
        }

        public int GetLocalAnchorUsages(ulong anchor)
        {
            return result.GetAnchorUsages(anchor);
        }

        public int GetGuidUsages(string guid)
        {
            return result.GetResourceUsgaes(guid);
        }

        public IEnumerable<ulong> GetComponentsFor(ulong gameObjectAnchor)
        {
            return result.GetAnchorComponents(gameObjectAnchor);
        }

        public void WriteToFile()
        {
            result.PrintCache();
        }
    }
}