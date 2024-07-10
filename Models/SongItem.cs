namespace MySpace.Crawler.Web.Models
{
    public class SongItem
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Date { get; set; }
        public string Album { get; set; }
        public string Duration { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            return $"{Artist} - {Title} | Duration: {Duration}";
        }
    }
}