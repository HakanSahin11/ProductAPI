using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketRound.Model
{
    public class HashSalt
    {
        public HashSalt(string pass, string salt)
        {
            Pass = pass;
            Salt = salt;
        }

        public string Pass { get; set; }
        public string Salt { get; set; }
    }
}
