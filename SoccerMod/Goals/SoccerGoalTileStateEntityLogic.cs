using System;
using System.Reflection;
using Plukit.Base;
using SoccerMod.Totem;
using Staxel;
using Staxel.Core;
using Staxel.Entities;
using Staxel.EntityActions;
using Staxel.Items;
using Staxel.Items.ItemComponents;
using Staxel.Logic;
using Staxel.Tiles;
using Staxel.TileStates;

namespace SoccerMod.Goals {
    public class SoccerGoalTileStateEntityLogic : TileStateEntityLogic, IUseItemEntityCallback {
        SoccerGoalComponentBuilder.GoalComponent _goalComponent;
        bool _done;
        TileConfiguration _configuration;
        uint _variant;
        Vector3D _countPosition;
        Shape _boundingShape;
        internal int GoalCount { get; private set; }
        public SoccerTotemTileStateEntityLogic Totem;
        public int Team = -1;
        public Timestep SpawnFireworksStart;
        private Timestep _lastSpawn;
        public bool SpawnRed;

        public SoccerGoalTileStateEntityLogic(Entity entity)
            : base(entity) {
            Entity.Physics.PriorityChunkRadius(0, false);
        }

        public override void PreUpdate(Timestep timestep, EntityUniverseFacade entityUniverseFacade) { }

        public override void Update(Timestep timestep, EntityUniverseFacade universe) {
            Tile tile;
            if (!universe.ReadTile(Location, TileAccessFlags.None, out tile))
                return;
            if ((tile.Configuration != _configuration) || (_variant != tile.Variant())) {
                _done = true;
                if (tile.Configuration == _configuration)
                    universe.RemoveTile(Entity, Location, TileAccessFlags.None);
                return;
            }

            _countPosition = tile.Configuration.TileCenterTop(Location, tile);

            Vector3F tileOffset;
            if (universe.TileOffset(Location, TileAccessFlags.None, out tileOffset))
                _countPosition.Y += tileOffset.Y;

            CheckIfEntityExists();

            if (!IsClaimed() || !Totem.HasGameStarted())
                return;

            if (SpawnFireworksStart + (long)(_goalComponent.FireworkLaunchingLength * Constants.TimestepsPerSecond) > timestep) {
                if (timestep - _lastSpawn > _goalComponent.FireworkTimeBetweenLaunch * Constants.TimestepsPerSecond) {
                    _lastSpawn = timestep;
                    SpawnFireworks(universe);
                }
                return;
            }

            if (Totem.CanStartNewGame())
                return;

            // get all entities inside the goal area
            var entities = universe.FindAllEntitiesInRange(tile.Configuration.TileCenter(Location, tile.Variant()), (float)_boundingShape.Radius.Length(),
                FindEntityCondition);
            if (entities == null || entities.Count <= 0)
                return;

            // if an item has moved into the goal area increase the score
            Console.WriteLine("Checking all entities in collision.");
            foreach (var entity in entities) {
                var itemlogic = entity.Logic as ItemEntityLogic;
                if (itemlogic == null)
                    continue;
                Console.WriteLine("There is an item logic.");
                ItemStack stack = (ItemStack)GetInstanceField(typeof(ItemEntityLogic), itemlogic, "Stack"); //Use reflection to find internal stack
                if (stack.Item.Configuration.Code == "mods.Deamon.Soccer.item.SoccerBall") {
                    SetInstanceField(typeof(ItemEntityLogic), itemlogic, "Stack", new ItemStack());
                    Totem.IncreaseScore(Team, timestep); //Increase the score
                }
            }
        }

        public void CheckIfEntityExists() {
            if (Totem != null && Totem.IsLingering())
                Totem = null;
        }

        public void SpawnFireworks(EntityUniverseFacade universe) {
            Item item;
            if (SpawnRed)
                item = GameContext.ItemDatabase.SpawnItem(_goalComponent.RedGoalFireworkItem, null);
            else
                item = GameContext.ItemDatabase.SpawnItem(_goalComponent.BlueGoalFireworkItem, null);

            for (var i = 0; i < _goalComponent.FireworkItemQuantity; ++i) {
                if (item.Configuration.Components.Contains<FireworkComponent>())
                    FireworkEntityBuilder.SpawnFirework(Entity, universe, item, _countPosition,
                        _goalComponent.FireworkLaunchVelocity +
                        _goalComponent.FireworkLaunchVelocitySpread * GameContext.RandomSource.NextVector3DInSphere(),
                        _goalComponent.FireworkFlightSeconds +
                        _goalComponent.FireworkFlightSecondsSpread * GameContext.RandomSource.NextFloat(0f, 1f));
                else
                    ItemEntityBuilder.SpawnDroppedItem(Entity, universe, new ItemStack(item, 1), _countPosition,
                        _goalComponent.FireworkLaunchVelocity +
                        _goalComponent.FireworkLaunchVelocitySpread * GameContext.RandomSource.NextVector3DInSphere(),
                        Vector3D.Zero, SpawnDroppedFlags.AchievementValid | SpawnDroppedFlags.SpawnJitter);
            }
        }

