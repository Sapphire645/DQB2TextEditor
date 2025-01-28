using System;
using System.Windows.Input;


//CODE FROM https://github.com/turtle-insect/DQB2
namespace DQB2TextEditor
{
	internal class CommandAction : ICommand
	{
		private readonly Action<object> mAction;
		public CommandAction(Action<object> action) => mAction = action;
		public event EventHandler CanExecuteChanged;
		public bool CanExecute(object parameter) => true;
		public void Execute(object parameter) => mAction(parameter);
	}
}
