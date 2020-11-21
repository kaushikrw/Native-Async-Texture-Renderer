LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)					# clean everything up to prepare for a modul

include $(CLEAR_VARS)					# clean everything up to prepare for a module

LOCAL_MODULE := openglrenderer
LOCAL_SRC_FILES :=	renderer.cpp
LOCAL_LDLIBS := -lGLESv3 -llog

include $(BUILD_SHARED_LIBRARY)			# start building based on everything since CLEAR_VARS
