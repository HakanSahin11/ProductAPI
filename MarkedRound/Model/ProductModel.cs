using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketRound.Model
{
    public class ProductModel
    {
        public ProductModel(ObjectId? id, string pictureId, ObjectId? sellerId, ObjectId? commentId, int price, string description, string title, List<string> tags, string salt)
        {
            _id = id;
            this.pictureId = pictureId;
            this.sellerId = sellerId;
            this.commentId = commentId;
            this.price = price;
            this.description = description;
            this.title = title;
            this.tags = tags;
            this.salt = salt;
        }

        public ObjectId? _id { get; set; }
        public string pictureId { get; set; }
        public ObjectId? sellerId { get; set; }
        public ObjectId? commentId { get; set; }
        public int price { get; set; }
        public string description { get; set; }
        public string title { get; set; }
        public List<string> tags { get; set; }
        public string salt { get; set; }
    }
}
