using System.Collections.Generic;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Static utility functions for dealing with <see cref="Ordinates"/> and dimension
    /// </summary>
    public static class OrdinatesUtility
    {
        /// <summary>
        /// Translates the <paramref name="ordinates"/>-flag to a number of dimensions.
        /// </summary>
        /// <param name="ordinates">The ordinates flag</param>
        /// <returns>The number of dimensions</returns>
        public static int OrdinatesToDimension(Ordinates ordinates)
        {
            // dimension should ALWAYS take X and Y into account.
            ordinates |= Ordinates.XY;

            // unset flags one-by-one until all flags are unset.
            // the number of times we did that is how many flags were initially set.
            int flagsUnsetSoFar = 0;
            while (ordinates != Ordinates.None)
            {
                ordinates &= (Ordinates)((int)ordinates - 1);

                ++flagsUnsetSoFar;
            }

            return flagsUnsetSoFar;
        }

        /// <summary>
        /// Translates the <paramref name="ordinates"/>-flag to a number of measures.
        /// </summary>
        /// <param name="ordinates">The ordinates flag</param>
        /// <returns>The number of measures</returns>
        public static int OrdinatesToMeasures(Ordinates ordinates)
        {
            return (int)(ordinates & Ordinates.M) >> (int)Ordinate.M;
        }

        /// <summary>
        /// Translates a dimension value to an <see cref="Ordinates"/>-flag.
        /// </summary>
        /// <remarks>The flag for <see cref="Ordinate.Z"/> is set first.</remarks>
        /// <param name="dimension">The dimension.</param>
        /// <returns>The ordinates-flag</returns>
        public static Ordinates DimensionToOrdinates(int dimension)
        {
            if (dimension == 3)
                return Ordinates.XYZ;
            if (dimension == 4)
                return Ordinates.XYZM;
            return Ordinates.XY;
        }

        /// <summary>
        /// Converts an <see cref="Ordinates"/> encoded flag to an array of <see cref="Ordinate"/> indices.
        /// </summary>
        /// <param name="ordinates">The ordinate flags</param>
        /// <param name="maxEval">The maximum oridinate flag that is to be checked</param>
        /// <returns>The ordinate indices</returns>
        public static Ordinate[] ToOrdinateArray(Ordinates ordinates, int maxEval = 4)
        {
            if (maxEval > 31) maxEval = 31;
            var intOrdinates = (int) ordinates;
            var ordinateList = new List<Ordinate>(maxEval);
            for (var i = 0; i < maxEval; i++)
            {
                if ((intOrdinates & (1<<i)) != 0) ordinateList.Add((Ordinate)i);
            }
            return ordinateList.ToArray();
        }

        /// <summary>
        /// Converts an array of <see cref="Ordinate"/> values to an <see cref="Ordinates"/> flag.
        /// </summary>
        /// <param name="ordinates">An array of <see cref="Ordinate"/> values</param>
        /// <returns>An <see cref="Ordinates"/> flag.</returns>
        public static Ordinates ToOrdinatesFlag(params Ordinate[] ordinates)
        {
            var result = Ordinates.None;
            foreach (var ordinate in ordinates)
            {
                result |= (Ordinates) (1 << ((int) ordinate));
            }
            return result;
        }
    }
}
