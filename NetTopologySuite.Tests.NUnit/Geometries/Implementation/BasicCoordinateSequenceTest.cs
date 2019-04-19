using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    [TestFixture]
    public class BasicCoordinateSequenceTest
    {
        [Test]
        public void TestClone()
        {
            var s1 = CoordinateArraySequenceFactory.Instance.Create(
                new[] { new Coordinate(1, 2), new Coordinate(3, 4) });
            var s2 = (CoordinateSequence)s1.Copy();
            Assert.IsTrue(s1.GetCoordinate(0).Equals(s2.GetCoordinate(0)));
            Assert.IsTrue(s1.GetCoordinate(0) != s2.GetCoordinate(0));
        }

        [Test]
        public void TestCloneDimension2()
        {
            var s1 = CoordinateArraySequenceFactory.Instance.Create(2, 2);
            s1.SetOrdinate(0, 0, 1);
            s1.SetOrdinate(0, 1, 2);
            s1.SetOrdinate(1, 0, 3);
            s1.SetOrdinate(1, 1, 4);

            var s2 = (CoordinateSequence)s1.Copy();
            Assert.IsTrue(s1.Dimension == s2.Dimension);
            Assert.IsTrue(s1.GetCoordinate(0).Equals(s2.GetCoordinate(0)));
            Assert.IsTrue(s1.GetCoordinate(0) != s2.GetCoordinate(0));
        }

        [Test]
        public void TestCloneDimension3()
        {
            var s1 = CoordinateArraySequenceFactory.Instance.Create(2, 3);
            s1.SetOrdinate(0, 0, 1);
            s1.SetOrdinate(0, 1, 2);
            s1.SetOrdinate(0, 2, 10);
            s1.SetOrdinate(1, 0, 3);
            s1.SetOrdinate(1, 1, 4);
            s1.SetOrdinate(1, 2, 20);

            var s2 = (CoordinateSequence)s1.Copy();
            Assert.IsTrue(s1.Dimension == s2.Dimension);
            Assert.IsTrue(s1.GetCoordinate(0).Equals(s2.GetCoordinate(0)));
            Assert.IsTrue(s1.GetCoordinate(0) != s2.GetCoordinate(0));
        }

        [Test]
        public void TestCloneDimension4()
        {
            var s1 = CoordinateArraySequenceFactory.Instance.Create(2, 4, 1);
            s1.SetOrdinate(0, 0, 1);
            s1.SetOrdinate(0, 1, 2);
            s1.SetOrdinate(0, 2, 10);
            s1.SetOrdinate(0, 3, 100);
            s1.SetOrdinate(1, 0, 3);
            s1.SetOrdinate(1, 1, 4);
            s1.SetOrdinate(1, 2, 20);
            s1.SetOrdinate(1, 3, 200);

            var s2 = (CoordinateSequence)s1.Copy();
            Assert.IsTrue(s1.Dimension == s2.Dimension);
            Assert.IsTrue(s1.GetCoordinate(0).Equals(s2.GetCoordinate(0)));
            Assert.IsTrue(s1.GetCoordinate(0) != s2.GetCoordinate(0));
        }

        /// <summary>
        /// A simple test that using CoordinateM works
        /// for creation and running a basic function.
        /// </summary>
        [Test]
        public void TestLengthWithXYM()
        {
            CoordinateM[] coords =
            {
                new CoordinateM(1, 1, 1),
                new CoordinateM(2, 1, 2),
            };

            var factory = new GeometryFactory();
            var line = factory.CreateLineString(coords);

            double len = line.Length;
            Assert.That(len, Is.EqualTo(1));
        }
    }
}
