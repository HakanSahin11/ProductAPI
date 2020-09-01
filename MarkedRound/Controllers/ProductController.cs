using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MarketRound.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MarkedRound.Model;
using MarketRound.HelpClasses;
using static MarketRound.Controllers.MainController;
using static MarketRound.HelpClasses.CreateNew;
using MarkedRound.HelpClasses;

namespace MarkedRound.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        public static readonly string collection = "products";
        public static List<ProductModel> Products(string _id, ObjectId? sellerId, ObjectId? commentId)
        {
            IMongoQueryable<ProductModel> usageQuery;
            var liste = new List<ProductModel>();
            //Either gets all products or just 1, depending on if _id is defined
            if(sellerId != null && commentId != null)
            {
                usageQuery = from c in client.GetCollection<ProductModel>("products").AsQueryable()
                             where c.sellerId == sellerId && c.commentId == commentId
                             select c;
            }
            else if (_id == null)
            {
                usageQuery = from c in client.GetCollection<ProductModel>("products").AsQueryable()
                             select c;
            }
            else
            {
                usageQuery = from c in client.GetCollection<ProductModel>("products").AsQueryable()
                             where c._id == ObjectId.Parse(_id)
                             select c;
            }

            Encryptor _encrypter = new Encryptor(publicKey);

            foreach (var item in usageQuery)
            {
                // Decrypts user information to readable text
                var decryptedUsers = _encrypter.ProductObjectToEncryptDecrypt(item, item.salt, "Decrypt");
                liste.Add(decryptedUsers);
            }
            return liste;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Ok(Products(null, null, null));
        }

        [HttpPost]
        public ActionResult Post([FromBody] ProductPostModel content)
        {
            switch (content.section)
            {
                case "Search":
                    // Example: https://gyazo.com/f6e11a26e75e07d8c32e3607ef6ee63c
                    var products = Products(null, null, null);
                    var Matches = products.Where(x => x.title.Contains(content.input.search, StringComparison.CurrentCultureIgnoreCase)
                                         || x.description.Contains(content.input.search, StringComparison.CurrentCultureIgnoreCase)
                                         || x.tags.Contains(content.input.search, StringComparer.OrdinalIgnoreCase)).ToList();
                    if(content.AdvancedSearch != null)
                    {
                        Matches = AdvancedSearch(Matches, content.AdvancedSearch);
                    }
                    return Ok(Matches);
                case "createOffer":
                    ObjectId sellerId = GetAllUsers(content.createProduct.username, "users").Result[0]._id;
                    createOffer(content.createProduct, client, sellerId);
                    break;
                case "Purchase":

                    break;
            }
            return Ok();
        }
        #region advancedSearch
        public List<ProductModel> AdvancedSearch(List<ProductModel> list, AdvancedSearchModel SearchOptions)
        {
            //Alternatively use a switch?

            //add search options here
            if(SearchOptions.PriceRange == true)
            {
                list.RemoveAll(x => x.price > SearchOptions.PriceRangeHigh || x.price < SearchOptions.PriceRangeLow);
            }
            if(SearchOptions.HighToLow == true)
            {
                list = list.OrderByDescending(x => x.price).ToList();
            }
            if(SearchOptions.LowToHigh == true)
            {
                list = list.OrderBy(x => x.price).ToList();
            }
            if(SearchOptions.AlphabeticalOrder == true)
            {
                list = list.OrderBy(x => x.title).ToList();
            }
            if (SearchOptions.ShowNewest)
            {
                //Sell date start
            }

            return list;
        }
        #endregion

        [HttpDelete("{_id}")]
        public ActionResult Delete( string _id)
        {
            try
            {
                var deleteId = ObjectId.Parse(_id);
                //delete product
                var DBcollection = client.GetCollection<BsonDocument>(collection);
                var deletefilter = Builders<BsonDocument>.Filter.Eq("_id", deleteId);

                DBcollection.DeleteOne(deletefilter);
                return Ok();
            }
            catch 
            {
                return Ok("Error");
            }
        }
    }
}
