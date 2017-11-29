using System.Collections.Generic;
using Plukit.Base;
using Staxel.Core;

namespace SoccerMod {
    public class SoccerGoalComponentBuilder : IComponentBuilder {
        public string Kind() {
            return "soccerGoals";
        }

        public object Instance(Blob config) {
            return new GoalComponent(config);
        }

        public class GoalComponent {
            public Blob GoalFireworkItem { get; private set; }
            public int FireworkItemQuantity { get; private set; }
            public Vector3D FireworkLaunchVelocity { get; private set; }
            public Vector3D FireworkLaunchVelocitySpread { get; private set; }
            public float FireworkFlightSeconds { get; private set; }
            public float FireworkFlightSecondsSpread { get; private set; }
            public HashSet<string> ScoreWithCategories { get; private set; }
            public float HitBoxScale { get; private set; }

            public GoalComponent(Blob config) {
                GoalFireworkItem = config.FetchBlob("goalFireworkItem");
                FireworkItemQuantity = (int)config.GetLong("fireworkItemQuantity", 1);
                FireworkLaunchVelocity = config.Contains("fireworkLaunchVelocity")
                    ? config.GetBlob("fireworkLaunchVelocity").GetVector3D()
                    : new Vector3D(0f, 17.5f, 0f);
                FireworkLaunchVelocitySpread = config.Contains("fireworkLaunchVelocitySpread")
                    ? config.GetBlob("fireworkLaunchVelocitySpread").GetVector3D()
                    : Vector3D.Zero;
                FireworkFlightSeconds = (float)config.GetDouble("fireworkFlightSeconds", 0.7);
                FireworkFlightSecondsSpread = (float)config.GetDouble("fireworkFlightSecondsSpread", 0.0);
                ScoreWithCategories = new HashSet<string>();
                foreach (var entry in config.FetchList("scoreWithItemCategories"))
                    ScoreWithCategories.Add(entry.GetString());
                HitBoxScale = (float)config.GetDouble("hitBoxScale", 0.5);
            }
        }
    }
}
