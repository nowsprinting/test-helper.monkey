# Monkey testing library for Unity UI (uGUI)

[![Meta file check](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/metacheck.yml/badge.svg)](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/metacheck.yml)
[![Test](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/test.yml/badge.svg)](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/test.yml)
[![openupm](https://img.shields.io/npm/v/com.nowsprinting.test-helper.monkey?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.nowsprinting.test-helper.monkey/)


## Features

### Monkey testing reference implementation

Run monkey testing for uGUI (2D, 3D and UI) elements.

```csharp
[Test]
public async Task MonkeyTesting()
{
    var config = new MonkeyConfig();
    var cancellationTokenSource = new CancellationTokenSource();

    await Monkey.Run(config, cancellationTokenSource.Token);
}
```

### Find and operation interactive uGUI elements

```csharp
[Test]
public void FindAndOperationInteractiveComponent()
{
    var component = InteractiveComponentCollector.FindInteractiveComponents().First();
    if (component.CanClick())
    {
        component.Click();
    }
}
```


## Limitations

In batchmode, `InteractiveComponent.IsReallyInteractiveFromUser` for UI elements is always false.
Because `UnityEngine.UI.GraphicRaycaster` does not work in batchmode.


## Installation

If you installed [openupm-cli](https://github.com/openupm/openupm-cli), run the command below

```bash
openupm add com.nowsprinting.test-helper.monkey
```

Or open Package Manager window (Window | Package Manager) and add package from git URL

```bash
https://github.com/nowsprinting/test-helper.monkey.git
```


## License

MIT License


## How to contribute

Open an issue or create a pull request.

Be grateful if you could label the PR as `enhancement`, `bug`, `chore` and `documentation`.
See [PR Labeler settings](.github/pr-labeler.yml) for automatically labeling from the branch name.


## Release workflow

Run `Actions | Create release pull request | Run workflow` and merge created PR.
(Or bump version in package.json on default branch)

Then, Will do the release process automatically by [Release](.github/workflows/release.yml) workflow.
And after tagged, OpenUPM retrieves the tag and updates it.

Do **NOT** manually operation the following operations:

- Create release tag
- Publish draft releases
