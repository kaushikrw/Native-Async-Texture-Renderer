using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

public class ProcessRawTexture
{
    public delegate void LoadCompleteEvent();
    public LoadCompleteEvent LoadComplete;

    public byte[] rawData;
    public string path;

    public ImageParameters imageParameters;

    public int width;
    public int height;

    //threading variables
    private bool m_IsDone = false;
    private object m_Handle = new object();
    private Thread m_Thread = null;

    public ProcessRawTexture(byte[] data, string destinationPath, ImageParameters parameters)
    {
        rawData = data;
        path = destinationPath;
        imageParameters = parameters;
        Start();
    }

    public bool IsDone
    {
        get
        {
            bool tmp;
            lock (m_Handle)
            {
                tmp = m_IsDone;
            }
            return tmp;
        }
        set
        {
            lock (m_Handle)
            {
                m_IsDone = value;
            }
        }
    }

    public void Start()
    {
        m_Thread = new Thread(Run);
        m_Thread.Start();
    }

    public void Abort()
    {
        m_Thread.Abort();
    }

    public void LoadRawData()
    {
        if(!File.Exists(path))
            File.WriteAllBytes(path, rawData);
        //Mat mat = Imgcodecs.imread(path);
        //width = (int)mat.size().width;
        //height = (int)mat.size().height;
        //imageParameters.width = width;
        //imageParameters.height = height;
    }

    protected void OnFinished()
    {
        m_Thread.Join();
    }

    private void Run()
    {
        LoadRawData();
        IsDone = true;
        //OnFinished();
    }
}