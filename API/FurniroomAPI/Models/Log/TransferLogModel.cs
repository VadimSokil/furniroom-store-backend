namespace FurniroomAPI.Models.Log
{
    public class TransferLogModel
    {
        public string HttpMethod { get; set; }
        public string Endpoint { get; set; }
        public string QueryParams { get; set; }
        public string RequestId { get; set; }
    }
}
