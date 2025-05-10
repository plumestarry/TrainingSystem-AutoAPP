using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexModule.Models
{
    public partial class InfoCard : ObservableObject
    {
        [ObservableProperty]
        private string? header;

        [ObservableProperty]
        private string? content;
    }
}
