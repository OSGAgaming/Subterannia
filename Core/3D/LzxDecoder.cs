using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using SkinnedModel;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Reflection;
using System.Diagnostics;
using Subterannia.Core.Utility;
using System.Runtime.InteropServices;

namespace Subterannia.Core.Mechanics
{


    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct LzxConstants
    {
        public enum BLOCKTYPE
        {
            INVALID,
            VERBATIM,
            ALIGNED,
            UNCOMPRESSED
        }

        public const ushort MIN_MATCH = 2;

        public const ushort MAX_MATCH = 257;

        public const ushort NUM_CHARS = 256;

        public const ushort PRETREE_NUM_ELEMENTS = 20;

        public const ushort ALIGNED_NUM_ELEMENTS = 8;

        public const ushort NUM_PRIMARY_LENGTHS = 7;

        public const ushort NUM_SECONDARY_LENGTHS = 249;

        public const ushort PRETREE_MAXSYMBOLS = 20;

        public const ushort PRETREE_TABLEBITS = 6;

        public const ushort MAINTREE_MAXSYMBOLS = 656;

        public const ushort MAINTREE_TABLEBITS = 12;

        public const ushort LENGTH_MAXSYMBOLS = 250;

        public const ushort LENGTH_TABLEBITS = 12;

        public const ushort ALIGNED_MAXSYMBOLS = 8;

        public const ushort ALIGNED_TABLEBITS = 7;

        public const ushort LENTABLE_SAFETY = 64;
    }

    internal class LzxDecoder
    {
        private class BitBuffer
        {
            private uint buffer;

            private byte bitsleft;

            private Stream byteStream;

            public BitBuffer(Stream stream)
            {
                byteStream = stream;
                InitBitStream();
            }

            public void InitBitStream()
            {
                buffer = 0u;
                bitsleft = 0;
            }

            public void EnsureBits(byte bits)
            {
                while (bitsleft < bits)
                {
                    int lo = (byte)byteStream.ReadByte();
                    int hi = (byte)byteStream.ReadByte();
                    buffer |= (uint)(((hi << 8) | lo) << 16 - bitsleft);
                    bitsleft += 16;
                }
            }

            public uint PeekBits(byte bits)
            {
                return buffer >> 32 - bits;
            }

            public void RemoveBits(byte bits)
            {
                buffer <<= (int)bits;
                bitsleft -= bits;
            }

            public uint ReadBits(byte bits)
            {
                uint ret = 0u;
                if (bits > 0)
                {
                    EnsureBits(bits);
                    ret = PeekBits(bits);
                    RemoveBits(bits);
                }
                return ret;
            }

            public uint GetBuffer()
            {
                return buffer;
            }

            public byte GetBitsLeft()
            {
                return bitsleft;
            }
        }

        private struct LzxState
        {
            public uint R0;

            public uint R1;

            public uint R2;

            public ushort main_elements;

            public int header_read;

            public LzxConstants.BLOCKTYPE block_type;

            public uint block_length;

            public uint block_remaining;

            public uint frames_read;

            public int intel_filesize;

            public int intel_curpos;

            public int intel_started;

            public ushort[] PRETREE_table;

            public byte[] PRETREE_len;

            public ushort[] MAINTREE_table;

            public byte[] MAINTREE_len;

            public ushort[] LENGTH_table;

            public byte[] LENGTH_len;

            public ushort[] ALIGNED_table;

            public byte[] ALIGNED_len;

            public uint actual_size;

            public byte[] window;

            public uint window_size;

            public uint window_posn;
        }

        public static uint[] position_base;

        public static byte[] extra_bits;

        private LzxState m_state;

