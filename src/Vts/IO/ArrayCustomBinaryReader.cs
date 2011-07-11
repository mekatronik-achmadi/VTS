﻿using System;
using System.Collections.Generic;
using System.IO;
using Vts.Extensions;
using System.Numerics;

namespace Vts.IO
{
    public class ArrayCustomBinaryReader<T> 
        : ICustomBinaryReader<Array> where T : struct
    {
        private int[] _dims;

        public ArrayCustomBinaryReader(int[] dims)
        {
            _dims = dims;
        }

        public ArrayCustomBinaryReader(int length)
            : this(new[] { length })
        {
        }

        public Array ReadFromBinary(BinaryReader br)
        {
            // Initialize the array
            Array dataOut = Array.CreateInstance(typeof(T), _dims);

            var dataType = typeof(T);

            if (dataType.Equals(typeof(double)))
            {
                dataOut.PopulateFromEnumerable(ReadDoubles(br, dataOut.Length));
                return dataOut;
            }

            if (dataType.Equals(typeof(float)))
            {
                dataOut.PopulateFromEnumerable(ReadFloats(br, dataOut.Length));
                return dataOut;
            }

            if (dataType.Equals(typeof(ushort)))
            {
                dataOut.PopulateFromEnumerable(ReadUShorts(br, dataOut.Length));
                return dataOut;
            }

            if (dataType.Equals(typeof(byte)))
            {
                dataOut.PopulateFromEnumerable(ReadBytes(br, dataOut.Length));
                return dataOut;
            }

            if (dataType.Equals(typeof(Complex)))
            {
                dataOut.PopulateFromEnumerable(ReadComplices(br, dataOut.Length));
                return dataOut;
            }

            throw new NotSupportedException("Type of T is not supported");
        }

        private static IEnumerable<double> ReadDoubles(BinaryReader br, int numberOfElements)
        {
            for (int i = 0; i < numberOfElements; i++)
            {
                yield return br.ReadDouble();
            }
        }

        private static IEnumerable<float> ReadFloats(BinaryReader br, int numberOfElements)
        {
            for (int i = 0; i < numberOfElements; i++)
            {
                yield return br.ReadSingle();
            }
        }

        private static IEnumerable<ushort> ReadUShorts(BinaryReader br, int numberOfElements)
        {
            for (int i = 0; i < numberOfElements; i++)
            {
                yield return br.ReadUInt16();
            }
        }

        private static IEnumerable<byte> ReadBytes(BinaryReader br, int numberOfElements)
        {
            return br.ReadBytes(numberOfElements);
        }

        private static IEnumerable<Complex> ReadComplices(BinaryReader br, int numberOfElements)
        {
            for (int i = 0; i < numberOfElements; i++)
            {
                yield return new Complex(br.ReadDouble(), br.ReadDouble());
            }
        }
    }
}
