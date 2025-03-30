using FileUpload.Data;

namespace FileUpload.Web.Models
{
    public class ImageViewModel
    {
        public Image Image { get; set; }
        public List<int> Ids { get; set; }
        public int Id { get; set; }
        public bool Validated { get; set; }
        public bool FirstVisit { get; set; }
    }
}
