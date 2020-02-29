// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Processing.Processors.Quantization
{
    /// <summary>
    /// Represents a quantized image frame where the pixels indexed by a color palette.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    public sealed class QuantizedFrame<TPixel> : IDisposable
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private IMemoryOwner<TPixel> palette;
        private IMemoryOwner<byte> pixels;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantizedFrame{TPixel}"/> class.
        /// </summary>
        /// <param name="memoryAllocator">Used to allocated memory for image processing operations.</param>
        /// <param name="width">The image width.</param>
        /// <param name="height">The image height.</param>
        /// <param name="palette">The color palette.</param>
        internal QuantizedFrame(MemoryAllocator memoryAllocator, int width, int height, ReadOnlySpan<TPixel> palette)
        {
            Guard.MustBeGreaterThan(width, 0, nameof(width));
            Guard.MustBeGreaterThan(height, 0, nameof(height));

            this.Width = width;
            this.Height = height;
            this.pixels = memoryAllocator.AllocateManagedByteBuffer(width * height, AllocationOptions.Clean);

            this.palette = memoryAllocator.Allocate<TPixel>(palette.Length);
            palette.CopyTo(this.palette.GetSpan());
        }

        /// <summary>
        /// Gets the width of this <see cref="QuantizedFrame{TPixel}"/>.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of this <see cref="QuantizedFrame{TPixel}"/>.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the color palette of this <see cref="QuantizedFrame{TPixel}"/>.
        /// </summary>
        public ReadOnlySpan<TPixel> Palette
        {
            [MethodImpl(InliningOptions.ShortMethod)]
            get { return this.palette.GetSpan(); }
        }

        /// <summary>
        /// Gets the pixels of this <see cref="QuantizedFrame{TPixel}"/>.
        /// </summary>
        /// <returns>The <see cref="Span{T}"/></returns>
        [MethodImpl(InliningOptions.ShortMethod)]
        public Span<byte> GetPixelSpan() => this.pixels.GetSpan();

        /// <summary>
        /// Gets the representation of the pixels as a <see cref="Span{T}"/> of contiguous memory
        /// at row <paramref name="rowIndex"/> beginning from the the first pixel on that row.
        /// </summary>
        /// <param name="rowIndex">The row.</param>
        /// <returns>The pixel row as a <see cref="Span{T}"/>.</returns>
        [MethodImpl(InliningOptions.ShortMethod)]
        public Span<byte> GetPixelRowSpan(int rowIndex)
            => this.GetPixelSpan().Slice(rowIndex * this.Width, this.Width);

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            this.pixels?.Dispose();
            this.palette?.Dispose();
            this.pixels = null;
            this.palette = null;
        }
    }
}
