﻿//#define ReaderWriterLockSlim
using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite
{
    /// <summary>
    /// A geometry service provider class
    /// </summary>
    public class NtsGeometryServices
    {
        private static volatile NtsGeometryServices _instance;

        private static readonly object LockObject1 = new object();
        private static readonly object LockObject2 = new object();

        /// <summary>
        /// Gets or sets the current instance
        /// </summary>
        public static NtsGeometryServices Instance
        {
            get
            {
                lock (LockObject1)
                {
                    if (_instance != null)
                        return _instance;

                    lock (LockObject2)
                    {
                        _instance = new NtsGeometryServices();
                    }
                    return _instance;
                }
            }

            set
            {
                //Never
                if (value == null)
                    return;
                lock (LockObject1)
                {
                    _instance = value;
                }
            }
        }

        #region Key
        private struct GeometryFactoryKey
        {
            private readonly PrecisionModel _precisionModel;
            private readonly CoordinateSequenceFactory _factory;
            private readonly int _srid;

            public GeometryFactoryKey(PrecisionModel precisionModel, CoordinateSequenceFactory factory, int srid)
            {
                _precisionModel = precisionModel;
                _factory = factory;
                _srid = srid;
            }

            public override int GetHashCode()
            {
                return 889377 ^ _srid ^ _precisionModel.GetHashCode() ^ _factory.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is GeometryFactoryKey))
                    return false;
                var other = (GeometryFactoryKey) obj;

                if (_srid != other._srid)
                    return false;
                if (!_precisionModel.Equals(other._precisionModel))
                    return false;

                return _factory.Equals(other._factory);
            }
        }
        #endregion

        #region SaveDictionary
#if ReaderWriterLockSlim
        private class SaveDictionary<TKey, TValue>
        {
            //private readonly object _lock = new object();
            private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
            private readonly System.Threading.ReaderWriterLockSlim _rwLockSlim =
                new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.NoRecursion);

            public void Add(TKey key, ref TValue value)
            {
                _rwLockSlim.EnterUpgradeableReadLock();
                try
                {
                    TValue tmp;
                    if (_dictionary.TryGetValue(key, out tmp))
                    {
                        value = tmp;
                        return;
                    }

                    _rwLockSlim.EnterWriteLock();
                    try
                    {
                        _dictionary.Add(key, value);
                    }
                    finally
                    {
                        _rwLockSlim.ExitWriteLock();
                    }
                }
                finally { _rwLockSlim.ExitUpgradeableReadLock();}
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                var res = false;
                _rwLockSlim.EnterReadLock();
                try
                {
                    res = _dictionary.TryGetValue(key, out value);
                }
                catch (Exception)
                {
                    value = default(TValue);
                }
                finally
                {
                    _rwLockSlim.ExitReadLock();
                }
                return res;
            }

            public int Count
            {
                get
                {
                    var ret = 0;
                    _rwLockSlim.EnterReadLock();
                    try
                    {
                        ret = _dictionary.Count;
                    }
                    finally
                    {
                        _rwLockSlim.ExitReadLock();
                    }
                    return ret;
                }
            }
        }
#else
        private class SaveDictionary<TKey, TValue>
        {
            private readonly object _lock = new object();
            private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

            public void Add(TKey key, ref TValue value)
            {
                lock (_lock)
                {
                    if (!_dictionary.ContainsKey(key))
                        _dictionary.Add(key, value);
                }
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                lock (_lock)
                    return _dictionary.TryGetValue(key, out value);
            }

            public int Count
            {
                get
                {
                    lock(_lock)
                        return _dictionary.Count;
                }
            }
        }
