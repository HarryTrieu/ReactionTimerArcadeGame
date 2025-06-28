namespace SimpleReactionMachine
{
    public class EnhancedReactionController : IController
    {
        private IGui _gui = null!;
        private IRandom _rng = null!;
        private IState _currentState = null!;
        private int _ticks;
        private int _randomDelay;
        private float[] _reactionTimes = new float[3];
        private int _gameCount;
        private bool _cheated;
        private bool _timeout;

        private interface IState
        {
            void OnCoinInserted();
            void OnGoStopPressed();
            void OnTick();
        }

        public EnhancedReactionController()
        {
        }

        public void Connect(IGui gui, IRandom rng)
        {
            _gui = gui;
            _rng = rng;
        }

        public void Init()
        {
            _currentState = new IdleState(this);
            _gui.SetDisplay("Insert coin");
            _gameCount = 0;
            _cheated = false;
            _ticks = 0;
            _randomDelay = 0;
            _timeout = false;
            _reactionTimes = new float[3];
        }

        public void CoinInserted()
        {
            _currentState.OnCoinInserted();
        }

        public void GoStopPressed()
        {
            _currentState.OnGoStopPressed();
        }

        public void Tick()
        {
            _currentState.OnTick();
        }

        private class IdleState : IState
        {
            private readonly EnhancedReactionController _controller;

            public IdleState(EnhancedReactionController controller)
            {
                _controller = controller;
            }

            public void OnCoinInserted()
            {
                _controller._currentState = new ReadyState(_controller);
                _controller._gameCount = 1;
                _controller._ticks = 0;
                _controller._cheated = false;
                _controller._timeout = false;
                _controller._randomDelay = 0;
                _controller._reactionTimes = new float[3];
                _controller._gui.SetDisplay("Press GO!");
            }

            public void OnGoStopPressed() { }
            public void OnTick() { }
        }

        private class ReadyState : IState
        {
            private readonly EnhancedReactionController _controller;

            public ReadyState(EnhancedReactionController controller)
            {
                _controller = controller;
            }

            public void OnCoinInserted() { }

            public void OnGoStopPressed()
            {
                _controller._randomDelay = _controller._rng.GetRandom(100, 251);
                _controller._ticks = 0; // Reset ticks
                _controller._currentState = new WaitingState(_controller);
                _controller._gui.SetDisplay("Wait...");
            }

            public void OnTick()
            {
                _controller._ticks++;
                if (_controller._ticks >= 1000)
                {
                    _controller._currentState = new IdleState(_controller);
                    _controller._gui.SetDisplay("Insert coin");
                }
            }
        }

        private class WaitingState : IState
        {
            private readonly EnhancedReactionController _controller;

            public WaitingState(EnhancedReactionController controller)
            {
                _controller = controller;
            }

            public void OnCoinInserted() { }

            public void OnGoStopPressed()
            {
                _controller._cheated = true;
                _controller._currentState = new IdleState(_controller);
                _controller._gui.SetDisplay("Insert coin");
            }

            public void OnTick()
            {
                _controller._ticks++;
                if (_controller._ticks == _controller._randomDelay) 
                {
                    _controller._ticks = 0;
                    _controller._currentState = new RunningState(_controller);
                }
            }
        }

        private class RunningState : IState
        {
            private readonly EnhancedReactionController _controller;

            public RunningState(EnhancedReactionController controller)
            {
                _controller = controller;
                _controller._ticks = 0;
                _controller._gui.SetDisplay("0.00");
            }

            public void OnCoinInserted() { }

            public void OnGoStopPressed()
            {
                _controller._reactionTimes[_controller._gameCount - 1] = _controller._ticks / 100.0f;
                _controller._ticks = 0;
                _controller._timeout = false;
                _controller._currentState = new ResultState(_controller);
                _controller._gui.SetDisplay($"{_controller._reactionTimes[_controller._gameCount - 1]:F2}");
            }

            public void OnTick()
            {
                _controller._ticks++;
                _controller._gui.SetDisplay($"{(_controller._ticks / 100.0):F2}");
                
                if (_controller._ticks >= 200)
                {
                    _controller._reactionTimes[_controller._gameCount - 1] = _controller._ticks / 100.0f;
                    _controller._ticks = 0;
                    _controller._timeout = true;
                    _controller._currentState = new ResultState(_controller);
                    _controller._gui.SetDisplay($"{_controller._reactionTimes[_controller._gameCount - 1]:F2}");
                }
            }
        }

        private class ResultState : IState
        {
            private readonly EnhancedReactionController _controller;

            public ResultState(EnhancedReactionController controller)
            {
                _controller = controller;
                _controller._ticks = 0; // Reset ticks when entering ResultState
            }

            public void OnCoinInserted() { }

            public void OnGoStopPressed()
            {
                _controller._ticks = 0; // Reset ticks
                if (_controller._gameCount < 3 && !_controller._cheated)
                {
                    _controller._gameCount++;
                    _controller._randomDelay = _controller._rng.GetRandom(100, 251);
                    _controller._currentState = new WaitingState(_controller);
                    _controller._gui.SetDisplay("Wait...");
                }
                else
                {
                    _controller._currentState = new AverageState(_controller);
                    float sum = 0;
                    int validGames = 0;
                    for (int i = 0; i < _controller._gameCount; i++)
                    {
                        if (_controller._reactionTimes[i] > 0)
                        {
                            sum += _controller._reactionTimes[i];
                            validGames++;
                        }
                    }
                    float average = validGames > 0 ? sum / validGames : 0;
                    _controller._gui.SetDisplay($"Average = {average:F2}");
                }
            }

            public void OnTick()
            {
                _controller._ticks++;
                if (_controller._ticks >= 300)
                {
                    if (_controller._timeout || _controller._cheated || _controller._gameCount >= 3)
                    {
                        _controller._currentState = new AverageState(_controller);
                        float sum = 0;
                        int validGames = 0;
                        for (int i = 0; i < _controller._gameCount; i++)
                        {
                            if (_controller._reactionTimes[i] > 0)
                            {
                                sum += _controller._reactionTimes[i];
                                validGames++;
                            }
                        }
                        float average = validGames > 0 ? sum / validGames : 0;
                        _controller._gui.SetDisplay($"Average = {average:F2}");
                    }
                    else
                    {
                        _controller._gameCount++;
                        _controller._randomDelay = _controller._rng.GetRandom(100, 251);
                        _controller._ticks = 0; // Reset ticks
                        _controller._currentState = new WaitingState(_controller);
                        _controller._gui.SetDisplay("Wait...");
                    }
                }
            }
        }

        private class AverageState : IState
        {
            private readonly EnhancedReactionController _controller;

            public AverageState(EnhancedReactionController controller)
            {
                _controller = controller;
                _controller._ticks = 0; // Reset ticks 
            }

            public void OnCoinInserted() { }

            public void OnGoStopPressed()
            {
                _controller._currentState = new IdleState(_controller);
                _controller._gui.SetDisplay("Insert coin");
            }

            public void OnTick()
            {
                _controller._ticks++;
                if (_controller._ticks >= 500)
                {
                    _controller._currentState = new IdleState(_controller);
                    _controller._gui.SetDisplay("Insert coin");
                }
            }
        }
    }
}