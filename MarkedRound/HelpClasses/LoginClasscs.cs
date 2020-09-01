using MarketRound.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MarketRound.HelpClasses.UpdateUser;
using static MarketRound.HelpClasses.HashingSalting;

namespace MarketRound.HelpClasses
{
    public class LoginClasses
    {
        public static string login(UserModel user, List<UserModel> dbUser)
        {
            try
            {
                //password hashing
                var HashSaltBase64 = HashSaltValues(user.password, Convert.FromBase64String(dbUser[0].salt));

                //Checks to see if the username returned an existing user
                if (dbUser.Count != 0)
                {
                    //Checks failed login attemts for the past 30 minutes, if its above 3, it denies the login
                    dbUser[0].failedLoginAttempts.Add(DateTime.UtcNow);
                    var minDate = DateTime.UtcNow.Subtract(new TimeSpan(0, 30, 0));
                    var sortedList = dbUser[0].failedLoginAttempts.Where(x => x >= minDate).ToList();

                    //Checks to see if the format is compatible (in case user has never been banned)
                    DateTime dateTimeVal;
                    bool dateTimeResult = DateTime.TryParse(dbUser[0].loginBan, out dateTimeVal);
                    if (dateTimeResult && dateTimeVal >= DateTime.UtcNow)
                    {
                        //Cheks for bans
                        var banTimer = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(dbUser[0].loginBan), TimeZoneInfo.Local);
                        return $"Error! Account Temporary Locked until: {banTimer}";
                    }
                    else
                    {
                        if (sortedList.Count >= 3)
                        {
                            //deny / ban
                            ChangeUserInput(user.username, "Users", "loginBan", null, null, null, DateTime.Now.AddMinutes(5), null, null, "LoginBan");
                            return $"Error! Account Temporary Locked until: {TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(dbUser[0].loginBan), TimeZoneInfo.Local)}!";
                        }
                        else
                        {
                            //Matches the hashed password value with the one saved in the DB
                            if (dbUser[0].password == HashSaltBase64.Pass)
                            {
                                return "Successful Login";
                            }
                            else
                            {
                                // Adds a failed login attemp to the DB if the user exists
                                ChangeUserInput(user.username, "Users", "failedLoginTries", null, null, dbUser[0].failedLoginAttempts, null, null, null, "failedLoginTries");
                                return "Error! Failed Login Attempt! : Wrong Password";
                            }
                        }
                    }
                }
                else
                {
                    return "Error! Failed Login Attempt : Non exiting Username";
                }
            }
            catch(InvalidCastException e)
            {
                return $"Error! {e}";
            }
        }
    }
}
