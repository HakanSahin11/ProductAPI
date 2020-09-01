using MarkedRound.HelpClasses;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MarketRound.Controllers.MainController;

namespace MarketRound.HelpClasses
{
    public class UpdateUser
    {
        public static bool ChangeUserInput(string username, string collection, string Document, string StrInput, int? IntInput, List<DateTime> failedLoginAttemps, DateTime? loginBan, string saltKey, ObjectId? _id, string Case)
        {
            try
            {
                //Gets database then selects the collection called
                IMongoDatabase client = new MongoClient($"mongodb://{"adminUser"}:{"silvereye"}@localhost:27017").GetDatabase("silkevejen");
                
                //Collection to edit
                var BsonColl = client.GetCollection<BsonDocument>(collection);

                //Filter for changes
                FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("username", username);
                
                UpdateDefinition<BsonDocument> update;
                Encryptor _encryptor = new Encryptor(publicKey);

                switch (Case)
                {
                    case "ChangeUserStr":
                        // Used for when a string value is given
                        var taskStr = Task.Run(() => _encryptor.Encrypt(Encoding.UTF8.GetBytes(StrInput), saltKey));
                        Task.WaitAll();
                        update = Builders<BsonDocument>.Update.Set(Document, Convert.ToBase64String(taskStr.Result));
                    break;

                    case "ChangeUserInt":
                        // Used for when an int value is given
                        update = Builders<BsonDocument>.Update.Set(Document, IntInput);
                    break;

                    case "LoginBan":
                        //        ChangeUserInput(user.username, "Users", "loginBan", null, null, null, DateTime.Now.AddMinutes(5), null, null, "LoginBan");
                        update = Builders<BsonDocument>.Update.Set(Document, loginBan.ToString());
                        break;

                    case "failedLoginTries":
                        // Used for failed login attempts
                        update = Builders<BsonDocument>.Update.Set(Document, failedLoginAttemps);
                        break;

                    case "ChangeProductCommentId":
                        //Used for when creating new products
                        filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(username));
                        update = Builders<BsonDocument>.Update.Set(Document, _id);
                        break;
                    case "AddProductToUser":
                        filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(username));
                        update = Builders<BsonDocument>.Update.Push(Document, _id);
                        break;

                    default:
                        return false;
                }
                BsonColl.UpdateOne(filter, update);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
