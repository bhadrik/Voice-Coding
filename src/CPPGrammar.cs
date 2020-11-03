using System;
using System.Collections.Generic;
using System.Speech.Recognition;

namespace Voice_Coding.src
{
    static class CPPGrammar
    {
        public static IDictionary<string, string> dictionary;
        //Choices for differet grammar

        private static readonly GrammarBuilder includeBuilder   = new GrammarBuilder();
        private static readonly GrammarBuilder namespaceBuilder = new GrammarBuilder();
        private static readonly GrammarBuilder functionBuilder  = new GrammarBuilder();
        private static readonly GrammarBuilder printBuilder     = new GrammarBuilder();

        private static readonly Choices AllRules;

        static CPPGrammar()
        {
            string[] doubleData = DataResource.data.Replace("\r","").Replace("\n","^").Split('^');
            dictionary = new Dictionary<string, string>();

            foreach (string str in doubleData)
            {
                string[] single = str.Split(':');
                dictionary.Add(new KeyValuePair<string, string>(single[0], single[1]));
            }

            includeBuilder = new GrammarBuilder();
            includeBuilder.Append("include");
            includeBuilder.Append(GetChoice("Headerfiles"));

            namespaceBuilder = new GrammarBuilder();
            namespaceBuilder.Append("using_namespace");
            namespaceBuilder.Append(GetChoice("namespace"));

            functionBuilder = new GrammarBuilder();
            functionBuilder.Append("function");
            functionBuilder.Append(GetChoice("datatype"));
            functionBuilder.AppendDictation();

            printBuilder = new GrammarBuilder();
            printBuilder.Append(GetChoice("printstyle"));
            printBuilder.Append(GetChoice("printvalue"));
            printBuilder.AppendDictation();

            AllRules = new Choices(
                new GrammarBuilder[] {
                    includeBuilder,
                    namespaceBuilder,
                    functionBuilder,
                    printBuilder
                });
        }

        private static Choices GetChoice(string key)
        {
            key = key.ToUpper();
            int startIndex = DataResource.database.IndexOf(key)
                             + key.Length
                             + 3;
            int endIndex = DataResource.database.IndexOf("}", startIndex) - 2;

            string[] send = DataResource.database.Substring(startIndex, endIndex - startIndex).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < send.Length; i++)
            {
                send[i] = send[i].TrimEnd('\r', '\n');
            }

            return new Choices(send);
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
    }
}