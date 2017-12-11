using System;
using Plukit.Base;
using SoccerMod.Center;
using SoccerMod.Goals;
using Staxel;
using Staxel.Items;
using Staxel.Logic;
using Staxel.Tiles;
using Staxel.TileStates;
using Staxel.TileStates.Totems;

namespace SoccerMod.Totem {
    public class SoccerTotemTileStateEntityLogic : TotemTileStateEntityLogic {
        private Timestep _timer;

        public int RedTeamScore;
        public SoccerGoalTileStateEntityLogic RedTeamGoal;
        public int BlueTeamScore;
        public SoccerGoalTileStateEntityLogic BlueTeamGoal;
        public CenterTileStateEntityLogic Center;
        public SoccerTotemComponentBuilder.SoccerTotemComponent Component;

        private Vector3D _tileCenter;
        private uint _variant;

        public float GetRotationInRadians() {
            return Configuration.GetRotationInRadians(_variant);
        }

        public Vector3D GetTileCenter() {
            return _tileCenter;
        }

        private bool _gameStarted;


        public SoccerTotemTileStateEntityLogic(Entity entity) : base(entity) { }

        public override void Update(Timestep timestep, EntityUniverseFacade universe) {
            base.Update(timestep, universe);

            Tile tile;
            if (!universe.ReadTile(Location, TileAccessFlags.None, out tile))
                return;
            _tileCenter = tile.Configuration.TileCenter(Location, tile.Variant());

            Vector3F tileOffset;
            if (universe.TileOffset(Location, TileAccessFlags.None, out tileOffset))
                _tileCenter.Y += tileOffset.Y;

            if (_timer == Timestep.Null) {
                _timer = timestep;
            }
            if (timestep > _timer + 3000000) {
                _timer = timestep;
                if (!IsReady()) {
                    ScanSurroundings();
                    BlueTeamScore = 0;
                    RedTeamScore = 0;
                    _gameStarted = false;
                }
            }
            CheckIfEntitiesStillExists();
        }

        public void CheckIfEntitiesStillExists() {
            if (RedTeamGoal != null && RedTeamGoal.IsLingering())
                RedTeamGoal = null;
            if (BlueTeamGoal != null && BlueTeamGoal.IsLingering())
                BlueTeamGoal = null;
            if (Center != null && Center.IsLingering())
                Center = null;
        }

        public bool HasGameStarted() {
            return _gameStarted;
        }

        public bool CanStartNewGame() {
            return !_gameStarted || BlueTeamScore >= 3 || RedTeamScore >= 3;
        }

        public void ResetGame() {
            RedTeamScore = 0;
            BlueTeamScore = 0;
            _gameStarted = true;
        }

        public void ResetBall(EntityUniverseFacade universe) {
            if (Center != null)
                Center.PrepareReset(universe.Step);
            else {
                Console.WriteLine("Attempted to spawn a ball when there is no center.");
            }
        }

        public override void Construct(Blob arguments, EntityUniverseFacade entityUniverseFacade) {
            base.Construct(arguments, entityUniverseFacade);

            _gameStarted = arguments.GetBool("gameStarted", false);
            RedTeamScore = (int)arguments.GetLong("redTeamScore", 0L);
            BlueTeamScore = (int)arguments.GetLong("blueTeamScore", 0L);
            _variant = (uint)arguments.GetLong("variant");
            Component = Configuration.Components.Get<SoccerTotemComponentBuilder.SoccerTotemComponent>();
        }

        public override void Store() {
            base.Store();
            _blob.SetBool("gameStarted", _gameStarted);
            _blob.SetLong("redTeamScore", RedTeamScore);
            _blob.FetchBlob("tileCenter").SetVector3D(_tileCenter);
            _blob.SetLong("blueTeamScore", BlueTeamScore);
            _blob.SetLong("variant", _variant);
        }

        public override void StorePersistenceData(Blob data) {
            base.StorePersistenceData(data);

            data.SetBool("gameStarted", _gameStarted);
            data.SetLong("redTeamScore", RedTeamScore);
            data.SetLong("blueTeamScore", BlueTeamScore);
            data.SetLong("variant", _variant);
            data.FetchBlob("tileCenter").SetVector3D(_tileCenter);
        }

