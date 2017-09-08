using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PacketIO
{
    public class PacketWriter : BinaryWriter
    {
        private MemoryStream ms;

        public PacketWriter() : base()
        {
            ms = new MemoryStream();
            OutStream = ms;
        }

        public PacketWriter(EOpCodes opcode) : base()
        {
            ms = new MemoryStream();
            OutStream = ms;
            Write((ushort)opcode);
        }

        public byte[] GetBytes()
        {
            Close();

            byte[] data = ms.ToArray();

            return data;
        }
    }

    public class PacketReader : BinaryReader
    {
        public EOpCodes header;

        public PacketReader(byte[] data) : base(new MemoryStream(data))
        {
            header = (EOpCodes)ReadUInt16();
        }

    }

    public interface IPacket
    {
        void Write();
    }
}


