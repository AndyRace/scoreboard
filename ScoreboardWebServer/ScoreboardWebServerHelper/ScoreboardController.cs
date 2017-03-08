using SelfHostedHttpServer;
using System;
using System.Collections.Generic;

using System.Linq;

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

        internal void Test()
        {
            // todo: Needs implementing
            // throw new NotImplementedException();
        }

        internal void AddText(string body)
        {
            // todo: Needs implementing
            // throw new NotImplementedException();
        }

        internal int GetValue(string group)
        {
            if(!groups.ContainsKey(group))
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