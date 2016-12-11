﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using ARIMA.timeseries.math;
using ARIMA.timeseries.models;
using MathNet.Numerics.LinearAlgebra;

namespace ARIMA.timeseries.stats
{
    // Augmented Dickey-Fuller Test
    public class ADF
    {

        Dictionary<int, Dictionary<int, double>> TCritcalVals;

        public ADF()
        {
            populateCritVals(ref TCritcalVals);
        }

        public bool adfuller(Vector<double> X, Vector<double> Y, double sig)
        {
            Vector<double> Xdiff = Vector<double>.Build.DenseOfArray(math.linalg.ArrayManipulation.diff(X.ToArray()));
            Vector<double> Xlag = Vector<double>.Build.Dense(X.Count - 1);
            for (int i = 1; i < X.Count; i++)
            {
                Xlag[i - 1] = X[i];
            }
            Vector<double> Xlagdiff = Vector<double>.Build.DenseOfArray(math.linalg.ArrayManipulation.diff(Xlag.ToArray()));
            Matrix<double> Xcomb = Matrix<double>.Build.Dense(Xlag.Count, 2);

            Console.WriteLine(Xlag.Count);
            Console.WriteLine(Xlagdiff.Count);

            Xcomb[0, 0] = Xlag[0];
            Xcomb[0, 1] = 0;

            for (int i = 0; i < Xlagdiff.Count; i++)
            {
                Xcomb[i + 1, 0] = Xlag[i + 1];
                Xcomb[i + 1, 1] = Xlagdiff[i];
            }

            Vector<double> ytest = Vector<double>.Build.Dense(Xlag.Count);

            Y.CopySubVectorTo(ytest, 2, 0, Y.Count - 2);

            RegressionModel OLS_instance = new RegressionModel(Xcomb, ytest, "OLS");

            Matrix<double> design = OLS_instance.designMatrix(Xcomb);
            Vector<double> coeff = OLS_instance.fit(design, ytest);

            Console.WriteLine(OLS_instance.Rsquared(Xcomb, Y, coeff));

            double tstat = OLS_instance.testValue(Xcomb, Y, 1, coeff);
            Console.WriteLine(tstat);

            int abs50 = Math.Abs(X.Count - 50);
            int abs100 = Math.Abs(X.Count - 100);
            int abs200 = Math.Abs(X.Count - 200);

            int significance = (int)(sig * 100);

            int min = Math.Min(abs50, Math.Min(abs100, abs200));

            if (min == abs50)
            {
                if (min  < TCritcalVals[50][significance])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            } else if (min == abs100)
            {
                if (min < TCritcalVals[100][significance])
                {
                    return true;
                } else
                {
                    return false;
                }
            }
            else
            {
                if (min < TCritcalVals[200][significance])
                {
                    return true;
                } else
                {
                    return false;
                }
            }
        }

        public static void populateCritVals(ref Dictionary<int, Dictionary<int, double>> criticalvals)
        {
            // critical values hard coded in
            // from http://homes.chass.utoronto.ca/~floyd/statabs.pdf
            criticalvals = new Dictionary<int, Dictionary<int, double>>();
            int[] samplesizes = new int[] { 50, 100, 200 };
            int[] critpercents = new int[] { 1, 5, 10 };
            foreach (int s in samplesizes)
            {
                foreach (int p in critpercents)
                {
                    if (s == 50)
                    {
                        if (p == 10)
                        {
                            if (criticalvals.ContainsKey(s) == false)
                            {
                                criticalvals.Add(s, new Dictionary<int, double>());
                            }
                            criticalvals[s].Add(p, 3.28);
                        } else if (p == 5)
                        {
                            if (criticalvals.ContainsKey(s) == false)
                            {
                                criticalvals.Add(s, new Dictionary<int, double>());
                            }
                            criticalvals[s].Add(p, 3.67);
                        } else if (p == 1)
                        {
                            if (criticalvals.ContainsKey(s) == false)
                            {
                                criticalvals.Add(s, new Dictionary<int, double>());
                            }
                            criticalvals[s].Add(p, 4.32);
                        }
                    } else if (s == 100)
                    {
                        if (p == 10)
                        {
                            if (criticalvals.ContainsKey(s) == false)
                            {
                                criticalvals.Add(s, new Dictionary<int, double>());
                            }
                            criticalvals[s].Add(p, 3.03);
                        }
                        else if (p == 5)
                        {
                            if (criticalvals.ContainsKey(s) == false)
                            {
                                criticalvals.Add(s, new Dictionary<int, double>());
                            }
                            criticalvals[s].Add(p, 3.37);
                        }
                        else if (p == 1)
                        {
                            if (criticalvals.ContainsKey(s) == false)
                            {
                                criticalvals.Add(s, new Dictionary<int, double>());
                            }
                            criticalvals[s].Add(p, 4.07);
                        }
                    } else if (s == 200)
                    {
                        if (p == 10)
                        {
                            if (criticalvals.ContainsKey(s) == false)
                            {
                                criticalvals.Add(s, new Dictionary<int, double>());
                            }
                            criticalvals[s].Add(p, 3.02);
                        }
                        else if (p == 5)
                        {
                            if (criticalvals.ContainsKey(s) == false)
                            {
                                criticalvals.Add(s, new Dictionary<int, double>());
                            }
                            criticalvals[s].Add(p, 3.37);
                        }
                        else if (p == 1)
                        {
                            if (criticalvals.ContainsKey(s) == false)
                            {
                                criticalvals.Add(s, new Dictionary<int, double>());
                            }
                            criticalvals[s].Add(p, 4.00);
                        }
                    }
                }
            }
        }

    }
}
