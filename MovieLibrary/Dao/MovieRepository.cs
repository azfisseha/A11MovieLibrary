using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MovieLibrary.Services.IO;
using MovieLibraryEntities.Dao;
using MovieLibraryEntities.Models;
using System.Text;

namespace MovieLibrary.Dao
{
    /// <summary>
    /// A business logic implementation layer used to hide potential complexities from the database layer.
    /// Also takes away user interaction from the database layer.
    /// </summary>
    public class MovieRepository : IMovieRepository
    {
        private readonly IOutputService _outputService;
        private readonly IInputService _inputService;
        private readonly IRepository _repository;

        public MovieRepository(IOutputService outputService, IInputService inputService, IRepository repository)
        {
            _outputService = outputService;
            _inputService = inputService;
            _repository = repository;
        }

        public void SearchMovie()
        {
            _outputService.WriteLine("Enter the title substring to search for (case-insensitive)");

            var searchString = _inputService.ReadLine();
            var movies = _repository.Search(searchString);
            if (movies.Count() == 0)
            {
                _outputService.WriteLine("No movie found.");
            }
            else
            {
                _outputService.WriteLine($"Your movies are: ");
                movies.ToList().ForEach(x => _outputService.WriteLine($"{x.ToString()}"));
            }
        }

        public void AddMovie()
        {
            _outputService.Write("Enter Title: ");
            var title = _inputService.ReadLine();

            //Check to see if this Title already exists in the DB
            var searchResults = _repository.Search(title);
            var duplicateEntries = searchResults.Where(x => x.Title == title);
            if (duplicateEntries.Count() > 0)
            {
                //Error - duplicate entry
                _outputService.WriteLine("Could not add movie entry - identical to existing entry");
                _outputService.WriteLine(duplicateEntries.First().ToString());
                _outputService.WriteLine("Returning to Menu...");
                return;
            }

            var releaseDate = GetReleaseDateFromUser("Enter Release Date (MM/DD/YYYY): ");

            //if(User either submitted a future date, or quit out of the prompt asking for a date.)
            if (releaseDate > DateTime.Now)
            {
                _outputService.WriteLine("Returning to Menu...");
                return;
            }


            var validGenres = _repository.GetAllGenres();
            _outputService.WriteLine("Valid Genres: " + string.Join(", ", validGenres.Select(x => x.Name)) + "\n");

            var genres = new List<String>();
            var addtlGenres = true;
            do
            {
                _outputService.Write("Enter Genre (X when done): ");

                var input = _inputService.ReadLine();
                if (input.ToUpper() == "X")
                {
                    if (genres.Count == 0)
                    {
                        _outputService.WriteLine("Enter at least one Genre to continue");
                    }
                    else
                    {
                        addtlGenres = false;
                    }
                }
                //No adding new genres here!
                else if (validGenres.SingleOrDefault(x => x.Name == input) == null)
                {
                    _outputService.WriteLine($"{input} does not match an existing genre.");
                }
                else
                {
                    genres.Add(input);
                }
            } while (addtlGenres);

            _repository.AddMovie(title, genres, releaseDate);
            _outputService.WriteLine($"Added {title} to the library.");
        }


        public void ListMovies()
        {
            _outputService.WriteLine($"{"Id",-5} | {"Title",-80} | {"Released",-10} | Genres ");
            var entry = 0;

            var movies = _repository.GetAll();

            foreach (var movie in movies)
            {
                var next10 = movies.Skip(entry).Take(10);

                foreach (var m in next10)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"{m.ToString()} | ")
                      .Append(string.Join(", ", m.MovieGenres.Select(x => x.Genre.Name)));

                    _outputService.WriteLine(sb.ToString());
                    entry++;
                }
                next10 = movies.Skip(entry).Take(10);

