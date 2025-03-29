using HospitalAiChatBot.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAiChatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScrapeController : ControllerBase
    {
        private IHospitalInformationProvider _hospitalInformationProvider;
        public ScrapeController()
        {
            _hospitalInformationProvider = new ChitgmaClinicScraper();
        }

        [HttpGet("openinghours")]
        public string GetOpeningHours()
        {
            return _hospitalInformationProvider.GetOpeningHours();
        }

        [HttpGet("callcentercontacts")]
        public string GetCallCenterContacts()
        {
            return _hospitalInformationProvider.GetCallCenterContacts();
        }
    }
}
