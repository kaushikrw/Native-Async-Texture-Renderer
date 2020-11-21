# Native-Async-Texture-Renderer

# Asynchronous Texture Uploads and Rendering for Unity

Native (C++) Unity plugin that can upload large textures to the GPU asynchronously and Render on a separate thread, without blocking the main render thread. This meant primarily for mobile platforms.

Supported Platforms and API:

 - Android armv7 x86 and x64 with openGL 2.0+
 - Unity 2018.1+
 - **ios** and metal api's might be coming soon. 

# Code Organization

Code is organized as follows:

 - `Project:`
	 - Unity project source.
 - `PluginSource` 
	 - `Renderer:` is the android studio project files and source code for the native c++ rendering plugin build with ndk.
	 - `LibJPEG wrapper:` source code for the underlying jpeg files decoding library. Based on libjpeg-turbo -> [link](https://github.com/libjpeg-turbo/libjpeg-turbo)
	 - `LibPNG :` .PNG texture files support will be added later.

## Building Android Studio Plugin

use **ndk-build** in the **JNI** folder to generate appropriate **.so** libraries.

## Usage

Check **RenderExample.cs** for a working example.

**Note**: Init Texture2D objects before the actual render call occurs. Even creating a texture instance is done on the Unity's main thread, hence it's a good idea to either use a re-usable TexturePool for a large amount of textures or pre-init textures for small amounts of rendering.


# Unity Synchronization and Async Approach

NativeTextureRenderer uses a RenderQueue managed by the main thread to add and run Render Events.
```
NativeTextureRenderer.RenderTexture(string path, IntPtr texturePtr, int width, int height);
```

The following method automatically, adds a render event into the Queue and executes it in FIFO order.

```
GL.IssuePluginEvent()
```

Is called at every renderevent drawcall to update the rendering instance from c++ to unity engine.
