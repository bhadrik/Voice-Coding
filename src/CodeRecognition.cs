using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
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
        private InputSimulator sim;

        public CodeRecognition()
        {
            rec = new SpeechRecognitionEngine(new CultureInfo("en-US"));
            sim = new InputSimulator();

            CPPGrammar.InitializeDefaultGrammer();

            rec.SetInputToDefaultAudioDevice();

            //Load all different grammars
            string[] commands = Resources.commands.Split(';');
            rec.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(commands))));
            rec.LoadGrammarAsync(CPPGrammar.Include);
            rec.LoadGrammarAsync(CPPGrammar.Function);
            rec.LoadGrammarAsync(CPPGrammar.Print);

            //All event handler
            rec.SpeechRecognized +=
                new EventHandler<SpeechRecognizedEventArgs>(Rec_Recognised);
            rec.SpeechDetected +=
                new EventHandler<SpeechDetectedEventArgs>(Rec_Detected);
            rec.RecognizeCompleted +=
                new EventHandler<RecognizeCompletedEventArgs>(Rec_Completed);

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
        }

        private void Rec_Recognised(object sender, SpeechRecognizedEventArgs e)
        {
            Console.Write(e.Result.Confidence);
            Console.WriteLine();
            RecognizedWordUnit firstWord = e.Result.Words.First();

            Console.Write("CMD: " + firstWord.Text);

            switch (firstWord.Text)
            {
                // INCLUDE "file_name"
                case "include":
                    RecognizedWordUnit tempWord = e.Result.Words.ElementAt(1);
                    Console.WriteLine(tempWord.Text);
                    sim.Keyboard.TextEntry("#include <" + tempWord.Text + ">");
                    sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    break;

                // FUNCTION "data_type" "Function_name"
                case "function":
                    tempWord = e.Result.Words.ElementAt(1);
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
                    for (int i = 0; i < level; i++)
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    break;

                // PRINTLINE  STRING/VAR "data_to_be_printed"
                case "printline":
                    tempWord = e.Result.Words.ElementAt(1);
                    Console.Write(" " + tempWord.Text);
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
                    for (int i = 0; i < level; i++)
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
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

                default:
                    sim.Keyboard.TextEntry(e.Result.Text);
                    Console.WriteLine("Default\n");
                    break;
            }
        }

        public void startRecognition()
        {
            //Start recognizer
            rec.RecognizeAsync(RecognizeMode.Multiple);
            recognising = true;
        }

        public void stopRecognition()
        {
            //Start recognizer
            rec.RecognizeAsyncCancel();
            recognising = true;
        }

        protected virtual void OnExitEvent()
        {
            if (ExitEvent != null)
                ExitEvent(this, EventArgs.Empty);
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

        public static Icon getIcon()
        {
            return Resources.icon_tray_b_w;
        }

    }
}
