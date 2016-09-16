﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace ReSource
{
    class WorldMap
    {   
        private Dictionary<Vector2i, MapTile> Tiles = new Dictionary<Vector2i, MapTile>();
        private List<Vector2i> ActiveTileIndices = new List<Vector2i>();

        private RectangleShape HighlightShape;
        private VertexArray vertices;
        private VertexArray downslopeArrows;
        private bool drawDownslopes = false;
        private bool drawRandomWalks = false;

        public static Font Font = new Font(@"..\..\..\resources\fonts\arial.ttf");

        public int TileSize = 32;        

        public float MaxElevation = 1.0f;
        public float MinElevation = 0.0f;
        public float SeaLevel = 0.2f;
        double mountainThreshold = 0.45d;       //cutoff height for a tile to be considered a mountain

        public Vector2i MapSize { get; private set; }        
        
        public static readonly bool SpriteDraw = false;

        public WorldMap(Vector2i mapSize)
        {
            this.MapSize = mapSize;
            
            ExecuteTimedFunction(CreateTiles);
            ExecuteTimedFunction(AssignTileNeighbours);
            ExecuteTimedFunction(() => GenerateRandomWalks(true, true, 8,8,5), "Generate Random Walks");
            ExecuteTimedFunction(CalculatePerlinCoefficients);
            ExecuteTimedFunction(CalculateGaussianCoefficients);
            ExecuteTimedFunction(CalculateVoronoiCoefficients);
            ExecuteTimedFunction(SetTileElevations);
            ExecuteTimedFunction(RescaleElevation);
            ExecuteTimedFunction(AssignOcean);
            ExecuteTimedFunction(AssignCoast);
            ExecuteTimedFunction(AssignDownslopes);
            ExecuteTimedFunction(CreateRivers);            
            ExecuteTimedFunction(AssignMoisture);
            ExecuteTimedFunction(AssignBiomes);
            ExecuteTimedFunction(InitialiseDisplay);
            ExecuteTimedFunction(CreateVertexArray);            
            
            Console.WriteLine("Build finished in {0} s", TotalBuildTime/1000d);
        }

        private bool IsMapBorder(MapTile t)
        {
            return (t.GlobalIndex.X == 0)
                || (t.GlobalIndex.Y == 0)
                || (t.GlobalIndex.X == MapSize.X - 1)
                || (t.GlobalIndex.Y == MapSize.Y - 1);
        }

        private long TotalBuildTime = 0;              
        private void ExecuteTimedFunction(Action action, String msg = "")
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
                    Tiles.Add(tileIndex, tile);
                }
            }
        }

        private void AssignTileNeighbours()
        {
            Parallel.ForEach(Tiles.Values, t =>
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

        List<VertexArray> randomWalks = new List<VertexArray>();
        private void GenerateRandomWalks(bool sides, bool topBot, int edgeWalks, int midWalks, int steps)
        {
            //generate a list of random walks that will be used to generate voronoi diagrams

            //add a line to the left and right sides if requested
            if (sides)
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
            if (topBot)
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

            //walks start in middle 80% of map
            for (int i = 0; i < midWalks; i++)
            {
                int x = MathHelper.rnd.Next((int)Math.Round(0.8d * MapSize.X)) + (int)Math.Round(0.1d * MapSize.X);
                int y = MathHelper.rnd.Next((int)Math.Round(0.8d * MapSize.Y)) + (int)Math.Round(0.1d * MapSize.Y);
                startPositions.Add(new Vector2i(x, y));
            }

            //walks start from edge
            for (int i = 0; i < edgeWalks; i++)
            {
                int rnd = MathHelper.rnd.Next(4);
                if (rnd == 0)
                {
                    //start on the top row
                    startPositions.Add(new Vector2i(MathHelper.rnd.Next(MapSize.X), 0));
                }
                else if (rnd == 1)
                {
                    //start on right column
                    startPositions.Add(new Vector2i(MapSize.X, MathHelper.rnd.Next(MapSize.Y)));
                }
                else if (rnd == 2)
                {
                    //start on bottom row
                    startPositions.Add(new Vector2i(MathHelper.rnd.Next(MapSize.X), MapSize.Y));
                }
                else if (rnd == 3)
                {
                    //start on left column
                    startPositions.Add(new Vector2i(0, MathHelper.rnd.Next(MapSize.Y)));
                }
            }

            foreach (Vector2i startPos in startPositions)
            {
                IntRect bounds = new IntRect(0, 0, MapSize.X * TileSize, MapSize.Y * TileSize);

                //randomWalks.Add(RandomWalker.GridWalk(startPos * TileSize, bounds, TileSize, steps));
                Vector2f start = new Vector2f(startPos.X, startPos.Y) * TileSize;
                randomWalks.Add(RandomWalker.RandomWalk(start, bounds, MapSize.Y, 10));
            }
        }        

        private void CalculatePerlinCoefficients()
        {
            foreach(MapTile t in Tiles.Values)
            {
                SetTilePerlin(t);
            }
            NormalisePerlinCoefficients();
        }

        private void SetTilePerlin(MapTile tile)
        {
            //get Perlin noise to get base elevation value            
            int featureScale = MapSize.Y / 8;
            tile.Perlin = PerlinGenerator.OctavePerlin(
                    (double)tile.GlobalIndex.X / featureScale,
                    (double)tile.GlobalIndex.Y / featureScale,
                    4, 0.5);
        }

        private void NormalisePerlinCoefficients()
        {
            //get highest and lowest elevations and normalise to world min and max
            double min = Double.MaxValue;
            double max = Double.MinValue;
            foreach (MapTile tile in Tiles.Values)
            {
                if (tile.Perlin < min)
                {
                    min = tile.Perlin;
                }

                if (tile.Perlin > max)
                {
                    max = tile.Perlin;
                }
            }

            foreach (MapTile t in Tiles.Values)
            {
                t.Perlin = MathHelper.Scale(min, max, 0, 1, t.Perlin);
            }
        }

        private void CalculateGaussianCoefficients()
        {
            foreach (MapTile t in Tiles.Values)
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
            tile.Gaussian = elevationDiff * Math.Exp(-Math.Pow(scaleX, 2d) / gaussianWidth) - (((double)elevationDiff - MaxElevation) / (double)elevationDiff);
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

            Parallel.ForEach(Tiles.Values, (t) =>
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
            tile.Voronoi = min;
        }

        private void NormaliseVoronoiCoefficients()
        {
            //get highest and lowest elevations and normalise to world min and max
            double min = Double.MaxValue;
            double max = Double.MinValue;
            foreach (MapTile tile in Tiles.Values)
            {
                if (tile.Voronoi < min)
                {
                    min = tile.Voronoi;
                }

                if (tile.Voronoi > max)
                {
                    max = tile.Voronoi;
                }
            }

            foreach (MapTile t in Tiles.Values)
            {
                t.Voronoi = MathHelper.Scale(min, max, 0.1, 1.5, t.Voronoi);
            }
        }

        private void SetTileElevations()
        {
            //multiply the weighting factors to get the final elevations
            foreach (MapTile tile in Tiles.Values)
            {
                //tile.Elevation = tile.Perlin * tile.Gaussian * tile.Voronoi;  
                tile.Elevation = tile.Perlin * tile.Voronoi;
            }
        } 

        private void RescaleElevation()
        {
            //get highest and lowest elevations and normalise to world min and max
            double min = Double.MaxValue;
            double max = Double.MinValue;
            foreach (MapTile tile in Tiles.Values)
            {
                if (tile.Elevation < min)
                {
                    min = tile.Elevation;
                }

                if (tile.Elevation > max)
                {
                    max = tile.Elevation;
                }
            }

            foreach (MapTile t in Tiles.Values)
            {
                t.Elevation = MathHelper.Scale(min, max, MinElevation, MaxElevation, t.Elevation);
            }
        }         

        private void AssignOcean()
        {
            Queue<MapTile> fillQ = new Queue<MapTile>();

            //go around the edge of the map and make edge chunks assign water at the edge of the map
            for(int x = 0; x < MapSize.X; x++)
            {
                for(int y = 0; y < MapSize.Y; y++)
                {
                    MapTile tile = GetTileByIndex(x, y);
                    if (IsMapBorder(tile) && tile.Elevation < SeaLevel)
                    {
                        tile.Water = WaterType.Ocean;
                        fillQ.Enqueue(tile);
                    }
                }
            }

            //assign any tiles with elevation < 0 touching ocean as oceans
            while(fillQ.Count > 0)
            {
                MapTile tile = fillQ.Dequeue();
                
                foreach(MapTile neighbour in tile.OrthogonalNeighbours)
                {
                    if(neighbour != null)
                    {
                        if (neighbour.Elevation < SeaLevel && neighbour.Water == WaterType.Unassigned)
                        {
                            neighbour.Water = WaterType.Ocean;
                            fillQ.Enqueue(neighbour);
                        }
                    }                    
                }                
            }

            //loop over the map again and assign the remaining tiles as either land or fresh water
            for (int x = 0; x < MapSize.X; x++)
            {
                for (int y = 0; y < MapSize.Y; y++)
                {
                    MapTile tile = GetTileByIndex(x, y);
                    if(tile.Water == WaterType.Unassigned)
                    {
                        if(tile.Elevation > SeaLevel)
                        {
                            tile.Water = WaterType.Land;
                        }
                        else
                        {
                            tile.Water = WaterType.Lake;
                        }
                        
                    }                    
                }
            }           
        }     

        private void AssignCoast()
        {
            //if tile has at least one ocean neighbour and one land neighbour it must be coast
            for (int x = 0; x < MapSize.X; x++)
            {
                for (int y = 0; y < MapSize.Y; y++)
                {
                    MapTile tile = GetTileByIndex(x, y);
                    int oceanNeighbour = 0;
                    int landNeighbour = 0;

                    foreach(MapTile n in tile.OrthogonalNeighbours)
                    {
                        if(n.Water == WaterType.Land)
                        {
                            landNeighbour++;
                        }

                        if(n.Water ==WaterType.Ocean)
                        {
                            oceanNeighbour++;
                        }
                    }

                    tile.Coast = (tile.Water == WaterType.Land) && (oceanNeighbour > 0) && (landNeighbour > 0);
                }
            }
        }    
     
        private void AssignDownslopes()
        {
            Parallel.ForEach(Tiles.Values, tile => AssignTileDownslope(tile));
            Parallel.ForEach(Tiles.Values, tile => AssignTileDownhillToSea(tile));
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

                if (nextTile.DownslopeDir == new Vector2i(0, 0))
                {
                    tile.DownhillToSea = false;
                    return;
                }

                if (nextTile.Water == WaterType.Ocean)
                {
                    tile.DownhillToSea = true;                    
                    return;
                }
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
            IEnumerable<MapTile> mountains = Tiles.Values
                .Where(t => t.DownhillToSea                 //which have a downhill path to the sea
                    && t.Water == WaterType.Land            //which are land
                    && t.Elevation >= mountainThreshold);   //and which are high enough to be considered mountains
                      
            return mountains
                .OrderBy(t => MathHelper.rnd.NextDouble())  //shuffle the list of mountains
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
                        if (MathHelper.rnd.NextDouble() > 0.95d)
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
   
        private void AssignMoisture()
        {
            //grab a list of only the water & tiles on the map
            //to speed up the moisture assignment OCEAN tiles are not assigned moisture
            IEnumerable<MapTile> waterTiles = Tiles.Values.Where(t => t.RiverVolume > 0 || t.Water == WaterType.Lake);
            IEnumerable<MapTile> landTiles = Tiles.Values.Where(t => t.Water != WaterType.Ocean);
            
            int moistureCount = 0;

            System.Timers.Timer timer = new System.Timers.Timer();
            decimal percent = 0;
            timer.Elapsed += (sender, e) =>
            {
                percent = Math.Round((decimal)moistureCount / landTiles.Count() * 100m, 2);
                Console.SetCursorPosition(System.Reflection.MethodBase.GetCurrentMethod().Name.Length, Console.CursorTop);
                Console.Write("{0}%", percent);
            };
            timer.Interval = 1000;
            timer.Enabled = true;
            
            Parallel.ForEach(landTiles, (t) =>
            {
                AssignTileMoisture(t, waterTiles);
                moistureCount++;
            });
            Console.SetCursorPosition(System.Reflection.MethodBase.GetCurrentMethod().Name.Length, Console.CursorTop);
            timer.Dispose();
        }

        double moistureConst = 0.95d;
        private void AssignTileMoisture(MapTile tile, IEnumerable<MapTile> waterTiles)
        {
            double minFreshWaterDist = Double.MaxValue;
            foreach(MapTile waterTile in waterTiles)
            {
                Vector2i dir = waterTile.GlobalIndex - tile.GlobalIndex;
                int freshWaterDist = Math.Abs(dir.X) + Math.Abs(dir.Y);

                if(freshWaterDist < minFreshWaterDist)
                {
                    minFreshWaterDist = freshWaterDist;
                }
            }
            tile.Moisture = Math.Pow(moistureConst, minFreshWaterDist);
            if (tile.Moisture > 1) Console.WriteLine("moisture > 1");
        }

        private void AssignBiomes()
        {
            Parallel.ForEach(Tiles.Values, t => AssignTileBiome(t));
            Console.WriteLine();
            foreach(Biome b in Biome.biomeList)
            {
                Console.WriteLine("{0}, count: {1}", b.Name, b.Count);
            }
            
        }

        private void AssignTileBiome(MapTile t)
        {
            if(t.Water == WaterType.Ocean)
            {
                if(t.Elevation > SeaLevel * 0.75d)
                {
                    t.Biome = Biome.Shallows;                    
                }
                else if (t.Elevation < SeaLevel * 0.25d)
                {
                    t.Biome = Biome.Depths;                    
                }
                else
                {
                    t.Biome = Biome.Ocean;                    
                }
            } 
            else if(t.Water == WaterType.Lake)
            {
                if(t.Elevation > mountainThreshold)
                {
                    t.Biome = Biome.AlpineLake;
                }
                else
                {
                    t.Biome = Biome.Lake;
                }               
            }
            else
            {
                int moistureZone = (int)Math.Floor(MathHelper.Scale(0, 1, 0, 5.999, t.Moisture));
                int eleZone = (int)Math.Floor(MathHelper.Scale(SeaLevel, 1, 0, 3.999, t.Elevation));                

                t.Biome = Biome.BiomeTable[moistureZone, eleZone];                
            }

            t.Biome.Count++;
        }

        private void InitialiseDisplay()
        {
            foreach (MapTile t in Tiles.Values)
            {
                t.SetElevationColor();
            }

            //activate tiles in the middle of the view            
            ActivateTilesAround((int)Math.Floor(MapSize.X / 2d), (int)Math.Floor(MapSize.Y / 2d), 1);

            HighlightShape = new RectangleShape(new Vector2f(TileSize, TileSize));
            HighlightShape.OutlineColor = Color.White;
            HighlightShape.OutlineThickness = -2;
            HighlightShape.FillColor = Color.Transparent;
        }

        private void CreateVertexArray()
        {
            vertices = new VertexArray(PrimitiveType.Quads);
            downslopeArrows = new VertexArray(PrimitiveType.Lines);

            foreach (MapTile tile in Tiles.Values)
            {
                Vertex vertex = new Vertex();
                vertex.Position = new Vector2f(tile.GlobalIndex.X, tile.GlobalIndex.Y) * TileSize;
                vertex.Color = tile.DisplayColour;
                vertices.Append(vertex);

                vertex = new Vertex();
                vertex.Position = new Vector2f(tile.GlobalIndex.X + 1, tile.GlobalIndex.Y) * TileSize;
                vertex.Color = tile.DisplayColour;
                vertices.Append(vertex);

                vertex = new Vertex();
                vertex.Position = new Vector2f(tile.GlobalIndex.X + 1, tile.GlobalIndex.Y + 1) * TileSize;
                vertex.Color = tile.DisplayColour;
                vertices.Append(vertex);

                vertex = new Vertex();
                vertex.Position = new Vector2f(tile.GlobalIndex.X, tile.GlobalIndex.Y + 1) * TileSize;
                vertex.Color = tile.DisplayColour;
                vertices.Append(vertex);

                //draw the downslope
                Vector2f tileCenter = new Vector2f(tile.GlobalIndex.X + 0.5f, tile.GlobalIndex.Y + 0.5f) * TileSize;

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
                vertex.Position = tileCenter + MathHelper.UnitNormal(tile.DownslopeDir) * TileSize / 4;
                vertex.Color = Color.White;
                downslopeArrows.Append(vertex);

                vertex = new Vertex();
                vertex.Position = tileCenter + (Vector2f)tile.DownslopeDir * TileSize / 2;
                vertex.Color = Color.White;
                downslopeArrows.Append(vertex);

                vertex = new Vertex();
                vertex.Position = tileCenter - MathHelper.UnitNormal(tile.DownslopeDir) * TileSize / 4;
                vertex.Color = Color.White;
                downslopeArrows.Append(vertex);
            }
        }
       
        public void Draw(RenderWindow window)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            window.Draw(vertices);

            if(drawDownslopes) window.Draw(downslopeArrows);
            if (drawRandomWalks) randomWalks.ForEach(va => window.Draw(va));            
           
            if (drawHighlight)
            {
                window.Draw(HighlightShape);
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
        private Vector2i highlightedTileIndex = new Vector2i(0, 0);
        bool drawHighlight = true;
        public void UpdateMouseHighlight(Vector2f pos)
        {
            //find highlighted tile's index from coordinates and check for out of bounds
            int xIndex = (int) Math.Floor(pos.X / TileSize);

            int yIndex = (int) Math.Floor(pos.Y / TileSize);
            
            highlightedTileIndex = new Vector2i(xIndex, yIndex);            

            if (drawHighlight)
            {
                HighlightShape.Position = new Vector2f(xIndex * TileSize, yIndex * TileSize);                
                
                MapTile t = GetTileByIndex(xIndex, yIndex);                                 
            }                      
        }
       
        private MapTile GetTileByIndex(int x, int y)
        {
            return GetTileByIndex(new Vector2i(x, y));
        }

        private MapTile GetTileByIndex(Vector2i index)
        {
            if(Tiles.ContainsKey(index))
            {
                return Tiles[index];
            }            

            return null;            
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
            RenderWindow window = (RenderWindow) sender;
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
                Console.WriteLine("Clicked tileIndex: ({0}, {1}), z = {2}, water = {3}", x, y, t.Elevation, t.Water);
                Console.WriteLine("WorldPos: ({0},{1})", index.X, index.Y);
                Console.WriteLine("DownslopeDir: ({0},{1}), Downhill to sea:{2}", t.DownslopeDir.X, t.DownslopeDir.Y, t.DownhillToSea);
                Console.WriteLine("River volume: {0}. River source: {1}", t.RiverVolume, t.RiverSource);
                Console.WriteLine();
            }           
        }

        public void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if(e.Code == Keyboard.Key.D)
            {
                drawDownslopes = !drawDownslopes;
            }
            if(e.Code == Keyboard.Key.F)
            {
                drawRandomWalks = !drawRandomWalks;
            }
            if(e.Code == Keyboard.Key.M)
            {
                foreach (MapTile t in Tiles.Values)
                {
                    t.SetMoistureColor();
                }
                CreateVertexArray();
            }
            if(e.Code == Keyboard.Key.B)
            {
                foreach (MapTile t in Tiles.Values)
                {
                    t.SetBiomeColor();
                }
                CreateVertexArray();
            }
            if (e.Code == Keyboard.Key.E)
            {
                foreach (MapTile t in Tiles.Values)
                {
                    t.SetElevationColor();
                }  
                CreateVertexArray();
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
            ActiveTileIndices.Clear();         
            
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
                    if (Tiles.ContainsKey(tileIndex))
                    {
                        ActiveTileIndices.Add(tileIndex);                   
                    }                    
                }
            }
        }        
    }
}