using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class LoadImage
{

	[DllImport ("ImgDecoder")]
	private static extern IntPtr LoadTextureIntoMemory (int w, int h, StringBuilder path);

    [DllImport("nativejpeg")]
    private static extern IntPtr read_Image(int size, IntPtr _img, StringBuilder logfile);

    [DllImport("nativejpeg")]
    private static extern int GetDataSize();

    [DllImport("nativejpeg")]
    private static extern int TestPlugin(StringBuilder path);

    public delegate void LoadCompleteEvent ();

	public LoadCompleteEvent LoadComplete;

	public IntPtr ImageData;
	public StringBuilder Path;
    public StringBuilder logfile;

	public TextureRender tRender;

	public string ImagePath;
	public string Status;

	public int width;
	public int height;

    public int size_t;
    public byte[] data;
    public int returnedSize;

	//threading variables
	private bool m_IsDone = false;
	private object m_Handle = new object ();
	private Thread m_Thread = null;

	/*public LoadImage (string ImagePath, int w, int h, TextureRender texRenderer)
	{
		Path = new StringBuilder ();
		Path.Append (ImagePath);
		width = w;
		height = h;
		tRender = texRenderer;
	}*/

    public LoadImage (int size, byte[] data, TextureRender textureRender, string logfilePath, string testpath, int w, int h)
    {
        Path = new StringBuilder();
        Path.Append(testpath);
        size_t = size;
        this.data = data;
        tRender = textureRender;
        logfile = new StringBuilder();
        logfile.Append(logfilePath);
        width = w;
        height = h;
    }

	public bool IsDone {
		get {
			bool tmp;
			lock (m_Handle) {
				tmp = m_IsDone;
			}
			return tmp;
		}
		set {
			lock (m_Handle) {
				m_IsDone = value;
			}
		}
	}

	public void start ()
	{
		m_Thread = new Thread (Run);
		m_Thread.Start ();
	}

	public void Abort ()
	{
		m_Thread.Abort ();
	}

	public void LoadPanoFunction ()
	{
		try
        {
			ImageData = new IntPtr (width * height * 4);

            IntPtr unmanagedPointer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, unmanagedPointer, data.Length);

            ImageData = read_Image(size_t, unmanagedPointer, logfile);

            returnedSize = TestPlugin(logfile);
            //ImageData = LoadTextureIntoMemory(width, height, Path);
            tRender.TextureData = ImageData;

            //Marshal.FreeHGlobal(unmanagedPointer);
        }
        catch (Exception e)
        {
			Status = e.Message + "|| TRACE:" + e.StackTrace;
		}
	}

	protected void OnFinished ()
	{
		m_Thread.Join ();
	}

	private void Run ()
	{
		LoadPanoFunction ();
		IsDone = true;
		//OnFinished();
	}
}
