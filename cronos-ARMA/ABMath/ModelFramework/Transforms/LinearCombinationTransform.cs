﻿#region License Info
//Component of Cronos Package, http://www.codeplex.com/cronos
//Copyright (C) 2009 Anthony Brockwell

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License, or (at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
#endregion

using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using ABMath.ModelFramework.Data;

namespace ABMath.ModelFramework.Transforms
{
    [Serializable]
    public class LinearCombinationTransform : TimeSeriesTransformation
    {
        [NonSerialized]
        protected TimeSeries combination;

        [CategoryAttribute("Parameter"), Description("If true, uses timestamps of first input, other inputs are regarded as step functions and overrides RequiresExactTimeMatch parameter")]
        public bool UseTimesFromFirst { get; set; }
        [CategoryAttribute("Parameter"), Description("If true, only includes points where all inputs have values, if false, regards all inputs as step functions")]
        public bool RequiresExactTimeMatch { get; set; }
        [CategoryAttribute("Parameter"), Description("Coefficients in linear combination, increase the array size to allow for more than two inputs")]
        public double[] Coefficients { get; set; }

        public LinearCombinationTransform()
        {
            Coefficients = new[] {1.0, -1.0};
        }

        public override void Recompute()
        {
            IsValid = false;
            combination = null;
            if (GetInputType(-1) != InputType.UnivariateTS)
                return;

            // hacky fix to use old saved files
            if (Coefficients == null)
                Coefficients = new[] { 1.0, -1.0 };

            int n = Coefficients.Length;

            combination = null;

            var series = GetInputBundle();
            if (series.Count != Coefficients.Length)
                return;
                
            // now we should be able to do something
            int[] ts = new int[n];  // index of next one in each ts to check
            int[] Ts = new int[n];  // counts

            for (int i=0 ; i<n ; ++i)
                Ts[i] = series[i].Count;

            combination = new TimeSeries();
            if (n == 2)
            combination.Title = string.Format("{0:0.0}x{1} {2} {3:0.0}x{4}",
                                              Coefficients[0], series[0].Title,
                                              Coefficients[1] >= 0 ? '+' : '-',
                                              Math.Abs(Coefficients[1]), series[1].Title);
            else
               combination.Title = "Linear Comb.";


            if (UseTimesFromFirst)
            {
                for (int t = 0 ; t<series[0].Count ; ++t)
                {
                    double sum = series[0][t]*Coefficients[0];
                    var tstamp = series[0].TimeStamp(t);
                    for (int i = 1; i < n; ++i)
                        sum += series[i].ValueAtTime(tstamp)*Coefficients[i];
                    combination.Add(tstamp, sum, true);
                }
            }
            else // it's either require exact time matching or else use step functions
            {
                bool done = false;
                while (!done)
                {
                    var dts = new DateTime[n];

                    bool allDatesSame = true;
                    int argmin = 0;
                    var minval = DateTime.MaxValue;
                    for (int i = 0; i < n; ++i)
                    {
                        dts[i] = ts[i] < Ts[i] ? series[i].TimeStamp(ts[i]) : DateTime.MaxValue;
                        if (dts[i] < minval)
                        {
                            argmin = i;
                            minval = dts[i];
                        }
                        if (i > 0)
                            allDatesSame &= dts[i] == dts[i - 1];
                    }

                    if (allDatesSame)
                    {
                        double sum = 0.0;
                        // got one!
                        for (int i = 0; i < n; ++i)
                            sum += series[i][ts[i]]*Coefficients[i];
                        combination.Add(series[0].TimeStamp(ts[0]), sum, false);
                        for (int i = 0; i < n; ++i)
                            ++ts[i];
                    }
                    else if (!RequiresExactTimeMatch)
                    {
                        // evaluate at the minimum and advance
                        double sum = 0.0;
                        bool valid = true;
                        for (int i = 0; i < n; ++i)
                            if (ts[i] < Ts[i] && series[i].TimeStamp(ts[i]) <= minval)
                                sum += Coefficients[i]*series[i][ts[i]];
                            else
                            {
                                if (ts[i] > 0)
                                    sum += Coefficients[i]*series[i][ts[i] - 1];
                                else
                                    valid = false;
                            }
                        if (valid)
                            combination.Add(minval, sum, false);
                        for (int i = 0; i < n; ++i)
                            if (ts[i] < Ts[i])
                                if (series[i].TimeStamp(ts[i]) <= minval)
                                    ++ts[i];
                    }
                    else if (ts[argmin] < Ts[argmin])
                        ++ts[argmin];

                    done = true;
                    for (int i = 0; i < n; ++i)
                        done &= ts[i] == Ts[i];
                }
            }

            if (combination.Count == 0)
                combination = null;
            else
                IsValid = true;
        }

        public override string GetDescription()
        {
            return "Linear combination of two or more time series.";
        }

        public override string GetShortDescription()
        {
            return "aX+bY";
        }

        //public override Icon GetIcon()
        //{
        //    return null;
        //}

        public override int NumInputs()
        {
            if (Coefficients == null)
                Coefficients = new[] { 1.0, -1.0 };
            return Coefficients.Length;
        }

        public override int NumOutputs()
        {
            return 1;
        }

        public override object GetOutput(int socket)
        {
            if (socket != 0)
                throw new SocketException();
            return combination;
        }

        public override string GetInputName(int index)
        {
            var s = string.Format("Time Series #{0}", index + 1);
            return s;
        }

        public override string GetOutputName(int index)
        {
            if (index == 0)
                return "Time Series";
            throw new ArgumentException("Invalid index.");
        }

        public override List<Type> GetAllowedInputTypesFor(int socket)
        {
            if (socket >= NumInputs())
                throw new SocketException();
            return new List<Type> { typeof(TimeSeries) };
        }

        public override List<Type> GetOutputTypesFor(int socket)
        {
            if (socket != 0)
                throw new SocketException();
            return new List<Type> { typeof(TimeSeries) };
        }

    }
}