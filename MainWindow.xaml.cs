using System;
using System.Linq;
using System.Windows;
using System.Speech.Recognition;
using System.Globalization;
using System.IO;
using WindowsInput;
using WindowsInput.Native;
using System.Threading;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Windows.Input;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Voice_Coding
{
    public partial class MainWindow : Window
    {

        bool recognizing;

        //Two recognizer objects
        private SpeechRecognitionEngine rec = new SpeechRecognitionEngine(new CultureInfo("en-US"));

        //Tray icon
        private NotifyIcon m_notifyIcon;
        private ContextMenu contextMenu1;
        private MenuItem menuItem1;
        private IContainer components;


        //Choices for differet grammar
        private Choices dataType =
            new Choices(new string[] { "void", "int", "char", "bool", "float", "double" });
        private Choices printType =
            new Choices(new string[] { "variable", "string" });
        private Choices printCmdType =
            new Choices(new string[] { "printf", "printline" });
        private Choices headers =
            new Choices(new string[] { "iostream", "cstdlib", "cmaths", "strnig" });

        //New grammar
        private Grammar includeChooser;
        private Grammar functionChooser;
        private Grammar printChooser;

        //Builder for new grammar
        private GrammarBuilder includeBuilder = new GrammarBuilder();
        private GrammarBuilder functionBuilder = new GrammarBuilder();
        private GrammarBuilder printBuilder = new GrammarBuilder();

        //To simpulat keybord & mouse input
        private InputSimulator sim = new InputSimulator();

        public MainWindow()
        {
            InitializeComponent();
            
            components = new System.ComponentModel.Container();
            contextMenu1 = new System.Windows.Forms.ContextMenu();
            menuItem1 = new System.Windows.Forms.MenuItem();

            contextMenu1.MenuItems.AddRange(new MenuItem[] { menuItem1 });

            menuItem1.Index = 0;
            menuItem1.Text = "Exit";
            menuItem1.Click += new EventHandler(exit_Click);

            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.Text = "Voice Coding";
            m_notifyIcon.Icon = new System.Drawing.Icon("Application_icon.ico");
            m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);
            m_notifyIcon.ContextMenu = contextMenu1;
            
            if(!m_notifyIcon.Visible) m_notifyIcon.Visible = true;

            //grammar generation
            includeBuilder.Append("include");
            includeBuilder.Append(headers);

            functionBuilder.Append("function");
            functionBuilder.Append(dataType);
            functionBuilder.AppendDictation();

            printBuilder.Append(printCmdType);
            printBuilder.Append(printType);
            printBuilder.AppendDictation();


            //Assigne grammar
            includeChooser = new Grammar(includeBuilder);
            functionChooser = new Grammar(functionBuilder);
            printChooser = new Grammar(printBuilder);

            rec.SetInputToDefaultAudioDevice();

            //Load all different kind of grammar
            rec.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(File.ReadAllLines(@"commands.txt")))));
            rec.LoadGrammarAsync(includeChooser);
            rec.LoadGrammarAsync(functionChooser);
            rec.LoadGrammarAsync(printChooser);

            //All event handler
            rec.SpeechRecognized +=
                new EventHandler<SpeechRecognizedEventArgs>(rec_Recognized);
            rec.SpeechDetected +=
                new EventHandler<SpeechDetectedEventArgs>(rec_Detected);
            rec.RecognizeCompleted +=
                new EventHandler<RecognizeCompletedEventArgs>(rec_Completed);

            //Start recognizer
            rec.RecognizeAsync(RecognizeMode.Multiple);
            recognizing = true;

            /*   AUTO INPUT
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
            //rec.EmulateRecognizeAsync("exit");
        }



        // NOTIFIER  -----------------------------------------------------

        void m_notifyIcon_Click(object sender, EventArgs e)
        {
            if (recognizing)
            {
                rec.RecognizeAsyncCancel();
                recognizing = false;
            }
            else
            {
                rec.RecognizeAsync(RecognizeMode.Multiple);
                recognizing = true;
            }
        }

        private void exit_Click(object sender, EventArgs e)
        {
            m_notifyIcon.Dispose();
            this.Close();
        }

        //-------------------------------------------------------------------

        private void rec_Recognized(object sender, SpeechRecognizedEventArgs e)
        {
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
                    sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    break;

                // PRINTF  STRING/VAR "data_to_be_printed"
                case "printf":
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
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
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
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    }
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
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    }

                    else if (tempWord.Text == "variable")
                    {
                        tempWord = e.Result.Words.ElementAt(2);
                        Console.Write("variable " + tempWord.Text);
                        sim.Keyboard.TextEntry(tempWord.Text);
                        sim.Keyboard.KeyPress(VirtualKeyCode.END);
                        sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                        sim.Keyboard.KeyPress(VirtualKeyCode.TAB);
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
                    recognizing = false;
                    Console.WriteLine("");
                    break;

                case "exit":
                    //System.Windows.Application.Current.Shutdown();
                    sim.Keyboard.TextEntry("You said exit");
                    this.Close();
                    break;

                default:
                    sim.Keyboard.TextEntry(e.Result.Text);
                    Console.WriteLine("");
                    break;
            }
        }

        private void rec_Detected(object sender, SpeechDetectedEventArgs e)
        {
            Console.WriteLine("Listening...");
        }

        private void rec_Completed(object sender, RecognizeCompletedEventArgs e)
        {
            Console.WriteLine("----------------------------");
        }

    }
}
