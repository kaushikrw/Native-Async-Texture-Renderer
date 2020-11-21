using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class RenderExample : MonoBehaviour
{
    [DllImport("nativejpeg")]
    private static extern int TestPlugin(StringBuilder sb);

    public string[] FileName;
    public Material[] Mat;
    public Texture2D[] tex;
    public byte[] imageData;
    public string datapath;

    void Start()
    {
        //yield return new WaitForSeconds(3f);
        datapath = Application.persistentDataPath;
        tex = new Texture2D[Mat.Length];
        for (int i = 0; i < tex.Length; i++)
        {
            tex[i] = new Texture2D(2048, 2048, TextureFormat.ARGB32, false);

            Mat[i].mainTexture = tex[i];
        }
    }

    public void RenderFunc()
    {
        //Directory.CreateDirectory(datapath + "/source/");
        for (int i = 0; i < Mat.Length; i++)
        {
            string TexturePath = datapath + "/" + FileName [i];
            NativeTextureRenderer.RenderTexture (TexturePath,tex[i].GetNativeTexturePtr(),tex[i].width,tex[i].height);
        }
    }
}
