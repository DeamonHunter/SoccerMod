using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Plukit.Base;
using Staxel;
using Staxel.Client;
using Staxel.Collections;
using Staxel.Core;
using Staxel.Draw;
using Staxel.Effects;
using Staxel.Logic;
using Staxel.Particles;
using Staxel.Rendering;
using Staxel.TileStates.Totems;

namespace SoccerMod.Totem {
    public class SoccerTotemTileStateEntityPainter : EntityPainter {
        EffectRenderer _effectRenderer = Allocator.EffectRenderer.Allocate();
        TotemParticleRenderer _particleRenderer = Allocator.TotemParticleRenderer.Allocate();

        Timestep _prevStep = Timestep.Null;

        const long ParticleEmitPeriod = 200000;

        readonly List<Pair<Particle, Vector3F>> _xParticles = new List<Pair<Particle, Vector3F>>();
        readonly List<Pair<Particle, Vector3F>> _yParticles = new List<Pair<Particle, Vector3F>>();
        readonly List<Pair<Particle, Vector3F>> _zParticles = new List<Pair<Particle, Vector3F>>();
        bool _init;

        EntityId _thisTotem = EntityId.NullEntityId;

        protected override void Dispose(bool disposing) {
            ClientContext.PlayerFacade.TurnOffTotemRegion(_thisTotem);
            if (disposing) {
                if (_effectRenderer != null) {
                    _effectRenderer.Dispose();
                    Allocator.EffectRenderer.Release(ref _effectRenderer);
                }
                if (_particleRenderer != null) {
                    _particleRenderer.Dispose();
                    Allocator.TotemParticleRenderer.Release(ref _particleRenderer);
                }
            }
        }

        public override void RenderUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade) {
            _effectRenderer.RenderUpdate(timestep, entity.Effects, entity, this, facade, entity.Physics.Position);
        }

        public override void ClientUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade) { }
        public override void ClientPostUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade) { }

        public override void BeforeRender(DeviceContext graphics, Vector3D renderOrigin, Entity entity, AvatarController avatarController, Timestep renderTimestep) {
            if (!_init) {
                var logic = entity.Logic as TotemTileStateEntityLogic;
                if (!ClientContext.PlayerFacade.IsShowingTotemRegionFor(entity.Id) && !logic.JustPlaced)
                    return;

                _particleRenderer.Init(logic.ScanCentre, logic.Region);
                _init = true;
            }
        }

        public override void Render(DeviceContext graphics, Matrix4F matrix, Vector3D renderOrigin, Entity entity, AvatarController avatarController, Timestep renderTimestep, RenderMode renderMode) {
            RenderTotem(graphics, matrix, renderOrigin, entity, avatarController, renderTimestep, renderMode);
            RenderScore(graphics, matrix, renderOrigin, entity, avatarController, renderTimestep, renderMode);
        }

        public void RenderTotem(DeviceContext graphics, Matrix4F matrix, Vector3D renderOrigin, Entity entity, AvatarController avatarController, Timestep renderTimestep, RenderMode renderMode) {
            _effectRenderer.Render(entity, this, renderTimestep, graphics, matrix, renderOrigin, renderMode);

            var logic = entity.Logic as TotemTileStateEntityLogic;
            _thisTotem = entity.Id;

            if (!ClientContext.PlayerFacade.IsShowingTotemRegionFor(entity.Id) && !logic.JustPlaced)
                return;

            _particleRenderer.Render(renderTimestep);
        }

        public void RenderScore(DeviceContext graphics, Matrix4F matrix, Vector3D renderOrigin, Entity entity, AvatarController avatarController, Timestep renderTimestep, RenderMode renderMode) {
            if (renderMode != RenderMode.Normal)
                return;

            var logic = entity.TileStateEntityLogic as SoccerTotemTileStateEntityLogic;
            if (logic == null || !logic.HasGameStarted())
                return;

            var delta = (logic.GetTileCenter() - renderOrigin).ToVector3F();

            var m1 = Matrix4F.CreateTranslation(logic.Component.BlueTeamScorePos.ToVector3F())
                .RotateUnitY(logic.GetRotationInRadians())
                .Translate(delta);



            logic.Component.BlueNumbers[logic.BlueTeamScore].Render(graphics, m1.Multiply(matrix));

            var m2 = Matrix4F.CreateTranslation(logic.Component.RedTeamScorePos.ToVector3F())
                .RotateUnitY(logic.GetRotationInRadians())
                .Translate(delta);

            logic.Component.RedNumbers[logic.RedTeamScore].Render(graphics, m2.Multiply(matrix));

        }

        public override bool AssociatedWith(Entity entity) {
            return entity.Logic is TotemTileStateEntityLogic;
        }

        public override void StartEmote(Entity entity, Timestep renderTimestep, EmoteConfiguration emote) { }
    }
}
