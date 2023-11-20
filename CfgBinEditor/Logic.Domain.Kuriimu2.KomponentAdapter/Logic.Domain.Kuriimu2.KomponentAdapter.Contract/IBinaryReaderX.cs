﻿using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract.DataClasses;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Kuriimu2.KomponentAdapter.Contract
{
    [MapException(typeof(BinaryReaderXException))]
    public interface IBinaryReaderX : IDisposable
    {
        Stream BaseStream { get; }

        BitOrder BitOrder { get; set; }
        BitOrder EffectiveBitOrder { get; }
        ByteOrder ByteOrder { get; set; }

        int BlockSize { get; set; }

        void SeekAlignment(int alignment = 0x10);

        bool ReadBoolean();
        byte ReadByte();
        sbyte ReadSByte();
        char ReadChar();
        short ReadInt16();
        ushort ReadUInt16();
        int ReadInt32();
        uint ReadUInt32();
        long ReadInt64();
        ulong ReadUInt64();
        float ReadSingle();
        double ReadDouble();
        decimal ReadDecimal();

        string ReadCStringSJIS();

        string ReadString();
        string ReadString(int length);
        string ReadString(int length, Encoding encoding);

        long ReadBits(int count);
        void ResetBitBuffer();
    }
}
