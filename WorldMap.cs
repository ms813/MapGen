using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace ReSource
{
    class WorldMap
    {   
        private List<MapTile> Tiles = new List<MapTile>();
        private List<Vector2i> activeTileIndices = new List<Vector2i>();
        private List<Landmass> landMasses = new List<Landmass>();

        private List<VertexArray> randomWalks = new List<VertexArray>();
        private VertexArray windArrows;

        private RectangleShape highlightShape;
        private VertexArray tileVisualisation;
        private VertexArray downslopeArrows;
        private bool drawDownslopes = false;
        private bool drawRandomWalks = false;
        private bool drawWind = false;
        private bool drawHighlight = true;

        private RandomWalkInitialiser randomWalkInitialiser;              

        //public static Font Font = new Font(@"..\..\..\resources\fonts\arial.ttf");

        public int TileSize { get; private set; }
        public double MaxElevation { get; private set; }
        public double MinElevation { get; private set; }
        public double SeaLevel { get; private set; }
        public double MountainThreshold { get; private set; }       //cutoff height for a tile to be considered a mountain                                                               
        public int WindThresholdDist { get; private set; } //threshold distance from coast for max wind
        public Vector2i MapSize { get; private set; }

        private int baseSeed;
        private int elevationSeed;
        private int randomWalkSeed;
        private int riverSourceSeed;
        private int windDirectionSeed;
        private int windSpeedSeed;
        private int temperatureSeed;
        private int rainSeed;

        private string MapName; 

        private long TotalBuildTime = 0;

        public WorldMap(WorldMapSaveData mapData)
        {
            //unpack mapData
            this.MapName = mapData.MapName;
            this.baseSeed = mapData.BaseSeed;
            this.TileSize = mapData.TileSize;
            this.MaxElevation = mapData.MaxElevation;
            this.MinElevation = mapData.MinElevation;
            this.SeaLevel = mapData.SeaLevel;
            this.MountainThreshold = mapData.MountainThreshold;
            this.WindThresholdDist = mapData.WindThresholdDist;
            this.MapSize = mapData.MapSize;
            this.randomWalkInitialiser = mapData.RandomWalkInitialiser;

            Init();
        }

        private void Init()
        {
            //initialise random seeds
            Random rnd = new Random(baseSeed);
            elevationSeed = rnd.Next();
            randomWalkSeed = rnd.Next();
            riverSourceSeed = rnd.Next();
            windDirectionSeed = rnd.Next();
            windSpeedSeed = rnd.Next();
            temperatureSeed = rnd.Next();
            rainSeed = rnd.Next();

            //actually build the World Map
            ExecuteTimedFunction(CreateTiles);
            ExecuteTimedFunction(AssignTileNeighbours);
            ExecuteTimedFunction(() => GenerateRandomWalks(randomWalkInitialiser)
                , "Generate Random Walks");
            ExecuteTimedFunction(CalculatePerlinCoefficients);
            //ExecuteTimedFunction(CalculateGaussianCoefficients);
            ExecuteTimedFunction(CalculateVoronoiCoefficients);
            ExecuteTimedFunction(SetTileElevations);
            ExecuteTimedFunction(RescaleElevation);
            ExecuteTimedFunction(AssignOcean);
            ExecuteTimedFunction(AssignLandMasses);
            ExecuteTimedFunction(AssignCoast);
            ExecuteTimedFunction(AssignElevationZones);
            ExecuteTimedFunction(AssignDownslopes);
            ExecuteTimedFunction(GenerateWindDirection);
            ExecuteTimedFunction(CalculateWindSpeed);
            ExecuteTimedFunction(CalculateTemperature);
            ExecuteTimedFunction(CreateRivers);
            ExecuteTimedFunction(CalculateRainShadow);
            ExecuteTimedFunction(AssignRainfall);
            ExecuteTimedFunction(AssignBiomes);
            ExecuteTimedFunction(InitialiseDisplay);
            ExecuteTimedFunction(CreateVertexArray);

            Console.WriteLine("{0} build finished in {1} s", MapName, TotalBuildTime / 1000d);            
            Save();
            Console.WriteLine("--------------------");
            Console.WriteLine();
        }

        public bool IsMapBorder(MapTile t)
        {            
            return (t.GlobalIndex.X == 0)
                || (t.GlobalIndex.Y == 0)
                || (t.GlobalIndex.X == MapSize.X - 1)
                || (t.GlobalIndex.Y == MapSize.Y - 1);
        }
                    
        private void ExecuteTimedFunction(Action action, string msg = "")
        {
            if (msg == "") msg = action.Method.Name;

            Console.Write(msg);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            action();
            watch.Stop();
            TotalBuildTime += watch.ElapsedMilliseconds;
            Console.WriteLine("... finished in {0} s", watch.ElapsedMilliseconds / 1000d);
        }

        private void CreateTiles()
        {
            //create all tiles and assign them a global index            
            for (int x = 0; x < MapSize.X; x++)
            {
                for (int y = 0; y < MapSize.Y; y++)
                {
                    Vector2i tileIndex = new Vector2i(x, y);
                    MapTile tile = new MapTile(this, tileIndex, TileSize);                    
                    Tiles.Add(tile);
                }
            }
        }

        private void AssignTileNeighbours()
        {
            Parallel.ForEach(Tiles, t =>
            {
                foreach (Vector2i dir in MathHelper.CardinalDirections)
                {
                    MapTile neighbour = GetTileByIndex(t.GlobalIndex + dir);
                    if (neighbour != null)
                    {
                        t.OrthogonalNeighbours.Add(neighbour);
                    }
                }

                foreach (Vector2i dir in MathHelper.OrdinalDirections)
                {
                    MapTile neighbour = GetTileByIndex(t.GlobalIndex + dir);
                    if (neighbour != null)
                    {
                        t.DiagonalNeighbours.Add(neighbour);
                    }
                }
            });
        }
        
        private void GenerateRandomWalks(RandomWalkInitialiser initialiser)
        {  
            //generate a list of random walks that will be used to generate voronoi diagrams

            //add a line to the left and right sides if requested
            if (initialiser.SideOcean)
            {
                VertexArray left = new VertexArray(PrimitiveType.LinesStrip);
                VertexArray right = new VertexArray(PrimitiveType.LinesStrip);

                //go up in step size of 5% of the map edge
                //this gets the same effect but makes it much much faster!
                for (int i = 0; i < MapSize.Y; i += MapSize.Y / 20)
                {
                    Vertex l = new Vertex(new Vector2f(0, i) * TileSize);
                    Vertex r = new Vertex(new Vector2f(MapSize.X, i) * TileSize);

                    left.Append(l);
                    right.Append(r);
                }
                randomWalks.Add(left);
                randomWalks.Add(right);
            }

            //add a line to the top and bottom sides if requested
            if (initialiser.TopBottomOcean)
            {
                VertexArray top = new VertexArray(PrimitiveType.LinesStrip);
                VertexArray bot = new VertexArray(PrimitiveType.LinesStrip);
                for (int i = 0; i < MapSize.X; i += MapSize.X / 20)
                {
                    Vertex t = new Vertex(new Vector2f(i, 0) * TileSize);
                    Vertex b = new Vertex(new Vector2f(i, MapSize.Y) * TileSize);

                    top.Append(t);
                    bot.Append(b);
                }
                randomWalks.Add(top);
                randomWalks.Add(bot);
            }

            //generate some random walks and add them to a list
            List<Vector2i> startPositions = new List<Vector2i>();
            Random random = new Random(randomWalkSeed);
            //walks start in middle 80% of map
            for (int i = 0; i < initialiser.MidRandomWalks; i++)
            {
                int x = random.Next((int)Math.Round(0.8d * MapSize.X)) + (int)Math.Round(0.1d * MapSize.X);
                int y = random.Next((int)Math.Round(0.8d * MapSize.Y)) + (int)Math.Round(0.1d * MapSize.Y);
                startPositions.Add(new Vector2i(x, y));
            }

            //walks start from edge
            for (int i = 0; i < initialiser.EdgeRandomWalks; i++)
            {
                int rnd = random.Next(4);
                if (rnd == 0)
                {
                    //start on the top row
                    startPositions.Add(new Vector2i(random.Next(MapSize.X), 0));
                }
                else if (rnd == 1)
                {
                    //start on right column
                    startPositions.Add(new Vector2i(MapSize.X, random.Next(MapSize.Y)));
                }
                else if (rnd == 2)
                {
                    //start on bottom row
                    startPositions.Add(new Vector2i(random.Next(MapSize.X), MapSize.Y));
                }
                else if (rnd == 3)
                {
                    //start on left column
                    startPositions.Add(new Vector2i(0, random.Next(MapSize.Y)));
                }
            }

            RandomWalker rndWalk = new RandomWalker(random);
            IntRect bounds = new IntRect(0, 0, MapSize.X * TileSize, MapSize.Y * TileSize);
            foreach (Vector2i startPos in startPositions)
            {               
                //randomWalks.Add(RandomWalker.GridWalk(startPos * TileSize, bounds, TileSize, steps));
                Vector2f start = new Vector2f(startPos.X, startPos.Y) * TileSize;
                randomWalks.Add(rndWalk.RandomWalk(start, bounds, MapSize.Y, initialiser.RandomWalkSteps));
            }
        }        

        private void CalculatePerlinCoefficients()
        {
            PerlinGenerator perlin = new PerlinGenerator(elevationSeed);
            foreach(MapTile t in Tiles)
            {
                //get Perlin noise to get base elevation value            
                int featureScale = MapSize.Y / 8;
                t.ElevationPerlin = perlin.OctavePerlin(
                        (double)t.GlobalIndex.X / featureScale,
                        (double)t.GlobalIndex.Y / featureScale,
                        4, 0.5);
            }
            NormalisePerlinCoefficients();
        }

        private void NormalisePerlinCoefficients()
        {
            //get highest and lowest elevations and normalise to world min and max
            double min = Tiles.Min(t => t.ElevationPerlin);
            double max = Tiles.Max(t => t.ElevationPerlin);            

            foreach (MapTile t in Tiles)
            {
                t.ElevationPerlin = MathHelper.Scale(min, max, 0, 1, t.ElevationPerlin);                
            }
        }

        private void CalculateGaussianCoefficients()
        {
            foreach (MapTile t in Tiles)
            {
                SetTileGaussian(t);
            }
        }

        private void SetTileGaussian(MapTile tile)
        {
            //generate a Gaussian distribution in the X direction to make oceans at the sides
            double scaleX = MathHelper.Scale(0, MapSize.X, -1.5, 1.5, tile.GlobalIndex.X);
            double elevationDiff = MaxElevation - MinElevation;
            double gaussianWidth = 1;
            tile.ElevationGaussian = elevationDiff * Math.Exp(-Math.Pow(scaleX, 2d) / gaussianWidth) - (((double)elevationDiff - MaxElevation) / (double)elevationDiff);
        }

        private void CalculateVoronoiCoefficients()
        {           
            int voronoiCount = 0;
            
            System.Timers.Timer timer = new System.Timers.Timer();
            decimal percent = 0;
            timer.Elapsed += (sender, e) =>
            {
                percent = Math.Round((decimal)voronoiCount / Tiles.Count * 100m, 2);
                Console.SetCursorPosition(System.Reflection.MethodBase.GetCurrentMethod().Name.Length, Console.CursorTop);
                Console.Write("{0}%", percent);                
            };
            timer.Interval = 1000;
            timer.Enabled = true;
            
            Parallel.ForEach(Tiles, (t) =>
            {
                SetTileVoronoi(t);
                voronoiCount++;      
            });          

            Console.SetCursorPosition(System.Reflection.MethodBase.GetCurrentMethod().Name.Length, Console.CursorTop);
            timer.Dispose();

            NormaliseVoronoiCoefficients();
        }
        
        private void SetTileVoronoi(MapTile tile)
        {
            //find distance to nearest random walk line         

            Vector2f worldPos = TileSize * (Vector2f)tile.GlobalIndex;
            double min = Double.MaxValue;
            foreach (VertexArray va in randomWalks)
            {
                for (uint i = 0; i < va.VertexCount; i++)
                {
                    double sqDist = Math.Pow(worldPos.X - va[i].Position.X, 2) + Math.Pow(worldPos.Y - va[i].Position.Y, 2);
                    if (sqDist < min)
                    {
                        min = sqDist;
                    }
                }
            }
            tile.ElevationVoronoi = min;
        }       

        private void NormaliseVoronoiCoefficients()
        {
            //get highest and lowest elevations and normalise to world min and max
            double min = Tiles.Min(t => t.ElevationVoronoi);
            double max = Tiles.Max(t => t.ElevationVoronoi);
         
            foreach (MapTile t in Tiles)
            {
                t.ElevationVoronoi = MathHelper.Scale(min, max, 0.1, 1.5, t.ElevationVoronoi);
            }
        }

        private void SetTileElevations()
        {
            //multiply the weighting factors to get the final elevations
            foreach (MapTile tile in Tiles)
            {
                //tile.Elevation = tile.Perlin * tile.Gaussian * tile.Voronoi;  
                tile.Elevation = tile.ElevationPerlin * tile.ElevationVoronoi;
            }
        } 

        private void RescaleElevation()
        {
            //get highest and lowest elevations and normalise to world min and max
            double min = Tiles.Min(t => t.Elevation);
            double max = Tiles.Max(t => t.Elevation);

            foreach (MapTile t in Tiles)
            {
                t.Elevation = MathHelper.Scale(min, max, MinElevation, MaxElevation, t.Elevation);               
            }
        }      
        
        private void AssignOcean()
        {            
            Queue<MapTile> fillQ = new Queue<MapTile>();

            //go around the edge of the map and make edge chunks assign water at the edge of the map
            foreach(MapTile tile in Tiles)
            {
                if (IsMapBorder(tile) && tile.Elevation < SeaLevel)
                {
                    tile.Water = WaterType.Ocean;
                    fillQ.Enqueue(tile);
                }
            }
           
            //assign any tiles with elevation < 0 touching ocean as oceans            
            while(fillQ.Count > 0)
            {
                MapTile tile = fillQ.Dequeue();

                foreach (MapTile neighbour in tile.OrthogonalNeighbours)
                {
                    if (neighbour.Elevation < SeaLevel && neighbour.Water == WaterType.Unassigned)
                    {
                        neighbour.Water = WaterType.Ocean;
                        fillQ.Enqueue(neighbour);
                    }
                }                
            }
        }     

        private Landmass CreateLandmass(MapTile firstTile, int landmassId)
        {
            Queue<MapTile> fillQ = new Queue<MapTile>();
            Landmass lm = new Landmass(landmassId);

            lm.AddTile(firstTile);
            firstTile.LandmassId = landmassId;
            
            fillQ.Enqueue(firstTile);

            while (fillQ.Count > 0)
            {
                MapTile tile = fillQ.Dequeue();
                if(tile.Water == WaterType.Unassigned)
                {
                    //need to check this both here and in the neighbours loop
                    //otherwise it bugs out (I think for very small islands where the queue empties too quickly)                    
                    if (tile.Elevation > SeaLevel)
                    {
                        tile.Water = WaterType.Land;
                    }
                    else
                    {
                        tile.Water = WaterType.Lake;
                    }
                }
                foreach (MapTile n in tile.OrthogonalNeighbours)
                {
                      if (n.Water == WaterType.Unassigned)
                      {                        
                        lm.AddTile(n);
                        n.LandmassId = landmassId;

                        if (n.Elevation > SeaLevel)
                        {
                            n.Water = WaterType.Land;
                        }
                        else
                        {
                            n.Water = WaterType.Lake;
                        }
                        fillQ.Enqueue(n);
                    }
                }             
            }

            return lm;
        }

        private void AssignLandMasses()
        {
            //loop over the tiles and assign Water == WaterType.Unassigned to land masses
            //using a flood fill
            int landmassCount = 0;
            List<MapTile> unassignedTiles = Tiles.Where(t => t.Water == WaterType.Unassigned).ToList();
            
            while (unassignedTiles.Count() > 0){
                landMasses.Add(CreateLandmass(unassignedTiles.First(), landmassCount++));
                unassignedTiles = Tiles.Where(t => t.Water == WaterType.Unassigned).ToList();
            }          
        }

        private void AssignCoast()
        {
            foreach(Landmass lm in landMasses)
            {
                foreach(MapTile tile in lm.Tiles)
                {
                    int oceanNeighbour = 0;
                    int landNeighbour = 0;

                    foreach (MapTile n in tile.OrthogonalNeighbours)
                    {
                        if (n.Water == WaterType.Land)
                        {
                            landNeighbour++;
                        }

                        if (n.Water == WaterType.Ocean)
                        {
                            oceanNeighbour++;
                        }
                    }

                    tile.Coast = (tile.Water == WaterType.Land) && (oceanNeighbour > 0) && (landNeighbour > 0);
                }
            }
        }    

        private void AssignElevationZones()
        {
            foreach(MapTile t in Tiles)
            {
                t.ElevationZone = GetElevationZone(t);
            }
        }

        private ClimateZone GetElevationZone(MapTile t)
        {
            //need to check if ocean first
            //land zones are then divided by the above sea level zones
            if (t.Water == WaterType.Ocean)
            {
                //return one of the three ocean zones
                if (t.Elevation < 0.25 * SeaLevel)
                {
                    //bottom of the ocean, return depths zone
                    return ClimateZone.GetElevationZone(0);
                }
                else if (t.Elevation > 0.75 * SeaLevel)
                {
                    //returns shallows zone
                    return ClimateZone.GetElevationZone(2);
                }
                else
                {
                    //returns oceans zone
                    return ClimateZone.GetElevationZone(1);
                }
            }
            else
            {
                double e = MathHelper.Scale(SeaLevel, MaxElevation, 3, ClimateZone.ElevationZones.Count() - 0.001f, t.Elevation);
                return ClimateZone.GetElevationZone((int)Math.Floor(e));
            }
        }
     
        private void AssignDownslopes()
        {
            Parallel.ForEach(Tiles, tile => AssignTileDownslope(tile));
            Parallel.ForEach(Tiles, tile => AssignTileDownhillToSea(tile));
        }
        
        private void AssignTileDownslope(MapTile tile)
        {
            double low = tile.Elevation;
            Vector2i lowDir = new Vector2i(0, 0);

            foreach (MapTile neighbour in tile.OrthogonalNeighbours)
            {
                if (neighbour.Elevation < low)
                {
                    low = neighbour.Elevation;
                    lowDir = neighbour.GlobalIndex - tile.GlobalIndex;
                }
            }
            tile.DownslopeDir = lowDir;   
        }

        private void AssignTileDownhillToSea(MapTile tile)
        {
            MapTile nextTile = tile;
            while (tile != null)
            {
                Vector2i nextDownhillTile = nextTile.GlobalIndex + nextTile.DownslopeDir;
                nextTile = GetTileByIndex(nextDownhillTile);

                if(nextTile == null || nextTile.Water == WaterType.Ocean)
                {
                    tile.DownhillToSea = true;
                    return;
                }
                
                if (nextTile.DownslopeDir == new Vector2i(0, 0))
                {
                    tile.DownhillToSea = false;
                    return;
                }
            }             
        }

        private void GenerateWindDirection()
        {
            //refresh the perlin generator to get new noise values
            PerlinGenerator perlin = new PerlinGenerator(windDirectionSeed);

            //assign each tile a wind direction
            foreach (MapTile t in Tiles)
            {
                t.PrevailingWindDir = WindHelper.GetPrevailingWindDirection(t);
                
                int featureScale = t.ParentMap.MapSize.Y / 8;
                t.WindNoise = perlin.OctavePerlin(
                    (double)t.GlobalIndex.X / featureScale,
                    (double)t.GlobalIndex.Y / featureScale,
                    5, 0.5);
            }

            //normalise wind noise to between -Pi < x < Pi
            double min = Tiles.Min(t => t.WindNoise);
            double max = Tiles.Max(t => t.WindNoise);

            foreach (MapTile t in Tiles)
            {
                t.WindNoise = MathHelper.Scale(min, max, -Math.PI, Math.PI, t.WindNoise);
                t.WindDirection = WindHelper.GetWindDirection(t);
            }

            //normalise the wind direction to 0 < x < 2pi
            min = Tiles.Min(t => t.WindDirection);
            max = Tiles.Max(t => t.WindDirection);
            foreach (MapTile t in Tiles)
            {
                t.WindDirection = MathHelper.Scale(min, max, 0, 2d * Math.PI, t.WindDirection);
            }
        }

        private void CalculateWindSpeed()
        {
            //we need to generate a new noise map for the wind speed
            //so randomise the perlin generator again
            PerlinGenerator perlin = new PerlinGenerator(windSpeedSeed);

            foreach(MapTile t in Tiles)
            {
                int featureScale = t.ParentMap.MapSize.Y / 8;
                double noise = perlin.OctavePerlin(
                    (double)t.GlobalIndex.X / featureScale,
                    (double)t.GlobalIndex.Y / featureScale,
                    5, 0.5);

                //base level wind strength map mixture of noise and latitude
                t.BaseWindStrength = noise + WindHelper.GetBaseWindStrength(t);
            }

            //normalise base wind strength map between 0 and 1
            double min = Tiles.Min(t => t.BaseWindStrength);
            double max = Tiles.Max(t => t.BaseWindStrength);
            foreach(MapTile t in Tiles)
            {
                t.BaseWindStrength = MathHelper.Scale(min, max, 0, 1, t.BaseWindStrength);
            }            

            //calculate continent wind strength based on distance to coast
            //do each land tile first
            Console.WriteLine();
            for(int i = 0; i < landMasses.Count; i++) { 
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("  Calculating distance to coast for landmass {0} of {1}", i + 1, landMasses.Count);

                //get a list of the coast tiles in each landmass
                List<MapTile> coastTiles = landMasses[i].GetCoastTiles();

                //set the distance to the coast for these to 0
                coastTiles.ForEach(t => t.DistanceToCoast = 0);

                Parallel.ForEach(coastTiles, coastTile =>
                {
                    List<MapTile> surrounding = GetTilesSurrounding(coastTile, WindThresholdDist);

                    foreach (MapTile surroundingTile in surrounding)
                    {
                        int dist = MathHelper.TaxicabDistance(surroundingTile.GlobalIndex, coastTile.GlobalIndex);

                        if (dist < surroundingTile.DistanceToCoast)
                        {
                            surroundingTile.DistanceToCoast = dist;
                        }
                    }
                });
            }
          
            foreach(MapTile t in Tiles)
            {
                //scale wind strength using the threshold value
                double x = (double)t.DistanceToCoast / WindThresholdDist;
                x = MathHelper.Clamp(x, 1, 0);
                if(t.Water == WaterType.Ocean)
                {
                    t.ContinentWindStrength = MathHelper.SmoothStep((1d + x) / 2d);
                }
                else
                {
                    t.ContinentWindStrength = MathHelper.SmoothStep((1d - x) / 2d);
                }                
            }

            //add the base speed and the continent speed to get the wind speed
            foreach(MapTile t in Tiles)
            {               
                //t.WindStrength = t.BaseWindStrength * t.ContinentWindStrength;
                t.WindStrength = t.BaseWindStrength + t.ContinentWindStrength;
                //t.WindStrength = t.ContinentWindStrength;
            }

            min = Tiles.Min(t => t.WindStrength);
            max = Tiles.Max(t => t.WindStrength);
            foreach(MapTile t in Tiles)
            {
                t.WindStrength = MathHelper.Scale(min, max, 0, 1, t.WindStrength);
            }
        }

        
        private void CalculateTemperature()
        {
            //re-randomise the perlin generator as we will be using it to create noise
            PerlinGenerator perlin = new PerlinGenerator(temperatureSeed);
            double temperatureDisortionFactor = 0.5d;
            double oceanTempScaleFactor = 1.0d;

            foreach (MapTile t in Tiles)
            {
                double fractionalLatitude = (double)t.GlobalIndex.Y / MapSize.Y;
                double tBase;              
                if(fractionalLatitude < 0.5)
                {
                    //tile is in the northern hemisphere

                    //assign temperature of 0 at the top, 1 at the equator
                    
                    //smoothstep
                    //tBase = MathHelper.SmoothStep(2 * fractionalLatitude);
                    
                    //linear
                    tBase = 2 * fractionalLatitude;
                }
                else
                {
                    //tile is in the southern hemisphere

                    //assign temperature of 1 in the middle, 0 at the bottom
                    
                    //smoothstep
                    //tBase = MathHelper.SmoothStep(2 * (1 - fractionalLatitude));
                    
                    //linear
                    tBase = 2 * (1 - fractionalLatitude);                           
                }

                int featureScale = MapSize.Y / 8;
                double tNoise = perlin.OctavePerlin(
                    (double)t.GlobalIndex.X / featureScale,
                    (double)t.GlobalIndex.Y / featureScale,
                    5, 0.5);

                t.Temperature = tBase + (temperatureDisortionFactor * tNoise);                                            
            }

            //normalise temperature to between 0 and 1
            //and set temperature zone
            double min = Tiles.Min(t => t.Temperature);
            double max = Tiles.Max(t => t.Temperature);
            foreach(MapTile t in Tiles)
            {
                t.Temperature = MathHelper.Scale(min, max, 0, 1, t.Temperature);

                //reduce sea temperatures after finding min so as not to skew cold areas
                if (t.Water == WaterType.Ocean)
                {
                    t.Temperature *= oceanTempScaleFactor;
                }

                //make high elevation areas colder after finding min so mountains don't skew the 
                //polar temperatures higher
                if (t.Elevation > MountainThreshold)
                {
                    double fractionalEle = MathHelper.Scale(MountainThreshold, MaxElevation, 0, 1, t.Elevation);
                    t.Temperature *= (1 - fractionalEle);
                }  

                int z = (int)Math.Floor(ClimateZone.TemperatureZones.Count() * t.Temperature);
                if (z == ClimateZone.TemperatureZones.Count()) z--;
                t.TemperatureZone = ClimateZone.TemperatureZones[z];
            }        
        }
        
        private void CreateRivers()
        {
            List<MapTile> riverSources = GenerateRiverSources((int)Math.Sqrt(MapSize.Y) * 2);
            foreach (MapTile t in riverSources)
            {
                ExtendRiverFromSource(t);
            }
        }

        private List<MapTile> GenerateRiverSources(int riverCount)
        {                      
            //get a list of tiles...
            IEnumerable<MapTile> mountains = Tiles
                .Where(t => t.DownhillToSea                 //which have a downhill path to the sea
                    && t.Water == WaterType.Land            //which are land
                    && t.Elevation >= MountainThreshold);   //and which are high enough to be considered mountains

            Random random = new Random(riverSourceSeed);
            return mountains
                .OrderBy(t => random.NextDouble())  //shuffle the list of mountains
                .Take(riverCount).ToList();                 //then take the first # as required
        }

        private void ExtendRiverFromSource(MapTile riverTile)
        {
            riverTile.RiverSource = true;
            while (riverTile != null && riverTile.Water == WaterType.Land)
            {
                riverTile.RiverVolume++;
                Vector2i nextRiverIndex = riverTile.GlobalIndex + riverTile.DownslopeDir;

                //add small chance for river to deviate sideways
                /*
                foreach(MapTile n in riverTile.OrthogonalNeighbours)
                {
                    if (n.Elevation < riverTile.Elevation
                        && n.RiverVolume == 0
                        && n.DownhillToSea)
                    {
                        if (random.NextDouble() > 0.95d)
                        {
                            nextRiverIndex = n.GlobalIndex;
                            break;
                        }
                    }
                }      */         

                riverTile = GetTileByIndex(nextRiverIndex);                

                if (riverTile.DownslopeDir == new Vector2i(0, 0))
                {
                    Console.WriteLine("River reached a minima, shouldnt happen!");
                }
            }            
        }         
   
        private void CalculateRainShadow()
        {
            //loop over non-ocean tiles
            foreach (MapTile t in Tiles.Where(t => t.Water != WaterType.Ocean))
            {
                //follow the wind direction backwards until we reach a water tile//Create a path of MapTiles by following the wind direction backwards until an ocean tile is reached
                List<MapTile> tilePath = new List<MapTile>();

                MapTile currentTile = t;
                while(currentTile != null && currentTile.Water != WaterType.Ocean)
                {
                    //get the principal direction opposite to which the wind is flowing
                    //(note the negative sign)
                    Vector2i reverseWindDir = -MathHelper.ToPrincipalDirection(t.WindDirection);                    

                    tilePath.Add(currentTile);

                    //move on to the next tile in the path by adding the current tile index to the reverse wind direction
                    currentTile = GetTileByIndex(currentTile.GlobalIndex + reverseWindDir);
                }

                //find the highest point in the path, then check if it is above the mountain threshhold
                double maxHeightInPath = tilePath.Max(pathTile => pathTile.Elevation);
                MapTile highestTile = tilePath.Find(pathTile => pathTile.Elevation == maxHeightInPath);
                double rainShadowThresholdDist = 50d;

                if(maxHeightInPath >= MountainThreshold)
                {
                    //there is a mountain between this tile and water, so it is in the rain shadow

                    //find the distance between the high point and the current tile
                    double dist = MathHelper.TaxicabDistance(t.GlobalIndex, highestTile.GlobalIndex);

                    //clamp the distance to the threshold distance
                    dist = MathHelper.Clamp(dist, rainShadowThresholdDist, 0);

                    //rescale the max height to between 0 and 1
                    double maxHeightScale = MathHelper.Scale(MountainThreshold, MaxElevation, 0, 1, maxHeightInPath);

                    //scale the rain shadow with distance to the highest point in the path,
                    //and by the elevation of the highest point in the path
                    double d = (1 - (dist / rainShadowThresholdDist)) * maxHeightScale;
                    t.RainShadow = MathHelper.SmoothStep(d);                    
                }
            }

            //normalise the rain shadow to between 0 and 1 (0 receives full rainfall)
            double min = Tiles.Min(t => t.RainShadow);
            double max = Tiles.Max(t => t.RainShadow);
            Tiles.ToList().ForEach(t => t.RainShadow = MathHelper.Scale(min, max, 0, 1, t.RainShadow));
        }

        private void AssignRainfall()
        {
            PerlinGenerator perlin = new PerlinGenerator(rainSeed);
            foreach(MapTile t in Tiles)
            {
                double featureScale = MapSize.Y / 2;
                t.Rainfall = perlin.OctavePerlin(
                    t.GlobalIndex.X / featureScale,
                    t.GlobalIndex.Y / featureScale,
                    5, 0.5);                
            }

            //normalise the noise, then scale by the rainshadow
            double min = Tiles.Min(t => t.Rainfall);
            double max = Tiles.Max(t => t.Rainfall);            
            
            foreach (MapTile t in Tiles)
            {
                t.Rainfall = MathHelper.Scale(min, max, 0, 1, t.Rainfall);
                t.Rainfall *= (1 - t.RainShadow);
                //set humidityZone here
                int z = (int)Math.Floor(ClimateZone.HumidityZones.Count() * t.Rainfall);
                if (z == ClimateZone.HumidityZones.Count()) z--;
                t.HumidityZone = ClimateZone.HumidityZones[z];
            }            
        }  

        private void AssignBiomes()
        {
            Dictionary<string, int> count = new Dictionary<string, int>();
            foreach(string b in Biome.Biomes.Keys)
            {
                count.Add(b, 0);
            }

            foreach(MapTile t in Tiles)
            {
                t.Biome = Biome.GetBiome(t);
                count[t.Biome.Name]++;
            }

            Console.WriteLine();
            foreach (KeyValuePair<string, int> pair in count)
            {                
                Console.WriteLine("Biome: {0}, count: {1}", pair.Key, pair.Value);
            }
        }        

        private void InitialiseDisplay()
        {
            TileColourHelper colorHelper = new TileColourHelper();
            foreach (MapTile t in Tiles)
            {
                colorHelper.SetElevationColor(t);
            }

            //activate tiles in the middle of the view            
            ActivateTilesAround((int)Math.Floor(MapSize.X / 2d), (int)Math.Floor(MapSize.Y / 2d), 1);

            highlightShape = new RectangleShape(new Vector2f(TileSize, TileSize));
            highlightShape.OutlineColor = Color.White;
            highlightShape.OutlineThickness = -2;
            highlightShape.FillColor = Color.Transparent;
        }
                
        private void CreateVertexArray()
        {
            tileVisualisation = new VertexArray(PrimitiveType.Quads);
            downslopeArrows = new VertexArray(PrimitiveType.Lines);
            windArrows = new VertexArray(PrimitiveType.Lines);

            foreach (MapTile tile in Tiles)
            {
                Vertex vertex = new Vertex();
                vertex.Position = new Vector2f(tile.GlobalIndex.X, tile.GlobalIndex.Y) * TileSize;
                vertex.Color = tile.DisplayColour;
                tileVisualisation.Append(vertex);

                vertex = new Vertex();
                vertex.Position = new Vector2f(tile.GlobalIndex.X + 1, tile.GlobalIndex.Y) * TileSize;
                vertex.Color = tile.DisplayColour;
                tileVisualisation.Append(vertex);

                vertex = new Vertex();
                vertex.Position = new Vector2f(tile.GlobalIndex.X + 1, tile.GlobalIndex.Y + 1) * TileSize;
                vertex.Color = tile.DisplayColour;
                tileVisualisation.Append(vertex);

                vertex = new Vertex();
                vertex.Position = new Vector2f(tile.GlobalIndex.X, tile.GlobalIndex.Y + 1) * TileSize;
                vertex.Color = tile.DisplayColour;
                tileVisualisation.Append(vertex);

                Vector2f tileCenter = new Vector2f(tile.GlobalIndex.X + 0.5f, tile.GlobalIndex.Y + 0.5f) * TileSize;
                
                //draw the downslope arrows
                if(drawDownslopes)
                {
                    Vector2f normal = MathHelper.UnitNormal((Vector2f)tile.DownslopeDir);
                    vertex = new Vertex();
                    vertex.Position = tileCenter;
                    vertex.Color = Color.White;
                    downslopeArrows.Append(vertex);

                    vertex = new Vertex();
                    vertex.Position = tileCenter + (Vector2f)tile.DownslopeDir * TileSize / 2;
                    vertex.Color = Color.White;
                    downslopeArrows.Append(vertex);

                    vertex = new Vertex();
                    vertex.Position = tileCenter + (Vector2f)tile.DownslopeDir * TileSize / 2;
                    vertex.Color = Color.White;
                    downslopeArrows.Append(vertex);

                    vertex = new Vertex();
                    vertex.Position = tileCenter + ((Vector2f)tile.DownslopeDir + normal) / 4 * TileSize;
                    vertex.Color = Color.White;
                    downslopeArrows.Append(vertex);
                }
                
                //draw wind arrows
                if(drawWind)
                {
                    Vector2f windDir = MathHelper.RadiansToUnitVector(tile.WindDirection);
                    Vector2f normal = MathHelper.UnitNormal(windDir);
                    vertex = new Vertex();
                    vertex.Position = tileCenter;
                    vertex.Color = Color.White;
                    windArrows.Append(vertex);

                    vertex = new Vertex();
                    vertex.Position = tileCenter + windDir * TileSize / 2;
                    vertex.Color = Color.White;
                    windArrows.Append(vertex);

                    vertex = new Vertex();
                    vertex.Position = tileCenter + windDir * TileSize / 2;
                    vertex.Color = Color.White;
                    windArrows.Append(vertex);

                    vertex = new Vertex();
                    vertex.Position = tileCenter + (windDir + normal) / 4 * TileSize;
                    vertex.Color = Color.White;
                    windArrows.Append(vertex);                
                }
            }
        }
       
        public void Draw(RenderWindow window)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            try {
                window.Draw(tileVisualisation);
            }
            catch (AccessViolationException e)
            {
                Console.WriteLine("AccessViolationException: {0}", e.Message);
            }           

            if(drawDownslopes) window.Draw(downslopeArrows);
            if (drawRandomWalks) randomWalks.ForEach(va => window.Draw(va));
            if (drawWind) window.Draw(windArrows);
           
            if (drawHighlight)
            {
                window.Draw(highlightShape);
            }

            watch.Stop();
            //Console.WriteLine("Draw: {0}:", watch.ElapsedMilliseconds);
        }

        public void Update(float dt, Vector2f viewCenter)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            /*foreach (Vector2i tileIndex in ActiveTileIndices)
            {
                Tiles[tileIndex].Update(dt);
            }
             * */
            watch.Stop();
            //Console.WriteLine("Update: {0}:", watch.ElapsedMilliseconds);
        }

        /*
         * Check which tile the mouse is currently hovering over and highlight it
         */               
        public void UpdateMouseHighlight(Vector2f pos)
        {
            //find highlighted tile's index from coordinates and check for out of bounds
            int xIndex = (int) Math.Floor(pos.X / TileSize);

            int yIndex = (int) Math.Floor(pos.Y / TileSize);

            Vector2i highlightedTileIndex = new Vector2i(xIndex, yIndex);            

            if (drawHighlight)
            {
                highlightShape.Position = new Vector2f(xIndex * TileSize, yIndex * TileSize);                
                
                MapTile t = GetTileByIndex(xIndex, yIndex);                                 
            }                      
        }
       
        private MapTile GetTileByIndex(int x, int y)
        {
            return GetTileByIndex(new Vector2i(x, y));
        }

        private MapTile GetTileByIndex(Vector2i index)
        {
            int i = index.X * MapSize.Y + index.Y;            

            if (index.X >= 0 && index.X < MapSize.X
                && index.Y >= 0 && index.Y < MapSize.Y
                && i >= 0 && i < Tiles.Count())
            {
                return Tiles[i];
            }                       
            else
            {                
                //Console.WriteLine("Tile: {0}, index: {1}", Tiles[i].GlobalIndex, index);
                return null;
            }                                     
        }

        public MapTile GetTileByWorldPos(Vector2f worldPos)
        {            
            int x = (int)Math.Floor(worldPos.X / TileSize);
            int y = (int)Math.Floor(worldPos.Y / TileSize);

            return GetTileByIndex(x, y);
        }

        public void OnMouseMoved(object sender, MouseMoveEventArgs e)
        {
            Vector2i mousePos = new Vector2i(e.X, e.Y);
            RenderWindow window = (RenderWindow)sender;
            Vector2f worldPos = window.MapPixelToCoords(mousePos);

            //highlight the tile the mouse is hovering over
            UpdateMouseHighlight(worldPos);
        }

        public void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;
            Vector2f index = window.MapPixelToCoords(new Vector2i(e.X, e.Y));

            int x = (int)Math.Floor((double)index.X / TileSize);
            int y = (int)Math.Floor((double)index.Y / TileSize);
            MapTile t = GetTileByIndex(x,y);            
            if(t != null && e.Button == Mouse.Button.Right)
            {              
                Console.WriteLine("Clicked tileIndex: ({0}, {1}), z = {2}, water = {3}", x, y, Math.Round(t.Elevation, 2), t.Water);
                Console.WriteLine("WorldPos: ({0},{1}), LandmassID: {2}", index.X, index.Y, t.LandmassId);
                Console.WriteLine("Temperature: {0}, rainfall: {1}", Math.Round(t.Temperature, 2), Math.Round(t.Rainfall, 2));
                Console.WriteLine("ElevationZone: {0}", t.ElevationZone.Name);
                Console.WriteLine("TemperatureZone: {0}", t.TemperatureZone.Name);
                Console.WriteLine("HumidityZone: {0}", t.HumidityZone.Name);
                Console.WriteLine("Biome: {0}", t.Biome.Name); 

                //Console.WriteLine("Wind dir: {0}, str: {1}, distToCoast: {2}", Math.Round(t.WindDirection, 3), Math.Round(t.WindStrength, 3), t.DistanceToCoast);
                //Console.WriteLine("DownslopeDir: ({0},{1}), Downhill to sea:{2}", t.DownslopeDir.X, t.DownslopeDir.Y, t.DownhillToSea);
                //Console.WriteLine("River volume: {0}. River source: {1}", t.RiverVolume, t.RiverSource);
                Console.WriteLine();
            }           
        }

        public void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if(e.Code == Keyboard.Key.D)
            {
                drawDownslopes = !drawDownslopes;
                drawWind = false;
                CreateVertexArray();
                Console.WriteLine("Displaying downslopes");
            }
            else if (e.Code == Keyboard.Key.F)
            {
                drawRandomWalks = !drawRandomWalks;
                CreateVertexArray();
                Console.WriteLine("Displaying random walks");
            }
            else if (e.Code == Keyboard.Key.B)
            {
                TileColourHelper colorHelper = new TileColourHelper();
                foreach (MapTile t in Tiles)
                {
                    colorHelper.SetBiomeColor(t);                    
                }
                CreateVertexArray();
                Console.WriteLine("Displaying biome map");
            }
            else if (e.Code == Keyboard.Key.E)
            {
                TileColourHelper colorHelper = new TileColourHelper();
                foreach (MapTile t in Tiles)
                {
                    colorHelper.SetElevationColor(t);
                }  
                CreateVertexArray();
                Console.WriteLine("Displaying elevation map");
            }
            else if (e.Code == Keyboard.Key.W)
            {
                TileColourHelper colorHelper = new TileColourHelper();
                foreach (MapTile t in Tiles)
                {
                    colorHelper.SetWindColor(t);                    
                }
                CreateVertexArray();
                Console.WriteLine("Displaying wind map");
            }
            else if (e.Code == Keyboard.Key.Q)
            {
                drawWind = !drawWind;
                drawDownslopes = false;
                CreateVertexArray();
                Console.WriteLine("Displaying wind direction");
            }
            else if (e.Code == Keyboard.Key.L)
            {
                TileColourHelper colorHelper = new TileColourHelper();
                foreach (MapTile t in Tiles)
                {
                    colorHelper.SetLandmassColor(t);
                }
                CreateVertexArray();
                Console.WriteLine("Displaying landmass map");
            }
            else if (e.Code == Keyboard.Key.T)
            {
                TileColourHelper colorHelper = new TileColourHelper();
                foreach (MapTile t in Tiles)
                {
                    colorHelper.SetTemperatureColor(t);                    
                }
                CreateVertexArray();
                Console.WriteLine("Displaying temperature map");
            }
            else if(e.Code == Keyboard.Key.S)
            {
                if(e.Control)
                {
                    Save();                    
                }
                else
                {
                    TileColourHelper colorHelper = new TileColourHelper();
                    foreach (MapTile t in Tiles)
                    {
                        colorHelper.SetRainShadowColor(t);
                    }
                    CreateVertexArray();
                    Console.WriteLine("Displaying rain shadow map");
                }                
            }
            else if (e.Code == Keyboard.Key.R)
            {
                TileColourHelper colorHelper = new TileColourHelper();
                foreach (MapTile t in Tiles)
                {
                    colorHelper.SetRainfallColor(t);
                }
                CreateVertexArray();
                Console.WriteLine("Displaying rainfall map");
            }
            else if (e.Code == Keyboard.Key.Num1)
            {
                TileColourHelper colorHelper = new TileColourHelper();
                foreach (MapTile t in Tiles)
                {
                    colorHelper.SetElevationZoneColor(t);
                }
                CreateVertexArray();
                Console.WriteLine("Displaying elevation zone map");
            }
            else if (e.Code == Keyboard.Key.Num2)
            {
                TileColourHelper colorHelper = new TileColourHelper();
                foreach (MapTile t in Tiles)
                {
                    colorHelper.SetTemperatureZoneColor(t);
                }
                CreateVertexArray();
                Console.WriteLine("Displaying temperature zone map");
            }
            else if (e.Code == Keyboard.Key.Num3)
            {
                TileColourHelper colorHelper = new TileColourHelper();
                foreach (MapTile t in Tiles)
                {
                    colorHelper.SetHumidityZoneColor(t);
                }
                CreateVertexArray();
                Console.WriteLine("Displaying humidity zone map");
            }
        }

        public void OnMapMoved(object sender, MapMoveEventArgs e)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew(); 
            int centerTileX = (int)Math.Floor(e.View.Center.X / TileSize);
            int centerTileY = (int)Math.Floor(e.View.Center.Y / TileSize);
            ActivateTilesAround(centerTileX, centerTileY, (int)Math.Ceiling(e.View.Size.Y / TileSize));
            watch.Stop();            
        }
        
        private void ActivateTilesAround(int centerTileX, int centerTileY, int tilesY)
        {
            activeTileIndices.Clear();         
            
            double defaultScale = Game.WindowSize.X / TileSize;
            int tilesX = (int)Math.Ceiling((double)Game.WindowSize.X / Game.WindowSize.Y * tilesY);            

            //make sure the tile count is an even number, then half it           
            if(tilesX % 2 != 0)
            {
                tilesX++;             
            }
            if(tilesY % 2 != 0)
            {
                tilesY++;
            }
            
            //loop from -half of the count to +half and activate the surrounding tiles
            for (int y = -tilesY/2 + centerTileY; y < tilesY/2 + centerTileY + 1; y++)
            {
                for (int x = -tilesX/2 + centerTileX; x < tilesX/2 + centerTileX + 1; x++)
                {
                    Vector2i tileIndex = new Vector2i(x, y);                    
                    if (x > 0 && x < MapSize.X
                        && y > 0 && y < MapSize.Y)
                    {
                        activeTileIndices.Add(tileIndex);                   
                    }                    
                }
            }
        }      
 
        private List<MapTile> GetTilesSurrounding(MapTile t, int radius)
        {
            int xs = t.GlobalIndex.X;
            int ys = t.GlobalIndex.Y; // Start coordinates
            List<MapTile> surrounding = new List<MapTile>();
            

            for (int d = 1; d < radius; d++)
            {
                for (int i = 0; i < d + 1; i++)
                {
                    int x = xs - d + i;
                    int y = ys - i;

                    if (GetTileByIndex(x, y) != null) surrounding.Add(GetTileByIndex(x, y));                        

                    x = xs + d - i;
                    y = ys + i;

                    if (GetTileByIndex(x, y) != null) surrounding.Add(GetTileByIndex(x, y));  
                }


                for (int i = 1; i < d; i++)
                {
                    int x = xs - i;
                    int y = ys + d - i;

                    if (GetTileByIndex(x, y) != null) surrounding.Add(GetTileByIndex(x, y));  

                    x = xs + d - i;
                    y = ys - i;

                    if (GetTileByIndex(x, y) != null) surrounding.Add(GetTileByIndex(x, y));  
                }
            }
            return surrounding;
        }

        public void Save()
        {
            MapIO ms = new MapIO();
            ms.Save(GetSaveData(), MapName);
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("{0} map successfully saved", MapName);
            Console.ResetColor();
        }

        private WorldMapSaveData GetSaveData()
        {
            return new WorldMapSaveData
            {
                MapName = MapName,
                BaseSeed = baseSeed,
                TileSize = TileSize,
                MaxElevation = MaxElevation,
                MinElevation = MinElevation,
                SeaLevel = SeaLevel,
                MountainThreshold = MountainThreshold,
                WindThresholdDist = WindThresholdDist,
                MapSize = MapSize,
                RandomWalkInitialiser = randomWalkInitialiser
            };    
        }
    }
}