namespace Models.Models
{
    public class RoomType
    {
        public int Id { get; set; }
        public Type Type { get; set; }
        public int AvailableRooms => Rooms.Count;
        public int MaxPersons { get; set; }
        public int PricePN { get; set; }
        public ICollection<Room> Rooms { get; set; }

    }

    public enum Type
    {
        Single,
        Double,
        Triple,
        Quadruple
    }
}
