using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lombard_Mongo_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class doomplay : ControllerBase
    {
        private const string IDDQD = "IDDQD";

        [HttpGet]
        public IActionResult Get(string cheatCode)
        {
            if (string.Equals(cheatCode, IDDQD, System.StringComparison.OrdinalIgnoreCase))
            {
                // Если передан верный чит-код, возвращаем ссылку
                string gameLink = "https://dos.zone/doom-dec-1993/";
                return Redirect(gameLink);
            }
            else
            {
                // Возвращаем сообщение об ошибке, если чит-код неверный
                return BadRequest("Invalid cheat code");
            }
        }

    }
}
