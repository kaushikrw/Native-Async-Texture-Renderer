using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TextureRender {

	public IntPtr TextureData;
	public IntPtr texture;
	public string TexturePath;
	public int Height;
	public int Width;
    public int finalSize = 0;

	public LoadImage loadImage;
    public ImageParameters imageParameters;
	private bool isLoading;
	public bool Done = false;
    public string status;

    public byte[] data;
    public string logfile;

    /*public TextureRender(IntPtr tex,string Path,int H,int W)
	{
		texture = tex;
		TexturePath = Path;
		Height = H;
		Width = W;
	}*/

    public TextureRender(byte[] data, string logfile, string path, int h, int w, IntPtr textureToRender)
    {
        this.data = data;
        this.logfile = logfile;
        TexturePath = path;
        Height = h;
        Width = w;
        texture = textureToRender;
    }

	public void LoadTexture()
	{
        //loadImage = new LoadImage (TexturePath, Width, Height, this);
        loadImage = new LoadImage(data.Length, data, this, logfile, TexturePath, Width, Height);
		loadImage.start ();
		isLoading = true;
	}

	public void Update()
	{
		if (isLoading) {
			if (loadImage.IsDone) {
				Done = true;
				isLoading = false;
			}
		}
	}

	public void OnLoadingComplete()
	{
		TextureData = loadImage.ImageData;
		if (TextureData == IntPtr.Zero) {
			Debug.Log ("NO DATA ERROR");
		} else {
			Debug.Log ("Done: " + loadImage.Status);
			Done = true;
			//NativeTextureRenderer.RenderTexture (TextureData, texture, Width, Height);
		}
	}
}

public class ImageParameters
{
    public int width;
    public int height;
}
