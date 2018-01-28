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

        protected override void Dispose(bool disposing) { }
        public override void RenderUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade, int updateSteps) { }

        public override void ClientUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade) { }
        public override void ClientPostUpdate(Timestep timestep, Entity entity, AvatarController avatarController, EntityUniverseFacade facade) { }

        public override void BeforeRender(DeviceContext graphics, Vector3D renderOrigin, Entity entity, AvatarController avatarController, Timestep renderTimestep) { }

        public override void Render(DeviceContext graphics, Matrix4F matrix, Vector3D renderOrigin, Entity entity, AvatarController avatarController, Timestep renderTimestep, RenderMode renderMode) {
            RenderScore(graphics, matrix, renderOrigin, entity, avatarController, renderTimestep, renderMode);
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

        public override void StartEmote(Entity entity, Timestep renderTimestep, EmoteConfiguration emote) { }
    }
}
