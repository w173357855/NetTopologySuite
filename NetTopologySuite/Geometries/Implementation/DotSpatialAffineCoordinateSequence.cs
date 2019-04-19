using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A coordinate sequence that follows the dotspatial shape range
    /// </summary>
    [Serializable]
    public class DotSpatialAffineCoordinateSequence :
        CoordinateSequence
        //IMeasuredCoordinateSequence
    {
        private readonly double[][] _ordinateArrays;
        private readonly double[] _xy;
        private readonly double[] _z;
        private readonly double[] _m;

        [NonSerialized]
        private WeakReference _coordinateArrayRef;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="coordinates">The coordinates</param>
        /// <param name="ordinates"></param>
        public DotSpatialAffineCoordinateSequence(IList<Coordinate> coordinates, Ordinates ordinates)
            : base(coordinates?.Count ?? 0, OrdinatesUtility.OrdinatesToDimension(ordinates), OrdinatesUtility.OrdinatesToMeasures(ordinates))
        {
            if (coordinates == null)
            {
                _xy = new double[0];
                return;
            }
            _xy = new double[2 * coordinates.Count];
            int j = 0;
            for (int i = 0; i < coordinates.Count; i++)
            {
                XY[j++] = coordinates[i].X;
                XY[j++] = coordinates[i].Y;
            }

            if ((ordinates & Ordinates.Z) == Ordinates.Z)
            {
                _z = new double[coordinates.Count];
                for (int i = 0; i < coordinates.Count; i++)
                    Z[i] = coordinates[i].Z;
            }

            if ((ordinates & Ordinates.M) == Ordinates.M)
            {
                _m = new double[coordinates.Count];
                for (int i = 0; i < coordinates.Count; i++)
                    M[i] = Coordinate.NullOrdinate;
            }
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        /// <param name="dimension">The number of dimensions.</param>
        public DotSpatialAffineCoordinateSequence(int size, int dimension)
            : base(size, dimension, dimension > 2 ? 1 : 0)
        {
            _xy = new double[2 * size];
            if (dimension <= 2) return;

            _z = new double[size];
            for (int i = 0; i < size; i++)
                _z[i] = double.NaN;

            _m = new double[size];
            for (int i = 0; i < size; i++)
                _m[i] = double.NaN;
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        /// <param name="ordinates">The kind of ordinates.</param>
        public DotSpatialAffineCoordinateSequence(int size, Ordinates ordinates)
            : base(size, OrdinatesUtility.OrdinatesToDimension(ordinates), OrdinatesUtility.OrdinatesToMeasures(ordinates))
        {
            _xy = new double[2 * size];
            if ((ordinates & Ordinates.Z) == Ordinates.Z)
            {
                _z = new double[size];
                for (int i = 0; i < size; i++)
                    _z[i] = Coordinate.NullOrdinate;
            }

            if ((ordinates & Ordinates.M) == Ordinates.M)
            {
                _m = new double[size];
                for (int i = 0; i < size; i++)
                    _m[i] = Coordinate.NullOrdinate;
            }
        }

        /// <summary>
        /// Creates a sequence based on the given coordinate sequence.
        /// </summary>
        /// <param name="coordSeq">The coordinate sequence.</param>
        /// <param name="ordinates">The ordinates to copy</param>
        public DotSpatialAffineCoordinateSequence(CoordinateSequence coordSeq, Ordinates ordinates)
            : base(coordSeq.Count, OrdinatesUtility.OrdinatesToDimension(ordinates), OrdinatesUtility.OrdinatesToMeasures(ordinates))
        {
            int count = coordSeq.Count;

            _xy = new double[2 * count];
            if ((ordinates & Ordinates.Z) == Ordinates.Z)
                _z = new double[count];
            if ((ordinates & Ordinates.M) == Ordinates.M)
                _m = new double[count];

            var dsCoordSeq = coordSeq as DotSpatialAffineCoordinateSequence;
            if (dsCoordSeq != null)
            {
                double[] nullOrdinateArray = null;
                Buffer.BlockCopy(dsCoordSeq._xy, 0, _xy, 0, 16 * count);
                if ((ordinates & Ordinates.Z) == Ordinates.Z)
                {
                    double[] copyOrdinateArray = dsCoordSeq.Z != null
                        ? dsCoordSeq.Z
                        : nullOrdinateArray = NullOrdinateArray(count);
                    Buffer.BlockCopy(copyOrdinateArray, 0, _z, 0, 8 * count);
                }

                if ((ordinates & Ordinates.M) == Ordinates.M)
                {
                    double[] copyOrdinateArray = dsCoordSeq.M != null
                        ? dsCoordSeq.M
                        : nullOrdinateArray ?? NullOrdinateArray(count);
                    Buffer.BlockCopy(copyOrdinateArray, 0, _m, 0, 8*count);
                }

                return;
            }

            int j = 0;
            for (int i = 0; i < coordSeq.Count; i++)
            {
                _xy[j++] = coordSeq.GetX(i);
                _xy[j++] = coordSeq.GetY(i);
                if (_z != null) _z[i] = coordSeq.GetZ(i);
                if (_m != null) _m[i] = coordSeq.GetM(i);
            }
        }

        private static double[] NullOrdinateArray(int size)
        {
            double[] res = new double[size];
            for (int i = 0; i < size; i++)
                res[i] = Coordinate.NullOrdinate;
            return res;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="xy"></param>
        /// <param name="z"></param>
        public DotSpatialAffineCoordinateSequence(double[] xy, double[] z)
            : base(xy?.Length / 2 ?? 0, z == null ? 3 : 2, 0)
        {
            _xy = xy;
            _z = z;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="xy"></param>
        /// <param name="z"></param>
        /// <param name="m"></param>
        public DotSpatialAffineCoordinateSequence(double[] xy, double[] z, double[] m)
            : base(xy?.Length / 2 ?? 0, 4, 1)
        {
            _xy = xy;
            _z = z;
            _m = m;
        }

        [Obsolete]
        public object Clone()
        {
            return Copy();

        }

        /// <inheritdoc cref="CoordinateSequence.Copy"/>
        public override CoordinateSequence Copy()
        {
            return new DotSpatialAffineCoordinateSequence(this, Ordinates);
        }

        public override double GetOrdinate(int index, int ordinateIndex)
        {
            switch (ordinateIndex)
            {
                case 0:
                case 1:
                    return _xy[index * 2 + ordinateIndex];

                case 2:
                    if (_z == null)
                    {
                        goto case 3;
                    }

                    return _z?[index] ?? Coordinate.NullOrdinate;

                case 3:
                    return _m?[index] ?? Coordinate.NullOrdinate;

                default:
                    throw new NotSupportedException();
            }
        }

        public override void SetOrdinate(int index, int ordinateIndex, double value)
        {
            switch (ordinateIndex)
            {
                case 0:
                case 1:
                    _xy[index * 2 + ordinateIndex] = value;
                    break;

                case 2:
                    if (_z != null)
                    {
                        _z[index] = value;
                        break;
                    }

                    goto case 3;

                case 3:
                    if (_m != null) _m[index] = value;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Coordinate[] GetCachedCoords()
        {
            var localCoordinateArrayRef = _coordinateArrayRef;
            if (localCoordinateArrayRef != null && localCoordinateArrayRef.IsAlive)
            {
                var arr = (Coordinate[])localCoordinateArrayRef.Target;
                if (arr != null)
                    return arr;

                _coordinateArrayRef = null;
                return null;
            }
            return null;
        }

        public override Coordinate[] ToCoordinateArray()
        {
            var ret = GetCachedCoords();
            if (ret == null)
            {
                ret = base.ToCoordinateArray();
                _coordinateArrayRef = new WeakReference(ret);
            }

            return ret;
        }

        /// <summary>
        /// Creates a reversed version of this coordinate sequence with cloned <see cref="Coordinate"/>s
        /// </summary>
        /// <returns>A reversed version of this sequence</returns>
        public override CoordinateSequence Reversed()
        {
            double[] xy = new double[_xy.Length];

            double[] z = null, m = null;
            if (_z != null) z = new double[_z.Length];
            if (_m != null) m = new double[_m.Length];

            int j = 2* Count;
            int k = Count;
            for (int i = 0; i < Count; i++)
            {
                xy[--j] = _xy[2 * i + 1];
                xy[--j] = _xy[2 * i];
                k--;
                if (_z != null) z[k] = _z[i];
                if (_m != null) m[k] = _m[i];
            }
            return new DotSpatialAffineCoordinateSequence(xy, z, m);
        }

        /// <summary>
        /// Gets the vector with x- and y-ordinate values;
        /// </summary>
        /// <remarks>If you modify the values of this vector externally, you need to call <see cref="ReleaseCoordinateArray"/>!</remarks>
        public double[] XY => _xy;

        /// <summary>
        /// Gets the vector with z-ordinate values
        /// </summary>
        /// <remarks>If you modify the values of this vector externally, you need to call <see cref="ReleaseCoordinateArray"/>!</remarks>
        public double[] Z => _z;

        /// <summary>
        /// Gets the vector with measure values
        /// </summary>
        /// <remarks>If you modify the values of this vector externally, you need to call <see cref="ReleaseCoordinateArray"/>!</remarks>
        public double[] M => _m;

        /// <summary>
        /// Releases the weak reference to the weak referenced coordinate array
        /// </summary>
        /// <remarks>This is necessary if you modify the values of the <see cref="XY"/>, <see cref="Z"/>, <see cref="M"/> arrays externally.</remarks>
        public void ReleaseCoordinateArray()
        {
            _coordinateArrayRef = null;
        }
    }
}
