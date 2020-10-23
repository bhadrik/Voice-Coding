using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Windows;
using WindowsInput;
using WindowsInput.Native;

namespace Voice_Coding.src
{
    class CodeRecognition
    {
        public bool recognising;
        int level = 0;

        public event EventHandler ExitEvent;

        //Recognizer objects
        private SpeechRecognitionEngine rec;

        //To simulat keybord & mouse input
        private readonly InputSimulator sim;

        private readonly StatusBar statusBar;

        public CodeRecognition()
        {
            rec = new SpeechRecognitionEngine(new CultureInfo("en-US"));
            sim = new InputSimulator();

            CPPGrammar.InitializeDefaultGrammer();

            rec.SetInputToDefaultAudioDevice();

            //Load all different grammars
            rec.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(Resources.commands.Split(';')))));
            rec.LoadGrammarAsync(CPPGrammar.Include);
            rec.LoadGrammarAsync(CPPGrammar.Namespace);
            rec.LoadGrammarAsync(CPPGrammar.Function);
            rec.LoadGrammarAsync(CPPGrammar.Print);

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
            //if (e.Result.Confidence < 0.8f)
            //{
            //    statusBar.changeText("//" + e.Result.Text + "//" + e.Result.Confidence.ToString());
            //    return;
            //}
            Console.Write(e.Result.Confidence);
            Console.WriteLine();
            RecognizedWordUnit firstWord = e.Result.Words.First();

            Console.WriteLine("CMD: " + firstWord.Text);
            statusBar.changeText(e.Result.Text + " " + e.Result.Confidence.ToString());

            string value, rslt ;

            switch (firstWord.Text)
            {
                // INCLUDE "file_name"
                case "include":
                    value = e.Result.Text.Substring(firstWord.Text.Length + 1);
                    rslt = findInDictionary(value);
                    Console.WriteLine(value);
                    Console.WriteLine("INCLUDE:" + value + ":" + rslt);
                    sim.Keyboard.TextEntry("#include <" + rslt + ">");
                    sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    break;

                case "using":
                    value = e.Result.Text.Substring("using namespace ".Length);
                    Console.WriteLine(value);
                    //CPPGrammar.dictionary.TryGetValue(value, out rslt);
                    sim.Keyboard.TextEntry("using namespace " + findInDictionary(value) + ";");
                    sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    break;


                // FUNCTION "data_type" "Function_name"
                case "function":
                    RecognizedWordUnit tempWord = e.Result.Words.ElementAt(1);
                    Console.Write(" " + tempWord.Text);
                    sim.Keyboard.TextEntry(tempWord.Text);
                    tempWord = e.Result.Words.ElementAt(2);
                    Console.WriteLine(" " + tempWord.Text);
                    sim.Keyboard.TextEntry(" " + tempWord.Text + "()");
                    sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    sim.Keyboard.TextEntry("{}");
                    sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                    sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    sim.Keyboard.KeyPress(VirtualKeyCode.UP);
                    level++;
                    break;

                // PRINTF  STRING/VAR "data_to_be_printed"
                case "print":
                    tempWord = e.Result.Words.ElementAt(1);
                    Console.Write(" " + tempWord.Text);
                    sim.Keyboard.TextEntry("cout<<;");
                    if (tempWord.Text == "string")
                    {
                        Console.WriteLine(" " + tempWord.Text + " <INPUT STRING>");
                        sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                        sim.Keyboard.TextEntry("\"\"");
                        sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                        string inputData = e.Result.Text;
                        inputData = inputData.Replace("printf string ", "");
                        sim.Keyboard.TextEntry(inputData);
                        sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    }
                    else if (tempWord.Text == "variable")
                    {
                        Console.Write(" " + tempWord.Text);
                        tempWord = e.Result.Words.ElementAt(2);
                        Console.WriteLine(" " + tempWord.Text);
                        sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                        sim.Keyboard.TextEntry(tempWord.Text);
                        sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                        sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    }
                    break;

                // PRINTLINE  STRING/VAR "data_to_be_printed"
                case "printline":
                    tempWord = e.Result.Words.ElementAt(1);

                    Console.Write(" " + tempWord.Text);
                    statusBar.changeText(tempWord.Text);

                    sim.Keyboard.TextEntry("cout<<<<endl;");
                    for (int i = 0; i < 7; i++) sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);

                    if (tempWord.Text == "string")
                    {
                        Console.WriteLine(" string" + " <INPUT STRING>");
                        sim.Keyboard.TextEntry("\"\"");
                        sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                        string inputData = e.Result.Text;
                        inputData = inputData.Replace("printline string ", "");
                        sim.Keyboard.TextEntry(inputData);
                        sim.Keyboard.KeyPress(VirtualKeyCode.END);
                        sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    }
                    else if (tempWord.Text == "variable")
                    {
                        tempWord = e.Result.Words.ElementAt(2);
                        Console.Write("variable " + tempWord.Text);
                        sim.Keyboard.TextEntry(tempWord.Text);
                        sim.Keyboard.KeyPress(VirtualKeyCode.END);
                        sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    }
                    break;

                case "back":
                    sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    Console.WriteLine("");
                    break;

                case "erase":
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    Console.WriteLine("");
                    break;

                case "clear":
                    sim.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                    sim.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                    sim.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    Console.WriteLine("");
                    break;

                case "left":
                    sim.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                    Console.WriteLine("");
                    break;

                case "right":
                    sim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                    Console.WriteLine("");
                    break;

                case "up":
                    sim.Keyboard.KeyPress(VirtualKeyCode.UP);
                    Console.WriteLine("");
                    break;

                case "down":
                    sim.Keyboard.KeyPress(VirtualKeyCode.DOWN);
                    Console.WriteLine("");
                    break;

                case "newline":
                    sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    Console.WriteLine("");
                    break;

                case "tab":
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    Console.WriteLine("");
                    break;

                case "stop":
                    rec.RecognizeAsyncCancel();
                    recognising = false;
                    Console.WriteLine("");
                    break;

                case "exit":
                    OnExitEvent();
                    break;
            }
            for(int i=0; i<level; i++)
            {
                sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
            }
            level = 0;
        }

        private void Rec_Detected(object sender, SpeechDetectedEventArgs e)
        {
            Console.WriteLine("Listening...");
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
            string str;
            CPPGrammar.dictionary.TryGetValue(value, out str);
            return str;
        }

        //----------------------------------------------------------------------------------------

        protected virtual void OnExitEvent()
        {
            if (ExitEvent != null)
                ExitEvent(this, EventArgs.Empty);
        }

        private void OnToggle(object src, RoutedEventArgs e)
        {
            if (recognising) { rec.RecognizeAsyncCancel(); recognising = false; statusBar.status.Content = "Recognition Stop";  }
            else { rec.RecognizeAsync(RecognizeMode.Multiple); recognising = true; statusBar.status.Content = "I'm listening..."; }
            statusBar.toggleColor(recognising);
        }

        //----------------------------------------------------------------------------------------

        public void startRecognition()
        {
            //Start recognizer
            rec.RecognizeAsync(RecognizeMode.Multiple);
            recognising = true;
        }

        public void stopRecognition()
        {
            //Start recognizer
            if (recognising)
            {
                rec.RecognizeAsyncCancel();
                recognising = false;
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
