using System.Collections.Generic;
using Plukit.Base;
using Staxel;
using Staxel.Core;
using Staxel.Draw;
using Staxel.Voxel;

namespace SoccerMod {
    public class CenterComponentBuilder : IComponentBuilder {
        public string Kind() {
            return "mod.deamon.soccer.soccerGoals";
        }

        public object Instance(Blob config) {
            return new CenterTotemComponent(config);
        }

        public class CenterTotemComponent {
            public Blob SoccerBall { get; private set; }
            public Vector3D BallSpawnLocation { get; private set; }
            public string NotClaimedNotification { get; private set; }
            public string TotemNotComplete { get; private set; }
            public string TickSound { get; private set; }
            public string StartRoundSound { get; private set; }
            public string[] numbers { get; private set; }

            public CenterTotemComponent(Blob config) {
                SoccerBall = config.FetchBlob("soccerBall");
                BallSpawnLocation = config.GetBlob("ballSpawnLocation").GetVector3D();
                NotClaimedNotification = config.GetString("notClaimedNotification", "");
                TotemNotComplete = config.GetString("totemNotComplete", "");
                TickSound = config.GetString("tickSound", "");
                StartRoundSound = config.GetString("startRoundSound", "");

                numbers = new string[6];
                var countdown = config.GetBlob("countdown");
                for (int i = 0; i < 6; i++) {
                    numbers[i] = countdown.GetString(i.ToString());
                    if (GameContext.Revalidate)
                        GameContext.Resources.FetchVoxelDrawableSync(numbers[i]);
                }
            }
        }
    }
}
