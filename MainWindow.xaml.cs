using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.ComponentModel;
using Voice_Coding.Source;

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.Win32;

namespace Voice_Coding
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	///
	public partial class MainWindow : Window
	{
		private readonly CodeRecognition Recogniser;

		public MainWindow()
		{
			InitializeComponent();
			Recogniser = new CodeRecognition(this);
			Recogniser.StartRecognition(false);

			//textEditor.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnGotKeyboardFocus);
			textEditor.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(OnLostKeyboardFocus);
			textEditor.TextArea.Caret.Location = new ICSharpCode.AvalonEdit.Document.TextLocation(1, 1);
			this.Closing += new CancelEventHandler(OnExitEvent);
		}

		string currentFileName;

		void OpenFileClick(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
			{
				CheckFileExists = true
			};

			if (dlg.ShowDialog() ?? false)
			{
				currentFileName = dlg.FileName;
				textEditor.Load(currentFileName);
				textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(currentFileName));
			}
		}

		void SaveFileClick(object sender, EventArgs e)
		{
			if (currentFileName == null)
			{
				Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
				{
					Filter = "C++ file (*.cpp)|*.cpp|C# file (*.cs)|*.cs|C file (*.c)|*.c|C header file (*.h)|*.h|Text file (*.txt)|*.txt",
					FileName = "Untitled",
					DefaultExt = ".cpp"
				};

				if (dlg.ShowDialog() ?? false)
				{
					currentFileName = dlg.FileName;
				}
				else
				{
					return;
				}
			}
			textEditor.Save(currentFileName);
		}

		void OnLostKeyboardFocus(object sender, EventArgs e)
		{
			if (Recogniser.recognising)
				Recogniser.StopRecognition();
		}

		public void OnToggleRecognition(object sender, RoutedEventArgs e)
		{
			if (Recogniser.recognising)
			{
				Recogniser.StopRecognition();
			}
			else
			{
				Recogniser.StartRecognition(false);
			}
		}

		private void OnExitEvent(object sender, CancelEventArgs e)
		{
			Recogniser.StopRecognition();
			System.Windows.Forms.DialogResult rslt = System.Windows.Forms.MessageBox.Show("Do you really want to close Voice Coding", "Exit", MessageBoxButtons.YesNo);
			if (rslt == System.Windows.Forms.DialogResult.Yes)
			{
				e.Cancel = false;
			}
			else
			{
				e.Cancel = true;
			}
		}
	}
}
