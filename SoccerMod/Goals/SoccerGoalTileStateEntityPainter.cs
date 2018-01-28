using Plukit.Base;
using Staxel;
using Staxel.Client;
using Staxel.Core;
using Staxel.Draw;
using Staxel.Effects;
using Staxel.Logic;
using Staxel.Rendering;

namespace SoccerMod.Goals {
    public class SoccerGoalTileStateEntityPainter : EntityPainter {
        readonly BillboardNumberRenderer _billboardNumberRenderer = new BillboardNumberRenderer();
        EffectRenderer _effectRenderer = Allocator.EffectRenderer.Allocate();

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (_effectRenderer != null) {
                    _effectRenderer.Dispose();
                    Allocator.EffectRenderer.Release(ref _effectRenderer);
                }
            }
        }

        public override void RenderUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade, int updateSteps) {
            _effectRenderer.RenderUpdate(timestep, entity.Effects, entity, this, facade, entity.Physics.Position);
        }

        public override void ClientUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade) { }
        public override void ClientPostUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade) { }

        public override void BeforeRender(DeviceContext graphics, Vector3D renderOrigin, Entity entity,
            AvatarController avatarController, Timestep renderTimestep) {
            _billboardNumberRenderer.Purge();
        }

        public override void Render(DeviceContext graphics, Matrix4F matrix, Vector3D renderOrigin, Entity entity, AvatarController avatarController, Timestep renderTimestep, RenderMode renderMode) {
            _effectRenderer.Render(entity, this, renderTimestep, graphics, matrix, renderOrigin, renderMode);
            if (renderMode != RenderMode.Normal)
                return;

            var logic = entity.TileStateEntityLogic as SoccerGoalTileStateEntityLogic;
            if (logic == null || !logic.IsClaimed())
                return;

            var renderPosition = logic.GetCountPosition();
            var distanceBetween = (avatarController.Physics.Position - renderPosition).LengthSquared();
            var renderCullDistance = Constants.DockedItemCountRenderCullDistanceSquared * 4f;
            if (distanceBetween < renderCullDistance) {
                var scale = (2f - (renderCullDistance - (float)distanceBetween) / renderCullDistance) * 2f;
                _billboardNumberRenderer.DrawInteger(logic.GoalCount, renderPosition, Vector3D.Zero, scale);
            }
            _billboardNumberRenderer.Draw(graphics, renderOrigin, matrix);
        }

        public override void StartEmote(Entity entity, Timestep renderTimestep, EmoteConfiguration emote) { }
    }
}
