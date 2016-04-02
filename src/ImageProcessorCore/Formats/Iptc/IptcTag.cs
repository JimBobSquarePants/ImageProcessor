namespace ImageProcessorCore.Formats
{
    public class IptcTag 
    {
        public string Name { get; private set; }

        public int Id { get; private set; }

        internal IptcTag(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
           return  $"[ Name={this.Name}, Id={this.Id} ]";
        }
    }
}
