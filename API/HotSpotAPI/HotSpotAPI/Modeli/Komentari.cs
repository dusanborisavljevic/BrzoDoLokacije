﻿namespace HotSpotAPI.Modeli
{
    public class Komentari
    {
        public int ID { get; set; }
        public int PostID { get; set; }
        public int UserID { get; set; }
        public string Text { get; set; } = String.Empty;
        public DateTime DateTime { get; set; }
    }
}
