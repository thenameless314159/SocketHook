using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SocketHook.HostedWpfSample.Commands
{
    public static class AsyncCommand
    {
        public static ICommand CreateWithInput<T>(Func<T, ValueTask> command, Action<Exception> errorHandler = default) => 
            new InputCommandAsync<T>(command, errorHandler);

        public static ICommand Create(Func<ValueTask> command, Action<Exception> errorHandler = default) =>
            new CommandAsync(command, errorHandler);

        /// <summary>
        /// This class propagate the exception to the UI thread if any happens by default, can be optimized
        /// using Task and a TaskCompletion <see cref="INotifyPropertyChanged"/> implementation.
        /// More infos at : https://github.com/DanStevens/AsyncCommands
        /// </summary>
        private class InputCommandAsync<TInput> : ICommand
        {
            private readonly Func<TInput, ValueTask> _command;
            private readonly Action<Exception> _errorHandler;
            private ValueTask _execution;

            public InputCommandAsync(Func<TInput, ValueTask> command, Action<Exception> errorHandler = default)
            {
                _errorHandler = errorHandler;
                _command = command;
            }

            public bool CanExecute(object parameter) => true; // can always execute, request will be denied anyway when executing the task.

            public void Execute(object parameter)
            {
                if (parameter != default && !(parameter is TInput))
                    throw new ArgumentException($"An invalid argument of type {parameter.GetType().Name} has been provided " +
                                                $"to an AsyncCommand with input that should be of type : {typeof(TInput).Name}.");
                try
                {
                    _execution = _command(parameter != null ? (TInput)parameter : default);
                    RaiseCanExecuteChanged();
                    if (_execution.IsCompletedSuccessfully)
                    {
                        RaiseCanExecuteChanged();
                        return;
                    }
                    RaiseCanExecuteChanged();
                    awaitTask(_execution); // will throw if not completed successfully
                    RaiseCanExecuteChanged();
                }
                catch (Exception e) { if (_errorHandler == default) throw; _errorHandler(e); }
                static async void awaitTask(ValueTask execution) => await execution;
            }

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            private static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
        }

        private class CommandAsync : ICommand
        {
            private readonly Func<ValueTask> _command;
            private readonly Action<Exception> _errorHandler;
            private ValueTask _execution;

            public CommandAsync(Func<ValueTask> command, Action<Exception> errorHandler = default)
            {
                _command = command;
                _errorHandler = errorHandler;
            }

            public bool CanExecute(object parameter) => true; // can always execute 

            public void Execute(object parameter)
            {
                try
                {
                    _execution = _command();
                    RaiseCanExecuteChanged();
                    if (_execution.IsCompletedSuccessfully)
                    {
                        RaiseCanExecuteChanged();
                        return;
                    }

                    RaiseCanExecuteChanged();
                    awaitTask(_execution); // will throw if not completed successfully
                    RaiseCanExecuteChanged();
                }
                catch (Exception e) { if (_errorHandler == default) throw; _errorHandler(e); }
                static async void awaitTask(ValueTask execution) => await execution;
            }

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            private static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
        }
    }
}
