# Monkey Test Helper

[![Meta file check](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/metacheck.yml/badge.svg)](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/metacheck.yml)
[![Test](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/test.yml/badge.svg)](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/test.yml)
[![openupm](https://img.shields.io/npm/v/com.nowsprinting.test-helper.monkey?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.nowsprinting.test-helper.monkey/)

Reference implementation that performs object-based Unity UI (uGUI) monkey tests and API for custom implementation.

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
- **Random**: Pseudo-random number generator
- **Logger**: Logger
- **Verbose**: Output verbose log if true
- **Gizmos**: Show Gizmos on `GameView` during running monkey test if true
- **Screenshots**: Take screenshots during running the monkey test if set a `ScreenshotOptions` instance.
    - **Directory**: Directory to save screenshots. If omitted, the directory specified by command line argument "-testHelperScreenshotDirectory" is used. If the command line argument is also omitted, `Application.persistentDataPath` + "/TestHelper/Screenshots/" is used.
    - **FilenameStrategy**: Strategy for file paths of screenshot images. Default is test case name and four digit sequential number.
    - **SuperSize**: The factor to increase resolution with. Default is 1.
    - **StereoCaptureMode**: The eye texture to capture when stereo rendering is enabled. Default is `LeftEye`.

More customize for your project:

- **IsInteractable**: Function returns the `Component` is interactable or not. The default implementation is support for standard Unity UI (uGUI) components.
- **IsIgnored**: Function returns the `GameObject` is ignored or not.
- **IsReachable**: Function returns the `GameObject` is reachable from user or not. Default implementation is using Raycaster and includes ScreenPointStrategy (GetScreenPoint function).
- **Operators**: Operators that the monkey invokes. Default is ClickOperator, ClickAndHoldOperator, and TextInputOperator. There is support for standard Unity UI (uGUI) components.


### Annotations for Monkey's behavior

You can control the Monkey's behavior by attaching the annotation components to the GameObject.
Use the `TestHelper.Monkey.Annotations` assembly by adding it to the Assembly Definition References.
Please note that this will be included in the release build due to the way it works.

> [!NOTE]  
> Even if the annotations assembly is removed from the release build, the link to the annotation component will remain Scenes and Prefabs in the asset bundle built.
> Therefore, a warning log will be output during instantiate.
> To avoid this, annotations assembly are included in release builds.

#### IgnoreAnnotation

Monkey will not operate objects with `IgnoreAnnotation` attached.

#### InputFieldAnnotation

Specify the character kind and length input into `InputField` with `InputFieldAnnotation`.

#### ScreenOffsetAnnotation

Specify the screen position offset on the screen space where Monkey operates.

#### ScreenPositionAnnotation

Specify the screen position where Monkey operates.

#### WorldOffsetAnnotation

Specify the screen position offset on world space where Monkey operates.

#### WorldPositionAnnotation

Specify the world position where Monkey operates.


### Find and operate interactable uGUI components

#### GameObjectFinder.FindByNameAsync

Find GameObject by name (wait until they appear).

Arguments:

- **name**: Find GameObject name
- **reachable**: Find only reachable object
- **interactable**: Find only interactable object

Usage:

```csharp
using NUnit.Framework;
using TestHelper.Monkey;

[TestFixture]
public class MyIntegrationTest
{
    [Test]
    public void MyTestMethod()
    {
        var finder = new GameObjectFinder(5d); // 5 seconds timeout
        var dialog = await finder.FindByNameAsync("ConfirmDialog", reachable: true, interactable: false);
    }
}
```

#### GameObjectFinder.FindByPathAsync

Find GameObject by path (wait until they appear).

Arguments:

- **path**: Find GameObject hierarchy path separated by `/`. Can specify glob pattern
- **reachable**: Find only reachable object
- **interactable**: Find only interactable object

Usage:

```csharp
using NUnit.Framework;
using TestHelper.Monkey;

[TestFixture]
public class MyIntegrationTest
{
    [Test]
    public void MyTestMethod()
    {
        var finder = new GameObjectFinder();
        var button = await finder.FindByPathAsync("/**/Confirm/**/Cancel", reachable: true, interactable: true);
    }
}
```

#### Get interactable components and operators

`GetInteractableComponents` are extensions of `GameObject` that return interactable components.

`SelectOperators` and `SelectOperators<T>` are extensions of `Component` that return available operators.
Operators implements `IOperator` interface. It has `OperateAsync` method that operates on the component.

Usage:

```csharp
using NUnit.Framework;
using TestHelper.Monkey;

[TestFixture]
public class MyIntegrationTest
{
    [Test]
    public void ClickStartButton()
    {
        var finder = new GameObjectFinder();
        var button = await finder.FindByNameAsync("StartButton", interactable: true);

        var buttonComponent = button.GetInteractableComponents().First();
        var clickOperator = buttonComponent.SelectOperators<IClickOperator>(_operators).First();
        clickOperator.OperateAsync(buttonComponent);
    }
}
```

#### Find interactable components on the scene

Returns interactable uGUI components.

Usage:

```csharp
using System.Linq;
using NUnit.Framework;
using TestHelper.Monkey;
using TestHelper.Monkey.Operators;

[TestFixture]
public class MyIntegrationTest
{
    [Test]
    public void MyTestMethod()
    {
        var components = new InteractableComponentsFinder().FindInteractableComponents();

        var firstComponent = components.First();
        var clickAndHoldOperator = firstComponent.SelectOperators<IClickAndHoldOperator>(_operators).First();
        await clickAndHoldOperator.OperateAsync(firstComponent);
    }
}
```



## Use in runtime code

The "Define Constraints" is set to `UNITY_INCLUDE_TESTS || COM_NOWSPRINTING_TEST_HELPER_ENABLE` in this package's assembly definition files, so it is generally excluded from release builds.

To use the feature in release builds, add `COM_NOWSPRINTING_TEST_HELPER_ENABLE` to the "Define Symbols" at build time.



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

> [!NOTE]  
> Do not forget to add `com.cysharp` into scopes. These are used within this package.

**Figure 1.** Package Manager tab in Project Settings window.

![](Documentation~/ProjectSettings_Dark.png#gh-dark-mode-only)
![](Documentation~/ProjectSettings_Light.png#gh-light-mode-only)

**Figure 2.** Select registries drop-down list in Package Manager window.

![](Documentation~/PackageManager_Dark.png#gh-dark-mode-only)
![](Documentation~/PackageManager_Light.png#gh-light-mode-only)


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

### Clone repo as a embedded package

Add this repository as a submodule to the Packages/ directory in your project.

```bash
git submodule add git@github.com:nowsprinting/test-helper.monkey.git Packages/com.nowsprinting.test-helper.monkey
```

> [!WARNING]  
> Required install packages for running tests (when adding to the `testables` in package.json), as follows:
> - [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest) package v1.3.4 or later
> - TextMesh Pro package or Unity UI package v2.0.0 or later


### Run tests

Generate a temporary project and run tests on each Unity version from the command line.

```bash
make create_project
UNITY_VERSION=2019.4.40f1 make -k test
```


### Release workflow

The release process is as follows:

1. Run **Actions > Create release pull request > Run workflow**
2. Merge created pull request

Then, will do the release process automatically by [Release](.github/workflows/release.yml) workflow.
After tagging, [OpenUPM](https://openupm.com/) retrieves the tag and updates it.

> [!CAUTION]  
> Do **NOT** manually operation the following operations:
> - Create a release tag
> - Publish draft releases

> [!CAUTION]  
> You must modify the package name to publish a forked package.
