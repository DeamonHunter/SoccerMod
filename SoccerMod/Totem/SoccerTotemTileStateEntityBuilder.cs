using Plukit.Base;
using SoccerMod;
using Staxel.Logic;
using Staxel.Tiles;

namespace Staxel.TileStates.Totems {
    class SoccerTotemTileStateEntityBuilder : IEntityPainterBuilder, IEntityLogicBuilder {
        public string Kind { get { return KindCode; } }
        public static string KindCode { get { return "mods.Deamon.Soccer.tileStateEntity.Totem"; } }

        EntityLogic IEntityLogicBuilder.Instance(Entity entity, bool server) {
            return new SoccerTotemTileStateEntityLogic(entity);
        }

        public void Load() { }

        EntityPainter IEntityPainterBuilder.Instance() {
            return new TotemTileStateEntityPainter();
        }

        public static Entity Spawn(EntityUniverseFacade facade, Tile tile, Vector3I location) {
            var spawnRecord = facade.AllocateNewEntityId();
            var entity = new Entity(spawnRecord, false, KindCode, true);
            var blob = BlobAllocator.Blob(true);
            blob.SetString("tile", tile.Configuration.Code);
            blob.FetchBlob("location").SetVector3I(location);
            blob.SetLong("variant", tile.Variant());
            blob.FetchBlob("position").SetVector3D(location.ToTileCenterVector3D());
            blob.FetchBlob("velocity").SetVector3D(Vector3D.Zero);
            entity.Construct(blob, facade);
            Blob.Deallocate(ref blob);
            facade.AddEntity(entity);
            return entity;
        }
    }
}
