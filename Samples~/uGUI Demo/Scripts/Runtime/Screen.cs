// Copyright (c) 2019-2024 Koji Hasegawa.

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.Samples.UGUIDemo
{
    public class Screen : MonoBehaviour
    {
        [SerializeField]
        private int delayMillis = 1000;

        // Buttons is interactable after 1 second.
        private void OnEnable()
        {
            var selectables = gameObject.GetComponentsInChildren<Selectable>();
            foreach (var selectable in selectables)
            {
                selectable.interactable = false;
            }

            var ct = this.GetCancellationTokenOnDestroy();
            UniTask.RunOnThreadPool(async () =>
                    {
                        await UniTask.Delay(delayMillis, cancellationToken: ct);
                        foreach (var selectable in selectables)
                        {
                            selectable.interactable = true;
                        }
                    },
                    cancellationToken: ct)
                .Forget();
        }
    }
}
