// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
#if OPENGL
#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
using GetParamName = MonoMac.OpenGL.All;
using GetPName = MonoMac.OpenGL.GetPName;
#else
using OpenTK.Graphics.OpenGL;
using GetParamName = OpenTK.Graphics.OpenGL.All;
using GetPName = OpenTK.Graphics.OpenGL.GetPName;
#endif
#elif GLES
using OpenTK.Graphics.ES20;
using GetParamName = OpenTK.Graphics.ES20.All;
using GetPName = OpenTK.Graphics.ES20.GetPName;
#else
using OpenGL;
using GetParamName = OpenGL.GetPName;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsCapabilities
    {
#if OPENGL
        /// <summary>
        /// True, if GL_ARB_framebuffer_object is supported; false otherwise.
        /// </summary>
        internal bool SupportsFramebufferObjectARB { get; private set; }

        /// <summary>
        /// True, if GL_EXT_framebuffer_object is supported; false otherwise.
        /// </summary>
        internal bool SupportsFramebufferObjectEXT { get; private set; }

        /// <summary>
        /// Gets the max texture anisotropy. This value typically lies
        /// between 0 and 16, where 0 means anisotropic filtering is not
        /// supported.
        /// </summary>
        internal int MaxTextureAnisotropy { get; private set; }
#endif

        internal void PlatformInitialize(GraphicsDevice device)
        {
			SupportsNonPowerOfTwo = GetNonPowerOfTwo(device);

#if OPENGL
            SupportsTextureFilterAnisotropic = device._extensions.Contains("GL_EXT_texture_filter_anisotropic");
#else
            SupportsTextureFilterAnisotropic = true;
#endif
#if GLES
			SupportsDepth24 = device._extensions.Contains("GL_OES_depth24");
			SupportsPackedDepthStencil = device._extensions.Contains("GL_OES_packed_depth_stencil");
			SupportsDepthNonLinear = device._extensions.Contains("GL_NV_depth_nonlinear");
            SupportsTextureMaxLevel = device._extensions.Contains("GL_APPLE_texture_max_level");
#else
            SupportsDepth24 = true;
			SupportsPackedDepthStencil = true;
			SupportsDepthNonLinear = false;
            SupportsTextureMaxLevel = true;
#endif

            // Texture compression
#if DIRECTX
            SupportsDxt1 = true;
            SupportsS3tc = true;
#elif OPENGL
            SupportsS3tc = device._extensions.Contains("GL_EXT_texture_compression_s3tc") ||
                device._extensions.Contains("GL_OES_texture_compression_S3TC") ||
                device._extensions.Contains("GL_EXT_texture_compression_dxt3") ||
                device._extensions.Contains("GL_EXT_texture_compression_dxt5");
            SupportsDxt1 = SupportsS3tc || device._extensions.Contains("GL_EXT_texture_compression_dxt1");
            SupportsPvrtc = device._extensions.Contains("GL_IMG_texture_compression_pvrtc");
            SupportsEtc1 = device._extensions.Contains("GL_OES_compressed_ETC1_RGB8_texture");
            SupportsAtitc = device._extensions.Contains("GL_ATI_texture_compression_atitc") ||
                device._extensions.Contains("GL_AMD_compressed_ATC_texture");
#endif

            // OpenGL framebuffer objects
#if OPENGL
#if GLES
            SupportsFramebufferObjectARB = true; // always supported on GLES 2.0+
            SupportsFramebufferObjectEXT = false;
#else
            // if we're on GL 3.0+, frame buffer extensions are guaranteed to be present, but extensions may be missing
            // it is then safe to assume that GL_ARB_framebuffer_object is present so that the standard function are loaded
            SupportsFramebufferObjectARB = device.glMajorVersion >= 3 || device._extensions.Contains("GL_ARB_framebuffer_object");
            SupportsFramebufferObjectEXT = device._extensions.Contains("GL_EXT_framebuffer_object");
#endif
#endif

            // Anisotropic filtering
#if OPENGL
            int anisotropy = 0;
            if (SupportsTextureFilterAnisotropic)
            {
#if __IOS__
                GL.GetInteger ((GetPName)All.MaxTextureMaxAnisotropyExt, out anisotropy);
#else
                GL.GetInteger((GetPName)GetParamName.MaxTextureMaxAnisotropyExt, out anisotropy);
#endif
                GraphicsExtensions.CheckGLError();
            }
            MaxTextureAnisotropy = anisotropy;
#endif

            // sRGB
#if DIRECTX
            SupportsSRgb = true;
#elif OPENGL
#if GLES
            SupportsSRgb = device._extensions.Contains("GL_EXT_sRGB");
#else
            SupportsSRgb = device._extensions.Contains("GL_EXT_texture_sRGB") && device._extensions.Contains("GL_EXT_framebuffer_sRGB");
#endif
#endif

#if DIRECTX
            SupportsTextureArrays = device.GraphicsProfile == GraphicsProfile.HiDef;
#elif OPENGL
            // TODO: Implement OpenGL support for texture arrays
            // once we can author shaders that use texture arrays.
            SupportsTextureArrays = false;
#endif

#if DIRECTX
            SupportsDepthClamp = device.GraphicsProfile == GraphicsProfile.HiDef;
#elif OPENGL
            SupportsDepthClamp = device._extensions.Contains("GL_ARB_depth_clamp");
#endif

#if DIRECTX
            SupportsVertexTextures = device.GraphicsProfile == GraphicsProfile.HiDef;
#elif OPENGL
            SupportsVertexTextures = false; // For now, until we implement vertex textures in OpenGL.
#endif
        }

        bool GetNonPowerOfTwo(GraphicsDevice device)
        {
#if OPENGL
#if GLES
            return device._extensions.Contains("GL_OES_texture_npot") ||
                   device._extensions.Contains("GL_ARB_texture_non_power_of_two") ||
                   device._extensions.Contains("GL_IMG_texture_npot") ||
                   device._extensions.Contains("GL_NV_texture_npot_2D_mipmap");
#else
            // Unfortunately non PoT texture support is patchy even on desktop systems and we can't
            // rely on the fact that GL2.0+ supposedly supports npot in the core.
            // Reference: http://aras-p.info/blog/2012/10/17/non-power-of-two-textures/
            return device._maxTextureSize >= 8192;
#endif

#else
            return device.GraphicsProfile == GraphicsProfile.HiDef;
#endif
        }
    }
}