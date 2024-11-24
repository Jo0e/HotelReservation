namespace Models.Models
{
    public class ReportDetails
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Reservation Reservation { get; set; }

    }
}
