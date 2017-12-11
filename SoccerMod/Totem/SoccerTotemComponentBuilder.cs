using Plukit.Base;
using Staxel;
using Staxel.Core;
using Staxel.Draw;
using Staxel.Voxel;

namespace SoccerMod.Totem {
    public class SoccerTotemComponentBuilder : IComponentBuilder {
        public string Kind() {
            return "mod.deamon.soccer.soccerTotem";
        }

        public object Instance(Blob config) {
            return new SoccerTotemComponent(config);
        }

        public class SoccerTotemComponent {
            public Drawable[] RedNumbers { get; private set; }
            public Drawable[] BlueNumbers { get; private set; }
            public Vector3D BlueTeamScorePos { get; private set; }
            public Vector3D RedTeamScorePos { get; private set; }

            public SoccerTotemComponent(Blob config) {
                RedNumbers = new Drawable[4];
                var redNumbers = config.GetBlob("redNumbers");
                for (int i = 0; i < 4; i++) {
                    RedNumbers[i] = GameContext.Resources.FetchVoxelDrawableSync(redNumbers.GetString(i.ToString()));
                }
                BlueNumbers = new Drawable[4];
                var blueNumbers = config.GetBlob("blueNumbers");
                for (int i = 0; i < 4; i++) {
                    BlueNumbers[i] = GameContext.Resources.FetchVoxelDrawableSync(blueNumbers.GetString(i.ToString()));
                }

                BlueTeamScorePos = config.GetBlob("blueTeamScorePos").GetVector3D();
                RedTeamScorePos = config.GetBlob("redTeamScorePos").GetVector3D();
            }
        }
    }
}
