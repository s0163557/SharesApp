using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharesApp.Server.Models;

namespace SharesApp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        [HttpGet]
        [Route("GetActiveSecurities")]
        public List<SecurityInfo> GetActiveSecurities()
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
            {
                var tradeRecordsQuery = from tradeRecord in dbContext.SecurityTradeRecords
                                        group tradeRecord by tradeRecord.SecurityInfoId into securityInfoGroup
                                        select new SecurityTradeRecord
                                        {
                                            SecurityInfoId = securityInfoGroup.Key,
                                            DateOfTrade = securityInfoGroup.Max(x => x.DateOfTrade),
                                        };

                var filteredRecords = from record in tradeRecordsQuery
                                      where record.DateOfTrade.Month - 3 <= DateTime.Now.Month
                                      join securityInfo in dbContext.SecurityInfos on record.SecurityInfoId equals securityInfo.SecurityInfoId
                                      orderby securityInfo.SecurityInfoId
                                      select new SecurityInfo
                                      {
                                          SecurityInfoId = securityInfo.SecurityInfoId,
                                          SecurityId = securityInfo.SecurityId,
                                          Isin = securityInfo.Isin,
                                          Name = securityInfo.Name,
                                      };

                return filteredRecords.ToList();
            }
        }

        [HttpGet]
        [Route("GetInactiveSecurities")]
        public List<SecurityInfo> GetInactiveSecurities()
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
            {
                var tradeRecordsQuery = from tradeRecord in dbContext.SecurityTradeRecords
                                        group tradeRecord by tradeRecord.SecurityInfoId into securityInfoGroup
                                        select new SecurityTradeRecord
                                        {
                                            SecurityInfoId = securityInfoGroup.Key,
                                            DateOfTrade = securityInfoGroup.Max(x => x.DateOfTrade),
                                        };

                var filteredRecords = from record in tradeRecordsQuery
                                      where record.DateOfTrade.Month - 3 >= DateTime.Now.Month
                                      join securityInfo in dbContext.SecurityInfos on record.SecurityInfoId equals securityInfo.SecurityInfoId
                                      orderby securityInfo.SecurityInfoId
                                      select new SecurityInfo
                                      {
                                          SecurityInfoId = securityInfo.SecurityInfoId,
                                          SecurityId = securityInfo.SecurityId,
                                          Isin = securityInfo.Isin,
                                          Name = securityInfo.Name,
                                      };

                return filteredRecords.ToList();
            }
        }

        [HttpGet]
        [Route("GetSecurity/{securityid}")]
        public SecurityInfo? GetSecurityInfo(string securityid)
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
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

        [HttpGet]
        [Route("GetSecurityTradeRecords/{securityid}")]
        public string GetSecurityTradeRecordsForChart(string securityid)
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
            {
                try
                {
                    List<SecurityTradeRecord> securityTradeRecords = dbContext.SecurityTradeRecords.Where(x => x.SecurityInfo.SecurityId == securityid).ToList();

                    var records = from record in securityTradeRecords
                                  select new
                                  {
                                      x = record.DateOfTrade,
                                      y = new List<double> { record.Open, record.High, record.Low, record.Close }
                                  };

                    return JsonConvert.SerializeObject(records, Formatting.Indented);
                }
                catch (Exception ex)
                {
                    return "";
                }
            }
        }

        [HttpGet]
        [Route("GetSecurityTradeRecordsByDay/{securityid}")]
        public string GetSecurityTradeRecordsByDay(string securityid)
        {
            using (SecuritiesContext dbContext = new SecuritiesContext())
            {
                try
                {
                    List<SecurityTradeRecord> securityTradeRecords = dbContext.SecurityTradeRecords.Where(x =>
                    x.SecurityInfo.SecurityId == securityid).OrderBy(x => x.DateOfTrade).ToList();
                    if (securityTradeRecords.Count > 30)
                        securityTradeRecords = securityTradeRecords.TakeLast(30).ToList();

                    var records = from record in securityTradeRecords
                                  select new
                                  {
                                      x = record.DateOfTrade,
                                      y = new List<double> { record.Open, record.High, record.Low, record.Close, record.NumberOfTrades }
                                  };

                    return JsonConvert.SerializeObject(records, Formatting.Indented);
                }
                catch (Exception ex)
                {
                    return "";
                }
            }
        }

        [HttpGet]
        [Route("GetSecurityTradeRecordsByWeek/{securityid}")]
        public string GetSecurityTradeRecordsByWeek(string securityid)
        {
            try
            {
                DateOnly yearDelay = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));
                using (SecuritiesContext dbContext = new SecuritiesContext())
                {
                    List<SecurityTradeRecordsByWeek> securityTradeRecords = dbContext.SecurityTradeRecordsByWeeks.Where(x =>
                    x.SecurityInfo.SecurityId == securityid &&
                    x.DateOfTrade >= yearDelay).ToList();

                    var records = from record in securityTradeRecords
                                  select new
                                  {
                                      x = record.DateOfTrade,
                                      y = new List<double> { record.Open, record.High, record.Low, record.Close, record.NumberOfTrades }
                                  };

                    return JsonConvert.SerializeObject(records, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        [HttpGet]
        [Route("GetSecurityTradeRecordsByMonth/{securityid}")]
        public string GetSecurityTradeRecordsByMonth(string securityid)
        {
            try
            {
                using (SecuritiesContext dbContext = new SecuritiesContext())
                {
                    List<SecurityTradeRecordsByMonth> securityTradeRecords = dbContext.SecurityTradeRecordsByMonths.Where(x =>
                    x.SecurityInfo.SecurityId == securityid).ToList();

                    var records = from record in securityTradeRecords
                                  select new
                                  {
                                      x = record.DateOfTrade,
                                      y = new List<double> { record.Open, record.High, record.Low, record.Close, record.NumberOfTrades }
                                  };

                    return JsonConvert.SerializeObject(records, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return "";
            }

        }


    }
}
