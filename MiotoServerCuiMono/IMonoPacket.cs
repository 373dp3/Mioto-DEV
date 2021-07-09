using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServer
{
    interface IMonoPacket
    {

        /// <summary>
        /// Monostick用パケットの解釈
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ofs">失敗時は位置を変更しない</param>
        /// <returns>失敗時にfalse</returns>
        bool parse(string msg, ref int ofs);

        string toCsv();

    }
}
