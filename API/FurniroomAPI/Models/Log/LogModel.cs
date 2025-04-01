namespace FurniroomAPI.Models.Log
{
    public class LogModel
    {
        public DateTime Date { get; set; } 
        public string HttpMethod { get; set; } 
        public string Endpoint { get; set; } 
        public string QueryParams { get; set; } 
        public string Status { get; set; } 
        public string RequestId { get; set; } 
    }
}
