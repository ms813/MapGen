using SFML.System;
using Newtonsoft.Json;

namespace ReSource
{
    [JsonObject(MemberSerialization.Fields)]
    class WorldMapSaveData
    {
        public string MapName;
        public int BaseSeed;
        public int TileSize;
        public double MaxElevation;
        public double MinElevation;
        public double SeaLevel;
        public double MountainThreshold;
        public int WindThresholdDist;
        public Vector2i MapSize;
        public RandomWalkInitialiser RandomWalkInitialiser;
        public WorldCalendar WorldCalendar;
    }
}