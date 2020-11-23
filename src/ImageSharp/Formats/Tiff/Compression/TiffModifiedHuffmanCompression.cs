// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using SixLabors.ImageSharp.Memory;

namespace SixLabors.ImageSharp.Formats.Tiff.Compression
{
    /// <summary>
    /// Class to handle cases where TIFF image data is compressed using Modified Huffman Compression.
    /// </summary>
    internal class TiffModifiedHuffmanCompression : T4TiffCompression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TiffModifiedHuffmanCompression" /> class.
        /// </summary>
        /// <param name="allocator">The memory allocator.</param>
        /// <param name="photometricInterpretation">The photometric interpretation.</param>
        /// <param name="width">The image width.</param>
        public TiffModifiedHuffmanCompression(MemoryAllocator allocator, TiffPhotometricInterpretation photometricInterpretation, int width)
            : base(allocator, photometricInterpretation, width)
        {
        }

        /// <inheritdoc/>
        public override void Decompress(Stream stream, int byteCount, Span<byte> buffer)
        {
            bool isWhiteZero = this.PhotometricInterpretation == TiffPhotometricInterpretation.WhiteIsZero;
            int whiteValue = isWhiteZero ? 0 : 1;
            int blackValue = isWhiteZero ? 1 : 0;

            using var bitReader = new T4BitReader(stream, byteCount, this.Allocator, isModifiedHuffman: true);

            buffer.Clear();
            uint bitsWritten = 0;
            uint pixelsWritten = 0;
            while (bitReader.HasMoreData)
            {
                bitReader.ReadNextRun();

                if (bitReader.RunLength > 0)
                {
                    if (bitReader.IsWhiteRun)
                    {
                        this.WriteBits(buffer, (int)bitsWritten, bitReader.RunLength, whiteValue);
                        bitsWritten += bitReader.RunLength;
                        pixelsWritten += bitReader.RunLength;
                    }
                    else
                    {
                        this.WriteBits(buffer, (int)bitsWritten, bitReader.RunLength, blackValue);
                        bitsWritten += bitReader.RunLength;
                        pixelsWritten += bitReader.RunLength;
                    }
                }

                if (pixelsWritten % this.Width == 0)
                {
                    bitReader.StartNewRow();

                    // Write padding bytes, if necessary.
                    uint pad = 8 - (bitsWritten % 8);
                    if (pad != 8)
                    {
                        this.WriteBits(buffer, (int)bitsWritten, pad, 0);
                        bitsWritten += pad;
                    }
                }
            }
        }
    }
}
