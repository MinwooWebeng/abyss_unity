# abyss_unity
Unity 3D abyss game engine

## Overall Structure
<h3> Executor : MonoBehavior </h3>
Holds reference to other MonoBehaviors that are defined under GlobalDependency namespace.
Modifies rendering hierarchy on unity thread.

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
This namespace defines the concurrent 