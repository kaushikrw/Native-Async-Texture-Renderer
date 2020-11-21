using System.Runtime.InteropServices;
using System;
using System.Text;
using UnityEngine;
using System.IO;

public class NativeTextureRenderer : MonoBehaviour
{
    public delegate void RenderTextureEvent(string Path, IntPtr texture, int Width, int Height);
    public static RenderTextureEvent RenderTexture;

    private TaskExecutor RenderQueue = new TaskExecutor();
    private TextureRender tRender;

    private bool taskExecuting;

#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("openglrenderer")]
#endif
    private static extern void SetTimeFromUnity(float t);


    // We'll also pass native pointer to a texture in Unity.
    // The plugin will fill texture data from native code.
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("openglrenderer")]
#endif
    private static extern void SetTextureFromUnity(IntPtr _img, System.IntPtr texture, int w, int h);



#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
    [DllImport("openglrenderer")]
#endif
    private static extern System.IntPtr GetRenderEventFunc();

    private void OnEnable()
    {
        RenderTexture += RenderToQueue;
    }

    private void OnDisable()
    {
        RenderTexture -= RenderToQueue;
    }

    private void RenderToQueue(string Path, IntPtr texture, int Width, int Height)
    {
        RenderQueue.ScheduleTask(new Task(delegate
        {
            //tRender = new TextureRender(texture, Path, Height, Width);
            byte[] data = File.ReadAllBytes(Path);
            tRender = new TextureRender(data, Application.persistentDataPath + "/logfile.txt", Path, Width, Height, texture);
            Debug.Log("leength : " + data.Length);
            tRender.LoadTexture();
        }));
    }

    private static void CreateTextureAndPassToPlugin(IntPtr Img, IntPtr Tex, int width, int height)
    {
        SetTextureFromUnity(Img, Tex, width, height);
        RenderObject();
    }

    private static void RenderObject()
    {
        Debug.Log("Sending Render Event");
        SetTimeFromUnity(Time.timeSinceLevelLoad);
        GL.IssuePluginEvent(GetRenderEventFunc(), 1);
    }

    public void Update()
    {
        if (RenderQueue.count > 0 && !taskExecuting)
        {
            if (RenderQueue.count > 0)
            {
                taskExecuting = true;
                RenderQueue.dequeTask();
            }
        }
        if (RenderQueue.count <= 0)
        {

        }

        if (taskExecuting)
        {
            tRender.Update();
            if (tRender.Done)
            {
                Debug.Log("FINAL RETURN : " + tRender.finalSize);
                CreateTextureAndPassToPlugin(tRender.TextureData, tRender.texture, 2048, 2048);
                taskExecuting = false;
            }

        }
    }

}
