using DBSyncer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTDBSyncer
{
    public class RootElement
    {
        public Channel channel { get; set; }
        public List<Entry> feeds { get; set; }
        public RootElement() { }
    }
}
