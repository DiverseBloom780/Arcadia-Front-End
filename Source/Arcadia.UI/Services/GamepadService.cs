using System;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.XInput;

namespace Arcadia.UI.Services
{
    public class GamepadService : IDisposable
    {
        private readonly Controller _controller;
        private bool _isRunning;
        private State _previousState;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public event Action<GamepadButtonFlags>? ButtonPressed;
        public event Action<Direction>? DPadPressed;

        public enum Direction { Up, Down, Left, Right }

        public GamepadService()
        {
            _controller = new Controller(UserIndex.Any);
        }

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            
            Task.Run(PollController, _cts.Token);
        }

        private async Task PollController()
        {
            while (_isRunning && !_cts.IsCancellationRequested)
            {
                if (_controller.IsConnected)
                {
                    var currentState = _controller.GetState();
                    if (currentState.PacketNumber != _previousState.PacketNumber)
                    {
                        ProcessInput(currentState.Gamepad);
                    }
                    _previousState = currentState;
                }
                
                await Task.Delay(16); // ~60fps polling
            }
        }

        private void ProcessInput(Gamepad gamepad)
        {
            var buttons = gamepad.Buttons;
            var prevButtons = _previousState.Gamepad.Buttons;

            // Detect NEW button presses
            var pressed = buttons & ~prevButtons;

            if (pressed.HasFlag(GamepadButtonFlags.DPadUp)) DPadPressed?.Invoke(Direction.Up);
            if (pressed.HasFlag(GamepadButtonFlags.DPadDown)) DPadPressed?.Invoke(Direction.Down);
            if (pressed.HasFlag(GamepadButtonFlags.DPadLeft)) DPadPressed?.Invoke(Direction.Left);
            if (pressed.HasFlag(GamepadButtonFlags.DPadRight)) DPadPressed?.Invoke(Direction.Right);

            if (pressed != GamepadButtonFlags.None)
            {
                ButtonPressed?.Invoke(pressed);
            }
        }

        public void Vibrate(ushort leftMotor, ushort rightMotor, int durationMs = 100)
        {
            if (!_controller.IsConnected) return;

            Task.Run(async () =>
            {
                _controller.SetVibration(new Vibration { LeftMotorSpeed = leftMotor, RightMotorSpeed = rightMotor });
                await Task.Delay(durationMs);
                _controller.SetVibration(new Vibration { LeftMotorSpeed = 0, RightMotorSpeed = 0 });
            });
        }

        public void Dispose()
        {
            _isRunning = false;
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
