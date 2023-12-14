using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieLibrary.Services
{
    public enum MenuOptions
    {
        SearchMovie = 1,
        AddMovie = 2,
        ListMovies = 3,
        UpdateMovie = 4,
        DeleteMovie = 5,
        Exit = 0,
        Invalid = -1
    }
}
