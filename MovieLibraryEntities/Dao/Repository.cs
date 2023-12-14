using Microsoft.EntityFrameworkCore;
using MovieLibraryEntities.Context;
using MovieLibraryEntities.Models;

namespace MovieLibraryEntities.Dao
{
    public class Repository : IRepository, IDisposable
    {
        private readonly IDbContextFactory<MovieContext> _contextFactory;
        private readonly MovieContext _context;

        public Repository(IDbContextFactory<MovieContext> contextFactory)
        {
            _contextFactory = contextFactory;
            _context = _contextFactory.CreateDbContext();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public IEnumerable<Movie> GetAll()
        {
            return _context.Movies.ToList();
        }

        public IEnumerable<Movie> Search(string searchString)
        {
            var allMovies = _context.Movies;
            var listOfMovies = allMovies.ToList();
            var temp = listOfMovies.Where(x => x.Title.Contains(searchString, StringComparison.CurrentCultureIgnoreCase));

            return temp;
        }

        //This was unused, but I chose to leave it here in case needed in the future.
        public Movie GetById(int id)
        {
            return _context.Movies.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Genre> GetAllGenres()
        {
            return _context.Genres.ToList();
        }

        public void AddMovie(string title, IEnumerable<string> genres, DateTime releaseDate)
        {
            var movie = new Movie
            {
                Title = title,
                ReleaseDate = releaseDate
            };
            _context.Movies.Add(movie);

            foreach (var genreName in genres)
            {
                var genre = _context.Genres.SingleOrDefault(x => x.Name == genreName);

                if (genre != null)
                {
                    var movieGenre = new MovieGenre
                    {
                        Movie = movie,
                        Genre = genre
                    };
                    _context.MovieGenres.Add(movieGenre);
                }
            }

            _context.SaveChanges();
        }

        

        public void UpdateMovie(Movie movie, string title, IEnumerable<string> genresToRemove, IEnumerable<string> genresToAdd, DateTime releaseDate)
        {
            movie.Title = title;
            movie.ReleaseDate = releaseDate;

            var movieGenres = movie.MovieGenres.ToList();

            //poor implementation, assumes business logic is handling genre name validation
            //should be checking for nulls here too.
            foreach (var genreName in genresToRemove)
            {
                var movieGenre = movieGenres.Where(x => x.Genre.Name == genreName).SingleOrDefault();
                movie.MovieGenres.Remove(movieGenre);
                _context.MovieGenres.Remove(movieGenre);
                _context.Genres
                    .ToList().Where(x => x.Name == genreName).SingleOrDefault()
                    .MovieGenres.Remove(movieGenre);
            }

            foreach(var genreName in genresToAdd)
            {
                var movieGenre = new MovieGenre
                {
                    Movie = movie,
                    Genre = _context.Genres.ToList().Where(x => x.Name == genreName).SingleOrDefault()
                };
                movie.MovieGenres.Add(movieGenre);
                _context.MovieGenres.Add(movieGenre);
                _context.Genres
                    .ToList().Where(x => x.Name == genreName).SingleOrDefault()
                    .MovieGenres.Add (movieGenre);
            }

            _context.SaveChanges();
        }

        public void DeleteMovie(Movie movie)
        {
            var movieGenres = _context.MovieGenres.Where(x => x.Movie.Id == movie.Id).ToList();
            var userMovies = _context.UserMovies.Where(x => x.Movie.Id == movie.Id).ToList();

            foreach(var movieGenre in movieGenres)
            {
                _context.Remove(movieGenre);
            }
            foreach(var userMovie in userMovies) 
            {
                _context.Remove(userMovie);
            }
            _context.Remove(movie);
            _context.SaveChanges();
        }
    }
}
