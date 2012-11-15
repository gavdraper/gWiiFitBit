using System;
using System.Collections.Generic;
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
        volatile bool measuring;
        WiimoteCollection controllers;


        public delegate void weightChanged(Weight weight);
        public event weightChanged OnWeightChanged;

        private float TotalWeight
        {
            get
            {
                var tmpWeights = (from w in weights select w).ToList();
                for (int i = 0; i < 120; i++)
                    tmpWeights.Remove(tmpWeights[0]);
                var unculledAverage = tmpWeights.Average();
                var threshold = (unculledAverage / 100) * 5;
                var dodgyWeight = (from w in tmpWeights where Math.Abs(unculledAverage - w) > threshold select w).ToList();
                var goodAverage = (from w in tmpWeights where !dodgyWeight.Contains(w) select w).Average();
                return goodAverage;
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
