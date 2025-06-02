using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs
{
    public class NextWaiveMarketDate
    {
        public DateTime nextWaiveMarketDate { get; set; }
    }
    public class DeleteProductCategory
    {
        public string complaintId { get; set; }
    }
    public class CreateCustomerComplaint
    {
        public string complaintId { get; set; }
        public string vendorId { get; set; }
        public string comPlaintMsg { get; set; }
        public string imageUrl { get; set; }
    }
    public class UpdateCustomerComplaint : CreateCustomerComplaint
    {
        public string complaintId { get; set; }
    }
    public class CreateProductCategory
    {
        public string categoryName { get; set; }
        public string description { get; set; }
    }
    public class IdModel
    {
        public string id { get; set; }
    }


}
