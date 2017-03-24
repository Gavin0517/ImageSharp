﻿// <copyright file="IccDataReader.NonPrimitives.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

namespace ImageSharp
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Provides methods to read ICC data types
    /// </summary>
    internal sealed partial class IccDataReader
    {
        /// <summary>
        /// Reads a DateTime
        /// </summary>
        /// <returns>the value</returns>
        public DateTime ReadDateTime()
        {
            try
            {
                return new DateTime(
                    year: this.ReadUInt16(),
                    month: this.ReadUInt16(),
                    day: this.ReadUInt16(),
                    hour: this.ReadUInt16(),
                    minute: this.ReadUInt16(),
                    second: this.ReadUInt16(),
                    kind: DateTimeKind.Utc);
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Reads an ICC profile version number
        /// </summary>
        /// <returns>the version number</returns>
        public Version ReadVersionNumber()
        {
            int version = this.ReadInt32();

            int major = (version >> 24) & 0xFF;
            int minor = (version >> 20) & 0x0F;
            int bugfix = (version >> 16) & 0x0F;

            return new Version(major, minor, bugfix);
        }

        /// <summary>
        /// Reads an XYZ number
        /// </summary>
        /// <returns>the XYZ number</returns>
        public Vector3 ReadXyzNumber()
        {
            return new Vector3(
                x: this.ReadFix16(),
                y: this.ReadFix16(),
                z: this.ReadFix16());
        }

        /// <summary>
        /// Reads a profile ID
        /// </summary>
        /// <returns>the profile ID</returns>
        public IccProfileId ReadProfileId()
        {
            return new IccProfileId(
                p1: this.ReadUInt32(),
                p2: this.ReadUInt32(),
                p3: this.ReadUInt32(),
                p4: this.ReadUInt32());
        }

        /// <summary>
        /// Reads a position number
        /// </summary>
        /// <returns>the position number</returns>
        public IccPositionNumber ReadPositionNumber()
        {
            return new IccPositionNumber(
                offset: this.ReadUInt32(),
                size: this.ReadUInt32());
        }

        /// <summary>
        /// Reads a response number
        /// </summary>
        /// <returns>the response number</returns>
        public IccResponseNumber ReadResponseNumber()
        {
            return new IccResponseNumber(
                deviceCode: this.ReadUInt16(),
                measurementValue: this.ReadFix16());
        }

        /// <summary>
        /// Reads a named color
        /// </summary>
        /// <param name="deviceCoordCount">Number of device coordinates</param>
        /// <returns>the named color</returns>
        public IccNamedColor ReadNamedColor(uint deviceCoordCount)
        {
            string name = this.ReadAsciiString(32);
            ushort[] pcsCoord = new ushort[3] { this.ReadUInt16(), this.ReadUInt16(), this.ReadUInt16() };
            ushort[] deviceCoord = new ushort[deviceCoordCount];

            for (int i = 0; i < deviceCoordCount; i++)
            {
                deviceCoord[i] = this.ReadUInt16();
            }

            return new IccNamedColor(name, pcsCoord, deviceCoord);
        }

        /// <summary>
        /// Reads a profile description
        /// </summary>
        /// <returns>the profile description</returns>
        public IccProfileDescription ReadProfileDescription()
        {
            uint manufacturer = this.ReadUInt32();
            uint model = this.ReadUInt32();
            IccDeviceAttribute attributes = (IccDeviceAttribute)this.ReadInt64();
            IccProfileTag technologyInfo = (IccProfileTag)this.ReadUInt32();
            this.ReadCheckTagDataEntryHeader(IccTypeSignature.MultiLocalizedUnicode);
            IccMultiLocalizedUnicodeTagDataEntry manufacturerInfo = this.ReadMultiLocalizedUnicodeTagDataEntry();
            this.ReadCheckTagDataEntryHeader(IccTypeSignature.MultiLocalizedUnicode);
            IccMultiLocalizedUnicodeTagDataEntry modelInfo = this.ReadMultiLocalizedUnicodeTagDataEntry();

            return new IccProfileDescription(
                manufacturer,
                model,
                attributes,
                technologyInfo,
                manufacturerInfo.Texts,
                modelInfo.Texts);
        }

        /// <summary>
        /// Reads a colorant table entry
        /// </summary>
        /// <returns>the profile description</returns>
        public IccColorantTableEntry ReadColorantTableEntry()
        {
            return new IccColorantTableEntry(
                name: this.ReadAsciiString(32),
                pcs1: this.ReadUInt16(),
                pcs2: this.ReadUInt16(),
                pcs3: this.ReadUInt16());
        }
    }
}
