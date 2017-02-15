using Microsoft.AspNetCore.Mvc;
using FlipDotServer.Models;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FlipDotServer.Controllers
{
    [Route("api/scoreboard")]
    public class ScoreboardController : Controller
    {
        public ScoreboardController(IScoreboardRepository scoreboardItems)
        {
            _scoreboardRepository = scoreboardItems;
        }
        public IScoreboardRepository _scoreboardRepository { get; set; }

        [HttpGet]
        public IEnumerable<ScoreboardValue> GetAll()
        {
            return _scoreboardRepository.GetAll();
        }

        [HttpGet("{key}", Name = "GetScoreboard")]
        public IActionResult GetById(string key)
        {
            var item = _scoreboardRepository.Find(key);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        //[HttpPost]
        //public IActionResult Create([FromBody] ScoreboardValue item)
        //{
        //    if (item == null)
        //    {
        //        return BadRequest();
        //    }

        //    _scoreboardRepository.Add(item);

        //    return CreatedAtRoute("GetScoreboard", new { id = item.Key }, item);
        //}

        [HttpPut("{key}")]
        public IActionResult Update(string key, [FromBody] ScoreboardValue item)
        {
            if (item == null || item.Key != key)
            {
                return BadRequest();
            }

            var scoreboard = _scoreboardRepository.Find(key);
            if (scoreboard == null)
            {
                return NotFound();
            }

            scoreboard.Value = item.Value;
            // scoreboard.Name = item.Name;

            _scoreboardRepository.Update(scoreboard);
            return new NoContentResult();
        }

        //[HttpDelete("{id}")]
        //public IActionResult Delete(string id)
        //{
        //    var scoreboard = _scoreboardRepository.Find(id);
        //    if (scoreboard == null)
        //    {
        //        return NotFound();
        //    }

        //    _scoreboardRepository.Remove(id);
        //    return new NoContentResult();
        //}

    }
}
