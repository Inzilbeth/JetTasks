using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Task2
{
    class FileParser
    {
        string currentString;
        StreamReader file;
        bool insideGameObject;

        public void Build(string path, Action interruptChecker)
        {
            file = File.OpenText(path);
            
            file.ReadLine();
            file.ReadLine();

            if ((currentString = file.ReadLine()) != null)
            {
                ParseObjectHeader();
            }

            InvokeNodeParsing(interruptChecker);
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

            if (currentString != null)
            {
                ParseObjectHeader();
                InvokeNodeParsing(InterruptChecker);
                try { InterruptChecker(); }
                catch { }
            }
        }

        private void ParseObjectHeader()
        {
            insideGameObject = false;
            string headerPattern = @"--- !u!(\d+) &(\d+)";
            var result = new Regex(headerPattern);
            Match match = result.Match(currentString);
            while (match.Success)
            {
                for (int i = 1; i <= 2; i++)
                {
                    Group group = match.Groups[i];
                    Console.WriteLine(group);
                }
                
                if (Convert.ToInt32(match.Groups[1].Value) == 1)
                {
                    insideGameObject = true;
                }

                match = match.NextMatch();
            }
            Console.WriteLine();
            // add count of match.Group[2] id
            // change currently inside a gameobject to true if match.Group[1] = 1;
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
                Console.WriteLine(matchId.Groups[1]);
            }
            Match matchGuid = guidRegex.Match(currentString);
            if (matchGuid.Success)
            {
                Console.WriteLine(matchGuid.Groups[1]);
            }
            if (insideGameObject)
            {
                string componentsPattern = @"m_Component";
                if(Regex.IsMatch(currentString, componentsPattern))
                {
                    ParseComponents();
                }
            }
        }

        private void ParseComponents()
        {
            //doesn't have a string
            while(Regex.IsMatch(currentString = file.ReadLine(), "-"))
            {
                string idPattern = @"{fileID: (\d+)}";
                var idRegex = new Regex(idPattern);
                Match match = idRegex.Match(currentString);
                while (match.Success)
                {
                    for (int i = 1; i <= match.Groups.Count; i++)
                    {
                        Group group = match.Groups[i];
                        Console.WriteLine(group);
                    }
                    match = match.NextMatch();
                }
            }
        }
    }
}