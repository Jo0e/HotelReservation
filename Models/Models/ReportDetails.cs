using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models.Models
{
    public class ReportDetails
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        [ValidateNever]
        public ApplicationUser User { get; set; }
        [ValidateNever]
        public Reservation Reservation { get; set; }

        // add list of room id

    }
}
