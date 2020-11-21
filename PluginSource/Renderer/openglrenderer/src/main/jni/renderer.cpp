#include "UnityPluginInterface.h"
#include "IUnityInterface.h"
#include "IUnityGraphics.h"

#include <math.h>
#include <stdio.h>
#include <assert.h>

// --------------------------------------------------------------------------
// Include headers for the graphics APIs we support


#include <GLES3/gl3.h>

// TODO: these are common to all plugins: just extract it

static float g_Time = 0.0f;
extern "C" void SetTimeFromUnity(float t)
{
    g_Time = t;
}

static unsigned char* data;
static void*	g_TexturePointer	= 0;
static int		g_TexWidth			= 0;
static int		g_TexHeight			= 0;

extern "C" void SetTextureFromUnity(unsigned char * _img,void* texturePtr, int w, int h, long size)
{
    g_TexturePointer	= texturePtr;
    g_TexWidth			= w;
    g_TexHeight			= h;
    //data = new unsigned char[size];
    data = _img;

}


// --------------------------------------------------------------------------
// shaders

#define VPROG_SRC(ver, attr, varying)								\
	ver																\
	attr " highp vec3 pos;\n"										\
	attr " lowp vec4 color;\n"										\
	"\n"															\
	varying " lowp vec4 ocolor;\n"									\
	"\n"															\
	"uniform highp mat4 worldMatrix;\n"								\
	"uniform highp mat4 projMatrix;\n"								\
	"\n"															\
	"void main()\n"													\
	"{\n"															\
	"	gl_Position = (projMatrix * worldMatrix) * vec4(pos,1);\n"	\
	"	ocolor = color;\n"											\
	"}\n"															\

static const char* kGlesVProgTextGLES2		= VPROG_SRC("\n", "attribute", "varying");
static const char* kGlesVProgTextGLES3		= VPROG_SRC("#version 300 es\n", "in", "out");

#undef VPROG_SRC

#define FSHADER_SRC(ver, varying, outDecl, outVar)	\
	ver												\
	outDecl											\
	varying " lowp vec4 ocolor;\n"					\
	"\n"											\
	"void main()\n"									\
	"{\n"											\
	"	" outVar " = ocolor;\n"						\
	"}\n"											\

static const char* kGlesFShaderTextGLES2	= FSHADER_SRC("\n", "varying", "\n", "gl_FragColor");
static const char* kGlesFShaderTextGLES3	= FSHADER_SRC("#version 300 es\n", "in", "out lowp vec4 fragColor;\n", "fragColor");

#undef FSHADER_SRC

static GLuint	g_VProg;
static GLuint	g_FShader;
static GLuint	g_Program;
static int		g_WorldMatrixUniformIndex;
static int		g_ProjMatrixUniformIndex;

static GLuint CreateShader(GLenum type, const char* text)
{
    GLuint ret = glCreateShader(type);
    glShaderSource(ret, 1, &text, NULL);
    glCompileShader(ret);

    return ret;
}


// --------------------------------------------------------------------------
// UnitySetGraphicsDevice

static int g_DeviceType = -1;

extern "C" void EXPORT_API UnitySetGraphicsDevice (void* device, int deviceType, int eventType)
{
    // Set device type to -1, i.e. "not recognized by our plugin"
    g_DeviceType = -1;

    if(deviceType == kGfxRendererOpenGLES20Mobile)
    {
        ::printf("OpenGLES 2.0 device\n");
        g_DeviceType = deviceType;

        g_VProg		= CreateShader(GL_VERTEX_SHADER, kGlesVProgTextGLES2);
        g_FShader	= CreateShader(GL_FRAGMENT_SHADER, kGlesFShaderTextGLES2);
    }
    else if(deviceType == kGfxRendererOpenGLES30)
    {
        ::printf("OpenGLES 3.0 device\n");
        g_DeviceType = deviceType;

        g_VProg		= CreateShader(GL_VERTEX_SHADER, kGlesVProgTextGLES3);
        g_FShader	= CreateShader(GL_FRAGMENT_SHADER, kGlesFShaderTextGLES3);
    }

    g_Program = glCreateProgram();
    glBindAttribLocation(g_Program, 1, "pos");
    glBindAttribLocation(g_Program, 2, "color");
    glAttachShader(g_Program, g_VProg);
    glAttachShader(g_Program, g_FShader);
    glLinkProgram(g_Program);

    g_WorldMatrixUniformIndex	= glGetUniformLocation(g_Program, "worldMatrix");
    g_ProjMatrixUniformIndex	= glGetUniformLocation(g_Program, "projMatrix");
}