        public bool IsClaimed() {
            return Totem != null;
        }

        bool FindEntityCondition(Entity entity) {
            var logic = entity.Logic as ItemEntityLogic;
            if (logic == null)
                return false;
            if (_goalComponent == null)
                return _boundingShape.Collision(entity.Physics.BoundingShape);

            var found = false;
            ItemStack stack = (ItemStack)GetInstanceField(typeof(ItemEntityLogic), logic, "Stack");
            //Console.WriteLine("Succeeding in finding Item Stack.");

            foreach (var category in stack.Item.Configuration.Categories) {
                if (!_goalComponent.ScoreWithCategories.Contains(category))
                    continue;
                found = true;
                break;
            }
            return found && _boundingShape.Collision(entity.Physics.BoundingShape);
        }

        internal static object GetInstanceField(Type type, object instance, string fieldName) {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        internal static void SetInstanceField(Type type, object instance, string fieldName, object value) {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            field.SetValue(instance, value);
        }


        public override void PostUpdate(Timestep timestep, EntityUniverseFacade universe) {
            if (_done)
                universe.RemoveEntity(Entity.Id);
        }

        public Vector3D GetCountPosition() {
            return _countPosition;
        }

        // if entry was accidentally a list, convert to a blob
        static Blob BackwardsCompatFetchBlob(Blob blob, string name) {
            BlobEntry entry;
            if (!blob.KeyValueIteratable.TryGetValue(name, out entry))
                return blob.FetchBlob(name);
            if (entry.Kind != BlobEntryKind.List)
                return entry.Blob();
            blob.Delete(name);
            return blob.FetchBlob(name);
        }
        public override void Store() {
            base.Store();
            _blob.FetchBlob("location").SetVector3I(Location);
            _blob.SetLong("variant", _variant);
            _blob.SetBool("done", _done);
            _blob.FetchBlob("countPosition").SetVector3D(_countPosition);
            _boundingShape.SetBlob(_blob.FetchBlob("boundingShape"));
            _blob.SetString("tile", _configuration.Code);
            _blob.SetLong("goalCount", GoalCount);
        }

        public override void Restore() {
            base.Restore();
            Location = _blob.FetchBlob("location").GetVector3I();
            _variant = (uint)_blob.GetLong("variant");
            _done = _blob.GetBool("done");
            _countPosition = _blob.GetBlob("countPosition").GetVector3D();
            _boundingShape = Shape.FromBlob(_blob.FetchBlob("boundingShape"));
            _configuration = GameContext.TileDatabase.GetTileConfiguration(_blob.GetString("tile"));
            _goalComponent = _configuration.Components.Get<SoccerGoalComponentBuilder.GoalComponent>();
            GoalCount = (int)_blob.GetLong("goalCount", 0);
        }

        public override void Construct(Blob arguments, EntityUniverseFacade entityUniverseFacade) {
            _configuration = GameContext.TileDatabase.GetTileConfiguration(arguments.GetString("tile"));
            _goalComponent = _configuration.Components.GetOrDefault<SoccerGoalComponentBuilder.GoalComponent>();
            var tile = _configuration.MakeTile();
            Vector3D ignored;
            _boundingShape = tile.Configuration.FetchBoundingBox(tile.Variant(), out ignored).ToShape().Scale(_goalComponent == null ? 1f : _goalComponent.HitBoxScale);
            Location = arguments.FetchBlob("location").GetVector3I();
            _variant = (uint)arguments.GetLong("variant");
            Entity.Physics.Construct(arguments.FetchBlob("position").GetVector3D(), Vector3D.Zero);
            Entity.Physics.MakePhysicsless();
        }

        public override void Bind() { }

        public override void Interact(Entity entity, EntityUniverseFacade facade, ControlState main, ControlState alt) {
            if (alt.DownClick)
                GoalCount = 0;
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
            data.FetchBlob("countPosition").SetVector3D(_countPosition);
            _boundingShape.SetBlob(data.FetchBlob("boundingShape"));
            data.SetLong("goalCount", GoalCount);
        }

        public override void RestoreFromPersistedData(Blob data, EntityUniverseFacade facade) {
            Entity.Construct(data.GetBlob("constructData"), facade);
            base.RestoreFromPersistedData(data, facade);
            _done = data.GetBool("done");
            _countPosition = data.GetBlob("countPosition").GetVector3D();
            GoalCount = (int)data.GetLong("goalCount", 0);
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

        public void UseItem(Entity entity, EntityUniverseFacade facade, Vector3I position) { }
    }
}
