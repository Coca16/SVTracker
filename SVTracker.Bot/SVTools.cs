using SpookVooper.Api.Entities;
using SpookVooper.Api.Entities.Groups;
using System;

using System.Threading.Tasks;

namespace SVTracker
{
    static public class SVTools
    {
        static public async Task<string> SVIDToName(string SVID)
        {
            bool isgroup = SVID.Contains("g-");
            bool isuser = SVID.Contains("u-");

            if (isgroup == false && isuser == false) { return null; }
            if (isgroup == true && isuser == true)
            {
                throw new Exception("This SVID applies both to a user and group");
            }
            else if (isgroup == false && isuser == true)
            {
                User user = new User(SVID);
                return await user.GetUsernameAsync();
            }
            else
            {
                Group group = new Group(SVID);
                return await group.GetNameAsync();
            }
        }
    }
}
