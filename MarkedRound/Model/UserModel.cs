using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketRound.Model
{
    public class UserModel
    {
        public UserModel(ObjectId id, string username, string password, string salt, string firstName, string lastName, double phoneNumber, string country, string city, string address, ObjectId?[] ongoingSales, int?[] salesHistory, int?[] reviews, List<DateTime> failedLoginAttempts, string loginBan, DateTime creationDate)
        {
            _id = id;
           this.username = username;
           this.password = password;
           this.salt = salt;
           this.firstName = firstName;
           this.lastName = lastName;
           this.phoneNumber = phoneNumber;
           this.country = country;
           this.city = city;
           this.address = address;
           this.ongoingSales = ongoingSales;
           this.salesHistory = salesHistory;
           this.reviews = reviews;
           this.failedLoginAttempts = failedLoginAttempts;
           this.loginBan = loginBan;
           this.creationDate = creationDate;
        }

        public ObjectId _id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string salt { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public double phoneNumber { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string address { get; set; }
        public ObjectId?[] ongoingSales { get; set; }
        public int?[] salesHistory { get; set; }
        public int?[] reviews { get; set; }
        public List<DateTime> failedLoginAttempts {get;set;}
        public string loginBan { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime creationDate { get; set; }
    }
}
