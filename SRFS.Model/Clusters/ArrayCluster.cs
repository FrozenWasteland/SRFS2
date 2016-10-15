﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace SRFS.Model.Clusters {

    public abstract class ArrayCluster : Cluster {

        // Public
        #region Fields

        public static new int HeaderLength => _headerLength;
        private static readonly int _headerLength;

        static ArrayCluster() {
            _headerLength = Cluster.HeaderLength + Offset_Data;
        }

        #endregion
        #region Properties

        public int NextClusterAddress {
            get {
                return base.OpenBlock.ToInt32(Offset_NextCluster);
            }
            set {
                base.OpenBlock.Set(Offset_NextCluster, value);
            }
        }

        #endregion
        #region Methods

        public static int CalculateElementCount(int elementSize) {
            return (Configuration.Geometry.BytesPerCluster - HeaderLength) / elementSize;
        }

        public override void Initialize() {
            base.Initialize();
            NextClusterAddress = Constants.NoAddress;
        }

        #endregion

        // Protected
        #region Constructors

        protected ArrayCluster(int address) : base(address, Configuration.Geometry.BytesPerCluster) {
            if (address == Constants.NoAddress) throw new ArgumentOutOfRangeException();
            _data = new DataBlock(base.OpenBlock, Offset_Data, base.OpenBlock.Length - Offset_Data);
            NextClusterAddress = Constants.NoAddress;
        }

        protected ArrayCluster(ArrayCluster c) : base(c) {
            _data = new DataBlock(base.OpenBlock, Offset_Data, base.OpenBlock.Length - Offset_Data);
        }

        #endregion
        #region Properties

        public override long AbsoluteAddress {
            get {
                return Address == Constants.NoAddress ? Constants.NoAddress : (long)Address * Configuration.Geometry.BytesPerCluster;
            }
        }

        protected new DataBlock OpenBlock => _data;

        #endregion

        // Private
        #region Fields

        private static readonly int Offset_NextCluster = 0;
        private static readonly int Length_NextCluster = sizeof(int);

        private static readonly int Offset_Data = Offset_NextCluster + Length_NextCluster;

        private DataBlock _data;

        #endregion
    }

    public abstract class ArrayCluster<T> : ArrayCluster, IEnumerable<T> {

        // Public
        #region Properties

        public T this[int index] {
            get {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException();
                return ReadElement(base.OpenBlock, index * _elementSize);
            }
            set {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException();
                WriteElement(value, base.OpenBlock, index * _elementSize);
            }
        }

        public int Count => _count;

        #endregion

        // Protected
        #region Constructors

        protected ArrayCluster(int address, int elementSize) : base(address) {
            _elementSize = elementSize;
            _count = CalculateElementCount(elementSize);
        }

        protected ArrayCluster(ArrayCluster<T> c) : base(c) {
            _elementSize = c._elementSize;
            _count = c._count;
        }

        #endregion
        #region Methods

        protected abstract void WriteElement(T value, DataBlock byteBlock, int offset);

        protected abstract T ReadElement(DataBlock byteBlock, int offset);

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < _count; i++) yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        // Private
        #region Fields

        private int _elementSize;
        private int _count;

        #endregion
    }
}
