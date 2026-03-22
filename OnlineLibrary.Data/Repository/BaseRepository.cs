namespace OnlineLibrary.Data.Repository
{
    public class BaseRepository : IDisposable
    {
        private bool isDisposed = false;
        private readonly OnlineLibraryDbContext dbContext;

        protected BaseRepository(OnlineLibraryDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected OnlineLibraryDbContext DbContext => dbContext;

        protected async Task<int> SaveChangesAsync()
        {
            return await DbContext.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    dbContext.Dispose();
                }

            }
            isDisposed = true;
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
