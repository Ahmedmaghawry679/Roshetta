namespace Roshetta.API.Models
{
    public class Specialization
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual List<Doctor> Doctors { get; set; }
    }
}