        public LzxDecoder(int window)
        {
            uint wndsize = (uint)(1 << window);
            if (window < 15 || window > 21)
            {
                //throw new UnsupportedWindowSizeRange();
            }
            m_state = default(LzxState);
            m_state.actual_size = 0u;
            m_state.window = new byte[wndsize];
            for (int m = 0; m < wndsize; m++)
            {
                m_state.window[m] = 220;
            }
            m_state.actual_size = wndsize;
            m_state.window_size = wndsize;
            m_state.window_posn = 0u;
            if (extra_bits == null)
            {
                extra_bits = new byte[52];
                int l = 0;
                int j2 = 0;
                for (; l <= 50; l += 2)
                {
                    byte[] array = extra_bits;
                    int num = l;
                    byte b;
                    extra_bits[l + 1] = (b = (byte)j2);
                    array[num] = b;
                    if (l != 0 && j2 < 17)
                    {
                        j2++;
                    }
                }
            }
            if (position_base == null)
            {
                position_base = new uint[51];
                int k = 0;
                int n = 0;
                for (; k <= 50; k++)
                {
                    position_base[k] = (uint)n;
                    n += 1 << (int)extra_bits[k];
                }
            }
            int posn_slots = window switch
            {
                20 => 42,
                21 => 50,
                _ => window << 1,
            };
            m_state.R0 = (m_state.R1 = (m_state.R2 = 1u));
            m_state.main_elements = (ushort)(256 + (posn_slots << 3));
            m_state.header_read = 0;
            m_state.frames_read = 0u;
            m_state.block_remaining = 0u;
            m_state.block_type = LzxConstants.BLOCKTYPE.INVALID;
            m_state.intel_curpos = 0;
            m_state.intel_started = 0;
            m_state.PRETREE_table = new ushort[104];
            m_state.PRETREE_len = new byte[84];
            m_state.MAINTREE_table = new ushort[5408];
            m_state.MAINTREE_len = new byte[720];
            m_state.LENGTH_table = new ushort[4596];
            m_state.LENGTH_len = new byte[314];
            m_state.ALIGNED_table = new ushort[144];
            m_state.ALIGNED_len = new byte[72];
            for (int j = 0; j < 656; j++)
            {
                m_state.MAINTREE_len[j] = 0;
            }
            for (int i = 0; i < 250; i++)
            {
                m_state.LENGTH_len[i] = 0;
            }
        }

