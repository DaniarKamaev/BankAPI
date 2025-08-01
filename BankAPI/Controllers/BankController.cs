using BankAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Controllers
{
    [ApiController]
    [Route("Bank")]
    public class BankController : ControllerBase
    {
        private static List<Account> _accounts = new List<Account>
        {
            new Account
            {

            }
        };

        [HttpGet]
        public IActionResult GetSize([FromQuery] Guid AccountId)
        {
            var res = _accounts.FirstOrDefault(x => x.Id == AccountId);
            if (res == null) { return NotFound(); };
            return Ok(res);
        }

        [HttpPost]
        public IActionResult openAccount([FromBody] Account account)
        {
            account.Balance = 0;
            _accounts.Add(account);
            return Ok("The Free account is open, Id = " + account.Id);
        }

        [HttpPost("Save")] //переминовать
        public IActionResult openVclad([FromBody] Account account)
        {
            //Отккрытие счета Надежный
            account.interestRate = (decimal?)0.03;
            account.Balance = 0;
            _accounts.Add(account);
            return Ok("The Free account is open, Id = " + account.Id);
        }

        [HttpPost("GetNal")] //переминовать
        public IActionResult Nal([FromBody] Account account, [FromQuery] Guid AccountId)
        {   //Пофиксить позже
            //account = _accounts.FirstOrDefault(x => x.Id == AccountId);
            _accounts.Add(account);
            return Ok("The Free account is open, Id = " + account.Id);
        }




    }
}