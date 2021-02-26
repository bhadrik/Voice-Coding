//#define debug

using System.Collections.Generic;
using System.Speech.Recognition;
using System.Xml;
using System.IO;
using System;

namespace Voice_Coding.src
{
    class CPPGrammar
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

        private readonly GrammarBuilder[] Builder;

        private readonly Choices AllRules;

        private readonly Choices defaultChoice = new Choices(new string[] { "<Default Choice>" });

        public CPPGrammar()
        {
            dictionary = new Dictionary<string, string>();

            Builder = new GrammarBuilder[5];

            //Include builder
            Builder[0] = new GrammarBuilder();
            Builder[0].Append("include");
            Builder[0].Append(GetChoice(Attribute.HeaderFiles));

            //Namespace builder
            Builder[1] = new GrammarBuilder();
            Builder[1].Append("using_namespace");
            Builder[1].Append(GetChoice(Attribute.Namespace));

            //Function builder
            Builder[2] = new GrammarBuilder();
            Builder[2].Append("function");
            Builder[2].Append(GetChoice(Attribute.Datatype));
            Builder[2].AppendDictation();

            //Print builder
            Builder[3] = new GrammarBuilder();
            Builder[3].Append(GetChoice(Attribute.PrintStyle));
            Builder[3].Append(GetChoice(Attribute.PrintValue));
            Builder[3].AppendDictation();

            //Variable builder
            Builder[4] = new GrammarBuilder();
            Builder[4].Append("add");
            Builder[4].Append(GetChoice(Attribute.Datatype));
            Builder[4].AppendDictation();

            AllRules = new Choices(Builder);
        }

        private Choices GetChoice(Attribute attribute)
        {
            XmlDocument doc = new XmlDocument();

#if debug
            doc.Load(@"..\..\res\MainResource.xml");
#else
            doc.Load(@"Resources\MainResource.xml");
#endif

            XmlNodeList nodeList = doc.GetElementsByTagName(attribute.ToString());

            SortedSet<string> dataList = new SortedSet<string>();

            if(attribute == Attribute.HeaderFiles)
            {
                foreach (XmlNode node in nodeList)
                {
                    if (node.Attributes["name"].Value.Equals("Path") && node.Attributes["type"].Value.Equals("custom"))
                    {
                        Console.WriteLine("Reading:" + node.Attributes["value"].Value);
                        DirectoryInfo dir = new DirectoryInfo(node.Attributes["value"].Value);

                        if (!dir.Exists)
                        {
                            Console.WriteLine("Directory not found, please provide existing directory path");
                            return defaultChoice;
                        }

                        DirectoryInfo[] dirArray = new DirectoryInfo[] { dir };
                        AddToList(dirArray, dataList);
                    }
                }
            }
            else
            {
                foreach (XmlNode node in nodeList)
                {
                    if(!dictionary.ContainsKey(node.Attributes["name"].Value))
                    dictionary.Add(node.Attributes["name"].Value, node.Attributes["value"].Value);
                    dataList.Add(node.Attributes["name"].Value);
                }
            }

            string[] str = new string[dataList.Count];
            dataList.CopyTo(str);

            return new Choices(str);
        }

        public Grammar GetGrammar
        {
            get {
                Grammar grammar = new Grammar(AllRules)
                {
                    Name = "VoiceCoding_CPP",
                };
                return grammar;
            }
        }

        private void AddToList(DirectoryInfo[] directoryInfos, SortedSet<string> list)
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