        public int Decompress(Stream inData, int inLen, Stream outData, int outLen)
        {
            BitBuffer bitbuf = new BitBuffer(inData);
            long startpos = inData.Position;
            long endpos = inData.Position + inLen;
            byte[] window = m_state.window;
            uint window_posn = m_state.window_posn;
            uint window_size = m_state.window_size;
            uint R0 = m_state.R0;
            uint R1 = m_state.R1;
            uint R2 = m_state.R2;
            int togo = outLen;
            bitbuf.InitBitStream();
            if (m_state.header_read == 0)
            {
                if (bitbuf.ReadBits(1) != 0)
                {
                    uint i = bitbuf.ReadBits(16);
                    uint j = bitbuf.ReadBits(16);
                    m_state.intel_filesize = (int)((i << 16) | j);
                }
                m_state.header_read = 1;
            }
            while (togo > 0)
            {
                if (m_state.block_remaining == 0)
                {
                    if (m_state.block_type == LzxConstants.BLOCKTYPE.UNCOMPRESSED)
                    {
                        if ((m_state.block_length & 1) == 1)
                        {
                            inData.ReadByte();
                        }
                        bitbuf.InitBitStream();
                    }
                    m_state.block_type = (LzxConstants.BLOCKTYPE)bitbuf.ReadBits(3);
                    uint i = bitbuf.ReadBits(16);
                    uint j = bitbuf.ReadBits(8);
                    m_state.block_remaining = (m_state.block_length = (i << 8) | j);
                    switch (m_state.block_type)
                    {
                        case LzxConstants.BLOCKTYPE.ALIGNED:
                            i = 0u;
                            j = 0u;
                            for (; i < 8; i++)
                            {
                                j = bitbuf.ReadBits(3);
                                m_state.ALIGNED_len[i] = (byte)j;
                            }
                            MakeDecodeTable(8u, 7u, m_state.ALIGNED_len, m_state.ALIGNED_table);
                            goto case LzxConstants.BLOCKTYPE.VERBATIM;
                        case LzxConstants.BLOCKTYPE.VERBATIM:
                            ReadLengths(m_state.MAINTREE_len, 0u, 256u, bitbuf);
                            ReadLengths(m_state.MAINTREE_len, 256u, m_state.main_elements, bitbuf);
                            MakeDecodeTable(656u, 12u, m_state.MAINTREE_len, m_state.MAINTREE_table);
                            if (m_state.MAINTREE_len[232] != 0)
                            {
                                m_state.intel_started = 1;
                            }
                            ReadLengths(m_state.LENGTH_len, 0u, 249u, bitbuf);
                            MakeDecodeTable(250u, 12u, m_state.LENGTH_len, m_state.LENGTH_table);
                            break;
                        case LzxConstants.BLOCKTYPE.UNCOMPRESSED:
                            {
                                m_state.intel_started = 1;
                                bitbuf.EnsureBits(16);
                                if (bitbuf.GetBitsLeft() > 16)
                                {
                                    inData.Seek(-2L, SeekOrigin.Current);
                                }
                                byte num = (byte)inData.ReadByte();
                                byte ml = (byte)inData.ReadByte();
                                byte mh = (byte)inData.ReadByte();
                                byte hi = (byte)inData.ReadByte();
                                R0 = (uint)(num | (ml << 8) | (mh << 16) | (hi << 24));
                                byte num2 = (byte)inData.ReadByte();
                                ml = (byte)inData.ReadByte();
                                mh = (byte)inData.ReadByte();
                                hi = (byte)inData.ReadByte();
                                R1 = (uint)(num2 | (ml << 8) | (mh << 16) | (hi << 24));
                                byte num3 = (byte)inData.ReadByte();
                                ml = (byte)inData.ReadByte();
                                mh = (byte)inData.ReadByte();
                                hi = (byte)inData.ReadByte();
                                R2 = (uint)(num3 | (ml << 8) | (mh << 16) | (hi << 24));
                                break;
                            }
                        default:
                            return -1;
                    }
                }
                if (inData.Position > startpos + inLen && (inData.Position > startpos + inLen + 2 || bitbuf.GetBitsLeft() < 16))
                {
                    return -1;
                }
                int this_run;
                while ((this_run = (int)m_state.block_remaining) > 0 && togo > 0)
                {
                    if (this_run > togo)
                    {
                        this_run = togo;
                    }
                    togo -= this_run;
                    m_state.block_remaining -= (uint)this_run;
                    window_posn &= window_size - 1;
                    if (window_posn + this_run > window_size)
                    {
                        return -1;
                    }
                    switch (m_state.block_type)
                    {
                        case LzxConstants.BLOCKTYPE.VERBATIM:
                            while (this_run > 0)
                            {
                                int main_element = (int)ReadHuffSym(m_state.MAINTREE_table, m_state.MAINTREE_len, 656u, 12u, bitbuf);
                                if (main_element < 256)
                                {
                                    window[window_posn++] = (byte)main_element;
                                    this_run--;
                                    continue;
                                }
                                main_element -= 256;
                                int match_length = main_element & 7;
                                if (match_length == 7)
                                {
                                    int length_footer = (int)ReadHuffSym(m_state.LENGTH_table, m_state.LENGTH_len, 250u, 12u, bitbuf);
                                    match_length += length_footer;
                                }
                                match_length += 2;
                                int match_offset = main_element >> 3;
                                if (match_offset > 2)
                                {
                                    if (match_offset != 3)
                                    {
                                        int extra = extra_bits[match_offset];
                                        int verbatim_bits = (int)bitbuf.ReadBits((byte)extra);
                                        match_offset = (int)(position_base[match_offset] - 2) + verbatim_bits;
                                    }
                                    else
                                    {
                                        match_offset = 1;
                                    }
                                    R2 = R1;
                                    R1 = R0;
                                    R0 = (uint)match_offset;
                                }
                                else
                                {
                                    switch (match_offset)
                                    {
                                        case 0:
                                            match_offset = (int)R0;
                                            break;
                                        case 1:
                                            match_offset = (int)R1;
                                            R1 = R0;
                                            R0 = (uint)match_offset;
                                            break;
                                        default:
                                            match_offset = (int)R2;
                                            R2 = R0;
                                            R0 = (uint)match_offset;
                                            break;
                                    }
                                }
                                int rundest = (int)window_posn;
                                this_run -= match_length;
                                int runsrc;
                                if (window_posn >= match_offset)
                                {
                                    runsrc = rundest - match_offset;
                                }
                                else
                                {
                                    runsrc = rundest + ((int)window_size - match_offset);
                                    int copy_length = match_offset - (int)window_posn;
                                    if (copy_length < match_length)
                                    {
                                        match_length -= copy_length;
                                        window_posn += (uint)copy_length;
                                        while (copy_length-- > 0)
                                        {
                                            window[rundest++] = window[runsrc++];
                                        }
                                        runsrc = 0;
                                    }
                                }
                                window_posn += (uint)match_length;
                                while (match_length-- > 0)
                                {
                                    window[rundest++] = window[runsrc++];
                                }
                            }
                            break;
                        case LzxConstants.BLOCKTYPE.ALIGNED:
                            while (this_run > 0)
                            {
                                int main_element = (int)ReadHuffSym(m_state.MAINTREE_table, m_state.MAINTREE_len, 656u, 12u, bitbuf);
                                if (main_element < 256)
                                {
                                    window[window_posn++] = (byte)main_element;
                                    this_run--;
                                    continue;
                                }
                                main_element -= 256;
                                int match_length = main_element & 7;
                                if (match_length == 7)
                                {
                                    int length_footer = (int)ReadHuffSym(m_state.LENGTH_table, m_state.LENGTH_len, 250u, 12u, bitbuf);
                                    match_length += length_footer;
                                }
                                match_length += 2;
                                int match_offset = main_element >> 3;
                                if (match_offset > 2)
                                {
                                    int extra = extra_bits[match_offset];
                                    match_offset = (int)(position_base[match_offset] - 2);
                                    if (extra > 3)
                                    {
                                        extra -= 3;
                                        int verbatim_bits = (int)bitbuf.ReadBits((byte)extra);
                                        match_offset += verbatim_bits << 3;
                                        int aligned_bits = (int)ReadHuffSym(m_state.ALIGNED_table, m_state.ALIGNED_len, 8u, 7u, bitbuf);
                                        match_offset += aligned_bits;
                                    }
                                    else if (extra == 3)
                                    {
                                        int aligned_bits = (int)ReadHuffSym(m_state.ALIGNED_table, m_state.ALIGNED_len, 8u, 7u, bitbuf);
                                        match_offset += aligned_bits;
                                    }
                                    else if (extra > 0)
                                    {
                                        int verbatim_bits = (int)bitbuf.ReadBits((byte)extra);
                                        match_offset += verbatim_bits;
                                    }
                                    else
                                    {
                                        match_offset = 1;
                                    }
                                    R2 = R1;
                                    R1 = R0;
                                    R0 = (uint)match_offset;
                                }
                                else
                                {
                                    switch (match_offset)
                                    {
                                        case 0:
                                            match_offset = (int)R0;
                                            break;
                                        case 1:
                                            match_offset = (int)R1;
                                            R1 = R0;
                                            R0 = (uint)match_offset;
                                            break;
                                        default:
                                            match_offset = (int)R2;
                                            R2 = R0;
                                            R0 = (uint)match_offset;
                                            break;
                                    }
                                }
                                int rundest = (int)window_posn;
                                this_run -= match_length;
                                int runsrc;
                                if (window_posn >= match_offset)
                                {
                                    runsrc = rundest - match_offset;
                                }
                                else
                                {
                                    runsrc = rundest + ((int)window_size - match_offset);
                                    int copy_length = match_offset - (int)window_posn;
                                    if (copy_length < match_length)
                                    {
                                        match_length -= copy_length;
                                        window_posn += (uint)copy_length;
                                        while (copy_length-- > 0)
                                        {
                                            window[rundest++] = window[runsrc++];
                                        }
                                        runsrc = 0;
                                    }
                                }
                                window_posn += (uint)match_length;
                                while (match_length-- > 0)
                                {
                                    window[rundest++] = window[runsrc++];
                                }
                            }
                            break;
                        case LzxConstants.BLOCKTYPE.UNCOMPRESSED:
                            {
                                if (inData.Position + this_run > endpos)
                                {
                                    return -1;
                                }
                                byte[] temp_buffer = new byte[this_run];
                                inData.Read(temp_buffer, 0, this_run);
                                temp_buffer.CopyTo(window, (int)window_posn);
                                window_posn += (uint)this_run;
                                break;
                            }
                        default:
                            return -1;
                    }
                }
            }
            if (togo != 0)
            {
                return -1;
            }
            int start_window_pos = (int)window_posn;
            if (start_window_pos == 0)
            {
                start_window_pos = (int)window_size;
            }
            start_window_pos -= outLen;
            outData.Write(window, start_window_pos, outLen);
            m_state.window_posn = window_posn;
            m_state.R0 = R0;
            m_state.R1 = R1;
            m_state.R2 = R2;
            if (m_state.frames_read++ < 32768 && m_state.intel_filesize != 0)
            {
                if (outLen <= 6 || m_state.intel_started == 0)
                {
                    m_state.intel_curpos += outLen;
                }
                else
                {
                    int dataend = outLen - 10;
                    uint curpos = (uint)m_state.intel_curpos;
                    m_state.intel_curpos = (int)curpos + outLen;
                    while (outData.Position < dataend)
                    {
                        if (outData.ReadByte() != 232)
                        {
                            curpos++;
                        }
                    }
                }
                return -1;
            }
            return 0;
        }

