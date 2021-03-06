﻿using Plukit.Base;
using Staxel.Logic;
using Staxel.Tiles;
using Staxel.TileStates;

namespace SoccerMod.Totem {
    class SoccerTotemTileStateBuilder : ITileStateBuilder {
        public void Dispose() { }
        public void Load() { }

        public string Kind() {
            return "mods.Deamon.Soccer.tileState.Totem";
        }

        public Entity Instance(Vector3I location, Tile tile, Universe universe) {
            return SoccerTotemTileStateEntityBuilder.Spawn(universe, tile, location);
        }
    }
}
