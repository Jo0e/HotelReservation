using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class RoomType
    {
        [Key]
        public int Id { get; set; }
        public Type Type { get; set; }
        public int? AvailableRooms => Rooms.Count;
        public int MaxPersons { get; set; }
        public int PricePN { get; set; }
        public int HotelId { get; set; }        
        public Hotel Hotel { get; set; }       
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
