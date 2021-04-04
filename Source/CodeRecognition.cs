#define debug

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Snippets;
using System;
using System.Globalization;
using System.Speech.Recognition;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Text;

namespace Voice_Coding.Source
{
    public class IdentifiedArgs : EventArgs
    {
        private string text;
        private int breakAtOffset;

        public IdentifiedArgs(string arg, int breakHere)
        {
            text = arg;
            breakAtOffset = breakHere;
        }

        public string Text
        {
            get
            {
                return text;
            }
        }

        public int Offset
        {
            get
            {
                return breakAtOffset;
            }
        }
    }
    struct cLocation
    {
        public cLocation(int l, int c, int o)
        {
            Line = l;
            Column = c;
            Offset = o;
        }
        public int Line { get; set; }
        public int Column{ get; set; }
        public int Offset{ get;set; }

        public void Set(int l, int c, int o)
        {
            Line = l;
            Column = c;
            Offset = o;
        }
        public TextLocation ToTextLocation()
        {
            return new TextLocation(Line, Column);
        }
    }
    enum Section
    {
        Header,
        Class,
        Global,
        Local
    }

    class CodeRecognition
    {
        #region Var declaration
        public bool recognising;
        private readonly SpeechRecognitionEngine rec;
        private readonly MainWindow window;
        private readonly string[] cmd;
        //private int                                 level = 0;
        //public  event    EventHandler               ExitEvent;
        //private          TextEditor                 window.textEditor;
        //private          Label                      window.status;
        private int[] iPointer;
        private cLocation[] location;
        private TextEditor Code;
        private ICSharpCode.AvalonEdit.Editing.Caret Caret;

        public EventHandler<IdentifiedArgs> Identified;

        #endregion

