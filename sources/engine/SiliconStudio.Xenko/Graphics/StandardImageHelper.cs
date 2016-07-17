﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// This class is responsible to provide image loader for png, gif, bmp.
    /// </summary>
    partial class StandardImageHelper
    {
        static unsafe void CopyMemoryBGRA(IntPtr dest, IntPtr src, int sizeInBytesToCopy)
        {
            if (sizeInBytesToCopy % 4 != 0)
                throw new ArgumentException("Should be a multiple of 4.", "sizeInBytesToCopy");

            var bufferSize = sizeInBytesToCopy / 4;
            var srcPtr = (uint*)src;
            var destPtr = (uint*)dest;
            for (int i = 0; i < bufferSize; ++i)
            {
                var value = *srcPtr++;
                // BGRA => RGBA
                value = (value & 0xFF000000) | ((value & 0xFF0000) >> 16) | (value & 0x00FF00) | ((value & 0x0000FF) << 16);
                *destPtr++ = value;
            }
        }

        static unsafe void CopyMemoryRRR1(IntPtr dest, IntPtr src, int sizeInBytesToCopy)
        {
            var bufferSize = sizeInBytesToCopy;
            var srcPtr = (byte*)src;
            var destPtr = (uint*)dest;
            for (int i = 0; i < bufferSize; ++i)
            {
                uint value = *srcPtr++;
                // R => RGBA
                value = (0xFF000000) | ((value) << 8) | (value) | ((value) << 16);
                *destPtr++ = value;
            }
        }
    }
}
