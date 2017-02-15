using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlipDotServer.Models
{
    public interface IScoreboardRepository
    {
        // void Add(ScoreboardItem item);
        IEnumerable<ScoreboardValue> GetAll();
        ScoreboardValue Find(string key);
        // void Remove(string key);
        void Update(ScoreboardValue item);
    }
}
