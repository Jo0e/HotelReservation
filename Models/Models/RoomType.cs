using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models.Models
{
    public class RoomType
    {
        public int Id { get; set; }
        public Type Type { get; set; }
        public int? AvailableRooms => Rooms.Count;
        public int MaxPersons { get; set; }
        public int PricePN { get; set; }
        public int? MealPrice { get; set; }

        public int HotelId { get; set; }
        [ValidateNever]
        public Hotel Hotel { get; set; }
        [ValidateNever]
        public ICollection<Room> Rooms { get; set; } = new List<Room>();

    }
    public enum Type
    {
        Single,
        Double,
        Triple,
        Quadruple
    }
}
