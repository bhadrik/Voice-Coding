using System.Collections.Generic;
using System.Speech.Recognition;
using System.Xml;
using System.IO;

namespace Voice_Coding.src
{
    static class CPPGrammar
    {
        public static IDictionary<string, string> dictionary;
        //Choices for differet grammar

        enum Attribute
        {
            HeaderFiles,
            Namespace,
            Datatype,
            PrintStyle,
            PrintValue
        }

        private static readonly GrammarBuilder includeBuilder   = new GrammarBuilder();
        private static readonly GrammarBuilder namespaceBuilder = new GrammarBuilder();
        private static readonly GrammarBuilder functionBuilder  = new GrammarBuilder();
        private static readonly GrammarBuilder printBuilder     = new GrammarBuilder();

        private static readonly Choices AllRules;

        static CPPGrammar()
        {
            dictionary = new Dictionary<string, string>();

            includeBuilder = new GrammarBuilder();
            includeBuilder.Append("include");
            includeBuilder.Append(GetChoice(Attribute.HeaderFiles));

            namespaceBuilder = new GrammarBuilder();
            namespaceBuilder.Append("using_namespace");
            namespaceBuilder.Append(GetChoice(Attribute.Namespace));

            functionBuilder = new GrammarBuilder();
            functionBuilder.Append("function");
            functionBuilder.Append(GetChoice(Attribute.Datatype));
            functionBuilder.AppendDictation();

            printBuilder = new GrammarBuilder();
            printBuilder.Append(GetChoice(Attribute.PrintStyle));
            printBuilder.Append(GetChoice(Attribute.PrintValue));
            printBuilder.AppendDictation();

            AllRules = new Choices(
                new GrammarBuilder[] {
                    includeBuilder,
                    namespaceBuilder,
                    functionBuilder,
                    printBuilder
                });
        }

        private static Choices GetChoice(Attribute attribute)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(DataResource.MainResource);

            XmlNodeList nodeList = doc.GetElementsByTagName(attribute.ToString());

            SortedSet<string> dataList = new SortedSet<string>();

            if(attribute == Attribute.HeaderFiles)
            {
                foreach (XmlNode node in nodeList)
                {
                    if (node.Attributes["name"]?.InnerText == "Path")
                    {
                        DirectoryInfo dir = new DirectoryInfo(node.Attributes["value"]?.InnerText);
                        DirectoryInfo[] dirArray = new DirectoryInfo[] { dir };
                        AddToList(dirArray, dataList);
                    }
                }
            }
            else
            {
                foreach (XmlNode node in nodeList)
                {
                    dictionary.Add(node.Attributes["name"]?.InnerText, node.Attributes["value"]?.InnerText);
                    dataList.Add(node.Attributes["name"]?.InnerText);
                }
            }

            string[] str = new string[dataList.Count];
            int tlength = str.Length;
            dataList.CopyTo(str);

            return new Choices(str);
        }

        public static Grammar GetGrammar
        {
            get {
                Grammar grammar = new Grammar(AllRules)
                {
                    Name = "VoiceCoding_CPP",
                };
                return grammar;
            }
        }

        private static void AddToList(DirectoryInfo[] directoryInfos, SortedSet<string> list)
        {
            if(directoryInfos.Length == 0)
            {
                return;
            }
            foreach (DirectoryInfo dir in directoryInfos)
            {
                FileInfo[] files = dir.GetFiles("*");
                foreach (FileInfo file in files)
                {
                    list.Add(file.Name);
                }
                AddToList(dir.GetDirectories(), list);
            }
        }
    }
}