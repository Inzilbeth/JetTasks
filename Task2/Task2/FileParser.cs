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
        //int  timestamp

        public AssetCache()
        {
            result = new Cache();
            currentString = null;
            currentStringNumber = 0;
            currentAnchor = 0;
            file = null;
            insideGameObject = false;
            nodesChecked = 0;
            //timestamp null
        }


        public Cache Build(string path, Action interruptChecker)
        {
            file = File.OpenText(path);
            //switch timestamp 
            // case null
            //  timestamp = file.timestamp;
            // case file.timestamp
            //  ContinueBuild();
            // default
            //  NewBuild();
            file.ReadLine();
            currentStringNumber++;
            file.ReadLine();
            currentStringNumber++;

            if ((currentString = file.ReadLine()) != null)
            {
                ParseObjectHeader();
            }

            InvokeNodeParsing(interruptChecker);

            return result;
        }

        private void InvokeNodeParsing(Action InterruptChecker)
        {
            var headerRegex = new Regex("---");
            while ((currentString = file.ReadLine()) != null && !headerRegex.IsMatch(currentString))
            {
                ParseObjectField();
                // object body (looking for {... guid: ...} / {file id: })
                // or if IsGameObject -> m_Components 
            }

            nodesChecked++;

            if (currentString != null)
            {
                ParseObjectHeader();
                InvokeNodeParsing(InterruptChecker);

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
            while (Regex.IsMatch(currentString = file.ReadLine(), "-"))
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