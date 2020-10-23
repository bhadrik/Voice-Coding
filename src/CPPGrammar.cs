using System;
using System.Collections.Generic;
using System.Speech.Recognition;

namespace Voice_Coding.src
{
    class CPPGrammar
    {
        public static IDictionary<string, string> dictionary;
        //Choices for differet grammar

        private static readonly GrammarBuilder includeBuilder = new GrammarBuilder();
        private static readonly GrammarBuilder namespaceBuilder = new GrammarBuilder();
        private static readonly GrammarBuilder functionBuilder = new GrammarBuilder();
        private static readonly GrammarBuilder printBuilder = new GrammarBuilder();

        public static void InitializeDefaultGrammer()
        {
            //Console.WriteLine(temp);
            string[] doubleData = Resources.data.Split('\n');
            dictionary = new Dictionary<string, string>();

            foreach (string str in doubleData)
            {
                string temp = str.TrimEnd('\r', '\n');
                string[] single = temp.Split(':');

                //Console.WriteLine(single[1]);
                dictionary.Add(new KeyValuePair<string, string>(single[0], single[1]));
            }

            includeBuilder.Append("include");
            includeBuilder.Append(getChoice("Headerfiles"));

            namespaceBuilder.Append("using namespace");
            //namespaceBuilder.Append("namespace");
            namespaceBuilder.Append(getChoice("namespace"));

            functionBuilder.Append("function");
            functionBuilder.Append(getChoice("Datatype"));
            functionBuilder.AppendDictation();

            printBuilder.Append(getChoice("Printstyle"));
            printBuilder.AppendDictation();
        }

        private static Choices getChoice(string key)
        {
            key = key.ToUpper();
            int startIndex = Resources.database.IndexOf(key) + key.Length + 3;
            int endIndex = Resources.database.IndexOf("}", startIndex) - 2;

            string[] send = Resources.database.Substring(startIndex, endIndex - startIndex).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            //Console.WriteLine("This is function call:::");
            for(int i=0; i<send.Length; i++)
            {
                send[i] = send[i].TrimEnd('\r', '\n');
            }
            //foreach(string str in send)
                //Console.WriteLine(str);
            Console.WriteLine("Total sent : " + send.Length);

            return new Choices(send);
        }

        public static Grammar Include
        {
            get { return new Grammar(includeBuilder); }
        }

        public static Grammar Namespace
        {
            get { return new Grammar(namespaceBuilder); }
        }

        public static Grammar Function
        {
            get { return new Grammar(functionBuilder); }
        }

        public static Grammar Print
        {
            get { return new Grammar(printBuilder); }
        }
    }
}
