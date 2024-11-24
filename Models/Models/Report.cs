namespace Models.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Revenue { get; set; }
        public double RatingAverage { get; set; }
        public int TotalReservation { get; set; }

        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        public ICollection<ReportDetails> ReportDetails { get; set; } = new List<ReportDetails>();
    }
}
