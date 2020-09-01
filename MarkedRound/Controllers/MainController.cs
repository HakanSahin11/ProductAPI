using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading.Tasks;
using MarketRound.HelpClasses;
using MarketRound.Model;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Net;
using MongoDB.Bson.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Globalization;

using static MarketRound.HelpClasses.UpdateUser;
using static MarketRound.HelpClasses.CreateNew;
using static MarketRound.HelpClasses.LoginClasses;
using static MarkedRound.Controllers.ProductController;

using MarkedRound.Model;
using MarkedRound.HelpClasses;
using System.Text;

//TODO
//isnullorempty
//regex for security?
//Try catches?
//___________________________________________________________________________________________

/*Notes:
 * Update Method:  ChangeUserInput(id, Document, StrInput, IntInput, collection)
 * Exempel: ChangeUserInput(1, "PhoneNumber", null, 6666, "Users");
 * 
 * Add new User method: CreateNewSecton(Collection, UserModel, DBClient);
 * 
 *New changes after feedback
 *Change db to have login info is specific collection
 *Fix models capitalization
 *Model annotations
 *Salt collection?
 *Token function
 *Remember med function
 *Forgot password aka mail service
 *
 *
 */
namespace MarketRound.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        //Database Connection
        // Local:
        public static IMongoDatabase client = new MongoClient($"mongodb://{"adminUser"}:{"silvereye"}@localhost:27017").GetDatabase("silkevejen");

        public static readonly string publicKey = "4556484548529632";
        public static readonly string collection = "users";
        public static async Task<List<UserModel>> GetAllUsers(string username, string section)
       // public static List<UserModel> GetAllUsers(string username, string section)
        {
            //Collection Query
            try
            {
                var ListOfUsers = new List<UserModel>();
                
                await Task.Run(() =>
                {
                    IMongoQueryable<UserModel> usageQuery;
                    if (username == null)
                    {
                        //Get all users if no id is specified
                        usageQuery = from c in client.GetCollection<UserModel>(section).AsQueryable()
                                     select c;
                    }
                    else
                    {
                        //Get a specific user 
                        usageQuery = from c in client.GetCollection<UserModel>(section).AsQueryable()
                                     where c.username == username
                                     select c;
                    }
                    Encryptor _encrypter = new Encryptor(publicKey);

                    foreach (var item in usageQuery)
                    {
                        // Decrypts user information to readable text
                        var decryptedUsers = _encrypter.ObjectToEncryptDecrypt(item, item.salt, "Decrypt");
                      ListOfUsers.Add(decryptedUsers);

                    }
                });
                
                return ListOfUsers;
            }
            catch
            {
                return null;
            }
        }

        // GET: api/<MainController>
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(GetAllUsers(null, collection).Result);
        }

        // GET api/<MainController>/5
        [HttpGet("{username}")]
        public ActionResult Get(string username)
        {
            return Ok(GetAllUsers(username, collection).Result);
        }

        // POST api/<MainController>
        [HttpPost]
        public ActionResult Post([FromBody] Usermodel_Post user)
        {
            switch (user.section)
            {
                case "CreateNew":
                    //Used to creation of new users
                    var result = CreateNewSecton(collection, user.userContent, client);
                    if (result != "Success")
                    {
                        return Ok(result);
                    }
                    return Ok(result);
                case "login":
                    // Used for login authentication, which uses both salting and decryption of encrypted hashvalue 
                    // hashvalue will still return hashed, but it gets encrypted, so we also decrypt it to the original hash value
                    // Example: https://gyazo.com/ceb108a3cf4755b641828e5d445b152c
                    var dbUser = GetAllUsers(user.userContent.username, collection);
                    if (dbUser.Result.Count > 0)
                        return Ok(login(user.userContent, dbUser.Result));
                    break;
                case "GetAllProductsFromUser":
                    // Returns list of all user's products which they have listed as ongoing sale by using their username, 
                    // then converts it to _id, then uses that to match the product creator by _id
                    dbUser = GetAllUsers(user.userContent.username, collection);
                    List<ProductModel> ongoingSales = new List<ProductModel>();
                    dbUser.Result[0].ongoingSales.ToList().ForEach(x => ongoingSales.Add(Products(null, x, null)[0]));
                    return Ok(ongoingSales);
            }
            return Ok("Error! Null / Empty / Non existing section detected");
        }
        
        // PUT api/<MainController>/5
        [HttpPut]
        public ActionResult Put([FromBody] ChangeUserModel changeUser)
        {
            //data validation
            if (changeUser.key.Length != changeUser.change.Length || changeUser.dataType.Length != changeUser.key.Length)
                return Ok("Missing Input");
            try
            {
                //change user section
                var DBUser = GetAllUsers(changeUser.username, collection).Result;
                if (DBUser.Count > 0)
                {
                    string saltKey = DBUser[0].salt;
                    var result = false;
                    for (int i = 0; i < changeUser.key.Length; i++)
                    {
                        switch (changeUser.dataType[i])
                        {
                            case "string":
                                result = ChangeUserInput(changeUser.username, changeUser.collection, changeUser.key[i], changeUser.change[i], null, null, null, saltKey, null, "ChangeUserStr");
                                break;
                            case "int":
                                result = ChangeUserInput(changeUser.username, changeUser.collection, changeUser.key[i], null, Convert.ToInt32(changeUser.change[i]), null, null, saltKey, null, "ChangeUserInt");
                                break;
                            default:
                                break;
                        }
                    }
                    return Ok(result);
                }
                return Ok("Error! Non existing user detected!");
            }
            catch
            {
                return Ok("Error");
            }
        }

        // DELETE api/<MainController>/5
        [HttpDelete("{username}")]
        public ActionResult Delete(string username)
        {
            try {
                //delete user
                var DBcollection = client.GetCollection<BsonDocument>(collection);
                var deletefilter = Builders<BsonDocument>.Filter.Eq("username", username);

                DBcollection.DeleteOne(deletefilter);
                return Ok("Success");
            }
            catch
            {
                return Ok("Error, delete action could not be completed");
            }
        }
    }
}
