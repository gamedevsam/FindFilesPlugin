#region Copyright
/*
 * The original .NET implementation of the SimMetrics library is taken from the Java
 * source and converted to NET using the Microsoft Java converter.
 * It is notclear who made the initial convertion to .NET.
 * 
 * This updated version has started with the 1.0 .NET release of SimMetrics and used
 * FxCop (http://www.gotdotnet.com/team/fxcop/) to highlight areas where changes needed 
 * to be made to the converted code.
 * 
 * this version with updates Copyright (c) 2006 Chris Parkinson.
 * 
 * For any queries on the .NET version please contact me through the 
 * sourceforge web address.
 * 
 * SimMetrics - SimMetrics is a java library of Similarity or Distance
 * Metrics, e.g. Levenshtein Distance, that provide float based similarity
 * measures between string Data. All metrics return consistant measures
 * rather than unbounded similarity scores.
 *
 * Copyright (C) 2005 Sam Chapman - Open Source Release v1.1
 *
 * Please Feel free to contact me about this library, I would appreciate
 * knowing quickly what you wish to use it for and any criticisms/comments
 * upon the SimMetric library.
 *
 * email:       s.chapman@dcs.shef.ac.uk
 * www:         http://www.dcs.shef.ac.uk/~sam/
 * www:         http://www.dcs.shef.ac.uk/~sam/stringmetrics.html
 *
 * address:     Sam Chapman,
 *              Department of Computer Science,
 *              University of Sheffield,
 *              Sheffield,
 *              S. Yorks,
 *              S1 4DP
 *              United Kingdom,
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the
 * Free Software Foundation; either version 2 of the License, or (at your
 * option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License
 * for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 */
#endregion

namespace SimMetricsMetricUtilities {
    using System;
    using SimMetricsApi;
    using SimMetricsUtilities;

    /// <summary>
    /// implements the smith waterman with gotoh extension using a windowed affine gap.
    /// </summary>
    [Serializable]
    public class SmithWatermanGotohWindowedAffine : AbstractStringMetric {
        const double defaultMismatchScore = 0.0;
        const double defaultPerfectScore = 1.0;
        const int defaultWindowSize = 100;

