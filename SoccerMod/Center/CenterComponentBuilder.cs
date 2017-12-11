using System.Collections.Generic;
using Plukit.Base;
using Staxel;
using Staxel.Core;
using Staxel.Draw;
using Staxel.Voxel;

namespace SoccerMod.Center {
    public class CenterComponentBuilder : IComponentBuilder {
        public string Kind() {
            return "mod.deamon.soccer.soccerCenter";
        }

        public object Instance(Blob config) {
            return new CenterTotemComponent(config);
        }

        public class CenterTotemComponent {
            public Blob SoccerBall { get; private set; }
            public Vector3D BallSpawnLocation { get; private set; }
            public string NotClaimedNotification { get; private set; }
            public string NotReadyNotification { get; private set; }
            public string TotemNotComplete { get; private set; }
            public string TickSound { get; private set; }
            public string StartRoundSound { get; private set; }
            public Drawable[] Numbers { get; private set; }

            public CenterTotemComponent(Blob config) {
                SoccerBall = config.FetchBlob("soccerBall");
                BallSpawnLocation = config.GetBlob("ballSpawnLocation").GetVector3D();
                NotClaimedNotification = config.GetString("notClaimedNotification", "");
                NotReadyNotification = config.GetString("notReadyNotification", "");
                TotemNotComplete = config.GetString("totemNotComplete", "");
                TickSound = config.GetString("tickSound", "");
                StartRoundSound = config.GetString("startRoundSound", "");

                Numbers = new Drawable[6];
                var countdown = config.GetBlob("countdown");
                for (int i = 0; i < 6; i++) {
                    Numbers[i] = GameContext.Resources.FetchVoxelDrawableSync(countdown.GetString(i.ToString()));
                }
            }
        }
    }
}
