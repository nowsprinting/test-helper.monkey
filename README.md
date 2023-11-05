# Monkey Test Helper

[![Meta file check](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/metacheck.yml/badge.svg)](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/metacheck.yml)
[![Test](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/test.yml/badge.svg)](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/test.yml)
[![openupm](https://img.shields.io/npm/v/com.nowsprinting.test-helper.monkey?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.nowsprinting.test-helper.monkey/)

Reference implementation that performs object-based Unity UI (uGUI) monkey tests and an API for custom implementation.

This library can use in runtime code because it does not depend on the Unity Test Framework.

Required Unity 2019 LTS or later.



## Features

### Monkey test reference implementation

Run a monkey test for uGUI (2D, 3D, and UI) elements.
`Monkey.Run` method operates on randomly selected objects. It does not use screen points.

Usage:

```csharp
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Monkey;

[TestFixture]
public class MyIntegrationTest
{
    [Test]
    public async Task MonkeyTesting()
    {
        var config = new MonkeyConfig
        {
            Lifetime = TimeSpan.FromMinutes(2),
            DelayMillis = 200,
            SecondsToErrorForNoInteractiveComponent = 5,
        };

        await Monkey.Run(config);
    }
}
```

Configurations in `MonkeyConfig`:

- **Lifetime**: Running time
- **DelayMillis**: Delay time between operations
- **SecondsToErrorForNoInteractiveComponent**: Seconds to determine that an error has occurred when an object that can be interacted with does not exist
- **TouchAndHoldDelayMillis**: Delay time for touch-and-hold
- **Random**: Random generator
- **Logger**: Logger
- **Gizmos**: Show Gizmos on `GameView` during running monkey test if true
- **Screenshots**: Take screenshots during running the monkey test if set a `ScreenshotOptions` instance.


### Find and operate interactive uGUI elements API

#### InteractiveComponentCollector.FindInteractiveComponents

Returns interactive uGUI components.
If the argument is true, return only user-really reachable components (using the `IsReallyInteractiveFromUser` method).

Usage:

```csharp
using System.Linq;
using NUnit.Framework;
using TestHelper.Monkey;

[TestFixture]
public class MyIntegrationTest
{
    [Test]
    public void MyTestMethod()
    {
        var component = InteractiveComponentCollector.FindInteractiveComponents(true)
            .First();

        Assume.That(component.CanClick(), Is.True);
        component.Click();
    }
}

```

#### InteractiveComponent.IsReallyInteractiveFromUser

Returns true if the component is really reachable from the user.

Usage:

```csharp
using System.Linq;
using NUnit.Framework;
using TestHelper.Monkey;

[TestFixture]
public class MyIntegrationTest
{
    [Test]
    public void MyTestMethod()
    {
        var component = InteractiveComponentCollector.FindInteractiveComponents(false)
            .First();

        Assume.That(component.IsReallyInteractiveFromUser(), Is.True);
        Assume.That(component.CanClick(), Is.True);
        component.Click();
    }
}
```



## Installation

You can choose from two typical installation methods.

### Install via Package Manager window

1. Open the **Package Manager** tab in Project Settings window (**Editor > Project Settings**)
2. Click **+** button under the **Scoped Registries** and enter the following settings (figure 1.):
   1. **Name:** `package.openupm.com`
   2. **URL:** `https://package.openupm.com`
   3. **Scope(s):** `com.nowsprinting` and `com.cysharp`
3. Open the Package Manager window (**Window > Package Manager**) and select **My Registries** in registries drop-down list (figure 2.)
4. Click **Install** button on the `com.nowsprinting.test-helper.monkey` package

> **Note**  
> Do not forget to add `com.cysharp` into scopes. These are used within this package.

**Figure 1.** Package Manager tab in Project Settings window.

![](Documentation~/ProjectSettings_Dark.png#gh-dark-mode-only)
![](Documentation~/ProjectSettings_Light.png#gh-light-mode-only)

**Figure 2.** Select registries drop-down list in Package Manager window.

![](Documentation~/PackageManager_Dark.png/#gh-dark-mode-only)
![](Documentation~/PackageManager_Light.png/#gh-light-mode-only)


### Install via OpenUPM-CLI

If you installed [openupm-cli](https://github.com/openupm/openupm-cli), run the command below:

```bash
openupm add com.nowsprinting.test-helper.monkey
```


### Add assembly reference

1. Open your test assembly definition file (.asmdef) in **Inspector** window
2. Add **TestHelper.Monkey** into **Assembly Definition References**



## License

MIT License



## How to contribute

Open an issue or create a pull request.

Be grateful if you could label the PR as `enhancement`, `bug`, `chore`, and `documentation`.
See [PR Labeler settings](.github/pr-labeler.yml) for automatically labeling from the branch name.



## How to development

Add this repository as a submodule to the Packages/ directory in your project.

```bash
git submodule add https://github.com/nowsprinting/test-helper.monkey.git Packages/com.nowsprinting.test-helper.monkey
```

Generate a temporary project and run tests on each Unity version from the command line.

```bash
make create_project
UNITY_VERSION=2019.4.40f1 make -k test
```

> **Warning**  
> - Required install packages for running tests (when adding to the `testables` in package.json), as follows:
>   - [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest) package v1.3.4 or later
> - Set the player screen resolution to 1920x1080 when running Play Mode tests on Player.



## Release workflow

Run **Actions > Create release pull request > Run workflow** and merge created pull request.
(Or bump version in package.json on default branch)

Then, Will do the release process automatically by [Release](.github/workflows/release.yml) workflow.
And after tagging, OpenUPM retrieves the tag and updates it.

Do **NOT** manually operation the following operations:

- Create a release tag
- Publish draft releases
