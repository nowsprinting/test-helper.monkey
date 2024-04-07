// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.ScreenshotFilenameStrategies;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.Samples.UGUIDemo
{
    /// <summary>
    /// Run/ Stop monkey testing.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MonkeyTestButton : MonoBehaviour
    {
        public int lifetimeSeconds = 30;
        public int delayMillis = 200;
        public int secondsToErrorForNoInteractiveComponent = 5;
        public bool gizmos;
        public bool screenshots = true;

        private Text _buttonLabel;

        private CancellationTokenSource _cts;
        private bool IsRunning => _cts != null;

        private void Start()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                OnClick().Forget();
            });
            _buttonLabel = button.GetComponentInChildren<Text>();
        }

        private async UniTask OnClick()
        {
            if (IsRunning)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            else
            {
                var config = new MonkeyConfig
                {
                    Lifetime = TimeSpan.FromSeconds(lifetimeSeconds),
                    DelayMillis = delayMillis,
                    SecondsToErrorForNoInteractiveComponent = secondsToErrorForNoInteractiveComponent,
                    Gizmos = gizmos,
                    Screenshots = screenshots
                        ? new ScreenshotOptions() { FilenameStrategy = new CounterBasedStrategy("uGUI Demo") }
                        : null
                };

                _cts = new CancellationTokenSource();
                await Monkey.Run(config, _cts.Token).SuppressCancellationThrow();
                _cts.Dispose();
                _cts = null;
            }
        }

        private void Update()
        {
            _buttonLabel.text = IsRunning ? "Stop monkey testing" : "Run monkey testing";
        }
    }
}
