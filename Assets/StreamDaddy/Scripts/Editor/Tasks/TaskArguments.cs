using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public class TaskArguments
    {
        /// <summary>
        /// Name of the current world being built.
        /// Type: string
        /// </summary>
        public static string WORLD_NAME = "worldname";

        /// <summary>
        /// List of chunks in the world.
        /// Type: List<EditorChunk>
        /// </summary>
        public static string CHUNKS = "chunks";

        /// <summary>
        /// Size of each chunk.
        /// Type: Vector3Int
        /// </summary>
        public static string CHUNK_SIZE = "chunksize";

        /// <summary>
        /// The class that holds all of the chunks
        /// Type: EditorChunkManager
        /// </summary>
        public static string CHUNK_MANAGER = "chunkmanager";

        /// <summary>
        /// A list of all asset bundles associated with the current world.
        /// Type: List<string>
        /// </summary>
        public static string ASSET_BUNDLES = "assetbundles";

        /// <summary>
        /// The name of the asset bundle containing all the chunk layouts.
        /// Type: strings
        /// </summary>
        public static string CHUNK_LAYOUT_BUNDLE = "chunklayoutbundle";

        /// <summary>
        /// A list of all the asset names of the chunk layouts.
        /// Type: List<string>
        /// </summary>
        public static string CHUNK_LAYOUT_NAMES = "chunklayoutnames";

        /// <summary>
        /// The terrain of the world, before it has been split into separate pieces.
        /// </summary>
        public static string TERRAIN_UNSPLIT = "terrainunsplit";

        /// <summary>
        /// List of terrains in the world.
        /// Type: List<Terrain>
        /// </summary>
        public static string TERRAIN_DATA = "terraindata";

        /// <summary>
        /// Material used for terrain that has been made into mesh
        /// Type: Material
        /// </summary>
        public static string TERRAIN_MESH_MATERIAL = "terrainmaterial";
    }
}


