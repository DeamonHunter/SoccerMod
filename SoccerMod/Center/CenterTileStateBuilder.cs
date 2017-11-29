using Plukit.Base;
using Staxel.Logic;
using Staxel.Tiles;
using Staxel.TileStates;

namespace SoccerMod {
    public class CenterTileStateBuilder : ITileStateBuilder {
        public void Dispose() { }
        public void Load() { }

        public string Kind() {
            return "mods.Deamon.Soccer.tileState.Center";
        }

        public Entity Instance(Vector3I location, Tile tile, Universe universe) {
            return CenterTileStateEntityBuilder.Spawn(universe, tile, location);
        }
    }
}