        private int MakeDecodeTable(uint nsyms, uint nbits, byte[] length, ushort[] table)
        {
            byte bit_num = 1;
            uint pos = 0u;
            uint table_mask = (uint)(1 << (int)nbits);
            uint bit_mask = table_mask >> 1;
            uint next_symbol = bit_mask;
            while (bit_num <= nbits)
            {
                for (ushort sym = 0; sym < nsyms; sym = (ushort)(sym + 1))
                {
                    if (length[sym] == bit_num)
                    {
                        uint leaf = pos;
                        if ((pos += bit_mask) > table_mask)
                        {
                            return 1;
                        }
                        uint fill = bit_mask;
                        while (fill-- != 0)
                        {
                            table[leaf++] = sym;
                        }
                    }
                }
                bit_mask >>= 1;
                bit_num = (byte)(bit_num + 1);
            }
            if (pos != table_mask)
            {
                for (ushort sym = (ushort)pos; sym < table_mask; sym = (ushort)(sym + 1))
                {
                    table[sym] = 0;
                }
                pos <<= 16;
                table_mask <<= 16;
                bit_mask = 32768u;
                while (bit_num <= 16)
                {
                    for (ushort sym = 0; sym < nsyms; sym = (ushort)(sym + 1))
                    {
                        if (length[sym] == bit_num)
                        {
                            uint leaf = pos >> 16;
                            for (uint fill = 0u; fill < bit_num - nbits; fill++)
                            {
                                if (table[leaf] == 0)
                                {
                                    table[next_symbol << 1] = 0;
                                    table[(next_symbol << 1) + 1] = 0;
                                    table[leaf] = (ushort)next_symbol++;
                                }
                                leaf = (uint)(table[leaf] << 1);
                                if (((pos >> (int)(15 - fill)) & 1) == 1)
                                {
                                    leaf++;
                                }
                            }
                            table[leaf] = sym;
                            if ((pos += bit_mask) > table_mask)
                            {
                                return 1;
                            }
                        }
                    }
                    bit_mask >>= 1;
                    bit_num = (byte)(bit_num + 1);
                }
            }
            if (pos == table_mask)
            {
                return 0;
            }
            for (ushort sym = 0; sym < nsyms; sym = (ushort)(sym + 1))
            {
                if (length[sym] != 0)
                {
                    return 1;
                }
            }
            return 0;
        }

