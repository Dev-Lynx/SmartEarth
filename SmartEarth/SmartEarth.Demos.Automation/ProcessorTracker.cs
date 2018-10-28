using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

namespace SmartEarth.Demos.Automation
{
    public static class ProcessorTracker
    {
        #region Properties
        public static bool Tracking { get; private set; }
        public static Timer TrackTimer { get; private set; }
        public static Process TargetProcess { get; private set; }
        public static event EventHandler<double> NewValue;
        #endregion

        #region Methods

        public static void TrackProcess(Process process, int updateInterval = 1000)
        {
            StopTracking();

            if (process == null) return;

            TrackTimer = new Timer(updateInterval)
            {
                AutoReset = true,
                Enabled = true
            };

            UpdateInfo(null, null);

            TrackTimer.Elapsed += UpdateInfo;
            TrackTimer.Start();
            Tracking = true;
            
        }

        public static void StopTracking()
        {
            if (!Tracking) return;
            TrackTimer.Stop();
            Tracking = false;
        }

        static void UpdateInfo(object sender, ElapsedEventArgs e)
        {

        }

        #endregion
    }
}
