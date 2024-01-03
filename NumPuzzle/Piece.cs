using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumPuzzle
{
    public partial class Piece:ObservableObject
    {
        [ObservableProperty]
        private int num;
        [ObservableProperty]
        private bool isNull;
        [ObservableProperty]
        private bool isSelected;
    }
}
