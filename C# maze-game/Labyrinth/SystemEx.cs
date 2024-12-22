using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth
{
    internal static class SystemEx
    {
        /// <summary>
        /// 跨线程操作控件
        /// </summary>
        /// <param name="con"></param>
        /// <param name="action"></param>
        public static void ExecBeginInvoke(this Control con, Action action)
        {
            if (action == null) return;
            if (con.InvokeRequired)
            {
                con.BeginInvoke(new Action(action));
            }
            else
            {
                action();
            }
        }
        /// <summary>
        /// 跨线程操作控件
        /// </summary>
        /// <param name="con"></param>
        /// <param name="action"></param>
        public static void ExecInvoke(this Control con, Action action)
        {
            if (action == null) return;
            if (con.InvokeRequired)
            {
                con.Invoke(new Action(action));
            }
            else
            {
                action();
            }
        }
    }
}
