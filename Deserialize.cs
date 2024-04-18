using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spworlds
{
    internal class Deserialize
    {
        public static T DeserializeObject<T>(string data) where T : class
        {
            T objectToReturn = JsonConvert.DeserializeObject<T>(data);
            if (objectToReturn == null)
                throw new Exception("User not player.");
            else
                return objectToReturn;
            
        }
    }
}