        public override void Restore() {
            base.Restore();
            _gameStarted = _blob.GetBool("gameStarted", false);
            RedTeamScore = (int)_blob.GetLong("redTeamScore", 0L);
            BlueTeamScore = (int)_blob.GetLong("blueTeamScore", 0L);
            _variant = (uint)_blob.GetLong("variant");
            _tileCenter = _blob.GetBlob("tileCenter").GetVector3D();
            Component = Configuration.Components.Get<SoccerTotemComponentBuilder.SoccerTotemComponent>();
        }

        public void IncreaseScore(int team, Timestep step) {
            switch (team) {
                case 0:
                    BlueTeamScore++;
                    if (BlueTeamScore > 3)
                        BlueTeamScore = 3;
                    break;
                case 1:
                    RedTeamScore++;
                    if (RedTeamScore > 3)
                        RedTeamScore = 3;
                    break;
                default:
                    throw new Exception("Score increased for invalid team");
            }
            if (!(RedTeamScore >= 3 || BlueTeamScore >= 3))
                Center.PrepareReset(step);
            else if (RedTeamScore >= 3) {
                RedTeamGoal.SpawnFireworksStart = step;
                RedTeamGoal.SpawnRed = true;
                BlueTeamGoal.SpawnFireworksStart = step;
                BlueTeamGoal.SpawnRed = true;
            }
            else if (BlueTeamScore >= 3) {
                RedTeamGoal.SpawnFireworksStart = step;
                RedTeamGoal.SpawnRed = false;
                BlueTeamGoal.SpawnFireworksStart = step;
                BlueTeamGoal.SpawnRed = false;
            }
            Console.WriteLine("Increased Score.");
        }

        public bool IsReady() {
            return RedTeamGoal != null && BlueTeamGoal != null && Center != null;
        }

        public void ScanSurroundings() {
            Console.WriteLine("Scanning area.");
            var facade = ServerContext.VillageDirector.UniverseFacade;

            var low = ScanCentre - Region;
            var high = ScanCentre + Region;

            var size = high - low + Vector3I.One;

            var tiles = Allocator.TileAllocator.Allocate(size.Volume());

            if (facade.ReadTileRegion(low, size, tiles, TileAccessFlags.None)) {
                var i = 0;
                for (var y = ScanCentre.Y - Region.Y; y <= ScanCentre.Y + Region.Y; ++y) {
                    for (var z = ScanCentre.Z - Region.Z; z <= ScanCentre.Z + Region.Z; ++z) {
                        for (var x = ScanCentre.X - Region.X; x <= ScanCentre.X + Region.X; ++x) {
                            var pos = new Vector3I(x, y, z);
                            var t = tiles[i];
                            i++;
                            if (t.Configuration.Open || t.Configuration.CompoundFiller)
                                continue;

                            if (t.Configuration.Code == "mods.Deamon.Soccer.tile.RedGoal") {
                                TileStateEntityLogic tse;
                                if (facade.TryFetchTileStateEntityLogic(pos, TileAccessFlags.None, out tse)) {
                                    Console.WriteLine("Claiming Red Goal.");
                                    var goal = tse as SoccerGoalTileStateEntityLogic;
                                    goal.Team = 0;
                                    goal.Totem = this;
                                    RedTeamGoal = goal;
                                }

                            }
                            else if (t.Configuration.Code == "mods.Deamon.Soccer.tile.BlueGoal") {
                                TileStateEntityLogic tse;
                                if (facade.TryFetchTileStateEntityLogic(pos, TileAccessFlags.None, out tse)) {
                                    Console.WriteLine("Claiming Blue Goal.");
                                    var goal = tse as SoccerGoalTileStateEntityLogic;
                                    goal.Team = 1;
                                    goal.Totem = this;
                                    BlueTeamGoal = goal;
                                }
                            }
                            else if (t.Configuration.Code == "mods.Deamon.Soccer.tile.Center") {
                                TileStateEntityLogic tse;
                                if (facade.TryFetchTileStateEntityLogic(pos, TileAccessFlags.None, out tse)) {
                                    Console.WriteLine("Claiming Center.");
                                    var center = tse as CenterTileStateEntityLogic;
                                    center.Totem = this;
                                    Center = center;
                                }

                            }
                            if (IsReady()) {
                                BlueTeamScore = 0;
                                RedTeamScore = 0;
                                _gameStarted = false;
                            }
                        }
                    }

                }

                Allocator.TileAllocator.Release(ref tiles);
            }
        }
    }
}