        private void ReadLengths(byte[] lens, uint first, uint last, BitBuffer bitbuf)
        {
            uint x;
            for (x = 0u; x < 20; x++)
            {
                uint y = bitbuf.ReadBits(4);
                m_state.PRETREE_len[x] = (byte)y;
            }
            MakeDecodeTable(20u, 6u, m_state.PRETREE_len, m_state.PRETREE_table);
            x = first;
            while (x < last)
            {
                int z = (int)ReadHuffSym(m_state.PRETREE_table, m_state.PRETREE_len, 20u, 6u, bitbuf);
                switch (z)
                {
                    case 17:
                        {
                            uint y = bitbuf.ReadBits(4);
                            y += 4;
                            while (y-- != 0)
                            {
                                lens[x++] = 0;
                            }
                            break;
                        }
                    case 18:
                        {
                            uint y = bitbuf.ReadBits(5);
                            y += 20;
                            while (y-- != 0)
                            {
                                lens[x++] = 0;
                            }
                            break;
                        }
                    case 19:
                        {
                            uint y = bitbuf.ReadBits(1);
                            y += 4;
                            z = (int)ReadHuffSym(m_state.PRETREE_table, m_state.PRETREE_len, 20u, 6u, bitbuf);
                            z = lens[x] - z;
                            if (z < 0)
                            {
                                z += 17;
                            }
                            while (y-- != 0)
                            {
                                lens[x++] = (byte)z;
                            }
                            break;
                        }
                    default:
                        z = lens[x] - z;
                        if (z < 0)
                        {
                            z += 17;
                        }
                        lens[x++] = (byte)z;
                        break;
                }
            }
        }

