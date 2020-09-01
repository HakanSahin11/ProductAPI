using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarkedRound.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using static MarketRound.Controllers.MainController;


namespace MarkedRound.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        public List<CommentModel> Comments(string _id)
        {
            IMongoQueryable<CommentModel> usageQuery;
            var liste = new List<CommentModel>();
            //Either gets all comments or just 1, depending on if _id is defined

            if (_id == null)
            {
                usageQuery = from c in client.GetCollection<CommentModel>("comments").AsQueryable()
                             select c;
            }
            else
            {
                usageQuery = from c in client.GetCollection<CommentModel>("comments").AsQueryable()
                             where c._id == ObjectId.Parse(_id)
                             select c;
            }
            liste.AddRange(usageQuery);
            return liste;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Ok(Comments(null));
        }

    }
}
