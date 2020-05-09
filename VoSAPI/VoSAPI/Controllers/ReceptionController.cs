using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoSAPI.Models;

namespace VoSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceptionController : ControllerBase
    {
        private readonly VosContext _context;

        public ReceptionController(VosContext context)
        {
            _context = context;
        }

        // GET: api/Reception
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RFIDCard>>> GetRFIDCard()
        {
            return await _context.RFIDCard.ToListAsync();
        }

        // GET: api/Reception/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RFIDCard>> GetRFIDCard(long id)
        {
            var rFIDCard = await _context.RFIDCard.FindAsync(id);

            if (rFIDCard == null)
            {
                return NotFound();
            }

            return rFIDCard;
        }

        [HttpGet("cardNumber")]
        public async Task<ActionResult<RFIDCard>> GetRFIDCardByNumber(string number)
        {
            var rFIDCard = await _context.RFIDCard.FirstOrDefaultAsync(e => e.CardNumber == number);

            if (rFIDCard == null)
            {
                return NotFound();
            }

            return rFIDCard;
        }

        [HttpPut("changeCardStatus")]
        public async Task<ActionResult<RFIDCard>> ChangeCardStatus(string number)
        {
            var rFIDCard = await _context.RFIDCard.FirstOrDefaultAsync(e => e.CardNumber == number);

            if (rFIDCard == null)
            {
                return NotFound();
            }

            rFIDCard.InSafeZone = !rFIDCard.InSafeZone;
            _context.Entry(rFIDCard).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return rFIDCard;
        }

        // PUT: api/Reception/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRFIDCard(long id, RFIDCard rFIDCard)
        {
            if (id != rFIDCard.RFIDCardID)
            {
                return BadRequest();
            }

            _context.Entry(rFIDCard).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RFIDCardExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Reception
        [HttpPost]
        public async Task<ActionResult<RFIDCard>> PostRFIDCard(RFIDCard rFIDCard)
        {
            _context.RFIDCard.Add(rFIDCard);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRFIDCard", new { id = rFIDCard.RFIDCardID }, rFIDCard);
        }

        // DELETE: api/Reception/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<RFIDCard>> DeleteRFIDCard(long id)
        {
            var rFIDCard = await _context.RFIDCard.FindAsync(id);
            if (rFIDCard == null)
            {
                return NotFound();
            }

            _context.RFIDCard.Remove(rFIDCard);
            await _context.SaveChangesAsync();

            return rFIDCard;
        }

        private bool RFIDCardExists(long id)
        {
            return _context.RFIDCard.Any(e => e.RFIDCardID == id);
        }
    }
}
