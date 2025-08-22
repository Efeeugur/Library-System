using LibraryManagementSystem.DataAccess.Interfaces;

namespace LibraryManagementSystem.BusinessLogic.Services
{
    public abstract class BaseService
    {
        protected readonly IDataRepository _dataRepository;

        protected BaseService(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        // Helper method for null validation
        protected static bool IsNullOrEmpty(string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        // Helper method for entity existence validation
        protected static bool IsEntityFound<T>(T? entity) where T : class
        {
            return entity != null;
        }
    }
}