        private uint ReadHuffSym(ushort[] table, byte[] lengths, uint nsyms, uint nbits, BitBuffer bitbuf)
        {
            bitbuf.EnsureBits(16);
            uint i;
            uint j;
            if ((i = table[bitbuf.PeekBits((byte)nbits)]) >= nsyms)
            {
                j = (uint)(1 << (int)(32 - nbits));
                do
                {
                    j >>= 1;
                    i <<= 1;
                    i |= (((bitbuf.GetBuffer() & j) != 0) ? 1u : 0u);
                    if (j == 0)
                    {
                        return 0u;
                    }
                }
                while ((i = table[i]) >= nsyms);
            }
            j = lengths[i];
            bitbuf.RemoveBits((byte)j);
            return i;
        }
    }

    internal class Lz4DecoderStream : Stream
    {
        private enum DecodePhase
        {
            ReadToken,
            ReadExLiteralLength,
            CopyLiteral,
            ReadOffset,
            ReadExMatchLength,
            CopyMatch
        }

        private long inputLength;

        private Stream input;

        private const int DecBufLen = 65536;

        private const int DecBufMask = 65535;

        private const int InBufLen = 128;

        private byte[] decodeBuffer = new byte[65664];

        private int decodeBufferPos;

        private int inBufPos;

        private int inBufEnd;

        private DecodePhase phase;

        private int litLen;

        private int matLen;

        private int matDst;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public Lz4DecoderStream()
        {
        }

        public Lz4DecoderStream(Stream input, long inputLength = long.MaxValue)
        {
            Reset(input, inputLength);
        }

        public void Reset(Stream input, long inputLength = long.MaxValue)
        {
            this.inputLength = inputLength;
            this.input = input;
            phase = DecodePhase.ReadToken;
            decodeBufferPos = 0;
            litLen = 0;
            matLen = 0;
            matDst = 0;
            inBufPos = 65536;
            inBufEnd = 65536;
        }

