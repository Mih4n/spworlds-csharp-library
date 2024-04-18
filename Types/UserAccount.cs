using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spworlds.Types
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class UserCard
    {
        public string id { get; set; }
        public string name { get; set; }
        public string number { get; set; }
        public int color { get; set; }
    }

    public class City
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int x { get; set; }
        public int z { get; set; }
        public bool isMayor { get; set; }
    }

    public class UserAccount
    {
        public string id { get; set; }
        public string username { get; set; }
        public string minecraftUUID { get; set; }
        public string status { get; set; }
        public List<object> roles { get; set; }
        public City city { get; set; }
        public List<UserCard> cards { get; set; }
        public DateTime createdAt { get; set; }
    }


}
