using FadeCandy;
using ScoreboardFadeCandy;
using SelfHostedHttpServer;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScoreboardWebServerHelper
{
  internal class ScoreboardController
  {
    private Dictionary<string, ScoreController> groups = new Dictionary<string, ScoreController>(StringComparer.CurrentCultureIgnoreCase);

    public ScoreboardController()
    {
      (new List<string> { "total", "overs", "wickets", "firstInnings" }).ForEach((group) =>
          groups.Add(group, new ScoreController()));
    }

    internal async Task Test(CancellationToken ct)
    {
      using (var fadeCandy = new ScoreboardFadeCandyController())
      {
        await fadeCandy.InitialiseAsync();

        // TODO: Configure the offset!
        await fadeCandy.ExecuteTestAsync(ct, 128);
      }
    }

    internal void AddText(string body)
    {
      // todo: Needs implementing
      // throw new NotImplementedException();
    }

    internal int GetValue(string group)
    {
      if (!groups.ContainsKey(group))
      {
        throw new BadRequestException($"Invalid group ({group})");
      }

      return groups[group].Value;
    }

    internal void SetValue(string group, int value)
    {
      if (!groups.ContainsKey(group))
      {
        throw new BadRequestException($"Invalid group ({group})");
      }

      groups[group].Value = value;
    }
  }
}