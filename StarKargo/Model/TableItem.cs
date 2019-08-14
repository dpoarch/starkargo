using System;
namespace StarKargo.Model
{
    public class TableItem
    {
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public int PackageQty { get; set; }
        public Guid GUID { get; set; }
        
    }
}