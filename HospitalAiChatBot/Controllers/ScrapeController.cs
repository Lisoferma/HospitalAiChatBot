using HospitalAiChatBot.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace HospitalAiChatbot.Controllers
{
    /// <summary>
    /// Контроллер для скрапера
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ScrapeController : ControllerBase
    {
        private IHospitalInformationProvider _hospitalInformationProvider;
        public ScrapeController()
        {
            _hospitalInformationProvider = new ChitgmaClinicScraper();
        }

        /// <summary>
        /// Возвращает расписание работы организации
        /// </summary>
        /// <returns></returns>
        [HttpGet("openinghours")]
        public string GetOpeningHours()
        {
            return _hospitalInformationProvider.GetOpeningHours();
        }

        /// <summary>
        /// Возвращает контакты колл-центра
        /// </summary>
        /// <returns></returns>
        [HttpGet("callcentercontacts")]
        public string GetCallCenterContacts()
        {
            return _hospitalInformationProvider.GetCallCenterContacts();
        }
    }
}
