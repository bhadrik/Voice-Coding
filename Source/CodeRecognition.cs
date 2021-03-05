#define debug

using System;
using System.Globalization;
using System.Speech.Recognition;
using System.Threading;
using System.Windows.Media;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Xml;

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Document;

namespace Voice_Coding.Source
{
    struct cLocation
    {
        public cLocation(int l, int c)
        {
            Line = l;
            Column = c;
        }
        public int Line
        {
            get;
            set;
        }
        public int Column
        {
            get;
            set;
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
        Global
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
        #endregion

        public CodeRecognition(MainWindow window)
        {
            ///<summery>
            ///iPointer[0] -> Header section pointer
            ///iPointer[1] -> Class section pointer
            ///iPointer[2] -> Global function pointer
            ///</summery>
            ///
            iPointer = new int[3];
            iPointer[0] = 0;
            iPointer[1] = 0;
            iPointer[2] = 0;

            location = new cLocation[3] {
                new cLocation(0, 0),
                new cLocation(0, 0),
                new cLocation(0, 0)
            };

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

            switch (words[0])
            {
                //INCLUDE "file_name"  2
                case "include":
                    //Console.WriteLine("O:"+window.textEditor.TextArea.Caret.Offset+ " L:"+ window.textEditor.TextArea.Caret.Line + " C:" + window.textEditor.TextArea.Caret.Column);
                    //window.textEditor.TextArea.Caret.Location = location[(int)Section.Header].ToTextLocation();
                    window.textEditor.Text = window.textEditor.Text.Insert(location[(int)Section.Header].Line, "#include<" + words[1] + ">" + ";\n");
                    location[(int)Section.Header].Line += 1;
                    window.textEditor.TextArea.Caret.Location = location[(int)Section.Header].ToTextLocation();
                    break;

                //USING_NAMESPACE "name_of_namespace"  2
                case "using_namespace":
                    window.textEditor.Text = window.textEditor.Text.Insert(1,"s");
                        //window.textEditor.AppendText("using namespace " + FindInDictionary(words[1]) + ";\n");
                    location[(int)Section.Header].Line += 1;
                    window.textEditor.TextArea.Caret.Location = location[(int)Section.Header].ToTextLocation();
                    break;

                //FUNCTION "data_type" "Function_name" 3
                case "function":
                    window.textEditor.AppendText(FindInDictionary(words[1]) + " " + words[2] + "(){\n\t");

                    int carr = window.textEditor.CaretOffset;

                    window.textEditor.AppendText("\n}");

                    window.textEditor.CaretOffset = carr;
                    break;

                //PRINT_LINE STRING/VAR "data_to_be_printed"  3+
                case "print_line":
                    if (words[1] == "string")
                        window.status.Content = words[0];
                    else
                        window.status.Content = words[0];
                    break;

                //PRINT STRING/VAR "data_to_be_printed" 3+
                case "print":
                    if (words[1] == "string")
                        window.textEditor.AppendText("cout<<\"\"<<");
                    else
                        window.status.Content = words[0];
                    break;

                case "undo":
                    window.textEditor.Undo();
                    break;

                case "back":
                    if (window.textEditor.CaretOffset != 0)
                        window.textEditor.CaretOffset -= 1;
                    break;

                case "erase":
                    window.status.Content = "-No idea what to do-";
                    break;

                case "clear":
                    window.textEditor.Clear();
                    break;

                case "left":
                    if (window.textEditor.CaretOffset != 0)
                        window.textEditor.CaretOffset -= 1;
                    break;

                case "right":
                    if (window.textEditor.CaretOffset < window.textEditor.Text.Length)
                        window.textEditor.CaretOffset += 1;
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
                    window.textEditor.Text = window.textEditor.Text.Insert(window.textEditor.CaretOffset, "\n");
                    window.textEditor.TextArea.Caret.Line += 1;
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

            window.status.Content = words[0];
        }

        protected virtual void OnExitEvent()
        {
            window.Close();
        }

        #region Private function

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
            window.ToggelButton.BorderThickness = new Thickness(e.AudioLevel * 7 / 10 + 3);
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
                rec.EmulateRecognizeAsync("using_namespace standard");
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("function void main");
                Thread.Sleep(500);
                rec.EmulateRecognizeAsync("print_line string This string is going to be printed");
            }
            else
            {
                recognising = true;
                rec.RecognizeAsync(RecognizeMode.Multiple);
                window.ToggelButton.Background = new SolidColorBrush(Color.FromArgb(255, 64, 192, 117));
                window.status.Content = "Listening...";
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
                window.status.Content = "Stop";
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