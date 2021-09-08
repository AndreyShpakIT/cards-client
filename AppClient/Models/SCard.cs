using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppClient.Models
{
    public class SCard
    {
        public long Id { get; set; } = -1;
        public string Title { get; set; } = "Untitled";
        public string ImageBytes { get; set; } = "[]";
    }
}
