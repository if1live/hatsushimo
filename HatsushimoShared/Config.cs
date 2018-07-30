namespace Hatsushimo
{
    public class Config
    {
        public const int Version = 1234;
        public const int ServerPort = 3000;

        public const float RoomWidth = 30;
        public const float RoomHeight = 30;

        public const int FoodCount = 10;
        public const int LeaderboardSize = 5;

        public const int MoveSyncIntervalMillis = 100;
        public const int LeaderboardSyncIntervalMillis = 500;
        public const int GameLoopIntervalMillis = 1000 / 60;

        public const float CoolTimeCommandPrimary = 0.3f;
        public const float CoolTimeCommandSecondary = 2;

        public const float HeartbeatInterval = 20.0f;
    }
}