#endif
        #endregion

        private readonly object _factoriesLock = new object();
        private readonly SaveDictionary<GeometryFactoryKey, GeometryFactory> _factories = new SaveDictionary<GeometryFactoryKey, GeometryFactory>();

        /// <summary>
        /// Creates an instance of this class, using the <see cref="CoordinateArraySequenceFactory"/> as default and a <see cref="PrecisionModels.Floating"/> precision model. No <see cref="DefaultSRID"/> is specified
        /// </summary>
        public NtsGeometryServices()
            : this(CoordinateArraySequenceFactory.Instance,
            new PrecisionModel(PrecisionModels.Floating), -1)
        {
        }

        /// <summary>
        /// Creates an instance of this class, using the provided <see cref="CoordinateSequenceFactory"/>, <see cref="PrecisionModel"/> and spatial reference Id (<paramref name="srid"/>.
        /// </summary>
        /// <param name="coordinateSequenceFactory">The coordinate sequence factory to use.</param>
        /// <param name="precisionModel">The precision model.</param>
        /// <param name="srid">The default spatial reference ID</param>
        public NtsGeometryServices(CoordinateSequenceFactory coordinateSequenceFactory,
            PrecisionModel precisionModel, int srid)
        {
            DefaultCoordinateSequenceFactory = coordinateSequenceFactory;
            DefaultPrecisionModel = precisionModel;
            DefaultSRID = srid;
        }

        /// <summary>
        /// Gets the default spatial reference id
        /// </summary>
        public int DefaultSRID { get; set; }

        /// <summary>
        /// Gets or sets the coordiate sequence factory to use
        /// </summary>
        public CoordinateSequenceFactory DefaultCoordinateSequenceFactory { get; private set; }

        /// <summary>
        /// Gets or sets the default precision model
        /// </summary>
        public PrecisionModel DefaultPrecisionModel { get; private set; }

        /// <summary>
        /// Creates a precision model based on given precision model type
        /// </summary>
        /// <returns>The precision model type</returns>
        public PrecisionModel CreatePrecisionModel(PrecisionModels modelType)
        {
            return new PrecisionModel(modelType);
        }

        /// <summary>
        /// Creates a precision model based on given precision model.
        /// </summary>
        /// <returns>The precision model</returns>
        public PrecisionModel CreatePrecisionModel(PrecisionModel precisionModel)
        {
            return new PrecisionModel(precisionModel);
        }

        /// <summary>
        /// Creates a precision model based on the given scale factor.
        /// </summary>
        /// <param name="scale">The scale factor</param>
        /// <returns>The precision model.</returns>
        public PrecisionModel CreatePrecisionModel(double scale)
        {
            return new PrecisionModel(scale);
        }

        public GeometryFactory CreateGeometryFactory()
        {
            return CreateGeometryFactory(DefaultSRID);
        }

        public GeometryFactory CreateGeometryFactory(int srid)
        {
            return CreateGeometryFactory(DefaultPrecisionModel, srid, DefaultCoordinateSequenceFactory);
        }

        public GeometryFactory CreateGeometryFactory(CoordinateSequenceFactory coordinateSequenceFactory)
        {
            return CreateGeometryFactory(DefaultPrecisionModel, DefaultSRID, coordinateSequenceFactory);
        }

        public GeometryFactory CreateGeometryFactory(PrecisionModel precisionModel)
        {
            return CreateGeometryFactory(precisionModel, DefaultSRID, DefaultCoordinateSequenceFactory);
        }

        public GeometryFactory CreateGeometryFactory(PrecisionModel precisionModel, int srid)
        {
            return CreateGeometryFactory(precisionModel, srid, DefaultCoordinateSequenceFactory);
        }

        public GeometryFactory CreateGeometryFactory(PrecisionModel precisionModel, int srid, CoordinateSequenceFactory coordinateSequenceFactory)
        {
            if (precisionModel == null)
                throw new ArgumentNullException("precisionModel");
            if (coordinateSequenceFactory == null)
                throw new ArgumentNullException("coordinateSequenceFactory");

            var gfkey = new GeometryFactoryKey(precisionModel, coordinateSequenceFactory, srid);
            GeometryFactory factory;
            if (!_factories.TryGetValue(gfkey, out factory))
            {
                if (!_factories.TryGetValue(gfkey, out factory))
                {
                    factory = new GeometryFactory(precisionModel, srid, coordinateSequenceFactory);
                    _factories.Add(gfkey, ref factory);
                }
            }
            return factory;
        }

        //    #region Implementation of ISerializable

        //    public GeometryServices(SerializationInfo info, StreamingContext context)
        //    {
        //        var pmType = (PrecisionModels) info.GetInt32("type");
        //        if (pmType != PrecisionModels.Fixed)
        //        {
        //            DefaultPrecisionModel = new PrecisionModel(pmType);
        //        }
        //        else
        //        {
        //            var scale = info.GetDouble("scale");
        //            DefaultPrecisionModel = new PrecisionModel(scale);
        //        }
        //        if (info.GetBoolean("csfPredefined"))
        //        {
        //            var csfAssembly = info.GetString("csfAssembly");
        //            var csfName = info.GetString("csfName");
        //            DefaultCoordinateSequenceFactory = Find(csfAssembly, csfName);
        //        }
        //        else
        //        {
        //            DefaultCoordinateSequenceFactory = (ICoordinateSequenceFactory)
        //                                        info.GetValue("csf", typeof (ICoordinateSequenceFactory));
        //        }
        //        /*_instance = this;*/
        //    }

        //    private ICoordinateSequenceFactory Find(string csfAssembly, string csfName)
        //    {
        //        switch (csfName)
        //        {
        //            case "NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory":
        //                return CoordinateArraySequenceFactory.Instance;
        //            case "NetTopologySuite.Geometries.Implementation.PackedCoordinateSequenceFactory.Double":
        //                return PackedCoordinateSequenceFactory.DoubleFactory;
        //            case "NetTopologySuite.Geometries.Implementation.PackedCoordinateSequenceFactory.Float":
        //                return PackedCoordinateSequenceFactory.FloatFactory;
        //            case "NetTopologySuite.Geometries.Implementation.DotSpatialAffineCoordinateSequenceFactory":
        //                return DotSpatialAffineCoordinateSequenceFactory.Instance;
        //        }
        //        Assert.ShouldNeverReachHere("CoordinateSequenceFactory instance not found!");
        //        return null;
        //    }

        //    public void GetObjectData(SerializationInfo info, StreamingContext context)
        //    {
        //        info.AddValue("type", (int)DefaultPrecisionModel.PrecisionModelType);
        //        if (DefaultPrecisionModel.PrecisionModelType == PrecisionModels.Fixed)
        //            info.AddValue("scale", (int)DefaultPrecisionModel.Scale);

        //        if (DefaultCoordinateSequenceFactory is CoordinateArraySequenceFactory ||
        //            DefaultCoordinateSequenceFactory is PackedCoordinateSequenceFactory ||
        //            DefaultCoordinateSequenceFactory is DotSpatialAffineCoordinateSequenceFactory)
        //        {
        //            var type = DefaultCoordinateSequenceFactory.GetType();
        //            var name = type.FullName;
        //            if (DefaultCoordinateSequenceFactory is PackedCoordinateSequenceFactory)
        //            {
        //                if (DefaultCoordinateSequenceFactory == PackedCoordinateSequenceFactory.DoubleFactory)
        //                    name += ".Double";
        //                else
        //                    name += ".Float";
        //            }
        //            info.AddValue("csfPredefined", true);
        //            info.AddValue("csfAssembly", type.Assembly.FullName);
        //            info.AddValue("csfName", name);
        //        }
        //        else
        //        {
        //            info.AddValue("csfPredefined", false);
        //            if (!(DefaultCoordinateSequenceFactory is ISerializable))
        //                throw new InvalidOperationException(string.Format("Cannot serialize '{0}'",
        //                                                                  DefaultCoordinateSequenceFactory.GetType().FullName));
        //            info.AddValue("csf", DefaultCoordinateSequenceFactory, typeof(ICoordinateSequenceFactory));
        //        }
        //    }

        //    #endregion

        /// <summary>
        /// Gets a value representing the number of geometry factories that have been stored in the cache
        /// </summary>
        public int NumFactories { get { lock (_factoriesLock) return _factories.Count; } }
    }
}