// --------------------------------------------------------------------------
// UnityRenderEvent
// This will be called for GL.IssuePluginEvent script calls; eventID will
// be the integer passed to IssuePluginEvent. In this example, we just ignore
// that value.


struct MyVertex {
    float x, y, z;
    unsigned int color;
};
static void SetDefaultGraphicsState();
static void DoRendering(const float* worldMatrix, const float* identityMatrix, float* projectionMatrix, const MyVertex* verts);


extern "C" void EXPORT_API UnityRenderEvent (int eventID)
{
    // Unknown graphics device type? Do nothing.
    if(g_DeviceType == -1)
        return;


    // A colored triangle. Note that colors will come out differently
    // in D3D9/11 and OpenGL, for example, since they expect color bytes
    // in different ordering.
    MyVertex verts[3] = {
            { -0.5f, -0.25f,  0, 0xFFff0000 },
            {  0.5f, -0.25f,  0, 0xFF00ff00 },
            {  0,     0.5f ,  0, 0xFF0000ff },
    };


    // Some transformation matrices: rotate around Z axis for world
    // matrix, identity view matrix, and identity projection matrix.

    float phi = g_Time;
    float cosPhi = cosf(phi);
    float sinPhi = sinf(phi);

    float worldMatrix[16] = {
            cosPhi,-sinPhi,0,0,
            sinPhi,cosPhi,0,0,
            0,0,1,0,
            0,0,0.7f,1,
    };
    float identityMatrix[16] = {
            1,0,0,0,
            0,1,0,0,
            0,0,1,0,
            0,0,0,1,
    };
    float projectionMatrix[16] = {
            1,0,0,0,
            0,1,0,0,
            0,0,1,0,
            0,0,0,1,
    };

    // Actual functions defined below
    SetDefaultGraphicsState();
    DoRendering(worldMatrix, identityMatrix, projectionMatrix, verts);
}




// --------------------------------------------------------------------------
// SetDefaultGraphicsState
//
// Helper function to setup some "sane" graphics state. Rendering state
// upon call into our plugin can be almost completely arbitrary depending
// on what was rendered in Unity before.
// Before calling into the plugin, Unity will set shaders to null,
// and will unbind most of "current" objects (e.g. VBOs in OpenGL case).
//
// Here, we set culling off, lighting off, alpha blend & test off, Z
// comparison to less equal, and Z writes off.

static void SetDefaultGraphicsState ()
{
    // Unknown graphics device type? Do nothing.
    if(g_DeviceType == -1)
        return;

    glDisable(GL_CULL_FACE);
    glDisable(GL_BLEND);
    glDepthFunc(GL_LEQUAL);
    glEnable(GL_DEPTH_TEST);
    glDepthMask(GL_FALSE);
}


// TODO: this is common code (platform-independent)


static void DoRendering (const float* worldMatrix, const float* identityMatrix, float* projectionMatrix, const MyVertex* verts)
{
    // Unknown graphics device type? Do nothing.
    if(g_DeviceType == -1)
        return;

    if (g_TexturePointer)
    {

        GLuint gltex = (GLuint)(size_t)(g_TexturePointer);
        glBindTexture(GL_TEXTURE_2D, gltex);

        glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, g_TexWidth, g_TexHeight, GL_RGBA, GL_UNSIGNED_BYTE, data);
        delete[] data;
    }
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
    return UnityRenderEvent;
}
