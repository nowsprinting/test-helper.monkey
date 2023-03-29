# Monkey test helper library for Unity UI (uGUI)

[![Meta file check](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/metacheck.yml/badge.svg)](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/metacheck.yml)
[![Test](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/test.yml/badge.svg)](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/test.yml)
[![openupm](https://img.shields.io/npm/v/com.nowsprinting.test-helper.monkey?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.nowsprinting.test-helper.monkey/)


Reference implementation that performs object-based Unity UI (uGUI) monkey tests and an API for implementation.


## Features

### Monkey test reference implementation

Run a monkey test for uGUI (2D, 3D, and UI) elements.
`Monkey.Run` method operates on randomly selected objects. It does not use screen points.

Usage:

```csharp
[Test]
public async Task MonkeyTesting()
{
    var config = new MonkeyConfig
    {
        Lifetime = TimeSpan.FromMinutes(3),
        DelayMillis = 200
    };
    var cancellationTokenSource = new CancellationTokenSource();

    await Monkey.Run(config, cancellationTokenSource.Token);
}
```

> **Warning**  
> In batchmode, does not operate UI elements on Canvas.
> Because `UnityEngine.UI.GraphicRaycaster` does not work in batchmode.


### Find and operate interactive uGUI elements API

#### InteractiveComponentCollector.FindInteractiveComponents

Returns interactive uGUI components.
If the argument is true, return only user really reachable components (using the `IsReallyInteractiveFromUser` method).

Usage:

```csharp
[Test]
public void FindAndOperationInteractiveComponent()
{
    var component = InteractiveComponentCollector.FindInteractiveComponents(true)
        .First();
    if (component.CanClick())
    {
        component.Click();
    }
}
```

> **Warning**  
> In batchmode, it does not return interactable UI elements on Canvas.
> Because `UnityEngine.UI.GraphicRaycaster` does not work in batchmode.


#### InteractiveComponent.IsReallyInteractiveFromUser

Returns true if the component is really reachable from the user.

Usage:

```csharp
[Test]
public void FindAndOperationInteractiveComponent()
{
    var component = InteractiveComponentCollector.FindInteractiveComponents(false)
        .First();
    if (component.IsReallyInteractiveFromUser() && component.CanClick())
    {
        component.Click();
    }
}
```

> **Warning**  
> In batchmode, `InteractiveComponent.IsReallyInteractiveFromUser` for UI elements is always false.
> Because `UnityEngine.UI.GraphicRaycaster` does not work in batchmode.


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

Be grateful if you could label the PR as `enhancement`, `bug`, `chore`, and `documentation`.
See [PR Labeler settings](.github/pr-labeler.yml) for automatically labeling from the branch name.


### How to development

Add this repository as a submodule to the Packages/ directory in your project.

Examples:

```bash
$ git submodule add https://github.com/nowsprinting/test-helper.monkey.git Packages/com.nowsprinting.test-helper.monkey
```

And required install [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest) package v1.3 or later for running tests.


## Release workflow

Run `Actions | Create release pull request | Run workflow` and merge created PR.
(Or bump version in package.json on default branch)

Then, Will do the release process automatically by [Release](.github/workflows/release.yml) workflow.
And after tagged, OpenUPM retrieves the tag and updates it.

Do **NOT** manually operation the following operations:

- Create release tag
- Publish draft releases
