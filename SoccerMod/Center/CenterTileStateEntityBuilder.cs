using System;
using Plukit.Base;
using Staxel.Logic;
using Staxel.Tiles;
using Staxel.TileStates;

namespace SoccerMod {
    public class CenterTileStateEntityBuilder : IEntityPainterBuilder, IEntityLogicBuilder {
        public string Kind { get { return KindCode; } }
        public static string KindCode { get { return "mods.Deamon.Soccer.tileStateEntity.Center"; } }

        EntityLogic IEntityLogicBuilder.Instance(Entity entity, bool server) {
            return new CenterTileStateEntityLogic(entity);
        }

        public void Load() { }

        EntityPainter IEntityPainterBuilder.Instance() {
            return new CenterTileStateEntityPainter();
        }

        public static Entity Spawn(EntityUniverseFacade facade, Tile tile, Vector3I location) {
            var spawnRecord = facade.AllocateNewEntityId();
            var entity = new Entity(spawnRecord, false, KindCode, true);
            var blob = BlobAllocator.Blob(true);
            blob.SetString("tile", tile.Configuration.Code);
            blob.FetchBlob("location").SetVector3I(location);
            blob.SetLong("variant", tile.Variant());
            blob.FetchBlob("position").SetVector3D(location.ToTileCenterVector3D());
            entity.Construct(blob, facade);
            Blob.Deallocate(ref blob);
            facade.AddEntity(entity);
            return entity;
        }
    }
}
