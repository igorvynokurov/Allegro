using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AllegroSearchService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadTranslates : ControllerBase
    {



        [HttpGet]
        public string Translate()
        {
            return "success";
        }
    }
}
