using Plukit.Base;
using Staxel.Logic;
using Staxel.Tiles;
using Staxel.TileStates;

namespace SoccerMod.Goals {
    public class SoccerGoalTileStateBuilder : ITileStateBuilder {
        public void Dispose() { }
        public void Load() { }

        public string Kind() {
            return "mods.Deamon.Soccer.tileState.Goal";
        }

        public Entity Instance(Vector3I location, Tile tile, Universe universe) {
            return SoccerGoalTileStateEntityBuilder.Spawn(universe, tile, location);
        }
    }
}
