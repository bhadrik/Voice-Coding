using System;
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

        //private static int index = 0;
        //private static SortedSet<string> dataList = new SortedSet<string>();

        static CPPGrammar()
        {
            //string[] doubleData = DataResource.data.Replace("\r","").Replace("\n","^").Split('^');
            dictionary = new Dictionary<string, string>();

            //foreach (string str in doubleData)
            //{
            //    string[] single = str.Split(':');
            //    dictionary.Add(new KeyValuePair<string, string>(single[0], single[1]));
            //}

            

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
            doc.Load(@"D:\Coding Project\Voice Coding\res\MainResource.xml");

            XmlNodeList nodeList = doc.GetElementsByTagName(attribute.ToString());

            SortedSet<string> dataList = new SortedSet<string>();
            //Dictionary<string, string> dataList = new Dictionary<string, string>();

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



            //if (attribute == Attribute.HeaderFiles)
            //{
            //    XmlNodeList nodeList = doc.GetElementsByTagName("HeaderFiles");

            //    foreach (XmlNode node in nodeList)
            //    {
            //        if (node.Attributes["name"]?.InnerText == "Path")
            //        {
            //            DirectoryInfo dir = new DirectoryInfo(node.Attributes["value"]?.InnerText);
            //            DirectoryInfo[] dirArray = new DirectoryInfo[] { dir };
            //            AddToList(dirArray);
            //            Console.WriteLine("Total Header files are : "+headerFilesList.Count);
            //        }

            //    }

            //    Console.WriteLine(Attribute.HeaderFiles.ToString());

            //    string[] str = new string[headerFilesList.Count];

            //    headerFilesList.CopyTo(str);

            //    return new Choices(str);

            //    //DirectoryInfo include = new DirectoryInfo(@"C:\cygwin64\lib\gcc\x86_64-pc-cygwin\10\include");
            //    ////DirectoryInfo include_cpp = new DirectoryInfo(@"C:\cygwin64\lib\gcc\x86_64-pc-cygwin\10\include\c++");

            //    //DirectoryInfo[] mainDirectory = new DirectoryInfo[] { include };

            //    //AddToList(mainDirectory);

            //    //Console.WriteLine("Total Header: " + headerFilesList.Count);
            //}

            //else
            //{
            //    int startIndex = DataResource.database.IndexOf(key)
            //                 + key.Length
            //                 + 2;
            //    int endIndex = DataResource.database.IndexOf("}", startIndex) - 2;

            //    string[] send = DataResource.database.Substring(startIndex, endIndex - startIndex).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            //    for (int i = 0; i < send.Length; i++)
            //    {
            //        send[i] = send[i].TrimEnd('\r', '\n');
            //    }

            //    return new Choices(send);
            //}
        }

        public static Grammar GetGrammar
        {
            get {
                Grammar grammar = new Grammar((GrammarBuilder)AllRules)
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