using System;
using Plukit.Base;
using Staxel;
using Staxel.Client;
using Staxel.Core;
using Staxel.Draw;
using Staxel.Effects;
using Staxel.Logic;
using Staxel.Rendering;
using Staxel.Sound;
using Staxel.Voxel;

namespace SoccerMod {
    public class CenterTileStateEntityPainter : EntityPainter {
        SoundEmitter _soundEmitter = new SoundEmitter();
        private Drawable[] drawables;
        bool _chimed;
        bool _chime;
        private bool _fetchedDrawables;

        protected override void Dispose(bool disposing) {
            if (!disposing)
                return;
            if (_soundEmitter == null)
                return;
            _soundEmitter.Dispose();
            _soundEmitter = null;
        }

        public override void RenderUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade) { }

        public override void ClientUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade) { }
        public override void ClientPostUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade) { }

        public override void BeforeRender(DeviceContext graphics, Vector3D renderOrigin, Entity entity,
            AvatarController avatarController, Timestep renderTimestep) { }

        public override void Render(DeviceContext graphics, Matrix4F matrix, Vector3D renderOrigin, Entity entity, AvatarController avatarController, Timestep renderTimestep, RenderMode renderMode) {
            //if (renderMode != RenderMode.Normal)
            //    return;

            var logic = entity.TileStateEntityLogic as CenterTileStateEntityLogic;
            if (logic == null && logic.RoundStartedTimestep == Timestep.Null || logic.RoundStartedTimestep + 6 * Constants.TimestepsPerSecond < renderTimestep)
                return;

            if (!_fetchedDrawables)
                Setup(logic);

            int drawNum = 0;
            if (logic.RoundStartedTimestep + 1 * Constants.TimestepsPerSecond > renderTimestep)
                drawNum = 5;
            else if (logic.RoundStartedTimestep + 2 * Constants.TimestepsPerSecond > renderTimestep)
                drawNum = 4;
            else if (logic.RoundStartedTimestep + 3 * Constants.TimestepsPerSecond > renderTimestep)
                drawNum = 3;
            else if (logic.RoundStartedTimestep + 4 * Constants.TimestepsPerSecond > renderTimestep)
                drawNum = 2;
            else if (logic.RoundStartedTimestep + 5 * Constants.TimestepsPerSecond > renderTimestep)
                drawNum = 1;


            var delta = (logic.GetSpawningPosition() - renderOrigin).ToVector3F();

            //var m = 
            //var m2 = Matrix4F.CreateTranslation(delta);
            //m = m.Multiply(m2);

            //drawables[drawNum].Render(graphics, m.Apply(matrix));
        }

        private void Setup(CenterTileStateEntityLogic logic) {
            var nums = logic.GetDrawableNumbers();
            drawables = new Drawable[nums.Length];
            for (int i = 0; i < nums.Length; i++) {
                drawables[i] = GameContext.Resources.FetchVoxelDrawableSync(nums[i]);
                Console.WriteLine(nums[i]);
            }
            _fetchedDrawables = true;
        }

        public override bool AssociatedWith(Entity entity) {
            return entity.Logic is CenterTileStateEntityLogic;
        }

        public override void StartEmote(Entity entity, Timestep renderTimestep, EmoteConfiguration emote) { }
    }
}
