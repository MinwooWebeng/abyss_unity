# abyss_unity
Unity 3D abyss game engine

## Overall Structure
<h3> Executor : MonoBehavior </h3>
Holds reference to other MonoBehaviors that are defined under GlobalDependency namespace.
Modifies rendering hierarchy on unity thread.

All GlobalDependency objects that are not Monobehavior should be initialized on Executor.OnEnable(), and cleared on Executor.OnDisable().
Monobehavior GlobalDependency must have earlier execution order than Executor.
The initialization has hidden dependencies, and its order must be carefully considered.

Executor constructs Host, which interprets rendering actions.
Then Executor.Update() calls Host.RenderingActionQueue.TryDequeue() to get unity-thread actions, and execute them. 

TODO: check if Executor is congested, and take action if required.

<h3> GlobalDependency </h3>
This namespace includes several globally referenced classes, for logging/default values for unity objects.
They should be prepared during the initial OnEnable, and cleared on OnDisable. 

CommonShaderLoader.cs - Default shaders
InteractionBase.cs - TODO, user interaction detection
Logger.cs - (for critical debugging) File logging
RendererBase.cs - Holds DOM elements and resources
Statics.cs - Holds static variables {CommonShaderLoader, user hash}
TemporalCameraMover.cs - user movement
UIBase.cs - Holds everything about UI
UnityThreadChecker.cs - global unity thread checker

<h3> Host </h3>
This namespace includes the core operation of AbyssUI. 

Host.cs - reads rendering request, interprets it, pushes actions to Host.RenderingActionQueue, which is accessed from Executor

Host.RxLoop() reads and interprets rendering requests. This should fail if and only if Abyss Engine gives a corrupted request. Also RxStdErrLoop().
The rendering requests are interpreted sequentially, therefore each request interpreting methods defined in HostInterpretRequest.cs is guaranteed to be called in sync as well.

HostInterpretRequest.cs defines Host.Init(), which prepares prerequisits for executing rendering requests. This injects dependencies to UIBase and InteractionBase. It must be called before Host.Start(), in unity thread.

<h3> EngineCom </h3>
This namespace defines IPC interfaces toward the Abyss Engine.