# Shark UI
[About](#about) | [Installation](#installation) | [Documentation](#documentation) | [Diagrams](#diagrams) | [License Notice](#license-notice)

## About
This repository holds a personal work-in-progress project attempting to create a custom text renderer Class Library, that will be extended to general UI rendering, using OpenGL through C# and OpenTK.

If the project meets the usecase of whoever happens to stubmle upon it, it is free to use as is in projects or to dissect as a learning resource.

## Installation
To Install the project, you currently clone this repository and open SharkUI.sln as a visual studio solution file.

## Documentation
To utilize this project, you first have to create a class that extends OpenTKs GameWindow, such as `AppWindow : GameWindow`, then you can utilize the `AppWindow` in a loop like the following:

```C#
using (AppWindow window = new(GameWindowSettings.Default, new() { ClientSize = (960, 540), Title = "App Window" })) {
  window.Run();
}
```

Then, the following funtions signatures can be overloaded to add OpenGL functionality:

```C#
protected override void OnLoad();                                           // Runs on program initialization
protected override void OnUpdateFrame(FrameEventArgs args);                 // Runs every frame for program logic
protected override void OnRenderFrame(FrameEventArgs args);                 // Runs once every frame to render the actual frame
protected override void OnMouseWheel(MouseWheelEventArgs e);                // Handles mousewheel events
protected override void OnFramebufferResize(FramebufferResizeEventArgs e);  // Handles window resizing
```

Alternatively, you can look at the testing project repository that is setup [here](https://github.com/GMcMichael/SharkUITesting).

## Diagrams
Diagram section is a WIP

## License Notice
This project is licensed under the **MIT License**. You are free to use, modify, and distribute this software for any purpose, including commercial use.
The only requirement is that you include the original copyright notice and give proper credit to the original author.

For more details, see the [full license text](https://github.com/GMcMichael/SharkUI/blob/main/LICENSE.txt).
