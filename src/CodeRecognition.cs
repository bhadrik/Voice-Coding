using System;
using System.Globalization;
using System.Speech.Recognition;
using System.Threading;
using System.Windows;
using WindowsInput;
using WindowsInput.Native;
using System.Text;
using System.Xml;

namespace Voice_Coding.src
{
    class CodeRecognition
    {
        #region Var declaration
        public  bool                                recognizing;
        private int                                 level = 0;
        public  event    EventHandler               ExitEvent;
        private readonly SpeechRecognitionEngine    rec;
        private readonly InputSimulator             sim;
        private readonly StatusBar                  statusBar;
        private readonly string[] cmd;
        #endregion

        public CodeRecognition()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"..\..\res\MainResource.xml");

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
                new EventHandler<SpeechRecognizedEventArgs>   (Rec_Recognised);
            rec.SpeechDetected +=
                new EventHandler<SpeechDetectedEventArgs>     (Rec_Detected);
            rec.RecognizeCompleted +=
                new EventHandler<RecognizeCompletedEventArgs> (Rec_Completed);
            rec.AudioLevelUpdated +=
                new EventHandler<AudioLevelUpdatedEventArgs>  (Rec_AudioUpdate);

            sim = new InputSimulator();

            statusBar                    = new StatusBar();
            statusBar.Show();
            statusBar.ToggleRecogniton  += new EventHandler<RoutedEventArgs>(OnToggle);
            statusBar.Exit              += new EventHandler<RoutedEventArgs>(OnExitEvent);
        }

        private void Rec_Recognised(object sender, SpeechRecognizedEventArgs e)
        {
            string[] words = e.Result.Text.Split(' ');
            string rslt="<NOT SETED>", data = "<NOT SETED>";

            Console.WriteLine("CMD: " + words[0]);
            
            if(words.Length > 1)
            {
                rslt = FindInDictionary(words[1]);
                data = e.Result.Text.Replace(words[0] + " " + words[1] + " ", "");
            }

            statusBar.ChangeText($"{e.Result.Text} [{e.Result.Confidence}] [{e.Result.Grammar.Name}]");

            switch (words[0])
            {
                //INCLUDE "file_name"  2
                case "include":
                    sim.Keyboard.TextEntry(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"#include < {words[1]} >\r")));
                    break;

                //USING_NAMESPACE "name_of_namespace"  2
                case "using_namespace":
                    sim.Keyboard.TextEntry($"using namespace {rslt};\r");
                    break;

                //FUNCTION "data_type" "Function_name" 3
                case "function":
                    level++;
                    sim.Keyboard.TextEntry(rslt + " " + words[2] + "()\r{\r" + Tab(level) + "\r}\r");
                    sim.Keyboard.KeyPress(
                        new VirtualKeyCode[] {
                            VirtualKeyCode.UP,
                            VirtualKeyCode.UP
                        });
                    break;

                //PRINT_LINE STRING/VAR "data_to_be_printed"  3
                case "print_line":
                    if (words[1] == "string")
                        sim.Keyboard.TextEntry($"cout<<\"{data}\"<<endl;\r");
                    else
                        sim.Keyboard.TextEntry($"cout<<{data}<<endl;\r");
                    break;

                //PRINT STRING/VAR "data_to_be_printed" 3
                case "print":
                    if (words[1] == "string")
                        sim.Keyboard.TextEntry($"cout<<\"{data}\";\r");
                    else
                        sim.Keyboard.TextEntry($"cout<<{data};\r");
                    break;

                case "undo":
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_Z);
                    sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    break;

                case "back":
                    sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    break;

                case "erase":
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    break;

                case "clear":
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                    sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    break;

                case "left":
                    sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                    break;

                case "right":
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    break;

                case "up":
                    sim.Keyboard.KeyPress(VirtualKeyCode.UP);
                    break;

                case "down":
                    sim.Keyboard.KeyPress(VirtualKeyCode.DOWN);
                    break;

                case "newline":
                    sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    break;

                case "tab":
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    break;

                case "stop":
                    rec.RecognizeAsyncCancel();
                    recognizing = false;
                    statusBar.ToggleColor(recognizing);
                    break;

                case "exit":
                    OnExitEvent(this, new RoutedEventArgs());
                    break;
            }
        }

        protected virtual void OnExitEvent(object sender, RoutedEventArgs e)
        {
            ExitEvent?.Invoke(sender, e);
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
            int maxBordersize = 7;
            statusBar.toggleBtn.BorderThickness = new Thickness(e.AudioLevel*maxBordersize/100 + 3);
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
            if (recognizing) { 
                rec.RecognizeAsyncCancel();
                recognizing = false;
                statusBar.status.Content = "Recognition Stop";
            }
            else {
                rec.RecognizeAsync(RecognizeMode.Multiple);
                recognizing = true;
                statusBar.status.Content = "I'm listening...";
            }
            statusBar.ToggleColor(recognizing);
        }

        private string Tab(int level)
        {
            string str = null;
            for (int i = 0; i < level; i++)
                str += '\t';
            Console.WriteLine(str.Replace("\t","TAB"));
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
                //Start recognizer
                rec.RecognizeAsync(RecognizeMode.Multiple);
                recognizing = true;
            }
        }

        public void StopRecognition()
        {
            //Start recognizer
            if (recognizing)
            {
                rec.RecognizeAsyncCancel();
                recognizing = false;
            }
        }

        public void Close()
        {
            StopRecognition();
            statusBar.Close();
        }

        public void ReloadGrammar()
        {
            if(recognizing)
            rec.RecognizeAsyncCancel();

            rec.UnloadAllGrammars();

            rec.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(cmd))));
            rec.LoadGrammar(new CPPGrammar().GetGrammar);

            if (recognizing)
            rec.RecognizeAsync(RecognizeMode.Multiple);

            Console.WriteLine("Reload Complete");
        }

        #endregion
    }
}