                if (movies.Count() < entry + 10 || !ContinueDisplaying())
                {
                    break;
                }
            }
        }

        public void UpdateMovie()
        {
            var id = GetMovieIDFromUser("Enter Movie ID to update: ");
            var movie = _repository.GetById(id);

            if (movie == null)
            {
                _outputService.WriteLine($"No movie found with ID: {id}");
                return;
            }

            _outputService.WriteLine(movie.ToString());
            _outputService.Write("Update Title (Enter if unchanged): ");
            var title = _inputService.ReadLine();

            var releaseDate = GetReleaseDateFromUser("Update Release Date (MM/DD/YYYY): ");
            if (releaseDate > DateTime.Now) { releaseDate = movie.ReleaseDate; }

            var genres = movie.MovieGenres.Select(x => x.Genre).ToList();
            var validGenres = _repository.GetAllGenres().ToList();
            var genresToAdd = new List<string>();
            var genresToRemove = new List<string>();

            _outputService.Write("Genres: ");
            _outputService.WriteLine(string.Join(", ", genres.Select(x => x.Name)));

            _outputService.Write("Enter Y to remove a Genre: ");
            var input = _inputService.ReadLine();
            while (input !=null && input.ToLower().Substring(0, 1) == "y")
            {
                _outputService.Write("Enter the Genre to remove: ");
                do
                {
                    input = _inputService.ReadLine();
                    if (genres.Where(x => x.Name == input).IsNullOrEmpty())
                    {
                        _outputService.WriteLine($"{input} is not a valid genre associated with {movie.Title}");
                    }
                    else
                    {
                        genresToRemove.Add(input);
                    }
                } while (genres.Where(x => x.Name == input).IsNullOrEmpty());

                _outputService.Write("Enter Y to remove another Genre: ");
                input = _inputService.ReadLine();
            }

            var genresAvailableToAdd = validGenres.Except(genres);
            _outputService.WriteLine($"Other genres: {string.Join(", ", genresAvailableToAdd.Select(x => x.Name))}");
            _outputService.Write("Enter Y to add a Genre: ");
            input = _inputService.ReadLine();
            while (input != null && input.ToLower().Substring(0, 1) == "y")
            {
                _outputService.Write("Enter the Genre to add: ");
                do
                {
                    input = _inputService.ReadLine();
                    if (genresAvailableToAdd.Where(x => x.Name == input).IsNullOrEmpty())
                    {
                        _outputService.WriteLine($"{input} is not a valid genre");
                    }
                    else
                    {
                        genresToAdd.Add(input);
                    }
                } while (genresAvailableToAdd.Where(x => x.Name == input).IsNullOrEmpty());

                _outputService.Write("Enter Y to add another Genre: ");
                input = _inputService.ReadLine();
            }

            _repository.UpdateMovie(movie, title, genresToRemove, genresToAdd, releaseDate);
            _outputService.WriteLine($"Movie updated successfully");
        }

        public void DeleteMovie()
        {
            var id = GetMovieIDFromUser("Enter Movie ID to delete: ");
            var movie = _repository.GetById(id);

            if (movie == null)
            {
                _outputService.WriteLine($"No movie found with ID: {id}");
                return;
            }

            _outputService.WriteLine(movie.ToString());
            _outputService.WriteLine("Type Y to confirm deletion. Any other response will cancel this attempt.");
            var input = _inputService.ReadLine();
            
            if(input != "" && input.ToLower() == "y") 
            {
                _repository.DeleteMovie(movie);
            }

        }

        private bool ContinueDisplaying()
        {
            _outputService.WriteLine("Hit Enter to continue or ESC to cancel");
            var input = _inputService.ReadKey();
            while (input.Key != ConsoleKey.Enter && input.Key != ConsoleKey.Escape)
            {
                input = _inputService.ReadKey();
                _outputService.WriteLine("Hit Enter to continue or ESC to cancel");
            }

            if (input.Key == ConsoleKey.Escape)
            {
                return false;
            }

            return true;
        }
        private DateTime GetReleaseDateFromUser(string prompt)
        {
            _outputService.Write(prompt);
            var input = _inputService.ReadLine();
            var exit = false;
            var releaseDate = DateTime.MaxValue;
            do
            {
                if (DateTime.TryParseExact(input, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime result))
                {
                    releaseDate = result;
                    exit = true;
                }
                else
                {
                    _outputService.WriteLine($"{input} is invalid.");
                    _outputService.Write("Please try again using the MM/DD/YYYY format, or press q to cancel: ");
                    input = _inputService.ReadLine();
                    if (input.ToLower() == "q")
                    {
                        exit = true;
                    }
                }
            } while (!exit);
            return releaseDate;
        }

        private int GetMovieIDFromUser(string prompt)
        {
            var id = -1;
            string input;
            do
            {
                _outputService.Write(prompt);
                input = _inputService.ReadLine();
                try
                {
                    id = Int32.Parse(input);
                }
                catch (Exception e)
                {

                }
                if (id < 0)
                {
                    _outputService.WriteLine($"{input} is invalid.");
                }
            } while (id < 0);

            return id;
        }
    }
}
