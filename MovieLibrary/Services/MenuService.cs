
using MovieLibrary.Services.IO;

namespace MovieLibrary.Services
{
    public class MenuService : IMenuService
    {
        private IOutputService _outputService;
        private IInputService _inputService;

        public MenuService(IOutputService outputService, IInputService inputService)
        {
            _outputService = outputService;
            _inputService = inputService;
        }

        /// <summary>
        /// Prompts the user with the menu options for the program.
        /// </summary>
        /// <returns>The <see cref="MenuOptions"/> choice selected by the user.</returns>
        public MenuOptions runMenu()
        {
            string input;
            PrintMenu();
            input = _inputService.ReadLine();

            return input.ToLower() switch
            {
                "1" => MenuOptions.SearchMovie,
                "2" => MenuOptions.AddMovie,
                "3" => MenuOptions.ListMovies,
                "4" => MenuOptions.UpdateMovie,
                "5" => MenuOptions.DeleteMovie,
                "q" => MenuOptions.Exit,
                _ => MenuOptions.Invalid,
            };
        }

        private void PrintMenu()
        {
            _outputService.WriteLine("1) Search for a Movie");
            _outputService.WriteLine("2) Add a Movie");
            _outputService.WriteLine("3) List Movies");
            _outputService.WriteLine("4) Update Movie");
            _outputService.WriteLine("5) Delete a Movie");
            _outputService.WriteLine("Enter q to quit");
        }
    }
}
