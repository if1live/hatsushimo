using System.Collections.Generic;

namespace HatsushimoServer
{
    public class InstanceWorldManager
    {
        public static readonly InstanceWorldManager Instance = new InstanceWorldManager();

        public const string DefaultID = "default";
        readonly Dictionary<string, InstanceWorld> worlds = new Dictionary<string, InstanceWorld>();

        public InstanceWorldManager()
        {
            // create default room
            var _ = Get(DefaultID);
        }

        internal InstanceWorld Create(string id)
        {
            var world = new InstanceWorld(id);
            world.Running = true;
            return world;
        }

        internal InstanceWorld Get(string id)
        {
            InstanceWorld world = null;
            if (!worlds.TryGetValue(id, out world))
            {
                world = Create(id);
                worlds[id] = world;
            }
            return world;
        }

        internal void Remove(string id)
        {
            InstanceWorld world = null;
            if (worlds.TryGetValue(id, out world))
            {
                world.Running = false;
                worlds.Remove(id);
            }
        }
    }
}
