using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarkedRound.Model
{
    public class CommentModel
    {
        public CommentModel(ObjectId id, ObjectId productId, ObjectId sellerId, Comments[] comments)
        {
            _id = id;
            this.productId = productId;
            this.sellerId = sellerId;
            this.comments = comments;
        }

        public ObjectId _id { get; set; }
        public ObjectId productId { get; set; }
        public ObjectId sellerId { get; set; }
        public Comments[] comments { get; set; }
    }

    public class Comments
    {
        public Comments(ObjectId userId, string comment)
        {
            this.userId = userId;
            this.comment = comment;
        }

        public ObjectId userId { get; set; }
        public string comment { get; set; }
    }
}
