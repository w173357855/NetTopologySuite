using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public static class PolygonizeFunctions
    {
        private static Geometry Polygonize(Geometry g, bool extractOnlyPolygonal)
        {
            var lines = LineStringExtracter.GetLines(g);
            var polygonizer = new Polygonizer(extractOnlyPolygonal);
            polygonizer.Add(lines);
            return polygonizer.GetGeometry();
            /*
            Collection polys = polygonizer.getPolygons();
            Polygon[] polyArray = GeometryFactory.toPolygonArray(polys);
            return g.getFactory().createGeometryCollection(polyArray);
            */
        }
        public static Geometry Polygonize(Geometry g)
        {
            var lines = LineStringExtracter.GetLines(g);
            var polygonizer = new Polygonizer();
            polygonizer.Add(lines);
            var polys = polygonizer.GetPolygons();
            var polyArray = GeometryFactory.ToPolygonArray(polys);
            return g.Factory.CreateGeometryCollection(polyArray);
        }

        public static Geometry PolygonizeDangles(Geometry g)
        {
            var lines = LineStringExtracter.GetLines(g);
            var polygonizer = new Polygonizer();
            polygonizer.Add(lines);
            var geom = polygonizer.GetDangles();
            return g.Factory.BuildGeometry(CollectionUtil.Cast<LineString, Geometry>(geom));
        }

        public static Geometry PolygonizeCutEdges(Geometry g)
        {
            var lines = LineStringExtracter.GetLines(g);
            var polygonizer = new Polygonizer();
            polygonizer.Add(lines);
            var geom = polygonizer.GetCutEdges();
            return g.Factory.BuildGeometry(CollectionUtil.Cast<LineString, Geometry>(geom));
        }

        public static Geometry PolygonizeInvalidRingLines(Geometry g)
        {
            var lines = LineStringExtracter.GetLines(g);
            var polygonizer = new Polygonizer();
            polygonizer.Add(lines);
            var geom = polygonizer.GetInvalidRingLines();
            return g.Factory.BuildGeometry(geom);
        }

        public static Geometry PolygonizeAllErrors(Geometry g)
        {
            var lines = LineStringExtracter.GetLines(g);
            var polygonizer = new Polygonizer();
            polygonizer.Add(lines);
            var errs = new List<LineString>();
            errs.AddRange(polygonizer.GetDangles());
            errs.AddRange(polygonizer.GetCutEdges());
            errs.AddRange(CollectionUtil.Cast<Geometry, LineString>(polygonizer.GetInvalidRingLines()));
            return g.Factory.BuildGeometry(CollectionUtil.Cast<LineString, Geometry>(errs));
        }
    }
}