using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarkedRound.Model
{
    public class CreateProductModel
    {
        public CreateProductModel(string username, string pictureId, int price, string title, string description, List<string> tags)
        {
            this.username = username;
            this.pictureId = pictureId;
            this.price = price;
            this.title = title;
            this.description = description;
            this.tags = tags;
        }

        public string username { get; set; }
        public string pictureId {get;set;}
        public int price { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public List<string> tags { get; set; }
    }
}
