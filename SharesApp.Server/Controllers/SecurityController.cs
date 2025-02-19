using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stock_Analysis_Web_App.Context;
using Stock_Analysis_Web_App.Models;

namespace SharesApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        [HttpGet]
        [Route("GetSecurities")]
        public List<SecurityInfo> GetListOfSecurities()
        {
            using (SecuritiesDbContext dbContext = new SecuritiesDbContext())
            {
                dbContext.SecurityInfos.Load();
                return dbContext.SecurityInfos.Select(x => x).ToList();
            }
        }

        [HttpGet]
        [Route("GetSecurity/{securityid}")]
        public SecurityInfo? GetSecurityInfo(string securityid)
        {
            using (SecuritiesDbContext dbContext = new SecuritiesDbContext())
            {
                dbContext.SecurityInfos.Load();
                try
                {
                    SecurityInfo securityInfo = dbContext.SecurityInfos.Where(x => x.SecurityId == securityid).First();
                    return securityInfo;
                }
                catch (Exception ex)
                {
                    SecurityInfo sc = new SecurityInfo();
                    return sc;
                }
            }
        }

    }
}