        protected override void Dispose(bool disposing)
        {
            input = null;
            base.Dispose(disposing);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || count < 0 || buffer.Length - count < offset)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (input == null)
            {
                throw new InvalidOperationException();
            }
            int num = count;
            byte[] array = decodeBuffer;
            int num7;
            switch (phase)
            {
                default:
                    {
                        int num2;
                        if (inBufPos < inBufEnd)
                        {
                            num2 = array[inBufPos++];
                        }
                        else
                        {
                            num2 = ReadByteCore();
                            if (num2 == -1)
                            {
                                break;
                            }
                        }
                        litLen = num2 >> 4;
                        matLen = (num2 & 0xF) + 4;
                        int num3 = litLen;
                        if (num3 != 0)
                        {
                            if (num3 == 15)
                            {
                                phase = DecodePhase.ReadExLiteralLength;
                                goto case DecodePhase.ReadExLiteralLength;
                            }
                            phase = DecodePhase.CopyLiteral;
                            goto case DecodePhase.CopyLiteral;
                        }
                        phase = DecodePhase.ReadOffset;
                        goto case DecodePhase.ReadOffset;
                    }
                case DecodePhase.ReadExLiteralLength:
                    while (true)
                    {
                        int num14;
                        if (inBufPos < inBufEnd)
                        {
                            num14 = array[inBufPos++];
                        }
                        else
                        {
                            num14 = ReadByteCore();
                            if (num14 == -1)
                            {
                                break;
                            }
                        }
                        litLen += num14;
                        if (num14 == 255)
                        {
                            continue;
                        }
                        goto IL_012e;
                    }
                    break;
                case DecodePhase.CopyLiteral:
                    do
                    {
                        int num4 = ((litLen < num) ? litLen : num);
                        if (num4 == 0)
                        {
                            break;
                        }
                        if (inBufPos + num4 <= inBufEnd)
                        {
                            int num5 = offset;
                            int num6 = num4;
                            while (num6-- != 0)
                            {
                                buffer[num5++] = array[inBufPos++];
                            }
                            num7 = num4;
                        }
                        else
                        {
                            num7 = ReadCore(buffer, offset, num4);
                            if (num7 == 0)
                            {
                                goto end_IL_0045;
                            }
                        }
                        offset += num7;
                        num -= num7;
                        litLen -= num7;
                    }
                    while (litLen != 0);
                    if (num == 0)
                    {
                        break;
                    }
                    phase = DecodePhase.ReadOffset;
                    goto case DecodePhase.ReadOffset;
                case DecodePhase.ReadOffset:
                    if (inBufPos + 1 < inBufEnd)
                    {
                        matDst = (array[inBufPos + 1] << 8) | array[inBufPos];
                        inBufPos += 2;
                    }
                    else
                    {
                        matDst = ReadOffsetCore();
                        if (matDst == -1)
                        {
                            break;
                        }
                    }
                    if (matLen == 19)
                    {
                        phase = DecodePhase.ReadExMatchLength;
                        goto case DecodePhase.ReadExMatchLength;
                    }
                    phase = DecodePhase.CopyMatch;
                    goto case DecodePhase.CopyMatch;
                case DecodePhase.ReadExMatchLength:
                    while (true)
                    {
                        int num13;
                        if (inBufPos < inBufEnd)
                        {
                            num13 = array[inBufPos++];
                        }
                        else
                        {
                            num13 = ReadByteCore();
                            if (num13 == -1)
                            {
                                break;
                            }
                        }
                        matLen += num13;
                        if (num13 == 255)
                        {
                            continue;
                        }
                        goto IL_0293;
                    }
                    break;
                case DecodePhase.CopyMatch:
                    {
                        int num8 = ((matLen < num) ? matLen : num);
                        if (num8 != 0)
                        {
                            num7 = count - num;
                            int num9 = matDst - num7;
                            if (num9 > 0)
                            {
                                int num10 = decodeBufferPos - num9;
                                if (num10 < 0)
                                {
                                    num10 += 65536;
                                }
                                int num11 = ((num9 < num8) ? num9 : num8);
                                while (num11-- != 0)
                                {
                                    buffer[offset++] = array[num10++ & 0xFFFF];
                                }
                            }
                            else
                            {
                                num9 = 0;
                            }
                            int num12 = offset - matDst;
                            for (int i = num9; i < num8; i++)
                            {
                                buffer[offset++] = buffer[num12++];
                            }
                            num -= num8;
                            matLen -= num8;
                        }
                        if (num == 0)
                        {
                            break;
                        }
                        phase = DecodePhase.ReadToken;
                        goto default;
                    }
                IL_0293:
                    phase = DecodePhase.CopyMatch;
                    goto case DecodePhase.CopyMatch;
                IL_012e:
                    phase = DecodePhase.CopyLiteral;
                    goto case DecodePhase.CopyLiteral;
                end_IL_0045:
                    break;
            }
            num7 = count - num;
            int num15 = ((num7 < 65536) ? num7 : 65536);
            int srcOffset = offset - num15;
            if (num15 == 65536)
            {
                Buffer.BlockCopy(buffer, srcOffset, array, 0, 65536);
                decodeBufferPos = 0;
            }
            else
            {
                int num16 = decodeBufferPos;
                while (num15-- != 0)
                {
                    array[num16++ & 0xFFFF] = buffer[srcOffset++];
                }
                decodeBufferPos = num16 & 0xFFFF;
            }
            return num7;
        }

