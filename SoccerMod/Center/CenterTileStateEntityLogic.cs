using System;
using System.Collections.Generic;
using System.Reflection;
using Plukit.Base;
using Staxel;
using Staxel.Core;
using Staxel.Draw;
using Staxel.Entities;
using Staxel.EntityActions;
using Staxel.Items;
using Staxel.Items.ItemComponents;
using Staxel.Logic;
using Staxel.Notifications;
using Staxel.Tiles;
using Staxel.TileStates;

namespace SoccerMod {
    public class CenterTileStateEntityLogic : TileStateEntityLogic {
        bool _done;
        TileConfiguration _configuration;
        uint _variant;
        private CenterComponentBuilder.CenterTotemComponent _component;
        public int Team = -1;
        public SoccerTotemTileStateEntityLogic Totem;
        private Item _ball;
        public Timestep RoundStartedTimestep;
        private bool _ballSpawned;

        public CenterTileStateEntityLogic(Entity entity)
            : base(entity) {
            Entity.Physics.PriorityChunkRadius(0, false);
        }

        public override void PreUpdate(Timestep timestep, EntityUniverseFacade entityUniverseFacade) { }

        public override void Update(Timestep timestep, EntityUniverseFacade universe) {
            Tile tile;
            if (!universe.ReadTile(Location, TileAccessFlags.None, out tile))
                return;
            if ((tile.Configuration != _configuration) || (_variant != tile.Variant()) || (_component == null)) {
                _done = true;
                if (tile.Configuration == _configuration)
                    universe.RemoveTile(Entity, Location, TileAccessFlags.None);
            }
            if (!_ballSpawned && RoundStartedTimestep + 6 * Constants.TimestepsPerSecond < timestep) {
                ResetBall(universe);
            }
        }

        public override void PostUpdate(Timestep timestep, EntityUniverseFacade universe) {
            if (_done)
                universe.RemoveEntity(Entity.Id);
        }

        public bool IsClaimed() {
            return Totem != null;
        }

        public Vector3D GetSpawningPosition() {
            return _configuration.TileCenter(Location, _variant) + _component.BallSpawnLocation;
        }


        public void PrepareReset(Timestep step) {
            _ballSpawned = false;
            RoundStartedTimestep = step;
        }

        public void ResetBall(EntityUniverseFacade universe) {
            var item = new ItemStack(_ball, 1);

            ItemEntityBuilder.SpawnDroppedItem(Entity, universe, item, GetSpawningPosition(),
                new Vector3D(0, 4, 0) +
                new Vector3D(1, 2, 1) * GameContext.RandomSource.NextVector3DInSphere(),
                Vector3D.Zero, SpawnDroppedFlags.None);
            _ballSpawned = true;
        }

        public override void Store() {
            base.Store();
            _blob.FetchBlob("location").SetVector3I(Location);
            _blob.SetLong("variant", _variant);
            _blob.SetBool("done", _done);
            _blob.SetString("tile", _configuration.Code);
            _blob.SetBool("ballSpawned", _ballSpawned);
            _blob.SetTimestep("roundStartedTimestep", RoundStartedTimestep);
        }

        public override void Restore() {
            base.Restore();
            Location = _blob.FetchBlob("location").GetVector3I();
            _variant = (uint)_blob.GetLong("variant");
            _done = _blob.GetBool("done");
            _configuration = GameContext.TileDatabase.GetTileConfiguration(_blob.GetString("tile"));
            _component = _configuration.Components.Get<CenterComponentBuilder.CenterTotemComponent>();
            _ballSpawned = _blob.GetBool("ballSpawned", false);
            RoundStartedTimestep = _blob.GetTimestep("roundStartedTimestep", Timestep.Null);
        }

        public override void Construct(Blob arguments, EntityUniverseFacade entityUniverseFacade) {
            _configuration = GameContext.TileDatabase.GetTileConfiguration(arguments.GetString("tile"));
            _component = _configuration.Components.GetOrDefault<CenterComponentBuilder.CenterTotemComponent>();
            Location = arguments.FetchBlob("location").GetVector3I();
            _variant = (uint)arguments.GetLong("variant");
            Entity.Physics.Construct(arguments.FetchBlob("position").GetVector3D(), Vector3D.Zero);
            Entity.Physics.MakePhysicsless();

            _ball = GameContext.ItemDatabase.SpawnItem(_component.SoccerBall, null);
        }

        public override void Bind() { }

        public override void Interact(Entity entity, EntityUniverseFacade facade, ControlState main, ControlState alt) {
            if (!alt.DownClick)
                return;
            if (!IsClaimed()) {
                var player = entity.PlayerEntityLogic;
                player.ShowNotification(GameContext.NotificationDatabase.CreateNotificationFromCode(_component.NotClaimedNotification, entity.Step, NotificationParams.EmptyParams));
                return;
            }
            if (!Totem.IsReady()) {
                var player = entity.PlayerEntityLogic;
                player.ShowNotification(GameContext.NotificationDatabase.CreateNotificationFromCode(_component.NotClaimedNotification, entity.Step, NotificationParams.EmptyParams));
                return;
            }

            if (!Totem.HasGameStarted()) {
                Totem.ResetGame();
                PrepareReset(facade.Step);
            }

        }



        public string[] GetDrawableNumbers() {
            return _component.numbers;
        }

        public override bool CanChangeActiveItem() {
            return true;
        }

        public override bool IsPersistent() {
            return true;
        }

        public override bool IsAtLastSavedPosition() {
            return true;
        }

        public override ChunkKey GetLastSavedPosition() {
            return new ChunkKey(Entity.Physics.Position);
        }

        public override void StorePersistenceData(Blob data) {
            base.StorePersistenceData(data);
            var constructData = data.FetchBlob("constructData");
            constructData.SetString("tile", _configuration.Code);
            constructData.FetchBlob("location").SetVector3I(Location);
            constructData.SetLong("variant", _variant);
            constructData.FetchBlob("position").SetVector3D(Entity.Physics.Position);
            data.SetBool("done", _done);
            data.SetBool("ballSpawned", _ballSpawned);
            data.SetTimestep("roundStartedTimestep", RoundStartedTimestep);
        }

        public override void RestoreFromPersistedData(Blob data, EntityUniverseFacade facade) {
            Entity.Construct(data.GetBlob("constructData"), facade);
            base.RestoreFromPersistedData(data, facade);
            _done = data.GetBool("done");
            _ballSpawned = _blob.GetBool("ballSpawned", false);
            RoundStartedTimestep = _blob.GetTimestep("roundStartedTimestep", Timestep.Null);
            Store();
        }

        public override bool IsLingering() {
            return _done;
        }


        public override bool Interactable() {
            return true;
        }

        public override void KeepAlive() { }

        public override void BeingLookedAt(Entity entity) { }

        public override bool IsBeingLookedAt() {
            return false;
        }

        public override string AltInteractVerb() {
            return "mods.deamon.soccer.controlHint.verb.SpawnBall";
        }
    }
}
