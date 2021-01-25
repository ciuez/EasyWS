using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EasyWSServices.Code;
using Newtonsoft.Json;
using SAPB1;

namespace EasyWSServices.Controllers
{

    [Route("/[controller]")]
    [ApiController]
    public class WebApiController : ControllerBase
    {

        [AllowAnonymous]
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> Login([FromForm] string username, [FromForm] string password)
        {
            apiResponse ret = Api.Login(username, password);

            if (ret.success)
            {
                return Ok(JsonConvert.SerializeObject(ret));
            }
            else
            {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> LoginAuth()
        {
            Int64 user_ID = Int64.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid).Value);
            return Ok(JsonConvert.SerializeObject(Api.LoginAuth(user_ID)));
        }

        [Authorize]
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> CliPIVA([FromForm] string piva)
        {
            string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            apiResponse ret = Api.CliPIVA(ipAddress, piva);
            if (ret.success == true)
            {
                cliente_piva b = new cliente_piva();
                if (ret.response != null)
                {
                    b = ret.response;
                }
                return Ok(b);
            }
            return Unauthorized();
        }
    }
}