using System;
using SharpDX.XInput;
using System.Windows.Threading;
using Arcadia.UI.ViewModels;
using System.Threading.Tasks;
using System.Threading;

namespace Arcadia.UI.Input
{
    /// <summary>
    /// Handles XInput (Xbox Controller) polling for the game wheel.
    /// </summary>
    public class InputManager
    {
        private readonly Controller _controller;
        private readonly GameWheelViewModel _viewModel;
        private readonly CancellationTokenSource _cts = new();
        private bool _isDPadDown;

        public InputManager(GameWheelViewModel viewModel)
        {
            _controller = new Controller(UserIndex.Any);
            _viewModel = viewModel;
            
            // High-frequency polling on a background thread to reduce input lag
            Task.Run(PollLoop, _cts.Token);
        }

        private async Task PollLoop()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                Poll();
                // Poll at ~250Hz (4ms) for near-instant response
                await Task.Delay(4);
            }
        }

        public void Stop() => _cts.Cancel();

        private void Poll()
        {
            if (!_controller.IsConnected) return;

            var state = _controller.GetState();
            var buttons = state.Gamepad.Buttons;

            // Navigation logic with debouncing
            if (buttons.HasFlag(GamepadButtonFlags.DPadDown) || buttons.HasFlag(GamepadButtonFlags.DPadRight))
            {
                if (!_isDPadDown) 
                { 
                    // Dispatch back to UI thread only for the state change
                    System.Windows.Application.Current?.Dispatcher?.BeginInvoke(_viewModel.MoveNext); 
                    _isDPadDown = true; 
                }
            }
            else if (buttons.HasFlag(GamepadButtonFlags.DPadUp) || buttons.HasFlag(GamepadButtonFlags.DPadLeft))
            {
                if (!_isDPadDown) 
                { 
                    System.Windows.Application.Current?.Dispatcher?.BeginInvoke(_viewModel.MovePrevious); 
                    _isDPadDown = true; 
                }
            }
            else
            {
                _isDPadDown = false;
            }

            // Selection
            if (buttons.HasFlag(GamepadButtonFlags.A))
            {
                System.Windows.Application.Current?.Dispatcher?.BeginInvoke(() => _viewModel.HandleKeyDown(System.Windows.Input.Key.Enter));
            }

            // Fullscreen Toggle
            if (buttons.HasFlag(GamepadButtonFlags.Start) && buttons.HasFlag(GamepadButtonFlags.Back))
            {
                System.Windows.Application.Current?.Dispatcher?.BeginInvoke(() => _viewModel.HandleKeyDown(System.Windows.Input.Key.F11));
            }
        }
    }
}