        public CodeRecognition(MainWindow window)
        {
            ///<summery>
            ///iPointer[0] -> Header section pointer
            ///iPointer[1] -> Class section pointer
            ///iPointer[2] -> Global function pointer
            ///</summery>
            ///
            iPointer = new int[4];
            iPointer[0] = 0;
            iPointer[1] = 0;
            iPointer[2] = 0;

            location = new cLocation[4] {
                new cLocation(1, 1, 0),
                new cLocation(1, 1, 0),
                new cLocation(1, 1, 0),
                new cLocation(1, 1, 0)
            };

            Code = window.textEditor;
            Caret = window.textEditor.TextArea.Caret;

            XmlDocument doc = new XmlDocument();
#if debug
            doc.Load(@"..\..\Resource\MainResource.xml");
#else
            doc.Load(@"Resource\MainResource.xml");
#endif

            this.window = window;

            XmlNodeList nodeList = doc.GetElementsByTagName("Command");

            cmd = new string[nodeList.Count];

            int i = 0;
            foreach (XmlNode node in nodeList)
            {
                cmd[i] = node.Attributes["value"].Value;
                i++;
            }

            //CPPGrammar.InitializeDefaultGrammer();
            rec = new SpeechRecognitionEngine(new CultureInfo("en-US"));
            rec.SetInputToDefaultAudioDevice();
            rec.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(cmd))));
            rec.LoadGrammarAsync(new CPPGrammar().GetGrammar);


            //All event handler
            rec.SpeechRecognized +=
                new EventHandler<SpeechRecognizedEventArgs>(Rec_Recognised);
            rec.SpeechDetected +=
                new EventHandler<SpeechDetectedEventArgs>(Rec_Detected);
            rec.RecognizeCompleted +=
                new EventHandler<RecognizeCompletedEventArgs>(Rec_Completed);
            rec.AudioLevelUpdated +=
                new EventHandler<AudioLevelUpdatedEventArgs>(Rec_AudioUpdate);
        }

        private void Rec_Recognised(object sender, SpeechRecognizedEventArgs e)
        {
            string[] words = e.Result.Text.Split(' ');
            string DictionaryValue = "<NOT SETED>", DictionaryKey = "<NOT SETED>";

            int length = window.textEditor.Text.Length;

            StringBuilder str = new StringBuilder();
            int temp = 0;

            cLocation returnLocatioin = new cLocation();
            returnLocatioin.Line = window.textEditor.TextArea.Caret.Line;
            returnLocatioin.Column = window.textEditor.TextArea.Caret.Column;

            Console.WriteLine("CMD: " + words[0]);

            if (words.Length > 1)
            {
                DictionaryValue = FindInDictionary(words[1]);
                DictionaryKey = e.Result.Text.Replace(words[0] + " " + words[1] + " ", "");
            }

            //statusBar.ChangeText($"{e.Result.Text} [{e.Result.Confidence}] [{e.Result.Grammar.Name}]");

            //Console.WriteLine("Caret.Location: " + window.textEditor.TextArea.Caret.Location.Line + ", " + window.textEditor.TextArea.Caret.Location.Column);
            switch (words[0])
            {
                case "now":
                    Console.WriteLine("Caret: "
                        + window.textEditor.TextArea.Caret.Location.Line + ","
                        + window.textEditor.TextArea.Caret.Location.Column + ","
                        + window.textEditor.TextArea.Caret.Offset);
                    Console.WriteLine("Header: "
                        + location[(int)Section.Header].Line + ","
                        + location[(int)Section.Header].Column + ","
                        + location[(int)Section.Header].Offset);
                    Console.WriteLine("Class: "
                        + location[(int)Section.Class].Line + ","
                        + location[(int)Section.Class].Column + ","
                        + location[(int)Section.Class].Offset);
                    Console.WriteLine("Global: "
                        + location[(int)Section.Global].Line + ","
                        + location[(int)Section.Global].Column + ","
                        + location[(int)Section.Global].Offset);
                    break;

                //INCLUDE "file_name"  2
                case "include":
                    str.Append("#include<" + words[1] + ">" + ";\r\n");
                    Console.WriteLine(str.ToString());
                    window.textEditor.Document.BeginUpdate();
                    window.textEditor.Document.Insert(Caret.Offset, str.ToString());
                    window.textEditor.Document.EndUpdate();
                    break;

                //USING_NAMESPACE "name_of_namespace"  2
                case "using_namespace":
                    str.Append("using namespace " + FindInDictionary(words[1]) + ";\r\n");
                    window.textEditor.Document.BeginUpdate();
                    window.textEditor.Document.Insert(Caret.Offset, str.ToString());
                    window.textEditor.Document.EndUpdate();
                    //Caret.Offset = Code.Text.Length;
                    break;

                //FUNCTION "data_type" "Function_name" 3
                case "function":
                    window.textEditor.Document.BeginUpdate();
                    str.Append(FindInDictionary(words[1]) + " " + words[2] + "(){\r\n\t");
                    window.textEditor.Document.Insert(Caret.Offset, str.ToString());
                    temp = Caret.Offset;
                    //Caret.Offset = Code.Text.Length;
                    str.Clear();
                    str.Append("\n}");
                    window.textEditor.Document.Insert(Caret.Offset, str.ToString());
                    window.textEditor.Document.EndUpdate();
                    Caret.Offset = temp;
                    break;

                case "for_loop":
                    window.textEditor.Document.BeginUpdate();
                    str.Append("for(int i=0; i<0; i++)\r\n{\r\n\t/*Body*/\r\n}");
                    window.textEditor.Document.Insert(Caret.Offset, str.ToString());
                    window.textEditor.Document.EndUpdate();
                    break;

                //PRINT_LINE STRING/VAR "data_to_be_printed"
                //cout<<word[0]+word[1]...world[n]<<endl;
                case "print_line":
                    str.Append("cout<<");
                    window.textEditor.Document.BeginUpdate();
                    window.textEditor.Document.Insert(Caret.Offset, str.ToString());
                    temp = Caret.Offset;

                    str.Clear();
                    str.Append("<<endl;");
                    window.textEditor.Document.Insert(Caret.Offset, str.ToString());
                    window.textEditor.Document.EndUpdate();
                    Caret.Offset = temp;
                    break;

                case "add":
                    str.Append(FindInDictionary(words[1])+" "+words[2]+";\r\n");
                    window.textEditor.Document.Insert(Caret.Offset, str.ToString());
                    /*temp = Caret.Offset;
                    str.Clear();

                    str.Append(";");
                    window.textEditor.Document.Insert(Caret.Offset, str.ToString());
                    Caret.Offset = temp;*/
                    break;

                //PRINT STRING/VAR "data_to_be_printed" 3+
                case "print":
                    str.Append("cout<<");
                    window.textEditor.Document.BeginUpdate();
                    window.textEditor.Document.Insert(Caret.Offset, str.ToString());
                    temp = Caret.Offset;

                    str.Clear();
                    str.Append(";");
                    window.textEditor.Document.Insert(Caret.Offset, str.ToString());
                    window.textEditor.Document.EndUpdate();
                    Caret.Offset = temp;
                    break;

                case "undo":
                    window.textEditor.Undo();
                    break;

                case "back":
                    if (Caret.Offset > 0)
                        Code.Text = Code.Text.Substring(0, Code.Text.Length-1);
                    break;

                case "erase":
                    window.status.Text = "<NEED TO BE IMPLEMENTED>";
                    break;

                case "clear":
                    window.textEditor.Clear();
                    break;

                case "left":
                    if (Caret.Offset != 0)
                        Caret.Offset -= 1;
                    break;

                case "right":
                    if (Caret.Offset < Code.Text.Length)
                        Caret.Offset += 1;
                    break;

                case "up":
                    if (window.textEditor.TextArea.Caret.Line != 0)
                        window.textEditor.TextArea.Caret.Line -= 1;
                    break;

                case "down":
                    if (window.textEditor.TextArea.Caret.Line != window.textEditor.TextArea.Caret.Column)
                        window.textEditor.TextArea.Caret.Line += 1;
                    break;

                case "newline":
                    window.textEditor.Document.Insert(Caret.Offset,"\r\n");
                    break;

                case "tab":
                    window.textEditor.AppendText("\t");
                    break;

                case "stop":
                    StopRecognition();
                    break;

                case "exit":
                    OnExitEvent();
                    break;
            }

            //location[(int)tempSection].Line += 1;
            //window.textEditor.TextArea.Caret.Location = location[(int)tempSection].ToTextLocation();
            //Console.WriteLine("cLocation: " + location[(int)tempSection].Line + ", " + location[(int)tempSection].Column);

            Console.WriteLine("Caret: " + window.textEditor.TextArea.Caret.Location.Line + "," + window.textEditor.TextArea.Caret.Location.Column + "," + window.textEditor.TextArea.Caret.Offset);
            Console.WriteLine("Header: " + location[(int)Section.Header].Line + "," + location[(int)Section.Header].Column + "," + location[(int)Section.Header].Offset);
            Console.WriteLine("Class: " + location[(int)Section.Class].Line + "," + location[(int)Section.Class].Column + "," + location[(int)Section.Class].Offset);
            Console.WriteLine("Global: " + location[(int)Section.Global].Line + "," + location[(int)Section.Global].Column + "," + location[(int)Section.Global].Offset);

            window.status.Text = words[0];
        }

        protected void DisplayDebug()
        {
            Console.WriteLine("Caret: " + window.textEditor.TextArea.Caret.Location.Line + "," + window.textEditor.TextArea.Caret.Location.Column + "," + window.textEditor.TextArea.Caret.Offset);
            Console.WriteLine("Header: " + location[(int)Section.Header].Line + "," + location[(int)Section.Header].Column + "," + location[(int)Section.Header].Offset);
            Console.WriteLine("Class: " + location[(int)Section.Class].Line + "," + location[(int)Section.Class].Column + "," + location[(int)Section.Class].Offset);
            Console.WriteLine("Global: " + location[(int)Section.Global].Line + "," + location[(int)Section.Global].Column + "," + location[(int)Section.Global].Offset);
        }

        protected virtual void OnExitEvent()
        {
            window.Close();
        }

        #region Private function

        private void OperationOn(Section section)
        {
            switch (section)
            {
                case Section.Header:
                    location[(int)Section.Class].Line += 1;
                    window.textEditor.TextArea.Caret.Location = location[(int)section].ToTextLocation();
                    location[(int)Section.Global].Line += 1;
                    window.textEditor.TextArea.Caret.Location = location[(int)section].ToTextLocation();
                    break;
                case Section.Class:
                    location[(int)Section.Global].Line += 1;
                    window.textEditor.TextArea.Caret.Location = location[(int)section].ToTextLocation();
                    break;
                case Section.Global:
                    break;
            }
            location[(int)section].Line += 1;
            window.textEditor.TextArea.Caret.Location = location[(int)section].ToTextLocation();
        }

        private void Rec_Detected(object sender, SpeechDetectedEventArgs e)
        {
            Console.WriteLine(".....");
        }

        private void Rec_Completed(object sender, RecognizeCompletedEventArgs e)
        {
            //In multiple mode, raised when async recognition is canceled 
            Console.WriteLine("----------------------------");
        }

        private void Rec_AudioUpdate(object obj, AudioLevelUpdatedEventArgs e)
        {
            window.ToggelButton.BorderThickness = new Thickness(e.AudioLevel * 7 / 100 + 3);
        }

        private string FindInDictionary(string value)
        {
            if (CPPGrammar.dictionary.TryGetValue(value, out string str))
                return str;
            else
                return "<NotFound>";
        }

        private void OnToggle(object src, RoutedEventArgs e)
        {
            if (recognising)
            {
                rec.RecognizeAsyncCancel();
                recognising = false;
                //statusBar.window.status.Content = "Recognition Stop";
            }
            else
            {
                rec.RecognizeAsync(RecognizeMode.Multiple);
                recognising = true;
                //statusBar.window.status.Content = "I'm listening...";
            }
            //statusBar.ToggleColor(recognising);
        }

        private string Tab(int level)
        {
            string str = null;
            for (int i = 0; i < level; i++)
                str += '\t';
            Console.WriteLine(str.Replace("\t", "TAB"));
            return str;
        }

        #endregion

        #region Public function

        public void StartRecognition(bool emulate)
        {
            if (emulate)
            {
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("include io_stream");
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("include charconv");
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("include charconv");
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("using_namespace standard");
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("function void main");
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("print_line string This string is going to be printed");
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("print_line string This string is going to be printed");
            }
            else
            {
                recognising = true;
                rec.RecognizeAsync(RecognizeMode.Multiple);
                window.ToggelButton.Background = new SolidColorBrush(Color.FromArgb(255, 64, 192, 117));
                window.status.Text = "Listening...";
            }
        }

        public void StopRecognition()
        {
            //Start recognizer
            if (recognising)
            {
                rec.RecognizeAsyncCancel();
                recognising = false;
                window.ToggelButton.Background = new SolidColorBrush(Color.FromArgb(255, 121, 121, 121));
                window.status.Text = "Stop";
                window.ToggelButton.BorderThickness = new Thickness(3);
            }

        }
        public void ReloadGrammar()
        {
            if (recognising)
                rec.RecognizeAsyncCancel();

            rec.UnloadAllGrammars();

            rec.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(cmd))));
            rec.LoadGrammar(new CPPGrammar().GetGrammar);

            if (recognising)
                rec.RecognizeAsync(RecognizeMode.Multiple);

            Console.WriteLine("Reload Complete");
        }

        #endregion
    }
}