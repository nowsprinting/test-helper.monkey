// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.Samples.UGUIDemo
{
    [RequireComponent(typeof(Button))]
    public class TransitionButton : MonoBehaviour
    {
        [SerializeField]
        private GameObject transitionTarget;

        [SerializeField]
        private int delayMillis = 1000;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        // Display target screen after delay.
        private async void OnClick()
        {
            _button.interactable = false; // Note: Not set interactable=false other buttons. This is a bug in this app.
            await UniTask.Delay(delayMillis, cancellationToken: this.GetCancellationTokenOnDestroy());

            _button.transform.parent.gameObject.SetActive(false); // Hide current screen.
            transitionTarget.SetActive(true); // Display target screen.
        }
    }
}
