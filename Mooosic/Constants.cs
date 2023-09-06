namespace Mooosic;

public static class Constants
{
    public const string APP_NAME = "Mooosic";
    public const string CONFIG_FILE = "./config.json";
    public const string NONE = "NONE";

    public static class CustomIds
    {
        public const string PREVIOUS_SONG = "previous_song";
        public const string NEXT_SONG = "next_song";
        public const string PAUSE_RESUME = "pause_resume";
        public const string STOP = "stop";
        public const string SHUFFLE = "shuffle";
        public const string LOOP_ONCE = "loop 1";
        public const string LOOP_FOREVER = "loop -1";
        public const string LOOP_NONE = "loop 0";
        public const string LOOP_CUSTOM_ID_FILTER = "loop *";
    }
    
    public static class Emojis
    {
        public const string PLAY = "▶️";
        public const string PAUSE = "⏸️";
        public const string STOP = "⏹️";
        public const string REPEAT = "🔁";
        public const string REPEAT_ONCE = "🔂";
        public const string SHUFFLE = "🔀";
        public const string PREVIOUS = "⏮️";
        public const string NEXT = "⏭️";
    }
}