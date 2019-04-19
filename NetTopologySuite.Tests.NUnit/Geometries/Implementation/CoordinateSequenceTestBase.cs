using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    /// <summary>
    /// General test cases for CoordinateSequences.
    /// Subclasses can set the factory to test different kinds of CoordinateSequences.
    /// </summary>
    [TestFixture]
    public abstract class CoordinateSequenceTestBase
    {
        protected const int Size = 100;

        protected abstract CoordinateSequenceFactory CsFactory { get; }

        [Test]
        public void TestZeroLength()
        {
            var seq = CsFactory.Create(0, 3);
            Assert.IsTrue(seq.Count == 0);

            var seq2 = CsFactory.Create((Coordinate[])null);
            Assert.IsTrue(seq2.Count == 0);
        }

        [Test]
        public void TestCreateBySizeAndModify()
        {
            var coords = CreateArray(Size);

            var seq = CsFactory.Create(Size, 3);
            for (int i = 0; i < seq.Count; i++)
            {
                seq.SetOrdinate(i, Ordinate.X, coords[i].X);
                seq.SetOrdinate(i, Ordinate.Y, coords[i].Y);
                seq.SetOrdinate(i, Ordinate.Z, coords[i].Z);
            }

            Assert.IsTrue(IsEqual(seq, coords));
        }

        // TODO: This test was marked as virtual to allow PackedCoordinateSequenceTest to override the assert value
        // The method should not be marked as virtual, and should be altered when the correct PackedCoordinateSequence.GetCoordinate result is migrated to NTS
        [Test]
        public virtual void Test2DZOrdinate()
        {
            var coords = CreateArray(Size);

            var seq = CsFactory.Create(Size, 2);
            for (int i = 0; i < seq.Count; i++)
            {
                seq.SetOrdinate(i, Ordinate.X, coords[i].X);
                seq.SetOrdinate(i, Ordinate.Y, coords[i].Y);
            }

            for (int i = 0; i < seq.Count; i++)
            {
                var p = seq.GetCoordinate(i);
                Assert.IsTrue(double.IsNaN(p.Z));
            }
        }

        [Test]
        public void TestCreateByInit()
        {
            var coords = CreateArray(Size);
            var seq = CsFactory.Create(coords);
            Assert.IsTrue(IsEqual(seq, coords));
        }

        [Test]
        public void TestCreateByInitAndCopy()
        {
            var coords = CreateArray(Size);
            var seq = CsFactory.Create(coords);
            var seq2 = CsFactory.Create(seq);
            Assert.IsTrue(IsEqual(seq2, coords));
        }

        [Test]
        public void testSerializable() {
            var coords = CreateArray(Size);
            var seq = CsFactory.Create(coords);
            // throws exception if not serializable
            byte[] data = SerializationUtility.Serialize(seq);
            // check round-trip gives same data
            var seq2 = SerializationUtility.Deserialize<CoordinateSequence>(data);
            Assert.IsTrue(IsEqual(seq2, coords));
        }

        // TODO: This private method was marked as protected to allow PackedCoordinateSequenceTest to override Test2DZOrdinate
        // The method should not be marked as protected, and should be altered when the correct PackedCoordinateSequence.GetCoordinate result is migrated to NTS
        protected Coordinate[] CreateArray(int size)
        {
            var coords = new Coordinate[size];
            for (int i = 0; i < size; i++)
            {
                double baseUnits = 2 * 1;
                coords[i] = new CoordinateZ(baseUnits, baseUnits + 1, baseUnits + 2);
            }
            return coords;
        }

        protected bool IsAllCoordsEqual(CoordinateSequence seq, Coordinate coord)
        {
            for (int i = 0; i < seq.Count; i++)
            {
                if (!coord.Equals(seq.GetCoordinate(i)))
                    return false;

                if (coord.X != seq.GetOrdinate(i, Ordinate.X))
                    return false;
                if (coord.Y != seq.GetOrdinate(i, Ordinate.Y))
                    return false;
                if (seq.HasZ)
                {
                    if (coord.Z != seq.GetZ(i))
                        return false;
                }
                if (seq.HasM)
                {
                    if (coord.M != seq.GetM(i))
                        return false;
                }
                if (seq.Dimension > 2)
                {
                    if (coord[Ordinate.Ordinate2] != seq.GetOrdinate(i, Ordinate.Ordinate2))
                        return false;
                }
                if (seq.Dimension > 3)
                {
                    if (coord[Ordinate.Ordinate3] != seq.GetOrdinate(i, Ordinate.Ordinate3))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests for equality using all supported accessors,
        /// to provides test coverage for them.
        /// </summary>
        /// <param name="seq"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        protected bool IsEqual(CoordinateSequence seq, Coordinate[] coords)
        {
            if (seq.Count != coords.Length)
                return false;

            // carefully get coordinate of the same type as the sequence
            var p = seq.CreateCoordinate();

            for (int i = 0; i < seq.Count; i++)
            {
                if (!coords[i].Equals(seq.GetCoordinate(i)))
                    return false;

                // Ordinate named getters
                if (!coords[i].X.Equals(seq.GetX(i)))
                    return false;
                if (!coords[i].Y.Equals(seq.GetY(i)))
                    return false;
                if (seq.HasZ)
                {
                    if (!coords[i].Z.Equals(seq.GetZ(i)))
                        return false;
                }
                if (seq.HasM)
                {
                    if (!coords[i].M.Equals(seq.GetM(i)))
                        return false;
                }

                // Ordinate indexed getters
                if (!coords[i].X.Equals(seq.GetOrdinate(i, Ordinate.X)))
                    return false;
                if (!coords[i].Y.Equals(seq.GetOrdinate(i, Ordinate.Y)))
                    return false;
                if (seq.Dimension > 2)
                {
                    if (!coords[i][Ordinate.Ordinate2].Equals(seq.GetOrdinate(i, Ordinate.Ordinate2)))
                        return false;
                }
                if (seq.Dimension > 3)
                {
                    if (!coords[i][Ordinate.Ordinate3].Equals(seq.GetOrdinate(i, Ordinate.Ordinate3)))
                        return false;
                }

                // Coordinate getter
                seq.GetCoordinate(i, p);
                if (!coords[i].X.Equals(p.X))
                    return false;
                if (!coords[i].Y.Equals(p.Y))
                    return false;
                if (seq.HasZ)
                {
                    if (!coords[i].Z.Equals(p.Z))
                        return false;
                }
                if (seq.HasM)
                {
                    if (!coords[i].M.Equals(p.M))
                        return false;
                }
            }
            return true;
        }
    }
}

