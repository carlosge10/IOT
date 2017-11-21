using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTDBSyncer
{
    public class Channel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string field1 { get; set; }
        public string field2 { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string last_entry_id { get; set; }

        public Channel() {
        }
    }
}
