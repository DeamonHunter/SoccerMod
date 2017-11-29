using System;
using Plukit.Base;
using Staxel;
using Staxel.Core;
using Staxel.Entities;
using Staxel.Items;
using Staxel.Logic;
using Staxel.TileStates;
using Staxel.TileStates.Totems;

namespace SoccerMod {
    public class SoccerTotemTileStateEntityLogic : TotemTileStateEntityLogic {
        private Timestep _timer;

        public int RedTeamScore;
        public SoccerGoalTileStateEntityLogic RedTeamGoal;
        public int BlueTeamScore;
        public SoccerGoalTileStateEntityLogic BlueTeamGoal;
        public CenterTileStateEntityLogic Center;

        private bool _gameStarted;


        public SoccerTotemTileStateEntityLogic(Entity entity) : base(entity) { }

        public override void Update(Timestep timestep, EntityUniverseFacade universe) {
            base.Update(timestep, universe);
            if (_timer == Timestep.Null) {
                _timer = timestep;
            }
            if (timestep > _timer + 3000000) {
                _timer = timestep;
                if (!IsReady())
                    ScanSurroundings();
            }

            //if (IsReady() && _notSpawnedBall) {
            //    Console.WriteLine("Spawning new Ball.");
            //    ResetBall(universe);
            //    _notSpawnedBall = false;
            //}
        }



        public bool HasGameStarted() {
            return _gameStarted;
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
        }

        public override void Store() {
            base.Store();
            _blob.SetBool("gameStarted", _gameStarted);
        }

        public override void Restore() {
            base.Restore();
            _gameStarted = _blob.GetBool("gameStarted", false);
        }

        public override void StorePersistenceData(Blob data) {
            base.StorePersistenceData(data);
            data.SetBool("gameStarted", _gameStarted);
        }

        public override void RestoreFromPersistedData(Blob data, EntityUniverseFacade facade) {
            base.RestoreFromPersistedData(data, facade);
            _gameStarted = data.GetBool("gameStarted", false);
        }

        public void IncreaseScore(int team) {
            switch (team) {
                case 1:
                    RedTeamScore++;
                    break;
                case 0:
                    BlueTeamScore++;
                    break;
                default:
                    throw new Exception("Score increased for invalid team");
            }
            Console.WriteLine("Increased Score.");
        }

        public bool IsReady() {
            return RedTeamGoal != null && BlueTeamGoal != null && Center != null;
        }

        public void ScanSurroundings() {
            Console.WriteLine("Scanning area.");
            var facade = ServerContext.VillageDirector.UniverseFacade;
            var region = TotemConfig.Region;


            var low = ScanCentre - region;
            var high = ScanCentre + region;

            var size = high - low + Vector3I.One;

            var tiles = Allocator.TileAllocator.Allocate(size.Volume());

            if (facade.ReadTileRegion(low, size, tiles, TileAccessFlags.None)) {
                var i = 0;
                for (var y = ScanCentre.Y - region.Y; y <= ScanCentre.Y + region.Y; ++y) {
                    for (var z = ScanCentre.Z - region.Z; z <= ScanCentre.Z + region.Z; ++z) {
                        for (var x = ScanCentre.X - region.X; x <= ScanCentre.X + region.X; ++x) {
                            var pos = new Vector3I(x, y, z);
                            var t = tiles[i];
                            i++;
                            if (t.Configuration.Open || t.Configuration.CompoundFiller)
                                continue;

                            if (t.Configuration.Code == "mods.Deamon.Soccer.RedGoal") {
                                TileStateEntityLogic tse;
                                if (facade.TryFetchTileStateEntityLogic(pos, TileAccessFlags.None, out tse)) {
                                    Console.WriteLine("Claiming Red Goal.");
                                    var goal = tse as SoccerGoalTileStateEntityLogic;
                                    goal.Team = 0;
                                    goal.Totem = this;
                                    RedTeamGoal = goal;
                                }

                            }
                            else if (t.Configuration.Code == "mods.Deamon.Soccer.BlueGoal") {
                                TileStateEntityLogic tse;
                                if (facade.TryFetchTileStateEntityLogic(pos, TileAccessFlags.None, out tse)) {
                                    Console.WriteLine("Claiming Blue Goal.");
                                    var goal = tse as SoccerGoalTileStateEntityLogic;
                                    goal.Team = 1;
                                    goal.Totem = this;
                                    BlueTeamGoal = goal;
                                }
                            }
                            else if (t.Configuration.Code == "mods.deamon.SoccerMod.Center") {
                                TileStateEntityLogic tse;
                                if (facade.TryFetchTileStateEntityLogic(pos, TileAccessFlags.None, out tse)) {
                                    Console.WriteLine("Claiming Center.");
                                    var center = tse as CenterTileStateEntityLogic;
                                    center.Totem = this;
                                    Center = center;
                                }

                            }
                        }
                    }

                }

                Allocator.TileAllocator.Release(ref tiles);
            }
        }
    }
}
