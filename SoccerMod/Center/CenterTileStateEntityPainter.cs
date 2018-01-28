using Plukit.Base;
using Staxel.Client;
using Staxel.Core;
using Staxel.Draw;
using Staxel.Logic;
using Staxel.Rendering;
using Staxel.Sound;

namespace SoccerMod.Center {
    public class CenterTileStateEntityPainter : EntityPainter {
        SoundEmitter _soundEmitter = new SoundEmitter();
        private bool _playStartRound;
        private bool _playTick;
        private int _numberToDraw;

        protected override void Dispose(bool disposing) {
            if (!disposing)
                return;
            if (_soundEmitter == null)
                return;
            _soundEmitter.Dispose();
            _soundEmitter = null;
        }

        public override void RenderUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade, int updateSteps) {
            var logic = entity.TileStateEntityLogic as CenterTileStateEntityLogic;

            int lastNum = _numberToDraw;

            if (logic == null || logic.RoundStartedTimestep == Timestep.Null || logic.RoundStartedTimestep + 6 * Constants.TimestepsPerSecond < timestep) {
                _numberToDraw = -1;
                return;
            }

            if (logic.RoundStartedTimestep + 1 * Constants.TimestepsPerSecond > timestep)
                _numberToDraw = 5;
            else if (logic.RoundStartedTimestep + 2 * Constants.TimestepsPerSecond > timestep)
                _numberToDraw = 4;
            else if (logic.RoundStartedTimestep + 3 * Constants.TimestepsPerSecond > timestep)
                _numberToDraw = 3;
            else if (logic.RoundStartedTimestep + 4 * Constants.TimestepsPerSecond > timestep)
                _numberToDraw = 2;
            else if (logic.RoundStartedTimestep + 5 * Constants.TimestepsPerSecond > timestep)
                _numberToDraw = 1;
            else if (logic.RoundStartedTimestep + 6 * Constants.TimestepsPerSecond > timestep)
                _numberToDraw = 0;

            if (lastNum != _numberToDraw && _numberToDraw == 0)
                _playStartRound = true;
            else if (lastNum != _numberToDraw && _numberToDraw > 0)
                _playTick = true;
        }

        public override void ClientUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade) { }
        public override void ClientPostUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade) { }

        public override void BeforeRender(DeviceContext graphics, Vector3D renderOrigin, Entity entity,
            AvatarController avatarController, Timestep renderTimestep) {
            var logic = entity.TileStateEntityLogic as CenterTileStateEntityLogic;
            if (logic == null || logic.IsLingering())
                return;

            _soundEmitter.Render(entity.Physics.Position + logic.Component.BallSpawnLocation, renderOrigin, RenderMode.Normal);

            if (!_soundEmitter.IsEmitting && _playStartRound) {
                if (!logic.Component.StartRoundSound.IsNullOrEmpty()) {
                    _soundEmitter.Emit(logic.Component.StartRoundSound);
                    _playStartRound = false;
                }
                else
                    _playStartRound = false;
            }
            if (!_soundEmitter.IsEmitting && _playTick) {
                if (!logic.Component.TickSound.IsNullOrEmpty()) {
                    _soundEmitter.Emit(logic.Component.TickSound);
                    _playTick = false;
                }
                else
                    _playTick = false;
            }
        }

        public override void Render(DeviceContext graphics, Matrix4F matrix, Vector3D renderOrigin, Entity entity, AvatarController avatarController, Timestep renderTimestep, RenderMode renderMode) {
            if (renderMode != RenderMode.Normal)
                return;

            var logic = entity.TileStateEntityLogic as CenterTileStateEntityLogic;
            if (logic == null || _numberToDraw < 0)
                return;

            var delta = (logic.GetSpawningPosition() - renderOrigin).ToVector3F();

            var m = Matrix4F.CreateTranslation(delta);

            logic.Component.Numbers[_numberToDraw].Render(graphics, m.Multiply(matrix));
        }

        public override void StartEmote(Entity entity, Timestep renderTimestep, EmoteConfiguration emote) { }
    }
}
