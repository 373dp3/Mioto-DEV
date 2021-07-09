/*
Copyright (c) 2018 Toshiaki Minami
Released under the MIT license
https://opensource.org/licenses/mit-license.php
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiotoServer
{
    public class Program
    {
        public static void d(string msg)
        {
            //Console.WriteLine(msg);
            MiotoServerWrapper.d(msg);
            Trace.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + " " + msg);
        }
    }
}
