

using MovieLibrary.Dao;
using MovieLibraryEntities.Dao;

namespace MovieLibrary.Services
{
    public class MainService : IMainService
    {
        private readonly IMenuService _menuService;
        private readonly IMovieRepository _movieRepository;


        public MainService(IMenuService menuService, IMovieRepository movieRepository)
        {
            _menuService = menuService;
            _movieRepository = movieRepository;
        }

        public void Invoke()
        {
            MenuOptions selection;
            do
            {
                selection = _menuService.runMenu();

                switch (selection) 
                {
                    case MenuOptions.SearchMovie:
                        _movieRepository.SearchMovie();
                        break;
                    case MenuOptions.AddMovie:
                        _movieRepository.AddMovie();
                        break;
                    case MenuOptions.ListMovies:
                        _movieRepository.ListMovies();
                        break;
                    case MenuOptions.UpdateMovie:
                        _movieRepository.UpdateMovie();
                        break;
                    case MenuOptions.DeleteMovie:
                        _movieRepository.DeleteMovie();
                        break;
                }
            } while (selection != MenuOptions.Exit);
        }
    }
}
