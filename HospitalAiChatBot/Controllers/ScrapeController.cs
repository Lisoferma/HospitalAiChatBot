using HospitalAiChatbot.Source.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAiChatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScrapeController : ControllerBase
    {
        [HttpGet("openinghours")]
        public string GetOpeningHours()
        {
            return ChitgmaClinicScraper.GetOpeningHours();
        }

        [HttpGet("callcentercontacts")]
        public string GetCallCenterContacts()
        {
            return ChitgmaClinicScraper.GetCallCenterContacts();
        }
    }
}
