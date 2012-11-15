using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WiimoteLib;

namespace gWiiFitBit.Lib
{
    public class WiiBoard : IDisposable
    {
        const int weightsToTakeBeforeMeasurement = 150;
        const int minWeightKgThreshold = 5;

        List<float> weights;
        WiimoteCollection controllers;

        public delegate void weightChanged(Weight weight);
        public event weightChanged OnWeightChanged;

        private float TotalWeight
        {
            get
            {
                //Remove first 60% of weights as when stepping on they are inaccurate
                var prunedWeights = (from w in weights select w).Skip(Convert.ToInt32(weightsToTakeBeforeMeasurement * 0.6)).ToList();
                //Remove any weights more than 5% from average
                prunedWeights = (from w in prunedWeights where Math.Abs(w - prunedWeights.Average()) < prunedWeights.Average() * 0.5 select w).ToList();
                return prunedWeights.Average();
            }
        }

        public void Connect()
        {
            weights = new List<float>();
            controllers = new WiimoteCollection();
            while (controllers.Count == 0)
            {
                try
                {
                    controllers.FindAllWiimotes();
                }
                catch (WiimoteNotFoundException ex)
                {
                    //Swallow ad keep trying
                }
                Thread.Sleep(50);
            }
            foreach (var c in controllers)
            {
                c.WiimoteChanged += stateChange;
                c.Connect();
                c.SetLEDs(1);
            }
        }

        public void Disconnect()
        {
            foreach (var c in controllers)
                c.Disconnect();
        }

        private void stateChange(object sender, WiimoteChangedEventArgs e)
        {
            if (e.WiimoteState.Disconected)
            {
                Connect();
                return;
            }
            if (e.WiimoteState.ExtensionType == ExtensionType.BalanceBoard)
            {
                lock (weights)
                {
                    if (e.WiimoteState.BalanceBoardState.WeightKg <= minWeightKgThreshold)
                        weights.Clear();
                    if (e.WiimoteState.BalanceBoardState.WeightKg > minWeightKgThreshold && weights.Count < weightsToTakeBeforeMeasurement)
                        weights.Add(e.WiimoteState.BalanceBoardState.WeightKg);
                    if (weights.Count == weightsToTakeBeforeMeasurement)
                        OnWeightChanged(new Weight(TotalWeight));
                }
            }
        }

        public void Dispose()
        {
            this.Disconnect();
        }
    }

}
