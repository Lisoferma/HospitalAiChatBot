using HospitalAiChatBot.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAiChatBot.Controllers;

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