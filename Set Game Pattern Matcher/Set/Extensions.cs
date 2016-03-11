using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Set_Game_Pattern_Matcher
{
    public static class Extensions
    {
        public static int Area(this Rectangle r)
        {
            return r.Width * r.Height;
        }

        public static object Raise(this MulticastDelegate multicastDelegate, object sender, object e)
        {
            object retVal = null;

            MulticastDelegate threadSafeMulticastDelegate = multicastDelegate;
            if (threadSafeMulticastDelegate != null)
            {
                foreach (Delegate d in threadSafeMulticastDelegate.GetInvocationList())
                {
                    var synchronizeInvoke = d.Target as ISynchronizeInvoke;
                    if ((synchronizeInvoke != null) && synchronizeInvoke.InvokeRequired)
                    {
                        retVal = synchronizeInvoke.EndInvoke(synchronizeInvoke.BeginInvoke(d, new[] { sender, e }));
                    }
                    else
                    {
                        retVal = d.DynamicInvoke(new[] { sender, e });
                    }
                }
            }

            return retVal;
        }
    }
}
