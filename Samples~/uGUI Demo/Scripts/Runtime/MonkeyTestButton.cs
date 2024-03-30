// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using TestHelper.Monkey.ScreenshotFilenameStrategies;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.Samples.UGUIDemo
{
    /// <summary>
    /// Run/ Stop monkey testing.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MonkeyController : MonoBehaviour
    {
        public int lifetimeSeconds = 30;
        public int delayMillis = 200;
        public int secondsToErrorForNoInteractiveComponent = 5;
        public int touchAndHoldDelayMillis = 1000;
        public bool gizmos;
        public bool screenshots;

        private bool _running;
        private Text _buttonLabel;
        private CancellationTokenSource _cts;

        private void Start()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(async () =>
            {
                await OnClick();
            });
            _buttonLabel = button.GetComponentInChildren<Text>();
        }

        private async Task OnClick()
        {
            if (_running)
            {
                _cts.Cancel();
                _cts.Dispose();
                _running = false;
            }
            else
            {
                var config = new MonkeyConfig
                {
                    Lifetime = TimeSpan.FromSeconds(lifetimeSeconds),
                    DelayMillis = delayMillis,
                    SecondsToErrorForNoInteractiveComponent = secondsToErrorForNoInteractiveComponent,
                    TouchAndHoldDelayMillis = touchAndHoldDelayMillis,
                    Gizmos = gizmos,
                    Screenshots = screenshots
                        ? new ScreenshotOptions() { FilenameStrategy = new CounterBasedStrategy("Demo") }
                        : null
                };
                _cts = new CancellationTokenSource();
                _running = true;
                await Monkey.Run(config, _cts.Token).SuppressCancellationThrow();
                _running = false;
            }
        }

        private void Update()
        {
            _buttonLabel.text = _running ? "Stop monkey testing" : "Run monkey testing";
        }
    }
}
