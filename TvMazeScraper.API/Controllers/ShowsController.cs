using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;
using TvMazeScraper.API.ViewModels;
using TvMazeScraper.Infrastructure;
using TvMazeScraper.Infrastructure.Model;

namespace TvMazeScraper.API.Controllers
{
    [Route("api/shows")]
    [ApiController]
    public class ShowsController : ControllerBase
    {
        private readonly TvMazeContext _context;

        public ShowsController(TvMazeContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get the list of shows with all the cast members. This endpoint is paginated, with a fixed set of 100 results per page.
        /// </summary>
        /// <param name="page">Page number</param>
        /// <returns>List of items with show and cast information</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ShowViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<IEnumerable<ShowViewModel>>> GetShowList(int page = 0)
        {
            if (page < 0)
            {
                return BadRequest("Please enter a valid page greater than 0");
            }

            var fixedPageSize = 100;
            List<ShowViewModel> showList = await _context.Shows
                .Include(s => s.Cast)
                .Skip(page * fixedPageSize)
                .Take(fixedPageSize)
                .Select(s => new ShowViewModel
                {
                    Id = s.ShowId,
                    Name = s.Name,
                    Cast = s.Cast.OrderByDescending(c => c.Birthday).Select(c => new ShowCastMemberViewModel
                    {
                        Id = c.PersonId,
                        Name = c.Name,
                        Birthday = c.Birthday.HasValue ? c.Birthday.Value.ToString("yyyy-MM-dd") : string.Empty,
                    }).ToList(),
                }).ToListAsync();

            return showList;
        }
    }
}
