using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiotoServerCuiMono.mioto_db
{
    public class date2row
    {
        //CREATE TABLE [date2row] ([date] INTEGER, [startrow] INTEGER, [endrow] INTEGER);
        public int date { get; set; }
        public long startrow { get; set; }
        public long endrow { get; set; }
    }
}
