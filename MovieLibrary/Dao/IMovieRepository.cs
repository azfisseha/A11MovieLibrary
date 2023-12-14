using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieLibrary.Dao
{
    public interface IMovieRepository
    {
        void SearchMovie();
        void AddMovie();
        void ListMovies();
        void UpdateMovie();
        void DeleteMovie();

    }
}
