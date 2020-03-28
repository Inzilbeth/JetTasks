using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Task2
{
    class AssetCache : IAssetCache
    {
        private const int CHECKAMOUNT = 20;

        private Cache result;
        private string currentString;
        private int currentStringNumber;
        private ulong currentAnchor;
        private StreamReader file;
        private bool insideGameObject;
        private int nodesChecked;
        DateTime? timestamp;

        public AssetCache()
        {
            result = new Cache();
            currentString = null;
            currentAnchor = 0;
            file = null;
            insideGameObject = false;
            nodesChecked = 0;
            timestamp = null;
        }


        public Cache Build(string path, Action interruptChecker)
        {
            file = File.OpenText(path);
            DateTime fileTimestamp = File.GetLastWriteTime(path);
            if (timestamp == null)
            {
                timestamp = fileTimestamp;
            }
            if (timestamp == fileTimestamp)
            {
                ContinueBuild(interruptChecker);
            }
            
            NewBuild(interruptChecker);
            
            return result;
        }

        private string ReadLine()
        {
            currentStringNumber++;
            return file.ReadLine();
        }

        private void NewBuild(Action interruptChecker)
        {
            currentStringNumber = 0;

            ReadLine();
            ReadLine();

            if ((currentString = ReadLine()) != null)
            {
                ParseObjectHeader();
            }

            InvokeNodeParsing(interruptChecker);
        }

        private void ContinueBuild(Action interruptChecker)
        {

        }

        private void InvokeNodeParsing(Action InterruptChecker)
        {
            var headerRegex = new Regex("---");
            while ((currentString = ReadLine()) != null && !headerRegex.IsMatch(currentString))
            {
                ParseObjectField();
                // object body (looking for {... guid: ...} / {file id: })
                // or if IsGameObject -> m_Components 
            }

            nodesChecked++;

            // interruption handler
            if (nodesChecked == CHECKAMOUNT)
            {
                try
                {
                    InterruptChecker();
                }
                catch
                {
                    return;
                }
            }

            if (currentString != null)
            {
                ParseObjectHeader();
                InvokeNodeParsing(InterruptChecker);
            }
        }

        private void ParseObjectHeader()
        {
            insideGameObject = false;
            string headerPattern = @"--- !u!(\d+) &(\d+)";
            var result = new Regex(headerPattern);
            Match match = result.Match(currentString);
            if (match.Success)
            {
                currentAnchor = Convert.ToUInt64(match.Groups[2].Value);
                this.result.AddAnchorUsage(currentAnchor);
                if (Convert.ToUInt64(match.Groups[1].Value) == 1)
                {
                    insideGameObject = true;
                }
            }
        }

        private void ParseObjectField()
        {
            // already have a string <!>
            // found guid -> add count, quit
            // found file id - > add count, quit
            // check if in a gameobject rightnow, if so, check for m_Components and call ParseCompField
            string idPattern = @"{fileID: (\d+)}";
            string guidPattern = @"guid: (\w+)";
            var idRegex = new Regex(idPattern);
            var guidRegex = new Regex(guidPattern);
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
                string componentsPattern = @"m_Component";
                if (Regex.IsMatch(currentString, componentsPattern))
                {
                    ParseComponents();
                }
            }
        }

        private void ParseComponents()
        {
            //doesn't have a string
            while (Regex.IsMatch(currentString = ReadLine(), "-"))
            {
                string idPattern = @"{fileID: (\d+)}";
                var idRegex = new Regex(idPattern);
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
            throw new NotImplementedException();
        }

        public int GetGuidUsages(string guid)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ulong> GetComponentsFor(ulong gameObjectAnchor)
        {
            throw new NotImplementedException();
        }

        public void Print()
        {
            result.PrintCache();
        }
    }
}