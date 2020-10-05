using System.Speech.Recognition;

namespace Voice_Coding.src
{
    class CPPGrammar
    {
        //Choices for differet grammar
        private static Choices dataType =
            new Choices(new string[] { "void", "int", "char", "bool", "float", "double" });
        private static Choices printType =
            new Choices(new string[] { "variable", "string" });
        private static Choices printCmdType =
            new Choices(new string[] { "printf", "printline" });
        private static Choices headers =
            new Choices(new string[] { "iostream", "cstdlib", "cmaths", "strnig" });

        private static GrammarBuilder includeBuilder = new GrammarBuilder();
        private static GrammarBuilder functionBuilder = new GrammarBuilder();
        private static GrammarBuilder printBuilder = new GrammarBuilder();

        public static void InitializeDefaultGrammer()
        {
            includeBuilder.Append("include");
            includeBuilder.Append(headers);

            functionBuilder.Append("function");
            functionBuilder.Append(dataType);
            functionBuilder.AppendDictation();

            printBuilder.Append(printCmdType);
            printBuilder.Append(printType);
            printBuilder.AppendDictation();
        }

        public static Grammar Include
        {
            get { return new Grammar(includeBuilder); }
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
