using System;
using System.Windows.Input;

namespace SocketHook.HostedWpfSample.Commands
{
    public class ActionCommand : ICommand
    {
        private readonly Action _execute;
        private Func<bool> _predicate;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private ActionCommand(Action execute, Func<bool> predicate = default)
        {
            _execute = execute;
            _predicate = predicate;
        }

        public static ICommand Create(Action execute, Func<bool> predicate = default) =>
            new ActionCommand(execute, predicate);

        public static ICommand Create<T>(Action<T> execute, Predicate<T> predicate = default) =>
            new ActionCommand<T>(execute, predicate);

        public bool CanExecute(object parameter) => (_predicate ??= () => true)();
        public void Execute(object parameter) => _execute();
    }

    public class ActionCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public ActionCommand(Action<T> execute)
            : this(execute, null) => _execute = execute;

        public ActionCommand(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);

        public void Execute(object parameter) => _execute((T)parameter);
    }
}