        private int ReadByteCore()
        {
            byte[] array = decodeBuffer;
            if (inBufPos == inBufEnd)
            {
                int num = input.Read(array, 65536, (int)((128 < inputLength) ? 128 : inputLength));
                if (num == 0)
                {
                    return -1;
                }
                inputLength -= num;
                inBufPos = 65536;
                inBufEnd = 65536 + num;
            }
            return array[inBufPos++];
        }

        private int ReadOffsetCore()
        {
            byte[] array = decodeBuffer;
            if (inBufPos == inBufEnd)
            {
                int num = input.Read(array, 65536, (int)((128 < inputLength) ? 128 : inputLength));
                if (num == 0)
                {
                    return -1;
                }
                inputLength -= num;
                inBufPos = 65536;
                inBufEnd = 65536 + num;
            }
            if (inBufEnd - inBufPos == 1)
            {
                array[65536] = array[inBufPos];
                int num2 = input.Read(array, 65537, (int)((127 < inputLength) ? 127 : inputLength));
                if (num2 == 0)
                {
                    inBufPos = 65536;
                    inBufEnd = 65537;
                    return -1;
                }
                inputLength -= num2;
                inBufPos = 65536;
                inBufEnd = 65536 + num2 + 1;
            }
            int result = (array[inBufPos + 1] << 8) | array[inBufPos];
            inBufPos += 2;
            return result;
        }

        private int ReadCore(byte[] buffer, int offset, int count)
        {
            int num = count;
            byte[] array = decodeBuffer;
            int num2 = inBufEnd - inBufPos;
            int num3 = ((num < num2) ? num : num2);
            if (num3 != 0)
            {
                int num4 = inBufPos;
                int num5 = num3;
                while (num5-- != 0)
                {
                    buffer[offset++] = array[num4++];
                }
                inBufPos = num4;
                num -= num3;
            }
            if (num != 0)
            {
                int num6;
                if (num >= 128)
                {
                    num6 = input.Read(buffer, offset, (int)((num < inputLength) ? num : inputLength));
                    num -= num6;
                }
                else
                {
                    num6 = input.Read(array, 65536, (int)((128 < inputLength) ? 128 : inputLength));
                    inBufPos = 65536;
                    inBufEnd = 65536 + num6;
                    num3 = ((num < num6) ? num : num6);
                    int num7 = inBufPos;
                    int num8 = num3;
                    while (num8-- != 0)
                    {
                        buffer[offset++] = array[num7++];
                    }
                    inBufPos = num7;
                    num -= num3;
                }
                inputLength -= num6;
            }
            return count - num;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
    internal class LzxDecoderStream : Stream
    {
        private LzxDecoder dec;

        private MemoryStream decompressedStream;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public LzxDecoderStream(Stream input, int decompressedSize, int compressedSize)
        {
            dec = new LzxDecoder(16);
            Decompress(input, decompressedSize, compressedSize);
        }

        private void Decompress(Stream stream, int decompressedSize, int compressedSize)
        {
            decompressedStream = new MemoryStream(decompressedSize);
            long position = stream.Position;
            long num = position;
            while (num - position < compressedSize)
            {
                int num2 = stream.ReadByte();
                int num3 = stream.ReadByte();
                int num4 = (num2 << 8) | num3;
                int num5 = 32768;
                if (num2 == 255)
                {
                    int num6 = num3;
                    num3 = (byte)stream.ReadByte();
                    num5 = (num6 << 8) | num3;
                    byte num7 = (byte)stream.ReadByte();
                    num3 = (byte)stream.ReadByte();
                    num4 = (num7 << 8) | num3;
                    num += 5;
                }
                else
                {
                    num += 2;
                }
                if (num4 == 0 || num5 == 0)
                {
                    break;
                }
                dec.Decompress(stream, num4, decompressedStream, num5);
                num += num4;
                stream.Seek(num, SeekOrigin.Begin);
            }
            if (decompressedStream.Position != decompressedSize)
            {
                throw new ContentLoadException("Decompression failed.");
            }
            decompressedStream.Seek(0L, SeekOrigin.Begin);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                decompressedStream.Dispose();
            }
            dec = null;
            decompressedStream = null;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return decompressedStream.Read(buffer, offset, count);
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }

}