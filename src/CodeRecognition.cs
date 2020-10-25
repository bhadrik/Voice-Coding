using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace Voice_Coding.src
{
    class CodeRecognition
    {
        public bool recognizing;
        int level = 0;

        public event EventHandler ExitEvent;

        //Recognizer objects
        private readonly SpeechRecognitionEngine rec;

        //To simulat keybord & mouse input
        private readonly InputSimulator sim;

        private readonly StatusBar statusBar;

        public CodeRecognition()
        {
            //CPPGrammar.InitializeDefaultGrammer();

            rec = new SpeechRecognitionEngine(new CultureInfo("en-US"));
            sim = new InputSimulator();

            rec.SetInputToDefaultAudioDevice();

            //Loading basic grammar
            rec.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(Resources.commands.Replace("\r","").Replace("\n",";").Split(';')))));
            //Loading C++ grammar
            //rec.LoadGrammarAsync(CPPGrammar.GetGrammar());
            rec.LoadGrammarAsync(CPPGrammar.GetGrammar);

            //All event handler
            rec.SpeechRecognized +=
                new EventHandler<SpeechRecognizedEventArgs>(Rec_Recognised);
            rec.SpeechDetected +=
                new EventHandler<SpeechDetectedEventArgs>(Rec_Detected);
            rec.RecognizeCompleted +=
                new EventHandler<RecognizeCompletedEventArgs>(Rec_Completed);
            rec.AudioLevelUpdated +=
                new EventHandler<AudioLevelUpdatedEventArgs>(Rec_AudioUpdate);

            //INPUT Simpulation
            /*
            Thread.Sleep(3000);
            rec.EmulateRecognizeAsync("include iostream");
            Thread.Sleep(3000);
            rec.EmulateRecognizeAsync("using_namespace standard");
            Thread.Sleep(500);
            rec.EmulateRecognizeAsync("function void main");
            Thread.Sleep(500);
            rec.EmulateRecognizeAsync("printline string This string is going to be printed");
            Thread.Sleep(500);
            rec.EmulateRecognizeAsync("printf variable date");
            Thread.Sleep(500);
            rec.EmulateRecognizeAsync("function int recognized");
            Thread.Sleep(500);
            */

            statusBar = new StatusBar();
            statusBar.Show();
            statusBar.toggleRecogniton += new EventHandler < RoutedEventArgs > (OnToggle);
        }

        private void Rec_Recognised(object sender, SpeechRecognizedEventArgs e)
        {
            string[] words = e.Result.Text.Split(' ');
            string rslt="<NOT SETED>", data;

            Console.WriteLine("CMD: " + words[0]);
            
            if(words.Length > 1)
            {
                rslt = findInDictionary(words[1]);
                data = e.Result.Text.Replace(words[0] + " " + words[1] + " ", "");
            }

            statusBar.changeText($"{e.Result.Text} => {e.Result.Confidence}");

            switch (words[0])
            {
                //INCLUDE "file_name"  2
                case "include":
                    sim.Keyboard.TextEntry($"#include < {rslt} >\r");
                    //sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    break;

                //USING_NAMESPACE "name_of_namespace"  2
                case "using_namespace":
                    sim.Keyboard.TextEntry($"using namespace {rslt};\r");
                    //sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    break;

                //FUNCTION "data_type" "Function_name" 3
                case "function":
                    //builder.Clear();
                    //builder.AppendLine();
                    sim.Keyboard.TextEntry(rslt + " " + words[2] + "()\r{\r\t\r}\r");
                    sim.Keyboard.KeyPress(
                        new VirtualKeyCode[] {
                            VirtualKeyCode.UP,
                            VirtualKeyCode.UP
                        });
                    level++;
                    break;

                //PRINT_LINE STRING/VAR "data_to_be_printed"  3
                case "print_line":
                    if (words[1] == "string")
                    {
                        sim.Keyboard.TextEntry($"cout<<\"{data}\"<<endl;\r");
                    }
                    else
                    {
                        sim.Keyboard.TextEntry($"cout<<{data}<<endl;\r");
                    }
                    break;

                //PRINT STRING/VAR "data_to_be_printed" 3
                case "print":
                    if (words[1] == "string")
                    {
                        sim.Keyboard.TextEntry($"cout<<\"{data}\";\r");
                    }
                    else
                    {
                        sim.Keyboard.TextEntry($"cout<<{data};");
                    }
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
                    break;

                case "exit":
                    OnExitEvent();
                    break;
            }
            for(int i=0; i<level; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            }
            Console.WriteLine("");
            level = 0;
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
            int maxBordersize = 7;
            statusBar.toggleBtn.BorderThickness = new Thickness(e.AudioLevel*maxBordersize/100);
        }

        private string findInDictionary(string value)
        {
            if (CPPGrammar.dictionary.TryGetValue(value, out string str))
                return str;
            else
                return "<NotFound>";
        }

        //----------------------------------------------------------------------------------------

        protected virtual void OnExitEvent()
        {
            if (ExitEvent != null)
                ExitEvent(this, EventArgs.Empty);
        }

        private void OnToggle(object src, RoutedEventArgs e)
        {
            if (recognizing) { rec.RecognizeAsyncCancel(); recognizing = false; statusBar.status.Content = "Recognition Stop";  }
            else { rec.RecognizeAsync(RecognizeMode.Multiple); recognizing = true; statusBar.status.Content = "I'm listening..."; }
            statusBar.toggleColor(recognizing);
        }

        //----------------------------------------------------------------------------------------

        public void startRecognition()
        {
            //Start recognizer
            rec.RecognizeAsync(RecognizeMode.Multiple);
            recognizing = true;
        }

        public void stopRecognition()
        {
            //Start recognizer
            if (recognizing)
            {
                rec.RecognizeAsyncCancel();
                recognizing = false;
            }
        }

        public static Icon getIcon()
        {
            return Resources.icon_tray_b_w;
        }

        public void Close()
        {
            stopRecognition();
            statusBar.Close();
        }
    }
}
