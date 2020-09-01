using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketRound.Model
{
    public class ChangeUserModel
    {
        public ChangeUserModel(string username, string collection, string[] key, string[] change, string[] dataType)
        {
            this.username = username;
            this.collection = collection;
            this.key = key;
            this.change = change;
            this.dataType = dataType;
        }

        public string username {get;set;}
        public string collection { get; set; }

        public string[] key { get; set; }
        public string[] change { get; set; }
        public string[] dataType { get; set; }
    }
}
