using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DinoCap.PortV2
{
    /// <summary>
    /// DEBUG_ONLY transition/event telemetry for Task027K4.2.
    /// Reads state and records events only; it never mutates gameplay state.
    /// </summary>
    public static class F2DebugTelemetry
    {
        const int Capacity=300;
        static readonly Queue<string> events=new Queue<string>(Capacity);

        public static void Log(string eventName,string details)
        {
            string line=
                Time.realtimeSinceStartup.ToString("0.000")+
                ","+eventName+
                ","+(details ?? "").Replace("\n"," ").Replace("\r"," ");

            if(events.Count>=Capacity)
                events.Dequeue();
            events.Enqueue(line);
        }

        public static void DumpToConsole()
        {
            StringBuilder b=new StringBuilder();
            b.AppendLine("[DC1-F2-TELEMETRY-BEGIN]");
            b.AppendLine("time,event,details");
            foreach(string line in events)
                b.AppendLine(line);
            b.AppendLine("[DC1-F2-TELEMETRY-END]");
            Debug.Log(b.ToString());
        }

        public static void Clear()
        {
            events.Clear();
        }
    }
}
