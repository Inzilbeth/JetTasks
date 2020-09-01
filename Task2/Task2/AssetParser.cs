using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace Task2
{
    public class AssetParser
    {
        private Cache result;
        private Dictionary<string, DateTime?> timestamp;
        private StreamReader file;

        private const int CHECKAMOUNT = 20;
        private string currentString;
        private int currentStringNumber;
        private ulong currentAnchor;
        private bool insideGameObject;
        private int nodesChecked;

        private Regex idRegex;
        private Regex guidRegex;
        private Regex headerBeginningRegex;
        private Regex headerRegex;

        private const string idPattern = @"{fileID: (\d+)}";
        private const string guidPattern = @"guid: (\w+)";
        private const string headerBeginningPattern = "---";
        private const string headerPattern = @"--- !u!(\d+) &(\d+)";
        private const string componentsPattern = @"m_Component";

        public AssetParser()
        {
            currentStringNumber = 0;
            idRegex = new Regex(idPattern);
            guidRegex = new Regex(guidPattern);
            headerBeginningRegex = new Regex(headerBeginningPattern);
            headerRegex = new Regex(headerPattern);
            timestamp = new Dictionary<string, DateTime?>();
            result = new Cache();
        }

        /// <summary>
        /// Builds a cache of an asset.
        /// </summary>
        /// <param name="path">Path to the asset.</param>
        /// <param name="interruptChecker">Interrupt checker.</param>
        /// <returns>Built cache.</returns>
        public object Build(string path, Action interruptChecker)
        {
            file = File.OpenText(path);
            DateTime fileTimestamp = File.GetLastWriteTime(path);

            if(!timestamp.ContainsKey(path))
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

            file.Close();
            return result;
        }

        /// <summary>
        /// Reads a line from file & increases <see cref="currentStringNumber"/> by one.
        /// </summary>
        /// <returns>Read line.</returns>
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
                    ParseAnchor(interruptChecker);
                    ParseAnchorBody(interruptChecker);
                }
            }
        }

        /// <summary>
        /// Parses anchor's body.
        /// </summary>
        /// <param name="InterruptChecker">Checks if interrpution is called, throws
        /// 'OperationCanceledException' if so.</param>
        private void ParseAnchorBody(Action InterruptChecker)
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
        /// Begins anchor parse.
        /// </summary>
        private void ParseAnchor(Action interruptChecker)
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

            ParseAnchorBody(interruptChecker);
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
                if (match.Success)
                {
                    result.AddAnchorComponent(currentAnchor, Convert.ToUInt64(match.Groups[1].Value));
                }
            }
        }
    }
}
