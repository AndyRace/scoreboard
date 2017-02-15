using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlipDotServer.Models
{
    public class ScoreboardRepository : IScoreboardRepository
    {
        private Object thisLock = new object();

        private readonly ScoreboardContext _context;

        public ScoreboardRepository(ScoreboardContext context)
        {
            _context = context;

            Add(new ScoreboardValue { Key = "total" });
            Add(new ScoreboardValue { Key = "wickets" });
            Add(new ScoreboardValue { Key = "overs" });
            Add(new ScoreboardValue { Key = "firstInnings" });
        }

        public IEnumerable<ScoreboardValue> GetAll()
        {
            lock (thisLock)
            {
                return _context.ScoreboardValues.ToList();
            }
        }

        private void Add(ScoreboardValue item)
        {
            lock (thisLock)
            {
                _context.ScoreboardValues.Add(item);
                _context.SaveChanges();
            }
        }

        public ScoreboardValue Find(string key)
        {
            lock (thisLock)
            {
                return _context.ScoreboardValues.FirstOrDefault(t => t.Key == key);
            }
        }

        //public void Remove(string key)
        //{
        //    var entity = _context.ScoreboardItems.First(t => t.key == key);
        //    _context.ScoreboardItems.Remove(entity);
        //    _context.SaveChanges();
        //}

        public void Update(ScoreboardValue item)
        {
            lock (thisLock)
            {
                _context.ScoreboardValues.Update(item);
                _context.SaveChanges();
            }
        }
    }
}
