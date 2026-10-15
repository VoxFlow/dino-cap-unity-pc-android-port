using System.Collections.Generic;
using UnityEngine;

namespace DinoCap.PortV2
{
    public static class NativeJungleHud
    {
        public static readonly Rect KillIcon=new Rect(450,9,91,72);
        public static readonly Rect TimeLabel=new Rect(715,15,200,60);
        public struct Digit {public char value;public Rect rect;}
        static readonly float[] widths={40,23,36,36,40,37,38,33,38,39};
        public static List<Digit> KillDigits(int count)
        {
            var result=new List<Digit>();float x=539;
            foreach(char c in count.ToString(System.Globalization.CultureInfo.InvariantCulture))
            {
                float width=widths[c-'0'];
                result.Add(new Digit {value=c,rect=new Rect(x,17,width,53)});
                x+=width-5;
            }
            return result;
        }
        //15F84 increments decimal second/minute fields, no hours cap.
        public static string TimeText(int elapsed)
        {
            int minutes=elapsed/60,seconds=elapsed%60;
            return (minutes/10)+" "+(minutes%10)+" : "+(seconds/10)+" "+(seconds%10);
        }
    }
}
