using MovieLibraryEntities.Models;

namespace MovieLibraryEntities.Dao
{
    public interface IRepository
    {
        IEnumerable<Movie> GetAll();
        IEnumerable<Movie> Search(string searchString);

        public Movie GetById(int id);
        public void AddMovie(string title, IEnumerable<string> genres, DateTime releaseDate);
        
        public IEnumerable<Genre> GetAllGenres();
        public void UpdateMovie(Movie movie, string title, IEnumerable<string> genresToRemove, IEnumerable<string> genresToAdd, DateTime releaseDate);

        public void DeleteMovie(Movie movie);
    }
}
