# Monkey Test Helper

[![Meta file check](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/metacheck.yml/badge.svg)](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/metacheck.yml)
[![Test](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/test.yml/badge.svg)](https://github.com/nowsprinting/test-helper.monkey/actions/workflows/test.yml)
[![openupm](https://img.shields.io/npm/v/com.nowsprinting.test-helper.monkey?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.nowsprinting.test-helper.monkey/)

Reference implementation that performs object-based Unity UI (uGUI) monkey testing and API for custom implementation.

This library can be used in runtime code because it does not depend on the Unity Test Framework.

Required Unity 2019 LTS or later.



## Features

### Monkey testing reference implementation

#### Monkey

Runs monkey tests for uGUI (2D, 3D, and UI) elements.
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


#### MonkeyConfig

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
- **IsInteractable**: Returns whether the `Component` is interactable or not. The default implementation returns true if the component is a uGUI compatible component and its `interactable` property is true.
- **IsIgnored**: Returns whether the `GameObject` is ignored or not. The default implementation returns true if the `GameObject` has `IgnoreAnnotation` attached.
- **IsReachable**: Returns whether the `GameObject` is reachable from the user or not. The default implementation returns true if it can raycast from `Camera.main` to the pivot position.
- **Operators**: Collection of `IOperator` that the monkey invokes. Default is ClickOperator, ClickAndHoldOperator, and TextInputOperator. There is support for standard uGUI components.


#### Annotations for Monkey's behavior

You can control the Monkey's behavior by attaching the annotation components to the `GameObject`.
Use the `TestHelper.Monkey.Annotations` assembly by adding it to the Assembly Definition References.
Please note that this will be included in the release build due to the way it works.

> [!NOTE]  
> Even if the annotations assembly is removed from the release build, the link to the annotation component will remain Scenes and Prefabs in the asset bundle built.
> Therefore, a warning log will be output during instantiate.
> To avoid this, annotations assembly are included in release builds.

##### IgnoreAnnotation

Monkey will not operate objects with `IgnoreAnnotation` attached.

##### InputFieldAnnotation

Specify the character kind and length input into `InputField` with `InputFieldAnnotation`.

##### ScreenOffsetAnnotation

Specify the screen position offset where Monkey operators operates.
Respects `CanvasScaler` but does not calculate the aspect ratio.

##### ScreenPositionAnnotation

Specify the screen position where Monkey operators operates.
Respects `CanvasScaler` but does not calculate the aspect ratio.

##### WorldOffsetAnnotation

Specify the world position offset where Monkey operators operates.

##### WorldPositionAnnotation

Specify the world position where Monkey operators operates.



### Find and operate interactable components API

`GameObjectFinder` is a class that finds `GameObject` by name or path ([glob](https://en.wikipedia.org/wiki/Glob_(programming))).
Can specify the timeout seconds and the functions of **IsInteractable** and **IsReachable** for the constructor.


#### Find GameObject by name

Find `GameObject` by name (wait until they appear).

Arguments:

- **name**: Find `GameObject` name
- **reachable**: Find only reachable object. Default is true
- **interactable**: Find only interactable object. Default is false

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


#### Find GameObject by path

Find `GameObject` by path (wait until they appear).

Arguments:

- **path**: Find `GameObject` hierarchy path separated by `/`. Can specify ([glob](https://en.wikipedia.org/wiki/Glob_(programming))) pattern
- **reachable**: Find only reachable object. Default is true
- **interactable**: Find only interactable object. Default is false

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
        var finder = new GameObjectFinder(); // Default is 1 second timeout
        var button = await finder.FindByPathAsync("/**/Confirm/**/Cancel", reachable: true, interactable: true);
    }
}
```


#### Operate interactable components

`GetInteractableComponents()` are extensions of `GameObject` that return interactable components.

`SelectOperators` and `SelectOperators<T>` are extensions of `Component` that return available operators.
Operators implement the `IOperator` interface. It has an `OperateAsync` method that operates on the component.

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
        var buttonObject = await finder.FindByNameAsync("StartButton", interactable: true);

        var button = buttonObject.GetInteractableComponents().First();
        var clickOperator = button.SelectOperators<IClickOperator>(_operators).First();
        clickOperator.OperateAsync(button);
    }
}
```



### Find interactable components on the scene

`InteractableComponentsFinder.FindInteractableComponents()` method returns all interactable components on the scene.
Can specify the **IsInteractable** function and **Operator**s for the constructor.

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
        var components = new InteractableComponentsFinder().FindInteractableComponents();
    }
}
```



## Customization

### Functions for the strategy pattern

If your game title uses a custom UI framework that is not uGUI compatible and/or requires special operating, you can customize the monkey's behavior using the following:


#### IsInteractable

Returns whether the `Component` is interactable or not.
`DefaultComponentInteractableStrategy.IsInteractable()` returns true if the component is a uGUI compatible component and its `interactable` property is true.

You should replace this when you want to control special components that comprise your game title.


#### IsIgnored

Returns whether the `GameObject` is ignored or not.
`DefaultIgnoreStrategy.IsIgnored()` returns true if the `GameObject` has `IgnoreAnnotation` attached.

You should replace this when you want to ignore specific objects (e.g., by name and/or path) in your game title.


#### IsReachable

Returns whether the `GameObject` is reachable from the user or not.
`DefaultReachableStrategy.IsReachable()` returns true if it can raycast from `Camera.main` to the pivot position.

You should replace this when you want to customize the raycast point (e.g., randomize position, specify camera).


#### Operators

Operators are a collection of `IOperator` that the monkey invokes.

You should replace this when you want to operate special components that comprise your game title (e.g., custom UI component, special click position).

A sub-interface of the `IOperator` (e.g., `IClickOperator`) must be implemented to represent the type of operator.
An operator must implement the `CanOperate` method to determine whether an operation such as click is possible and the `OperateAsync` method to execute the operation.



## Troubleshooting

### Monkey

#### Thrown TimeoutException

If thrown `TimeoutException` with the following message:

```
Interactive component not found in 5 seconds
```

This indicates that no `GameObject` with an interactable component appeared in the scene within specified seconds.
`GameObject` determined to be Ignored will be excluded, even if they are interactable.
`GameObject` that are not reachable by the user are excluded, even if they are interactable.

More details can be output using the verbose option (`MonkeyConfig.Verbose`).

The waiting seconds can be specified in the `MonkeyConfig.SecondsToErrorForNoInteractiveComponent`.


#### Operation log message

```
UGUIClickOperator operates to StartButton (UGUIMonkeyAgent01_0001.png)
```

This log message is output just before the operator `UGUIClickOperator` operates on the `GameObject` named `StartButton`.
"UGUIMonkeyAgent01_0001.png" is the screenshot file name taken just before the operation.

Screenshots are taken when the `MonkeyConfig.Screenshots` is set.


#### Verbose log messages

You can output details logs when the `MonkeyConfig.Verbose` is true.

##### Lottery entries

```
Lottery entries: {
  StartButton(30502):Button:UGUIClickOperator,
  StartButton(30502):Button:UGUIClickAndHoldOperator,
  MenuButton(30668):Button:UGUIClickOperator,
  MenuButton(30668):Button:UGUIClickAndHoldOperator}
```

Each entry format is `GameObject` name (instance ID) : `Component` type : `Operator` type.

This log message shows the lottery entries that the monkey can operate.
Entries are made by the `IsInteractable` and `Operator.CanOperate` method.
`IsIgnore` and `IsReachable` are not used at this time.

If there are zero entries, the following message is output:

```
No lottery entries.
```

##### Ignored GameObject

If the lotteries `GameObject` is ignored, the following message will be output and lottery again.

```
Ignored QuitButton(30388).
```

##### Not reachable GameObject

If the lotteries `GameObject` is not reachable by the user, the following messages will be output and lottery again.

```
Not reachable to CloseButton(-2278), position=(515,-32). Raycast is not hit.
```

Or

```
Not reachable to BehindButton(-2324), position=(320,240). Raycast hit other objects: {BlockScreen, FrontButton}
```

The former output is when the object is off-screen, and the latter is when other objects hide the pivot position.
The position to send the raycast can be arranged using annotation components such as `ScreenOffsetAnnotation`.

##### No GameObjects that are operable

If all lotteries `GameObject` are not operable, the following message is displayed.
If this condition persists, a `TimeoutException` will be thrown.

```
Lottery entries are empty or all of not reachable.
```



### GameObjectFinder

#### Thrown TimeoutException

##### Not found

If no `GameObject` is found with the specified name or path, throw `TimeoutException` with the following message:

```
GameObject `Target` is not found.
```

##### Not match path

If `GameObject` is found with the specified name but does not match path, throw `TimeoutException` with the following message:

```
GameObject `Target` is found, but it does not match path `Path/To/Target`.
```

##### Not reachable

If `GameObject` is found with the specified name or path but not reachable, throw `TimeoutException` with the following message:

```
GameObject `Target` is found, but not reachable.
```

If you need detailed logs, pass an `ILogger` instance to the constructor of `GameObjectFinder`.

##### Not interactable

If `GameObject` is found with the specified name or path but not interactable, throw `TimeoutException` with the following message:

```
GameObject `Target` is found, but not interactable.
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
> Required installation packages for running tests (when embedded package or adding to the `testables` in manifest.json), as follows:
> - [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest) package v1.3.4 or later
> - TextMesh Pro package or Unity UI package v2.0.0 or later


### Run tests

Generate a temporary project and run tests on each Unity version from the command line.

```bash
make create_project
UNITY_VERSION=2019.4.40f1 make -k test
```

> [!WARNING]  
> You must select "Input Manager (Old)" or "Both" in the **Project Settings > Player > Active Input Handling** for running tests.


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
