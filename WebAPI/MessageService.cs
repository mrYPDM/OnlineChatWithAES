using System;

namespace WebAPI
{
    public static partial class MessageService
    {
        static public string Log => "log";
        static public string File => "file";
        static public string Disconnect => "disconnect";
        static public string NoPlaces => "no_places_on_server";
        static public string MaxFileSize(int newSize) => $"max_file_size\x1{newSize}";
    }
}