        /// <summary>
        /// constructor - default (empty).
        /// </summary>
        public SmithWatermanGotohWindowedAffine()
            : this(new AffineGapRange5To0Multiplier1(), new SubCostRange5ToMinus3(), defaultWindowSize) {}

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="affineGapWindowSize">the size of the affine gap window to use</param>
        public SmithWatermanGotohWindowedAffine(int affineGapWindowSize)
            : this(new AffineGapRange5To0Multiplier1(), new SubCostRange5ToMinus3(), affineGapWindowSize) {}

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="gapCostFunction">the gap cost function</param>
        public SmithWatermanGotohWindowedAffine(AbstractAffineGapCost gapCostFunction)
            : this(gapCostFunction, new SubCostRange5ToMinus3(), defaultWindowSize) {}

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="gapCostFunction">the gap cost function</param>
        /// <param name="affineGapWindowSize">the size of the affine gap window to use</param>
        public SmithWatermanGotohWindowedAffine(AbstractAffineGapCost gapCostFunction, int affineGapWindowSize)
            : this(gapCostFunction, new SubCostRange5ToMinus3(), affineGapWindowSize) {}

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="gapCostFunction">the gap cost function</param>
        /// <param name="costFunction">the cost function to use</param>
        public SmithWatermanGotohWindowedAffine(AbstractAffineGapCost gapCostFunction, AbstractSubstitutionCost costFunction)
            : this(gapCostFunction, costFunction, defaultWindowSize) {}

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="gapCostFunction">the gap cost function</param>
        /// <param name="costFunction">the cost function to use</param>
        /// <param name="affineGapWindowSize">the size of the affine gap window to use</param>
        public SmithWatermanGotohWindowedAffine(AbstractAffineGapCost gapCostFunction, AbstractSubstitutionCost costFunction,
                                                int affineGapWindowSize) {
            gGapFunction = gapCostFunction;
            dCostFunction = costFunction;
            windowSize = affineGapWindowSize;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="costFunction">the cost function to use</param>
        public SmithWatermanGotohWindowedAffine(AbstractSubstitutionCost costFunction)
            : this(new AffineGapRange5To0Multiplier1(), costFunction, defaultWindowSize) {}

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="costFunction">the cost function to use</param>
        /// <param name="affineGapWindowSize">the size of the affine gap window to use</param>
        public SmithWatermanGotohWindowedAffine(AbstractSubstitutionCost costFunction, int affineGapWindowSize)
            : this(new AffineGapRange5To0Multiplier1(), costFunction, affineGapWindowSize) {}

        /// <summary>
        /// the private cost function used in the SmithWatermanGotoh distance.
        /// </summary>
        AbstractSubstitutionCost dCostFunction;
        /// <summary>
        /// a constant for calculating the estimated timing cost.
        /// </summary>
        double estimatedTimingConstant = 4.5e-005F;
        /// <summary>
        /// the private cost function for affine gaps.
        /// </summary>
        AbstractAffineGapCost gGapFunction;
        /// <summary>
        /// private field for the maximum affine gap window size.
        /// </summary>
        int windowSize;

        /// <summary>
        /// gets the similarity of the two strings using Smith-Waterman-Gotoh distance.
        /// </summary>
        /// <param name="firstWord"></param>
        /// <param name="secondWord"></param>
        /// <returns>a value between 0-1 of the similarity</returns>
        public override double GetSimilarity(string firstWord, string secondWord) {
            if ((firstWord != null) && (secondWord != null)) {
                double smithWatermanGotoh = GetUnnormalisedSimilarity(firstWord, secondWord);
                double maxValue = Math.Min(firstWord.Length, secondWord.Length);
                if (dCostFunction.MaxCost > -gGapFunction.MaxCost) {
                    maxValue *= dCostFunction.MaxCost;
                }
                else {
                    maxValue *= (-gGapFunction.MaxCost);
                }
                if (maxValue == defaultMismatchScore) {
                    return defaultPerfectScore;
                }
                else {
                    return smithWatermanGotoh / maxValue;
                }
            }
            return defaultMismatchScore;
        }

        /// <summary> gets a div class xhtml similarity explaining the operation of the metric.</summary>
        /// <param name="firstWord">string 1</param>
        /// <param name="secondWord">string 2</param>
        /// <returns> a div class html section detailing the metric operation.</returns>
        public override string GetSimilarityExplained(string firstWord, string secondWord) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// gets the estimated time in milliseconds it takes to perform a similarity timing.
        /// </summary>
        /// <param name="firstWord"></param>
        /// <param name="secondWord"></param>
        /// <returns>the estimated time in milliseconds taken to perform the similarity measure</returns>
        public override double GetSimilarityTimingEstimated(string firstWord, string secondWord) {
            if ((firstWord != null) && (secondWord != null)) {
                double firstLength = firstWord.Length;
                double secondLength = secondWord.Length;
                return
                    (firstLength * secondLength * windowSize + firstLength * secondLength * windowSize) *
                    estimatedTimingConstant;
            }
            return 0.0;
        }

        /// <summary> 
        /// gets the un-normalised similarity measure of the metric for the given strings.</summary>
        /// <param name="firstWord"></param>
        /// <param name="secondWord"></param>
        /// <returns> returns the score of the similarity measure (un-normalised)</returns>
        public override double GetUnnormalisedSimilarity(string firstWord, string secondWord) {
            if ((firstWord != null) && (secondWord != null)) {
                int n = firstWord.Length;
                int m = secondWord.Length;
                // check for zero length input
                if (n == 0) {
                    return m;
                }
                if (m == 0) {
                    return n;
                }
                double[][] d = new double[n][];
                for (int i = 0; i < n; i++) {
                    d[i] = new double[m];
                }
                //process first row and column first as no need to consider previous rows/columns
                double maxSoFar = 0.0;
                for (int i = 0; i < n; i++) {
                    // get the substution cost
                    double cost = dCostFunction.GetCost(firstWord, i, secondWord, 0);
                    if (i == 0) {
                        d[0][0] = Math.Max(defaultMismatchScore, cost);
                    }
                    else {
                        double maxGapCost = defaultMismatchScore;
                        int windowStart = i - windowSize;
                        if (windowStart < 1) {
                            windowStart = 1;
                        }
                        for (int k = windowStart; k < i; k++) {
                            maxGapCost = Math.Max(maxGapCost, d[i - k][0] - gGapFunction.GetCost(firstWord, i - k, i));
                        }

                        d[i][0] = MathFunctions.MaxOf3(defaultMismatchScore, maxGapCost, cost);
                    }
                    //update max possible if available
                    if (d[i][0] > maxSoFar) {
                        maxSoFar = d[i][0];
                    }
                }

                for (int j = 0; j < m; j++) {
                    // get the substution cost
                    double cost = dCostFunction.GetCost(firstWord, 0, secondWord, j);
                    if (j == 0) {
                        d[0][0] = Math.Max(defaultMismatchScore, cost);
                    }
                    else {
                        double maxGapCost = defaultMismatchScore;
                        int windowStart = j - windowSize;
                        if (windowStart < 1) {
                            windowStart = 1;
                        }
                        for (int k = windowStart; k < j; k++) {
                            maxGapCost = Math.Max(maxGapCost, d[0][j - k] - gGapFunction.GetCost(secondWord, j - k, j));
                        }

                        d[0][j] = MathFunctions.MaxOf3(defaultMismatchScore, maxGapCost, cost);
                    }
                    //update max possible if available
                    if (d[0][j] > maxSoFar) {
                        maxSoFar = d[0][j];
                    }
                }

                // cycle through rest of table filling values from the lowest cost value of the three part cost function
                for (int i = 1; i < n; i++) {
                    for (int j = 1; j < m; j++) {
                        // get the substution cost
                        double cost = dCostFunction.GetCost(firstWord, i, secondWord, j);
                        // find lowest cost at point from three possible
                        double maxGapCost1 = defaultMismatchScore;
                        double maxGapCost2 = defaultMismatchScore;
                        int windowStart = i - windowSize;
                        if (windowStart < 1) {
                            windowStart = 1;
                        }
                        for (int k = windowStart; k < i; k++) {
                            maxGapCost1 = Math.Max(maxGapCost1, d[i - k][j] - gGapFunction.GetCost(firstWord, i - k, i));
                        }

                        windowStart = j - windowSize;
                        if (windowStart < 1) {
                            windowStart = 1;
                        }
                        for (int k = windowStart; k < j; k++) {
                            maxGapCost2 = Math.Max(maxGapCost2, d[i][j - k] - gGapFunction.GetCost(secondWord, j - k, j));
                        }

                        d[i][j] = MathFunctions.MaxOf4(defaultMismatchScore, maxGapCost1, maxGapCost2, d[i - 1][j - 1] + cost);
                        if (d[i][j] > maxSoFar) {
                            maxSoFar = d[i][j];
                        }
                    }
                }

                // return max value within matrix as holds the maximum edit score
                return maxSoFar;
            }
            return 0.0;
        }

        /// <summary>
        /// get the d(i,j) cost function.
        /// </summary>
        public AbstractSubstitutionCost DCostFunction { get { return dCostFunction; } set { dCostFunction = value; } }

        /// <summary>
        /// get the g gap cost function.
        /// </summary>
        public AbstractAffineGapCost GGapFunction { get { return gGapFunction; } set { gGapFunction = value; } }

        /// <summary>
        /// returns the long string identifier for the metric.
        /// </summary>
        public override string LongDescriptionString {
            get {
                return
                    "Implements the Smith-Waterman-Gotoh algorithm with a windowed affine gap providing a similarity measure between two string";
            }
        }

        /// <summary>
        /// returns the string identifier for the metric.
        /// </summary>
        public override string ShortDescriptionString { get { return "SmithWatermanGotohWindowedAffine"; } }
    }
}