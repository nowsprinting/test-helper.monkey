# Monkey testing helper library for Unity Test Framework

[![Meta file check](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/metacheck.yml/badge.svg)](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/metacheck.yml)


## Features

### Monkey testing reference implementation

Run monkey testing for uGUI elements.

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


## Installation

If you installed [openupm-cli](https://github.com/openupm/openupm-cli), run the command below

```bash
openupm add com.nowsprinting.test-helper.monkey
```

Or open Package Manager window (Window | Package Manager) and add package from git URL

```
https://github.com/nowsprinting/test-helper.monkey.git
```


## Limitations

In batchmode, `InteractiveComponent.IsReallyInteractiveFromUser` for UI elements is always false.
Because `UnityEngine.UI.GraphicRaycaster` does not work in batchmode.


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
